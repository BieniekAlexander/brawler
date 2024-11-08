using UnityEngine;

public class EffectEncumber: Effect {
    protected override void OnInitialize() {
        About.GetComponent<IMoves>().EncumberedStack++;
    }

    protected override void OnDestruction() {
        About.GetComponent<IMoves>().EncumberedStack--;
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<IMoves>() != null;
}
