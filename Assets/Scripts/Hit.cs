using System;
using System.Collections.Generic;
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
    public HashSet<CharacterBehavior> collisionIds = new HashSet<CharacterBehavior>();
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

    public void Awake()
    {
        hitBox = GetComponent<Collider>();
        
        // resize wireframe for vizualization
        if (hitBox is CapsuleCollider) {
            CapsuleCollider collider = hitBox as CapsuleCollider;
            collider.transform.localScale = new Vector3(collider.radius, collider.height, collider.radius);
        } else if (hitBox is SphereCollider) {
            SphereCollider collider = hitBox as SphereCollider;
            collider.transform.localScale = new Vector3(collider.radius, collider.radius, collider.radius);
        } else {
            throw new Exception("Unhandled wireframe resizing");
        }
    }

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
    }

    private void FixedUpdate()
    {
        HandleMove();

        if (--duration <= 0)
            Destroy(gameObject);
    }

    public void HandleMove()
    {
        if (origin != null){
            transform.position = origin.position + origin.rotation * offset;
            transform.rotation = origin.rotation * initialRotation;
        }
    }

    public Collider[] GetColliders()
    {
        return MyOverlap(hitBox);
    }

    public void HandleHitCollisions()
    {
        foreach (Collider otherCollider in GetColliders())
        {
            GameObject go = otherCollider.gameObject;
            CharacterBehavior cb = go.GetComponent<CharacterBehavior>();

            if (cb is not null && !collisionIds.Contains(cb))
            {
                collisionIds.Add(cb);
                Vector3 kb = GetKnockBackVector(cb.transform.position);
                cb.TakeDamage(damage, kb, .1f);
            }
        }
    }
}
