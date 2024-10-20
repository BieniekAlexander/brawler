using UnityEngine;

public class EffectMaim: Cast {
    protected override void OnInitialize() {
        About.GetComponent<ICasts>().MaimStack++;
    }

    protected override void OnDestruction() {
        if (About != null) {
            About.GetComponent<ICasts>().MaimStack--;
        }
    }

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is ICasts;
    }
}
