using UnityEngine;

public class CommandMovementBase : MonoBehaviour {
    private Vector3 path;
    public Vector3 velocity;
    private int frame = 0;
    private int duration;
    [SerializeField] public float range;
    [SerializeField] public float speed;

    public virtual void Initialize(Vector3 _destination, Vector3 initialPosition) {
        path = (_destination-initialPosition).normalized*Mathf.Min((_destination-initialPosition).magnitude, range);
        path.y=0;
        velocity = path.normalized*speed*Time.deltaTime;
        duration = Mathf.FloorToInt((path.magnitude/speed)/Time.fixedDeltaTime);
        Debug.Log(duration);
        Debug.Log(path);
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
