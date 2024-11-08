using UnityEngine;

public abstract class CommandMovement : Effect {
    override public bool AppliesTo(GameObject go) => go.GetComponent<IMoves>() != null;
    protected IMoves Mover;

    protected override void OnInitialize() {
        base.OnInitialize();
        Mover = About.gameObject.GetComponent<IMoves>();
        
        if (Mover is Character ch) {
            ch.CommandMovement = this;
        }
    }

    public virtual Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return _currentRotation;
    }

    virtual public bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        // default CommandMovement behavior - same as Running
        if (collidable is Character otherCharacter) {
            if (info!=null && MovementUtils.inXZ(otherCharacter.Velocity).sqrMagnitude < MovementUtils.inXZ(Mover.Velocity).sqrMagnitude) {
                MovementUtils.Push(Mover, otherCharacter, info.Normal);
                return true;
            }
            else {
                return false;
            }
        } else if (collidable is StageTerrain terrain) {
            Mover.Velocity = MovementUtils.GetBounce(Mover.Velocity, info.Normal);
            return true;
        } else {
            return false;
        }
    }
}
