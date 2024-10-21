using UnityEngine;

public abstract class CommandMovement : Effect {
    override public bool AppliesTo(GameObject go) => go.GetComponent<IMoves>() != null;
    protected IMoves Mover;

    protected override void OnInitialize() {
        base.OnInitialize();
        Mover = About.gameObject.GetComponent<IMoves>();
        
        if (Mover is Character ch) {
            ch.CommandMovement = this;
        }
    }

    public virtual Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return _currentRotation;
    }
}
