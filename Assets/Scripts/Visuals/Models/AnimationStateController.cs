using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ServiceStack;
using UnityEngine.Assertions;
using ServiceStack.Script;


public class AnimationStateController : MonoBehaviour
{
    public RuntimeAnimatorController animatorController;
    private Animator animator;
    private Character character;
    private float timer = 1f;
    private float duration = 0f;
    private string currentStateName = "";

    // pattern for setting animation in code https://www.youtube.com/watch?v=ZwLekxsSY3Y
    void Awake()
    {
        character = GetComponent<Character>();
        animator = GetComponent<Animator>();
        // TODO find some way of chceking that the state names are tracked by the animator
        // the below doesn't work because all of the animationclips are named Mixamo.org smh
        // animatorController = animator.runtimeAnimatorController;
        // IEnumerable<CharacterState> states = (
        //     from t in typeof(CharacterState).Assembly.GetTypes() 
        //     where t.IsSubclassOf(typeof(CharacterState)) && !t.IsAbstract
        //     select character.StateFactory.Get(t)
        // );
        // List<string> unaccountedForStates = (
        //     from s in states
        //     where !animator.runtimeAnimatorController.animationClips.Any(i => i.name==s.Name)
        //     select s.Name
        // ).ToList();
        // Assert.IsTrue(
        //     unaccountedForStates.Count() == 0,
        //     $"Animation states not accounted for: {unaccountedForStates.Join(", ")}"
        // );
    }

    /// <summary>
    /// Run the state-named animation on repeat until the state gets updated to something else :^)
    /// </summary>
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > duration || character.State.Name!=currentStateName) {
            currentStateName = character.State.Name;
            animator.CrossFade(currentStateName, 0.25f, 0);
            duration = animator.GetCurrentAnimatorStateInfo(0).length;
            duration = 1f;
            timer = 0f;
        }
    }

    

    
}
