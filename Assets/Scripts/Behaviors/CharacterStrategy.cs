using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Potential strategies:<br/>
/// - Neutral:<br/>
///     - Rush      -> Rush in on a passive opponent and try to hit<br/>
///     - Poke      -> Preemptively stuff your opponent's approaches<br/>
///     - Punish    -> catch your opponent's preemptive attack with a punish<br/>
/// - Advantage:<br/>
///     - Pursue    -> Continue attacking your opponent to press the advantage<br/>
///     - Reset     -> reestablish a neutral state, setting up for an advantage<br/>
/// - Escape:<br/>
///     - Flee      -> Get away from your opponent to reset to neutral<br/>
///     - Reverse   -> Punish your opponent's pursuit to put them in disadvantage<br/>
/// </summary>
public class CharacterStrategyFactory {
    Dictionary<string, CharacterStrategy> _strategyDict = new Dictionary<string, CharacterStrategy>();

    public CharacterStrategyFactory() {
        // Neutral
        _strategyDict["Rush"] = new CharacterStrategyRush(this);
        _strategyDict["Poke"] = new CharacterStrategyPoke(this);
        _strategyDict["Punish"] = new CharacterStrategyPunish(this);
        // Advantage
        _strategyDict["Pursue"] = new CharacterStrategyPursue(this);
        _strategyDict["Reset"] = new CharacterStrategyReset(this);
        // Escape
        _strategyDict["Flee"] = new CharacterStrategyFlee(this);
        _strategyDict["Reverse"] = new CharacterStrategyReverse(this);
    }

    public CharacterStrategy Rush() => _strategyDict["Rush"];
    public CharacterStrategy Poke() => _strategyDict["Poke"];
    public CharacterStrategy Punish() => _strategyDict["Punish"];
    public CharacterStrategy Pursue() => _strategyDict["Pursue"];
    public CharacterStrategy Reset() => _strategyDict["Reset"];
    public CharacterStrategy Flee() => _strategyDict["Flee"];
    public CharacterStrategy Reverse() => _strategyDict["Reverse"];
}

public abstract class CharacterStrategy {
    CharacterStrategyFactory Factory;

    public CharacterStrategy(CharacterStrategyFactory factory) {
        Factory = factory;
    }

    /// <summary>
    /// Get the next decision that the character should make, given some game state observations
    /// </summary>
    /// <param name="perspective"/>
    /// <returns></returns>
    public abstract CharacterAction GetAction(CharacterBehavior state);

    /// <summary>
    /// Switch the Behavior's current strategy, given some observations, plus some randomness
    /// </summary>
    /// <param name="perspective"/>
    /// <returns></returns>
    public abstract CharacterStrategy GetNewStrategy(CharacterBehavior state);
}

public class CharacterStrategyRush: CharacterStrategy {
    public CharacterStrategyRush(CharacterStrategyFactory factory): base(factory) {}
    override public CharacterAction GetAction(CharacterBehavior state) => null;
    override public CharacterStrategy GetNewStrategy(CharacterBehavior state) => null;
}

public class CharacterStrategyPoke: CharacterStrategy {
    public CharacterStrategyPoke(CharacterStrategyFactory factory): base(factory) {}
    override public CharacterAction GetAction(CharacterBehavior state) => null;
    override public CharacterStrategy GetNewStrategy(CharacterBehavior state) => null;
}

public class CharacterStrategyPunish: CharacterStrategy {
    public CharacterStrategyPunish(CharacterStrategyFactory factory): base(factory) {}

    override public CharacterAction GetAction(CharacterBehavior state) {
        return CharacterActionFactory.Approach();
    }

    override public CharacterStrategy GetNewStrategy(CharacterBehavior state) => null;
}

public class CharacterStrategyPursue: CharacterStrategy {
    public CharacterStrategyPursue(CharacterStrategyFactory factory): base(factory) {}
    override public CharacterAction GetAction(CharacterBehavior state) => null;
    override public CharacterStrategy GetNewStrategy(CharacterBehavior state) => null;
}

public class CharacterStrategyReset: CharacterStrategy {
    public CharacterStrategyReset(CharacterStrategyFactory factory): base(factory) {}
    override public CharacterAction GetAction(CharacterBehavior state) => null;
    override public CharacterStrategy GetNewStrategy(CharacterBehavior state) => null;
}

public class CharacterStrategyFlee: CharacterStrategy {
    public CharacterStrategyFlee(CharacterStrategyFactory factory): base(factory) {}
    override public CharacterAction GetAction(CharacterBehavior state) => null;
    override public CharacterStrategy GetNewStrategy(CharacterBehavior state) => null;
}

public class CharacterStrategyReverse: CharacterStrategy {
    public CharacterStrategyReverse(CharacterStrategyFactory factory): base(factory) {}
    override public CharacterAction GetAction(CharacterBehavior state) => null;
    override public CharacterStrategy GetNewStrategy(CharacterBehavior state) => null;
}
