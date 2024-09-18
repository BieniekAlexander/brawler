using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    /* Movement */
    private CharacterController _cc;
    private Vector3 velocity;
    private Character Caster;
    private Transform origin;
    public Transform target;
    [SerializeField] float rotationalControl = 120f;
    [SerializeField] float maxSpeed = 15f;
    [SerializeField] public int duration;
    public int frame;

    /* Target */
    [SerializeField] private Hit hit;

    public static Projectile Initiate(Projectile _projectile, Character _caster, Transform _origin, Transform _target) {
        Projectile projectile = Instantiate(_projectile);
        projectile.Initialize(_caster, _origin, _target);
        return projectile;
    }

    public static Projectile Initiate(Projectile _projectile, Vector3 _position, Quaternion _rotation, Character _caster, Transform _origin, Transform _target) {
        Projectile projectile = Instantiate(_projectile, _position, _rotation);
        projectile.Initialize(_caster, _origin, _target);
        return projectile;
    }


    private void Initialize(Character _caster, Transform _origin, Transform _target) {
        Caster =  _caster;
        origin = _origin;
        target = _target;
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

        Hit.Initiate(
            hit,
            transform.position,
            transform.rotation,
            Caster,
            t,
            false // TODO mirror this at some point? idk
        );

        Destroy(gameObject);
    }

    private void Reflect(Character character) {

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

                Character ch = c.gameObject.GetComponent<Character>();

                if (ch!=null && ch.Reflects) {
                    Vector3 normal = (c.transform.position - transform.position);
                    Vector3 bounceDirection = (velocity-2*Vector3.Project(velocity, normal)).normalized;
                    velocity = bounceDirection * velocity.magnitude;
                } else {
                    Resolve();
                }
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