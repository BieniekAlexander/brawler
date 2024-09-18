using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public abstract class CommandMovementBase : MonoBehaviour {
    [SerializeField] public int duration;
    [SerializeField] public float range;

    public Vector3 Path { get; set; }
    public Vector3 Velocity { get; set; }
    public int Frame { get; private set; } = 0;
    public List<StatusEffectBase> Effects { get; private set; } = new();

    public virtual void Initialize(Character mover,  Vector3 _destination, Vector3 initialPosition) {
        Path = (_destination-initialPosition).normalized*Mathf.Min((_destination-initialPosition).magnitude, range);
    }

    public virtual Vector3 GetDPosition() {
        return Velocity;
    }

    public virtual void FixedUpdate() {
        if (++Frame >= duration) {
            Destroy(gameObject);
        }
    }

    public virtual void OnDestroy() {
        for (int i = 0; i < Effects.Count; i++){
            DestroyImmediate(Effects[i]);
        }
    }
}
