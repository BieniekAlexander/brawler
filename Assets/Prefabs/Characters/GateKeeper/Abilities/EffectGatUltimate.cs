using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectGatUltimate : Effect {
   override protected void OnInitialize() {
        base.OnInitialize();
        Character character = About.gameObject.GetComponent<Character>();
        // TODO fix later :)
        // character.CastMap[(int)CastType.Ability4].charges += 4;
    }

    override protected void OnDestruction() {
        if (About.gameObject.GetComponent<Character>() is Character character) {
            // TODO fix later :)
            // character.CastMap[(int)CastType.Ability4].charges = Math.Min(character.CastMap[(int)CastType.Ultimate].charges, 1);
        }
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<Character>();
}
