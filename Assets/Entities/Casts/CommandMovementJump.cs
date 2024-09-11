using UnityEngine;

public class CommandMovementJump : CommandMovementBase
{
    private float gravity = .5f;

    public override void Initialize(Character mover, Vector3 _destination, Vector3 initialPosition) {
        base.Initialize(mover, _destination, initialPosition);
        velocity = path*Time.fixedDeltaTime*(60f/duration);
        velocity.y = gravity*(duration-2)*Time.fixedDeltaTime/2;
    }

    public override void FixedUpdate() {
        velocity.y -= gravity * Time.deltaTime;
        base.FixedUpdate();
    }
}
