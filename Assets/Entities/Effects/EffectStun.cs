using UnityEngine;

public class StatusEffectStun: Effect {
    public override void Initialize(MonoBehaviour _target) {
        base.Initialize(_target);
         // TODO do I need this class? Might I just use the Hit hitstun thing?
    }

    public override void Expire() {
        ;
    }

    public override bool CanEffect(MonoBehaviour _target) {
        throw new System.NotImplementedException();
    }
}
