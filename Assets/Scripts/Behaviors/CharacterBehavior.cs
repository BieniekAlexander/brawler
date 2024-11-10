using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterBehavior : MonoBehaviour {
    // Objects of interest
    public Character Character;
    public Character Enemy;
    public List<Cast> Threats;

    // state
    public CharacterStateType StateType {get {return Character.State.Type; }}

    // Strategy
    private CharacterStrategyFactory factory;
    private CharacterStrategy strategy;
    private CharacterAction action;
    private CharacterFrameInput input;

    // Context Based Steering
    public int steerEvaluationRate = 5; private int steerEvaluationTimer = 0;
    public List<float> SteerInterests = Enumerable.Repeat(0f, ContextSteering.SteerDirections.Count).ToList();
    
    /* MonoBehavior */
    public void Awake() {
        Character = GetComponent<Character>();
        factory = new();
        strategy = factory.Wait;
        input = new();
    }

    public void FixedUpdate() {
        // evaluate strategy
        CharacterStrategy newStrategy = factory.GetNewStrategy(this, strategy);
        if (newStrategy!=null) {
            strategy = newStrategy;
            action = null;
        }

        // decide an action, given the current strategy
        if (action == null || action.IsDone(this)) {
            action = strategy.GetAction(this);
        }

        action.ApplyTo(this, input);
        Character.LoadCharacterFrameInput(input);
    }

    /* Observation */
    public void ScanForThreats() {
        Threats = (
            from GameObject go in (
                GameObject.FindGameObjectsWithTag("Hit")
                .Union(GameObject.FindGameObjectsWithTag("Projectile"))
            ) select go.GetComponent<ICollidable>() as Cast
        ).ToList();
    }

    /* Debug */
    public string BehaviorSummary {get {
        return $@"
            Character={Character.name}
            State={Character.State}
        ";
    }}
}

public static class CharacterObservations {
    public static bool CloseToEnemy(CharacterBehavior behavior) {
        return (behavior.Character.transform.position-behavior.Enemy.transform.position).magnitude < 2f;
    }

    public static bool SpacedFromEnemy(CharacterBehavior behavior) {
        return (behavior.Character.transform.position-behavior.Enemy.transform.position).magnitude < 4f;
    }

    public static bool CastBufferEmpty(CharacterBehavior behavior) {
        return behavior.Character.InputCastId == -1;
    }

    public static bool NotDisadvantage(CharacterBehavior behavior) {
        return behavior.StateType != CharacterStateType.DISADVANTAGE;
    }

    public static bool FarFromEnemy(CharacterBehavior behavior) {
        return (behavior.Character.transform.position-behavior.Enemy.transform.position).magnitude > 6f;
    }
}