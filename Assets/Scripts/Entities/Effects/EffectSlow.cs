using System;
using UnityEngine;

public class EffectSlow: Effect {
    [SerializeField] public float slowFactor = .7f;

    public override void Initialize(MonoBehaviour _target) {
        base.Initialize(_target);
        (Target as IMoves).BaseSpeed *= slowFactor;
    }

    public override void Expire() {
        if (Target != null) {
            (Target as IMoves).BaseSpeed /= slowFactor;
        }
    }

    public override bool CanEffect(MonoBehaviour _target) {
        return _target is IMoves;
    }
}
