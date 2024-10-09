using UnityEngine;

public class EffectBurning: Effect {
    [SerializeField] int damage;
    private IDamageable Damageable;

    public override bool CanEffect(MonoBehaviour _target) {
        return _target is IDamageable;
    }

    public override void Initialize(MonoBehaviour _target) {
        // TODO I would like this to work on IDamageable,
        // but the interface doesn't apply to the gameObject,
        // so I'm not sure how to actually verify:
        // (_target is IDamageable)
        if (_target is IDamageable _damagable){
            base.Initialize(_target);
            Damageable = _damagable;
        }
    }

    public override void Tick() {
        Damageable.TakeDamage(Target.transform.position, damage, HitTier.Pure);
    }
}
