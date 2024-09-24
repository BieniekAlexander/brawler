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
    public override void OnCollideWith(ICollidable Other) {
        Physics.Raycast(
                transform.position,
                Other.GetCollider().transform.position,
                out RaycastHit hitInfo,
                (1<<LayerMask.NameToLayer("Characters"))
                | (1<<LayerMask.NameToLayer("Projectiles")) // TODO dunno if this will hit shields, but it should
            ); // TODO this will also currently ignore other IDamagables and IMovables, but I'll fix this later

        if (Other is IDamageable OtherDamagable) {
            if ((Caster==OtherDamagable && !HitsFriendlies)
                || (Caster!=OtherDamagable && !HitsEnemies)
            ) {
                return;
            } else {
                OtherDamagable.TakeDamage(
                    hitInfo.point,
                    damage,
                    HitTier
                );
            }
        }

        if (Other is IMoves OtherMover) {
            if ((Caster==OtherMover && !HitsFriendlies)
                || (Caster!=OtherMover && !HitsEnemies)
                || BaseKnockbackVector == Vector3.zero // if the attack has no knockback - TODO is this the best way to enforce this?
            ) {
                return;
            } else {
                Vector3 knockBackVector = GetKnockBackVector(OtherMover.GetTransform().position);
                float targetKnockbackFactor = OtherMover.TakeKnockBack(
                    hitInfo.point,
                    hitLagDuration,
                    knockBackVector,
                    hitStunDuration,
                    HitTier
                );

                if (targetKnockbackFactor < 0f && (Caster is IMoves thisMover)) {
                    // TODO this might apply knockback to things other than characters
                    thisMover.TakeKnockBack(
                        thisMover.GetTransform().position,
                        hitLagDuration,
                        knockBackVector,
                        hitStunDuration,
                        HitTier
                    );
                }
            }
        }
    }
}

