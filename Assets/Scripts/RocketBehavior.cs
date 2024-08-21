using System.Drawing;
using System.Transactions;
using UnityEngine;

public class RocketBehavior : MonoBehaviour
{
    /* Movement */
    private CharacterController _cc;
    private Vector3 velocity;
    [SerializeField] public Transform transTarget;
    [SerializeField] float rotationalControl = 120f;
    [SerializeField] float maxSpeed = 7.5f;

    /* Target */
    [SerializeField] private HurtBox hurtBox;

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
        velocity = (transTarget.position - _cc.transform.position).normalized * maxSpeed;
    }

    private void FixedUpdate()
    {
        if (transTarget != null)
        {
            // source: https://www.youtube.com/watch?v=Z6qBeuN-H1M
            Vector3 targetDirection = transTarget.position - _cc.transform.position;
            var targetRotation = Quaternion.FromToRotation(velocity, targetDirection);
            Quaternion currentRotation = Quaternion.RotateTowards(
                Quaternion.Euler(velocity.normalized),
                targetRotation,
                rotationalControl * Time.deltaTime
            );

            velocity = currentRotation * velocity.normalized * maxSpeed * (180 - Quaternion.Angle(transform.rotation, targetRotation)) / 180;
            velocity.y = 0;
        }

        _cc.Move(velocity * Time.deltaTime);
    }

private void OnControllerColliderHit(ControllerColliderHit hit)
{
        Vector3 contactCoordinates = hit.collider.ClosestPoint(_cc.transform.position);

        //Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        Quaternion rotation = Quaternion.Euler(Vector3.zero);
        Instantiate(
            hurtBox,
            contactCoordinates,
            rotation
        ).Initialize(contactCoordinates, rotation);
        
        Destroy(gameObject);
    }
}