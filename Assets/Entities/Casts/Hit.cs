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
    [SerializeField] private CoordinateSystem positionCoordinateSystem;
    [SerializeField] private Vector3[] positions;
    [SerializeField] private Quaternion[] rotations;
    [SerializeField] private Vector3[] dimensions;

    /* Collision */
    private Collider hitBox;
    public HashSet<Character> collisionIds = new();
    [SerializeField] public List<StatusEffectBase> effects = new();

    /* Duration */
    [SerializeField] public int duration;
    private int frame = 0;

    /* Knockback */
    [SerializeField] private CoordinateSystem knockbackCoordinateSystem;
    [SerializeField] private Vector3 knockbackVector;
    [SerializeField][Range(0, 3)] private int hitTier;

    /* Effects */
    [SerializeField] private int damage;
    [SerializeField] private int hitStunDuration;
    [SerializeField] private bool hitsEnemies = true;
    [SerializeField] private bool hitsFriendlies = false;

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
        if (knockbackCoordinateSystem==CoordinateSystem.Cartesian) {
            Vector3 knockBackDirection = origin.transform.rotation * knockbackVector;
            knockBackDirection.y = 0f;
            return knockBackDirection;
        } else { // polar
            Vector3 hitboxToTargetNormalized = (targetPosition - transform.position).normalized;
            Vector3 knockBackDirection = Quaternion.Euler(0, knockbackVector.x, 0) * hitboxToTargetNormalized * knockbackVector.z;
            knockBackDirection.y = 0f;
            return knockBackDirection;
        }
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

        Vector3 offset = (positionCoordinateSystem==CoordinateSystem.Cartesian)
            ? positions[positionFrame]
            : Quaternion.Euler(0, positions[positionFrame].x, 0)*Vector3.forward*positions[positionFrame].z;

        Quaternion orientation = (positionCoordinateSystem==CoordinateSystem.Cartesian)
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
        List<Collider> otherColliders = GetOverlappingColliders(hitBox).ToList();

        foreach (Collider otherCollider in GetOverlappingColliders(hitBox)) {
            GameObject go = otherCollider.gameObject;
            Character character = go.GetComponent<Character>();

            if (character is not null && !collisionIds.Contains(character)) {
                collisionIds.Add(character); // this hitbox already hit this character - skip them

                if (
                    (character == caster && !hitsFriendlies)
                    || (character != caster && !hitsEnemies)
                    ) {
                    continue;
                }

                Shield shield = character.GetComponentInChildren<Shield>();

                if (
                    shield==null || !shield.isActiveAndEnabled
                    || ((transform.position - shield.transform.position).sqrMagnitude > (transform.position - character.transform.position).sqrMagnitude)
                ) {
                    Vector3 kb = GetKnockBackVector(character.transform.position);
                    character.TakeDamage(damage, kb, hitStunDuration, hitTier);

                    for (int i = 0; i < effects.Count; i++) {
                        // TODO what if I don't want the status effect to stack?
                        // maybe check if an effect of the same type is active, and if so, do some sort of resolution
                        // e.g. if two slows are applied, refresh slow
                        StatusEffectBase effect = Instantiate(effects[i]);
                        effect.Initialize(character);
                    }
                } else {
                    ;// hitting shield - any shield logic?
                }
            }
        }

    }

    private void OnDestroy() {
        if (origin.GetComponent<Character>() == null) {
            Destroy(origin.gameObject);
        }
    }
}
