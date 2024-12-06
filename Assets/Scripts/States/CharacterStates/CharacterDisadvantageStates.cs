using System;
using UnityEngine;

public static class KnockBackUtils {
    public static int getHitStop(HitTier HitTier) {
        return HitTier switch {
            HitTier.Soft => 0,
            HitTier.Light => 7,
            HitTier.Medium => 10,
            HitTier.Heavy => 15,
            HitTier.Pure => 15, // TODO play around with these values
            _ => throw new NotImplementedException($"Unknown HitTier value: {HitTier}")
        };
    }

    public static int getHitStun(HitTier HitTier) {
        return HitTier switch {
            HitTier.Soft => 0,
            HitTier.Light => 10,
            HitTier.Medium => 15,
            HitTier.Heavy => 20,
            HitTier.Pure => 20, // TODO play around with these values
            _ => throw new NotImplementedException($"Unknown HitTier value: {HitTier}")
        };
    }
}

public class CharacterStatePushedBack : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.DISADVANTAGE; }}

    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStatePushedBack(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

    protected override Type GetNewStateType() {
        if (Character.HitStunTimer<=0) {
            return typeof(CharacterStateStanding);
        } else {
            return null;
        }
    }

    protected override void EnterState() {
        base.EnterState();
        Character.SetBusy(true, true, 0f);

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

    protected override void Tick() {
        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
            MovementUtils.Slide(Character);
        }
    }

    protected override void ExitState() {
        Character.UnsetBusy();
    }

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info)  => false;
}

public class CharacterStateKnockedBack : CharacterState {
    public override CharacterStateType Type {get {return CharacterStateType.DISADVANTAGE; }}
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateKnockedBack(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

    protected override Type GetNewStateType() {
        // TODO implement knockdown, teching
        if (Character.HitStunTimer<=0) {
            return typeof(CharacterStateTumbling);
        } else {
            return null;
        }
    }

    protected override void EnterState() {
        base.EnterState();
        Character.SetBusy(true, true, 0f);

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

    protected override void Tick() {
        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
            MovementUtils.Slide(Character);
        }
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
    public override CharacterStateType Type {get {return CharacterStateType.DISADVANTAGE; }}
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateBlownBack(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

    protected override Type GetNewStateType() {
        // TODO implement knockdown
        if (Mathf.Approximately(Character.Velocity.magnitude, 0f)) {
            return typeof(CharacterStateKnockedDown);
        } else {
            return null;
        }
    }

    protected override void EnterState() {
        base.EnterState();
        Character.SetBusy(true, true, 0f);

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

    protected override void Tick() {
        if (Character.IsGrounded()) {
            Character.HitStunTimer--;
            MovementUtils.Slide(Character);
        }
    }

    protected override void ExitState() { }
    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info)  => false;
}
