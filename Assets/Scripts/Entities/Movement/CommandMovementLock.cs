using UnityEngine;

public class CommandMovementLock : CommandMovement {
    private IMoves Effected;
    private IMoves Lock;

    protected override void OnInitialize() {
        Effected = About.GetComponent<IMoves>();
        Lock = Target.GetComponent<IMoves>();
    }

    protected override void Tick() {
        if (Effected != null && Lock != null) {
            Effected.Velocity = Lock.Velocity;
        }
    }

    public override Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return Quaternion.LookRotation(Lock.Transform.position-_currentPosition, Vector3.up);
    }
}
