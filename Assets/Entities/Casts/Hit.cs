using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.TextCore.Text;
using System.Runtime.InteropServices.WindowsRuntime;


/// <summary>
/// Object that represents a Hitbox, which collides with entities in the game to heal, damage, afflict, etc.
/// NOTE: I can't get the collider properties to comport with its visualization for the life of me when 
///     I update its properties from default values, so any changes to the size will only be to transform.localScale
/// </summary>
[Serializable]
public class Hit : MonoBehaviour {
    enum CoordinateSystem { Polar, Cartesian };

    /* Position */
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
    [SerializeField] private int hitLagDuration = 0;
    [SerializeField] private int hitStunDuration = 10;
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
            if (mirror) knockBackDirection.x *= -1; // TODO is this mirror redundant? I'm adding this because cartesian KB vectors go the wrong way before this change
            return knockBackDirection;
        } else { // polar
            Vector3 hitboxToTargetNormalized = Vector3.Scale(targetPosition - transform.position, new Vector3(1,0,1)).normalized;
            Vector3 knockBackDirection = Quaternion.Euler(0, knockbackVector.x, 0) * hitboxToTargetNormalized * knockbackVector.z;
            return knockBackDirection;
        }
    }

    public static IEnumerable<Collider> GetOverlappingColliders(Collider collider) {
        Collider[] colliders = FindObjectsOfType<Collider>();
        return (from c in colliders where collider.bounds.Intersects(c.bounds) select c);
    }

    public static Hit Initiate(Hit _hit, Transform _origin, bool _mirror) {
        Hit hit = Instantiate(_hit);
        hit.Initialize(_origin, _mirror);
        return hit;
    }

    public static Hit Initiate(Hit _hit, Vector3 _position, Quaternion _rotation, Transform _origin, bool _mirror) {
        Hit hit = Instantiate(_hit, _position, _rotation);
        hit.Initialize(_origin, _mirror);
        return hit;
    }

    private void Initialize(Transform _origin, bool _mirror) {
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

    public static GameObject GetClosestGameObject(GameObject reference, params GameObject[] others) {
        Array.Sort(
            others,
            (o1, o2) => { return ((reference.transform.position - o1.transform.position).sqrMagnitude).CompareTo((reference.transform.position - o2.transform.position).sqrMagnitude); }
        );

        return others[0];
    }

    public static float GetKnockBackFactor(Hit hit, Shield shield) {
        if (hit.hitTier==0) return 0;
        else return ((float)hit.hitTier - (int)shield.ShieldLevel)/hit.hitTier;
    }

    public Vector3 GetHitLagVelocity(Character target) {
        // TODO generalize to someting that's moving (not just a character)
        if (origin.CompareTag("Character")) {
            Vector3 toTarget = target.transform.position-origin.position;
            Vector3 originVelocity = origin.GetComponent<Character>().Velocity;
            Vector3 frameVelocity = target.Velocity - originVelocity;
            Vector3 normal = new Vector3(-toTarget.z, 0f, toTarget.x).normalized;
            Debug.Log(normal);

            Vector3 velocityPerp = Vector3.Project(originVelocity, toTarget);
            Vector3 hitLagNormal = Vector3.Project(originVelocity, normal);
            
            if (Vector3.Dot(velocityPerp, toTarget)>0) { // if the frame of reference is towards the target
                return velocityPerp + hitLagNormal;
            } else {
                return hitLagNormal;
            }
        }
        
        // fallback
        return Vector3.zero;
    }

    public void HandleHitCollisions() {
        List<Collider> otherColliders = GetOverlappingColliders(hitBox).ToList();

        foreach (Collider otherCollider in GetOverlappingColliders(hitBox)) {
            // identify the character that was hit
            GameObject go = otherCollider.gameObject;
            Character character = go.GetComponent<Character>();
            if (character == null) { // the case that something else of the c
                character = go.GetComponentInParent<Character>();
            }

            if (character is not null && !collisionIds.Contains(character)) {
                collisionIds.Add(character); // this hitbox already hit this character - skip them

                if (
                    (origin==character.transform && !hitsFriendlies)
                    || (origin!=character.transform && !hitsEnemies)
                    ) {
                    continue;
                }

                Vector3 baseKnockbackVector = GetKnockBackVector(character.transform.position);
                float knockbackFactor = 1f;
                Shield shield = character.GetComponentInChildren<Shield>();
                int d = damage;

                if ( // hitting shield
                    shield!=null
                    && shield.isActiveAndEnabled
                    && GetClosestGameObject(gameObject, character.gameObject, shield.gameObject)==shield.gameObject
                ) {
                    knockbackFactor = GetKnockBackFactor(this, shield);
                } else { // hitting player
                    for (int i = 0; i < effects.Count; i++) {
                        // TODO what if I don't want the status effect to stack?
                        // maybe check if an effect of the same type is active, and if so, do some sort of resolution
                        // e.g. if two slows are applied, refresh slow
                        StatusEffectBase effect = Instantiate(effects[i]);
                        effect.Initialize(character);
                        character.TakeDamage(d, hitTier);
                    }
                }

                if (knockbackFactor>0) {
                    character.TakeKnockback(
                        GetHitLagVelocity(character),
                        hitLagDuration,
                        knockbackFactor*baseKnockbackVector,
                        hitStunDuration);
                } else if (knockbackFactor<0 && origin.gameObject.CompareTag("Character")) {
                    Character c = origin.GetComponent<Character>();
                    c.TakeKnockback(
                        Vector3.zero,
                        0,
                        knockbackFactor*baseKnockbackVector,
                        hitStunDuration);
                }
            }
        }

    }
}
