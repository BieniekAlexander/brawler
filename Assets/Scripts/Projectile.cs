using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody _rb;
    private float projectileSpeed = 3f;
    private float duration = 1.5f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 direction)
    {
        _rb.velocity = direction * projectileSpeed;
    }

    void FixedUpdate()
    {
        duration -= Time.deltaTime;

        if (duration <= 0) {
            Destroy(gameObject);
        }
    }
}
