using UnityEngine;

public class EffectMaim: Effect {
    public override void Initialize(MonoBehaviour _target) {
        base.Initialize(_target);
        (Target as ICasts).MaimStack++;
    }

    public override void Expire() {
        if (Target != null) {
            (Target as ICasts).MaimStack--;
        }
    }

    public override bool CanEffect(MonoBehaviour _target) {
        return (_target is ICasts);
    }
}
