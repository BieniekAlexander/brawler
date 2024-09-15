using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.EventSystems;

public class CommandMovementDash: CommandMovementBase {
    [SerializeField] public float speed;

    public override void Initialize(Character mover, Vector3 _destination, Vector3 initialPosition) {
        base.Initialize(mover, _destination, initialPosition);
        Velocity = Path.normalized*speed*Time.deltaTime;
        duration = Mathf.FloorToInt((Path.magnitude/speed)/Time.fixedDeltaTime);
    }

    public override void OnDestroy() {
        GameObject cast = GetComponentInParent<CastBase>().gameObject;
        ExecuteEvents.Execute<ICastMessage>(cast, null, (c, data) => c.FinishCast());
        base.OnDestroy();
    }
}
