using System;

public enum CharacterStateType {
    ACTION,
    DISADVANTAGE,
    RECOVERY,
    MOVEMENT
}

public abstract class CharacterState {
    // reference: https://www.youtube.com/watch?v=Vt8aZDPzRjI
    private Character _character;
    public Character Character { get { return _character; } set { _character = value; } }

    protected CharacterStateFactory _factory;
    public CharacterStateFactory Factory { get { return _factory; } set { _factory = value; } }

    protected bool _isRootState = false;
    public abstract CharacterStateType Type { get; }
    public bool IsRootState { get { return _isRootState; } set { _isRootState = value; } }

    protected CharacterState SuperState {get; private set; }
    protected CharacterState SubState {get; private set; }
    public CharacterState(Character _machine, CharacterStateFactory _factory) {
        Character = _machine;
        Factory = _factory;
    }

    public static CharacterState GetSpawnState(CharacterStateFactory factory) {
        CharacterState root = factory.Ready();
        root.SetSubState(factory.Walking());
        return root;
    }

    public virtual void EnterState() {
        InitializeSubState();
    }

    protected abstract void FixedUpdateState();
    protected abstract void ExitState();

    protected abstract CharacterState CheckGetNewState();
    protected abstract void InitializeSubState();

    public void FixedUpdateStates() {
        if (CheckGetNewState() is CharacterState newState) {
            SwitchState(newState);
        } else {
            FixedUpdateState();
            SubState?.FixedUpdateStates();
        }
    }

    protected void SwitchState(CharacterState newState) {
        ExitState();
        newState.EnterState();

        if (_isRootState) {
            Character.State = newState;
        } else {
            SuperState?.SetSubState(newState);
        }
    }

    private void SetSuperState(CharacterState newSuperState) {
        SuperState = newSuperState;
    }

    protected void SetSubState(CharacterState newSubState) {
        SubState?.ExitState();
        SubState = newSubState;
        SubState.SetSuperState(this);
    }

    public abstract bool OnCollideWith(ICollidable collidable, CollisionInfo info);

    public bool StateInHierarchy(Type stateType) {
        if (GetType()==stateType) {
            return true;
        } else if (SubState != null) {
            return SubState.StateInHierarchy(stateType);
        } else {
            return false;
        }
    }

    public virtual bool HandleCollisionWithStates(ICollidable collidable, CollisionInfo info) {
        bool ret = false;

        ret |= OnCollideWith(collidable, info);

        if (SubState != null) {
           ret |= SubState.HandleCollisionWithStates(collidable, info);
        }

        return ret;
    }

    override public string ToString() {
        return GetType().Name.Replace("CharacterState", "");
    }

    public string getNames() {
        string subStateNames = (SubState!=null)?SubState.getNames():"";
        return $"{this}->{subStateNames}";
    }
}
