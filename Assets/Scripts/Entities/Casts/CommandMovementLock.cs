using System;
using UnityEngine;

public class CommandMovementLock : CommandMovement {
    [SerializeField] float Offset;
    private Transform Lock;

    public void Initialize(IMoves mover, Transform _destination) {
        base.Initialize(mover, _destination);
        Lock = _destination;
    }

    public override Vector3 GetDPosition() {
        if (Lock.gameObject.CompareTag("Character")) {
            Character character = Lock.GetComponent<Character>();
            return character.HorizontalVelocity * Time.deltaTime;
        } else {
            throw new NotImplementedException("Don't know how to handle movement if not tied to char"); // TODO pending refactor, make movability interface
        }
    }

    public override Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return Quaternion.LookRotation(Lock.position-_currentPosition, Vector3.up);
    }
}
