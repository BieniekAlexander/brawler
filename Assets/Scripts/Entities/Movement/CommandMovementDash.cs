using UnityEngine;
using UnityEngine.EventSystems;

public class CommandMovementDash: CommandMovement {
    private float initialSpeed;
    private float _boostSpeedBump = .1f;
    private float boostDecay;
    private float _boostSpeedEnd;

    protected override void OnInitialize() {
        base.OnInitialize();

        _boostSpeedEnd = Mathf.Max(MovementUtils.inXZ(Mover.Velocity).magnitude, Mover.BaseSpeed)+_boostSpeedBump;
        float boostSpeedStart = _boostSpeedEnd + 2*_boostSpeedBump;
        boostDecay = (_boostSpeedEnd-boostSpeedStart)/Duration;

        Vector3 v = (Mover as Character).InputAimDirection;
        Mover.Velocity = new Vector3(v.x, 0, v.z).normalized*boostSpeedStart;
    }

    override protected void Tick() {
        Vector3 horizontalVelocity = MovementUtils.inXZ(Mover.Velocity);
        Mover.Velocity = horizontalVelocity.normalized * (horizontalVelocity.magnitude+boostDecay);
    }

    override protected void OnDestruction() {
        Mover.Velocity = Mover.Velocity.normalized * _boostSpeedEnd;
    }

    override public bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (base.OnCollideWith(collidable, info)) {
            Destroy(gameObject);
            return true;
        } else {
            return false;
        }
    }

    override public bool AppliesTo(GameObject go) => go.GetComponent<Character>() != null;
}
