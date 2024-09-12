using UnityEngine;
using UnityEngine.EventSystems;

public interface IStatusEffectMessage : IEventSystemHandler {
    void OnCast(CastId castId);
}

public abstract class StatusEffectBase : MonoBehaviour, IStatusEffectMessage {
    [SerializeField] int tickRate;
    [SerializeField] public int duration;
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
    public virtual void Tick() {; }

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

    public virtual void OnCast(CastId castId) {; }

    public void OnDestroy() {
        Expire();
    }
}
