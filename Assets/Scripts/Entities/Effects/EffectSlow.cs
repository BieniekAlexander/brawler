using System;
using UnityEngine;

public class EffectSlow: Castable {
    [SerializeField] public float slowFactor = .7f;

    protected override void OnInitialize() {
        About.GetComponent<IMoves>().BaseSpeed *= slowFactor;
    }

    protected override void OnDestruction() {
        if (About != null) {
            About.GetComponent<IMoves>().BaseSpeed /= slowFactor;
        }
    }

    public override bool AppliesTo(MonoBehaviour _target) {
        return _target is IMoves;
    }
}
