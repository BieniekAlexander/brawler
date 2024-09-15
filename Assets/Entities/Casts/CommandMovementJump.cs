using UnityEngine;

public class CommandMovementJump : CommandMovementBase
{
    private float gravity = .5f;

    public override void Initialize(Character mover, Vector3 _destination, Vector3 initialPosition) {
        base.Initialize(mover, _destination, initialPosition);
        Velocity = Path*Time.fixedDeltaTime*(60f/duration);
        Velocity = new Vector3(Velocity.x, gravity*(duration-2)*Time.fixedDeltaTime/2, Velocity.z);
    }

    public override void FixedUpdate() {
        Velocity -= new Vector3(0f, gravity * Time.deltaTime, 0f);
        base.FixedUpdate();
    }
}
