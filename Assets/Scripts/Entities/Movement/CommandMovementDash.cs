using Codice.Client.BaseCommands;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class CommandMovementDash: CommandMovement {
    [SerializeField] public float speed;
    private float initialSpeed;

    public override void Initialize(IMoves mover, Transform target) {
        base.Initialize(mover, target);
        initialSpeed = Mathf.Min(mover.Velocity.magnitude, mover.BaseSpeed);
        mover.Velocity = Path.normalized*Mathf.Max(initialSpeed, speed);
        Duration = Mathf.FloorToInt((Path.magnitude/speed)/Time.fixedDeltaTime);
    }

    public override void OnDestroy() {
        if (GetComponentInParent<Cast>() is Cast parentCast) {
            GameObject go = GetComponentInParent<Cast>().gameObject;
            ExecuteEvents.Execute<ICastMessage>(go, null, (c, data) => c.FinishCast());
        }

        Mover.Velocity = Vector3.ClampMagnitude(Mover.Velocity, initialSpeed);
        base.OnDestroy();
    }
}
