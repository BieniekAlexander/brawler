using UnityEngine;

public abstract class CommandMovement : Cast {
    protected override void OnDestruction() {
        foreach (Cast activeCastable in CastableChlidren) {
            Destroy(activeCastable);
        }
    }

    public override bool AppliesTo(MonoBehaviour mono) {
        return mono is IMoves;
    }

    public virtual Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return _currentRotation;
    }
}
