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
        name = _name;
        castPrefab = null;
        cooldown = -1;
        defaultChargeCount = 1;
    }

    public string name;
    public Cast castPrefab;
    public int cooldown;
    public int defaultChargeCount;
    public bool overwriteOnNewCast = true;
}

public class CastContainer {
    public CastContainer(CastSlot _castSlot) {
        castPrefab = _castSlot.castPrefab;
        cast = null;
        cooldown = _castSlot.cooldown;
        timer = 0;
        charges = _castSlot.defaultChargeCount;
        overwriteOnNewCast = _castSlot.overwriteOnNewCast;
}

    public Cast castPrefab;
    public Cast cast;
    public List<Castable> castables; // TODO if we have castables still active from a previous cast, and we create a new cast, use this to inherit the castables from the old one
    public int cooldown;
    public int timer;
    public int charges;
    public bool overwriteOnNewCast;
}

public enum CastId {
    Light1 = 0,
    Light2 = 1,
    LightS = 2,
    Medium1 = 3,
    Medium2 = 4,
    MediumS = 5,
    Heavy1 = 6,
    Heavy2 = 7,
    HeavyS = 8,
    DashAttack = 9,
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
        Vector2 moveDirection,
        Vector3 aimDirection,
        bool blocking,
        bool shielding,
        bool running,
        bool dash,
        int castId) {
        MoveDirectionX = moveDirection.x;
        MoveDirectionZ = moveDirection.y;
        AimDirectionX = aimDirection.x;
        AimDirectionZ = aimDirection.z;
        Blocking = blocking;
        Shielding = shielding;
        Running = running;
        Dash = dash;
        CastId = castId;
    }

    public float MoveDirectionX = 0f;
    public float MoveDirectionZ = 0f;
    public float AimDirectionX = 0f;
    public float AimDirectionZ = 0f;
    public bool Blocking = false;
    public bool Shielding = false;
    public bool Running = false;
    public bool Dash = false;
    public int CastId = -1;
}

public class Character : MonoBehaviour, IDamageable, IMoves, ICasts, ICharacterActions, ICollidable {
    // is this me? TODO better way to do this
    [SerializeField] public bool me = false;

    /* State WIP */
    CharacterState _state;
    public CharacterState State { get { return _state; } set { _state = value; } }
    CharacterStateFactory StateFactory;

    public int BusyTimer { get; set; } = 0;
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
            return (CastEncumberedTimer>0) ? Vector3.zero : new Vector3(InputMoveDirection.x, 0f, InputMoveDirection.y);
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
            Quaternion newRotation = Quaternion.FromToRotation(Vector3.forward, direction);

            float yRotationDiff = Mathf.DeltaAngle(newRotation.eulerAngles.y, transform.rotation.eulerAngles.y);
            if (Mathf.Abs(yRotationDiff) > MinimumRotationThreshold) {
                RotatingClockwise = (yRotationDiff<0);
            }

            transform.rotation = newRotation;
        }
    }

    public bool InputDash { get; set; } = false;
    public bool InputRunning { get; set; } = false;
    public bool InputBlocking { get; set; } = false;
    public bool InputShielding { get; set; } = false;

    /* Abilities */
    [SerializeField] private CastSlot[] castSlots = Enum.GetNames(typeof(CastId)).Select(name => new CastSlot(name)).ToArray();
    public CastContainer[] CastContainers = new CastContainer[Enum.GetNames(typeof(CastId)).Length];
    public static int[] boostedIds = new int[] { (int)CastId.LightS, (int)CastId.MediumS, (int)CastId.HeavyS, (int)CastId.ThrowS };
    public static int[] specialIds = new int[] { (int)CastId.Special1, (int)CastId.Special2 };
    public int CastBufferTimer { get; private set; } = 0;
    private CastContainer _activeCastContainer = null;
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

    /* Resources */
    private int maxCharges = 5;
    public int Charges { get; set; } = 3;
    private int chargeCooldownMax = 60;
    private int chargeCooldown = 0;
    private int rechargeRate = 300;
    private int rechargeTimer = 0;
    private int shieldDuration = -1;
    private int energy = 100;
    private int maxEnergy = 100;

    /* Signals */
    // this is probably not a very good way to implement this, but I just want to propagate damage dealt by my RL agent to learning
    private int _damageDealt;
    public int DamageDealt {
        get { int temp = _damageDealt; _damageDealt=0; return temp; } // so, when the value is read, it's set to zero
        set {
            _damageDealt=value;
        }
    }

    /* Status Effects */
    private List<Effect> statusEffects = new();

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
        InputDash=context.ReadValueAsButton();
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
            MoveDirection,
            InputAimDirection,
            InputBlocking,
            InputShielding,
            InputRunning,
            InputDash,
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

    /* Casts */
    /// <summary/>
    /// <param name="castId"></param>
    /// <returns>Whether the cast was initiated or not</returns>
    private bool StartCast(int castId) {
        ref CastContainer castContainer = ref CastContainers[castId];

        if (
            castContainer.cast is not null // cast has not expired
            && castContainer.charges == 0 // cast is still on cooldown
        ) {
            // we're updating another cast - allowed
            return castContainer.cast.Recast(CursorTransform);
        } else {
            if (castContainer.charges==0 || BusyTimer>0) {
                return false;
            } else {
                if (castContainer.castPrefab == null) {
                    Debug.Log("No cast supplied for cast"+(int)castId);
                    return true;
                }
                if (
                    (Charges>0 || !boostedIds.Contains(castId))
                    && (energy >= 50 || !specialIds.Contains(castId))
                    && (energy >= 100 || castId!=(int)CastId.Ultimate)
                ) {
                    if (boostedIds.Contains(castId)) {
                        Charges--;
                    } else if (specialIds.Contains(castId)) {
                        energy -= 50;
                    } else if (castId == (int)CastId.Ultimate) {
                        energy -= 100;
                    }

                    if (_activeCastContainer!=null && _activeCastContainer.cast != null && _activeCastContainer.overwriteOnNewCast) {
                        // TODO I think I'll want the new Cast to inheret any active Castables from any previous casts
                        Destroy(_activeCastContainer.cast.gameObject);
                    }

                    _activeCastContainer = castContainer;
                    castContainer.cast = Cast.Initiate(
                        castContainer.castPrefab,
                        this,
                        transform,
                        CursorTransform,
                        RotatingClockwise);

                    castContainer.charges--;
                    castContainer.timer = castContainer.cooldown;
                    BusyTimer = castContainer.castPrefab.startupTime;
                    CastEncumberedTimer = (castContainer.castPrefab.Encumbering) ? BusyTimer : 0;
                    return true;
                } else {
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Reduces all cooldowns and evalutes cast completions
    /// </summary>
    /// <returns>The index if the cast being casted, or -1 if otherwise</returns>
    void TickCasts() {
        CastEncumberedTimer--;

        // resolve cooldowns and cast expirations
        for (int i = 0; i<CastContainers.Length; i++) {
            if (CastContainers[i].charges < castSlots[i].defaultChargeCount) {
                if (--CastContainers[i].timer<=0) {
                    CastContainers[i].charges++;
                    CastContainers[i].timer = castSlots[i].cooldown;
                }
            }

            if (CastContainers[i].cast == null) {
                CastContainers[i].cast = null;
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

    public IEnumerable<Castable> ActiveCastables {
        get {
            return (
                from castContainer in CastContainers
                where castContainer.cast != null
                select castContainer.cast.ActiveCastables
            ).SelectMany(castable => castable);
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
        StateFactory = new(this);
        SwitchState(StateFactory.Idle());

        cc =GetComponent<CharacterController>();
        standingY = transform.position.y + standingOffset;
        healMax = HP;

        // Children
        Shield = Instantiate(ShieldPrefab, transform);
        Shield.gameObject.SetActive(false);

        // Visuals
        Material = GetComponent<Renderer>().material;
        tr = GetComponent<TrailRenderer>();

        for (int i = 0; i<castSlots.Length; i++) {
            CastContainers[i] = new CastContainer(castSlots[i]);
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
        else if (chargeCooldown<0&&Charges>0)
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

        chargeCooldown--;
    }

    void FixedUpdate() {
        if (_recordingControls) {
            WriteCharacterFrameInput(_recordControlStream);
        } else if (_collectingControls) {
            LoadCharacterFrameInput(ReadCharacterFrameInput(_recordControlStream));
        }

        // Handle Casts
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

        // apply statusEffects
        for (int i = 0; i < statusEffects.Count; i++) {
            statusEffects[i].Tick();
        }

        statusEffects.RemoveAll((Effect effect) => { return (effect == null); });
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
    public int CastEncumberedTimer { get; set; } = 0;
    public int StunStack { get; set; } // stuns
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
                Invulnerability.Initialize(this);
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
                HitStunTimer = hitStunDuration;
                CastEncumberedTimer = 0;
                BusyTimer = 0;

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
    public int ParryWindow { get; set; } = 0;
    public int InvulnerableStack { get; set; } = 0;

    public int TakeDamage(Vector3 _contactPoint, int damage, HitTier hitTier) {
        // TODO add some sort of implementation to ignore shield
        // TODO add taking damage from armor before HP
        if ((hitTier!=HitTier.Pure) && (InvulnerableStack>0 || HitsShield(_contactPoint))) return 0;

        HP -= damage;
        healMax = Mathf.Min(healMax, HP+healMaxOffset);

        if (HP<=0) {
            OnDeath();
        }

        return damage-Math.Min(HP, 0);
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

    void OnControllerColliderHit(ControllerColliderHit hit) {
        GameObject collisionObject = hit.collider.gameObject;

        if (collisionObject.GetComponent<ICollidable>() is ICollidable collidable) {
            State.HandleCollisionWithStates(collidable, new CollisionInfo(hit.normal));
        } else {
            throw new Exception("Unhandled collision type");
        }
    }

    public void OnCollideWith(ICollidable other, CollisionInfo info) {
        if (other is Character otherCharacter
            && (
                ImmaterialStack > 0
                || otherCharacter.ImmaterialStack > 0
            )
        ) return;
        else {
            _state.HandleCollisionWithStates(other, info);
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
            InputMoveDirection,
            InputAimDirection,
            InputBlocking,
            InputShielding,
            InputRunning,
            InputDash,
            InputCastId
        );

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(outputStream, input);
    }

    public void LoadCharacterFrameInput(CharacterFrameInput input) {
        InputMoveDirection = new Vector2(input.MoveDirectionX, input.MoveDirectionZ);
        InputAimDirection = new Vector3(input.AimDirectionX, 0, input.AimDirectionZ);
        InputDash = input.Dash;
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
