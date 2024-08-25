using System;
using UnityEngine;

[Serializable]
public class Hit : MonoBehaviour
{
    /* Position */
    private Vector3 position;
    private Quaternion rotation;
    public Transform origin; // origin of the cast, allowing for movement during animation
    [SerializeField] private Vector3[] trajectory;

       
    /* Damage */
    [SerializeField] private float damage = 40f;

    /* Knockback */
    [SerializeField] private float knockbackMagnitude = 1f;
    [SerializeField] private Vector3 knockbackTransform;
    [SerializeField] private String knockbackTransformType = "ANGULAR"; // TODO name

    // Collision
    [SerializeField] private Collider HitBox;

    // Visualization
    private ParticleSystem ps;
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

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        animationDuration = ps.main.duration;
    }

    public void Initialize(Vector3 _position, Quaternion _rotation)
    {
        position = _position;
        rotation = _rotation;

        HitBox.transform.position = position;
    
        foreach (Collider otherCollider in MyOverlap(HitBox))
        {
            GameObject go = otherCollider.gameObject;
            CharacterBehavior cb = go.GetComponent<CharacterBehavior>();

            if (cb)
            {
                cb.TakeDamage(damage, GetKnockBackVector(cb.transform.position), .1f);
                // TODO produce knockback
            }
        }
    }
}
