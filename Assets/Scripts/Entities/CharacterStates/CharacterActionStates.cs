using System.Linq;
using UnityEngine;

public static class DefenseUtils {

}

public class CharacterStateReady : CharacterState {
    public CharacterStateReady(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    public override CharacterState CheckGetNewState() {
        // TODO make sure that you can't shield if you're dashing, in disadvantage, etc.
        if (Character.BusyMutex.Busy) {
            return Factory.Busy();
        } else if (Character.InputShielding && !Character.Parried) {
            return Factory.Shielding();
        } else if (Character.InputBlocking && !Character.Parried) {
            return Factory.Blocking();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
        Character.BusyMutex.Reset();
        Character.Shield.gameObject.SetActive(false);
    }

    public override void FixedUpdateState() {
        // disable the parry flag only when the character has been moved to ready and they're no longer inputting a block/shield
        if (!Character.InputShielding && !Character.InputBlocking && Character.Parried) {
            Character.Parried = false;
        }
    }

    public override void ExitState(){}
    public override void InitializeSubState(){}
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}

public class CharacterStateBusy : CharacterState {
    private readonly CharacterState[] _validExitParentStates;

    public CharacterStateBusy(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
        _validExitParentStates = new CharacterState[] {
            Factory.Idle(),
            Factory.Walking(),
            Factory.Running()
        };
    }

    public override CharacterState CheckGetNewState() {
        // TODO make sure that you can't shield if you're dashing, in disadvantage, etc.
        if (Character.BusyMutex.Busy && _validExitParentStates.Contains(_superState)) {
            return Factory.Ready();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }
    
    public override void FixedUpdateState() {
    }

    public override void ExitState() {
    }

    public override void InitializeSubState() { }
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info)  => false;
}

public class CharacterStateBlocking : CharacterState {
    private float _maxAcceleration = 1f;
    private int _exposedDuration = 15;

    public CharacterStateBlocking(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.Parried) {
            Character.Shield.gameObject.SetActive(false);
            return Factory.Ready();
        } else if (Character.InputShielding) {
            return Factory.Shielding();
        } else if (Character.InputBlocking) {
            return null;
        } else {
            Character.BusyMutex.Lock(_exposedDuration, false, false, 180f);
            return Factory.Busy();
        }
    }

    public override void EnterState() {
        base.EnterState();
        Character.Shield.gameObject.SetActive(true);
        Character.Shield.ShieldTier = ShieldTier.Blocking;
    }

    public override void ExitState() {
        if (!Character.Parried){
            Character.BusyMutex.Lock(_exposedDuration, false, false, 180f);
        }

        Character.Shield.gameObject.SetActive(false);
    }

    public override void FixedUpdateState() {
        // TODO slow down movement if the player is looking (and therefore blocking)
        // in the direction of movement, scaled by the norm or whatever
        if (_superState is CharacterStateRunning) {
            float dSpeed = Mathf.Max(
            Vector3.Dot(
                MovementUtils.inXZ(Character.Velocity).normalized,
                Character.InputAimDirection
            ),
            0
        ) * _maxAcceleration;

            Character.Velocity =
                MovementUtils.ClampMagnitude(
                    MovementUtils.ChangeMagnitude(
                        MovementUtils.inXZ(Character.Velocity),
                        -dSpeed*Time.deltaTime
                    ),
                    Character.BaseSpeed,
                    Mathf.Infinity
            );
        }
    }

    public override void InitializeSubState(){}
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}


public class CharacterStateShielding : CharacterState {
    private float _maxAcceleration = 3f;
    private float _rotationalSpeed = 300f*Mathf.Deg2Rad;
    private int _exposedDuration = 15;

    public CharacterStateShielding(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.Parried) {
            Character.Shield.gameObject.SetActive(false);
            return Factory.Ready();
        } else if (Character.InputShielding) {
            return null;
        } else if (Character.InputBlocking) {
            return Factory.Blocking();
        } else {
            Character.BusyMutex.Lock(_exposedDuration, false, false, 180f);
            return Factory.Busy();
        }
    }

    public override void EnterState() {
        base.EnterState();
        Character.Shield.gameObject.SetActive(true);
        Character.Shield.ShieldTier = ShieldTier.Shielding;
    }

    public override void ExitState() {
        if (!Character.Parried){
            Character.BusyMutex.Lock(_exposedDuration, false, false, 180f);
        }

        Character.Shield.gameObject.SetActive(false);
    }

    public override void FixedUpdateState() {
        // TODO return to this implementation because I don't know how to make it more elegant,
        // but the goal is:
        // - shielding towards the direction of movement slows you down, scaled up as direction is more parallel
        // - if you're running, shielding towards the direction of movement should rotate you (but not too much the same dir?)
        // - the faster you move, the more finnicky it should be
        // - :) https://www.youtube.com/watch?v=v3zT3Z5apaM
        Vector3 shieldDirection = Character.InputAimDirection;
        float directionDot = Mathf.Max(Vector3.Dot(MovementUtils.inXZ(Character.Velocity).normalized, shieldDirection), 0);
        float acceleration = _maxAcceleration*directionDot*Time.deltaTime;
        Vector3 direction = Vector3.RotateTowards(Character.Velocity, -shieldDirection, 90f*Mathf.Deg2Rad*directionDot, 0);

        if (Character.Running) {
            Character.Velocity = Vector3.RotateTowards(
                MovementUtils.inXZ(Character.Velocity),
                direction,
                _rotationalSpeed*Time.deltaTime, // rotate at speed according to whether we're running TODO tune rotation scaling
                0
            );
        }
        Character.Velocity = MovementUtils.ClampMagnitude(
            MovementUtils.ChangeMagnitude(MovementUtils.inXZ(Character.Velocity), -acceleration),
            Character.BaseSpeed,
            Mathf.Infinity
        );
    }

    public override void InitializeSubState(){}
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}
