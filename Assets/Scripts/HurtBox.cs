using System;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    [SerializeField] private float damage = 40f;
    // [SerializeField] private float knockback = 1f;
    private Vector3 position;
    private Quaternion rotation;
    
    // Collision
    [SerializeField] private Collider collider;

    // Visual Effects
    [SerializeField] private Transform explosionPrefab;

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
                Debug.Log(go);
                cb.TakeDamage(damage);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject);
    }
}
