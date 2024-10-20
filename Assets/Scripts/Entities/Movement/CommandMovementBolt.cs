using UnityEngine;
using UnityEngine.EventSystems;

public class CommandMovementBolt: CommandMovement {
    [SerializeField] public float Speed;
    private float initialSpeed;
    private IMoves Mover;

    protected override void OnInitialize() {
        Mover = About.GetComponent<IMoves>();
        Vector3 trajectory = (Target.position-About.position).normalized
            * Mathf.Min(
                (Target.position-Mover.Transform.position).magnitude,
                Speed*Duration*Time.fixedDeltaTime
            );

        initialSpeed = Mathf.Max(Mover.Velocity.magnitude, Mover.BaseSpeed);
        Mover.Velocity = trajectory.normalized*Mathf.Max(initialSpeed, Speed);
        Duration = Mathf.FloorToInt(trajectory.magnitude/Speed/Time.fixedDeltaTime);
    }

    protected override void OnDestruction() {
        Mover.Velocity = Vector3.ClampMagnitude(Mover.Velocity, initialSpeed);
    }
}
