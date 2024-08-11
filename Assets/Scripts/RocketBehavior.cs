using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class RocketBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody _rb;
    [SerializeField] public Transform transTarget;
    [SerializeField] float rotationalControl = 120f;
    [SerializeField] float maxSpeed = 3f;

    // Visual Effects
    public Transform explosionPrefab;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = (transTarget.position - _rb.position).normalized * maxSpeed;
    }

    private void FixedUpdate()
    {
        if (transTarget != null)
        {
            // source: https://www.youtube.com/watch?v=Z6qBeuN-H1M
            Vector3 direction = transTarget.position - _rb.position;
            var rotation = Quaternion.LookRotation(direction);
            _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, rotationalControl * Time.deltaTime));
            _rb.velocity = transform.forward * maxSpeed * (180 - Quaternion.Angle(transform.rotation, rotation)) / 180;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion.FromToRotation(Vector3.up, contact.normal);

        Instantiate(
            explosionPrefab,
            contact.point,
            Quaternion.FromToRotation(
                Vector3.up,
                contact.normal
            )
        );

        Destroy(collision.gameObject);
        Destroy(gameObject);
    }
}