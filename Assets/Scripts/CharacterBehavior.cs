using UnityEngine;

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

public class CharacterBehavior : MonoBehaviour
{
    // is this me? TODO better way to do this
    [SerializeField] bool me;

    /* Movement */
    private CharacterController cc;
    public Vector3 moveVelocity = new Vector3();
    private Vector3 bounceDirection = new Vector3();
    
    // knockback
    private Vector3 knockBackVelocity = new Vector3();
    private float knockBackDecceleration = 25f;
    private float hitLagTimer = 0f;

    // Walking
    private float walkAcceleration = 100f;
    private float walkSpeedMax = 10f;

    // Running
    private float runSpeed = 0f;
    private float runDeceleration = 10f;
    private float runRotationalSpeed = 5f;

    // Bolt
    private int boltDuration = 0;
    private int boltDurationMax = 15;
    private float boltMaxSpeed = 30f;
    private float boltSpeedBump = 2.5f;

    /* Abilities */
    // TODO [SerializeField]
    private Ability[] abilities = {
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
    private int charges;
    private int maxCharges = 3;
    private float chargeCooldown = 0f;
    private float chargeCooldownMax = 1f;
    private float rechargeTimer = 0f;
    private float rechargeRate = 3f;
    private bool boltPressed = false;
    private bool runningHeld = false;
    private int moveUp      = 0;
    private int moveDown    = 0;
    private int moveLeft    = 0;
    private int moveRight   = 0;

    void HandleControls()
    {
        if (me) {
            // movement
            moveUp      = Input.GetKey(KeyCode.W) ? 1 : 0;
            moveDown    = Input.GetKey(KeyCode.S) ? 1 : 0;
            moveLeft    = Input.GetKey(KeyCode.D) ? 1 : 0;
            moveRight   = Input.GetKey(KeyCode.A) ? 1 : 0;

            boltPressed |= Input.GetKeyDown(KeyCode.LeftShift);
            runningHeld = Input.GetKey(KeyCode.Space);

            // aiming
            Vector3 worldPosition = getCursorWorldPosition();
            var playerPosition = cc.transform.position;
            var direction = new Vector3(worldPosition.x - playerPosition.x, 0, worldPosition.z - playerPosition.z).normalized;
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);

            // abilities
            abilities[0].pressed = Input.GetMouseButton(0);
            abilities[1].pressed = Input.GetMouseButton(1);
        }
    }

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        _material = GetComponent<Renderer>().material;
        tr = GetComponent<TrailRenderer>();
        var f = GetComponent<Transform>();
        charges = maxCharges;
    }

    private void HandleVisuals()
    {
        /*
         * Bolt Indicator
         */
        // duration
        if (boltDuration > 0)
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
        bool aboveWalkSpeed = (moveVelocity.magnitude > (walkSpeedMax+.05));
        float acceleration = aboveWalkSpeed ? 0 : walkAcceleration;
        float deceleration = aboveWalkSpeed ? runDeceleration : walkAcceleration * 2;

        if (hitLagTimer > 0f)
        {
            hitLagTimer -= Time.deltaTime;
            return;
        }

        /*
         * MOVEMENT SPEED
         */
        if (boltPressed && charges>0 && chargeCooldown<=0) { // bolting - update speed and direction
            charges -= 1;
            chargeCooldown = chargeCooldownMax;
            runSpeed = Mathf.Min(Mathf.Max(moveVelocity.magnitude, walkSpeedMax) + boltSpeedBump, boltMaxSpeed);
            boltDuration = boltDurationMax;
            boltPressed = false;
            Vector3 v = (getCursorWorldPosition() - cc.transform.position);
            moveVelocity = new Vector3(v.x, 0, v.z).normalized*boltMaxSpeed;
            knockBackVelocity.Set(0f, 0f, 0f);
        } else
        { // not bolting
            // handle direction
            Vector3 biasDirection = new Vector3(moveLeft - moveRight, 0, moveUp - moveDown).normalized; // TODO check

            // handle speed
            if (aboveWalkSpeed) {
                if (boltDuration >= 0) {
                    moveVelocity = moveVelocity.normalized * (boltMaxSpeed - ((boltDurationMax - boltDuration) / boltDurationMax) * (boltMaxSpeed - runSpeed));
                } else { // regular run
                    moveVelocity = Vector3.RotateTowards(
                        Mathf.Max(moveVelocity.magnitude - (runningHeld?0:(deceleration * Time.deltaTime)), 0) * moveVelocity.normalized, // update velocity
                        biasDirection != Vector3.zero ? biasDirection : moveVelocity, // update direction if it was supplied
                        runRotationalSpeed * Time.deltaTime / (runningHeld ? 2 : 1), // rotate at speed according to whether we're running TODO tune rotation scaling
                        0 // don't change specified speed
                    );
                }   
            } else {
                if (biasDirection == Vector3.zero && !runningHeld) {    // stop walking
                    moveVelocity += moveVelocity.normalized * Mathf.Min(Time.deltaTime * runDeceleration, -moveVelocity.magnitude);
                } 
                else
                {
                    moveVelocity += (biasDirection * acceleration * Time.deltaTime);
                    if (moveVelocity.magnitude > walkSpeedMax) {
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
            if (!aboveWalkSpeed) {
                moveVelocity.Set(0f, 0f, 0f);
                knockBackVelocity.Set(0f, 0f, 0f); // TODO how to handle knockback?
            } else {
                moveVelocity = bounceDirection * moveVelocity.magnitude;
                knockBackVelocity = bounceDirection * knockBackVelocity.magnitude;
            }

            bounceDirection.Set(0f, 0f, 0f);
        }

        /* 
         * Knockback
         */
        Vector3 tansformVelocity = moveVelocity + knockBackVelocity;
        knockBackVelocity = knockBackVelocity.normalized * Mathf.Max(knockBackVelocity.magnitude-knockBackDecceleration*Time.deltaTime, 0);

        /*
         * FINALIZE
         */
        cc.Move(tansformVelocity * Time.deltaTime);
        boltDuration--;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject collisionObject = hit.collider.gameObject;
        
        if (collisionObject.layer == LayerMask.NameToLayer("Terrain")) {
            if (runningHeld)
            {
                Vector3 mirror = new Vector3(hit.normal.x, 0, hit.normal.z);
                bounceDirection = (moveVelocity - 2 * Vector3.Project(moveVelocity, mirror)).normalized;
                // TODO add something here:
                // - stop for some frames
            }
            else
            {
                moveVelocity = Vector3.zero;
                knockBackVelocity = Vector3.zero;
            }
        } else if (collisionObject.layer == LayerMask.NameToLayer("Characters")) {
            CharacterBehavior otherCharacter = collisionObject.GetComponent<CharacterBehavior>();
            Vector3 dvNormal = hit.normal * runDeceleration*20 * Time.deltaTime;
            moveVelocity += dvNormal;
            otherCharacter.moveVelocity += dvNormal;
        }
        
        /* 
         * TODO change up the logic a bit if it's a character that you're running into
         * Push the characters according to the faster velocity, and decelerate the faster velocity until it equals the lower "real velocity"
         * */
    }

    private void HandleAbilities()
    {
        if (abilities[0].pressed) {
            // TODO generalize
            abilities[0].pressed = false;

            if (abilities[0].timer <= 0)
            {
                abilities[0].timer = abilities[0].cooldown;
                // TODO generalize
                float shotRadius = cc.GetComponent<CharacterController>().radius * 1.5f;
                Vector3 position = transform.position + transform.rotation * Vector3.forward * shotRadius;
                Vector3 direction = transform.rotation * Vector3.forward;

                var projectile = Instantiate(projectilePrefab, position, transform.rotation);
                projectile.Fire(direction);
            }
        } else if (abilities[1].pressed)
        {
            // TODO generalize
            abilities[1].pressed = false;

            if (abilities[1].timer <= 0)
            {
                abilities[1].timer = abilities[1].cooldown;
                // position
                Cast cast = Instantiate(casts[0]);
                cast.Initialize(gameObject.transform);
            }
        }

        for (int i = 0; i < abilities.Length; i++)
            abilities[i].timer--;
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
                boltDuration = -1;
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
