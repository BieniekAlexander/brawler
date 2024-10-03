using static CharacterControls;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine;

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
}

public struct CastContainer {
    public CastContainer(CastSlot _castSlot) {
        castPrefab = _castSlot.castPrefab;
        cast = null;
        cooldown = _castSlot.cooldown;
        timer = 0;
        charges = _castSlot.defaultChargeCount;
    }

    public Cast castPrefab;
    public Cast cast;
    public int cooldown;
    public int timer;
    public int charges;
}

public enum CastId {
    Light1 = 0,
    Light2 = 1,
    BoostedLight = 2,
    Medium1 = 3,
    Medium2 = 4,
    BoostedMedium = 5,
    Heavy1 = 6,
    Heavy2 = 7,
    BoostedHeavy = 8,
    DashAttack = 9,
    Throw1 = 10,
    Throw2 = 11,
    BoostedThrow = 12,
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

    /* State WIP */
    CharacterState _state;
    public CharacterState State { get { return _state; } set { _state = value; } }
    CharacterStateFactory StateFactory;

    /* Movement */
    public CharacterController cc { get; set; }
    public Vector3 Velocity { get { return HorizontalVelocity + Vector3.up*VerticalVelocity; } private set { throw new NotImplementedException("TODO don't set this directly"); } }
    public Vector3 HorizontalVelocity = new();
    public float VerticalVelocity = new();
    public bool CastEncumbered { get; set; } = false;

    public CommandMovement CommandMovement { get; private set; } = null;
    public bool Stunned { get; set; } = false;
    private float standingY; private float standingOffset = .1f;

    // knockback
    public HitTier KnockBackHitTier { get; set; }
    public float KnockBackDecay { get; private set; } = 1f;
    public Vector3 KnockBack { get; set; } = new();
    public int HitStunTimer { get; set; } = 0;
    public int HitStopTimer { get; set; } = 0;
    public int RecoveryTimer { get; set; } = 0;
    public int BusyTimer { get; set; } = 0;

    // Walking
    public float WalkSpeedMax { get; set; } = 7.5f;
    private float gravity = -.5f;

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
    private Vector3 _inputMoveDirection = new();
    public Vector3 InputMoveDirection {
        get {
            return CastEncumbered ? Vector3.zero : _inputMoveDirection;
        }
        set {
            _inputMoveDirection = value;
        }
    }
    public bool InputDash { get; set; } = false;
    public bool InputRunning { get; set; } = false;
    public bool InputBlocking { get; set; } = false;
    public bool InputShielding { get; set; } = false;

    /* Abilities */
    [SerializeField] private CastSlot[] castSlots = Enum.GetNames(typeof(CastId)).Select(name => new CastSlot(name)).ToArray();
    public CastContainer[] castContainers = new CastContainer[Enum.GetNames(typeof(CastId)).Length];
    public static int[] boostedIds = new int[] { (int)CastId.BoostedLight, (int)CastId.BoostedMedium, (int)CastId.BoostedHeavy, (int)CastId.BoostedThrow };
    public static int[] specialIds = new int[] { (int)CastId.Special1, (int)CastId.Special2 };
    private int inputCastId = -1;
    public int CastIdBuffer { get; set; } = -1;

    /* Resources */
    public int HP { get; set; } = HPMax;
    public static int HPMax = 1000;
    public bool Reflects { get; set; } = false;
    private int healMax; private int healMaxOffset = 100;
    private int maxCharges = 5;
    public int Charges { get; set; } = 3;
    private int chargeCooldownMax = 60;
    private int chargeCooldown = 0;
    private int rechargeRate = 300;
    private int rechargeTimer = 0;
    private int shieldDuration = -1;
    private int parryWindow = 30;
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
    private bool invincible = false;

    /* Children */
    [SerializeField] public Shield ShieldPrefab;
    public Shield Shield { get; private set; }

    /*
     * CONTROLS
     */
    //movement
    public void OnMovement(InputAction.CallbackContext context) {
        var direction = context.ReadValue<Vector2>();
        InputMoveDirection = (direction.x==0 && direction.y==0) ? Vector3.zero : new Vector3(direction.x, 0, direction.y).normalized;
    }
    public void OnRunning(InputAction.CallbackContext context) {
        // TODO fix tihs - shielding sets running to false
        InputRunning=context.ReadValueAsButton();
    }
    public void OnDash(InputAction.CallbackContext context) {
        InputDash=context.ReadValueAsButton();
    }

    // attacks
    public void OnLight1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.Light1;
        }
    }
    public void OnLight2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.Light2;
        }
    }
    public void OnBoostedLight(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.BoostedLight;
        }
    }
    public void OnMedium1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.Medium1;
        }
    }
    public void OnMedium2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.Medium2;
        }
    }
    public void OnBoostedMedium(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.BoostedMedium;
        }
    }
    public void OnHeavy1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.Heavy1;
        }
    }
    public void OnHeavy2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.Heavy2;
        }
    }
    public void OnBoostedHeavy(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            CastIdBuffer = (int)CastId.BoostedHeavy;
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
            CastIdBuffer = (int)CastId.Throw1;
        }
    }
    public void OnThrow2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Throw2;
    }
    public void OnBoostedThrow(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.BoostedThrow;
    }

    // abilities
    public void OnAbility1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Ability1;
    }
    public void OnAbility2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Ability2;
    }
    public void OnAbility3(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Ability3;
    }
    public void OnAbility4(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Ability4;
    }
    public void OnSpecial1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Special1;
    }
    public void OnSpecial2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Special2;
    }
    public void OnUltimate(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Ultimate;
    }

    public void SetMe() {
        me = true; // work on this more
        characterControls = new CharacterControls();
        characterControls.Enable(); // TODO do I need to disable this somewhere?
        characterControls.character.SetCallbacks(this);
    }

    void HandleControls() {
        if (me) {
            // aiming TODO move to input manager
            UpdateCursorTransformWorldPosition();
            var playerPosition = cc.transform.position;
            aimPlane.SetNormalAndPosition(Vector3.up, playerPosition);
            var direction = new Vector3(CursorTransform.position.x-playerPosition.x, 0, CursorTransform.position.z-playerPosition.z).normalized;
            Quaternion newRotation = Quaternion.FromToRotation(Vector3.forward, direction);

            float yRotationDiff = Mathf.DeltaAngle(newRotation.eulerAngles.y, transform.rotation.eulerAngles.y);
            if (Mathf.Abs(yRotationDiff) > MinimumRotationThreshold) {
                RotatingClockwise = (yRotationDiff<0);
            }

            transform.rotation = newRotation;
        }
    }

    /* Casts */
    /// <summary/>
    /// <param name="castId"></param>
    /// <returns>Whether the cast was initiated or not</returns>
    private bool StartCast(int castId) {
        ref CastContainer castContainer = ref castContainers[castId];

        if (castContainer.cast is not null) {
            // we're updating another cast - allowed
            castContainer.cast.Recast(CursorTransform);
            return true;
        } else {
            if (castContainer.charges==0) {
                return false;
            } else {
                // nothing is being casted, so start this new one
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

                    castContainer.cast = Cast.Initiate(
                        castContainer.castPrefab,
                        this,
                        transform,
                        CursorTransform,
                        RotatingClockwise);

                    castContainer.charges--;
                    castContainer.timer = castContainer.cooldown;
                    CastEncumbered = castContainer.castPrefab.Encumbering;
                    BusyTimer = castContainer.castPrefab.startupTime;
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
        // resolve cooldowns and cast expirations
        for (int i = 0; i<castContainers.Length; i++) {
            if (castContainers[i].charges < castSlots[i].defaultChargeCount) {
                if (--castContainers[i].timer<=0) {
                    castContainers[i].charges++;
                    castContainers[i].timer = castSlots[i].cooldown;
                }
            }

            if (castContainers[i].cast == null) {
                castContainers[i].cast = null;
            }
        }
        // resolve cast startup
        int j = (int)inputCastId;
        if (j >= 0) { // if an active cast is designated
            if (castContainers[j].cast == null) {
                inputCastId = -1;
            } else if (castContainers[j].cast.Frame >= castContainers[j].cast.startupTime) { // if startup time has elapsed
                inputCastId = -1;
            }
        }
    }

    /* ICasts Methods */
    // TODO bake these into getters
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
                from castContainer in castContainers
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
        GameObject cursorGameObject = new GameObject("Player Cursor Object");
        cursorGameObject.transform.parent = transform;
        CursorTransform = cursorGameObject.transform;
        aimPlane = new Plane(Vector3.up, transform.position);

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
            castContainers[i] = new CastContainer(castSlots[i]);
        }
    }

    private void HandleVisuals() {
        /*
         * Bolt Indicator
         */
        // duration
        if (HitStopTimer>0)
            Material.color=new Color(0, 0, 0);
        else if (_state is CharacterStateBlownBack // disadvantage
            || _state is CharacterStateKnockedBack
            || _state is CharacterStatePushedBack)
            Material.color=new Color(255, 0, 0);
        else if (_state is CharacterStateTumbling // vulnerable
            || _state is CharacterStateKnockedDown)
            Material.color=new Color(125, 125, 0);
        else if (_state is CharacterStateRolling // recovering
            || _state is CharacterStateGettingUp)
            Material.color=new Color(255, 255, 0);
        else if (inputCastId >= 0)
            Material.color=Color.magenta;
        else if (chargeCooldown<0&&Charges>0)
            Material.color=Color.green;
        else
            Material.color=Color.gray;

        /*
         * Trail Renderer
         */
        tr.emitting=(HorizontalVelocity.magnitude>12.6f); // TODO return to this later
    }

    private void Update() {
        // handle inputs
        HandleControls();
        HandleVisuals();
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
        // Handle Casts
        State.FixedUpdateStates();

        if (CommandMovement !=  null) {
            cc.Move(CommandMovement.GetDPosition());
        } else if (HitStopTimer==0) {
            // TODO I think I'm gonna need more vertical velocity considerations wrt state
            VerticalVelocity = GetVerticalVelocity(gravity);
            cc.Move((HorizontalVelocity+Vector3.up*VerticalVelocity)*Time.deltaTime);
        }

        HandleCharges();
        if (CastIdBuffer >= 0) {
            if (StartCast(CastIdBuffer)) {
                CastIdBuffer = -1;
            }
        }
        TickCasts();
        //Move();
        HandleCollisions();

        // apply statusEffects
        for (int i = 0; i < statusEffects.Count; i++) {
            statusEffects[i].Tick();
        }

        statusEffects.RemoveAll((Effect effect) => { return (effect == null); });
    }

    // Movement evaluations
    public Vector3 LookDirection {
        get {
            Vector3 LookDirection = CursorTransform.position-cc.transform.position;
            LookDirection.y = 0;
            return LookDirection.normalized;
        }
        set {
            // used for AI
            CursorTransform.position = cc.transform.position+value;
            transform.rotation = Quaternion.LookRotation(value, Vector3.up);
        }
    }

    public void SetCommandMovement(CommandMovement _commandMovement) {
        CommandMovement = _commandMovement;
        HorizontalVelocity = CommandMovement.Velocity.normalized * HorizontalVelocity.magnitude; // TODO maybe not the best spot to do this
    }

    /// <summary>
    /// Determines the acceleration of the Character's movemnt while the shield is up
    /// Generally, the Character shall slow down when the direction of their shield is in the direction of their movement
    /// </summary>
    /// <param name="level"></param>
    /// <param name="velocity"></param>
    /// <param name="lookVector"></param>
    /// <returns></returns>
    private float GetShieldAccelerationFactor(ShieldTier level, Vector3 velocity, Vector3 lookVector) {
        if (Shield.ShieldTier == ShieldTier.Exposed) return 0;
        else {
            float x = (int)Shield.ShieldTier * Vector3.Dot(velocity.normalized, lookVector.normalized);
            return Mathf.Pow(x, (int)Shield.ShieldTier);
        }
    }

    /// <summary>
    /// Determines how much the player's velocity rotates when using a boosted shield
    /// </summary>
    /// <param name="level"></param>
    /// <param name="velocity"></param>
    /// <param name="lookVector"></param>
    /// <returns></returns>
    private float GetShieldRotationFactor(ShieldTier level, Vector3 velocity, Vector3 lookVector) {
        if (Shield.ShieldTier == ShieldTier.Shielding) {

            return 2.5f * (1-Vector3.Dot(velocity.normalized, lookVector.normalized));
        } else return 0;
    }

    public bool IsGrounded() {
        if (VerticalVelocity>0) {
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

    private float GetVerticalVelocity(float gravity) {
        if (IsGrounded()) {
            // // snap the character to the standing Height - TODO current implementation seems hacky, but it works?
            transform.position = new Vector3(transform.position.x, standingY, transform.position.z);
            return 0f;
        } else {
            return VerticalVelocity + gravity;
        }
    }

    private Vector3 GetDecayedVector(Vector3 vector, float decayRate) {
        return vector.normalized*Mathf.Max(vector.magnitude-decayRate*Time.deltaTime, 0);
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        GameObject collisionObject = hit.collider.gameObject;

        if (collisionObject.GetComponent<ICollidable>() is ICollidable collidable) {
            OnCollideWith(collidable, new CollisionInfo(hit.normal));
        } else {
            throw new Exception("Unhandled collision type");
        }
    }

    public void UpdateCursorTransformWorldPosition() {
        if (me){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (aimPlane.Raycast(ray, out float distance)) {
                CursorTransform.position = ray.GetPoint(distance);
            } else {
                CursorTransform.position = transform.position; // TODO is this good? currently, the cursor will just point to self
            }
        }
    }

    /* IMoves Methods */
    public Transform GetTransform() { return transform; }
    public Vector3 GetVelocity() { return HorizontalVelocity; }

    public static float GetKnockBackFactor(HitTier HitTier, ShieldTier ShieldTier) {
        if (HitTier==HitTier.Soft) return 0;
        else if (HitTier==HitTier.Pure) return 1;
        else return ((int)HitTier - (int)ShieldTier)/(float)HitTier;
    }

    /// <summary>
    /// Returns true if the contact poins is not intersecting with the <paramref name="Character"/>'s <paramref name="Shield"/>
    /// </summary>
    /// <remarks>I'll leave it up to the caller as to where the <paramref name="contactPoint"/> is</remarks>
    /// <param name="contactPoint"></param>
    /// <returns></returns>
    private bool HitsShield(Vector3 contactPoint) {
        // I want the attack to hit shield if the contact point is not inside the shield
        // 
        /// https://www.desmos.com/3d
        // \left(\left(x-.5\right)\right)^{2}+\left(y\right)^{2}>\left(x^{2}+y^{2}\right)
        // x^{2}+y^{2}=1
        if (Shield.isActiveAndEnabled) {
            return !(Shield.GetCollider().bounds.Contains(contactPoint));
        } else {
            return false;
        }
    }

    public float TakeKnockBack(Vector3 contactPoint, int hitStopDuration, Vector3 knockBackVector, int hitStunDuration, HitTier hitTier) {
        float knockBackFactor = HitsShield(contactPoint)
            ? GetKnockBackFactor(hitTier, (Shield.isActiveAndEnabled ? Shield.ShieldTier : 0))
            : 1f;

        // TODO account for pushback of HitTier.Soft
        KnockBackHitTier = hitTier;

        if (knockBackFactor > 0) {
            Destroy(CommandMovement);
            CommandMovement = null;
            KnockBack = knockBackFactor*knockBackVector;
            HitStunTimer = hitStunDuration;
        }

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
                HorizontalVelocity += KnockBack;
            }
        }

        return knockBackFactor;
    }

    /* IDamageable Methods */
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_contactPoint"></param>
    /// <param name="damage"></param>
    public int TakeDamage(Vector3 _contactPoint, int damage, HitTier hitTier) {
        // TODO add some sort of implementation to ignore shield
        if ((hitTier!=HitTier.Pure) && (invincible || HitsShield(_contactPoint))) return 0;

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

    public void OnDeath() {
        // Destroy(gameObject); // TODO disabling for RL
    }

    void OnGUI() {
        // TODO remove: here for debugging
        if (!me) return;
        GUI.Label(new Rect(20, 40, 200, 20), HorizontalVelocity.magnitude+"m/s");
        GUI.Label(new Rect(20, 70, 200, 20), Charges+"/"+maxCharges);
        GUI.Label(new Rect(20, 100, 200, 20), "HP: "+HP);
        GUI.Label(new Rect(20, 130, 200, 20), "Energy: "+energy);
        GUI.Label(new Rect(20, 160, 200, 20), _state.GetType().Name);
    }

    /* ICollidable */
    public void OnCollideWith(ICollidable other, CollisionInfo info) {
        _state.OnCollideWith(other, info);
    }

    public void HandleCollisions() {
        ;
    }

    public Collider GetCollider() {
        // TODO this might be poor performance, but it's the quickest way for me to get this for now
        return GetComponent<Collider>();
    }
}
