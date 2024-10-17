using System;
using UnityEditor.Experimental.GraphView;

public abstract class CharacterState {
    // reference: https://www.youtube.com/watch?v=Vt8aZDPzRjI
    private Character _character;
    public Character Character { get { return _character; } set { _character = value; } }

    protected CharacterStateFactory _factory;
    public CharacterStateFactory Factory { get { return _factory; } set { _factory = value; } }

    protected bool _isRootState = false;
    public bool IsRootState { get { return _isRootState; } set { _isRootState = value; } }

    protected CharacterState _superState;
    protected CharacterState _subState;
    public CharacterState(Character _machine, CharacterStateFactory _factory) {
        Character = _machine;
        Factory = _factory;
    }

    public virtual void EnterState() {
        InitializeSubState();
    }

    public abstract void FixedUpdateState();
    public abstract void ExitState();

    public abstract CharacterState CheckGetNewState();
    public abstract void InitializeSubState();

    public void FixedUpdateStates() {
        if (CheckGetNewState() is CharacterState newState) {
            SwitchState(newState);
        } else {
            FixedUpdateState();

            if (_subState != null) {
                _subState.FixedUpdateStates();
            }
        }
    }

    protected void SwitchState(CharacterState newState) {
        ExitState();
        newState.EnterState();

        if (_isRootState) {
            Character.State = newState;

        } else if (_superState != null) {
            _superState.SetSubState(newState);
        }
    }

    protected void SetSuperState(CharacterState newSuperState) {
        _superState = newSuperState;
    }

    protected void SetSubState(CharacterState newSubState) {
        _subState = newSubState;
        newSubState.SetSuperState(this);
    }

    public abstract void OnCollideWith(ICollidable collidable, CollisionInfo info);

    public virtual bool StateInHierarchy(Type stateType) {
        if (this.GetType()==stateType) {
            return true;
        } else if (_subState != null) {
            return _subState.StateInHierarchy(stateType);
        } else {
            return false;
        }
    }

    public virtual void HandleCollisionWithStates(ICollidable collidable, CollisionInfo info) {
        OnCollideWith(collidable, info);

        if (_subState != null) {
            _subState.HandleCollisionWithStates(collidable, info);
        }
    }
}
