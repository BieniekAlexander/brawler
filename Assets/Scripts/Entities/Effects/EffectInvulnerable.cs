using UnityEngine;

public class EffectInvulnerable: Cast {
    private IDamageable Effected;

    protected override void OnInitialize() {
        Effected = About.GetComponent<IDamageable>();
        Effected.InvulnerableStack++;
    }

    protected override void OnDestruction() {
        if (Effected != null) {
            Effected.InvulnerableStack--;
        }
    }

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is IDamageable;
    }
}    