using UnityEngine;

public abstract class CommandMovement : Castable {
    protected override void OnExpire() {
        foreach (Castable activeCastable in ActiveCastables) {
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
