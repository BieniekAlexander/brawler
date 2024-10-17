using System;
using UnityEngine;

public class EffectEncumbered: Castable {
    protected override void OnInitialize() {
        About.GetComponent<IMoves>().EncumberedStack++;
    }

    protected override void OnExpire() {
        if (About != null) {
            About.GetComponent<IMoves>().EncumberedStack--;
        }
    }

    public override bool AppliesTo(MonoBehaviour _target) {
        return _target is IMoves;
    }
}
