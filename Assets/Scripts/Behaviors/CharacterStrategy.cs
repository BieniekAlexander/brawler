using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Potential strategies:<br/>
/// - Rush  -> Rush in on an opponent<br/>
/// - Wait  -> Keep a safe distance from opponent<br/>
/// - Flee  -> Make more space from your opponent<br/>
/// </summary>
public class CharacterStrategyFactory {
    /// <summary>
    /// For each character state, describes the character's proclivity to transition to another strategy
    /// </summary>
    /// <remarks>
    /// E.g. if a character has a .001/1 chance of transitioning to <typeparamref name="CharacterStateFlee"/>,
    /// there's a .1% chance to switch to that strategy each frame, which averages out to happening once every 1/.001/60f=16.666 seconds
    /// </remarks>
    Dictionary<string, Dictionary<CharacterStateType, (float[], CharacterStrategy[])>> _transitionDict;
    Dictionary<string, CharacterStrategy> _strategyDict;

    public CharacterStrategyFactory() {
        _strategyDict = new() {
            {"Rush", new CharacterStrategyRush(this)},
            {"Wait", new CharacterStrategyWait(this)},
            {"Flee", new CharacterStrategyFlee(this)}
        };

        _transitionDict = new() {
            {
                "Rush", new() {
                    {CharacterStateType.ACTIVE, (new float[]{.994f, .005f, .001f}, new CharacterStrategy[]{null, Wait, Flee})},
                    {CharacterStateType.DISADVANTAGE, (new float[]{.992f, .004f, .004f}, new CharacterStrategy[]{null, Wait, Flee})},
                    {CharacterStateType.RECOVERY, (new float[]{.992f, .004f, .004f}, new CharacterStrategy[]{null, Wait, Flee})}
                }
            }, {
                "Wait", new() {
                    {CharacterStateType.ACTIVE, (new float[]{.997f, .002f, .001f}, new CharacterStrategy[]{null, Rush, Flee})},
                    {CharacterStateType.DISADVANTAGE, (new float[]{.99f, .001f, .009f}, new CharacterStrategy[]{null, Rush, Flee})},
                    {CharacterStateType.RECOVERY, (new float[]{.99f, .001f, .009f}, new CharacterStrategy[]{null, Rush, Flee})}
                }
            }, {
                "Flee", new() {
                    {CharacterStateType.ACTIVE, (new float[]{.99f, .008f, .002f}, new CharacterStrategy[]{null, Wait, Rush})},
                    {CharacterStateType.DISADVANTAGE, (new float[]{.99f, .009f, .001f}, new CharacterStrategy[]{null, Wait, Rush})},
                    {CharacterStateType.RECOVERY, (new float[]{.99f, .009f, .001f}, new CharacterStrategy[]{null, Wait, Rush})}
                }
            }
        };
    }

    public CharacterStrategy Rush { get {return _strategyDict["Rush"];}}
    public CharacterStrategy Wait { get {return _strategyDict["Wait"];}}
    public CharacterStrategy Flee { get {return _strategyDict["Flee"];}}

    /// <summary>
    /// Switch the Behavior's current strategy, given character behavior
    /// </summary>
    /// <param name="behavior"/>
    /// <returns></returns>
    public CharacterStrategy GetNewStrategy(CharacterBehavior behavior, CharacterStrategy currentStrategy) {
        Dictionary<CharacterStateType, (float[], CharacterStrategy[])> strategyTransitions = _transitionDict[currentStrategy.Name];

        if (strategyTransitions.ContainsKey(behavior.StateType)) {
            (float[], CharacterStrategy[]) transitionOptions = strategyTransitions[behavior.StateType];
            return RandomUtils.Choice(transitionOptions.Item1, transitionOptions.Item2);
        } else {
            return null;
        }
    }
}

public abstract class CharacterStrategy {
    protected CharacterStrategyFactory _factory;
    abstract public string Name { get; }

    public CharacterStrategy(CharacterStrategyFactory factory) {
        _factory = factory;
    }

    /// <summary>
    /// Get the next decision that the character should make, given some game state observations
    /// </summary>
    /// <param name="perspective"/>
    /// <returns></returns>
    public abstract CharacterAction GetAction(CharacterBehavior behavior);
}

public class CharacterStrategyRush: CharacterStrategy {
    override public string Name { get {return "Rush";} }
    public CharacterStrategyRush(CharacterStrategyFactory factory): base(factory) {}

    override public CharacterAction GetAction(CharacterBehavior behavior) {
        if (behavior.StateType == CharacterStateType.ACTIVE) {
            if (CharacterObservations.CloseToEnemy(behavior)) {
                return CharacterActionFactory.Attack();
            } else if (Random.Range(0f, 1f)<.5f) {
                return CharacterActionFactory.DashIn();
            } else {
                return CharacterActionFactory.Approach();
            }
        } else if (behavior.StateType == CharacterStateType.DISADVANTAGE) {
            return CharacterActionFactory.HastenGetUp();
        } else if (behavior.StateType == CharacterStateType.RECOVERY) {
            return CharacterActionFactory.Approach(); // TODO make get-up attack
        }

        throw new System.NotImplementedException($"Can't pick a valid action for {behavior.BehaviorSummary}");
    }
}


public class CharacterStrategyWait: CharacterStrategy {
    override public string Name { get {return "Wait";} }
    public CharacterStrategyWait(CharacterStrategyFactory factory): base(factory) {}

    override public CharacterAction GetAction(CharacterBehavior behavior) {
        if (behavior.StateType == CharacterStateType.ACTIVE) {
            if (CharacterObservations.CloseToEnemy(behavior) || Random.Range(0f, 1f)<.005f) { // E[x]=3.3s
                return CharacterActionFactory.Attack();
            } else {
                return CharacterActionFactory.Space();
            }
        } else if (behavior.StateType == CharacterStateType.DISADVANTAGE) {
            return CharacterActionFactory.Idle();
        } else if (behavior.StateType == CharacterStateType.RECOVERY) {
            return CharacterActionFactory.Space();
        }

        throw new System.NotImplementedException($"Can't pick a valid action for {behavior.BehaviorSummary}");
    }
}

public class CharacterStrategyFlee: CharacterStrategy {
    override public string Name { get {return "Flee";} }
    public CharacterStrategyFlee(CharacterStrategyFactory factory): base(factory) {}

    override public CharacterAction GetAction(CharacterBehavior behavior) {
        if (behavior.StateType == CharacterStateType.ACTIVE) {
            return CharacterActionFactory.BackPedal(); // TODO something else
        } else if (behavior.StateType == CharacterStateType.DISADVANTAGE) {
            return CharacterActionFactory.Idle(); // TODO something else
        } else if (behavior.StateType == CharacterStateType.RECOVERY) {
            return CharacterActionFactory.BackPedal(); // TODO something else
        }

        throw new System.NotImplementedException($"Can't pick a valid action for {behavior.BehaviorSummary}");
    }
}