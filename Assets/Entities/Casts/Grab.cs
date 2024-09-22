using System;
using System.Buffers;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public class Grab : Hit {
    /* Command Movement */
    [SerializeField] public CommandMovement commandMovementPrefab;

    /* Cast, to be triggered on grab */
    [SerializeField] Castable TriggerCastPrefab;

    public virtual void OnCollideWith(ICollidable other) {
        // identify the character that was hit
        if (other.GetCollider().transform.GetComponent<Character>() is Character Target) {
            if (
                (Caster==(Target as ICasts) && !HitsFriendlies)
                || (Caster!=(Target as ICasts) && !HitsEnemies)
            ) {
                return;
            } else {
                CommandMovementLock cmLock = (CommandMovementLock)commandMovementPrefab;
                CommandMovementLock commandMovementLock = Instantiate(cmLock);
                Target.SetCommandMovement(commandMovementLock);
                commandMovementLock.Initialize(
                    Target,
                    (Caster as IMoves).GetTransform()
                );

                Castable c = Cast(
                    TriggerCastPrefab,
                    Caster,
                    Caster.GetOriginTransform(),
                    Target.GetTransform(),
                    false); // TODO clockwise
            }
        } else if (other.GetCollider().transform.GetComponent<Shield>() is Shield Shield) {
            Character Owner = Shield.GetComponentInParent<Character>();
            if (GetClosestGameObject(gameObject, Owner.gameObject, Owner.gameObject)==Shield.gameObject
                && (int)HitTier <= (int)Shield.ShieldTier)
                ; // TODO no-op - need to implement things like grab techs and such
                //OnCollideWith(Shield);
        }
    }
}
