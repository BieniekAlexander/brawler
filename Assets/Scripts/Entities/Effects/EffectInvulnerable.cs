using UnityEngine;

public class EffectInvulnerable: Effect {
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

    override public bool AppliesTo(GameObject go) => go.GetComponent<IDamageable>()!=null;
}    