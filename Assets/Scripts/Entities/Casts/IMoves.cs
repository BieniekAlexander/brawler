using UnityEngine;

public interface IMoves {
    /// <summary/>
    /// <returns>the <typeparamref name="Transform"/></returns>
    public Transform GetTransform();

    /// <summary/>
    /// <returns>the <typeparamref name="Vector3"/>  velocity</returns>
    public Vector3 GetVelocity();

    /// <summary>
    /// Take in a <typeparamref name="CommandMovement"/>, which will temporarily update the movement of the <typeparamref name="IMoves"/>
    /// </summary>
    /// <param name="CommandMovement">The new, temporary movement</param>
    public void SetCommandMovement(CommandMovement CommandMovement);

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
    // TODO I hope I don't accidentally have tow knockback events classinfinitely recurse :) I should make the implementation more explicit about that
}
