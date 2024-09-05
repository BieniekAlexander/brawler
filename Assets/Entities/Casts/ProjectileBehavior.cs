using System.Drawing;
using System.Transactions;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

public class ProjectileBehavior : MonoBehaviour
{
    /* Movement */
    private CharacterController _cc;
    private Vector3 velocity;
    private CharacterBehavior caster;
    public Transform target;
    [SerializeField] private int duration;
    [SerializeField] float rotationalControl = 120f;
    [SerializeField] float maxSpeed = 15f;

    /* Target */
    [SerializeField] private Hit hit;

    public void Initialize(CharacterBehavior _caster, Transform _target) {
        target = _target;
        caster = _caster;
    }

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
        velocity = (transform.rotation * Vector3.forward).normalized * maxSpeed;
    }

    private void Resolve()
    {
        Transform t = new GameObject("HitBox Origin").transform;
        t.position = transform.position;

        Instantiate(
            hit,
            transform.position,
            transform.rotation
        ).Initialize(
            caster,
            t,
            false // TODO mirror this at some point? idk
        );

        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            // source: https://www.youtube.com/watch?v=Z6qBeuN-H1M
            Vector3 targetDirection = target.position - _cc.transform.position;
            var targetRotation = Quaternion.FromToRotation(velocity, targetDirection);
            transform.rotation = Quaternion.RotateTowards(
                Quaternion.Euler(velocity.normalized),
                targetRotation,
                rotationalControl * Time.deltaTime
            );

            velocity = transform.rotation * velocity.normalized * maxSpeed * (180 - Quaternion.Angle(transform.rotation, targetRotation)) / 180;
            velocity.y = 0;
        }

        _cc.Move(velocity * Time.deltaTime);
        handleCollisions();

        if (--duration <= 0) {
            Resolve();
        }
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

            Resolve();
            break;
        }
    }

    public void UpdateTarget(Vector3 position) {
        target.position = position;
    }

    private void OnDestroy() {
        if (target!=null && target.GetComponent<CharacterBehavior>() is null) {
            Destroy(target.gameObject);
        }
    }
}