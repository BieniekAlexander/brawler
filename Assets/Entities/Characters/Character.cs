using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterControls;



[Serializable]
public class CastSlot {
    public CastSlot(string _name) {
        name = _name;
        castPrefab = null;
        cooldown = -1;
    }

    public string name;
    public CastBase castPrefab;
    public int cooldown;
}

public struct CastContainer {
    public CastContainer(CastSlot _castSlot) {
        castPrefab = _castSlot.castPrefab;
        cast = null;
        cooldown = _castSlot.cooldown;
        timer = 0;
        activated = false;
    }

    public CastBase castPrefab;
    public CastBase cast;
    public int cooldown;
    public int timer;
    public bool activated;
}

public enum CastId {
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    BoostedAttack1 = 3,
    BoostedAttack2 = 4,
    BoostedAttack3 = 5,
    Throw1 = 6,
    Throw2 = 7,
    BoostedThrow = 8,
    Ability1 = 9,
    Ability2 = 10,
    Ability3 = 11,
    Ability4 = 12,
    Special1 = 13,
    Special2 = 14,
    Ultimate = 15
}

public class Character : MonoBehaviour, ICharacterActions {
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
    
    [SerializeField] private CastSlot[] castSlots = Enum.GetNames(typeof(CastId)).Select(name => new CastSlot(name)).ToArray();
    private CastContainer[] castContainers = new CastContainer[Enum.GetNames(typeof(CastId)).Length];
    public static int[] boostedIds = new int[] { (int)CastId.BoostedAttack1, (int)CastId.BoostedAttack2, (int)CastId.BoostedAttack3, (int)CastId.BoostedThrow };
    public static int[] specialIds = new int[] { (int)CastId.Special1, (int)CastId.Special2 };

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
    private int energy = 100;
    private int maxEnergy = 100;

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
        castContainers[(int)CastId.Attack1].activated=context.ReadValueAsButton();
    }
    public void OnAttack2(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Attack2].activated=context.ReadValueAsButton();
    }
    public void OnAttack3(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Attack3].activated=context.ReadValueAsButton();
    }
    public void OnBoostedAttack1(InputAction.CallbackContext context) {
        castContainers[(int)CastId.BoostedAttack1].activated=context.ReadValueAsButton();
    }
    public void OnBoostedAttack2(InputAction.CallbackContext context) {
        castContainers[(int)CastId.BoostedAttack2].activated=context.ReadValueAsButton();
    }
    public void OnBoostedAttack3(InputAction.CallbackContext context) {
        castContainers[(int)CastId.BoostedAttack3].activated=context.ReadValueAsButton();
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
        castContainers[(int)CastId.Throw1].activated=context.ReadValueAsButton();
    }
    public void OnThrow2(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Throw2].activated=context.ReadValueAsButton();
    }
    public void OnBoostedThrow(InputAction.CallbackContext context) {
        castContainers[(int)CastId.BoostedThrow].activated=context.ReadValueAsButton();
    }

    // abilities
    public void OnAbility1(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Ability1].activated=context.ReadValueAsButton();
    }
    public void OnAbility2(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Ability2].activated=context.ReadValueAsButton();
    }
    public void OnAbility3(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Ability3].activated=context.ReadValueAsButton();
    }
    public void OnAbility4(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Ability4].activated=context.ReadValueAsButton();
    }
    public void OnSpecial1(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Special1].activated=context.ReadValueAsButton();
    }
    public void OnSpecial2(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Special2].activated=context.ReadValueAsButton();
    }
    public void OnUltimate(InputAction.CallbackContext context) {
        castContainers[(int)CastId.Ultimate].activated=context.ReadValueAsButton();
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

    void HandleAbilities() {
        for (int i = 0; i < castContainers.Length; i++) {
            if (castContainers[i].activated) {
                if (castContainers[i].castPrefab == null) {
                    Debug.Log("No cast supplied for cast"+i);
                    return;
                }

                if (castContainers[i].cast is not null) { // recast
                    castContainers[i].cast.UpdateCast(getCursorWorldPosition());
                } else if (castContainers[i].timer<=0) { // cooldown fresh
                    if (
                        (charges>0 || !boostedIds.Contains(i))
                        && (energy >= 50 || !specialIds.Contains(i))
                    ) {
                        if (boostedIds.Contains(i)) {
                            charges--;
                        } else if (specialIds.Contains(i)) {
                            energy -= 50;
                        }

                        castContainers[i].cast = Instantiate(castContainers[i].castPrefab);
                        castContainers[i].cast.Initialize(this, rotatingClockwise);
                        castContainers[i].timer = castContainers[i].cooldown;
                        return;
                    }
                }
            }
        }

        // resolve cooldowns and cast expirations
        for (int i = 0; i<castContainers.Length; i++) {
            castContainers[i].timer--;

            if (castContainers[i].cast == null) {
                castContainers[i].cast = null;
            } 
        }
            
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

        for ( int i = 0; i<castSlots.Length; i++) {
            castContainers[i] = new CastContainer(castSlots[i]);
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
            Character otherCharacter = collisionObject.GetComponent<Character>();

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
        GUI.Label(new Rect(20, 130, 80, 20), "Energy: "+energy);
    }
}