using UnityEngine;

public class CharacterStateKnockedDown : CharacterState {
    public CharacterStateKnockedDown(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.HitStopTimer > 0) {
            return Factory.HitStopped();
        } else if (Character.InputMoveDirection != Vector2.zero) {
            return Factory.GettingUp();
        } else if (Character.InputCastId == (int)CastId.Dash) { // TODO do I want to be reading directly from InputCastId?
            return Factory.Rolling();
        } else {
            return null;
        }
    }

    public override void EnterState(){
        base.EnterState();
    }

    protected override void ExitState() {}
    protected override void FixedUpdateState() {}
    protected override void InitializeSubState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}

public class CharacterStateTumbling : CharacterState {
    private float _tumbleAcceleration = .2f;
    
    public CharacterStateTumbling(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.HitStopTimer > 0) {
            return Factory.HitStopped();
        } else if (Character.InputMoveDirection != Vector2.zero) {
            return Factory.GettingUp();
        } else if (Character.InputCastId == (int)CastId.Dash) { // TODO do I want to be reading directly from InputCastId?
            return Factory.Rolling();
        } else if (Mathf.Approximately(Character.Velocity.magnitude, 0f)) {
            return Factory.KnockedDown();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
    }

    protected override void ExitState() {}
    protected override void InitializeSubState() {
        SetSubState(Factory.Sliding());
    }
    protected override void FixedUpdateState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain stageTerrain) {
            // TODO improve interaction of tumbling into wall
            Character.Velocity = new();
            return true;
        } else {
            return false;
        }
    }
}

public class CharacterStateRolling : CharacterState {
    private float _rollSpeed = .36f;
    private int _recoveryDuration = 15;

    public CharacterStateRolling(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.HitStopTimer > 0) {
            return Factory.HitStopped();
        } else if (Character.RecoveryTimer == 0) {
            return Factory.Ready();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
        // TODO not the prettiest place to do this, but I want this action to consume input
        // otherwise, I'm having an issue where the roll happens, but the dash input is still buffered
        Character.InputCastId = -1;
        Character.RecoveryTimer = _recoveryDuration;
        Character.Velocity = Character.InputAimDirection.normalized * _rollSpeed;
        Character.InvulnerableStack++;
    }

    protected override void ExitState(){
        Character.InvulnerableStack--;
        Character.UnsetBusy();
    }

    protected override void FixedUpdateState() {
        Character.RecoveryTimer--;
    }

    protected override void InitializeSubState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain stageTerrain) {
            // TODO improve interaction of tumbling into wall
            Character.Velocity = new();
            return true;
        } else {
            return false;
        }
    }
}

public class CharacterStateGettingUp : CharacterState {
    private int _recoveryDuration = 15;

    public CharacterStateGettingUp(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.HitStopTimer > 0) {
            return Factory.HitStopped();
        } else if (Character.RecoveryTimer == 0) {
            return Factory.Ready();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
        Character.RecoveryTimer = _recoveryDuration;
        Character.Velocity = new();
        Character.InvulnerableStack++;
    }

    protected override void ExitState(){
        Character.InvulnerableStack--;
        Character.UnsetBusy();
    }

    protected override void FixedUpdateState() {
        Character.RecoveryTimer--;
    }

    protected override void InitializeSubState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}

public class CharacterStateGetUpAttacking : CharacterState {
    private int _recoveryDuration = 60;

    public CharacterStateGetUpAttacking(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    protected override CharacterState CheckGetNewState() {
        if (Character.HitStopTimer > 0) {
            return Factory.HitStopped();
        } else if (Character.RecoveryTimer == 0) {
            return Factory.Ready();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        base.EnterState();
        Character.RecoveryTimer = _recoveryDuration;
        Character.Velocity = new();
        Character.InvulnerableStack++;
    }

    protected override void ExitState() {
        Character.InvulnerableStack--;
        Character.UnsetBusy(); // TODO
    }

    protected override void FixedUpdateState() {
        Character.RecoveryTimer--;
    }

    protected override void InitializeSubState() {}

    public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}
