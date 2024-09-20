using UnityEngine;
using UnityEngine.EventSystems;

public interface IStatusEffectMessage : IEventSystemHandler {
    void OnCast(CastId castId);
}

public abstract class EffectBase : MonoBehaviour, IStatusEffectMessage {
    [SerializeField] int tickRate;
    [SerializeField] public int duration;
    [HideInInspector] public Character target;
    private int frame;

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
            if (target != null) {
                // TODO I'm worried about target suddenly being null because of destruction
                // https://community.gamedev.tv/t/question-about-destroy-gameobject/176998/4
                Expire();
            }

            Destroy(gameObject);
        }

        if (tickRate>0 && frame%tickRate == 0 && target != null) Tick();
    }

    /// <summary>
    /// What to do when the status effect expires
    /// </summary>
    public virtual void Expire() {; }

    public virtual void OnCast(CastId castId) {; }
}
