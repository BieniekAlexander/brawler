using UnityEngine;

public interface IMoves {
    /// <summary/>
    /// <returns>the <typeparamref name="Transform"/></returns>
    public Transform Transform { get; }

    public float BaseSpeed { get; set; }

    /// <summary>
    /// Getter/Setter of Velocity
    /// </summary>
    public Vector3 Velocity { get; set; }

    /// <summary>
    /// Getter/Setter of <typeparamref name="CommandMovement"/>, which will temporarily update the movement of the <typeparamref name="IMoves"/>
    /// </summary>
    public CommandMovement CommandMovement { get; set; }

    /// <summary>
    /// Knock back the <typeparamref name="IMoves"/> as specified.
    /// </summary>
    /// <param name="contactPoint">The point at which the hit contacted the <typeparamref name="IMoves"/></param>
    /// <param name="hitLagDuration">The duration of the hitlag, before knockback starts</param>
    /// <param name="knockBackVector">The vector (direction and magnitude) of the knockback</param>
    /// <param name="hitStunDuration">The duration of the recipient's hitStun (during which they can't modify movemnet as normal)</param>
    /// <param name="hitTier">The tier of the hit, applicable for hits getting shielded and such</param>
    /// <returns>
    /// The degree of knockback taken by the target - if less than 0, the caller shall receive some knockback
    /// </returns>
    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, HitTier hitTier);
    // TODO I hope I don't accidentally have two knockback events infinitely recurse :) I should make the implementation more explicit about that

    // Status effects
    public int EncumberedTimer { get; set; } // can't issue move command, but can run
    public int StunStack { get; set; } // stuns
    public Transform ForceMoveDestination { get; set; } // taunts, fears, etc.
}
