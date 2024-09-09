using UnityEngine;

public class CommandMovementJump : CommandMovementBase
{
    private float gravity = .5f;

    public override void Initialize(Vector3 _destination, Vector3 initialPosition) {
        base.Initialize(_destination, initialPosition);
        velocity = path*Time.fixedDeltaTime*(60f/duration);
        velocity.y = gravity*(duration-2)*Time.fixedDeltaTime/2;
        Debug.Log(velocity);
        // TODO stupid hack where I decreasted the duration in the calculation to make sure the final position is lower than the initial position
        // fix this caculation, and otherwise make sure that they return to initial y position
    }

    public override void FixedUpdate() {
        base.FixedUpdate();
        velocity.y -= gravity * Time.deltaTime;
    }
}
