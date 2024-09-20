using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Grab : Hit {
    enum CoordinateSystem { Polar, Cartesian };

    /* Command Movement */
    [SerializeField] public CommandMovementBase commandMovementPrefab;

    /* Cast, to be triggered on grab */
    [SerializeField] CastBase TriggerCastPrefab;

    public override void HandleCollisions() {
        List<Collider> otherColliders = GetOverlappingColliders(Collider).ToList();

        foreach (Collider otherCollider in GetOverlappingColliders(Collider)) {
            // identify the character that was hit
            GameObject go = otherCollider.gameObject;
            Character target = go.GetComponent<Character>();
            if (target == null) { // the case that something else of the c
                target = go.GetComponentInParent<Character>();
            }

            if (target is not null && !TriggeredColliderIds.Contains(target)) {
                TriggeredColliderIds.Add(target); // this hitbox already hit this character - skip them

                if (
                    (Caster==target && !HitsFriendlies)
                    || (Caster!=target && !HitsEnemies)
                    ) {
                    continue;
                }

                Shield shield = target.GetComponentInChildren<Shield>();

                if ( // hitting shield
                    shield!=null
                    && shield.isActiveAndEnabled
                    && GetClosestGameObject(gameObject, target.gameObject, shield.gameObject)==shield.gameObject
                    && HitTier <= (int)shield.ShieldLevel
                ) {
                    ;   // TODO implement grab techs
                } else { // grabbing player
                    // TODO rename these vars - they're very mixed up
                    CommandMovementLock z = (CommandMovementLock) commandMovementPrefab;
                    CommandMovementLock commandMovementLock = Instantiate(z);
                    target.SetCommandMovement(commandMovementLock);
                    commandMovementLock.Initialize(target,
                        Vector2.zero, // TODO what are these zeroes? Are they okay?
                        Vector2.zero,
                        Origin,
                        z.range*Vector3.forward,
                        z.duration);
                    
                    CastBase cast = CastBase.Initiate(
                        TriggerCastPrefab,
                        Caster,
                        target.transform,
                        target.transform,
                        false); // TODO clockwise
                }
            }
        }

    }
}
