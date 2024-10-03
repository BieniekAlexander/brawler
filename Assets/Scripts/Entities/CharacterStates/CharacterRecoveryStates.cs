using UnityEngine;

public class CharacterStateKnockedDown : CharacterState {
    public CharacterStateKnockedDown(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.InputMoveDirection != Vector3.zero) {
            return Factory.GettingUp();
        } else if (Character.InputDash) {
            return Factory.Rolling();
        } else {
            return null;
        }
    }

    public override void EnterState(){}
    public override void ExitState(){} 
    public override void FixedUpdateState(){}
    public override void InitializeSubState(){}
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info){}
}

public class CharacterStateTumbling : CharacterState {
    private float _tumbleAcceleration = 50f;
    
    public CharacterStateTumbling(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.InputMoveDirection!=Vector3.zero) {
            return Factory.GettingUp();
        } else if (Character.InputDash) {
            return Factory.Rolling();
        } else if (Mathf.Approximately(Character.Velocity.magnitude, 0f)) {
            return Factory.KnockedDown();
        } else {
            return null;
        }
    }

    public override void EnterState(){}
    public override void ExitState(){}
    public override void InitializeSubState(){}

    public override void FixedUpdateState() {
        Character.Velocity = MovementUtils.ChangeMagnitude(Character.Velocity, -_tumbleAcceleration*Time.deltaTime);
    }

    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain stageTerrain) {
            // TODO improve interaction of tumbling into wall
            Character.Velocity = new();
        }
    }
}

public class CharacterStateRolling : CharacterState {
    private float _rollSpeed = 10f;
    private int _recoveryDuration = 20;

    public CharacterStateRolling(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.RecoveryTimer == 0) {
            return Factory.Idle();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.RecoveryTimer = _recoveryDuration;
        Character.Velocity = Character.LookDirection * _rollSpeed;
        // add invincibility
    }

    public override void ExitState(){
        // undo invincibility
    }

    public override void FixedUpdateState() {
        Character.RecoveryTimer--;
    }

    public override void InitializeSubState() {}
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) {
        if (collidable is StageTerrain stageTerrain) {
            // TODO improve interaction of tumbling into wall
            Character.Velocity = new();
        }
    }
}

public class CharacterStateGettingUp : CharacterState {
    private int _recoveryDuration = 15;

    public CharacterStateGettingUp(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.RecoveryTimer == 0) {
            return Factory.Idle();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.RecoveryTimer = _recoveryDuration;
        Character.Velocity = new();
        // TODO invulnerability
    }

    public override void ExitState(){
        // TODO invulnerability
    }

    public override void FixedUpdateState() {
        Character.RecoveryTimer--;
    }

    public override void InitializeSubState(){}
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info){}
}


public class CharacterStateGetUpAttacking : CharacterState {
    private int _recoveryDuration = 60;

    public CharacterStateGetUpAttacking(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) {
        _isRootState = true;
    }

    public override CharacterState CheckGetNewState() {
        if (Character.RecoveryTimer == 0) {
            return Factory.Idle();
        } else {
            return null;
        }
    }

    public override void EnterState() {
        Character.RecoveryTimer = _recoveryDuration;
        Character.Velocity = new();
        // TODO invulnerability
    }

    public override void ExitState() {
        // TODO invulnerability
    }

    public override void FixedUpdateState() {
        Character.RecoveryTimer--;
    }

    public override void InitializeSubState() { }
    public override void OnCollideWith(ICollidable collidable, CollisionInfo info) { }
}
