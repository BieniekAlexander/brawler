using System.Collections.Generic;
using UnityEngine;

public abstract class CommandMovement : MonoBehaviour {
    [SerializeField] public int Duration;
    [SerializeField] public float Range;

    public Vector3 Path { get; set; }
    public int Frame { get; private set; } = 0;
    public List<Effect> Effects { get; private set; } = new();
    public IMoves Mover { get; private set; }
    public Transform Target { get; private set; }

    public virtual void Initialize(IMoves mover, Transform target) {
        Mover = mover;
        Target = target;
        Path = (target.position-mover.Transform.position).normalized
            * Mathf.Min(
                (target.position-mover.Transform.position).magnitude,
                Range
            );
    }

    public virtual Quaternion GetRotation(Vector3 _currentPosition, Quaternion _currentRotation) {
        return _currentRotation;
    }

    public virtual void FixedUpdate() {
        if (++Frame >= Duration) {
            Destroy(gameObject);
        }
    }

    public virtual void OnDestroy() {
        for (int i = 0; i < Effects.Count; i++){
            DestroyImmediate(Effects[i]);
        }
    }
}
