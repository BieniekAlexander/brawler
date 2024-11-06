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
            return MovementUtils.setXZ(velocity, bounceDirection*velocity.magnitude);
    }

    public static void Push(IMoves pushing, IMoves pushed, Vector3 normal) {
            Vector3 dvNormal = normal*Mathf.Max(pushing.BaseSpeed, pushed.BaseSpeed)*.1f;
            pushing.Velocity += dvNormal;
            pushed.Velocity = pushing.Velocity-dvNormal;
    }

    public static float StrafeSpeedMultiplier(Vector3 aimDirection, Vector3 moveDirection) {
        float dot = Vector3.Dot(aimDirection.normalized, moveDirection.normalized);

        if (dot>0) {
            return 1f;
        } else {
            return 1f+dot/5f;
        }
    }
}

public class CharacterStateRunning : CharacterState {
    public CharacterStateRunning(Character _machine, CharacterStateFactory _factory)
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

    public override void EnterState() {
        base.EnterState();
    }

    protected override void ExitState() { }

    protected override void FixedUpdateState() {
        // TODO what if I'm in the air? Air control?
        Vector3 horizontalVelocity = MovementUtils.inXZ(Character.Velocity);
        float strafeSpeed = Character.BaseSpeed * MovementUtils.StrafeSpeedMultiplier(Character.MoveDirection, Character.InputAimDirection);

        float dSpeed = Mathf.Clamp(
            Character.BaseSpeed - Vector3.Dot(horizontalVelocity, Character.MoveDirection),
            0, Character.RunAcceleration
        );

        Vector3 newVelocity = Vector3.ClampMagnitude(
            horizontalVelocity + dSpeed*(
                (Character.MoveDirection == Vector3.zero)
                ? -horizontalVelocity.normalized
                : Character.MoveDirection
            ),
            Mathf.Max(
                strafeSpeed,
                horizontalVelocity.magnitude-Character.Friction
            )
        );

        Character.Velocity = MovementUtils.setXZ(
            Character.Velocity,
            (Vector3.Dot(horizontalVelocity, newVelocity)<0) ? Vector3.zero : newVelocity
        );
    }

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

    public override void EnterState() {
        base.EnterState();
    }

    protected override void ExitState() {}

    protected override void FixedUpdateState() {
        if (Character.InputMoveDirection == Vector2.zero && Character.IsGrounded()) {
            Character.Velocity = MovementUtils.ChangeMagnitude(Character.Velocity, -Character.Friction);
        }
    }

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

public class CharacterStateCommandMovement : CharacterState {
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

    protected override void ExitState() {
        Character.UnsetBusy();
    }
    protected override void FixedUpdateState() {}
    protected override void InitializeSubState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        return Character.CommandMovement.OnCollideWith(collidable, info);
    }
}