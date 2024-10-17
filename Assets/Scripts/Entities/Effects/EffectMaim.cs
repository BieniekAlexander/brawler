using UnityEngine;

public class EffectMaim: Castable {
    protected override void OnInitialize() {
        About.GetComponent<ICasts>().MaimStack++;
    }

    protected override void OnExpire() {
        if (About != null) {
            About.GetComponent<ICasts>().MaimStack--;
        }
    }

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is ICasts;
    }
}
