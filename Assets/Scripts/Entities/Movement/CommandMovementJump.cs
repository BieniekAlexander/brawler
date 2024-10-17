using UnityEngine;

public class CommandMovementJump : CommandMovement
{
    [SerializeField] private float JumpVelocity;

    protected override void OnInitialize() {
        IMoves mover = About.gameObject.GetComponent<IMoves>();
        mover.Velocity += Vector3.up * JumpVelocity;
    }

    // TODO what do I do about air control?
}
