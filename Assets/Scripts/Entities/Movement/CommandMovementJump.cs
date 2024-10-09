using UnityEngine;

public class CommandMovementJump : CommandMovement
{
    [SerializeField] private float JumpVelocity;

    // TODO I have no idea if this implementation will work
    public override void Initialize(IMoves mover, Transform target) {
        base.Initialize(mover, target);
        Mover.Velocity += Vector3.up * JumpVelocity;
    }
}
