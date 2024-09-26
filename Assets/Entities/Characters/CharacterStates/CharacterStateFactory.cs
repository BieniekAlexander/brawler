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

    // disadvantage
}
