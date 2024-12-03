using System.Collections.Generic;
using UnityEngine;

public class CharacterStateFactory {
    // somebody suggested the following for visualization: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Experimental.GraphView.GraphView.html
    Character _character;
    Dictionary<string, CharacterState> _stateDict = new Dictionary<string, CharacterState>();

    public CharacterStateFactory(Character character) {
        _character = character;

        // Movement
        _stateDict["Walking"] = new CharacterStateWalking(_character, this);
        _stateDict["Dashing"] = new CharacterStateDashing(_character, this);
        _stateDict["BackDashing"] = new CharacterStateBackDashing(_character, this);
        _stateDict["Sliding"] = new CharacterStateSliding(_character, this);
        _stateDict["CommandMovement"] = new CharacterStateCommandMovement(_character, this);
        // Action
        _stateDict["Ready"] = new CharacterStateReady(_character, this);
        _stateDict["Blocking"] = new CharacterStateBlocking(_character, this);
        _stateDict["Busy"] = new CharacterStateBusy(_character, this);
        // Disadvantage
        _stateDict["PushedBack"] = new CharacterStatePushedBack(_character, this);
        _stateDict["KnockedBack"] = new CharacterStateKnockedBack(_character, this);
        _stateDict["BlownBack"] = new CharacterStateBlownBack(_character, this);
        // Recovery
        _stateDict["Tumbling"] = new CharacterStateTumbling(_character, this);
        _stateDict["KnockedDown"] = new CharacterStateKnockedDown(_character, this);
        _stateDict["GettingUp"] = new CharacterStateGettingUp(_character, this);
        _stateDict["Rolling"] = new CharacterStateRolling(character, this);
        _stateDict["GetUpAttacking"] = new CharacterStateGetUpAttacking(_character, this);
    }

    // movement
    public CharacterState Walking() => _stateDict["Walking"];
    public CharacterState Dashing() => _stateDict["Dashing"];
    public CharacterState BackDashing() => _stateDict["BackDashing"];
    public CharacterState Sliding() => _stateDict["Sliding"];
    public CharacterState CommandMovement() => _stateDict["CommandMovement"];

    // action
    public CharacterState Ready() => _stateDict["Ready"];
    public CharacterState Blocking() => _stateDict["Blocking"];
    public CharacterState Shielding() => _stateDict["Shielding"];
    public CharacterState Busy() => _stateDict["Busy"];

    // disadvantage
    public CharacterState PushedBack() => _stateDict["PushedBack"];
    public CharacterState KnockedBack() => _stateDict["KnockedBack"];
    public CharacterState BlownBack() => _stateDict["BlownBack"];

    // recovery
    public CharacterState Tumbling() => _stateDict["Tumbling"];
    public CharacterState KnockedDown() => _stateDict["KnockedDown"];
    public CharacterState GettingUp() => _stateDict["GettingUp"];
    public CharacterState GetUpAttacking() => _stateDict["GetUpAttacking"];
    public CharacterState Rolling() => _stateDict["Rolling"];
}
