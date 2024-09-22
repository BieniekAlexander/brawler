using UnityEngine;

public class Projectile : Castable, IMoves, ICollidable
{
    /* Movement */
    private Vector3 Velocity;
    [SerializeField] float MaxSpeed = 15f;
    [SerializeField] float InitialOffset = 15f;
    [SerializeField] float RotationalControl = 120f; // I'll just give this the rocket behavior - if it can't rotate, it'll be a normal projectile

    // TODO currently, it seems to make the most sense to me that all projectile movement is directional,
    // but for absolute casts (e.g. an AOE stun), it works to act as a projectile,
    // so I'll cast AOEs and such as projectiles for now and change it if it no longer makes sense
    [SerializeField] Positioning Positioning = Positioning.Directional;

    /// </summary/>
    /// <param name="_caster"></param>
    /// <param name="_origin"></param>
    /// <param name="_target"></param>
    /// <param name="_mirrored"></param>
    public override void Initialize(ICasts _caster, Transform _origin, Transform _target, bool _mirrored) {
        base.Initialize(_caster, _origin, _target, _mirrored);
        Vector3 Direction = Target.position - Origin.position;
        Velocity = MaxSpeed * Direction;

        if (Positioning == Positioning.Directional) {
            transform.position = Origin.position + InitialOffset*Direction;
        } else { // Positioning == Positioning.Absolute
            float CastDistance = (Target.position - Origin.position).magnitude;
            transform.position = Mathf.Min(InitialOffset, CastDistance)*Direction;
        }
    }

    public override void FixedUpdate() {
        Move();
        HandleCollisions();

        if (Frame == Duration) {
            Destroy(gameObject);
        }

        Frame++;
    }

    /* IMoves Methods */
    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, int hitTier) {
        return 0f;
    }

    public void Move() {
        transform.position += Velocity;
    }
    /* TODO code ported from old Rocket implementation
    private void FixedUpdate() {
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
    */

    public Transform GetTransform() { return transform; }

    public Vector3 GetVelocity() { return Velocity; }

    public void SetCommandMovement(CommandMovement CommandMovement) {
        // TODO implement this
        return;
    }

    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, HitTier hitTier) {
        return 0f; // TODO I won't let this take knockback right now, but maybe!
    }

    /* ICollidable Methods */
    public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this, null); // TODO I don't konw if it makes sense for this to have a collision log
    }


    private void Resolve()
    {
        Transform t = new GameObject("HitBox Origin").transform;
        t.position = transform.position;
        /*
        Hit.Initiate(
            hit,
            transform.position,
            transform.rotation,
            Caster,
            t,
            false // TODO mirror this at some point? idk
        );*/

        Destroy(gameObject);
    }

    public void OnCollideWith(ICollidable other) {
        if (other is Character Character) {

        }
    }

    public Collider GetCollider() {
        return GetComponent<Collider>();
    }
}