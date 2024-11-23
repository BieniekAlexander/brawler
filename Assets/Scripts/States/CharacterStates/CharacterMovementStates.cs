using UnityEngine;

public enum AimMovementOrientation {
    PARALLEL,
    PERPENDICULAR,
    ANTIPARALLEL
}

public enum RotationMovementOrientation {
    WITH,
    AGAINST
}

public static class MovementUtils {
    public static Vector3 toXZ(Vector2 vector) {
        return new Vector3(vector.x, 0f, vector.y);
    }

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

    public static Vector3 ChangeMagnitude(Vector3 vector, float changeInMagnitude, float clampMinimum=0f, float clampMaximum=Mathf.Infinity) {
        if ((vector.magnitude+changeInMagnitude)<0) {
            changeInMagnitude=-vector.magnitude;
        }

        return ClampMagnitude(
                vector.normalized * (vector.magnitude + changeInMagnitude),
                clampMinimum,
                clampMaximum
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
            return setXZ(velocity, bounceDirection*velocity.magnitude);
    }

    public static void Push(IMoves pushing, IMoves pushed, Vector3 normal) {
            Vector3 dvNormal = normal*Mathf.Max(pushing.BaseSpeed, pushed.BaseSpeed)*.1f;
            pushing.Velocity += dvNormal;
            pushed.Velocity = pushing.Velocity-dvNormal;
    }

    public static float StrafeSpeedMultiplier(Vector3 aimDirection, Vector3 moveDirection) {
        float dot = Vector3.Dot(aimDirection, moveDirection);

        if (dot>0) {
            return 1f;
        } else {
            return 1f+dot/5f;
        }
    }

    /// <summary>
    /// Fires if a move direction button was recently pressed (according to the buffer) and if said move direction was pressed again
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static Vector3 GetDashDirection(Character c) {
        if (
            (c.InputMoveDirection.x<0f && c.DashBuffer[0]>0)
            || (c.InputMoveDirection.x>0f && c.DashBuffer[1]>0)
            || (c.InputMoveDirection.y<0f && c.DashBuffer[2]>0)
            || (c.InputMoveDirection.y>0f && c.DashBuffer[3]>0)
        ) {
            return new Vector3(
                c.DashBuffer[0]>0?-1:(c.DashBuffer[1]>0?1:0),
                0f,
                c.DashBuffer[2]>0?-1:(c.DashBuffer[3]>0?1:0)
            ).normalized;
        } else {
            return Vector3.zero;
        }
    }

    public static AimMovementOrientation GetAimMovementOrientation(Vector3 aimDirVector, Vector3 moveDirVector) {
        float aimMovementNorm = Vector3.Dot(moveDirVector.normalized, aimDirVector.normalized);
        return aimMovementNorm switch {
            >.707f => AimMovementOrientation.PARALLEL,
            <-.707f => AimMovementOrientation.ANTIPARALLEL,
            _ => AimMovementOrientation.PERPENDICULAR
        };
    }

    public static RotationMovementOrientation GetRotationMovementOrientation(Character c) {
        Vector3 relativeMoveDirection = Quaternion.Inverse(c.transform.rotation) * c.MoveDirection;

        if (relativeMoveDirection.x>0 == c.IsRotatingClockwise()) {
            return RotationMovementOrientation.WITH;
        } else {
            return RotationMovementOrientation.AGAINST;
        }
    }
}

public class CharacterStateRunning : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.MOVEMENT; }}

    public CharacterStateRunning(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.CommandMovement != null) {
            return Factory.CommandMovement();
        } else {
            Vector3 dashDirection = MovementUtils.GetDashDirection(Character);

            if (dashDirection != Vector3.zero) {
            for (int i = 0; i < Character.DashBuffer.Length; i++) { Character.DashBuffer[i]=0;}

            AimMovementOrientation a = MovementUtils.GetAimMovementOrientation(Character.InputAimVector, dashDirection);
            return a switch {
                AimMovementOrientation.PARALLEL => Factory.Dashing(),
                AimMovementOrientation.PERPENDICULAR => Factory.Slipping(),
                _ => Factory.BackDashing()
                };
            } else {
                return null;
            }
        }
    }

    public override void EnterState() => base.EnterState();
    protected override void ExitState() {}
    protected override void InitializeSubState() {}

    protected override void FixedUpdateState() {
        Vector3 horizontalVelocity = MovementUtils.inXZ(Character.Velocity);
        float strafeSpeedMult = MovementUtils.StrafeSpeedMultiplier(
            Character.MoveDirection.normalized,
            Character.InputAimVector.normalized
        );

        float dSpeed = Mathf.Clamp(
            strafeSpeedMult*Mathf.Max(Character.BaseSpeed, horizontalVelocity.magnitude) - Vector3.Dot(horizontalVelocity, Character.MoveDirection),
            Character.Friction*(strafeSpeedMult-1),
            Character.RunAcceleration
        );

        Vector3 newVelocity = Vector3.ClampMagnitude(
            horizontalVelocity + dSpeed*(
                (Character.MoveDirection == Vector3.zero)
                ? -horizontalVelocity.normalized
                : Character.MoveDirection
            ),
            Mathf.Max(Character.BaseSpeed, horizontalVelocity.magnitude)
        );
              
        Character.Velocity = MovementUtils.setXZ(
            Character.Velocity,
            (Vector3.Dot(horizontalVelocity, newVelocity)<0) ? Vector3.zero : newVelocity
        );
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain && info!=null) {
            Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
            return true;
        } else if (collidable is Character otherCharacter) {
            Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
            Character.Transform.position += MovementUtils.inXZ(decollisionVector);
            return true;
        } else {
            return false;
        }
    }
}

public class CharacterStateDashing : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.MOVEMENT; }}
    private readonly int _duration = 4;
    private readonly float[] velocityCurve = new float[]{.13f, .15f, .17f, .19f};
    private int _frame;

    public CharacterStateDashing(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    protected override CharacterState CheckGetNewState() {
        if (++_frame==_duration) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.Velocity = Character.InputAimVector.normalized*velocityCurve[0];
        _frame = 0;
    }

    protected override void FixedUpdateState() => Character.Velocity = Character.Velocity.normalized*velocityCurve[_frame];
    protected override void ExitState() {}
    protected override void InitializeSubState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain terrain && info!=null) {
            Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
            return true;
        } else if (collidable is Character otherCharacter) {
            Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
            Character.Transform.position += MovementUtils.inXZ(decollisionVector);
            return true;
        } else {
            return false;
        }
    }
}

public class CharacterStateSlipping : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.MOVEMENT; }}
    private readonly int _duration = 12;
    private int _timer;

    public CharacterStateSlipping(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    protected override CharacterState CheckGetNewState() {
        if (--_timer==0) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.Velocity = Quaternion.AngleAxis(
            Mathf.Atan(
                Character.InputAimVector.magnitude
            )*Mathf.Rad2Deg,
            (
                (Quaternion.Inverse(Character.transform.rotation)*Character.MoveDirection).x<0
                ? Vector3.down : Vector3.up
            )
        ) * Character.InputAimVector.normalized * .3f;
        _timer = _duration;
    }

    protected override void ExitState() => Character.Velocity = Character.Velocity.normalized*Character.BaseSpeed;
    protected override void FixedUpdateState() {}
    protected override void InitializeSubState() {}
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}

public class CharacterStateBackDashing : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.MOVEMENT; }}
    private readonly int _duration = 18;
    private int _timer;

    public CharacterStateBackDashing(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    protected override CharacterState CheckGetNewState() {
        if (--_timer==0) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.Velocity = -Character.InputAimVector.normalized*.2f;
        _timer = _duration;
    }

    protected override void FixedUpdateState() => Character.Velocity = -Character.InputAimVector.normalized*.2f;
    protected override void ExitState() => Character.Velocity = Character.Velocity.normalized*Character.BaseSpeed;
    protected override void InitializeSubState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain terrain && info!=null) {
            Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
            return true;
        } else if (collidable is Character otherCharacter) {
            Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
            Character.Transform.position += MovementUtils.inXZ(decollisionVector);
            return true;
        } else {
            return false;
        }
    }
}

public class CharacterStateSliding : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.MOVEMENT; }}

    public CharacterStateSliding(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.CommandMovement != null) {
            return Factory.CommandMovement();
        } else {
            return null;
        }
    }

    public override void EnterState() => base.EnterState();
    protected override void ExitState() {}
    protected override void InitializeSubState() {}

    protected override void FixedUpdateState() {
        float VelocityDotAim = Vector3.Dot(
            Character.InputMoveDirection.normalized,
            Character.Velocity.normalized
        );

        if (Character.IsGrounded()) {
            if (VelocityDotAim<-.5f) {
                Character.Velocity = MovementUtils.ChangeMagnitude(Character.Velocity, -2*Character.Friction);
            }
            //  else if (VelocityDotAim<.5f || SuperState.Type!=CharacterStateType.ACTION) {
            //     // Character.Velocity = MovementUtils.ChangeMagnitude(Character.Velocity, -Character.Friction);
            // } 
            else {
                Character.Velocity = MovementUtils.ChangeMagnitude(Character.Velocity, -Character.Friction);
            }
        }
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain terrain && info!=null) {
            Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
            return true;
        } else if (collidable is Character otherCharacter) {
            Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
            Character.Transform.position += MovementUtils.inXZ(decollisionVector);
            return true;
        } else {
            return false;
        }
    }
}

public class CharacterStateCommandMovement : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.MOVEMENT; }}

    public CharacterStateCommandMovement(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = false;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.CommandMovement == null) {
            return Factory.Running();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }

    protected override void ExitState() {}
    protected override void FixedUpdateState() {}
    protected override void InitializeSubState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        return Character.CommandMovement.OnCollideWith(collidable, info);
    }
}