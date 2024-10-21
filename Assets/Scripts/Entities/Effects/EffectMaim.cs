using UnityEngine;

public class EffectMaim: Effect {
    protected override void OnInitialize() {
        About.GetComponent<ICasts>().MaimStack++;
    }

    protected override void OnDestruction() {
        if (About != null) {
            About.GetComponent<ICasts>().MaimStack--;
        }
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<ICasts>() != null;
}
