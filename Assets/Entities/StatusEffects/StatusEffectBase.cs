using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class StatusEffectBase : MonoBehaviour {
    [SerializeField] int tickRate;
    [SerializeField] int duration;
    private int frame;
    public Character target;

    /// <summary>
    /// What to do when the status effect starts
    /// </summary>
    public virtual void Initialize(Character _character) {
        target = _character;
    }

    /// <summary>
    /// What to do during each frame that the status effect is active
    /// </summary>
    public abstract void Tick();

    public void FixedUpdate() {
        if (++frame >= duration) {
            Destroy(gameObject);
        }

        if (tickRate>0 && frame%tickRate == 0 && target != null) Tick();
    }

    /// <summary>
    /// What to do when the status effect expires
    /// </summary>
    public virtual void Expire() {; }

    public void OnDestroy() {
        if (target != null)
            Expire();
    }
}
