using System;
using UnityEngine;

public static class KnockBackUtils {
    public static int getHitLag(HitTier HitTier) {
        return (HitTier) switch {
            HitTier.Soft => 0,
            HitTier.Light => 3,
            HitTier.Medium => 5,
            HitTier.Heavy => 7,
            HitTier.Pure => 10, // TODO play around with these values
            _ => throw new NotImplementedException($"Unknown HitTier value: {HitTier}")
        };
    }

    public static int getHitStun(Vector3 knockbackVector) {
        int hitstun = Mathf.FloorToInt(
            MovementUtils.inXZ(knockbackVector).magnitude * .8f
        );
        
        return hitstun;
    }

    public static float KnockBackDecay = 10f;
}

public class CharacterStateHitStopped : CharacterState {
    public CharacterStateHitStopped(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
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

    public override void EnterState() {
        base.EnterState();
        Character.Shield.gameObject.SetActive(false);
    }

    public override void ExitState() { }
    public override void InitializeSubState() {
        _subState = Factory.Busy();
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
    }

    public override CharacterState CheckGetNewState() {
        if (Character.HitStunTimer==0) {
            return Factory.Idle();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();

        Character.KnockBack = Vector3.RotateTowards(
            Character.KnockBack,
            (
                Character.MoveDirection==Vector3.zero
                ? Character.KnockBack
                : Character.MoveDirection
            ),
            _maxAngleChange,
            0f
        );

        Character.Velocity = Character.KnockBack;
        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {
        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
            Character.Velocity = MovementUtils.ChangeMagnitude(
                MovementUtils.inXZ(Character.Velocity),
                -KnockBackUtils.KnockBackDecay*Time.deltaTime
            );
        }
    }

    public override void ExitState() { }

    public override void InitializeSubState() {
        _subState = Factory.Busy();
    }

    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}

public class CharacterStateKnockedBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateKnockedBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
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
        base.EnterState();

        Character.KnockBack = Vector3.RotateTowards(
            Character.KnockBack,
            (
                Character.MoveDirection==Vector3.zero
                ? Character.KnockBack
                : Character.MoveDirection
            ),
            _maxAngleChange,
            0f
        );

        Character.Velocity = Character.KnockBack;
        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {

        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
            Character.Velocity = MovementUtils.ChangeMagnitude(
                MovementUtils.inXZ(Character.Velocity),
                -KnockBackUtils.KnockBackDecay*Time.deltaTime
            );
        }
    }

    public override void InitializeSubState() {
        _subState = Factory.Busy();
    }

    public override void ExitState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}

public class CharacterStateBlownBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateBlownBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        // TODO implement knockdown
        if (Mathf.Approximately(Character.Velocity.magnitude, 0f)) {
            return Factory.KnockedDown();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();

        Character.KnockBack = Vector3.RotateTowards(
            Character.KnockBack,
            (
                Character.MoveDirection==Vector3.zero
                ? Character.KnockBack
                : Character.MoveDirection
            ),
            _maxAngleChange,
            0f
        );

        Character.Velocity = Character.KnockBack;
        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {
        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
            Character.Velocity = MovementUtils.ChangeMagnitude(
                MovementUtils.inXZ(Character.Velocity),
                -KnockBackUtils.KnockBackDecay*Time.deltaTime
            );
        }
    }

    public override void InitializeSubState() {
        _subState = Factory.Busy();
    }

    public override void ExitState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}
