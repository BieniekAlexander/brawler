using System.Collections.Concurrent;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class CharacterBehavior : MonoBehaviour
{
    // is this me? TODO better way to do this
    [SerializeField] bool me;

    /* Movement */
    private CharacterController cc;
    private Vector3 velocity = new Vector3();
    private Vector3 bounceDirection = new Vector3();
    private float hitStunTimer = 0f;

    // Walking
    private float walkAcceleration = 50f;
    private float walkSpeedMax = 5f;

    // Running
    private float runSpeed = 0f;
    private float runDeceleration = 5f;
    private float runRotationalSpeed = 5f;

    // Bolt
    private float boltDuration = 0f;
    private float boltDurationMax = .25f;
    private float boltMaxSpeed = 25f;
    private float boltSpeedBump = 2.5f;

    /* Offense */
    // Shooting
    // TODO get these constants from the other file
    private float reloadTimer = 0f;
    private float reloadDuration = 1f;
    [SerializeField] private Projectile projectilePrefab;

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

    void CheckControls()
    {
        if (me) {
            boltPressed |= (chargeCooldown <= 0) && Input.GetKeyDown(KeyCode.LeftShift);
            runningHeld = Input.GetKey(KeyCode.Space);
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
        if (boltDuration > 0f)
            _material.color = Color.red;
        if (chargeCooldown < 0 && charges>0)
            _material.color = Color.green;
        else
            _material.color = Color.gray;

        /*
         * Trail Renderer
         */
        tr.emitting = (velocity.magnitude > boltMaxSpeed / 2);
    }

    private void Update()
    {
        // handle inputs
        CheckControls();
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
        HandleMove();
        HandleShoot();
    }

    private void HandleKnockback()
    {

    }

    private void HandleMove()
    {
        bool aboveWalkSpeed = (velocity.magnitude > (walkSpeedMax + .05f));
        if (!me) return;

        /*
         * MOVEMENT SPEED
         */
        if (boltPressed && charges>0 && chargeCooldown<0)
        { // bolting - update speed and direction
            charges -= 1;
            chargeCooldown = chargeCooldownMax;
            runSpeed = Mathf.Min(Mathf.Max(velocity.magnitude, walkSpeedMax) + boltSpeedBump, boltMaxSpeed);
            boltDuration = boltDurationMax;
            boltPressed = false;
            Vector3 v = (getCursorWorldPosition() - cc.transform.position);
            velocity = new Vector3(v.x, 0, v.z).normalized*boltMaxSpeed;
        }
        else
        { // not bolting
            // handle direction
            Vector3 biasDirection = new Vector3(0, 0, 0);
            biasDirection.z = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            biasDirection.x = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);

            if (biasDirection != Vector3.zero)
                biasDirection = biasDirection.normalized;

            // handle speed
            if (aboveWalkSpeed) {
                if (runningHeld || boltDuration > 0f) { // keeping up running speed - limit rotational control
                    if (boltDuration > 0f && boltDuration - Time.deltaTime > 0f)
                        velocity = velocity.normalized * (boltMaxSpeed - ((boltDurationMax - boltDuration) / boltDurationMax) * (boltMaxSpeed - runSpeed));
                    else if (boltDuration > 0f)
                        velocity = velocity.normalized * runSpeed;

                    if (biasDirection != Vector3.zero)
                        velocity = (velocity + biasDirection * walkAcceleration / 2 * Time.deltaTime).normalized * velocity.magnitude;
                } else {  // running not held - drop speed and free up rotational control
                    velocity = velocity.normalized * Mathf.Max(velocity.magnitude - runDeceleration*Time.deltaTime, 0);
                    if (biasDirection != Vector3.zero)
                        velocity = Vector3.RotateTowards(velocity, biasDirection, runRotationalSpeed * Time.deltaTime, 1).normalized*velocity.magnitude;
                    // TODO bugged- this is plummeting velocity
                }
            } else {
                if (biasDirection == Vector3.zero)
                    biasDirection = -velocity.normalized;

                Vector3 newVelocity = velocity + (biasDirection * walkAcceleration * Time.deltaTime);

                if (newVelocity.normalized == -velocity.normalized)
                    velocity.Set(0f, 0f, 0f);
                else
                    velocity = newVelocity.normalized * Mathf.Min(newVelocity.magnitude, walkSpeedMax);
            }
        }

        /* 
         * BOUNCE
         */
        if (bounceDirection != Vector3.zero)
        {
            if (!aboveWalkSpeed)
                velocity.Set(0f, 0f, 0f);

            velocity = bounceDirection*velocity.magnitude;
            bounceDirection.Set(0f, 0f, 0f);
        }

        /*
         * FINALIZE
         */
        cc.Move(velocity*Time.deltaTime);
        boltDuration -= Time.deltaTime;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 mirror = new Vector3(hit.normal.x, 0, hit.normal.z);
        bounceDirection = (velocity - 2 * Vector3.Project(velocity, mirror)).normalized;
        // TODO add something here:
        // - stop for some frames
    }

    private void HandleShoot()
    {
        if (!me) return;

        if (Input.GetMouseButton(0) && (reloadTimer == 0))
        {
            reloadTimer = reloadDuration;

            // get trajectory
            Vector3 worldPosition = getCursorWorldPosition();
            var playerPosition = cc.transform.position;
            var direction = new Vector3(worldPosition.x - playerPosition.x, 0, worldPosition.z - playerPosition.z).normalized;

            // position
            var shotRadius = cc.GetComponent<CharacterController>().radius * 1.5f;
            var position = playerPosition + shotRadius * direction;

            var projectile = Instantiate(projectilePrefab, position, transform.rotation);
            projectile.Fire(direction);
        }

        reloadTimer = math.max(reloadTimer - Time.deltaTime, 0);
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
        Vector3 knockbackDirection,
        float knockBackMagnitude,
        float hitStunDuration
        )
    {
        hp -= damage;
        if (hp < 0f) {
            Destroy(gameObject);
        } else {
            hitStunTimer = hitStunDuration; // TODO maybe I should only reapply it if hitStunDuration>0f

        }
            
    }

    void OnGUI(){
        // TODO remove: here for debugging
        if (!me) return;
        GUI.Label(new Rect(20,40,80,20), velocity.magnitude + "m/s");
        GUI.Label(new Rect(20, 70, 80, 20), charges+"/"+maxCharges);
        GUI.Label(new Rect(20, 100, 80, 20), "HP: "+hp);
    }
}
