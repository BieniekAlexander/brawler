using UnityEngine;

public class StatusEffectSlow: StatusEffectBase {
    private float slowFactor = .5f;

    public override void Initialize(Character _character) {
        base.Initialize(_character);
        target.WalkSpeedMax *= slowFactor;
    }

    public override void Expire() {
        if (target != null) {
            target.WalkSpeedMax /= slowFactor;
        }
    }
}
