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
public class Hit : Trigger {
    /* Effects */
    [SerializeField] private int damage;
    [SerializeField] public bool HitsEnemies = true;
    [SerializeField] public bool HitsFriendlies = false;

    /* Knockback */
    [SerializeField] private CoordinateSystem KnockbackCoordinateSystem;
    [SerializeField] private Vector3 BaseKnockbackVector;
    [SerializeField][Range(0, 3)] public int HitTier;
    [SerializeField] private int hitLagDuration = 0;
    [SerializeField] private int hitStunDuration = 10;

    /* Visualization */
    private float animationDuration;

    /* Getters */
    private Vector3 GetKnockBackVector(Vector3 targetPosition) {
        if (KnockbackCoordinateSystem==CoordinateSystem.Cartesian) {
            // TODO knockback is behaving very oddly with this setting, particularly with the throw hitbox I was working on - why?
            Vector3 knockBackDirection = Origin.transform.rotation * BaseKnockbackVector;
            if (Mirror) knockBackDirection.x *= -1; // TODO is this mirror redundant? I'm adding this because cartesian KB vectors go the wrong way before this change
            return knockBackDirection;
        } else { // polar
            Vector3 hitboxToTargetNormalized = Vector3.Scale(targetPosition - transform.position, new Vector3(1, 0, 1)).normalized;
            Vector3 knockBackDirection = Quaternion.Euler(0, BaseKnockbackVector.x, 0) * hitboxToTargetNormalized * BaseKnockbackVector.z;
            return knockBackDirection;
        }
    }

    /* Initialization */
    public static Hit Initiate(Hit _hit, Character _owner, Transform _origin, bool _mirror) {
        Hit hit = Instantiate(_hit);
        hit.Initialize(_owner, _origin, _mirror);
        return hit;
    }

    public static Hit Instantiate(Grab _grab, Character _owner, Transform _origin, bool _mirror) {
        Hit hit = Instantiate(_grab);
        hit.Initialize(_owner, _origin, _mirror);
        return hit;
    }

    public static Hit Initiate(Hit _hit, Vector3 _position, Quaternion _rotation, Character _owner, Transform _origin, bool _mirror) {
        Hit hit = Instantiate(_hit, _position, _rotation);
        hit.Initialize(_owner, _origin, _mirror);
        return hit;
    }

    private void Initialize(Character _owner, Transform _origin, bool _mirror) {
        Caster = _owner;
        Mirror = _mirror;

        if (_origin is null) { // if the constructor didn't supply an origin to follow, make one
            Origin=new GameObject().transform;
            Origin.position=transform.position;
            Origin.rotation=transform.rotation;
        } else {
            Origin=_origin;
        }

        transform.rotation=Origin.rotation*rotations[0];
        transform.position=Origin.position+positions[0];
    }

    /* Utility Functions */
    public static float GetKnockBackFactor(Hit hit, Shield shield) {
        if (hit.HitTier==0) return 0;
        else return ((float)hit.HitTier - (int)shield.ShieldLevel)/hit.HitTier;
    }

    /* State Handling */
    private void FixedUpdate() {
        if (frame>=duration)
            Destroy(gameObject);

        UpdateTransform();
        HandleCollisions();
        frame++;
    }

    public virtual void HandleCollisions() {
        List<Collider> otherColliders = GetOverlappingColliders(Collider).ToList();

        foreach (Collider otherCollider in GetOverlappingColliders(Collider)) {
            // identify the character that was hit
            GameObject go = otherCollider.gameObject;
            Character target = go.GetComponent<Character>();
            if (target == null) { // the case that the hitbox hit a child of the character
                target = go.GetComponentInParent<Character>();
            }

            if (target is not null && !RedundantCollisions && !TriggeredColliderIds.Contains(target)) {
                TriggeredColliderIds.Add(target); // this hitbox already hit this character - skip them

                if (
                    (Caster==target && !HitsFriendlies)
                    || (Caster!=target && !HitsEnemies)
                    ) {
                    continue;
                }

                Vector3 baseKnockbackVector = GetKnockBackVector(target.transform.position);
                float knockbackFactor = HitTier>0?1f:0f;
                Shield shield = target.GetComponentInChildren<Shield>();
                int d = damage;

                if ( // hitting shield
                    shield!=null
                    && shield.isActiveAndEnabled
                    && GetClosestGameObject(gameObject, target.gameObject, shield.gameObject)==shield.gameObject
                ) {
                    knockbackFactor = GetKnockBackFactor(this, shield);
                } else { // hitting player
                    for (int i = 0; i < effects.Count; i++) {
                        // TODO what if I don't want the status effect to stack?
                        // maybe check if an effect of the same type is active, and if so, do some sort of resolution
                        // e.g. if two slows are applied, refresh slow
                        EffectBase effect = Instantiate(effects[i]);
                        effect.Initialize(target);
                    }

                    target.TakeDamage(d);
                }

                if (knockbackFactor>0) {
                    target.TakeKnockback(
                        hitLagDuration,
                        knockbackFactor*baseKnockbackVector,
                        hitStunDuration);

                    if (Origin.transform == Caster.transform) {
                        Caster.TakeKnockback(hitLagDuration);
                    }
                } else if (knockbackFactor<0 && Origin.gameObject.CompareTag("Character")) {
                    Character c = Origin.GetComponent<Character>();
                    c.TakeKnockback(
                        0,
                        knockbackFactor*baseKnockbackVector,
                        hitStunDuration);
                }
            }
        }

    }
}
