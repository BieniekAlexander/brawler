using UnityEngine;

public class EffectStun: Effect {
    protected override void OnInitialize() {
        About.GetComponent<IMoves>().StunStack++;
    }

    protected override void OnDestruction() {
        About.GetComponent<IMoves>().StunStack--;
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<IMoves>() != null;
}
