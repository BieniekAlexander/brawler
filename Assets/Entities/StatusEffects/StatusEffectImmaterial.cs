using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class StatusEffectImmaterial: EffectBase {
    public override void Initialize(Character _character) {
        base.Initialize(_character);
        _character.gameObject.layer = LayerMask.NameToLayer("Immaterial");
    }

    public override void Tick() {;}

    public override void Expire() {
        if (target != null){
            target.gameObject.layer = LayerMask.NameToLayer("Characters");
        }
    }
}
