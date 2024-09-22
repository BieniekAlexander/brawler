using UnityEngine;

public class CommandMovementJump : CommandMovement
{
    private float gravity = .5f;

    // TODO I have no idea if this implementation will work
    public override void Initialize(IMoves mover, Transform target) {
        base.Initialize(mover, target);
        Velocity = (60f/duration)*Time.fixedDeltaTime*Path;
        Velocity = new Vector3(Velocity.x, gravity*(duration-2)*Time.fixedDeltaTime/2, Velocity.z);
    }

    public override void FixedUpdate() {
        Velocity -= new Vector3(0f, gravity * Time.deltaTime, 0f);
        base.FixedUpdate();
    }
}
