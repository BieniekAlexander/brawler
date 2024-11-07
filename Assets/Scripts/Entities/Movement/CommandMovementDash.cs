using UnityEngine;
using UnityEngine.EventSystems;

public class CommandMovementDash: CommandMovement {
    [SerializeField] public float _boostSpeedBump = .2f;
    
    override protected void OnInitialize() {
        if (About != null && About.gameObject.GetComponent<Character>() is Character c) {
            c.Velocity = c.InputAimDirection.normalized * (Mathf.Max(c.BaseSpeed, c.Velocity.magnitude) + _boostSpeedBump);
            ;
        }
    }

    override public bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        return base.OnCollideWith(collidable, info);
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<Character>() != null;
}
