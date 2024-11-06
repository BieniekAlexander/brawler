using System;
using UnityEngine;

public static class KnockBackUtils {
    public static int getHitLag(HitTier HitTier) {
        return (HitTier) switch {
            HitTier.Soft => 0,
            HitTier.Light => 7,
            HitTier.Medium => 10,
            HitTier.Heavy => 15,
            HitTier.Pure => 15, // TODO play around with these values
            _ => throw new NotImplementedException($"Unknown HitTier value: {HitTier}")
        };
    }

    public static int getHitStun(HitTier HitTier) {
        return (HitTier) switch {
            HitTier.Soft => 0,
            HitTier.Light => 10,
            HitTier.Medium => 15,
            HitTier.Heavy => 20,
            HitTier.Pure => 20, // TODO play around with these values
            _ => throw new NotImplementedException($"Unknown HitTier value: {HitTier}")
        };
    }
}

public class CharacterStateHitStopped : CharacterState {
    public CharacterStateHitStopped(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.HitStopTimer==0) {
            return Character.KnockBackHitTier switch {
                HitTier.Heavy => Factory.BlownBack(),
                HitTier.Medium => Factory.KnockedBack(),
                HitTier.Light => Factory.PushedBack(),
                _ => Factory.Ready()
            };
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
        Character.Shield.gameObject.SetActive(false);
    }

    protected override void ExitState() {}
    protected override void InitializeSubState() {}
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info)  => false;

    protected override void FixedUpdateState() {
        Character.HitStopTimer--;
    }
}

public class CharacterStatePushedBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStatePushedBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.HitStunTimer<=0) {
            return Factory.Ready();
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

    protected override void FixedUpdateState() {
        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
        }
    }

    protected override void ExitState() {
        Character.UnsetBusy();
    }

    protected override void InitializeSubState() {
        SetSubState(Factory.Sliding());
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info)  => false;
}

public class CharacterStateKnockedBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateKnockedBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        // TODO implement knockdown, teching
        if (Character.HitStunTimer<=0) {
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

    protected override void FixedUpdateState() {
        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
        }
    }

    protected override void InitializeSubState() {
        SetSubState(Factory.Sliding());
    }

    protected override void ExitState() {}
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain terrain) {
            Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
            return true;
        } else {
            return false;
        }
    }
}

public class CharacterStateBlownBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateBlownBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
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

    protected override void FixedUpdateState() {
        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
        }
    }

    protected override void InitializeSubState() {
        SetSubState(Factory.Sliding());
    }

    protected override void ExitState() { }
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info)  => false;
}
