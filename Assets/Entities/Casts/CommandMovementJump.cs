using UnityEngine;

public class CommandMovementJump : CommandMovementBase
{
    [SerializeField] private float gravity;

    private void Awake() {
        
    }

    public override Vector3 GetDPosition() {
        /*
        velocity -= new Vector3(0f, gravity*Time.deltaTime, 0f);
        return velocity*Time.deltaTime;
        */
        return Vector3.left;
    }
}
