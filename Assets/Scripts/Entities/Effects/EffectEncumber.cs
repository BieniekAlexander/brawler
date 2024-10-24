using UnityEngine;

public class EffectEncumber: Effect {
    protected override void OnInitialize() {
        About.GetComponent<IMoves>().EncumberedTimer = Duration;
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<IMoves>() != null;
}
