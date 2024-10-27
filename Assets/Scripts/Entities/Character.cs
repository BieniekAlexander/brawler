using static CharacterControls;
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
    Dash = 0,
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

[Serializable]
public class CharacterFrameInput {
    public CharacterFrameInput(
        Vector3 aimDirection,
        Vector2 moveDirection,
        bool blocking,
        bool shielding,
        bool running,
        int castId) {
        MoveDirectionX = moveDirection.x;
        MoveDirectionZ = moveDirection.y;
        AimDirectionX = aimDirection.x;
        AimDirectionZ = aimDirection.z;
        Blocking = blocking;
        Shielding = shielding;
        Running = running;
        CastId = castId;
    }

    public float MoveDirectionX = 0f;
    public float MoveDirectionZ = 0f;
    public float AimDirectionX = 0f;
    public float AimDirectionZ = 0f;
    public bool Blocking = false;
    public bool Shielding = false;
    public bool Running = false;
    public int CastId = -1;
}

/// <summary>
/// Object that manages whether the character is available to perform another action
/// </summary>
public class BusyMutex {
    public BusyMutex(Character character) {
        _character = character;
        _busyTimer = 0;
    }

    private Character _character;
    private int _busyTimer; // can't recast
    private bool _encumbered; // can't redirect
    private bool _stunned; // can't move

    public bool Busy {get { return _busyTimer!=0; }}

    /// <summary>
    /// Locks the Mutex indefinitely
    /// </summary>
    /// <param name="encumbered"></param>
    /// <param name="stunned"></param>
    /// <param name="rotationCap"></param>
    public void Lock(bool encumbered, bool stunned, float rotationCap) {
        Lock(-1, encumbered, stunned, rotationCap);
    }

    /// <summary>
    /// Locks the Mutex for the specified duration
    /// </summary>
    /// /// <param name="newBusyTimer"></param>
    /// <param name="encumbered"></param>
    /// <param name="stunned"></param>
    /// <param name="rotationCap"></param>
    public void Lock(int newBusyTimer, bool encumbered, bool stunned, float rotationCap) {
        if (_busyTimer>0) throw new Exception("Can't grab mutex, because it's already taken");

        _busyTimer = newBusyTimer;
        if (encumbered) {
            _encumbered = true;
            _character.EncumberedStack++;
        }

        if (stunned) {
            _stunned = true;
            _character.StunStack++;
        }

        if (rotationCap < 175) {
            _character.RotationCap = rotationCap;
        }
    }

    public void Unlock() {
        _busyTimer = 0;
        _character.RotationCap = 180f;

        if (_encumbered) {
            _character.EncumberedStack--;
            _encumbered = false;
        }
        
        if (_stunned) {
            _character.StunStack--;
            _stunned = false;
        }
    }

    public void Tick() {
        if (_busyTimer>0 && --_busyTimer==0) {
            Unlock();
        }        
    }
}

public class Character : MonoBehaviour, IDamageable, IMoves, ICasts, ICharacterActions, ICollidable {
    // is this me? TODO better way to do this
    [SerializeField] public bool me = false;
    [SerializeField] public int _teamBit = 1; // TODO refactor
    [SerializeField] public int TeamBitMask {get {return _teamBit; } set {_teamBit = value; }}

    /* State WIP */
    CharacterState _state;
    public CharacterState State { get { return _state; } set { _state = value; } }

    public bool StateIsActive(Type state) {
        return State.StateInHierarchy(state);
    }

    CharacterStateFactory StateFactory;
    public bool Parried = false;

    /* Visuals */
    // Bolt Visualization
    public Material Material { get; private set; }
    TrailRenderer tr;

    /* Controls */
    private CharacterControls characterControls;
    private bool RotatingClockwise = false;
    public float MinimumRotationThreshold { get; private set; } = 1f; // TODO put this in some kind of control config
    public Transform CursorTransform { get; private set; }
    private Plane aimPlane;

    public Vector2 InputMoveDirection { get; set; } = new();
    public Vector3 MoveDirection {
        get {
            // TODO is this the best way to do this
            return (EncumberedStack>0 || StunStack>0)
            ? Vector3.zero :
            new Vector3(InputMoveDirection.x, 0f, InputMoveDirection.y);
        }
    }

    public Vector3 InputAimDirection { // TODO make sure that the Y position is locked to the character's Y
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
                RotatingClockwise = (yRotationDiff<0);
            }

            transform.rotation = newRotation;
        }
    }

    public bool InputRunning { get; set; } = false;
    public bool Running { get { return InputRunning && StunStack==0; } }
    public bool InputBlocking { get; set; } = false;
    public bool InputShielding { get; set; } = false;

    /* Abilities */
    private int maxCharges = 5;
    public int Charges { get; set; } = 4;
    private int rechargeRate = 300;
    private int rechargeTimer = 0;
    private int energy = 10000; // TODO lower
    // private int maxEnergy = 100;

    [SerializeField] private CastSlot[] castSlots = Enum.GetNames(typeof(CastId)).Select(name => new CastSlot(name)).ToArray();
    public CastContainer[] CastContainers = new CastContainer[Enum.GetNames(typeof(CastId)).Length];
    public static int[] boostedIds = new int[] { (int)CastId.LightS, (int)CastId.MediumS, (int)CastId.HeavyS, (int)CastId.ThrowS };
    public static int[] specialIds = new int[] { (int)CastId.Special1, (int)CastId.Special2 };
    public int CastBufferTimer { get; private set; } = 0;
    private Cast _activeCastable = null;
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
    [SerializeField] public EffectInvulnerable ParryInvulnerabilityPrefab;
    public Shield Shield { get; private set; }

    /*
     * CONTROLS
     */
    //movement
    public void OnAim(InputAction.CallbackContext context) {
        Vector2 InputAimPosition = context.action.ReadValue<Vector2>();

        if (Camera.main != null) { // yells at me when the game is closed
            Ray ray = Camera.main.ScreenPointToRay(InputAimPosition);

            if (aimPlane.Raycast(ray, out float distance)) {
                Vector3 aimPoint = ray.GetPoint(distance);
                // cursor position getting locked to Z by inputrelaimpos set
                InputAimDirection = MovementUtils.inXZ(aimPoint-cc.transform.position);
            }
        }
    }

    public void OnMovement(InputAction.CallbackContext context) {
        InputMoveDirection = context.ReadValue<Vector2>();
    }

    public void OnRunning(InputAction.CallbackContext context) {
        InputRunning=context.ReadValueAsButton();
    }

    public void OnDash(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            InputCastId = (int)CastId.Dash;
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
    public void OnBlock(InputAction.CallbackContext context) {
        InputBlocking = context.ReadValueAsButton();
    }
    public void OnShield(InputAction.CallbackContext context) {
        InputShielding = context.ReadValueAsButton();
    }

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
            InputAimDirection,
            MoveDirection,
            InputBlocking,
            InputShielding,
            InputRunning,
            InputCastId
        );
    }

    public void SetMe() {
        me = true; // work on this more
        characterControls = new CharacterControls();
        characterControls.Enable(); // TODO do I need to disable this somewhere?
        characterControls.character.SetCallbacks(this);
    }

    public void UnsetMe() {
        me = false;
        characterControls.Disable();
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
        if (attackCastIdSet.Contains(castId) && _activeCastable!=null) {
            if(_activeCastable.OnAttackCastables(CursorTransform)) {
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
        } else if (!BusyMutex.Busy && _hasResourcesForCast(castContainer.CastSlot.Costs)) {
            useCastResources(castContainer.CastSlot.Costs);

            if (_activeCastable!=null && _activeCastable.DestroyOnRecast) {
                Destroy(_activeCastable.gameObject);
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
            _activeCastable = Cast.Initiate(
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

    /* ICasts Methods */
    // TODO bake these into getters
    public int SilenceStack { get; set; } = 0;
    public int MaimStack { get; set; } = 0;

    public bool IsRotatingClockwise() {
        return RotatingClockwise;
    }

    public Transform GetOriginTransform() {
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
    public void SwitchState(CharacterState state) {
        State = state;
        State.EnterState();
    }

    /* MonoBehavior */
    private void Awake() {
        // Controls
        if (me) SetMe(); // TODO lazy way to set up controls
        _collider = GetComponent<Collider>();
        GameObject cursorGameObject = new GameObject("Player Cursor Object");
        cursorGameObject.transform.parent = transform;
        CursorTransform = cursorGameObject.transform;
        aimPlane = new Plane(Vector3.up, transform.position);
        InputCastId = -1;

        // State WIP
        BusyMutex = new(this);
        StateFactory = new(this);
        SwitchState(StateFactory.Idle());

        cc = GetComponent<CharacterController>();
        standingY = transform.position.y + standingOffset;
        healMax = HP;

        // Children
        Shield = Instantiate(ShieldPrefab, transform);
        Shield.gameObject.SetActive(false);

        // Visuals
        Material = GetComponent<Renderer>().material;
        tr = GetComponent<TrailRenderer>();

        for (int i = 0; i<castSlots.Length; i++) {
            CastContainers[i] = new CastContainer(castSlots[i], this, castSlots[i].Name);
        }
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
        else if (_state is CharacterStateBlownBack // disadvantage
            || _state is CharacterStateKnockedBack
            || _state is CharacterStatePushedBack)
            Material.color = new Color(255, 0, 0);
        else if (_state is CharacterStateTumbling // vulnerable
            || _state is CharacterStateKnockedDown)
            Material.color = new Color(125, 125, 0);
        else if (_state is CharacterStateRolling // recovering
            || _state is CharacterStateGettingUp)
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
        tr.emitting=(Velocity.sqrMagnitude>12.6f*12.6f); // TODO return to this later
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
            if (characterControls!=null) { characterControls.Disable(); }
        } else {
            Debug.Log("Terminating control collection");
            _collectingControls = false;
            _collectingControlsRestart = false;
            _recordControlStream.Close();
            if (me) { characterControls.Enable(); }
        }
    }

    private void HandleCharges() {
        rechargeTimer=(Charges>=maxCharges) ? rechargeRate : rechargeTimer-1;
        if (rechargeTimer<=0) {
            Charges+=1;
            rechargeTimer=rechargeRate;
        }
    }

    void FixedUpdate() {
        if (_recordingControls) {
            WriteCharacterFrameInput(_recordControlStream);
        } else if (_collectingControls) {
            LoadCharacterFrameInput(ReadCharacterFrameInput(_recordControlStream));
        }

        // Handle Casts
        BusyMutex.Tick();
        State.FixedUpdateStates();

        if (HitStopTimer==0) {
            // TODO I think I'm gonna need more vertical velocity considerations wrt state
            if (!IsGrounded()) {
                Velocity += Vector3.up*gravity;
            } else {
                Velocity = MovementUtils.setY(Velocity, Vector3.zero);
            }
        }

        cc.Move(Velocity*Time.deltaTime);
        HandleCharges();

        if (--CastBufferTimer == 0) {
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
            int excludeAllButTerrainLayer = 1<<LayerMask.NameToLayer("Terrain");
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, cc.radius+standingOffset+.01f, excludeAllButTerrainLayer);
            if (hitInfo.transform) {
                standingY = hitInfo.point.y + cc.radius + standingOffset;
                return true;
            } else {
                return false;
            }
        }
    }

    private Vector3 GetDecayedVector(Vector3 vector, float decayRate) {
        return vector.normalized*Mathf.Max(vector.magnitude-decayRate*Time.deltaTime, 0);
    }

    /* IMoves Methods */
    public float BaseSpeed { get; set; } = 7.5f;
    public Transform Transform { get { return transform; } }
    public CharacterController cc { get; set; }
    public Vector3 Velocity { get; set; } = Vector3.zero;
    public BusyMutex BusyMutex;
    public int EncumberedStack { get; set; } = 0;
    public int StunStack { get; set; } = 0;
    public float RotationCap {get; set; } = 180f;
    public Transform ForceMoveDestination { get; set; } = null; // taunts, fears, etc.
    public CommandMovement CommandMovement { get; set; } = null;
    private float gravity = -.5f;
    private float standingY; private float standingOffset = .1f;
    public HitTier KnockBackHitTier { get; set; }
    public float KnockBackDecay { get; private set; } = 1f;
    public Vector3 KnockBack { get; set; } = new();
    public int HitStunTimer { get; set; } = 0;
    public int HitStopTimer { get; set; } = 0;
    public int RecoveryTimer { get; set; } = 0;

    public static float GetKnockBackFactor(HitTier HitTier, ShieldTier ShieldTier) {
        if (HitTier==HitTier.Soft) return 0;
        else if (HitTier==HitTier.Pure) return 1;
        else return ((int)HitTier - (int)ShieldTier)/(float)HitTier;
    }

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

                if (MathUtils.pointInTriangle(s, a, b, c)) {
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

        if (HitsShield(contactPoint)) {
            if (Shield.ParryWindow > 0) {
                EffectInvulnerable Invulnerability = Instantiate(ParryInvulnerabilityPrefab, transform);
                Cast.Initiate(Invulnerability, this, transform, null, false, null);
                Parried = true;
                // TODO:
                // - reflect projectiles
                // - set 
            }

            knockBackFactor = GetKnockBackFactor(hitTier, (Shield.isActiveAndEnabled ? Shield.ShieldTier : 0));
            return knockBackFactor;
        } else {
            if (InvulnerableStack>0) {
                return 0;
            }

            // TODO account for pushback of HitTier.Soft
            KnockBackHitTier = hitTier;
            knockBackFactor = 1;

            if (knockBackFactor > 0) {
                Destroy(CommandMovement);
                CommandMovement = null;
                KnockBack = knockBackFactor*knockBackVector;
                BusyMutex.Unlock(); // forcibly grab the busy mutex until the character recovers
                BusyMutex.Lock(false, false, 0f);

                if (hitStopDuration > 0) {
                    HitStopTimer = hitStopDuration;
                    SwitchState(StateFactory.HitStopped());
                } else if (KnockBack != Vector3.zero) {
                    if (hitTier >= HitTier.Heavy) {
                        SwitchState(StateFactory.BlownBack());
                    } else if (hitTier == HitTier.Medium) {
                        SwitchState(StateFactory.KnockedBack());
                    } else if (hitTier == HitTier.Light) {
                        SwitchState(StateFactory.PushedBack());
                    } else {
                        Velocity += KnockBack;
                    }
                }
            }

            return knockBackFactor;
        }


    }

    /* IDamageable Methods */
    public int HP { get; set; } = HPMax;
    public static int HPMax = 1000;
    private int healMax; private int healMaxOffset = 100;
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
        // Destroy(gameObject); // TODO disabling for RL
    }

    void OnGUI() {
        // TODO remove: here for debugging
        if (!me) return;
        GUI.Label(new Rect(20, 40, 200, 20), MovementUtils.inXZ(Velocity).magnitude+"m/s");
        GUI.Label(new Rect(20, 70, 200, 20), Charges+"/"+maxCharges);
        GUI.Label(new Rect(20, 100, 200, 20), "HP: "+HP);
        GUI.Label(new Rect(20, 130, 200, 20), "Energy: "+energy);
        GUI.Label(new Rect(20, 160, 200, 20), _state.GetType().Name);
    }

    /* ICollidable */
    private Collider _collider;
    public int ImmaterialStack { get; set; } = 0;

    bool OnControllerColliderHit(ControllerColliderHit hit) {
        GameObject collisionObject = hit.collider.gameObject;

        if (collisionObject.GetComponent<ICollidable>() is ICollidable collidable) {
            return State.HandleCollisionWithStates(collidable, new CollisionInfo(hit.normal));
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
            return _state.HandleCollisionWithStates(other, info);
        }
    }

    public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this);
    }

    public Collider Collider { get { return GetComponent<Collider>(); } }

    /* Control Recording */
    private bool _recordingControls;
    private bool _collectingControls;
    private bool _collectingControlsRestart;
    private FileStream _recordControlStream = null;
    private void WriteCharacterFrameInput(FileStream outputStream) {
        CharacterFrameInput input = new CharacterFrameInput(
            InputAimDirection,
            InputMoveDirection,
            InputBlocking,
            InputShielding,
            InputRunning,
            InputCastId
        );

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(outputStream, input);
    }

    public void LoadCharacterFrameInput(CharacterFrameInput input) {
        InputAimDirection = new Vector3(input.AimDirectionX, 0, input.AimDirectionZ);
        InputMoveDirection = new Vector2(input.MoveDirectionX, input.MoveDirectionZ);
        InputRunning = input.Running;
        InputCastId = input.CastId;
        InputBlocking = input.Blocking;
        InputShielding = input.Shielding;
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
                if (me) { characterControls.Enable(); }
            }
        }

        return ret;
    }// TODO collect inputs of user to replay them
}
