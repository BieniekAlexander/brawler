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
            Character.WalkSpeedMax *= slowFactor;
        }
    }

    public override void Expire() {
        if (Character != null) {
            Character.WalkSpeedMax /= slowFactor;
        }
    }
}
