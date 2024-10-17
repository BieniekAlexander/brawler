using System.Collections.Generic;
using UnityEngine;

public class Projectile : Castable, IMoves, ICollidable, IDamageable, ICasts {
    /* Movement */
    public Vector3 Velocity { get; set; } = new();
    public CommandMovement CommandMovement { get; set; }
    [SerializeField] float MaxSpeed = 15f;
    [SerializeField] float InitialOffset = 1f;
    [SerializeField] float RotationalControl = 120f; // I'll just give this the rocket behavior - if it can't rotate, it'll be a normal projectile

    // TODO currently, it seems to make the most sense to me that all projectile movement is directional,
    // but for absolute casts (e.g. an AOE stun), it works to act as a projectile,
    // so I'll cast AOEs and such as projectiles for now and change it if it no longer makes sense
    [SerializeField] Positioning Positioning = Positioning.Directional;
    private Quaternion InitialRotation;

    new public void Awake() {
        _collider = GetComponent<Collider>();
        base.Awake();
    }

    public void Start() {
        // TODO clean this up later - currently copying target's position to a new transform
        // because I'm having an issue where the Castable's parent cast expires, and the Target is destroyed
        GameObject go = new("Projectile Target");
        go.transform.position = Target.transform.position;
        Target = go.transform;

        InitialRotation = transform.rotation;
        Vector3 Direction = About.rotation * Vector3.forward;
        Velocity = MaxSpeed * Direction;

        if (Positioning == Positioning.Directional) {
            transform.position = About.position + InitialOffset*Direction;
        } else if (Positioning == Positioning.Absolute) { // Positioning == Positioning.Absolute
            float CastDistance = (Target.position - About.position).magnitude;
            transform.position = About.position + Mathf.Min(InitialOffset, CastDistance)*Direction;
            transform.rotation = About.rotation * transform.rotation;
        }
    }

    protected override void Tick() {
        Move();
        HandleCollisions();
    }

    protected override void OnExpire() {
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
    }

    public override bool OnRecast(Transform _target) {
        if (!Mathf.Approximately(RotationalControl, 0)) {
            Target.position = _target.position;
            return true;
        } else {
            // nothing to recast
            return false;
        }
    }

    /* IMoves Methods */
    public Transform ForceMoveDestination { get; set; } = null;
    public int StunStack { get { return 0; } set {; } } // do I want these to be stunnable? doubt it
    public float BaseSpeed { get { return MaxSpeed; } set { MaxSpeed = value; } }
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

            // force movement only in XZ
            Velocity = MovementUtils.inXZ(newRotation * Velocity.normalized * MaxSpeed * (240 - Quaternion.Angle(transform.rotation, targetRotation)) / 240);
            transform.rotation = Quaternion.LookRotation(Velocity.normalized, Vector3.up)*InitialRotation;
        }

        transform.position += Velocity * Time.deltaTime;
    }

    public Transform Transform { get { return transform; } }

    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, HitTier hitTier) {
        return 0f; // TODO I won't let this take knockback right now, but maybe!
    }

    /* ICollidable Methods */
    private Collider _collider;
    public int ImmaterialStack { get { return 0; } set {; } }

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

    public Collider Collider { get { return _collider; } }

    /* IDamageable */
    [SerializeField] private int _hp;
    public int HP { get { return _hp; } }
    public List<Armor> Armors { get { return null; } set {; } }
    public int ParryWindow { get { return 0; } set {; } }
    public int InvulnerableStack { get { return 0; } set {; } }
    public int EncumberedStack { get { return 0; } set {; } }

    public int TakeDamage(Vector3 contactPoint, int damage, HitTier hitTier) {
        _hp -= damage;

        if (_hp <= 0)
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

    public void TakeArmor(Armor armor) { ; }

    public override bool AppliesTo(MonoBehaviour mono) {
        return true;
    }
}