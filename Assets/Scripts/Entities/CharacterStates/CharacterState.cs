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

    public static CharacterState GetSpawnState(CharacterStateFactory factory) {
        CharacterState root = factory.Idle();
        root.SetSubState(factory.Ready());
        return root;
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

        if (_subState!=null){
            newState.SetSubState(_subState);
        }

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

    public abstract bool OnCollideWith(ICollidable collidable, CollisionInfo info);

    public bool StateInHierarchy(Type stateType) {
        if (this.GetType()==stateType) {
            return true;
        } else if (_subState != null) {
            return _subState.StateInHierarchy(stateType);
        } else {
            return false;
        }
    }

    public virtual bool HandleCollisionWithStates(ICollidable collidable, CollisionInfo info) {
        bool ret = false;

        ret |= OnCollideWith(collidable, info);

        if (_subState != null) {
           ret |= _subState.HandleCollisionWithStates(collidable, info);
        }

        return ret;
    }

    override public string ToString() {
        return GetType().Name.Replace("CharacterState", "");
    }

    public string getNames() {
        string subStateNames = (_subState!=null)?_subState.getNames():"";
        return $"{this}->{subStateNames}";
    }
}
