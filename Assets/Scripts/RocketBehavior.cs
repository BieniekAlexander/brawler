using System.Drawing;
using System.Transactions;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

public class RocketBehavior : MonoBehaviour
{
    /* Movement */
    private CharacterController _cc;
    private Vector3 velocity;
    public Transform transTarget;
    [SerializeField] float rotationalControl = 120f;
    [SerializeField] float maxSpeed = 7.5f;

    /* Target */
    [SerializeField] private Hit hit;

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
        handleCollisions();
    }

    private void handleCollisions()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.x);

        foreach (Collider c in colliders)
        {
            if (c.gameObject == gameObject)
                continue;
            if (c.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                continue;
            
            Quaternion rotation = Quaternion.Euler(Vector3.zero);
            Hit h = Instantiate(
                hit,
                transform.position,
                rotation
            );

            h.Initialize(null);

            Destroy(gameObject);
            break;
        }
    }
}