using UnityEngine;

public abstract class CommandMovement : Cast {
    public override bool AppliesTo(GameObject go) => go.GetComponent<IMoves>() != null;

    public virtual Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return _currentRotation;
    }
}
