using UnityEngine;

public class EffectSilence: Cast {
    private ICasts Effected;

    protected override void OnInitialize() {
        Effected = About.GetComponent<ICasts>();
        Effected.SilenceStack++;
    }

    protected override void OnDestruction() {
        if (Effected != null) {
            Effected.SilenceStack--;
        }
    }

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is ICasts;
    }
}
