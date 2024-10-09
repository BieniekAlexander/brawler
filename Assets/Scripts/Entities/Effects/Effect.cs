using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IStatusEffectMessage : IEventSystemHandler {
    void OnCast(CastId castId);
}

public abstract class Effect : MonoBehaviour, IStatusEffectMessage {
    [SerializeField] int tickRate;
    [SerializeField] public int _duration;
    [HideInInspector] public MonoBehaviour Target;
    private int frame;

    /// <summary>
    /// What to do when the status effect starts
    /// </summary>
    public virtual void Initialize(MonoBehaviour _target) {
        Target = _target;
    }

    /// <summary>
    /// Return whether this effect has any meaningful impact on the detected Monobehavior
    /// </summary>
    /// <param name="_target"></param>
    /// <returns></returns>
    public abstract bool CanEffect(MonoBehaviour _target);

    /// <summary>
    /// What to do during each frame that the status effect is active
    /// </summary>
    public virtual void Tick() {; }

    public void FixedUpdate() {
        if (++frame >= _duration) {
            if (Target != null) {
                // TODO I'm worried about target suddenly being null because of destruction
                // https://community.gamedev.tv/t/question-about-destroy-gameobject/176998/4
                Expire();
            }

            Destroy(gameObject);
        }

        if (tickRate>0 && frame%tickRate == 0 && Target != null) Tick();
    }

    /// <summary>
    /// What to do when the status effect expires
    /// </summary>
    public virtual void Expire() {; }

    public virtual void OnCast(CastId castId) {; }
}
