using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


/// <summary>
/// Object that represents a Hitbox, which collides with entities in the game to heal, damage, afflict, etc.
/// NOTE: I can't get the collider properties to comport with its visualization for the life of me when 
///     I update its properties from default values, so any changes to the size will only be to transform.localScale
/// </summary>
[Serializable]
public class Hit : MonoBehaviour {
    enum CoordinateSystem { Polar, Cartesian };

    /* Position */
    private Character caster;
    private Transform origin; // origin of the cast, allowing for movement during animation
    private bool mirror = false;
    [SerializeField] private CoordinateSystem coordinateSystem;
    [SerializeField] private Vector3[] positions;
    [SerializeField] private Quaternion[] rotations;
    [SerializeField] private Vector3[] dimensions;

    /* Collision */
    private Collider hitBox;
    public HashSet<Character> collisionIds = new();

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
    private float animationDuration;

    public void Awake() {
        // ensure that the hitbox information comports with the duration
        Assert.IsTrue(1<=positions.Length&&positions.Length<=duration);
        Assert.IsTrue(1<=rotations.Length&&rotations.Length<=duration);
        Assert.IsTrue(1<=dimensions.Length&&dimensions.Length<=duration);

        hitBox=GetComponent<Collider>();
    }

    public void Start() {
        HandleMove();
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

    public void Initialize(Character _caster, Transform _origin, bool _mirror) {
        caster = _caster;
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

        transform.localScale = dimensions[dimensionFrame];
        transform.position = origin.position+origin.rotation*offset;
        transform.rotation = origin.rotation*orientation*rotations[rotationFrame];
    }

    public void HandleHitCollisions() {
        foreach (Collider otherCollider in GetOverlappingColliders(hitBox)) {
            GameObject go = otherCollider.gameObject;
            Character cb = go.GetComponent<Character>();

            if (cb is not null&&cb!=caster&&!collisionIds.Contains(cb)) {
                collisionIds.Add(cb);
                Vector3 kb = GetKnockBackVector(cb.transform.position);
                cb.TakeDamage(damage, kb, hitStunDuration, hitTier);
            }
        }
    }

    private void OnDestroy() {
        if (origin.GetComponent<Character>() is null) {
            Destroy(origin.gameObject);
        }
    }
}
