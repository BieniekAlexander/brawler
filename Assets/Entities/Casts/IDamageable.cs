using UnityEngine;
using System;

[Serializable]
public enum HitTier {
    Feather = 0, // TODO I have absoutely no idea what to call this right now
    Light = 1,
    Medium = 2,
    Heavy = 3,
    Pure = 4
}

public interface IDamageable
{

    /// <summary>
    /// Take <paramref name="damage"/> damage and do some related behaviors, if applicable.
    /// </summary>
    /// <param name="damage">The amount of damage to take. Should be positive.</param>
    /// <param name="contactPoint">The contact point at which damage should be applied. Applicable for shield blocks, backstabs, etc.</param>
    void TakeDamage(Vector3 contactPoint, int damage, HitTier hitTier);

    /// <summary>
    /// Take <paramref name="damage"/> damage and do some related behaviors, if applicable.
    /// </summary>
    /// <remarks>
    /// I'm breaking this out into a function different from TakeDamage in the case that an <typeparamref name="IDamageable"/> shouldn't be healable, e.g. a projectile.
    /// </remarks>
    /// <param name="damage">The amount of damage to heal. Should be positive.</param>
    void TakeHeal(int damage);

    /// <summary>
    /// What to do when the <typeparamref name="IDamageable"/> takes excess damage.
    /// </summary>
    void OnDeath();
}
