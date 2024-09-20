using UnityEngine;

public class StatusEffectReflect: EffectBase {
    public override void Initialize(Character _character) {
        base.Initialize(_character);
        _character.Reflects = true; // TODO I realize that this implementation won't allow for stacking stuns, so I have to come back to this
    }

    public override void Expire() {
        if (target != null) {
            target.Reflects = false;
        }
    }
}
