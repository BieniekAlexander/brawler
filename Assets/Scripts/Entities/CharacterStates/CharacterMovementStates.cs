using UnityEngine;

public static class MovementUtils {
    public static Vector3 inXZ(Vector3 vector) {
        return Vector3.Scale(vector, new Vector3(1, 0, 1));
    }

    public static Vector3 setXZ(Vector3 vector, Vector3 change) {
        return inY(vector) + inXZ(change);
    }

    public static Vector3 inY(Vector3 vector) {
        return Vector3.Scale(vector, Vector3.up);
    }

    public static Vector3 setY(Vector3 vector, Vector3 change) {
        return inY(change) + inXZ(vector);
    }

    public static Vector3 ChangeMagnitude(Vector3 vector, float changeInMagnitude, bool clampToZero = true) {
        if ((vector.magnitude+changeInMagnitude)<0) {
            changeInMagnitude=-vector.magnitude;
        }

        return ClampMagnitude(
                vector.normalized * (vector.magnitude + changeInMagnitude),
                clampToZero ? 0 : Mathf.NegativeInfinity,
                Mathf.Infinity
            );
    }

    public static Vector3 ClampMagnitude(Vector3 v, float min, float max) {
        double sm = v.sqrMagnitude;
        if (sm > (double)max * (double)max) return v.normalized * max;
        else if (sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }
}

public class CharacterStateIdle : CharacterState {
    private float _acceleration = 50f;

    public CharacterStateIdle(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.CommandMovement != null) {
            return Factory.CommandMovement();
        } else if (Character.InputDash) {
            return Factory.Dashing();
        } else if (!Mathf.Approximately(MovementUtils.inXZ(Character.Velocity).magnitude, 0f)) {
            return Factory.Walking();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }

    public override void ExitState() {
    }

    public override void FixedUpdateState() {
        // TODO I guess it can be assumed that velocity.magnitude is zero here?
        Character.Velocity = MovementUtils.setXZ(Character.Velocity, Character.MoveDirection*(_acceleration*Time.deltaTime));
    }

    public override void InitializeSubState() {
        if (Character.BusyTimer>0) {
            SetSubState(Factory.Busy());
        } else if (Character.InputBlocking) {
            SetSubState(Factory.Blocking());
        } else if (Character.InputShielding) {
            SetSubState(Factory.Shielding());
        } else {
            SetSubState(Factory.Ready());
        }
    }

    /// <summary>
    /// If I'm stationary and inside another character, push myself out
    /// </summary>
    /// <param name="collidable"></param>
    /// <param name="info"></param>
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is Character otherCharacter) {
            Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
            Character.Transform.position += MovementUtils.inXZ(decollisionVector);
        } else if (collidable is StageTerrain terrain) {
            Vector3 mirror = info.Normal;
            Vector3 bounceDirection = (Character.Velocity-2*Vector3.Project(Character.Velocity, mirror)).normalized;
            Character.Velocity = MovementUtils.setXZ(Character.Velocity, bounceDirection*Character.Velocity.magnitude);
        }
    }
}

public class CharacterStateWalking : CharacterState {
    private float _acceleration = 50f;

    public CharacterStateWalking(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.CommandMovement != null) {
            return Factory.CommandMovement();
        } else if (Character.InputDash) {
            return Factory.Dashing();
        } else if (Mathf.Approximately(MovementUtils.inXZ(Character.Velocity).magnitude, 0f)) {
            return Factory.Idle();
        } else if (MovementUtils.inXZ(Character.Velocity).magnitude>Character.BaseSpeed+.1f) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }

    public override void ExitState() {
    }

    public override void FixedUpdateState() {
        // TODO what if I'm in the air? Air control?
        Vector3 horizontalVelocity = MovementUtils.inXZ(Character.Velocity);
        float dSpeed = Mathf.Clamp(
            Character.BaseSpeed - Vector3.Dot(horizontalVelocity, Character.MoveDirection),
            0, _acceleration*Time.deltaTime
        );

        Vector3 newVelocity = Vector3.ClampMagnitude(
            horizontalVelocity + dSpeed*(
                (Character.MoveDirection == Vector3.zero)
                ? (Character.InputRunning ? Vector3.zero : (-horizontalVelocity.normalized))
                : Character.MoveDirection
            ),
            Mathf.Max(Character.BaseSpeed, horizontalVelocity.magnitude)
        );
        Character.Velocity = MovementUtils.setXZ(
            Character.Velocity,
            (Vector3.Dot(horizontalVelocity, newVelocity)<0) ? Vector3.zero : newVelocity
        );
    }

    public override void InitializeSubState() {
        if (Character.BusyTimer>0) {
            SetSubState(Factory.Busy());
        } else if (Character.InputShielding) {
            SetSubState(Factory.Shielding());
        } else if (Character.InputBlocking) {
            SetSubState(Factory.Blocking());
        } else {
            SetSubState(Factory.Ready());
        }
    }

    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain terrain && info!=null) {
            Vector3 mirror = info.Normal;
            Vector3 bounceDirection = (Character.Velocity-2*Vector3.Project(Character.Velocity, mirror)).normalized;
            Character.Velocity = MovementUtils.setXZ(
                Character.Velocity,
                MovementUtils.inXZ(bounceDirection*Character.Velocity.magnitude)
            );
        }
    }
}


public class CharacterStateRunning : CharacterState {
    private float _rotationalSpeed = 120f*Mathf.Deg2Rad; // how quickly the character can rotate velocity
    private float _acceleration = 10f;

    public CharacterStateRunning(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.CommandMovement != null) {
            return Factory.CommandMovement();
        } else if (Character.InputDash) {
            return Factory.Dashing();
        } else if (MovementUtils.inXZ(Character.Velocity).magnitude<=7.51f) {
            return Factory.Walking();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }

    public override void ExitState() {
    }

    public override void FixedUpdateState() {
        Vector3 horizontalVelocity = MovementUtils.inXZ(Character.Velocity);
        float speed = (Character.InputRunning)
            ? horizontalVelocity.magnitude
            : horizontalVelocity.magnitude - _acceleration*Time.deltaTime;

        Character.Velocity = MovementUtils.setXZ(
            Character.Velocity,
            (Character.MoveDirection!=Vector3.zero)
            ? Vector3.RotateTowards(
                horizontalVelocity.normalized,
                Character.MoveDirection,
                _rotationalSpeed*Time.deltaTime, // rotate at speed according to whether we're running TODO tune rotation scaling
                0) * speed
            : horizontalVelocity.normalized * speed
        );
    }

    public override void InitializeSubState() {
        if (Character.BusyTimer>0) {
            SetSubState(Factory.Busy());
        } else if (Character.InputBlocking) {
            SetSubState(Factory.Blocking());
        } else if (Character.InputShielding) {
            SetSubState(Factory.Shielding());
        } else {
            SetSubState(Factory.Ready());
        }
    }

    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is Character otherCharacter) {
            if (info!=null && MovementUtils.inXZ(otherCharacter.Velocity).sqrMagnitude < MovementUtils.inXZ(Character.Velocity).sqrMagnitude) {
                // TODO I haven't tested this on moving targets yet, so I haven't tested the second term
                // TODO figure out this implementation and calibrate the deceleration more - the behavior is very confusing right now
                // TODO hardcoding the 5f/20 because that used to be some stupid calculationfrom dash decay, which was constant
                Vector3 dvNormal = Vector3.Project(MovementUtils.inXZ(Character.Velocity)-MovementUtils.inXZ(otherCharacter.Velocity), info.Normal)*(-(5f/20))*Time.deltaTime*30;
                dvNormal.y = 0f;
                Character.Velocity-=dvNormal;
                otherCharacter.Velocity=MovementUtils.inXZ(Character.Velocity)+dvNormal;
            }
        } else if (collidable is StageTerrain terrain) {
            //Vector3 mirror = new Vector3(info.Normal.x, 0, info.Normal.z); // TODO maybe restore?
            Vector3 mirror = info.Normal;
            Vector3 bounceDirection = (Character.Velocity-2*Vector3.Project(Character.Velocity, mirror)).normalized;
            Character.Velocity = MovementUtils.setXZ(Character.Velocity, bounceDirection*Character.Velocity.magnitude);
        }
    }
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
        if (Character.CommandMovement != null) { // do I want this to be possible while dashing? probably, at least for getting grabbed
            return Factory.CommandMovement();
        } else if (_boostTimer == 0) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        // TODO should I have a boost cooldown? I'm not sure
        // chargeCooldown = chargeCooldownMax;
        base.EnterState();
        _boostTimer = _boostMaxDuration;
        Character.Charges--;

        _boostSpeedEnd = Mathf.Max(MovementUtils.inXZ(Character.Velocity).magnitude, Character.BaseSpeed)+_boostSpeedBump;
        float boostSpeedStart = _boostSpeedEnd + 2*_boostSpeedBump;
        boostDecay = (_boostSpeedEnd-boostSpeedStart)/_boostMaxDuration;

        Vector3 v = Character.InputAimDirection;
        Character.Velocity = new Vector3(v.x, 0, v.z).normalized*boostSpeedStart;
        // TODO I think I'm losing the vertical velocity implementation here

    }

    public override void ExitState() {
        Character.Velocity = Character.Velocity.normalized * _boostSpeedEnd;
    }

    public override void FixedUpdateState() {
        // write stuff that happens during each frame of game logic in this state
        Vector3 horizontalVelocity = MovementUtils.inXZ(Character.Velocity);
        Character.Velocity = horizontalVelocity.normalized * (horizontalVelocity.magnitude+boostDecay);
        _boostTimer--;
    }

    public override void InitializeSubState() {
        SetSubState(Factory.Ready());
    }

    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is Character otherCharacter) {
            if (info != null && MovementUtils.inXZ(otherCharacter.Velocity).sqrMagnitude < MovementUtils.inXZ(Character.Velocity).sqrMagnitude) {
                // TODO I haven't tested this on moving targets yet, so I haven't tested the second term
                // TODO figure out this implementation and calibrate the deceleration more - the behavior is very confusing right now
                // TODO hardcoding the 5f/20 because that used to be some stupid calculationfrom dash decay, which was constant
                Vector3 dvNormal = Vector3.Project(MovementUtils.inXZ(Character.Velocity)-MovementUtils.inXZ(otherCharacter.Velocity), info.Normal)*(-(5f/20))*Time.deltaTime*30;
                dvNormal.y = 0f;
                Character.Velocity-=dvNormal;
                otherCharacter.Velocity=MovementUtils.inXZ(Character.Velocity)+dvNormal;
            }
        } else if (info != null && collidable is StageTerrain terrain) {
            // TODO sometimes this collision is breaking because info is null?
            Vector3 mirror = info.Normal;
            Vector3 bounceDirection = (Character.Velocity-2*Vector3.Project(Character.Velocity, mirror)).normalized;
            Character.Velocity = MovementUtils.inY(Character.Velocity) + MovementUtils.inXZ(bounceDirection*Character.Velocity.magnitude);
        }
    }
}

public class CharacterStateCommandMovement : CharacterState {
    public CharacterStateCommandMovement(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.CommandMovement == null) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }

    public override void ExitState() { }
    public override void FixedUpdateState() { }

    public override void InitializeSubState() {
        // override any sort of inputs - I want this state to generally be locked in
        SetSubState(Factory.Ready());
    }

    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is Character otherCharacter) {
            if (MovementUtils.inXZ(otherCharacter.Velocity).sqrMagnitude < MovementUtils.inXZ(Character.Velocity).sqrMagnitude) {
                // TODO I haven't tested this on moving targets yet, so I haven't tested the second term
                // TODO figure out this implementation and calibrate the deceleration more - the behavior is very confusing right now
                // TODO hardcoding the 5f/20 because that used to be some stupid calculationfrom dash decay, which was constant
                Vector3 dvNormal = Vector3.Project(MovementUtils.inXZ(Character.Velocity)-MovementUtils.inXZ(otherCharacter.Velocity), info.Normal)*(-(5f/20))*Time.deltaTime*30;
                dvNormal.y = 0f;
                Character.Velocity-=dvNormal;
                otherCharacter.Velocity=MovementUtils.inXZ(Character.Velocity)+dvNormal;
            }
        } else if (collidable is StageTerrain terrain) {
            //Vector3 mirror = new Vector3(info.Normal.x, 0, info.Normal.z); // TODO maybe restore?
            Vector3 mirror = info.Normal;
            Vector3 bounceDirection = (Character.Velocity-2*Vector3.Project(Character.Velocity, mirror)).normalized;
            Character.Velocity = MovementUtils.inY(Character.Velocity) + MovementUtils.inXZ(bounceDirection*Character.Velocity.magnitude);
        }
    }
}