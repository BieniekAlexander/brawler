using System;
using UnityEngine;

[Serializable]
public class Grab : Trigger, ICollidable {
    /* Command Movement */
    [SerializeField] public CommandMovement commandMovementPrefab;

    /* Cast, to be triggered on grab */
    [SerializeField] public Cast OnGrabCast;
    [SerializeField] public bool HitsEnemies = true;
    [SerializeField] public bool HitsFriendlies = false;
    [SerializeField] HitTier HitTier;

    public override void OnCollideWith(ICollidable other, CollisionInfo info) {
        // TODO probably hardcode Caster not grabbing oneself

        if (other is IMoves OtherMover) {
            if (
                (Caster==OtherMover && !HitsFriendlies)
                || (Caster!=OtherMover && !HitsEnemies)
            ) {
                return;
            } else {
                CommandMovementLock cmLock = (CommandMovementLock)commandMovementPrefab;
                CommandMovementLock commandMovementLock = Instantiate(cmLock);
                OtherMover.SetCommandMovement(commandMovementLock);
                commandMovementLock.Initialize(
                    OtherMover,
                    (Caster as IMoves).GetTransform()
                );
            }

            if (other is Character GrabbedCharacter) {
                Cast.Initiate(
                    OnGrabCast,
                    Caster,
                    GrabbedCharacter.transform,
                    Caster.GetTargetTransform(),
                    true
                );
            } else if (other.GetCollider().transform.GetComponent<Shield>() is Shield Shield) {
                Character Owner = Shield.GetComponentInParent<Character>();
                if (GetClosestGameObject(gameObject, Owner.gameObject, Owner.gameObject)==Shield.gameObject
                    && (int)HitTier <= (int)Shield.ShieldTier)
                    ; // TODO no-op - need to implement things like grab techs and such
                      //OnCollideWith(Shield);
            }
        }
        // identify the character that was hit
        
    }
}