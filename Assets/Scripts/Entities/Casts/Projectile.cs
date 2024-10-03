using UnityEngine;

public class Projectile : Castable, IMoves, ICollidable, IDamageable, ICasts
{
    /* Movement */
    private Vector3 Velocity;
    [SerializeField] float MaxSpeed = 15f;
    [SerializeField] float InitialOffset = 1f;
    [SerializeField] float RotationalControl = 120f; // I'll just give this the rocket behavior - if it can't rotate, it'll be a normal projectile

    /* Damage */
    [SerializeField] int HP = 100;

    // TODO currently, it seems to make the most sense to me that all projectile movement is directional,
    // but for absolute casts (e.g. an AOE stun), it works to act as a projectile,
    // so I'll cast AOEs and such as projectiles for now and change it if it no longer makes sense
    [SerializeField] Positioning Positioning = Positioning.Directional;
    private Quaternion InitialRotation;

    public void Start() {
        // TODO maybe make use of the Target transform instead? I took a while to debug this when trying to get the AI Agent to shoot,
        // because I was updating the Aim Cursor position, but not the transform's rotation, and so there's some redundancy in this implementation
        InitialRotation = transform.rotation;
        Vector3 Direction = Origin.rotation * Vector3.forward;
        Velocity = MaxSpeed * Direction;

        if (Positioning == Positioning.Directional) {
            transform.position = Origin.position + InitialOffset*Direction;
        } else { // Positioning == Positioning.Absolute
            float CastDistance = (Target.position - Origin.position).magnitude;
            transform.position = Origin.position + Mathf.Min(InitialOffset, CastDistance)*Direction;
            transform.rotation = Origin.rotation * transform.rotation;
        }
    }

    public override void FixedUpdate() {
        Move();
        HandleCollisions();

        if (Frame++ == Duration) {
            if (ConditionCastablesMap.TryGetValue(CastableCondition.OnExpire, out Castable[] value)) {
                foreach (Castable Castable in value) {
                    CreateCast(
                        Castable,
                        Caster,
                        transform,
                        null,
                        false
                    );
                }
            }
            Destroy(gameObject);
        }
    }

    /* IMoves Methods */
    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, int hitTier) {
        return 0f;
    }

    public void Move() {
        if (RotationalControl != 0) {
            // https://www.youtube.com/watch?v=Z6qBeuN-H1M
            Vector3 targetDirection = Target.position - transform.position;
            Quaternion targetRotation = Quaternion.FromToRotation(Velocity, targetDirection);
            
            Quaternion newRotation = Quaternion.RotateTowards(
                Quaternion.Euler(Velocity.normalized),
                targetRotation,
                RotationalControl * Time.deltaTime);

            Velocity = newRotation * Velocity.normalized * MaxSpeed * (240 - Quaternion.Angle(transform.rotation, targetRotation)) / 240;
            transform.rotation = Quaternion.LookRotation(Velocity.normalized, Vector3.up)*InitialRotation;
            Velocity.y = 0; // hack to lock rotation to horizontal plane
        }

        transform.position += Velocity * Time.deltaTime;
    }

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

    public void OnCollideWith(ICollidable other, CollisionInfo info) {
        if (
            other is Character Character
            || (other is Projectile Projectile && Vector3.Dot(Velocity, Projectile.Velocity)<=0) // if the rockets are relatively antiparallel, make them collide
        ) {
            foreach (Castable Castable in ConditionCastablesMap[CastableCondition.OnCollide]) {
                CreateCast(
                    Castable,
                    Caster,
                    transform,
                    null,
                    false
                );
            }

            // Might I not want a projectile to Destroy on collision? I can't think of a case, but maybe?
            Destroy(gameObject);
        }
    }

    public Collider GetCollider() {
        return GetComponent<Collider>();
    }

    public int TakeDamage(Vector3 contactPoint, int damage, HitTier hitTier) {
        HP -= damage;

        if (HP <= 0)
            OnDeath();

        return damage;
    }

    public int TakeHeal(int damage) {
        return 0;
    }

    public void OnDeath() {
        if (ConditionCastablesMap.ContainsKey(CastableCondition.OnDeath)) {
            foreach (Castable Castable in ConditionCastablesMap[CastableCondition.OnDeath]) {
                CreateCast(
                    Castable,
                    Caster,
                    transform,
                    null,
                    false
                );
            }
        }
        Destroy(gameObject);
    }
}