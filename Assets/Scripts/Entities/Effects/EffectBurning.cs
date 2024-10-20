using UnityEngine;

public class EffectBurning : Cast {
    [SerializeField] int TickRate;
    [SerializeField] int Damage;
    private IDamageable Damageable;

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is IDamageable;
    }

    protected override void OnInitialize() {
        Damageable = About.gameObject.GetComponent<IDamageable>();
    }

    override protected void Tick() {
        if (Frame % TickRate == 0 && Damageable != null) {
            Damageable.TakeDamage(Target.transform.position, Damage, HitTier.Pure);
        }
    }
}
