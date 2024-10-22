using UnityEngine;
using System.Collections.Generic;


public class CharacterStateFactory {
    // somebody suggested the following for visualization: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Experimental.GraphView.GraphView.html
    Character _character;
    Dictionary<string, CharacterState> _stateDict = new Dictionary<string, CharacterState>();

    public CharacterStateFactory(Character character) {
        _character = character;

        // Movement
        _stateDict["Idle"] = new CharacterStateIdle(_character, this);
        _stateDict["Walking"] = new CharacterStateWalking(_character, this);
        _stateDict["Running"] = new CharacterStateRunning(_character, this);
        _stateDict["CommandMovement"] = new CharacterStateCommandMovement(_character, this);
        // Action
        _stateDict["Ready"] = new CharacterStateReady(_character, this);
        _stateDict["Blocking"] = new CharacterStateBlocking(_character, this);
        _stateDict["Shielding"] = new CharacterStateShielding(_character, this);
        //_stateDict["Casting"] = new CharacterStateCasting(_character, this);
        _stateDict["Busy"] = new CharacterStateBusy(_character, this);
        // Disadvantage
        _stateDict["HitStopped"] = new CharacterStateHitStopped(_character, this);
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
    public CharacterState Idle() { return _stateDict["Idle"]; }
    public CharacterState Walking() { return _stateDict["Walking"]; }
    public CharacterState Running() { return _stateDict["Running"]; }
    public CharacterState CommandMovement() { return _stateDict["CommandMovement"]; }

    // action
    public CharacterState Ready() { return _stateDict["Ready"]; }
    public CharacterState Blocking() { return _stateDict["Blocking"]; }
    public CharacterState Shielding() { return _stateDict["Shielding"]; }
    // public CharacterState Casting() { return _stateDict["Casting"]; } TODO
    public CharacterState Busy() { return _stateDict["Busy"]; }

    // disadvantage
    public CharacterState HitStopped() { return _stateDict["HitStopped"]; }
    public CharacterState PushedBack() { return _stateDict["PushedBack"]; }
    public CharacterState KnockedBack() { return _stateDict["KnockedBack"]; }
    public CharacterState BlownBack() { return _stateDict["BlownBack"]; }

    // recovery
    public CharacterState Tumbling() { return _stateDict["Tumbling"]; }
    public CharacterState KnockedDown() { return _stateDict["KnockedDown"]; }
    public CharacterState GettingUp() { return _stateDict["GettingUp"]; }
    public CharacterState Rolling() { return _stateDict["Rolling"]; }
    public CharacterState GetUpAttacking() { return _stateDict["GetUpAttacking"]; }
}
