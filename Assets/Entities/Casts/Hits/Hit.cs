using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;


[Serializable]
public class Hit : MonoBehaviour {
    enum CoordinateSystem { Polar, Cartesian };

    /* Position */
    private CharacterBehavior caster;
    private Transform origin; // origin of the cast, allowing for movement during animation
    private bool mirror = false;
    [SerializeField] private CoordinateSystem coordinateSystem;
    [SerializeField] private Vector3[] positions;
    [SerializeField] private Quaternion[] rotations;
    [SerializeField] private Vector3[] dimensions;

    /* Collision */
    private Collider hitBox;
    public HashSet<CharacterBehavior> collisionIds = new();

    /* Duration */
    [SerializeField] public int duration;
    private int frame = 0;

    /* Knockback */
    [SerializeField] private float knockbackMagnitude;
    [SerializeField] private Vector3 knockbackTransform;
    [SerializeField][Range(1, 4)] private int hitTier;

    /* Effects */
    [SerializeField] private int damage;
    [SerializeField] private int hitStunDuration;

    // Visualization
    //private ParticleSystem ps;
    private float animationDuration;

    public void Awake() {
        // ensure that the hitbox information comports with the duration
        Assert.IsTrue(1<=positions.Length&&positions.Length<=duration);
        Assert.IsTrue(1<=rotations.Length&&rotations.Length<=duration);
        Assert.IsTrue(1<=dimensions.Length&&dimensions.Length<=duration);


        // resize wireframe for vizualization
        hitBox=GetComponent<Collider>();
        SetHitboxDImensions(dimensions[0]);

        if (hitBox is CapsuleCollider) {
            CapsuleCollider collider = hitBox as CapsuleCollider;
            collider.transform.localScale=new Vector3(collider.radius, collider.height, collider.radius);
        } else if (hitBox is SphereCollider) {
            SphereCollider collider = hitBox as SphereCollider;
            collider.transform.localScale=new Vector3(collider.radius, collider.radius, collider.radius);
        } else {
            throw new Exception("Unhandled wireframe resizing");
        }
    }

    private Vector3 GetKnockBackVector(Vector3 targetPosition) {
        Vector3 toTarget = targetPosition-origin.position;

        Vector3 knockBackDirection = Quaternion.Euler(0, knockbackTransform.x, 0)*toTarget;
        knockBackDirection.y=0f;
        return knockBackDirection.normalized*knockbackMagnitude;
    }

    public static IEnumerable<Collider> GetOverlappingColliders(Collider collider) {
        Collider[] colliders = FindObjectsOfType<Collider>();
        return (from c in colliders where collider.bounds.Intersects(c.bounds) select c);
    }

    public void Initialize(CharacterBehavior _caster, Transform _origin, bool _mirror) {
        caster=_caster;
        mirror = _mirror;

        if (_origin is null) { // if the constructor didn't supply an origin to follow, make one
            origin=new GameObject().transform;
            origin.position=transform.position;
            origin.rotation=transform.rotation;
        } else {
            origin=_origin;
        }

        transform.rotation=origin.rotation*rotations[0];
        transform.position=origin.position+positions[0];
    }

    private void FixedUpdate() {
        if (frame>=duration)
            Destroy(gameObject);

        HandleMove();
        HandleHitCollisions();
        frame++;
    }

    public void HandleMove() {
        int positionFrame = Mathf.Min(frame, positions.Length-1);
        int rotationFrame = Mathf.Min(frame, rotations.Length-1);
        int dimensionFrame = Mathf.Min(frame, dimensions.Length-1);

        Vector3 offset = (coordinateSystem==CoordinateSystem.Cartesian)
            ? positions[positionFrame]
            : Quaternion.Euler(0, positions[positionFrame].x, 0)*Vector3.forward*positions[rotationFrame].z;

        Quaternion orientation = (coordinateSystem==CoordinateSystem.Cartesian)
            ? Quaternion.identity
            : Quaternion.Euler(0, positions[positionFrame].x, 0);

        // TODO make sure that the calculations without an origin are correct
        if (mirror) {
            offset.x *= -1;
            orientation.y *= -1;
        }

        transform.position=origin.position+origin.rotation*offset;
        transform.rotation=origin.rotation*orientation*rotations[rotationFrame];
    }

    public void HandleHitCollisions() {
        foreach (Collider otherCollider in GetOverlappingColliders(hitBox)) {
            GameObject go = otherCollider.gameObject;
            CharacterBehavior cb = go.GetComponent<CharacterBehavior>();

            if (cb is not null&&cb!=caster&&!collisionIds.Contains(cb)) {
                collisionIds.Add(cb);
                Vector3 kb = GetKnockBackVector(cb.transform.position);
                cb.TakeDamage(damage, kb, hitStunDuration, hitTier);
            }
        }
    }

    private void SetHitboxDImensions(Vector3 scale) {
        if (hitBox is CapsuleCollider) {
            CapsuleCollider cc = hitBox as CapsuleCollider;
            cc.radius=scale.x;
            cc.height=scale.y;
        } else if (hitBox is SphereCollider) {
            SphereCollider sc = hitBox as SphereCollider;
            sc.radius=scale.x;
        }
    }

    private void OnDestroy() {
        if (caster is null) {
            Destroy(origin.gameObject);
        }
    }
}
