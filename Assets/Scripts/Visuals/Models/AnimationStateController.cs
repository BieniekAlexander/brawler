using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine.Assertions;
using ServiceStack;

[Serializable]
public class AnimationStatePair {
    public string name;
    public AnimationClip animation;
}

public class AnimationStateController : MonoBehaviour
{
    private AnimatorController animatorController;
    private Animator animator;
    private Character character;
    private float timer = 1f;
    private float duration = 0f;
    private string currentStateName = "";

    // pattern for setting animation in code https://www.youtube.com/watch?v=ZwLekxsSY3Y
    // https://stackoverflow.com/a/41728640/3600382
    void Start()
    {
        character = GetComponent<Character>();
        animator = GetComponent<Animator>();
        animatorController = (AnimatorController) animator.runtimeAnimatorController;

        List<string> animatorStateNames = getAnimatorStateNames();
        IEnumerable<CharacterState> states = (
            from t in typeof(CharacterState).Assembly.GetTypes() 
            where t.IsSubclassOf(typeof(CharacterState)) && !t.IsAbstract
            select character.StateFactory.Get(t)
        );
    
        List<string> unaccountedForStates = (
            from s in states
            where !animatorStateNames.Contains(s.Name)
            select s.Name
        ).ToList();

        Assert.IsTrue(
            unaccountedForStates.Count() == 0,
            $"Animation states not accounted for: {unaccountedForStates.Join(", ")}"
        );
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > duration || character.State.Name!=currentStateName) {
            currentStateName = character.State.Name;
            animator.CrossFade(currentStateName, .2f);
            duration = animator.GetCurrentAnimatorStateInfo(0).length;
            timer = 0f;
        }
    }

    private List<string> getAnimatorStateNames() {
        List<string> stateNames = new();
        AnimatorControllerLayer[] allLayer = animatorController.layers;

        for (int i = 0; i < allLayer.Length; i++) {
            ChildAnimatorState[] states = allLayer[i].stateMachine.states;

            for (int j = 0; j < states.Length; j++) {
                stateNames.Add(states[j].state.name);
            }
        }

        return stateNames;
    }

    
}
