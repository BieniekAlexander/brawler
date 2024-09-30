using UnityEngine;

public static class KnockBackUtils {
    // TODO maybe this isn't the best spot, but I think it's okay for now :)
    public static void DecomposeKnockback(Vector3 knockBackVector, ref Vector3 horizontalVector, ref float verticalMagnitude) {
        horizontalVector = Vector3.Scale(new Vector3(1f, 0f, 1f), knockBackVector);
        verticalMagnitude = knockBackVector.y;
    }

    public static float KnockBackDecay = 10f;
}

public class CharacterStateHitStopped : CharacterState {
    public CharacterStateHitStopped(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        if (Character.HitStopTimer==0) {
            return Character.KnockBackHitTier switch {
                HitTier.Heavy => Factory.BlownBack(),
                HitTier.Medium => Factory.KnockedBack(),
                HitTier.Light => Factory.PushedBack(),
                _ => Factory.Idle()
            };
        } else {
            return null;
        }
    }

    public override void EnterState() { }
    public override void ExitState() { }
    public override void InitializeSubState() {
        _subState = null;
    }

    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }

    public override void FixedUpdateState() {
        Character.HitStopTimer--;
    }

}

public class CharacterStatePushedBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStatePushedBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        if (Character.HitStunTimer==0) {
            return Factory.Idle();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.KnockBack = Vector3.RotateTowards(
            Character.KnockBack,
            (
                Character.InputMoveDirection==Vector3.zero
                ? Character.KnockBack
                : Character.InputMoveDirection
            ),
            _maxAngleChange,
            0f
        );

        KnockBackUtils.DecomposeKnockback(Character.KnockBack, ref Character.HorizontalVelocity, ref Character.VerticalVelocity);
        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {
        Character.HitStunTimer--;

        if (Character.IsGrounded()) {
                Character.HorizontalVelocity = MovementUtils.ChangeMagnitude(
                Character.HorizontalVelocity, -KnockBackUtils.KnockBackDecay*Time.deltaTime
            );
        }
    }

    public override void ExitState() { }
    public override void InitializeSubState() {
        _subState = null;
    }

    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}

public class CharacterStateKnockedBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateKnockedBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        // TODO implement knockdown, teching
        if (Character.HitStunTimer==0) {
            return Factory.Tumbling();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.KnockBack = Vector3.RotateTowards(
            Character.KnockBack,
            (
                Character.InputMoveDirection==Vector3.zero
                ? Character.KnockBack
                : Character.InputMoveDirection
            ),
            _maxAngleChange,
            0f
        );

        KnockBackUtils.DecomposeKnockback(Character.KnockBack, ref Character.HorizontalVelocity, ref Character.VerticalVelocity);
        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {
        Character.HitStunTimer--;

        if (Character.IsGrounded()) {
            Character.HorizontalVelocity = MovementUtils.ChangeMagnitude(
            Character.HorizontalVelocity, -KnockBackUtils.KnockBackDecay * Time.deltaTime
        );
        }
    }

    public override void InitializeSubState() {
        _subState = null;
    }

    public override void ExitState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}

public class CharacterStateBlownBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateBlownBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
        InitializeSubState();
    }

    public override CharacterState CheckGetNewState() {
        // TODO implement knockdown
        if (Mathf.Approximately(Character.HorizontalVelocity.magnitude, 0f)) {
            return Factory.KnockedDown();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.KnockBack = Vector3.RotateTowards(
            Character.KnockBack,
            (
                Character.InputMoveDirection==Vector3.zero
                ? Character.KnockBack
                : Character.InputMoveDirection
            ),
            _maxAngleChange,
            0f
        );

        KnockBackUtils.DecomposeKnockback(Character.KnockBack, ref Character.HorizontalVelocity, ref Character.VerticalVelocity);
        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {
        Character.HitStunTimer--;

        if (Character.IsGrounded()){
            Character.HorizontalVelocity = MovementUtils.ChangeMagnitude(
                Character.HorizontalVelocity, -KnockBackUtils.KnockBackDecay*Time.deltaTime
            );
        }
    }

    public override void InitializeSubState() {
        _subState = null;
    }

    public override void ExitState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}
