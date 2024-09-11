using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public abstract class CommandMovementBase : MonoBehaviour {
    [SerializeField] public int duration;
    [SerializeField] public float range;
    [SerializeField] public List<StatusEffectBase> statusEffectPrefabs;

    public Vector3 path;
    public Vector3 velocity;
    public int frame = 0;
    public List<StatusEffectBase> effects = new();


    public virtual void Initialize(Character mover,  Vector3 _destination, Vector3 initialPosition) {
        path = (_destination-initialPosition).normalized*Mathf.Min((_destination-initialPosition).magnitude, range);

        for (int i = 0; i < statusEffectPrefabs.Count; i++) {
            StatusEffectBase effect = Instantiate(statusEffectPrefabs[i]);
            effect.Initialize(mover);
            effects.Add(effect);
        }
    }

    public virtual Vector3 GetDPosition() {
        return velocity;
    }

    public virtual void FixedUpdate() {
        if (++frame >= duration) {
            Destroy(gameObject);
        }
    }

    public virtual void OnDestroy() {
        for (int i = 0; i < effects.Count; i++){
            DestroyImmediate(effects[i]);
        }
    }
}
