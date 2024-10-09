using UnityEngine;

public class CommandMovementLock : CommandMovement {
    [SerializeField] float Offset;
    private IMoves Lock;

    new public void Initialize(IMoves mover, Transform _destination) {
        base.Initialize(mover, _destination);
        Lock = _destination.gameObject.GetComponent<IMoves>();
    }

    public override void FixedUpdate() {
        Mover.Velocity = Lock.Velocity;
        base.FixedUpdate();
    }

    public override Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return Quaternion.LookRotation(Lock.Transform.position-_currentPosition, Vector3.up);
    }
}
