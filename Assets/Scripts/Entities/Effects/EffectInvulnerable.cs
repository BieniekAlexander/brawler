using UnityEngine;

public class EffectInvulnerable: Effect {
    public override void Initialize(MonoBehaviour _target) {
        base.Initialize(_target);
        (Target as IDamageable).InvulnerableStack++;
        _duration = 120;
    }

    public override void Expire() {
        if (Target != null){
            (Target as IDamageable).InvulnerableStack--;
        }
    }

    public override bool CanEffect(MonoBehaviour _target) {
        return (_target is IDamageable);
    }
}    