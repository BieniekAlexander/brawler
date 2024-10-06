using UnityEngine;

public class EffectSilence: Effect {
    public override void Initialize(MonoBehaviour _target) {
        base.Initialize(_target);
        (Target as ICasts).SilenceStack++;
    }

    public override void Expire() {
        if (Target != null) {
            (Target as ICasts).SilenceStack--;
        }
    }

    public override bool CanEffect(MonoBehaviour _target) {
        return (_target is ICasts);
    }
}
