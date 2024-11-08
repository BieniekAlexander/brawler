using UnityEngine;

public class CommandMovementLock : CommandMovement {
    private IMoves Lock;

    protected override void OnInitialize() {
        base.OnInitialize();
        Lock = Caster.GetAboutTransform().GetComponent<IMoves>();
        Mover.Transform.position = Lock.Transform.position + Lock.Transform.rotation*Vector3.forward*Range;
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
