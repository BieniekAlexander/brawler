using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    /* Movement */
    private CharacterController _cc;
    private Vector3 velocity;
    private Character caster;
    public Transform target;
    [SerializeField] float rotationalControl = 120f;
    [SerializeField] float maxSpeed = 15f;
    [SerializeField] public int duration;
    public int frame;

    /* Target */
    [SerializeField] private Hit hit;

    public void Initialize(Character _caster, Transform _target) {
        target = _target;
        caster = _caster;
    }

    public void Start()
    {
        _cc = GetComponent<CharacterController>();
        velocity = (transform.rotation * Vector3.forward).normalized * maxSpeed;
        frame = 0;
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
        if (rotationalControl != 0) {
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

        if (++frame >= duration) {
            Resolve();
        }
    }
    
    private void handleCollisions()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Casts")) {
            return;
        } else {
            Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.x);

            foreach (Collider c in colliders) {
                if (c.gameObject == gameObject)
                    continue;
                if (c.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                    continue;

                Resolve();
                break;
            }
        }
    }

    public void UpdateTarget(Vector3 position) { 
        target.position = position;
    }

    private void OnDestroy() {
        if (target!=null && target.GetComponent<Character>() is null) {
            Destroy(target.gameObject);
        }
    }
}