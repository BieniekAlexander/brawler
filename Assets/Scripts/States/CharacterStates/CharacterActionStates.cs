using UnityEngine;

public class CharacterStateReady : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.ACTION; }}

    public CharacterStateReady(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.Busy) {
            return Factory.Busy();
        } else if (Character.InputBlocking && !Character.Parried) {
            return Factory.Blocking();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }

    protected override void FixedUpdateState() {}
    protected override void ExitState(){}
    protected override void InitializeSubState() {
        SetSubState(Factory.Walking());
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}

public class CharacterStateBusy : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.ACTION; }}

    public CharacterStateBusy(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        // TODO make sure that you can't shield if you're dashing, in disadvantage, etc.
        if (!Character.Busy) {
            return Factory.Ready();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }
    
    protected override void FixedUpdateState() {}
    protected override void ExitState() {}
    protected override void InitializeSubState(){
        SetSubState(Factory.Sliding());
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info)  => false;
}

public class CharacterStateBlocking : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.ACTION; }}
    private float _maxAcceleration = .002f;
    private float _rotationalSpeed = 5f*Mathf.Deg2Rad;
    private int _exposedDuration = 15;

    public CharacterStateBlocking(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.InputBlocking) {
            return null;
        } else {
            Character.UnsetBusy();

            if (Character.Parried) {
                Character.Parried = false;
                return Factory.Ready();   
            } else {
                Character.SetBusy(_exposedDuration, false, false, 180f);
                return Factory.Busy();
            }
        }
    }

    public override void EnterState() {
        base.EnterState();
        Character.Shield.gameObject.SetActive(true);
        Character.Parried = false;
        Character.SetBusy(false, false, 180f);
    }

    protected override void ExitState() {
        Character.Shield.gameObject.SetActive(false);
    }

    protected override void FixedUpdateState() {
        // TODO return to this implementation because I don't know how to make it more elegant,
        // but the goal is:
        // - shielding towards the direction of movement slows you down, scaled up as direction is more parallel
        // - if you're running, shielding towards the direction of movement should rotate you (but not too much the same dir?)
        // - the faster you move, the more finnicky it should be
        // - :) https://www.youtube.com/watch?v=v3zT3Z5apaM
        // Vector3 shieldDirection = Character.InputAimVector;
        // float directionDot = Mathf.Max(Vector3.Dot(MovementUtils.inXZ(Character.Velocity).normalized, shieldDirection), 0);
        
        // Character.Velocity = Vector3.RotateTowards(
        //     MovementUtils.inXZ(Character.Velocity),
        //     Vector3.RotateTowards(Character.Velocity, -shieldDirection, 90f*Mathf.Deg2Rad*directionDot, 0),
        //     _rotationalSpeed, // rotate at speed according to whether we're running TODO tune rotation scaling
        //     0
        // );

        // if (directionDot > .5f) {
        //     float acceleration = _maxAcceleration*directionDot;
        //     Character.Velocity = MovementUtils.ChangeMagnitude(MovementUtils.inXZ(Character.Velocity), -acceleration);
        // }
    }

    protected override void InitializeSubState() {
        SetSubState(Factory.Sliding());
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}
