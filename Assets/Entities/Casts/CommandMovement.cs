using System.Collections.Generic;
using UnityEngine;

public abstract class CommandMovement : MonoBehaviour {
    [SerializeField] public int duration;
    [SerializeField] public float range;

    public Vector3 Path { get; set; }
    public Vector3 Velocity { get; set; }
    public int Frame { get; private set; } = 0;
    public List<Effect> Effects { get; private set; } = new();

    public virtual void Initialize(IMoves mover, Transform target) {
        Path = (target.position-mover.GetTransform().position).normalized
            * Mathf.Min(
                (target.position-mover.GetTransform().position).magnitude,
                range
            );
    }

    public virtual Vector3 GetDPosition() {
        return Velocity;
    }

    public virtual Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return _currentRotation;
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
