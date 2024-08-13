using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class PlayerBehavior : MonoBehaviour
{
    /* Movement */
    // Regular Movement
    private CharacterController cc;
    private Vector2 direction = new Vector2(0, 0);
    private float walkingAcceleration = 50f;
    private float runSpeed = 0f;
    private float runSpeedMax = 5f;
    private Vector3 bounceVelocity = new Vector3();

    // Bolt
    private float boltDuration = 0f;
    private float boltDurationMax = .25f;
    private float boltCooldown = 0f;
    private float boltCooldownMax = 5f;
    private Vector3 boltVelocity = new Vector3();
    private float boltMaxSpeed = 25f;
    private float boltSpeedBump = 1f;

    // Shooting
    // TODO get these constants from the other file
    private float reloadTimer = 0f;
    private float reloadDuration = 1f;
    [SerializeField] private Projectile projectilePrefab;

    /* Visuals */
    // Bolt Visualization
    private Material _material;

    /* Controls */
    private bool boltPressed = false;
    private bool runningHeld = false;

    void CheckControls()
    {
        boltPressed |= (boltCooldown<=0) && Input.GetKeyDown(KeyCode.LeftShift);
        runningHeld = Input.GetKey(KeyCode.Space);
    }

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        _material = GetComponent<Renderer>().material;
        var f = GetComponent<Transform>();
    }

    private void HandleBolt()
    {
        boltDuration -= Time.deltaTime;
        boltCooldown -= Time.deltaTime;
        Vector3 direction = cc.velocity.normalized;


        if (boltPressed && boltCooldown <= 0f)
        {
            runSpeed = Mathf.Max(cc.velocity.magnitude, runSpeedMax)+boltSpeedBump;
            boltDuration = boltDurationMax;
            boltPressed = false;
            boltCooldown = boltCooldownMax;

            Vector3 v = (getCursorWorldPosition() - cc.transform.position);
            direction = new Vector3(v.x, 0, v.z).normalized;
        }
        
        if (boltDuration > 0f && boltDuration-Time.deltaTime>0f)
        {
            float boltSpeed = boltMaxSpeed - ((boltDurationMax-boltDuration)/boltDurationMax) * (boltMaxSpeed-runSpeed);
            boltVelocity = boltSpeed * direction;
            if (bounceVelocity != Vector3.zero)
            {
                boltVelocity = bounceVelocity;
                bounceVelocity.Set(0f, 0f, 0f);
            }

            cc.Move(boltVelocity * Time.deltaTime);
        } 
        else if (boltDuration > 0f)
        {
            cc.Move(direction * runSpeed * Time.deltaTime);
        }
    }

    private void HandleVisuals()
    {
        // duration
        if (boltDuration > 0)
        {
            _material.color = Color.red;
        }
        else if (boltCooldown <= 0f)
        {
            _material.color = Color.green;
        }
        else
        {
            _material.color = Color.gray;
        }
    }

    private void Update()
    {
        // handle inputs
        CheckControls();
        HandleVisuals();
    }

    void FixedUpdate()
    {
        HandleBolt();
        if (boltDuration<=0) {
            HandleMove();
        }

        HandleShoot();
    }

    private void HandleMove()
    {
        Assert.AreEqual(cc.velocity.y, 0); // TODO ensure this somehow
        direction.y = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
        direction.x = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);

        if (bounceVelocity != Vector3.zero) {
            cc.Move(bounceVelocity*Time.deltaTime);
            bounceVelocity.Set(0f, 0f, 0f);
        } else if (direction.x != 0 || direction.y != 0) {
            // issuing move command
            // TODO make sure that the movement logic is different below a threshold
            float currentSpeed = cc.velocity.magnitude;
            bool atMaxSpeed = (currentSpeed < (runSpeedMax + .05));
            float acceleration = atMaxSpeed ? walkingAcceleration : walkingAcceleration / 2;
            
            Vector3 v = cc.velocity + new Vector3(direction.x, 0, direction.y).normalized * acceleration * Time.deltaTime;

            Vector3 newVelocity = 
                (runningHeld && !atMaxSpeed)
                ? ((v).normalized * cc.velocity.magnitude)
                : ((v).normalized * Mathf.Min(v.magnitude, runSpeedMax));

            cc.Move(newVelocity * Time.deltaTime);
        } else {
            if (!runningHeld) {
                Vector3 reverse = -(new Vector3(cc.velocity.x, 0, cc.velocity.z));
                Vector3 dVelocity = cc.velocity + reverse.normalized * walkingAcceleration  * Time.deltaTime;
                cc.Move(dVelocity * Time.deltaTime);
            } else
            {
                cc.Move(cc.velocity*Time.deltaTime);
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (runningHeld)
        {
            Vector3 mirror = new Vector3(hit.normal.x, 0, hit.normal.z);
            bounceVelocity = (cc.velocity - 2 * Vector3.Project(cc.velocity, mirror)).normalized * cc.velocity.magnitude;
            // TODO add something here:
            // - stop for some frames
        }
    }

    private void HandleShoot()
    {
        if (Input.GetMouseButton(0) && (reloadTimer == 0))
        {
            reloadTimer = reloadDuration;

            // get trajectory
            Vector3 worldPosition = getCursorWorldPosition();
            var playerPosition = cc.transform.position;
            var direction = new Vector3(worldPosition.x - playerPosition.x, 0, worldPosition.z - playerPosition.z).normalized;

            // position
            var shotRadius = cc.GetComponent<SphereCollider>().radius * 1.5f;
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

    void OnGUI(){
        // TODO remove: here for debugging
        GUI.Label(new Rect(20,40,80,20), cc.velocity.magnitude + "m/s");
    }
}
