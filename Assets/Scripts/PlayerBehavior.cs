using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
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
    private Vector3 direction = new Vector3();
    private float walkingAcceleration = 50f;
    private float currentSpeed = 0f;
    private float runSpeed = 0f;
    private float runSpeedMax = 5f;
    private Vector3 bounceDirection = new Vector3();

    // Bolt
    private float boltDuration = 0f;
    private float boltDurationMax = .25f;
    private float boltCooldown = 0f;
    private float boltCooldownMax = 5f;
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

    private Vector3 GetBiasedDirection(Vector3 currentPath, Vector3 bias) { // TODO name?
        return (currentPath + bias).normalized;
    }

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
        HandleMove();
        HandleShoot();
    }

    private void HandleMove()
    {
        Assert.AreEqual(cc.velocity.y, 0); // TODO ensure this somehow

        /*
         * MOVEMENT SPEED
         */
        if (boltPressed && boltCooldown <= 0f)
        { // bolting - update speed and direction
            runSpeed = Mathf.Max(cc.velocity.magnitude, runSpeedMax) + boltSpeedBump;
            currentSpeed = boltMaxSpeed;
            boltDuration = boltDurationMax;
            boltPressed = false;
            boltCooldown = boltCooldownMax;
            Vector3 v = (getCursorWorldPosition() - cc.transform.position);
            direction = new Vector3(v.x, 0, v.z).normalized;
        }
        else
        { // not bolting
            // handle direction
            Vector3 biasDirection = new Vector3(0, 0, 0);
            biasDirection.z = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            biasDirection.x = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);

            if (biasDirection == Vector3.zero && !runningHeld)
                biasDirection = -cc.velocity.normalized;
            else
                biasDirection = biasDirection.normalized;

            // handle speed
            bool atRunSpeed = (currentSpeed > (runSpeedMax + .05f));
            
            if (atRunSpeed) {
                if (runningHeld) {
                    if (boltDuration > 0f && boltDuration - Time.deltaTime > 0f)
                        currentSpeed = boltMaxSpeed - ((boltDurationMax - boltDuration) / boltDurationMax) * (boltMaxSpeed - runSpeed);
                    else if (boltDuration > 0f)
                        currentSpeed = runSpeed;
                } else {
                    currentSpeed = Mathf.Max(currentSpeed - walkingAcceleration*Time.deltaTime, 0);
                }

                if (biasDirection != Vector3.zero)
                    direction = GetBiasedDirection(cc.velocity, biasDirection * walkingAcceleration * Time.deltaTime / 2);
            } else {
                Vector3 newVelocity = (cc.velocity + (biasDirection*walkingAcceleration * Time.deltaTime));

                if (newVelocity.normalized == -cc.velocity.normalized)
                    currentSpeed = 0;
                else
                {
                    direction = newVelocity.normalized;
                    currentSpeed = Mathf.Min(newVelocity.magnitude, runSpeedMax);
                }
            }
        }

        /* 
         * BOUNCE
         */
        if (bounceDirection != Vector3.zero)
        {
            direction = bounceDirection;
            bounceDirection.Set(0f, 0f, 0f);
        }

        /*
         * FINALIZE
         */
        cc.Move(direction * currentSpeed * Time.deltaTime);
        boltDuration -= Time.deltaTime;
        boltCooldown -= Time.deltaTime;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (runningHeld)
        {
            Vector3 mirror = new Vector3(hit.normal.x, 0, hit.normal.z);
            bounceDirection = (cc.velocity - 2 * Vector3.Project(cc.velocity, mirror)).normalized;
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
