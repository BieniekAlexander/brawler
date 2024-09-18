using System;
using UnityEngine;

public class CommandMovementLock : CommandMovementBase {
    private Transform Lock;
    private Vector3 Offset;

    public void Initialize(Character mover, Vector3 _destination, Vector3 initialPosition, Transform _lock, Vector3 _offset, int _duration) {
        base.Initialize(mover, _destination, initialPosition);
        Lock = _lock;
        Offset = _offset;
    }

    public override Vector3 GetDPosition() {
        if (Lock.gameObject.CompareTag("Character")) {
            Character character = Lock.GetComponent<Character>();
            return character.Velocity * Time.deltaTime;
        } else {
            throw new NotImplementedException("Don't know how to handle movement if not tied to char"); // TODO pending refactor, make movability interface
        }
    }

    public override Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return Quaternion.LookRotation(Lock.position-_currentPosition, Vector3.up);
    }
}
