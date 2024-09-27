using System.Linq.Expressions;
using UnityEngine;

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

    public override void EnterState() { }
    public override void ExitState() { }
    public override void InitializeSubState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }

    public override void FixedUpdateState() {
        Character.HitStopTimer--;
    }

}

public class CharacterStatePushedBack : CharacterState {
    private float _knockBackDecay = 1f;

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
        // TODO I don't know if I want DI on light hits
        Character.Velocity = Character.KnockBack;
        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {
        Character.HitStunTimer--;
        MovementUtils.ChangeMagnitude(
            Character.Velocity, -_knockBackDecay*Time.deltaTime
        );
    }

    public override void ExitState() { }
    public override void InitializeSubState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}

public class CharacterStateKnockedBack : CharacterState {
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;
    private float _knockBackDecay = 1f;

    public CharacterStateKnockedBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        // TODO implement knockdown, teching
        if (Character.HitStunTimer==0) {
            return Factory.Idle();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.Velocity = Vector3.RotateTowards(
            Character.KnockBack,
            (
                Character.InputMoveDirection==Vector3.zero
                ? Character.KnockBack
                : Character.InputMoveDirection
            ),
            _maxAngleChange,
            0f
        );

        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {
        Character.HitStunTimer--;
        MovementUtils.ChangeMagnitude(
            Character.Velocity, -_knockBackDecay * Time.deltaTime
        );
    }

    public override void ExitState() { }
    public override void InitializeSubState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}

public class CharacterStateBlownBack : CharacterState {
    private float _knockBackDecay = 1f;
    private float _maxAngleChange = 15f*Mathf.Deg2Rad;

    public CharacterStateBlownBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        // TODO implement knockdown
        if (Character.HitStunTimer==0) {
            return Factory.Idle();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.Velocity = Vector3.RotateTowards(
            Character.KnockBack,
            (
                Character.InputMoveDirection==Vector3.zero
                ? Character.KnockBack
                : Character.InputMoveDirection
            ),
            _maxAngleChange,
            0f
        );

        Character.KnockBack = new();
    }

    public override void FixedUpdateState() {
        Character.HitStunTimer--;
        MovementUtils.ChangeMagnitude(
            Character.Velocity, -_knockBackDecay*Time.deltaTime
        );
    }

    public override void ExitState() { }
    public override void InitializeSubState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}
