using static CharacterControls;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
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
    [SerializeField] public bool me { get; set; } = false;

    /* State WIP */
    CharacterState _state;
    public CharacterState State { get { return _state; } set { _state = value; } }
    CharacterStateFactory StateFactory;

    /* Movement */
    public CharacterController cc;
    private Vector3 _velocity = new();
    public Vector3 Velocity { get { return _velocity; } set { _velocity = value; } }

    public CommandMovement CommandMovement { get; private set; } = null;
    public bool Stunned { get; set; } = false;
    private float standingY; private float standingOffset = .1f;

    // knockback
    private int hitStunTimer = 0;
    public float KnockBackDecay { get; private set; } = 1f;
    public int HitLagTimer { get; private set; } = 0;

    // Walking
    private float WalkAcceleration = 75f;
    public float WalkSpeedMax { get; set; } = 7.5f;
    private float gravity = -.5f;

    // Running
    private float runDeceleration = 10f;
    private float runRotationalSpeed = 2.5f;

    // Bolt


    /* Visuals */
    // Bolt Visualization
    public Material Material { get; private set; }
    TrailRenderer tr;

    /* Controls */
    private CharacterControls characterControls;
    private bool RotatingClockwise = false;
    [SerializeField] public float MinimumRotationThreshold = 1f;
    public Transform CursorTransform { get; private set; }
    private Plane aimPlane;
    public Vector3 InputMoveDirection = new();
    public bool InputDash = false;
    public bool InputRunning = false;

    /* Abilities */
    [SerializeField] private CastSlot[] castSlots = Enum.GetNames(typeof(CastId)).Select(name => new CastSlot(name)).ToArray();
    public CastContainer[] castContainers = new CastContainer[Enum.GetNames(typeof(CastId)).Length];
    public static int[] boostedIds = new int[] { (int)CastId.BoostedLight, (int)CastId.BoostedMedium, (int)CastId.BoostedHeavy, (int)CastId.BoostedThrow };
    public static int[] specialIds = new int[] { (int)CastId.Special1, (int)CastId.Special2 };
    private int inputCastId = -1;
    public int CastIdBuffer = -1;

    /* Resources */
    public int HP { get; private set; } = HPMax; public static int HPMax = 1000;
    public bool Reflects { get; set; } = false;
    private int healMax; private int healMaxOffset = 100;
    private int maxCharges = 5;
    public int Charges = 3;
    private int chargeCooldownMax = 60;
    private int chargeCooldown = 0;
    private int rechargeRate = 300;
    private int rechargeTimer = 0;
    private int shieldDuration = -1;
    private int parryWindow = 30;
    private int energy = 100;
    private int maxEnergy = 100;

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
    public void OnBoost(InputAction.CallbackContext context) {
        InputDash=context.ReadValueAsButton();
    }

    // attacks
    public void OnLight1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Light1;
        }
    }
    public void OnLight2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Light2;
        }
    }
    public void OnBoostedLight(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.BoostedLight;
        }
    }
    public void OnMedium1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Medium1;
        }
    }
    public void OnMedium2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Medium2;
        }
    }
    public void OnBoostedMedium(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.BoostedMedium;
        }
    }
    public void OnHeavy1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Heavy1;
        }
    }
    public void OnHeavy2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Heavy2;
        }
    }
    public void OnBoostedHeavy(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (State is CharacterStateDashing)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.BoostedHeavy;
        }
    }

    // shields
    public void OnShield(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            Shield.gameObject.SetActive(true);
            Shield.ShieldTier = ShieldTier.Normal;
        } else {
            Shield.gameObject.SetActive(false);
            Shield.ShieldTier = ShieldTier.None;
        }
    }
    public void OnBoostedShield(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            Shield.gameObject.SetActive(true);
            Shield.ShieldTier = ShieldTier.Boosted;
        } else {
            Shield.gameObject.SetActive(false);
            Shield.ShieldTier = ShieldTier.None;
        }
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
    public bool IsRotatingClockwise() {
        return RotatingClockwise;
    }

    public Transform GetOriginTransform() {
        return transform;
    }

    public Transform GetTargetTransform() {
        return CursorTransform;
    }

    /* State Machine */
    public void SwitchState(CharacterState state) {
        State = state;
        State.EnterState();
    }

    /* MonoBehavior */
    private void Awake() {
        // Controls
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
        if (HitLagTimer>0)
            Material.color=Color.red;
        else if (Reflects)
            Material.color=Color.blue;
        else if (inputCastId >= 0)
            Material.color=Color.magenta;
        else if (chargeCooldown<0&&Charges>0)
            Material.color=Color.green;
        else
            Material.color=Color.gray;

        /*
         * Trail Renderer
         */
        tr.emitting=(Velocity.magnitude>12.6f); // TODO return to this later
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
    public Vector3 GetLookDirection() {
        Vector3 LookDirection = CursorTransform.position-cc.transform.position;
        LookDirection.y = 0;
        return LookDirection.normalized;
    }

    public void SetCommandMovement(CommandMovement _commandMovement) {
        CommandMovement = _commandMovement;
        Velocity = CommandMovement.Velocity.normalized * Velocity.magnitude; // TODO maybe not the best spot to do this
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
        if (Shield.ShieldTier == ShieldTier.None) return 0;
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
        if (Shield.ShieldTier == ShieldTier.Boosted) {

            return 2.5f * (1-Vector3.Dot(velocity.normalized, lookVector.normalized));
        } else return 0;
    }

    private float GetVerticalVelocity(float currentYVelocity, float gravity) {
        int excludeAllButTerrainLayer = 1<<LayerMask.NameToLayer("Terrain");
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, cc.radius+standingOffset+.01f, excludeAllButTerrainLayer);

        if (currentYVelocity <= 0 && hitInfo.transform!=null) {
            // // snap the character to the standing Height - TODO current implementation seems hacky, but it works?
            standingY = hitInfo.point.y + cc.radius + standingOffset;
            transform.position = new Vector3(transform.position.x, standingY, transform.position.z);
            return 0f;
        } else {
            return currentYVelocity + gravity;
        }

    }

    private bool IsAboveWalkSpeed() {
        return (Velocity.magnitude>(WalkSpeedMax+.25));
    }

    private Vector3 GetDecayedVector(Vector3 vector, float decayRate) {
        return vector.normalized*Mathf.Max(vector.magnitude-decayRate*Time.deltaTime, 0);
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        GameObject collisionObject = hit.collider.gameObject;

        if (collisionObject.layer==LayerMask.NameToLayer("Terrain")) {
            if (InputRunning) { // if you hit a 
                if (IsAboveWalkSpeed()) {
                    Vector3 mirror = new Vector3(hit.normal.x, 0, hit.normal.z);
                    Vector3 bounceDirection = (Velocity-2*Vector3.Project(Velocity, mirror)).normalized;
                    Debug.Log("I pray you're not here");
                    Velocity = bounceDirection*Velocity.magnitude;
                } else {
                    Debug.Log("nor here");
                    Velocity.Set(0f, 0f, 0f);
                }
                HitLagTimer = 2;
                // TODO add something here:
                // - stop for some frames
            } else {
                Debug.Log("or even here please");
                Velocity=Vector3.zero;
            }
        } else if (collisionObject.layer==LayerMask.NameToLayer("Characters")&&IsAboveWalkSpeed()) {

            Character otherCharacter = collisionObject.GetComponent<Character>();

            if (otherCharacter.Velocity.magnitude<Velocity.magnitude) {
                // TODO I haven't tested this on moving targets yet, so I haven't tested the second term
                // TODO figure out this implementation and calibrate the deceleration more - the behavior is very confusing right now
                // TODO hardcoding the 5f/20 because that used to be some stupid calculationfrom dash decay, which was constant
                Vector3 dvNormal = Vector3.Project(Velocity-otherCharacter.Velocity, hit.normal)*(-(5f/20))*Time.deltaTime*30;
                dvNormal.y = 0f;
                Velocity-=dvNormal;
                otherCharacter.Velocity=Velocity+dvNormal;
            }
        }
    }

    public void UpdateCursorTransformWorldPosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (aimPlane.Raycast(ray, out float distance)) {
            CursorTransform.position = ray.GetPoint(distance);
        } else {
            CursorTransform.position = transform.position; // TODO is this good? currently, the cursor will just point to self
        }
    }

    /* IMoves Methods */
    public Transform GetTransform() { return transform; }
    public Vector3 GetVelocity() { return Velocity; }

    /*
    public void Move() {
        if (CommandMovement != null) {
            cc.Move(CommandMovement.GetDPosition());
            transform.rotation = CommandMovement.GetRotation(transform.position, transform.rotation);
        } else {
            Vector3 horizontalPlane = new Vector3(1, 0, 1);
            Vector3 HorizontalVelocity = Vector3.Scale(horizontalPlane, Velocity);
            float verticalVelocity = GetVerticalVelocity(Velocity.y, gravity);

            if (HitLagTimer-- > 0) {
                return; // no-op - don't move, don't update velocity, no nothing
            } else if (hitStunTimer-- > 0) {
                float acceleration = (verticalVelocity==0f) ? KnockBackDecay : 0f;
                HorizontalVelocity = GetDecayedVector(HorizontalVelocity, acceleration);
            } else { // normal movement
                HorizontalVelocity = GetHorizontalVelocity(HorizontalVelocity, InputMoveDirection);
            }

            Velocity = HorizontalVelocity + Vector3.up * verticalVelocity;
            cc.Move(Velocity*Time.deltaTime);
        }
    }*/

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

    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, HitTier hitTier) {
        float knockBackFactor = HitsShield(contactPoint)
            ? GetKnockBackFactor(hitTier, (Shield.isActiveAndEnabled ? Shield.ShieldTier : 0))
            : 1f;

        if (knockBackFactor > 0) {
            Destroy(CommandMovement);
            CommandMovement = null;

            // hit lag, hit stun
            HitLagTimer = hitLagDuration;
            hitStunTimer = hitStunDuration;

            if (knockBackVector != Vector3.zero) {
                Velocity = knockBackFactor*knockBackVector;
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
    public void TakeDamage(Vector3 _contactPoint, int damage, HitTier hitTier) {
        // TODO add some sort of implementation to ignore shield
        if ((hitTier!=HitTier.Pure) && (invincible || HitsShield(_contactPoint))) return;

        HP = (HP-damage<HP) ? (HP-damage) : Mathf.Min(HP-damage, healMax);
        healMax = Mathf.Min(healMax, HP+healMaxOffset);

        if (HP<=0) {
            OnDeath();
        }
    }

    public void TakeHeal(int damage) {
        HP = Math.Max(HP+damage, healMax);
    }

    public void OnDeath() {
        Destroy(gameObject);
    }

    void OnGUI() {
        // TODO remove: here for debugging
        if (!me) return;
        GUI.Label(new Rect(20, 40, 80, 20), Velocity.magnitude+"m/s");
        GUI.Label(new Rect(20, 70, 80, 20), Charges+"/"+maxCharges);
        GUI.Label(new Rect(20, 100, 80, 20), "HP: "+HP);
        GUI.Label(new Rect(20, 130, 80, 20), "Energy: "+energy);
    }

    /* ICollidable */
    public void OnCollideWith(ICollidable other) {
        ; // TODO noop for now - maybe the Character never has to be the one to handle their own collisions? I don't know yet
    }

    public void HandleCollisions() {
        ;
    }

    public Collider GetCollider() {
        // TODO this might be poor performance, but it's the quickest way for me to get this for now
        return GetComponent<Collider>();
    }
}
