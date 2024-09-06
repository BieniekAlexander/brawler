using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterControls;

[Serializable]
public struct Ability {
    public Cast cast;
    public bool pressed;
    public int cooldown;
    public int timer;
}

public class CharacterBehavior : MonoBehaviour, ICharacterActions {
    // is this me? TODO better way to do this
    [SerializeField] bool me;

    /* Movement */
    private CharacterController cc;
    private bool rotatingClockwise = false;
    public Vector3 moveVelocity = new();
    private Vector3 bounceDirection = new();

    // knockback
    private Vector3 knockBackVelocity = new();
    private float knockBackDecceleration = 25f;
    private float hitLagTimer = 0;

    // Walking
    private float walkAcceleration = 50f;
    private float walkSpeedMax = 10f;

    // Running
    private float runSpeed = 0f;
    private float runDeceleration = 10f;
    private float runRotationalSpeed = 5f;

    // Bolt
    private bool bolting = false;
    private float boltMaxSpeed = 30f;
    private float boltSpeedBump = 2.5f;
    private float boltDeceleration = 20f;

    /* Visuals */
    // Bolt Visualization
    private Material _material;
    TrailRenderer tr;

    /* Controls */
    private CharacterControls characterControls;
    private Vector3 movementDirection = new Vector3();
    private bool boost = false;
    private bool running = false;
    private bool shielding = false;
    private bool boostedShielding = false;

    /* Abilities */
    [SerializeField] private Ability[] attacks = new Ability[3];
    [SerializeField] private Ability[] boostedAttacks = new Ability[3];
    [SerializeField] private Ability[] throws = new Ability[2];
    [SerializeField] private Ability boostedThrow;
    [SerializeField] private Ability[] abilities = new Ability[4];
    [SerializeField] private Ability[] specials = new Ability[2];
    [SerializeField] private Ability ultimate;
    private Ability nullAbility = new();

    /* Resources */
    private int hp = 1000;
    private int maxCharges = 3;
    private int charges = 3;
    private int chargeCooldownMax = 60;
    private int chargeCooldown = 0;
    private int rechargeRate = 180;
    private int rechargeTimer = 0;
    private int shieldDuration = -1;
    private int parryWindow = 30;

    /* Temp */
    [SerializeField] private Projectile missilePrefab;
    [SerializeField] private Projectile bulletPrefab;
    private Projectile p = null;
    private Transform pTarget;

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
        attacks[0].pressed=context.ReadValueAsButton();
    }
    public void OnAttack2(InputAction.CallbackContext context) {
        attacks[1].pressed=context.ReadValueAsButton();
    }
    public void OnAttack3(InputAction.CallbackContext context) {
        attacks[2].pressed=context.ReadValueAsButton();
    }
    public void OnBoostedAttack1(InputAction.CallbackContext context) {
        boostedAttacks[0].pressed=context.ReadValueAsButton();
    }
    public void OnBoostedAttack2(InputAction.CallbackContext context) {
        boostedAttacks[1].pressed=context.ReadValueAsButton();
    }
    public void OnBoostedAttack3(InputAction.CallbackContext context) {
        boostedAttacks[2].pressed=context.ReadValueAsButton();
    }

    // shields
    public void OnShield(InputAction.CallbackContext context) {
        shielding=context.ReadValueAsButton();
    }
    public void OnBoostedShield(InputAction.CallbackContext context) {
        var boostedShielding = context.ReadValueAsButton();
    }

    // throws
    public void OnThrow1(InputAction.CallbackContext context) {
        throws[0].pressed=context.ReadValueAsButton();
    }
    public void OnThrow2(InputAction.CallbackContext context) {
        throws[1].pressed=context.ReadValueAsButton();
    }
    public void OnBoostedThrow(InputAction.CallbackContext context) {
        boostedThrow.pressed=context.ReadValueAsButton();
    }

    // abilities
    public void OnAbility1(InputAction.CallbackContext context) {
        abilities[0].pressed=context.ReadValueAsButton();
    }
    public void OnAbility2(InputAction.CallbackContext context) {
        abilities[1].pressed=context.ReadValueAsButton();
    }
    public void OnAbility3(InputAction.CallbackContext context) {
        abilities[2].pressed=context.ReadValueAsButton();
    }
    public void OnAbility4(InputAction.CallbackContext context) {
        abilities[3].pressed=context.ReadValueAsButton();
    }
    public void OnSpecial1(InputAction.CallbackContext context) {
        specials[0].pressed=context.ReadValueAsButton();
    }
    public void OnSpecial2(InputAction.CallbackContext context) {
        specials[1].pressed=context.ReadValueAsButton();
    }
    public void OnUltimate(InputAction.CallbackContext context) {
        ultimate.pressed=context.ReadValueAsButton();
    }

    void HandleControls() {
        if (me) {
            // aiming TODO move to input manager
            Vector3 cursorWorldPosition = getCursorWorldPosition();
            var playerPosition = cc.transform.position;
            var direction = new Vector3(cursorWorldPosition.x-playerPosition.x, 0, cursorWorldPosition.z-playerPosition.z).normalized;
            Quaternion newRotation = Quaternion.FromToRotation(Vector3.forward, direction);
            float yRotationDiff = newRotation.y - transform.rotation.y;
            
            if (yRotationDiff != 0) {
                rotatingClockwise = (yRotationDiff>0);
                // TODO somewhere in Q3, the yRotation goes from positive to negative, and I don't know why,
                // but it's producing an issue where for about 10% of the circle, rotation direction is incorrect
                // Debug.Log("old"+transform.rotation.y+" new "+newRotation.y);
                // Debug.Log("yRotationDiff "+yRotationDiff +" RotatingClockwise "+rotatingClockwise);
            }
            
            transform.rotation = newRotation;       
        }
    }

    /// <summary>
    /// Returns a reference to the ability that the user is pressing
    /// I can't find a prettier way to do this in C# :(
    /// </summary>
    /// <returns></returns>
    private ref Ability getActivatedAbility() {
        // TODO edge-cases:
        // - what if the user is pressing multiple abilities?
        // - what if the selected ability is on cooldown, or the user doesn't have the resources for it?
        //   - keep in mind that some abilities will need to be recasted for effect, even if the main ability is still on cooldown
        
        for (int i = 0; i < boostedAttacks.Length; i++) {
            if (boostedAttacks[i].pressed) return ref boostedAttacks[i];
        }
        for (int i = 0; i < throws.Length; i++) {
            if (throws[i].pressed) return ref throws[i];
        }
        if (boostedThrow.pressed) return ref boostedThrow;
        for (int i = 0; i < abilities.Length; i++) {
            if (abilities[i].pressed) return ref abilities[i];
        }
        for (int i = 0; i < specials.Length; i++) {
            if (specials[i].pressed) return ref specials[i];
        }
        if (ultimate.pressed) return ref ultimate;

        return ref nullAbility;
    }

    void HandleAbilities() {
        Ability activatedAbility = getActivatedAbility();
        ref Ability refNullAbility = ref nullAbility;

        for (int i = 0; i < attacks.Length; i++) {
            if (attacks[i].pressed && attacks[i].timer<=0) {
                attacks[i].timer = attacks[i].cooldown;
                Instantiate(attacks[i].cast).Initialize(this, rotatingClockwise);
                return;
            }
        }

        for (int i = 0; i < boostedAttacks.Length; i++) {
            if (boostedAttacks[i].pressed && boostedAttacks[i].timer<=0 && charges>0) {
                boostedAttacks[i].timer = attacks[i].cooldown;
                Instantiate(boostedAttacks[i].cast).Initialize(this, rotatingClockwise);
                charges--;
                return;
            }
        }

        // activate abilities, if applicable
        if (attacks[0].pressed&&attacks[0].timer<=0) { // Basic 1
            
        } else if (attacks[1].pressed&&attacks[1].timer<=0) { // Basic 2
            // TODO generalize
            float shotRadius = cc.GetComponent<CharacterController>().radius*1.5f;
            Vector3 position = transform.position+transform.rotation*Vector3.forward*shotRadius;
            Vector3 direction = transform.rotation*Vector3.forward;
            Instantiate(bulletPrefab, position, transform.rotation).Initialize(this, null);
            attacks[1].timer = attacks[1].cooldown;
        } else if (attacks[2].pressed) { // Basic 3
            Debug.Log("pressed attack 3");
        } else if (abilities[0].pressed) {
            // NOTE - if CD is ready but a rocket is still up, it'll just redirect rocket
            if (p==null) {
                if (abilities[0].timer<=0) {
                    float shotRadius = cc.GetComponent<CharacterController>().radius*1.5f;
                    Vector3 position = transform.position+transform.rotation*Vector3.forward*shotRadius;
                    Transform target = new GameObject("Rocket Target").transform;
                    target.position=getCursorWorldPosition();

                    p=Instantiate(missilePrefab, position, transform.rotation);
                    p.Initialize(this, target);
                    abilities[0].timer=abilities[0].cooldown;
                }
            } else {
                p.UpdateTarget(getCursorWorldPosition());
            }
        }


        // decrease cooldown on abilities
        for (int i = 0; i<abilities.Length; i++)
            abilities[i].timer--;
        for (int i = 0; i<attacks.Length; i++)
            attacks[i].timer--;
        for (int i = 0; i<boostedAttacks.Length; i++)
            boostedAttacks[i].timer--;
    }

    private void Awake() {
        cc=GetComponent<CharacterController>();
        _material=GetComponent<Renderer>().material;
        tr=GetComponent<TrailRenderer>();

        if (me) // set up controls
        {
            characterControls=new CharacterControls();
            characterControls.Enable(); // TODO do I need to disable this somewhere?
            characterControls.character.SetCallbacks(this);
        }
    }

    private void HandleVisuals() {
        /*
         * Bolt Indicator
         */
        // duration
        if (shielding)
            _material.color=Color.blue;
        else if (chargeCooldown<0&&charges>0)
            _material.color=Color.green;
        else
            _material.color=Color.gray;

        /*
         * Trail Renderer
         */
        tr.emitting=(moveVelocity.magnitude>boltMaxSpeed/2);
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
        HandleCharges();
        HandleShield();
        HandleTransform();
        HandleAbilities();
    }

    private void HandleShield() {
        if (shielding) {
            shieldDuration++;
        } else {
            shieldDuration=-1;
        }
    }

    private void HandleTransform() {
        bool aboveWalkSpeed = isAboveWalkSpeed();
        float acceleration = aboveWalkSpeed ? 0 : walkAcceleration;
        float deceleration = (aboveWalkSpeed ? runDeceleration : walkAcceleration*4)*(shielding ? 2 : 1);
        running&=!shielding;

        if (hitLagTimer>0) {
            hitLagTimer--;
            return;
        }

        /*
         * MOVEMENT SPEED
         */
        if (boost&&charges>0&&chargeCooldown<=0) { // bolting - update speed and direction
            charges--;
            chargeCooldown=chargeCooldownMax;
            runSpeed=Mathf.Min(Mathf.Max(moveVelocity.magnitude, walkSpeedMax)+boltSpeedBump, boltMaxSpeed);
            Vector3 v = (getCursorWorldPosition()-cc.transform.position);
            moveVelocity=new Vector3(v.x, 0, v.z).normalized*boltMaxSpeed;
            knockBackVelocity.Set(0f, 0f, 0f);
        } else { // not bolting
            // handle direction
            Vector3 biasDirection = shielding ? Vector3.zero : movementDirection.normalized;

            // handle speed
            if (aboveWalkSpeed) {
                if (moveVelocity.magnitude>runSpeed) {
                    moveVelocity=moveVelocity.normalized*Mathf.Max(runSpeed, moveVelocity.magnitude-walkAcceleration*Time.deltaTime);
                } else { // regular run
                    moveVelocity=Vector3.RotateTowards(
                        Mathf.Max(moveVelocity.magnitude-(running ? 0 : (deceleration*Time.deltaTime)), 0)*moveVelocity.normalized, // update velocity
                        biasDirection!=Vector3.zero ? biasDirection : moveVelocity, // update direction if it was supplied
                        runRotationalSpeed*Time.deltaTime/(running ? 2 : 1), // rotate at speed according to whether we're running TODO tune rotation scaling
                        0 // don't change specified speed
                    );
                }
            } else {
                if (biasDirection==Vector3.zero&&!running) {    // stop walking
                    moveVelocity+=moveVelocity.normalized*Mathf.Min(Time.deltaTime*runDeceleration, -moveVelocity.magnitude);
                } else {
                    moveVelocity+=(biasDirection*acceleration*Time.deltaTime);
                    if (moveVelocity.magnitude>walkSpeedMax) {
                        moveVelocity=moveVelocity.normalized*walkSpeedMax;
                    }
                }
            }
        }

        /* 
         * BOUNCE
         */
        if (bounceDirection!=Vector3.zero) {
            if (!aboveWalkSpeed) {
                moveVelocity.Set(0f, 0f, 0f);
                knockBackVelocity.Set(0f, 0f, 0f); // TODO how to handle knockback?
            } else {
                moveVelocity=bounceDirection*moveVelocity.magnitude;
                knockBackVelocity=bounceDirection*knockBackVelocity.magnitude;
            }

            bounceDirection.Set(0f, 0f, 0f);
        }

        /* 
         * Knockback
         */
        Vector3 tansformVelocity = moveVelocity+knockBackVelocity;
        knockBackVelocity=knockBackVelocity.normalized*Mathf.Max(knockBackVelocity.magnitude-knockBackDecceleration*Time.deltaTime, 0);

        /*
         * FINALIZE
         */
        cc.Move(tansformVelocity*Time.deltaTime);
    }

    private bool isAboveWalkSpeed() {
        return (moveVelocity.magnitude>(walkSpeedMax+.05));
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        GameObject collisionObject = hit.collider.gameObject;

        if (collisionObject.layer==LayerMask.NameToLayer("Terrain")) {
            if (running) { // if you hit a 
                Vector3 mirror = new Vector3(hit.normal.x, 0, hit.normal.z);
                bounceDirection=(moveVelocity-2*Vector3.Project(moveVelocity, mirror)).normalized;
                hitLagTimer=.03f;
                // TODO add something here:
                // - stop for some frames
            } else {
                moveVelocity=Vector3.zero;
                knockBackVelocity=Vector3.zero;
            }
        } else if (collisionObject.layer==LayerMask.NameToLayer("Characters")&&isAboveWalkSpeed()) {
            CharacterBehavior otherCharacter = collisionObject.GetComponent<CharacterBehavior>();

            if (otherCharacter.moveVelocity.magnitude<moveVelocity.magnitude) {
                Vector3 dvNormal = Vector3.Project(moveVelocity-otherCharacter.moveVelocity, hit.normal)*boltDeceleration*Time.deltaTime; // TODO I haven't tested this on moving targets yet, so I haven't tested the second term
                moveVelocity-=dvNormal;
                otherCharacter.moveVelocity=moveVelocity+dvNormal;
            }
        }
    }

    public Vector3 getCursorWorldPosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000)) {
            /*
             * NOTE: the cast is hitting the floor and the bullet is floating over that point
             * Maybe what I should do is add an invisible plane in the gamespace? have the raycast hit that
             */
            return hitData.point;
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

        if (isAboveWalkSpeed()) // TODO should this be evaluated based on `isRunning` or the movement speed? What if I'm shielding?
            damageTier++;

        if (hp<=0) {
            Destroy(gameObject);
        } else {
            if (shielding&&damageTier<=2) {
                damage=0;
            } else { // not shielding
                if (knockbackVector!=Vector3.zero) {
                    hitLagTimer=hitStunDuration; // TODO maybe I should only reapply it if hitStunDuration>0f
                    knockBackVelocity=knockbackVector;
                }
            }
        }

        if (damageTier>=2) {
            moveVelocity*=.5f;
        }


        hp-=damage;
    }

    void OnGUI() {
        // TODO remove: here for debugging
        if (!me) return;
        GUI.Label(new Rect(20, 40, 80, 20), moveVelocity.magnitude+"m/s");
        GUI.Label(new Rect(20, 70, 80, 20), charges+"/"+maxCharges);
        GUI.Label(new Rect(20, 100, 80, 20), "HP: "+hp);
    }
}
