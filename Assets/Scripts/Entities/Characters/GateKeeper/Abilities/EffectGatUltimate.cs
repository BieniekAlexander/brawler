using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectGatUltimate : Effect {
   override protected void OnInitialize() {
        Character character = About.gameObject.GetComponent<Character>();
        character.CastContainers[(int)CastId.Ability4].charges += 4;
    }

    override protected void OnDestruction() {
        if (About.gameObject.GetComponent<Character>() is Character character) {
            character.CastContainers[(int)CastId.Ability4].charges = Math.Min(character.CastContainers[(int)CastId.Ultimate].charges, 1);
        }
    }

    public virtual bool AppliesTo(GameObject go) => go.GetComponent<Character>();
}
