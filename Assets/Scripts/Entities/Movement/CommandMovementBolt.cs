using UnityEngine;
using UnityEngine.EventSystems;

public class CommandMovementBolt: CommandMovement {
    [SerializeField] public float Speed;
    private float initialSpeed;

    protected override void OnInitialize() {
        base.OnInitialize();
        Vector3 trajectory = (Target.position-About.position).normalized
            * Mathf.Min(
                (Target.position-Mover.Transform.position).magnitude,
                Speed*Duration
            );

        initialSpeed = Mathf.Max(Mover.Velocity.magnitude, Mover.BaseSpeed);
        Mover.Velocity = trajectory.normalized*Mathf.Max(initialSpeed, Speed);
        Duration = Mathf.FloorToInt(trajectory.magnitude/Speed);
    }

    protected override void OnDestruction() {
        Mover.Velocity = Vector3.ClampMagnitude(Mover.Velocity, initialSpeed);
    }
}
