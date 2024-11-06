using UnityEngine;
using UnityEngine.EventSystems;

public class CommandMovementDash: CommandMovement {
    private int chargeDuration = 5; // TODO temp implementation
    private float _boostSpeedBump = .125f;
    
    override protected void OnDestruction() {}

    override protected void Tick() {
        Character c = Mover as Character;

        if (chargeDuration--==0) {
            Mover.Velocity += c.InputAimDirection.normalized * _boostSpeedBump;
        } else if (chargeDuration<0) {
            if (c.InputCastId == (int)CastId.Dash) {
                Mover.Velocity = c.InputAimDirection.normalized * Mover.Velocity.magnitude;
            }
        }
    }

    override public bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        return base.OnCollideWith(collidable, info);
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<Character>() != null;
}
