using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterControls;

public struct Ability {
    public Ability(int _cooldown) {
        pressed = false;
        cooldown = _cooldown;
        timer = 0;
    }

    public bool pressed;
    public int cooldown;
    public int timer;
}

public class CharacterBehavior : MonoBehaviour, ICharacterActions
{
    // is this me? TODO better way to do this
    [SerializeField] bool me;

    /* Movement */
    private CharacterController cc;
    public Vector3 moveVelocity = new Vector3();
    private Vector3 collisionVelocity = new Vector3();
    private Vector3 bounceDirection = new Vector3();
    
    // knockback
    private Vector3 knockBackVelocity = new Vector3();
    private float knockBackDecceleration = 25f;
    private float hitLagTimer = 0f;

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

    /* Abilities */
    // TODO [SerializeField]
    private Ability[] abilitiesOld = {
        new Ability(60),
        new Ability(30)
    };
    // TODO get these constants from the other file
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Cast[] casts;

    /* Defense */
    private float hp = 100f;

    /* Visuals */
    // Bolt Visualization
    private Material _material;
    TrailRenderer tr;

    /* Controls */
    private CharacterControls characterControls;
    private bool boost = false;
    private bool running = false;
    private bool shielding = false;

    /* Resources */
    private int charges;
    private int maxCharges = 3;
    private float chargeCooldown = 0f;
    private float chargeCooldownMax = 1f;
    private float rechargeTimer = 0f;
    private float rechargeRate = 3f;


    private Vector3 movementDirection = new Vector3();
    public InputAction shield;
    public InputAction boostedShield;
    public InputAction[] attacks = new InputAction[3];
    public InputAction[] boostedAttacks = new InputAction[3];
    public InputAction[] abilities = new InputAction[4];
    public InputAction[] specialAbilities = new InputAction[2];
    public InputAction ultimate;


    

    /*
     * CONTROLS
     */
    public void OnMovement(InputAction.CallbackContext context){
        var direction = context.ReadValue<Vector2>();
        movementDirection.Set(direction.x, 0, direction.y);
    }
    public void OnRunning(InputAction.CallbackContext context) {
        running = context.ReadValueAsButton();
    }
    public void OnBoost(InputAction.CallbackContext context){
        Debug.Log("toggling boost");
        boost = context.ReadValueAsButton();
    }
    public void OnAttack1(InputAction.CallbackContext context){}
    public void OnAttack2(InputAction.CallbackContext context){}
    public void OnAttack3(InputAction.CallbackContext context){}
    public void OnBoostedAttack1(InputAction.CallbackContext context){}
    public void OnBoostedAttack2(InputAction.CallbackContext context){}
    public void OnBoostedAttack3(InputAction.CallbackContext context){}
    public void OnShield(InputAction.CallbackContext context){
        shielding = context.ReadValueAsButton();
        Debug.Log("shielding: "+ shielding);
        // TODO fix this - regardless of whether the modifier is pushed, OnBoostedShield is overriding this
    }
    public void OnBoostedShield(InputAction.CallbackContext context){
        var boostedShielding = context.ReadValueAsButton();
        Debug.Log("boostedShielding: "+boostedShielding);
    }
    public void OnThrow1(InputAction.CallbackContext context){}
    public void OnThrow2(InputAction.CallbackContext context){}
    public void OnBoostedThrow(InputAction.CallbackContext context){}
    public void OnAbility1(InputAction.CallbackContext context){}
    public void OnAbility2(InputAction.CallbackContext context){}
    public void OnAbility3(InputAction.CallbackContext context){}
    public void OnAbility4(InputAction.CallbackContext context){}
    public void OnSpecial1(InputAction.CallbackContext context){}
    public void OnSpecial2(InputAction.CallbackContext context){}
    public void OnUltimate(InputAction.CallbackContext context){}

    void HandleControls()
    {
        if (me) {
            // movement
            

            // aiming
            Vector3 worldPosition = getCursorWorldPosition();
            var playerPosition = cc.transform.position;
            var direction = new Vector3(worldPosition.x - playerPosition.x, 0, worldPosition.z - playerPosition.z).normalized;
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);

            // abilities
            abilitiesOld[0].pressed = Input.GetMouseButton(0);
            abilitiesOld[1].pressed = Input.GetMouseButton(1);
        }
    }

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        _material = GetComponent<Renderer>().material;
        tr = GetComponent<TrailRenderer>();

        if (me) // set up controls
        {
            characterControls = new CharacterControls();
            characterControls.Enable(); // TODO do I need to disable this somewhere?
            characterControls.character.SetCallbacks(this);
        }
    }

    private void Start()
    {
        charges = maxCharges;
    }

    private void HandleVisuals()
    {
        /*
         * Bolt Indicator
         */
        // duration
        if (moveVelocity.magnitude>runSpeed) // TODO maybe I need to store this in an actual bool?
            _material.color = Color.red;
        if (chargeCooldown < 0 && charges>0)
            _material.color = Color.green;
        else
            _material.color = Color.gray;

        /*
         * Trail Renderer
         */
        tr.emitting = (moveVelocity.magnitude > boltMaxSpeed / 2);
    }

    private void Update()
    {
        // handle inputs
        HandleControls();
        HandleVisuals();
    }

    private void HandleCharges()
    {
        rechargeTimer = (charges >= maxCharges) ? rechargeRate: rechargeTimer-Time.deltaTime;
        if (rechargeTimer <= 0f) {
            charges += 1;
            rechargeTimer = rechargeRate;
        }

        chargeCooldown -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        HandleCharges();
        HandleTransform();
        HandleAbilities();
    }

    private void HandleTransform()
    {
        bool aboveWalkSpeed = isAboveWalkSpeed();
        float acceleration = aboveWalkSpeed ? 0 : walkAcceleration;
        float deceleration = aboveWalkSpeed ? runDeceleration : walkAcceleration * 4;

        if (hitLagTimer > 0f)
        {
            hitLagTimer -= Time.deltaTime;
            return;
        }

        /*
         * MOVEMENT SPEED
         */
        if (boost && charges > 0 && chargeCooldown <= 0)
        { // bolting - update speed and direction
            charges -= 1;
            chargeCooldown = chargeCooldownMax;
            runSpeed = Mathf.Min(Mathf.Max(moveVelocity.magnitude, walkSpeedMax) + boltSpeedBump, boltMaxSpeed);
            boost = false;
            Vector3 v = (getCursorWorldPosition() - cc.transform.position);
            moveVelocity = new Vector3(v.x, 0, v.z).normalized * boltMaxSpeed;
            knockBackVelocity.Set(0f, 0f, 0f);
        }
        else
        { // not bolting
            // handle direction
            Vector3 biasDirection = movementDirection.normalized; // TODO check

            // handle speed
            if (aboveWalkSpeed)
            {
                if (moveVelocity.magnitude > runSpeed)
                {
                    moveVelocity = moveVelocity.normalized * Mathf.Max(runSpeed, moveVelocity.magnitude - walkAcceleration*Time.deltaTime);
                }
                else
                { // regular run
                    moveVelocity = Vector3.RotateTowards(
                        Mathf.Max(moveVelocity.magnitude - (running ? 0 : (deceleration * Time.deltaTime)), 0) * moveVelocity.normalized, // update velocity
                        biasDirection != Vector3.zero ? biasDirection : moveVelocity, // update direction if it was supplied
                        runRotationalSpeed * Time.deltaTime / (running ? 2 : 1), // rotate at speed according to whether we're running TODO tune rotation scaling
                        0 // don't change specified speed
                    );
                }
            }
            else
            {
                if (biasDirection == Vector3.zero && !running)
                {    // stop walking
                    moveVelocity += moveVelocity.normalized * Mathf.Min(Time.deltaTime * runDeceleration, -moveVelocity.magnitude);
                }
                else
                {
                    moveVelocity += (biasDirection * acceleration * Time.deltaTime);
                    if (moveVelocity.magnitude > walkSpeedMax)
                    {
                        moveVelocity = moveVelocity.normalized * walkSpeedMax;
                    }
                }
            }
        }

        /* 
         * BOUNCE
         */
        if (bounceDirection != Vector3.zero)
        {
            if (!aboveWalkSpeed)
            {
                moveVelocity.Set(0f, 0f, 0f);
                knockBackVelocity.Set(0f, 0f, 0f); // TODO how to handle knockback?
            }
            else
            {
                moveVelocity = bounceDirection * moveVelocity.magnitude;
                knockBackVelocity = bounceDirection * knockBackVelocity.magnitude;
            }

            bounceDirection.Set(0f, 0f, 0f);
        }

        /* 
         * Knockback
         */
        Vector3 tansformVelocity = moveVelocity + knockBackVelocity;
        knockBackVelocity = knockBackVelocity.normalized * Mathf.Max(knockBackVelocity.magnitude - knockBackDecceleration * Time.deltaTime, 0);

        /*
         * FINALIZE
         */
        cc.Move(tansformVelocity * Time.deltaTime);
    }

    private bool isAboveWalkSpeed()
    {
        return (moveVelocity.magnitude > (walkSpeedMax + .05));
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject collisionObject = hit.collider.gameObject;
        
        if (collisionObject.layer == LayerMask.NameToLayer("Terrain")) {
            if (running) { // if you hit a 
                Vector3 mirror = new Vector3(hit.normal.x, 0, hit.normal.z);
                bounceDirection = (moveVelocity - 2 * Vector3.Project(moveVelocity, mirror)).normalized;
                hitLagTimer = .03f;
                // TODO add something here:
                // - stop for some frames
            }
            else
            {
                moveVelocity = Vector3.zero;
                knockBackVelocity = Vector3.zero;
            }
        } else if (collisionObject.layer == LayerMask.NameToLayer("Characters") && isAboveWalkSpeed()) {
            CharacterBehavior otherCharacter = collisionObject.GetComponent<CharacterBehavior>();

            if (otherCharacter.moveVelocity.magnitude < moveVelocity.magnitude){
                Vector3 dvNormal = Vector3.Project(moveVelocity-otherCharacter.moveVelocity, hit.normal)*boltDeceleration*Time.deltaTime; // TODO I haven't tested this on moving targets yet, so I haven't tested the second term
                moveVelocity -= dvNormal;
                otherCharacter.moveVelocity = moveVelocity+dvNormal;
            }
        }
    }

    private void HandleAbilities()
    {
        if (abilitiesOld[0].pressed) {
            // TODO generalize
            abilitiesOld[0].pressed = false;

            if (abilitiesOld[0].timer <= 0)
            {
                abilitiesOld[0].timer = abilitiesOld[0].cooldown;
                // TODO generalize
                float shotRadius = cc.GetComponent<CharacterController>().radius * 1.5f;
                Vector3 position = transform.position + transform.rotation * Vector3.forward * shotRadius;
                Vector3 direction = transform.rotation * Vector3.forward;

                var projectile = Instantiate(projectilePrefab, position, transform.rotation);
                projectile.Fire(direction);
            }
        } else if (abilitiesOld[1].pressed)
        {
            // TODO generalize
            abilitiesOld[1].pressed = false;

            if (abilitiesOld[1].timer <= 0)
            {
                abilitiesOld[1].timer = abilitiesOld[1].cooldown;
                // position
                Cast cast = Instantiate(casts[0]);
                cast.Initialize(gameObject.transform);
            }
        }

        for (int i = 0; i < abilitiesOld.Length; i++)
            abilitiesOld[i].timer--;
    }

    private static Vector3 getCursorWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            /*
             * NOTE: the cast is hitting the floor and the bullet is floating over that point
             * Maybe what I should do is add an invisible plane in the gamespace? have the raycast hit that
             */
            return hitData.point;
        } else
        {
            return new Vector3(); // TODO maybe return null? I don't know how to handle this in C# yet though
        }
    }

    public void TakeDamage(
        float damage,
        Vector3 knockbackVector,
        float hitStunDuration
        )
    {
        hp -= damage;
        if (hp < 0f) {
            Destroy(gameObject);
        } else {
            if (knockbackVector != Vector3.zero && hitLagTimer <= 0)
            {
                hitLagTimer = hitStunDuration; // TODO maybe I should only reapply it if hitStunDuration>0f
                knockBackVelocity = knockbackVector;
                moveVelocity *= .5f;
            }
        }   
    }

    void OnGUI(){
        // TODO remove: here for debugging
        if (!me) return;
        GUI.Label(new Rect(20,40,80,20), moveVelocity.magnitude + "m/s");
        GUI.Label(new Rect(20, 70, 80, 20), charges+"/"+maxCharges);
        GUI.Label(new Rect(20, 100, 80, 20), "HP: "+hp);
    }
}
