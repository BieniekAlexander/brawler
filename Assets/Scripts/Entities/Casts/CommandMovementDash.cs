using UnityEngine;
using UnityEngine.EventSystems;

public class CommandMovementDash: CommandMovement {
    [SerializeField] public float speed;

    public override void Initialize(IMoves mover, Transform target) {
        base.Initialize(mover, target);
        Velocity = Path.normalized*speed*Time.deltaTime;
        Duration = Mathf.FloorToInt((Path.magnitude/speed)/Time.fixedDeltaTime);
    }

    public override void OnDestroy() {
        GameObject cast = GetComponentInParent<Cast>().gameObject;
        ExecuteEvents.Execute<ICastMessage>(cast, null, (c, data) => c.FinishCast());
        base.OnDestroy();
    }
}
