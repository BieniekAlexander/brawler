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
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    DashAttack = 3,
    BoostedAttack1 = 4,
    BoostedAttack2 = 5,
    BoostedAttack3 = 6,
    Throw1 = 7,
    Throw2 = 8,
    BoostedThrow = 9,
    Ability1 = 10,
    Ability2 = 11,
    Ability3 = 12,
    Ability4 = 13,
    Special1 = 14,
    Special2 = 15,
    Ultimate = 16
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
    private int hitLagTimer = 0;

    // Walking
    private float walkAcceleration = 50f;
    public float walkSpeedMax = 10f;
    private Vector3 gravity = new Vector3(0, -.25f, 0);
    private Vector3 fallVelocity = Vector3.zero;

    // Running
    private float runSpeed = 0f;
    private float runDeceleration = 10f;
    private float runRotationalSpeed = 5f;

    // Bolt
    private int boostTimer = 0;
    private int boostMaxDuration = 20;
    private float boostMaxSpeed = 25f;
    private float boostSpeedBump = 2.5f;
    private float boostDV;

    /* Visuals */
    // Bolt Visualization
    public Material Material { get; private set; }
    TrailRenderer tr;

    /* Controls */
    private CharacterControls characterControls;
    private Vector3 movementDirection = new();
    private bool boost = false;
    private bool running = false;
    private bool shielding = false;
    private bool boostedShielding = false;

    /* Abilities */

    [SerializeField] private CastSlot[] castSlots = Enum.GetNames(typeof(CastId)).Select(name => new CastSlot(name)).ToArray();
    public CastContainer[] castContainers = new CastContainer[Enum.GetNames(typeof(CastId)).Length];
    public static int[] boostedIds = new int[] { (int)CastId.BoostedAttack1, (int)CastId.BoostedAttack2, (int)CastId.BoostedAttack3, (int)CastId.BoostedThrow };
    public static int[] specialIds = new int[] { (int)CastId.Special1, (int)CastId.Special2 };
    private int activeCastId = -1;
    private int inputCastId = -1;

    /* Resources */
    public int HP { get; private set; } = 1000;
    public bool Reflects { get; set; } = false;
    private int healMax; private int healMaxOffset = 100;
    private int maxCharges = 3;
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
        movementDirection.Set(direction.x, 0, direction.y);
    }
    public void OnRunning(InputAction.CallbackContext context) {
        // TODO fix tihs - shielding sets running to false
        running=context.ReadValueAsButton();
    }
    public void OnBoost(InputAction.CallbackContext context) {
        boost=context.ReadValueAsButton();
    }

    // attacks
    public void OnAttack1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                inputCastId = (int)CastId.DashAttack;
            else
                inputCastId = (int)CastId.Attack1;
        }
    }
    public void OnAttack2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                inputCastId = (int)CastId.DashAttack;
            else
                inputCastId = (int)CastId.Attack2;
        }
    }
    public void OnAttack3(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                inputCastId = (int)CastId.DashAttack;
            else
                inputCastId = (int)CastId.Attack3;
        }
    }
    public void OnBoostedAttack1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                inputCastId = (int)CastId.DashAttack;
            else
                inputCastId = (int)CastId.BoostedAttack1;
        }
    }
    public void OnBoostedAttack2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                inputCastId = (int)CastId.DashAttack;
            else
                inputCastId = (int)CastId.BoostedAttack2;
        }
    }
    public void OnBoostedAttack3(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (boostTimer > 0)
                inputCastId = (int)CastId.DashAttack;
            else
                inputCastId = (int)CastId.BoostedAttack3;
        }
    }

    // shields
    public void OnShield(InputAction.CallbackContext context) {
        if (me) {
            Shield.gameObject.SetActive(context.ReadValueAsButton());
        }
    }
    public void OnBoostedShield(InputAction.CallbackContext context) {
        var boostedShielding = context.ReadValueAsButton();
    }

    // throws
    public void OnThrow1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Throw1;
    }
    public void OnThrow2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Throw2;
    }
    public void OnBoostedThrow(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.BoostedThrow;
    }

    // abilities
    public void OnAbility1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Ability1;
    }
    public void OnAbility2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Ability2;
    }
    public void OnAbility3(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Ability3;
    }
    public void OnAbility4(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Ability4;
    }
    public void OnSpecial1(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Special1;
    }
    public void OnSpecial2(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Special2;
    }
    public void OnUltimate(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton())
            inputCastId = (int)CastId.Ultimate;
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

    private int StartCast(int castId) {
        ref CastContainer castContainer = ref castContainers[castId];

        if (castContainer.cast is not null) {
            // we're updating another cast - allowed
            Vector3 cursorWorldPosition = GetCursorWorldPosition();
            castContainer.cast.UpdateCast(cursorWorldPosition);
            return activeCastId;
        } else if (activeCastId==-1) {
            // nothing is being casted, so start this new one
            if (castContainer.castPrefab == null) {
                Debug.Log("No cast supplied for cast"+(int)castId);
                return -1;
            }
            if (
                castContainer.charges>0
                && (charges>0 || !boostedIds.Contains(castId))
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
                return castId;
            } else {
                return -1;
            }
        } else {
            // there's something being actively casted - return that
            return activeCastId;

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
        int j = (int)activeCastId;
        if (j >= 0) { // if an active cast is designated
            if (castContainers[j].cast == null) {
                activeCastId = -1;
            } else if (castContainers[j].cast.Frame >= castContainers[j].cast.startupTime) { // if startup time has elapsed
                activeCastId = -1;
            }
        }
    }

    private void Awake() {
        cc=GetComponent<CharacterController>();
        Shield = GetComponentInChildren<Shield>();
        Shield.gameObject.SetActive(false);
        Material=GetComponent<Renderer>().material;
        tr=GetComponent<TrailRenderer>();

        standingY = transform.position.y + standingOffset;
        healMax = HP;

        if (me) // set up controls
        {
            characterControls=new CharacterControls();
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
        if (Reflects)
            Material.color=Color.blue;
        else if (activeCastId >= 0)
            Material.color=Color.magenta;
        else if (chargeCooldown<0&&charges>0)
            Material.color=Color.green;
        else
            Material.color=Color.gray;

        /*
         * Trail Renderer
         */
        tr.emitting=(MoveVelocity.magnitude>boostMaxSpeed/2);
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
        if (inputCastId >= 0) {
            activeCastId = StartCast(inputCastId);
            inputCastId = -1;
        }
        TickCasts();

        // Update Position
        if (CommandMovement != null) {
            cc.Move(CommandMovement.GetDPosition());
        } else {
            cc.Move(GetDPosition());
            //transform.position = new Vector3(transform.position.x, standingY, transform.position.z); // TODO hardcoding the Y position, but I should fix the y calculations
        }

        // apply statusEffects
        for (int i = 0; i < statusEffects.Count; i++) {
            statusEffects[i].Tick();
        }

        statusEffects.RemoveAll((StatusEffectBase effect) => { return (effect == null); });
    }

    // Movement evaluations
    private bool CanTurn() {
        return (boostTimer <= 0);
    }

    /// <summary>
    /// Boost
    /// </summary>
    private void Boost() {
        charges--;
        chargeCooldown = chargeCooldownMax;
        boostTimer = boostMaxDuration;
        runSpeed = Mathf.Min(Mathf.Max(MoveVelocity.magnitude, walkSpeedMax)+boostSpeedBump, boostMaxSpeed);
        boostDV = (boostMaxSpeed-runSpeed)/boostMaxDuration;
        Vector3 v = (GetCursorWorldPosition()-cc.transform.position);
        MoveVelocity = new Vector3(v.x, 0, v.z).normalized*boostMaxSpeed;
        KnockBackVelocity.Set(0f, 0f, 0f);
    }

    public void SetCommandMovement(CommandMovementBase _commandMovement) {
        CommandMovement = _commandMovement;
        MoveVelocity = CommandMovement.velocity.normalized * MoveVelocity.magnitude; // TODO maybe not the best spot to do this
    }

    private Vector3 GetDPosition() {
        if (hitLagTimer>0) {
            hitLagTimer--;
            return Vector3.zero;
        }

        if (boost&&charges>0&&chargeCooldown<=0) {
            Boost();
        }

        bool aboveWalkSpeed = IsAboveWalkSpeed();
        Vector3 changeInDirection = (activeCastId>=0) ? Vector3.zero : movementDirection;
        bool isRunning = running && !Stunned; // really ugly implementation of stun behavior updates - TODO make better
        if (Stunned) changeInDirection = Vector3.zero;
        // TODO:
        // - casting?
        // - shielding?

        if (!aboveWalkSpeed) { // walking
            if (changeInDirection==Vector3.zero && !isRunning) {
                MoveVelocity += (-MoveVelocity.normalized) * Mathf.Min(walkAcceleration*4*Time.deltaTime, MoveVelocity.magnitude);
            } else if (changeInDirection!=Vector3.zero) {
                MoveVelocity += changeInDirection.normalized * walkAcceleration*Time.deltaTime;

                if (MoveVelocity.magnitude > walkSpeedMax) {
                    MoveVelocity = MoveVelocity.normalized* walkSpeedMax;
                }
            }
        } else if (boostTimer > 0) { // boosting
            MoveVelocity -= MoveVelocity.normalized*boostDV;
            boostTimer--;
        } else { // running
            MoveVelocity=Vector3.RotateTowards(
                Mathf.Max(MoveVelocity.magnitude-(isRunning ? 0 : (runDeceleration*Time.deltaTime)), 0)*MoveVelocity.normalized, // update velocity
                changeInDirection!=Vector3.zero ? changeInDirection : MoveVelocity, // update direction if it was supplied
                runRotationalSpeed*Time.deltaTime/(isRunning ? 2 : 1), // rotate at speed according to whether we're running TODO tune rotation scaling
                0 // don't change specified speed
            );
        }

        /*
         * FALLING
         */
        RaycastHit hitInfo;

        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, cc.radius+standingOffset+.01f)) {
            standingY = hitInfo.point.y + cc.radius + standingOffset;
            fallVelocity = new Vector3(0, transform.position.y - standingY, 0);
        } else {
            fallVelocity += gravity;
        }
        
        /*
         * FINALIZE
         */
        KnockBackVelocity = KnockBackVelocity.normalized*Mathf.Max(KnockBackVelocity.magnitude-KnockBackDecceleration*Time.deltaTime, 0);
        Vector3 finalVelocity = MoveVelocity + KnockBackVelocity + fallVelocity;
        return finalVelocity * Time.deltaTime;
    }

    private bool IsAboveWalkSpeed() {
        return (MoveVelocity.magnitude>(walkSpeedMax+.25));
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
                hitLagTimer = 2;
                // TODO add something here:
                // - stop for some frames
            } else {
                MoveVelocity=Vector3.zero;
                KnockBackVelocity=Vector3.zero;
            }
        } else if (collisionObject.layer==LayerMask.NameToLayer("Characters")&&IsAboveWalkSpeed()) {

            Character otherCharacter = collisionObject.GetComponent<Character>();

            if (otherCharacter.MoveVelocity.magnitude<MoveVelocity.magnitude) {
                Vector3 dvNormal = Vector3.Project(MoveVelocity-otherCharacter.MoveVelocity, hit.normal)*boostDV*Time.deltaTime*5; // TODO I haven't tested this on moving targets yet, so I haven't tested the second term
                dvNormal.y = 0f;
                MoveVelocity-=dvNormal;
                otherCharacter.KnockBackVelocity=MoveVelocity+dvNormal;
            }
        }
    }

    public Vector3 GetCursorWorldPosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000)) {
            /*
             * NOTE: the cast is hitting the floor and the bullet is floating over that point
             * Maybe what I should do is add an invisible plane in the gamespace? have the raycast hit that
             */
            Vector3 cursorWorldPosition = hitData.point;
            cursorWorldPosition.y = transform.position.y;
            return cursorWorldPosition;
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
            hitLagTimer=hitStunDuration; // TODO maybe I should only reapply it if hitStunDuration>0f
            KnockBackVelocity=knockbackVector;
        }

        HP = (HP-damage<HP)?(HP-damage):Mathf.Min(HP-damage, healMax);
        healMax = Mathf.Min(healMax, HP+healMaxOffset);

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
