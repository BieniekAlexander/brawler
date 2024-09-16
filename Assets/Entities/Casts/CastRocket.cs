using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastRocket : CastBase
{
   override public bool UpdateCast(Vector3 worldPosition) {
        for (int i = 0; i < projectiles.Count; i++) {
            if (projectiles[i] != null) {
                projectiles[i].target.position = worldPosition;
            }
        }

        return true;
    }
}
