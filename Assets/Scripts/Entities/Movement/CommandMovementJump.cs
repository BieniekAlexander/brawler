using UnityEngine;

public class CommandMovementJump : CommandMovement
{
    [SerializeField] private int JumpSquatDuration;
    [SerializeField] private float JumpVelocity;
    private int jumpSquatTimer;

    // TODO I have no idea if this implementation will work
    public override void Initialize(IMoves mover, Transform target) {
        base.Initialize(mover, target);
        jumpSquatTimer = JumpSquatDuration;
        Velocity = mover.Velocity;
    }

    public override void FixedUpdate() {
        if (--jumpSquatTimer==0) {
            Mover.Velocity += Vector3.up * JumpVelocity;
        }
        
        base.FixedUpdate();
    }
}
