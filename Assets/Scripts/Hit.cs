using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;


 [Serializable]
public class Hit : MonoBehaviour
{
    enum CoordinateSystem { Polar, Cartesian };
    enum KnockbackOrigin { Origin, HitBox };

    /* Position */
    private Transform origin = null; // origin of the cast, allowing for movement during animation
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    [SerializeField] private CoordinateSystem coordinateSystem;
    [SerializeField] private Vector3[] positions;
    [SerializeField] private Quaternion[] rotations;
    [SerializeField] private Vector3[] dimensions;

    /* Collision */
    private Collider hitBox;
    public HashSet<CharacterBehavior> collisionIds = new HashSet<CharacterBehavior>();

    /* Duration */
    [SerializeField] public int duration;
    private int frame = 0;

    /* Knockback */
    [SerializeField] private float knockbackMagnitude = 1f;
    [SerializeField] private KnockbackOrigin knockbackOrigin;
    [SerializeField] private Vector3 knockbackTransform;

    /* Effects */
    [SerializeField] private int damage = 40;

    // Visualization
    //private ParticleSystem ps;
    private float animationDuration;

    public void Awake()
    {
        // ensure that the hitbox information comports with the duration
        Assert.IsTrue(1 <= positions.Length && positions.Length <= duration);
        Assert.IsTrue(1 <= rotations.Length && rotations.Length <= duration);
        Assert.IsTrue(1 <= dimensions.Length && dimensions.Length <= duration);
        
        
        // resize wireframe for vizualization
        hitBox = GetComponent<Collider>();
        SetHitboxDImensions(dimensions[0]);

        if (hitBox is CapsuleCollider) {
            CapsuleCollider collider = hitBox as CapsuleCollider;
            collider.transform.localScale = new Vector3(collider.radius, collider.height, collider.radius);
        } else if (hitBox is SphereCollider) {
            SphereCollider collider = hitBox as SphereCollider;
            collider.transform.localScale = new Vector3(collider.radius, collider.radius, collider.radius);
        } else {
            throw new Exception("Unhandled wireframe resizing");
        }
    }

    public Vector3 GetOriginPosition() {
        return (origin != null) ? origin.position : initialPosition;
    }

    private Vector3 GetKnockBackVector(Vector3 targetPosition) {
        Vector3 toTarget = -((knockbackOrigin == KnockbackOrigin.HitBox) ? transform.position : GetOriginPosition()) + targetPosition;

        Vector3 knockBackDirection = Quaternion.Euler(0, knockbackTransform.x, 0) * toTarget;
        knockBackDirection.y = 0f;
        return knockBackDirection.normalized*knockbackMagnitude;
    }

    public static IEnumerable<Collider> GetOverlappingColliders(Collider collider)
    {
        Collider[] colliders = FindObjectsOfType<Collider>();
        return (from c in colliders where collider.bounds.Intersects(c.bounds) select c);
    }

    public void Initialize(Transform _origin)
    {
        origin = _origin;
        transform.rotation = rotations[0];
        transform.position = origin.position + positions[0];
    }

    public void Initialize(Vector3 _initialPosition, Quaternion _initialRotation)
    {
        initialPosition = _initialPosition;
        initialRotation = _initialRotation;
        transform.rotation = initialRotation * rotations[0];
        transform.position = initialPosition + positions[0];
    }

    private void FixedUpdate()
    {
        if (frame >= duration)
            Destroy(gameObject);
        
        HandleMove();
        HandleHitCollisions();
        frame++;
    }

    public void HandleMove()
    {
        int positionFrame = Mathf.Min(frame, positions.Length - 1);
        int rotationFrame = Mathf.Min(frame, rotations.Length - 1);
        int dimensionFrame = Mathf.Min(frame, dimensions.Length - 1);

        Vector3 offset = (coordinateSystem == CoordinateSystem.Cartesian)
            ? positions[positionFrame]
            : Quaternion.Euler(0, positions[positionFrame].x, 0) * Vector3.forward * positions[rotationFrame].z;

        Quaternion orientation = (coordinateSystem == CoordinateSystem.Cartesian)
            ? Quaternion.identity
            : Quaternion.Euler(0, positions[positionFrame].x, 0);

        // TODO make sure that the calculations without an origin are correct
        transform.position = ((origin!=null)?origin.position:initialPosition) + origin.rotation * offset;
        transform.rotation = ((origin!=null)?origin.rotation:initialRotation) * orientation * rotations[rotationFrame];
    }

    public void HandleHitCollisions()
    {
        foreach (Collider otherCollider in GetOverlappingColliders(hitBox))
        {
            GameObject go = otherCollider.gameObject;
            CharacterBehavior cb = go.GetComponent<CharacterBehavior>();

            if (cb is not null && !collisionIds.Contains(cb))
            {
                collisionIds.Add(cb);
                Vector3 kb = GetKnockBackVector(cb.transform.position);
                cb.TakeDamage(damage, kb, 6);
            }
        }
    }

    private void SetHitboxDImensions(Vector3 scale) {
        if (hitBox is CapsuleCollider) {
            CapsuleCollider cc = hitBox as CapsuleCollider;
            cc.radius = scale.x;
            cc.height = scale.y;
        } else if (hitBox is SphereCollider) {
            SphereCollider sc = hitBox as SphereCollider;
            sc.radius = scale.x;
        }
    }
}
