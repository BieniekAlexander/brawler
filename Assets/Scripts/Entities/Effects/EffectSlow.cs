using System;
using UnityEngine;

public class StatusEffectSlow: Effect {
    // TODO this implementation should probably be a lot more complicated,
    // considering IMovable and accelerations and such
    // I'll just leave it as a weak implementation rn
    private float slowFactor = .5f;
    private Character Character;

    public override void Initialize(MonoBehaviour _target) {
        if (_target.CompareTag("Character")) {
            base.Initialize(_target);
            Character.BaseSpeed *= slowFactor;
        } else {
            throw new NotImplementedException("haven't programmed slow ability to slow all movable things");
        }
    }

    public override void Expire() {
        if (Character != null) {
            Character.BaseSpeed /= slowFactor;
        }
    }

    public override bool CanEffect(MonoBehaviour _target) {
        return _target is IMoves;
    }
}
