public class CharacterStateBlowBack : CharacterState {
    public CharacterStateBlowBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) { }

    public override CharacterState CheckGetNewState() {
        throw new System.NotImplementedException();
    }

    public override void EnterState() {
        // write stuff that happens when the state starts
        throw new System.NotImplementedException();
    }

    public override void ExitState() {
        throw new System.NotImplementedException();
    }

    public override void FixedUpdateState() {
        // write stuff that happens during each frame of game logic in this state
        // this includes state-internal changes, state-transitions, and handling inputs to the state
        throw new System.NotImplementedException();
    }

    public override void InitializeSubState() {
        throw new System.NotImplementedException();
    }

    /*public override void OnCollisionEnter(CharacterStateMachine _machine) {
        // write stuff 
    }*/
}

public class CharacterStateKnockedBack : CharacterState {
    public CharacterStateKnockedBack(Character _machine, CharacterStateFactory _factory)
    : base(_machine, _factory) { }

    public override CharacterState CheckGetNewState() {
        throw new System.NotImplementedException();
    }

    public override void EnterState() {
        // write stuff that happens when the state starts
        throw new System.NotImplementedException();
    }

    public override void ExitState() {
        throw new System.NotImplementedException();
    }

    public override void FixedUpdateState() {
        // write stuff that happens during each frame of game logic in this state
        // this includes state-internal changes, state-transitions, and handling inputs to the state
        throw new System.NotImplementedException();
    }

    public override void InitializeSubState() {
        throw new System.NotImplementedException();
    }

    /*public override void OnCollisionEnter(CharacterStateMachine _machine) {
        // write stuff 
    }*/
}
