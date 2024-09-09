using UnityEngine;

public class CommandMovementBase : MonoBehaviour {
    public Vector3 path;
    public Vector3 velocity;
    public int frame = 0;
    [SerializeField] public int duration;
    [SerializeField] public float range;

    public virtual void Initialize(Vector3 _destination, Vector3 initialPosition) {
        path = (_destination-initialPosition).normalized*Mathf.Min((_destination-initialPosition).magnitude, range);
    }

    public virtual Vector3 GetDPosition() {
        return velocity;
    }

    public virtual void FixedUpdate() {
        if (++frame >= duration) {
            Destroy(gameObject);
        }
    }
}
