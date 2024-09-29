using UnityEngine;

public class StatusEffectImmaterial: Effect {
    private int originalLayer;

    public override void Initialize(MonoBehaviour _target) {
        base.Initialize(_target);
        originalLayer = Target.gameObject.layer;
        Target.gameObject.layer = LayerMask.NameToLayer("Immaterial");
    }

    public override void Tick() {;}

    public override void Expire() {
        if (Target != null){
            Target.gameObject.layer = originalLayer;
        }
    }

    public override bool CanEffect(MonoBehaviour _target) {
        throw new System.NotImplementedException();
    }
}
