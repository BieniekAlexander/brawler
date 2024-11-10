using System;
using UnityEngine;

public enum KnockbackReference {
    About,
    Caster
};

/// <summary>
/// Object that represents a Hitbox, which collides with entities in the game to heal, damage, afflict, etc.
/// NOTE: I can't get the collider properties to comport with its visualization for the life of me when 
///     I update its properties from default values, so any changes to the size will only be to transform.localScale
/// </summary>
[Serializable]
public class Hit : Trigger, ICollidable {
    /* Effects */
    [SerializeField] private FieldExpression<Hit, int> damageExpression = new("0");
    private int Damage { get { return damageExpression.Value; } }

    override protected void OnInitialize() {
        base.OnInitialize();
        FieldExpressionParser.instance.RenderValue(this, damageExpression);
    }

    /* Knockback */
    [SerializeField] private KnockbackReference KnockbackReference = KnockbackReference.About;
    [SerializeField] private Vector3 BaseKnockbackVector;
    [SerializeField] public HitTier HitTier;

    /* Visualization */
    private float animationDuration;

    /* Getters */
    private Vector3 GetKnockBackVector(Vector3 targetPosition) {
        switch (KnockbackReference) {
            case KnockbackReference.About:
                return Quaternion.FromToRotation(
                    Vector3.forward,
                    MovementUtils.inXZ(targetPosition - transform.position).normalized
                ) * BaseKnockbackVector;
            case KnockbackReference.Caster:
                return Caster.GetAboutTransform().rotation * new Vector3(
                    Mirrored?BaseKnockbackVector.x:-BaseKnockbackVector.x,
                    BaseKnockbackVector.y,
                    BaseKnockbackVector.z
                );
            default:
                throw new Exception($"Unhandled KnockbackReference type: {KnockbackReference}");
        }
    }

    /* ICollidable Methods */
    public override bool OnCollideWith(ICollidable Other, CollisionInfo info) {
        bool ret = false;
        base.OnCollideWith(Other, info);

        Physics.Raycast(
                transform.position,
                Other.Collider.transform.position,
                out RaycastHit hitInfo,
                (1<<LayerMask.NameToLayer("Characters"))
                | (1<<LayerMask.NameToLayer("Projectiles")) // TODO dunno if this will hit shields, but it should
            ); // TODO this will also currently ignore other IDamagables and IMovables, but I'll fix this later

        if (Other is IDamageable OtherDamagable) {
            if ((Caster==OtherDamagable && !HitsFriendlies)
                || (Caster!=OtherDamagable && !HitsEnemies)
            ) {
            } else {
                int remainingDamage = OtherDamagable.TakeDamage(
                    hitInfo.point,
                    Damage,
                    HitTier
                );

                if (Caster is Character c) { // Throwing this in for RL agent - TODO clean this up
                    c.DamageDealt = Damage-remainingDamage;
                }

                ret = true;
            }
        }

        if (Other is IMoves OtherMover) {
            if ((Caster==OtherMover && !HitsFriendlies)
                || (Caster!=OtherMover && !HitsEnemies)
                || BaseKnockbackVector == Vector3.zero // if the attack has no knockback - TODO is this the best way to enforce this?
            ) {
                ;
            } else {
                Vector3 knockBackVector = GetKnockBackVector(OtherMover.Transform.position);
                int hitStunDuration = KnockBackUtils.getHitStun(HitTier);
                int hitStopDuration = KnockBackUtils.getHitStop(HitTier);

                float shieldKnockBackFactor = OtherMover.TakeKnockBack(
                    hitInfo.point,
                    hitStopDuration,
                    knockBackVector,
                    hitStunDuration,
                    HitTier
                );

                if (shieldKnockBackFactor < 0f && (Caster is IMoves thisMover)) {
                    // TODO this might apply knockback to things other than characters
                    thisMover.TakeKnockBack(
                        thisMover.Transform.position,
                        hitStopDuration,
                        shieldKnockBackFactor*knockBackVector,
                        hitStunDuration,
                        HitTier
                    );
                } else if (Caster!=null && Caster.GetAboutTransform()==About && Caster is IDamageable damageable && shieldKnockBackFactor>0f) {
                    damageable.HitStopTimer = hitStopDuration;
                }

                ret = true;
            }
        }
        
        return ret;
    }
}
