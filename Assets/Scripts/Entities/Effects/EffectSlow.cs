using System;
using UnityEngine;

public class EffectSlow: Cast {
    [SerializeField] public float slowFactor = .7f;

    protected override void OnInitialize() {
        About.GetComponent<IMoves>().BaseSpeed *= slowFactor;
    }

    protected override void OnDestruction() {
        if (About != null) {
            About.GetComponent<IMoves>().BaseSpeed /= slowFactor;
        }
    }

    public override bool AppliesTo(GameObject go) => go.GetComponent<IMoves>() != null;
}
