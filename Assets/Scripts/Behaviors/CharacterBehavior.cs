using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterBehavior : MonoBehaviour {
    // Objects of interest
    public Character Character;
    public Character Enemy;
    public List<Cast> Threats;

    // Strategy
    private CharacterStrategyFactory factory;
    private CharacterStrategy strategy;
    private CharacterAction action;
    private CharacterFrameInput input;

    // Context Based Steering
    public int steerEvaluationRate = 5; private int steerEvaluationTimer = 0;
    public List<float> SteerInterests = Enumerable.Repeat(0f, ContextSteering.SteerDirections.Count).ToList();
    
    public void Awake() {
        Character = GetComponent<Character>();
        factory = new();
        strategy = factory.Wait();
        input = new();
    }

    public void ScanForThreats() {
        Threats = (
            from GameObject go in (
                GameObject.FindGameObjectsWithTag("Hit")
                .Union(GameObject.FindGameObjectsWithTag("Projectile"))
            ) select go.GetComponent<ICollidable>() as Cast
        ).ToList();
    }

    public void FixedUpdate() {
        CharacterStrategy newStrategy = strategy.GetNewStrategy(this);

        if (newStrategy!=null) {
            strategy = newStrategy;
        }

        if (action == null || action.IsDone(this)) {
            action = strategy.GetAction(this);
        }

        action.ApplyTo(this, input);
        Character.LoadCharacterFrameInput(input);
    }
}

public static class CharacterObservations {
    public static bool CloseToEnemy(CharacterBehavior state) {
        return (state.Character.transform.position-state.Enemy.transform.position).magnitude < 2f;
    }

    public static bool SpacedFromEnemy(CharacterBehavior state) {
        return (state.Character.transform.position-state.Enemy.transform.position).magnitude < 4f;
    }

    public static bool CastBufferEmpty(CharacterBehavior state) {
        return state.Character.InputCastId == -1;
    }
}