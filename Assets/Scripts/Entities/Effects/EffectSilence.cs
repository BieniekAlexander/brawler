using UnityEngine;

public class EffectSilence: Castable {
    private ICasts Effected;

    protected override void OnInitialize() {
        Effected = About.GetComponent<ICasts>();
        Effected.SilenceStack++;
    }

    protected override void OnExpire() {
        if (Effected != null) {
            Effected.SilenceStack--;
        }
    }

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is ICasts;
    }
}
