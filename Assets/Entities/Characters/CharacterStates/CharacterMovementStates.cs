using System.Runtime.InteropServices;
using UnityEngine;

public static class MovementUtils {
    public static Vector3 ChangeMagnitude(Vector3 vector, float changeInMagnitude) {
        return vector.normalized * (vector.magnitude + changeInMagnitude);
    }

    public static Vector3 ClampMagnitude(Vector3 v, float min, float max) {
        double sm = v.sqrMagnitude;
        if (sm > (double)max * (double)max) return v.normalized * max;
        else if (sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }
}

public class CharacterStateIdle : CharacterState {
    private float _acceleration = 20f;

    public CharacterStateIdle(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        if (Character.InputDash) {
            return Factory.Dashing();
        } else if (!Mathf.Approximately(Character.Velocity.magnitude, 0f)) {
            return Factory.Walking();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        // write stuff that happens when the state starts
    }

    public override void ExitState() {
    }

    public override void FixedUpdateState() {
        // TODO I guess it can be assumed that velocity.magnitude is zero here?
        Character.Velocity = Character.InputMoveDirection*(_acceleration*Time.deltaTime);
    }

    public override void InitializeSubState() {
        if (Character.InputShielding) {
            SetSubState(Factory.Shielding());
        } else if (Character.InputBlocking) {
            SetSubState(Factory.Blocking());
        } else {
            SetSubState(Factory.Exposed());
        }
    }

    /*public override void OnCollisionEnter(CharacterStateMachine _machine) {
        // write stuff 
    }*/
}

public class CharacterStateWalking : CharacterState {
    private float _acceleration = 20f;

    public CharacterStateWalking(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        if (Character.InputDash) {
            return Factory.Dashing();
        } else if (Mathf.Approximately(Character.Velocity.magnitude, 0f)) {
            return Factory.Idle();
        } else if (Character.Velocity.magnitude>Character.WalkSpeedMax+.1f) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        // write stuff that happens when the state starts
    }

    public override void ExitState() {
    }

    public override void FixedUpdateState() {
        // write stuff that happens during each frame of game logic in this state
        // this includes state-internal changes, state-transitions, and handling inputs to the state
        Vector3 horizontalVelocity = Vector3.Scale(Character.Velocity, new Vector3(1f, 0f, 1f));
        float dSpeed = Mathf.Clamp(
            Character.WalkSpeedMax - Vector3.Dot(horizontalVelocity, Character.InputMoveDirection),
            0, _acceleration*Time.deltaTime
        );

        Vector3 newVelocity = Vector3.ClampMagnitude(
            horizontalVelocity + dSpeed*(
                (Character.InputMoveDirection == Vector3.zero)
                ? (Character.InputRunning ? Vector3.zero : (-horizontalVelocity.normalized))
                : Character.InputMoveDirection
            ),
            Mathf.Max(Character.WalkSpeedMax, horizontalVelocity.magnitude)
        );

        Character.Velocity = (Vector3.Dot(horizontalVelocity, newVelocity)<0) ? Vector3.zero : newVelocity;
    }

    public override void InitializeSubState() {
        if (Character.InputShielding) {
            SetSubState(Factory.Shielding());
        } else if (Character.InputBlocking) {
            SetSubState(Factory.Blocking());
        } else {
            SetSubState(Factory.Exposed());
        }
    }

    /*public override void OnCollisionEnter(CharacterStateMachine _machine) {
        // write stuff 
    }*/
}


public class CharacterStateRunning : CharacterState {
    private float _rotationalSpeed = 120f*Mathf.Deg2Rad; // how quickly the character can rotate velocity
    private float _acceleration = 10f;

    public CharacterStateRunning(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        if (Character.InputDash) {
            return Factory.Dashing();
        } else if (Character.Velocity.magnitude<=7.51f) {
            return Factory.Walking();
        } else {
            return null;
        }
    }

    public override void EnterState() {}

    public override void ExitState() {
    }

    public override void FixedUpdateState() {
        Vector3 horizontalVelocity = Vector3.Scale(Character.Velocity, new Vector3(1f, 0f, 1f));
        float speed = (Character.InputRunning)
            ? horizontalVelocity.magnitude
            : horizontalVelocity.magnitude - _acceleration*Time.deltaTime;

        Character.Velocity = (Character.InputMoveDirection!=Vector3.zero)
            ? Character.Velocity = Vector3.RotateTowards(
                horizontalVelocity.normalized,
                Character.InputMoveDirection,
                _rotationalSpeed*Time.deltaTime, // rotate at speed according to whether we're running TODO tune rotation scaling
                0) * speed
            : Character.Velocity.normalized * speed;
    }

    public override void InitializeSubState() {
        SetSubState(Factory.Exposed());
    }

    /*public override void OnCollisionEnter(CharacterStateMachine _machine) {
        // write stuff 
    }*/
}

public class CharacterStateDashing : CharacterState {
    private int _boostTimer;
    private int _boostMaxDuration = 20;
    private float _boostSpeedBump = 2.5f;
    private float boostDecay;
    private float _boostSpeedEnd;

    public CharacterStateDashing(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (_boostTimer == 0) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        // TODO should I have a boost cooldown? I'm not sure
        // chargeCooldown = chargeCooldownMax;
        _boostTimer = _boostMaxDuration;
        Character.Charges--;

        _boostSpeedEnd = Mathf.Max(Character.Velocity.magnitude, Character.WalkSpeedMax)+_boostSpeedBump;
        float boostSpeedStart = _boostSpeedEnd + 2*_boostSpeedBump;
        boostDecay = (_boostSpeedEnd-boostSpeedStart)/_boostMaxDuration;

        Vector3 v = Character.GetLookDirection();
        Character.Velocity = new Vector3(v.x, 0, v.z).normalized*boostSpeedStart;
        // TODO I think I'm losing the vertical velocity implementation here

    }

    public override void ExitState() {
        Character.Velocity = Character.Velocity.normalized * _boostSpeedEnd;
    }

    public override void FixedUpdateState() {
        // write stuff that happens during each frame of game logic in this state
        Vector3 horizontalVelocity = Vector3.Scale(new Vector3(1f, 0f, 1f), Character.Velocity);
        Character.Velocity = horizontalVelocity.normalized * (horizontalVelocity.magnitude+boostDecay);
        _boostTimer--;
    }

    public override void InitializeSubState() {
        if (Character.InputShielding) {
            SetSubState(Factory.Shielding());
        } else if (Character.InputBlocking) {
            SetSubState(Factory.Blocking());
        } else {
            SetSubState(Factory.Exposed());
        }
    }

    /*public override void OnCollisionEnter(CharacterStateMachine _machine) {
        // write stuff 
    }*/
}