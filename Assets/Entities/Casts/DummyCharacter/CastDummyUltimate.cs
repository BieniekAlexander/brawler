using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastDummyUltimate : CastBase
{
    private void Start() {
        Caster.castContainers[(int)CastId.Ability4].charges = 5;
    }
}
