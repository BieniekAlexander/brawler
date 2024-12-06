using static PlayableCharacterInputs;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class CastSlot {
    public CastSlot(string _name) {
        Name = _name;
        CastPrefab = null;
        Cooldown = -1;
        DefaultChargeCount = 1;
    }

    public string Name;
    public Cast CastPrefab;
    public CastCosts Costs;
    public int Cooldown;
    public int DefaultChargeCount;
}

public class CastContainer {
    static GameObject RootCastablePrefab = Resources.Load<GameObject>("RootCastable");

    public CastContainer(CastSlot castSlot, Character character, string name) {
        CastSlot = castSlot;
        timer = 0;
        charges = castSlot.DefaultChargeCount;

        RootCastable = GameObject.Instantiate(
            RootCastablePrefab,
            character.transform
        ).GetComponent<Cast>();

        RootCastable.name = $"{name}Root";
    }

    public CastSlot CastSlot;
    public Cast RootCastable { get; private set; }
    public int timer;
    public int charges;
}

public enum CastId {
    Rush = 0,
    Light1 = 1,
    Light2 = 2,
    LightS = 3,
    Medium1 = 4,
    Medium2 = 5,
    MediumS = 6,
    Heavy1 = 7,
    Heavy2 = 8,
    HeavyS = 9,
    Throw1 = 10,
    Throw2 = 11,
    ThrowS = 12,
    Ability1 = 13,
    Ability2 = 14,
    Ability3 = 15,
    Ability4 = 16,
    Special1 = 17,
    Special2 = 18,
    Ultimate = 19
    
}

public class Character : MonoBehaviour, IDamageable, IMoves, ICasts, ICharacterActions, ICollidable {
    // is this me? TODO better way to do this
    [SerializeField] public bool me = false;
    [SerializeField] public int TeamBitMask { get; set; } = 1;

    /* Visuals */
    // Bolt Visualization
    public Material Material { get; private set; }
    TrailRenderer tr;

    /* Controls */
    private PlayableCharacterInputs characterActions;
    private bool RotatingClockwise = false;
    public float MinimumRotationThreshold { get; private set; } = 1f; // TODO put this in some kind of control config; minimum rotation to mirror casts
    public Transform CursorTransform { get; private set; }
    private Plane aimPlane;

    public bool InputJump { get; private set; }
    public bool InputDash { get; private set; }
    public bool hasAirDash = false;

    private Vector2 _inputMoveDirection = new(); 
    public Vector2 InputMoveDirection { 
        get => _inputMoveDirection;
        set => _inputMoveDirection = value;
    }

    public Vector3 MoveDirection {
        get {
            // TODO is this the best way to do this
            return (EncumberedStack>0 || StunStack>0)
            ? Vector3.zero :
            new Vector3(InputMoveDirection.x, 0f, InputMoveDirection.y);
        }
    }

    public Vector3 InputAimVector { // TODO make sure that the Y position is locked to the character's Y
        get {
            return CursorTransform.position-cc.transform.position;
        }
        set {
            if (value == Vector3.zero) {
                value = Vector3.forward;
            }

            // setting direction
            CursorTransform.position = cc.transform.position+value;

            // setting rotation
            var direction = value.normalized;
            Quaternion newRotation = (RotationCap>=175f)
                ? Quaternion.FromToRotation(Vector3.forward, direction)
                : Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(Vector3.forward, direction), RotationCap);

            float yRotationDiff = Mathf.DeltaAngle(newRotation.eulerAngles.y, transform.rotation.eulerAngles.y);
            if (Mathf.Abs(yRotationDiff) > MinimumRotationThreshold) {
                RotatingClockwise = yRotationDiff<0;
            }

            transform.rotation = newRotation;
        }
    }

    public bool InputBlocking { get; set; } = false;

    /* Signals */
    // this is a very bad way to implement this, but I just want to propagate damage dealt by my RL agent to learning
    private int _damageDealt;
    public int DamageDealt {
        get { int temp = _damageDealt; _damageDealt=0; return temp; } // when the value is read, it's set to zero
        set {
            _damageDealt=value;
        }
    }

    /* Children */
    [SerializeField] public Shield ShieldPrefab;
    private EffectInvulnerable ParryInvulnerabilityPrefab;
    public Shield Shield { get; private set; }

    /* Controls */
    //movement
    public void OnAim(InputAction.CallbackContext context) {
        Vector2 InputAimPosition = context.action.ReadValue<Vector2>();

        if (Camera.main != null) { // yells at me when the game is closed
            Ray ray = Camera.main.ScreenPointToRay(InputAimPosition);

            if (aimPlane.Raycast(ray, out float distance)) {
                Vector3 aimPoint = ray.GetPoint(distance);
                // cursor position getting locked to Z by inputrelaimpos set
                InputAimVector = MovementUtils.inXZ(aimPoint-cc.transform.position);
            }
        }
    }

    public void OnMovement(InputAction.CallbackContext context) => InputMoveDirection = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context) {
        InputJump = context.ReadValueAsButton();
    }

    public void OnDash(InputAction.CallbackContext context) {
        InputDash = context.ReadValueAsButton();
    }

    public void OnRush(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Rush;
        }
    }

    // attacks
    public void OnLight1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Light1;
        }
    }
    public void OnLight2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Light2;
        }
    }
    public void OnBoostedLight(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.LightS;
        }
    }
    public void OnMedium1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Medium1;
        }
    }
    public void OnMedium2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Medium2;
        }
    }
    public void OnBoostedMedium(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.MediumS;
        }
    }
    public void OnHeavy1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Heavy1;
        }
    }
    public void OnHeavy2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Heavy2;
        }
    }
    public void OnBoostedHeavy(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.HeavyS;
        }
    }

    // shields
    public void OnBlock(InputAction.CallbackContext context) => InputBlocking = context.ReadValueAsButton();

    // throws
    public void OnThrow1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Throw1;
        }
    }
    public void OnThrow2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Throw2;
        }
    }
    public void OnBoostedThrow(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.ThrowS;
        }
    }

    // abilities
    public void OnAbility1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Ability1;
        }
    }
    public void OnAbility2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Ability2;
        }
    }
    public void OnAbility3(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Ability3;
        }
    }
    public void OnAbility4(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Ability4;
        }
    }
    public void OnSpecial1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Special1;
        }
    }
    public void OnSpecial2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Special2;
        }
    }
    public void OnUltimate(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Ultimate;
        }
    }

    public CharacterFrameInput GetCharacterFrameInput() {
        return new CharacterFrameInput(
            InputAimVector,
            MoveDirection,
            InputBlocking,
            InputCastId
        );
    }

    public void SetMe() {
        me = true; // work on this more
        characterActions = new PlayableCharacterInputs();
        characterActions.Enable(); // TODO do I need to disable this somewhere?
        characterActions.character.SetCallbacks(this);
    }

    public void UnsetMe() {
        me = false;
        characterActions.Disable();
    }

    private static HashSet<int> attackCastIdSet = new(){(int) CastId.Light1, (int) CastId.Light2, (int) CastId.LightS, (int) CastId.Medium1, (int) CastId.Medium2, (int) CastId.MediumS, (int) CastId.Heavy1, (int) CastId.Heavy2, (int) CastId.HeavyS};

    private bool _hasResourcesForCast(CastCosts costs) => energy >= costs.energy && Charges >= costs.charges;
    private void useCastResources(CastCosts costs) {
        energy -= costs.energy;
        Charges -= costs.charges;
    }

    /* Casts */
    /// <summary/>
    /// <param name="castId"></param>
    /// <returns>whether a cast/recast was initiated</returns>
    private bool StartCast(int castId) {
        if (!Busy && attackCastIdSet.Contains(castId) && ActiveCastable!=null) {
            if (ActiveCastable.OnAttackCastables(CursorTransform)) {
                return true;
            }
        }

        // charges are shared if defaultChargeCount<0 - index is indicated by the negated index
        CastContainer castContainer = CastContainers[castId];
        CastContainer cdContainer = (castSlots[castId].DefaultChargeCount>=0)
             ? castContainer
             : CastContainers[castId+castSlots[castId].DefaultChargeCount];

        if (cdContainer.charges == 0) {
            // we're updating another cast - allowed
            return castContainer.RootCastable.OnRecastCastables(CursorTransform);
        } else if (castContainer.CastSlot.CastPrefab == null) {
            Debug.Log("No cast supplied for cast"+(int)castId);
            return false;
        } else if (!Busy && _hasResourcesForCast(castContainer.CastSlot.Costs)) {
            useCastResources(castContainer.CastSlot.Costs);

            if (ActiveCastable!=null && ActiveCastable.DestroyOnRecast) {
                Destroy(ActiveCastable.gameObject);
            }

            Transform castOrigin = transform;

            if (castContainer.CastSlot.CastPrefab.TargetResolution == TargetResolution.SnapTarget) {
                Character t = CastUtils.GetSnapTarget(CursorTransform.position, TeamBitMask, castContainer.CastSlot.CastPrefab.Range);

                if (t==null) {
                    // TODO if there's no target snap, the buffer will cause the code to retry `buffer` times,
                    // which I don't want, but I don't want to return true because nothing was casted
                    return false;
                } else {
                    castOrigin = t.transform;
                }
            }

            Cast castPrefab = castContainer.CastSlot.CastPrefab;
            ActiveCastable = Cast.Initiate(
                castPrefab,
                this,
                castOrigin,
                CursorTransform,
                !RotatingClockwise,
                castContainer.RootCastable
                // TODO might be cleaner to write if I have the parent collect the castable ref
            );

            cdContainer.charges--;
            cdContainer.timer = cdContainer.CastSlot.Cooldown;
            return true;
        } else {
            // TODO I think it goes here if you can't afford the cast, meaning it will stay in buffer
            // I think I need to change this
            return false;
        }
    }

    /// <summary>
    /// Reduces all cooldowns and evalutes cast completions
    /// </summary>
    /// <returns>The index if the cast being casted, or -1 if otherwise</returns>
    void TickCasts() {
        // resolve cooldowns and cast expirations
        for (int i = 0; i<CastContainers.Length; i++) {

            if (CastContainers[i].charges < castSlots[i].DefaultChargeCount) {
                if (--CastContainers[i].timer<=0) {
                    CastContainers[i].charges++;
                    CastContainers[i].timer = castSlots[i].Cooldown;
                }
            }
        }
    }    

    private void HandleCharges() {
        rechargeTimer=(Charges>=maxCharges) ? rechargeRate : rechargeTimer-1;
        if (rechargeTimer<=0) {
            Charges+=1;
            rechargeTimer=rechargeRate;
        }
    }

    /* ICasts */
    [SerializeField] private CastSlot[] castSlots = Enum.GetNames(typeof(CastId)).Select(name => new CastSlot(name)).ToArray();
    public CastContainer[] CastContainers = new CastContainer[Enum.GetNames(typeof(CastId)).Length];
    public static int[] boostedIds = new int[] { (int)CastId.LightS, (int)CastId.MediumS, (int)CastId.HeavyS, (int)CastId.ThrowS };
    public static int[] specialIds = new int[] { (int)CastId.Special1, (int)CastId.Special2 };
    public int CastBufferTimer { get; private set; } = 0;
    public Cast ActiveCastable = null;
    private int _nextCastId;
    public int InputCastId {
        get { return _nextCastId; }
        set {
            _nextCastId = value;
            if (value>0) {
                CastBufferTimer = 60;
            }
        }
    }

    private int maxCharges = 5;
    public int Charges { get; set; } = 4;
    private int rechargeRate = 300;
    private int rechargeTimer = 0;
    private int energy = 10000; // TODO lower
    public int SilenceStack { get; set; } = 0;
    public int MaimStack { get; set; } = 0;

    public bool IsRotatingClockwise() {
        return RotatingClockwise;
    }

    public Transform GetAboutTransform() {
        return transform;
    }

    public Transform GetTargetTransform() {
        return CursorTransform;
    }

    public List<Cast> Children {
        get {
            return (from CastContainer cc in CastContainers select cc.RootCastable).ToList();
        }
    }

    /* State Machine */
    public CharacterStateFactory StateFactory;
    public CharacterState State { get; set ; }

    public void SwitchState(Type _characterStateType) {
        State = State.SwapState(_characterStateType);
    }

    public bool Parried = false;
    private int _busyTimer;
    private bool _busyEncumbered;
    private bool _busyStunned;
    public bool Busy {get {return _busyTimer != 0; }}

    public void SetBusy(bool encumbered, bool stunned, float rotationCap) {SetBusy(-1, encumbered, stunned, rotationCap);}
    public void SetBusy(int newBusyTimer, bool encumbered, bool stunned, float rotationCap) {
        _busyTimer = newBusyTimer;

        if (encumbered && !_busyEncumbered) {
            _busyEncumbered = true;
            EncumberedStack++;
        } else if (!encumbered && _busyEncumbered) {
            _busyEncumbered = false;
            EncumberedStack--;
        }

        if (stunned && !_busyStunned) { // maybe rip _busyStunned to getters and setters
            _busyStunned = true;
            StunStack++;
        } else if (!stunned && _busyStunned) {
            _busyStunned = false;
            StunStack--;
        }

        if (rotationCap < 175) {
            RotationCap = rotationCap;
        }
    }

    public void UnsetBusy() {
        if (_busyEncumbered) {
            _busyEncumbered = false;
            EncumberedStack--;
        }

        if (_busyStunned) {
            _busyStunned = false;
            StunStack--;
        }

        _busyTimer = 0;
        RotationCap = 180f;
    }

    /* MonoBehavior */
    public void OnRespawn() {
        // position
        standingY = transform.position.y + standingOffset;
        
        // resources
        healMax = HPMax;
        HP = HPMax;

        // state
        _busyTimer = 0;
        SwitchState(CharacterState.GetSpawnState());
        InputCastId = -1;
    }
    
    private void Awake() {
        // Controls
        if (me) SetMe(); // TODO lazy way to set up controls
        RespawnDelayEffectPrefab = Resources.Load<GameObject>("RespawnDelayEffect"); // TODO put this in a scriptable object
        aimPlane = new Plane(Vector3.up, transform.position);
        GameObject cursorGameObject = new GameObject("Player Cursor Object");
        cursorGameObject.transform.parent = transform;
        CursorTransform = cursorGameObject.transform;

        // components
        _collider = GetComponent<Collider>();
        cc = GetComponent<CharacterController>();
        ParryInvulnerabilityPrefab = Resources.Load<GameObject>("ParryInvulnerability").GetComponent<EffectInvulnerable>();
        Shield = Instantiate(ShieldPrefab, transform);
        Shield.gameObject.SetActive(false);

        // initialize casts
        for (int i = 0; i<castSlots.Length; i++) {
            CastContainers[i] = new CastContainer(castSlots[i], this, castSlots[i].Name);
        }

        // Visuals
        Material = GetComponent<Renderer>().material;
        tr = GetComponent<TrailRenderer>();

        // State
        StateFactory = new(this);
        State = StateFactory.Get(CharacterState.GetSpawnState());
        OnRespawn();
    }

    private void HandleVisuals() {
        /*
         * Bolt Indicator
         */
        // duration
        if (InvulnerableStack>0)
            Material.color = new Color(255, 255, 255);
        else if (HitStopTimer>0)
            Material.color = new Color(0, 0, 0);
        else if (State is CharacterStateBlownBack // disadvantage
            || State is CharacterStateKnockedBack
            || State is CharacterStatePushedBack)
            Material.color = new Color(255, 0, 0);
        else if (State is CharacterStateTumbling // vulnerable
            || State is CharacterStateKnockedDown)
            Material.color = new Color(125, 125, 0);
        else if (State is CharacterStateRolling // recovering
            || State is CharacterStateGettingUp)
            Material.color = new Color(255, 255, 0);
        else if (InputCastId >= 0)
            Material.color=Color.magenta;
        else if (Charges>0)
            Material.color=Color.green;
        else
            Material.color=Color.gray;

        /*
         * Trail Renderer
         */
        tr.emitting=(Velocity.sqrMagnitude>(.2f*.2f)); // TODO return to this later
    }

    private void Update() {
        // handle inputs
        if (me) {
            if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                ToggleRecordingControls("characterInput");
            }
        }

        HandleVisuals();
    }

    private void ToggleRecordingControls(string recordFileName) {
        if (!_recordingControls) {
            Debug.Log("Starting to record controls");
            _recordingControls = true;
            _recordControlStream = new FileStream(recordFileName, FileMode.Create);
        } else {
            Debug.Log("Finished recording controls");
            _recordingControls = false;
            _recordControlStream.Close();
        }
    }

    public void ToggleCollectingControls(string recordFileName, bool restartAfterDone) {
        if (!_collectingControls) {
            Debug.Log("Starting to collect controls");
            _collectingControls = true;
            _collectingControlsRestart = restartAfterDone;
            _recordControlStream = new FileStream(recordFileName, FileMode.Open);
            if (characterActions!=null) { characterActions.Disable(); }
        } else {
            Debug.Log("Terminating control collection");
            _collectingControls = false;
            _collectingControlsRestart = false;
            _recordControlStream.Close();
            if (me) { characterActions.Enable(); }
        }
    }

    void FixedUpdate() {
        if (_recordingControls) {
            WriteCharacterFrameInput(_recordControlStream);
        } else if (_collectingControls) {
            LoadCharacterFrameInput(ReadCharacterFrameInput(_recordControlStream));
        }

        // Handle Casts
        if (_busyTimer>0 && --_busyTimer==0) {
            UnsetBusy();
        }


        if (HitStopTimer--<=0) {
            if (State.FixedUpdateState() is CharacterState newState) {
                State = newState;
            }

            // TODO I think I'm gonna need more vertical velocity considerations wrt state
            if (!IsGrounded()) {
                Velocity += Vector3.up*gravity;
            } else {
                Velocity = MovementUtils.setY(Velocity, Vector3.zero);
            }
            
            cc.Move(Velocity);
        }
        
        HandleCharges();

        if (CastBufferTimer-- == 0) {
            InputCastId = -1;
        } else if (InputCastId >= 0) {
            if (StartCast(InputCastId)) {
                InputCastId = -1;
            }
        }

        TickCasts();
        HandleCollisions();
    }

    public void SetCommandMovement(CommandMovement _commandMovement) {
        CommandMovement = _commandMovement;
    }

    public bool IsGrounded() {
        if (Velocity.y>0) {
            return false;
        } else {
            // TODO ugly implementation for now - checking if grounded, updating if true, returning false if not
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, cc.radius+standingOffset+.01f, 1<<LayerMask.NameToLayer("Terrain"));
            if (hitInfo.transform) {
                standingY = hitInfo.point.y + cc.radius + standingOffset;
                return true;
            } else {
                return false;
            }
        }
    }

    private Vector3 GetDecayedVector(Vector3 vector, float decayRate) {
        return vector.normalized*Mathf.Max(vector.magnitude-decayRate, 0);
    }

    /* IMoves Methods */
    public float BaseSpeed { get; set; } = .125f;
    public float RunAcceleration = .05f;
    public readonly float Friction = .003f;

    public Transform Transform { get { return transform; } }
    public CharacterController cc { get; set; }
    public Vector3 Velocity { get; set; } = Vector3.zero;
    public int EncumberedStack { get; set; } = 0;
    public int StunStack { get; set; } = 0;
    public float RotationCap {get; set; } = 180f;
    public Transform ForceMoveDestination { get; set; } = null; // taunts, fears, etc.
    public CommandMovement CommandMovement { get; set; } = null;
    private float gravity = -.01f;
    private float standingY; private float standingOffset = .1f;
    public HitTier KnockBackHitTier { get; set; }
    public Vector3 KnockBack { get; set; } = new();
    public int HitStunTimer { get; set; } = 0;
    public int RecoveryTimer { get; set; } = 0;

    /// <summary>
    /// Returns true if the contact point is between the <paramref name="Character"/>'s center and their <paramref name="Shield"/>
    /// </summary>
    /// <remarks>I'll leave it up to the caller as to where the <paramref name="contactPoint"/> is</remarks>
    /// <param name="contactPoint"></param>
    /// <returns></returns>
    private bool HitsShield(Vector3 contactPoint) {
        if (Shield.isActiveAndEnabled) {
            Vector2 s = new Vector2(contactPoint.x, contactPoint.z);
            Vector2 a = new Vector2(transform.position.x, transform.position.z);

            if ((s-a).magnitude>=.5) {
                return true;
            } else {
                Vector2 mid = new Vector2(Shield.transform.position.x, Shield.transform.position.z);
                Vector2 perp = Vector2.Perpendicular(mid-a).normalized;
                Vector2 b = mid + perp*Shield.transform.localScale.x/2;
                Vector2 c = mid - perp*Shield.transform.localScale.x/2;

                if (MathUtils.PointInTriangle(s, a, b, c)) {
                    return true;
                } else {
                    return false;
                }
            }
        } else {
            return false;
        }
    }

    public float TakeKnockBack(Vector3 contactPoint, int hitStopDuration, Vector3 knockBackVector, int hitStunDuration, HitTier hitTier) {
        float knockBackFactor;
        bool hitsShield = HitsShield(contactPoint);

        if (hitsShield && Shield.ParryWindow > 0) {
            EffectInvulnerable Invulnerability = Instantiate(ParryInvulnerabilityPrefab, transform);
            Cast.Initiate(Invulnerability, this, transform, null, false, null);
            Parried = true;
            // TODO:
            // - reflect projectiles
            // - set 
            return 0; // TODO more interesting knockback interactions
        } else {
            if (InvulnerableStack>0) {
                return 0;
            }

            // TODO account for pushback of HitTier.Soft
            KnockBackHitTier = hitTier;
            knockBackFactor = 1;

            if (knockBackFactor > 0) {
                if (CommandMovement!=null) {
                    Destroy(CommandMovement.gameObject);
                    CommandMovement = null;
                }
                
                if (ActiveCastable!=null) {
                    Destroy(ActiveCastable.gameObject);
                    ActiveCastable = null;
                }
                
                KnockBack = knockBackFactor*knockBackVector;
                HitStunTimer = hitStunDuration;
                HitStopTimer = hitStopDuration;

                if (KnockBack != Vector3.zero) {
                    if (hitsShield) {
                        Velocity += KnockBack;
                    } else if (hitTier >= HitTier.Heavy) {
                        SwitchState(typeof(CharacterStateBlownBack));
                    } else if (hitTier == HitTier.Medium) {
                        SwitchState(typeof(CharacterStateKnockedBack));
                    } else if (hitTier == HitTier.Light) {
                        SwitchState(typeof(CharacterStatePushedBack));
                    } else {
                        Velocity += KnockBack;
                    }
                }
            }

            return knockBackFactor;
        }


    }

    /* IDamageable Methods */
    public int LifeCount = -1; // 
    private static GameObject RespawnDelayEffectPrefab;
    public int HP {get; private set;} = 1000;
    public static int HPMax = 1000;
    private int healMax; private int healMaxOffset = 100;
    public int HitStopTimer { get; set; } = 0;
    public List<Armor> Armors { get; } = new List<Armor>();
    public int AP {get { return Enumerable.Sum(from Armor armor in Armors select armor.AP ); } }
    public int ParryWindow { get; set; } = 0;
    public int InvulnerableStack { get; set; } = 0;

    public int TakeDamage(Vector3 _contactPoint, in int damage, HitTier hitTier) {
        // TODO add a better implementation to ignore shield
        if ((hitTier!=HitTier.Pure) && (InvulnerableStack>0 || HitsShield(_contactPoint))) return 0;
        int remainingDamage = damage;

        for (int i = Armors.Count-1; i>=0; i--) {
            remainingDamage = Armors[i].TakeDamage(remainingDamage);
        }

        int takenDamage = IDamageable.GetTakenDamage(remainingDamage, HP);
        HP -= takenDamage;
        healMax = Mathf.Min(healMax, HP+healMaxOffset);

        if (HP<=0) {
            OnDeath();
        }

        return remainingDamage-takenDamage;
    }

    public int TakeHeal(int damage) {
        HP = Math.Max(HP+damage, healMax);
        return Math.Min(damage, healMax-damage); // TODO I think this calculation is correct, but I'm not positive :)
    }

    public void TakeArmor(Armor armor) {
        Armors.Add(armor);
    }

    public void OnDeath() {
        if (LifeCount > 0) {
            if (--LifeCount==0) {
                Destroy(gameObject);
                return;
            }
        }

        Cast.Initiate(RespawnDelayEffectPrefab.GetComponent<Effect>(), null, transform, null, false, null);
    }

    void OnGUI() {
        // TODO remove: here for debugging
        if (!me) return;
        GUI.Label(new Rect(20, 40, 200, 20), MovementUtils.inXZ(Velocity).magnitude+"m/tick");
        GUI.Label(new Rect(20, 70, 200, 20), Charges+"/"+maxCharges);
        GUI.Label(new Rect(20, 100, 200, 20), "HP: "+HP);
        GUI.Label(new Rect(20, 130, 200, 20), "Energy: "+energy);
        GUI.Label(new Rect(20, 160, 200, 20), State.Name);
    }

    /* ICollidable */
    private Collider _collider;
    public Collider Collider { get { return _collider; } }
    public int ImmaterialStack { get; set; } = 0;

    bool OnControllerColliderHit(ControllerColliderHit hit) {
        GameObject collisionObject = hit.collider.gameObject;

        if (collisionObject.GetComponent<ICollidable>() is ICollidable collidable) {
            return State.OnCollideWith(collidable, new CollisionInfo(hit.normal));
        } else {
            throw new Exception("Unhandled collision type");
        }
    }

    public bool OnCollideWith(ICollidable other, CollisionInfo info) {
        if (other is Character otherCharacter
            && (
                ImmaterialStack > 0
                || otherCharacter.ImmaterialStack > 0
            )
        ) {
            return false;
        } else {
            return State.OnCollideWith(other, info);
        }
    }

    public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this);
    }

    /* Control Recording */
    private bool _recordingControls;
    private bool _collectingControls;
    private bool _collectingControlsRestart;
    private FileStream _recordControlStream = null;
    private void WriteCharacterFrameInput(FileStream outputStream) {
        CharacterFrameInput input = new CharacterFrameInput(
            InputAimVector,
            InputMoveDirection,
            InputBlocking,
            InputCastId
        );

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(outputStream, input);
    }

    public void LoadCharacterFrameInput(CharacterFrameInput input) {
        InputAimVector = new Vector3(input.AimDirectionX, 0, input.AimDirectionZ);
        InputMoveDirection = new Vector2(input.MoveDirectionX, input.MoveDirectionZ);
        InputCastId = input.CastId;
        InputBlocking = input.Blocking;
    }

    public CharacterFrameInput ReadCharacterFrameInput(FileStream inputStream) {
        BinaryFormatter formatter = new BinaryFormatter();
        CharacterFrameInput ret = (CharacterFrameInput)formatter.Deserialize(inputStream);

        if (inputStream.Position==inputStream.Length) {
            Debug.Log("Finishing reading recording");

            if (_collectingControlsRestart) {
                Debug.Log("restarting recording");
                inputStream.Seek(0, SeekOrigin.Begin);
            } else {
                Debug.Log("Closing recording recording");
                inputStream.Close();
                _collectingControlsRestart = false;
                _collectingControls = false;
                if (me) { characterActions.Enable(); }
            }
        }

        return ret;
    }// TODO collect inputs of user to replay them
}
