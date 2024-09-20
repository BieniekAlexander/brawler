using UnityEngine;

public class StatusEffectBurning: EffectBase {
    [SerializeField] int damage;

    public override void Initialize(Character _character) {
        base.Initialize(_character);
    }

    public override void Tick() {
        target.TakeDamage(damage);
    }
}
