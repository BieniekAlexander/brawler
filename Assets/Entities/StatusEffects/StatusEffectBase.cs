using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffectBase {
    public int duration;

    /// <summary>
    /// What to do when the status effect starts
    /// </summary>
    public abstract void Apply();

    /// <summary>
    /// What to do during each frame that the status effect is active
    /// </summary>
    public abstract void Tick();

    /// <summary>
    /// What to do when the status effect expires
    /// </summary>
    public abstract void Expire();
}
