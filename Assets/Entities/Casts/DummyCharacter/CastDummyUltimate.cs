using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastDummyUltimate : CastBase
{
    private void Start() {
        Caster.CastContainers[(int)CastId.Ability4].charges = 5;
    }
    override public void UpdateCast(Vector3 worldPosition) {
        for (int i = 0; i < projectiles.Count; i++) {
            if (projectiles[i] != null) {
                projectiles[i].target.position = worldPosition;
            }
        }
    }
}
