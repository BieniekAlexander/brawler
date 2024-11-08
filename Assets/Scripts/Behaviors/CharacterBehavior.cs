using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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
        strategy = factory.Punish();
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

        if (action == null) {
            action = strategy.GetAction(this);
        }

        action.ApplyTo(this, input);
        Character.LoadCharacterFrameInput(input);
    }
}