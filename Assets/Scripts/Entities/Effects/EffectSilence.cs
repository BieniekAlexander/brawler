using UnityEngine;

public class EffectSilence: Effect {
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

    override public bool AppliesTo(GameObject go) => go.GetComponent<ICasts>() != null;
}
