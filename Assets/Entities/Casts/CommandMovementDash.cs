using UnityEngine;

public class CommandMovementDash: CommandMovementBase {
    [SerializeField] public float speed;

    public override void Initialize(Vector3 _destination, Vector3 initialPosition) {
        base.Initialize(_destination, initialPosition);
        velocity = path.normalized*speed*Time.deltaTime;
        duration = Mathf.FloorToInt((path.magnitude/speed)/Time.fixedDeltaTime);
        Debug.Log(velocity);
        Debug.Log(duration);
    }
}
