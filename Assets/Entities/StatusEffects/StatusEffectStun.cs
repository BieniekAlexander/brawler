using UnityEngine;

public class StatusEffectStun: EffectBase {
    private float slowFactor = .5f;

    public override void Initialize(Character _character) {
        base.Initialize(_character);
        _character.Stunned = true; // TODO I realize that this implementation won't allow for stacking stuns, so I have to come back to this
    }

    public override void Expire() {
        if (target != null) {
            target.Stunned = false;
        }
    }
}
