using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterControls;
using System.Collections.Generic;

[Serializable]
public class CastSlot {
    public CastSlot(string _name) {
        name = _name;
        castPrefab = null;
        cooldown = -1;
        defaultChargeCount = 1;
    }

    public string name;
    public CastBase castPrefab;
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

    public CastBase castPrefab;
    public CastBase cast;
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
    Throw1 = 9,
    Throw2 = 10,
    BoostedThrow = 11,
    DashAttack = 12,
    Ability1 = 13,
    Ability2 = 14,
    Ability3 = 15,
    Ability4 = 16,
    Special1 = 17,
    Special2 = 18,
    Ultimate = 19
}

public class Character : MonoBehaviour, ICharacterActions {
    // is this me? TODO better way to do this
    [SerializeField] bool me;

    /* Movement */
    private CharacterController cc;
    public Shield Shield { get; private set; }
    public bool RotatingClockwise { get; private set; } = false;
    public Vector3 MoveVelocity { get; private set; } = new();
    public CommandMovementBase CommandMovement { get; private set; } = null;
    public bool Stunned { get; set; } = false;
    private float standingY; private float standingOffset = .1f;

    // knockback
    public Vector3 KnockBackVelocity { get; private set; } = new();
    public float KnockBackDecceleration { get; private set; } = 25f;
    public int HitLagTimer { get; private set; } = 0;

    // Walking
    private float WalkAcceleration = 75f;
    public float WalkSpeedMax { get; set; } = 10f;
    private Vector3 Gravity = new Vector3(0, -.25f, 0);
    private Vector3 VerticalVelocity = Vector3.zero;

    // Running
    private float runDeceleration = 10f;
    private float runRotationalSpeed = 2.5f;

    // Bolt
    private int boostTimer = 0;
    private int boostMaxDuration = 20;
    private float boostSpeedBump = 2.5f;
    private float boostDecay;

    /* Visuals */
    // Bolt Visualization
    public Material Material { get; private set; }
    TrailRenderer tr;

    /* Controls */
    private CharacterControls characterControls;
    private Plane aimPlane;
    private Vector3 movementDirection = new();
    private bool boost = false;
    private bool running = false;
    private ShieldLevel ShieldLevel = ShieldLevel.None;

    /* Abilities */
    [SerializeField] private CastSlot[] castSlots = Enum.GetNames(typeof(CastId)).Select(name => new CastSlot(name)).ToArray();
    public CastContainer[] castContainers = new CastContainer[Enum.GetNames(typeof(CastId)).Length];
    public static int[] boostedIds = new int[] { (int)CastId.BoostedLight, (int)CastId.BoostedMedium, (int)CastId.BoostedHeavy, (int)CastId.BoostedThrow };
    public static int[] specialIds = new int[] { (int)CastId.Special1, (int)CastId.Special2 };
    private int inputCastId = -1;
    public int CastIdBuffer = -1;

    /* Resources */
    public int HP { get; private set; } = 1000;
    public bool Reflects { get; set; } = false;
    private int healMax; private int healMaxOffset = 100;
    private int maxCharges = 5;
    private int charges = 3;
    private int chargeCooldownMax = 60;
    private int chargeCooldown = 0;
    private int rechargeRate = 300;
    private int rechargeTimer = 0;
    private int shieldDuration = -1;
    private int parryWindow = 30;
    private int energy = 100;
    private int maxEnergy = 100;

    /* Status Effects */
    private List<StatusEffectBase> statusEffects = new();
    private bool invincible = false;

    /*
     * CONTROLS
     */
    //movement
    public void OnMovement(InputAction.CallbackContext context) {
        var direction = context.ReadValue<Vector2>();
        movementDirection = (direction.x==0 && direction.y==0) ? Vector3.zero : new Vector3(direction.x, 0, direction.y).normalized;
    }
    public void OnRunning(InputAction.CallbackContext context) {
        // TODO fix tihs - shielding sets running to false
        running=context.ReadValueAsButton();
    }
    public void OnBoost(InputAction.CallbackContext context) {
        boost=context.ReadValueAsButton();
    }

    // attacks
    public void OnLight1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Light1;
        }
    }
    public void OnLight2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Light2;
        }
    }
    public void OnBoostedLight(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.BoostedLight;
        }
    }
    public void OnMedium1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Medium1;
        }
    }
    public void OnMedium2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Medium2;
        }
    }
    public void OnBoostedMedium(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.BoostedMedium;
        }
    }
    public void OnHeavy1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Heavy1;
        }
    }
    public void OnHeavy2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.Heavy2;
        }
    }
    public void OnBoostedHeavy(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                CastIdBuffer = (int)CastId.DashAttack;
            else
                CastIdBuffer = (int)CastId.BoostedHeavy;
        }
    }

    // shields
    public void OnShield(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            ShieldLevel = ShieldLevel.Normal;
            Shield.gameObject.SetActive(true);
            Shield.ShieldLevel = ShieldLevel;
        } else {
            ShieldLevel = ShieldLevel.None;
            Shield.gameObject.SetActive(false);
        }
    }
    public void OnBoostedShield(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            ShieldLevel = ShieldLevel.Boosted;
            Shield.gameObject.SetActive(context.ReadValueAsButton());
            Shield.ShieldLevel = ShieldLevel;
        } else {
            ShieldLevel = ShieldLevel.None;
            Shield.gameObject.SetActive(false);
        }
    }

    // throws
    public void OnThrow1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            CastIdBuffer = (int)CastId.Throw1;
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

    void HandleControls() {
        if (me) {
            // aiming TODO move to input manager
            Vector3 cursorWorldPosition = GetCursorWorldPosition();
            var playerPosition = cc.transform.position;
            var direction = new Vector3(cursorWorldPosition.x-playerPosition.x, 0, cursorWorldPosition.z-playerPosition.z).normalized;
            Quaternion newRotation = Quaternion.FromToRotation(Vector3.forward, direction);
            float yRotationDiff = newRotation.y - transform.rotation.y;

            if (yRotationDiff != 0) {
                RotatingClockwise = (yRotationDiff>0);
                // TODO somewhere in Q3, the yRotation goes from positive to negative, and I don't know why,
                // but it's producing an issue where for about 10% of the circle, rotation direction is incorrect
                // Debug.Log("old"+transform.rotation.y+" new "+newRotation.y);
                // Debug.Log("yRotationDiff "+yRotationDiff +" RotatingClockwise "+rotatingClockwise);
            }

            transform.rotation = newRotation;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="castId"></param>
    /// <returns>Whether the cast was initiated or not</returns>
    private bool StartCast(int castId) {
        ref CastContainer castContainer = ref castContainers[castId];

        if (castContainer.cast is not null) {
            // we're updating another cast - allowed
            Vector3 cursorWorldPosition = GetCursorWorldPosition();
            return castContainer.cast.UpdateCast(cursorWorldPosition);
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
                    (charges>0 || !boostedIds.Contains(castId))
                    && (energy >= 50 || !specialIds.Contains(castId))
                    && (energy >= 100 || castId!=(int)CastId.Ultimate)
                ) {
                    if (boostedIds.Contains(castId)) {
                        charges--;
                    } else if (specialIds.Contains(castId)) {
                        energy -= 50;
                    } else if (castId == (int)CastId.Ultimate) {
                        energy -= 100;
                    }

                    castContainer.cast = Instantiate(castContainer.castPrefab);
                    castContainer.cast.Initialize(this, RotatingClockwise);
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

    private void Awake() {
        cc=GetComponent<CharacterController>();
        Shield = GetComponentInChildren<Shield>();
        Shield.gameObject.SetActive(false);
        Material = GetComponent<Renderer>().material;
        tr = GetComponent<TrailRenderer>();
        
        aimPlane = new Plane(Vector3.up, transform.position);
        standingY = transform.position.y + standingOffset;
        healMax = HP;

        if (me) // set up controls
        {
            characterControls = new CharacterControls();
            characterControls.Enable(); // TODO do I need to disable this somewhere?
            characterControls.character.SetCallbacks(this);
        }

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
        else if (chargeCooldown<0&&charges>0)
            Material.color=Color.green;
        else
            Material.color=Color.gray;

        /*
         * Trail Renderer
         */
        tr.emitting=(MoveVelocity.magnitude>(WalkSpeedMax+boostSpeedBump*2));
    }

    private void Update() {
        // handle inputs
        HandleControls();
        HandleVisuals();
    }

    private void HandleCharges() {
        rechargeTimer=(charges>=maxCharges) ? rechargeRate : rechargeTimer-1;
        if (rechargeTimer<=0) {
            charges+=1;
            rechargeTimer=rechargeRate;
        }

        chargeCooldown--;
    }

    void FixedUpdate() {
        // Handle Casts
        HandleCharges();
        if (CastIdBuffer >= 0) {
            if (StartCast(CastIdBuffer)) {
                CastIdBuffer = -1;
            }
        }
        TickCasts();

        // Update Position
        if (CommandMovement != null) {
            cc.Move(CommandMovement.GetDPosition());
        } else {
            if (HitLagTimer == 0) {
                MoveVelocity = GetMoveVelocity(MoveVelocity, movementDirection);
                KnockBackVelocity = GetDecayedVector(KnockBackVelocity, KnockBackDecceleration); // TODO this will decay the knockback before the first frame of movement - is this what I want? it's probably not a huge deal?
                cc.Move((MoveVelocity+KnockBackVelocity)*Time.deltaTime);
                transform.position = new Vector3(transform.position.x, standingY, transform.position.z); // TODO hardcoding the Y position, but I should fix the y calculations
            } else {
                HitLagTimer--;
            }
        }

        // apply statusEffects
        for (int i = 0; i < statusEffects.Count; i++) {
            statusEffects[i].Tick();
        }

        statusEffects.RemoveAll((StatusEffectBase effect) => { return (effect == null); });
    }

    // Movement evaluations
    private Vector3 GetLookDirection() {
        return (GetCursorWorldPosition()-cc.transform.position).normalized; 
    }

    /// <summary>
    /// Boost
    /// </summary>
    private Vector3 Boost(Vector3 currentVelocity) {
        //charges--;
        chargeCooldown = chargeCooldownMax;
        boostTimer = boostMaxDuration;
        float boostSpeedEnd = Mathf.Max(MoveVelocity.magnitude, WalkSpeedMax)+boostSpeedBump;
        float boostSpeedStart = boostSpeedEnd + 2*boostSpeedBump;
        boostDecay = (boostSpeedEnd-boostSpeedStart)/boostMaxDuration;

        KnockBackVelocity.Set(0f, 0f, 0f); // TODO return to this
        Vector3 v = GetLookDirection();
        return new Vector3(v.x, 0, v.z).normalized*boostSpeedStart;
    }

    public void SetCommandMovement(CommandMovementBase _commandMovement) {
        CommandMovement = _commandMovement;
        MoveVelocity = CommandMovement.Velocity.normalized * MoveVelocity.magnitude; // TODO maybe not the best spot to do this
    }

    /// <summary>
    /// TODO WIP
    /// </summary>
    /// <param name="level"></param>
    /// <param name="velocity"></param>
    /// <param name="lookVector"></param>
    /// <returns></returns>
    private float GetShieldAccelerationFactor(ShieldLevel level, Vector3 velocity, Vector3 lookVector) {
        if (ShieldLevel == ShieldLevel.None) return 0; else {
            float x = (int)ShieldLevel * Vector3.Dot(velocity.normalized, lookVector.normalized);
            return Mathf.Pow(x, (int) ShieldLevel);
        }
    }

    private float GetShieldRotationFactor(ShieldLevel level, Vector3 velocity, Vector3 lookVector) {
        if (ShieldLevel == ShieldLevel.Boosted) {
            
            return 2.5f * (1-Vector3.Dot(velocity.normalized, lookVector.normalized));
        }
        else return 0;
    }

    private Vector3 GetMoveVelocity(Vector3 currentVelocity, Vector3 biasDirection) {
        if (boost&&charges>0&&chargeCooldown<=0) {
            return Boost(currentVelocity);
        } else if (boostTimer-->0) {
            currentVelocity = currentVelocity.normalized * (currentVelocity.magnitude+boostDecay);
            return currentVelocity;
        }  else if (running) {
            float rotationalSpeed = runRotationalSpeed;
            if (ShieldLevel==ShieldLevel.Boosted) {
                Vector3 lookDirection = GetLookDirection();
                biasDirection = -GetLookDirection();
                rotationalSpeed += GetShieldRotationFactor(ShieldLevel, currentVelocity, GetLookDirection());
            }

            return (biasDirection==Vector3.zero)
                ? currentVelocity
                : Vector3.ClampMagnitude(
                    Vector3.RotateTowards(
                        currentVelocity, // current velocity
                        biasDirection, // update direction if it was supplied
                        rotationalSpeed*Time.deltaTime, // rotate at speed according to whether we're running TODO tune rotation scaling
                        0),
                    Mathf.Max(WalkSpeedMax, currentVelocity.magnitude));
        } else { // not running
            float shieldAccelerationFactor = GetShieldAccelerationFactor(ShieldLevel, currentVelocity, GetLookDirection());
            float accerleration = Mathf.Max(WalkAcceleration-(3/(1+shieldAccelerationFactor))*currentVelocity.magnitude, 20f);
            float dSpeed = Mathf.Clamp(
                WalkSpeedMax - Vector3.Dot(currentVelocity, biasDirection),
                0, accerleration*Time.deltaTime);

            Vector3 newVelocity = Vector3.ClampMagnitude(
                currentVelocity + dSpeed*((biasDirection == Vector3.zero)?(-currentVelocity.normalized):biasDirection),
                Mathf.Max(WalkSpeedMax, currentVelocity.magnitude)
            );

            return (Vector3.Dot(currentVelocity, newVelocity)<0) ? Vector3.zero : newVelocity;
        }
    }

    private bool IsAboveWalkSpeed() {
        return (MoveVelocity.magnitude>(WalkSpeedMax+.25));
    }

    private Vector3 GetDecayedVector(Vector3 vector, float decayRate) {
        return vector.normalized*Mathf.Max(vector.magnitude-decayRate*Time.deltaTime, 0);
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        GameObject collisionObject = hit.collider.gameObject;

        if (collisionObject.layer==LayerMask.NameToLayer("Default")) {
            if (running) { // if you hit a 
                if (IsAboveWalkSpeed()) {
                    Vector3 mirror = new Vector3(hit.normal.x, 0, hit.normal.z);
                    Vector3 bounceDirection = (MoveVelocity-2*Vector3.Project(MoveVelocity, mirror)).normalized;
                    MoveVelocity = bounceDirection*MoveVelocity.magnitude;
                    KnockBackVelocity = bounceDirection*KnockBackVelocity.magnitude; // TODO is this right?
                } else {
                    MoveVelocity.Set(0f, 0f, 0f);
                    KnockBackVelocity.Set(0f, 0f, 0f); // TODO how to handle knockback?
                }
                HitLagTimer = 2;
                // TODO add something here:
                // - stop for some frames
            } else {
                MoveVelocity=Vector3.zero;
                KnockBackVelocity=Vector3.zero;
            }
        } else if (collisionObject.layer==LayerMask.NameToLayer("Characters")&&IsAboveWalkSpeed()) {

            Character otherCharacter = collisionObject.GetComponent<Character>();

            if (otherCharacter.MoveVelocity.magnitude<MoveVelocity.magnitude) {
                Vector3 dvNormal = Vector3.Project(MoveVelocity-otherCharacter.MoveVelocity, hit.normal)*(-boostDecay)*Time.deltaTime*5; // TODO I haven't tested this on moving targets yet, so I haven't tested the second term
                dvNormal.y = 0f;
                MoveVelocity-=dvNormal;
                otherCharacter.MoveVelocity=MoveVelocity+dvNormal;
            }
        }
    }

    public Vector3 GetCursorWorldPosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (aimPlane.Raycast(ray, out float distance)) {
            /*
             * NOTE: the cast is hitting the floor and the bullet is floating over that point
             * Maybe what I should do is add an invisible plane in the gamespace? have the raycast hit that
             */
            return ray.GetPoint(distance);
        } else {
            return new Vector3(); // TODO maybe return null? I don't know how to handle this in C# yet though
        }
    }

    public void TakeDamage(
        int damage,
        Vector3 knockbackVector,
        int hitStunDuration,
        int damageTier = 1
        ) {
        // TODO finish implementation:
        // - evaluate knockback diff
        // - evaluate damage according to whether I'm shielding and such
        // - evaluate changes according to hit level
        if (invincible) return;

        if (IsAboveWalkSpeed()) // TODO should this be evaluated based on `isRunning` or the movement speed? What if I'm shielding?
            damageTier++;

        if (knockbackVector!=Vector3.zero) {
            HitLagTimer=hitStunDuration; // TODO maybe I should only reapply it if hitStunDuration>0f
            KnockBackVelocity=knockbackVector;
        }

        HP = (HP-damage<HP)?(HP-damage):Mathf.Min(HP-damage, healMax);
        healMax = Mathf.Min(healMax, HP+healMaxOffset);
        MoveVelocity = Vector3.zero;

        if (HP<=0) {
            Destroy(gameObject);
        }
    }

    void OnGUI() {
        // TODO remove: here for debugging
        if (!me) return;
        GUI.Label(new Rect(20, 40, 80, 20), MoveVelocity.magnitude+"m/s");
        GUI.Label(new Rect(20, 70, 80, 20), charges+"/"+maxCharges);
        GUI.Label(new Rect(20, 100, 80, 20), "HP: "+HP);
        GUI.Label(new Rect(20, 130, 80, 20), "Energy: "+energy);
    }
}
