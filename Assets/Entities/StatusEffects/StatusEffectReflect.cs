using UnityEngine;

public class StatusEffectReflect: Effect {
    // TODO reimpliment this - ideally, I think the castable checks statuses of the Character
    // when it collides with them - how do I do that?
    /*
    public override void Initialize(Character _character) {
        base.Initialize(_character);
        _character.Reflects = true; // TODO I realize that this implementation won't allow for stacking stuns, so I have to come back to this
    }

    public override void Expire() {
        if (Target != null) {
            Target.Reflects = false;
        }
    }*/
}
