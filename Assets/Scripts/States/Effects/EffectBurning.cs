using UnityEngine;

public class EffectBurning : Effect {
    [SerializeField] int TickRate;
    [SerializeField] int Damage;
    private IDamageable Damageable;

    override public bool AppliesTo(GameObject go) => go.GetComponent<IDamageable>()!=null;

    protected override void OnInitialize() {
        Damageable = About.gameObject.GetComponent<IDamageable>();
    }

    override protected void Tick() {
        if (Frame % TickRate == 0 && Damageable != null) {
            Damageable.TakeDamage(Target.transform.position, Damage, HitTier.Pure);
        }
    }
}
