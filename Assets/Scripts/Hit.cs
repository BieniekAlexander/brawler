using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Hit : MonoBehaviour
{
    /* Position */
    private Quaternion rotation;
    private Transform origin = null; // origin of the cast, allowing for movement during animation
    private Quaternion initialRotation;
    [SerializeField] public Vector3 offset;

    /* Collision */
    private Collider hitBox;
    [SerializeField] private Vector3[] trajectory;
    [SerializeField] private Vector3[] scaling;

    /* Duration */
    [SerializeField] public int duration = 3;

    /* Knockback */
    [SerializeField] private float knockbackMagnitude = 1f;
    [SerializeField] private Vector3 knockbackTransform;
    [SerializeField] private String knockbackTransformType = "ANGULAR"; // TODO name

    /* Effects */
    [SerializeField] private float damage = 40f;

    // Visualization
    //private ParticleSystem ps;
    private float animationDuration;

    private Vector3 GetKnockBackVector(Vector3 targetPosition) {
        Vector3 toTarget = targetPosition - transform.position;
        Vector3 knockBackDirection = (knockbackTransformType == "ANGULAR") ? Quaternion.Euler(knockbackTransform) * toTarget : knockbackTransform;
        knockBackDirection.y = 0f;
        return knockBackDirection.normalized*knockbackMagnitude;
    }

    public static Collider[] MyOverlap(Collider collider)
    {
        if (collider == null)
            throw new ArgumentException("Collider is null.");
        else if (collider is SphereCollider){
            SphereCollider sc = (SphereCollider)collider;
            return Physics.OverlapSphere(
                sc.transform.position,
                sc.radius);
        } else if (collider is CapsuleCollider) {
            CapsuleCollider cc = (CapsuleCollider)collider;
            return new Collider[0]; // TODO
            /*return Physics.OverlapCapsule(
                cc.transform.position+cc.direction*cc.height,
                cc.transform.position-cc.direction*cc.height,
                cc.radius);
            */
        } else if (collider is BoxCollider) {
            return new Collider[0]; // TODO
        } else {
            throw new ArgumentException("Unhandled collider type.");
        }
    }

    public void Initialize(Transform _origin)
    {
        origin = _origin;
        initialRotation = transform.rotation;
        hitBox = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        HandleMove();
        HandleHitCollisions();

        if (--duration <= 0)
            Destroy(gameObject);
    }

    private void HandleMove()
    {
        if (origin != null){
            transform.position = origin.position + origin.rotation * offset;
            Debug.Log(origin.rotation);
            Quaternion rotation = origin.rotation*initialRotation;
            transform.rotation = rotation;
        }
    }

    private void HandleHitCollisions()
    {
        foreach (Collider otherCollider in MyOverlap(hitBox))
        {
            GameObject go = otherCollider.gameObject;
            CharacterBehavior cb = go.GetComponent<CharacterBehavior>();

            if (cb)
            {
                cb.TakeDamage(damage, GetKnockBackVector(cb.transform.position), .1f);
            }
        }
    }
}
