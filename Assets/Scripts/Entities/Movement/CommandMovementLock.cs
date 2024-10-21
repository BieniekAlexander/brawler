using UnityEngine;

public class CommandMovementLock : CommandMovement {
    private IMoves Lock;

    protected override void OnInitialize() {
        base.OnInitialize();
        Lock = Target.GetComponent<IMoves>();
    }

    protected override void Tick() {
        if (Mover != null && Lock != null) {
            Mover.Velocity = Lock.Velocity;
        }
    }

    public override Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return Quaternion.LookRotation(Lock.Transform.position-_currentPosition, Vector3.up);
    }
}
