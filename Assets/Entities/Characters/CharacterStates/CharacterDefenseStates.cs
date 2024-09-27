using System.Runtime.InteropServices;
using UnityEngine;

public static class DefenseUtils {

}

public class CharacterStateReady : CharacterState {
    public CharacterStateReady(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        // TODO make sure that you can't shield if you're dashing, in disadvantage, etc.
        if (Character.InputShielding) {
            return Factory.Shielding();
        } else if (Character.InputBlocking) {
            return Factory.Blocking();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.Shield.gameObject.SetActive(false);
    }

    public override void ExitState(){}
    public override void FixedUpdateState(){}
    public override void InitializeSubState(){}
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info){}
}

public class CharacterStateExposed : CharacterState {
    private int _exposedDuration = 10;

    public CharacterStateExposed(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        // TODO make sure that you can't shield if you're dashing, in disadvantage, etc.
        if (Character.ExposedTimer==0) {
            return Factory.Ready();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.ExposedTimer = _exposedDuration;
    }
    
    public override void FixedUpdateState() {
        Character.ExposedTimer--;
    }

    public override void ExitState() { }
    public override void InitializeSubState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}

public class CharacterStateBlocking : CharacterState {
    private float _maxAcceleration = 10f;

    public CharacterStateBlocking(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        if (Character.InputShielding) {
            return Factory.Shielding();
        } else if (Character.InputBlocking) {
            return null;
        } else {
            return Factory.Exposed();
        }
    }

    public override void EnterState() {
        Character.Shield.gameObject.SetActive(true);
        Character.Shield.ShieldTier = ShieldTier.Blocking;
    }

    public override void ExitState() {
        Character.Shield.gameObject.SetActive(false);
    }

    public override void FixedUpdateState() {
        // TODO slow down movement if the player is looking (and therefore blocking)
        // in the direction of movement, scaled by the norm or whatever
        if (_superState is CharacterStateRunning) {
            float dSpeed = Mathf.Max(
            Vector3.Dot(
                Character.Velocity.normalized,
                Character.GetLookDirection()
            ),
            0
        ) * _maxAcceleration;

            Character.Velocity =
                MovementUtils.ClampMagnitude(
                    MovementUtils.ChangeMagnitude(
                        Character.Velocity,
                        -dSpeed*Time.deltaTime
                    ),
                    Character.WalkSpeedMax,
                    Mathf.Infinity
            );
        }
    }

    public override void InitializeSubState(){}
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info){}
}


public class CharacterStateShielding : CharacterState {
    private float _maxAcceleration = 29f;
    private float _rotationalSpeed = 300f*Mathf.Deg2Rad;

    public CharacterStateShielding(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        if (Character.InputShielding) {
            return null;
        } else if (Character.InputBlocking) {
            return Factory.Blocking();
        } else {
            return Factory.Exposed();
        }
    }

    public override void EnterState() {
        Character.Shield.gameObject.SetActive(true);
        Character.Shield.ShieldTier = ShieldTier.Shielding;
    }


    public override void FixedUpdateState() {
        // TODO return to this implementation because I don't know how to make it more elegant,
        // but the goal is:
        // - shielding towards the direction of movement slows you down, scaled up as direction is more parallel
        // - if you're running, shielding towards the direction of movement should rotate you (but not too much the same dir?)
        // - the faster you move, the more finnicky it should be
        // - :) https://www.youtube.com/watch?v=v3zT3Z5apaM
        Vector3 characterDirection = Character.Velocity.normalized;
        Vector3 shieldDirection = Character.GetLookDirection();
        float acceleration = _maxAcceleration*Mathf.Max(
            Vector3.Dot(
                characterDirection,
                shieldDirection
            ),
            0
        )*Time.deltaTime;

        if (Character.InputRunning) {
            Character.Velocity = Vector3.RotateTowards(
                Character.Velocity,
                -shieldDirection,
                _rotationalSpeed*Time.deltaTime, // rotate at speed according to whether we're running TODO tune rotation scaling
                0
            );
        }
        Character.Velocity = MovementUtils.ClampMagnitude(
            MovementUtils.ChangeMagnitude(Character.Velocity, -acceleration),
            0,
            Mathf.Infinity
        );
    }

    public override void ExitState(){
        Character.Shield.gameObject.SetActive(false);
    }

    public override void InitializeSubState(){}
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info){}
}
