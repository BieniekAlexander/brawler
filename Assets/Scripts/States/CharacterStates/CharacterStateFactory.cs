using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Design;

public class CharacterStateFactory {
    // somebody suggested the following for visualization: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Experimental.GraphView.GraphView.html
    Character _character;
    Dictionary<string, CharacterState> _stateDict = new Dictionary<string, CharacterState>();
    ServiceContainer sc = new();

    public CharacterStateFactory(Character character) {
        _character = character;

        // https://stackoverflow.com/questions/5411694/get-all-inherited-classes-of-an-abstract-class
        IEnumerable<CharacterState> states = (
            from t in typeof(CharacterState).Assembly.GetTypes() 
            where (t.IsSubclassOf(typeof(CharacterState)) && !t.IsAbstract)
            select (CharacterState) Activator.CreateInstance(t, character, this)
        );

        foreach (CharacterState s in states) {
            sc.AddService(s.GetType(), s);
        }
    }

    public CharacterState Get(Type t) => (CharacterState) sc.GetService(t);
}
