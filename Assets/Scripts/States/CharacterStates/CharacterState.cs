using System;

public enum CharacterStateType {
    ACTIVE,
    DISADVANTAGE,
    RECOVERY
}

public abstract class CharacterState {
    // reference: https://www.youtube.com/watch?v=Vt8aZDPzRjI
    private Character _character;
    public Character Character { get { return _character; } set { _character = value; } }

    protected CharacterStateFactory _factory;
    public CharacterStateFactory Factory { get { return _factory; } set { _factory = value; } }
    public abstract CharacterStateType Type { get; } // TODO I'll be reimplementing state organization, likely with a composition-type pattern, so probably change this

    public CharacterState(Character _machine, CharacterStateFactory _factory) {
        Character = _machine;
        Factory = _factory;
    }

    public static Type GetSpawnState() {
        return typeof(CharacterStateStanding);
    }

    public CharacterState SwapState(Type _newStateType) {
        ExitState();
        CharacterState newState = Factory.Get(_newStateType);
        newState.EnterState();
        return newState;
    }

    protected virtual void EnterState() {}
    protected virtual void ExitState() {}
    protected abstract Type GetNewStateType();
    protected virtual void Tick() {}

    public CharacterState FixedUpdateState() {
        if (GetNewStateType() is Type newStateType) {
            ExitState();
            CharacterState newState = Factory.Get(newStateType);
            newState.EnterState();
            return newState;
        } else {
            Tick();
            return null;
        }
    }

    public virtual bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;

    public virtual string Name { 
        get => GetType().Name.Replace("CharacterState", "");
    }
}
