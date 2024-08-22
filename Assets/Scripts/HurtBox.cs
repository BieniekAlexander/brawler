using System;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    /* Position */
    private Vector3 position;
    private Quaternion rotation;
       
    /* Damage */
    [SerializeField] private float damage = 40f;

    /* Knockback */
    [SerializeField] private float knockbackMagnitude = 1f;
    [SerializeField] private Vector3 knockbackTransform;
    [SerializeField] private String knockbackTransformType = "ANGULAR"; // TODO name

    // Collision
    [SerializeField] private Collider collider;

    // Visual Effects
    [SerializeField] private Transform explosionPrefab;

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

    public void Initialize(Vector3 _position, Quaternion _rotation)
    {
        position = _position;
        rotation = _rotation;

        Instantiate(
            explosionPrefab,
            position, 
            rotation
        );

        collider.transform.position = position;
    
        foreach (Collider otherCollider in MyOverlap(collider))
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

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject);
    }
}
