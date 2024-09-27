public class CharacterStateFactory {
    // somebody suggested the following for visualization: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Experimental.GraphView.GraphView.html
    Character _character;

    public CharacterStateFactory(Character c) {
        _character = c;
    }

    // movement
    public CharacterState Idle() { return new CharacterStateIdle(_character, this); }
    public CharacterState Walking() { return new CharacterStateWalking(_character, this); }
    public CharacterState Running() { return new CharacterStateRunning(_character, this); }
    public CharacterState Dashing() { return new CharacterStateDashing(_character, this); }

    // defense
    public CharacterState Exposed() { return new CharacterStateExposed(_character, this); }
    public CharacterState Blocking() { return new CharacterStateBlocking(_character, this); }
    public CharacterState Shielding() { return new CharacterStateShielding(_character, this); }

    // disadvantage
    public CharacterState HitStopped() { return new CharacterStateHitStopped(_character, this); }
    public CharacterState PushedBack() { return new CharacterStatePushedBack(_character, this); }
    public CharacterState KnockedBack() { return new CharacterStateKnockedBack(_character, this); }
    public CharacterState BlownBack() { return new CharacterStateBlownBack(_character, this); }
}
