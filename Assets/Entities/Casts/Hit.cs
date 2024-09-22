using System;
using UnityEngine;



/// <summary>
/// Object that represents a Hitbox, which collides with entities in the game to heal, damage, afflict, etc.
/// NOTE: I can't get the collider properties to comport with its visualization for the life of me when 
///     I update its properties from default values, so any changes to the size will only be to transform.localScale
/// </summary>
[Serializable]
public class Hit : Trigger, ICollidable {
    /* Effects */
    [SerializeField] private int damage;
    [SerializeField] public bool HitsEnemies = true;
    [SerializeField] public bool HitsFriendlies = false;

    /* Knockback */
    [SerializeField] private CoordinateSystem KnockbackCoordinateSystem;
    [SerializeField] private Vector3 BaseKnockbackVector;
    [SerializeField] public HitTier HitTier;
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

    /* ICollidable Methods */
    public virtual void OnCollideWith(ICollidable other) {
        Physics.Raycast(
                transform.position,
                other.GetCollider().transform.position,
                out RaycastHit hitInfo,
                LayerMask.NameToLayer("Characters") // TODO dunno if this will hit shields, but it should
            ); // TODO this will also currently ignore other IDamagables and IMovables, but I'll fix this later

        if (other is IMoves Mover) {
            float targetKnockbackFactor = Mover.TakeKnockBack(
                    hitInfo.point,
                    hitLagDuration,
                    BaseKnockbackVector,
                    hitStunDuration,
                    HitTier
                );

            if (targetKnockbackFactor > 0f && (Caster is IMoves thisMover)) {
                // TODO this might apply knockback to things other than characters
                thisMover.TakeKnockBack(
                    thisMover.GetTransform().position,
                    hitLagDuration,
                    targetKnockbackFactor  * BaseKnockbackVector,
                    hitStunDuration,
                    HitTier
                );
            }
        }

        if (other is IDamageable Damagable) {
            Damagable.TakeDamage(
                hitInfo.point,
                damage,
                HitTier
            );
        }
    }

    /*
    foreach (Collider target in GetOverlappingColliders(Collider)) {
        if (
            (Caster==target.gameObject && !HitsFriendlies)
            || (Caster!=target.gameObject && !HitsEnemies)
        ) {
            ;
        } else {
            // find the contact point of the hit and apply knockback
            // as implemented below, the contact point is the raycast from the hitbox center to the target 
            Physics.Raycast(
                transform.position,
                target.transform.position,
                out RaycastHit hitInfo,
                LayerMask.NameToLayer("Characters") // TODO dunno if this will hit shields, but it should
            );

            if (target is IMoves) { // if it can be knocked back, do so
                // TODO make sure this all behaves properly
                float targetKnockbackFactor = (target as IMoves).TakeKnockBack(
                    hitInfo.point,
                    hitLagDuration,
                    BaseKnockbackVector,
                    hitStunDuration,
                    HitTier
                );

                if (targetKnockbackFactor > 0f && Caster.CompareTag("Character")) {
                    (Caster.GetComponent<Character>()).TakeKnockBack(
                        Caster.transform.position,
                        hitLagDuration,
                        targetKnockbackFactor  * BaseKnockbackVector,
                        hitStunDuration,
                        1
                    );
                }
            }

            if (target is IDamageable) {
                (target as IDamageable).TakeDamage(
                    hitInfo.point,
                    damage
                );
            }   
        }   
    }*/
}

