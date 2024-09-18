using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.TextCore.Text;
using System.Runtime.InteropServices.WindowsRuntime;


/// <summary>
/// Object that represents a Hitbox, which collides with entities in the game to heal, damage, afflict, etc.
/// NOTE: I can't get the collider properties to comport with its visualization for the life of me when 
///     I update its properties from default values, so any changes to the size will only be to transform.localScale
/// </summary>
[Serializable]
public class Grab : Hit {
    enum CoordinateSystem { Polar, Cartesian };

    /* Command Movement */
    [SerializeField] public CommandMovementBase commandMovementPrefab;

    /* Hits */
    [SerializeField] List<HitEvent> hitEvents;

    public override void HandleHitCollisions() {
        List<Collider> otherColliders = GetOverlappingColliders(HitBox).ToList();

        foreach (Collider otherCollider in GetOverlappingColliders(HitBox)) {
            // identify the character that was hit
            GameObject go = otherCollider.gameObject;
            Character character = go.GetComponent<Character>();
            if (character == null) { // the case that something else of the c
                character = go.GetComponentInParent<Character>();
            }

            if (character is not null && !collisionIds.Contains(character)) {
                collisionIds.Add(character); // this hitbox already hit this character - skip them

                if (
                    (Origin==character.transform && !HitsFriendlies)
                    || (Origin!=character.transform && !HitsEnemies)
                    ) {
                    continue;
                }

                Shield shield = character.GetComponentInChildren<Shield>();

                if ( // hitting shield
                    shield!=null
                    && shield.isActiveAndEnabled
                    && GetClosestGameObject(gameObject, character.gameObject, shield.gameObject)==shield.gameObject
                    && HitTier <= (int)shield.ShieldLevel
                ) {
                    ;   // TODO implement grab techs
                } else { // grabbing player
                    // TODO rename these vars - they're very mixed up
                    CommandMovementLock z = (CommandMovementLock) commandMovementPrefab;
                    CommandMovementLock commandMovementLock = Instantiate(z);
                    commandMovementLock.Initialize(character,
                        Vector2.zero, // TODO what are these zeroes? Are they okay?
                        Vector2.zero,
                        Origin,
                        z.range*Vector3.forward,
                        z.duration);

                    character.SetCommandMovement(commandMovementLock);
                }
            }
        }

    }
}
