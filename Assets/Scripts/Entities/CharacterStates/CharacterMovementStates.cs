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

    public static Vector3 GetBounce(Vector3 velocity, Vector3 normal) {
            Vector3 bounceDirection = (velocity-2*Vector3.Project(velocity, normal)).normalized;
            return MovementUtils.setXZ(velocity, bounceDirection*velocity.magnitude);
    }

    public static void Push(IMoves pushing, IMoves pushed, Vector3 normal) {
            Vector3 dvNormal = normal*Mathf.Max(pushing.BaseSpeed, pushed.BaseSpeed)*Time.deltaTime;
            pushing.Velocity += dvNormal;
            pushed.Velocity = pushing.Velocity-dvNormal;
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
        Character.Velocity = MovementUtils.setXZ(Character.Velocity, Character.MoveDirection*(_acceleration*Time.deltaTime));
    }

    public override void InitializeSubState() {
        if (Character.BusyMutex.Busy) {
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
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is Character otherCharacter) {
            Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
            Character.Transform.position += MovementUtils.inXZ(decollisionVector);
            return true;
        } else if (collidable is StageTerrain terrain) {
            Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
            return true;
        } else {
            return false;
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
                ? (Character.Running ? Vector3.zero : (-horizontalVelocity.normalized))
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
        if (Character.BusyMutex.Busy) {
            SetSubState(Factory.Busy());
        } else if (Character.InputShielding) {
            SetSubState(Factory.Shielding());
        } else if (Character.InputBlocking) {
            SetSubState(Factory.Blocking());
        } else {
            SetSubState(Factory.Ready());
        }
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain terrain && info!=null) {
            Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
            return true;
        } else {
            return false;
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
        } else if (MovementUtils.inXZ(Character.Velocity).magnitude<=7.51f) {
            return Factory.Walking();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }

    public override void ExitState() {}

    public override void FixedUpdateState() {
        Vector3 horizontalVelocity = MovementUtils.inXZ(Character.Velocity);
        float speed = (Character.Running)
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
        if (Character.BusyMutex.Busy) {
            SetSubState(Factory.Busy());
        } else if (Character.InputBlocking) {
            SetSubState(Factory.Blocking());
        } else if (Character.InputShielding) {
            SetSubState(Factory.Shielding());
        } else {
            SetSubState(Factory.Ready());
        }
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is Character otherCharacter) {
            if (info!=null && MovementUtils.inXZ(otherCharacter.Velocity).sqrMagnitude < MovementUtils.inXZ(Character.Velocity).sqrMagnitude) {
                MovementUtils.Push(Character, otherCharacter, info.Normal);
                return true;
            } else {
                return false;
            }
        } else if (collidable is StageTerrain terrain) {
            Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
            return true;
        } else {
            return false;
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

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        return Character.CommandMovement.OnCollideWith(collidable, info);
    }
}