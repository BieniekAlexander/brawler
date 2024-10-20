using System.Collections.Generic;
using UnityEngine;

public class Projectile : Cast, IMoves, ICollidable, IDamageable, ICasts {
    /* Movement */
    public Vector3 Velocity { get; set; } = new();
    public CommandMovement CommandMovement { get; set; }
    [SerializeField] float RotationalControl = 120f; // I'll just give this the rocket behavior - if it can't rotate, it'll be a normal projectile

    new public void Awake() {
        _collider = GetComponent<Collider>();
        base.Awake();
    }

    override protected void OnInitialize() {
        base.OnInitialize();
        FieldExpressionParser.instance.RenderValue(this, baseSpeedExpression);
        Velocity = transform.rotation*Vector3.forward*BaseSpeed;

        if (Positioning == Positioning.Relative) {
            transform.position = About.position + transform.rotation * Vector3.forward * Range;
            transform.rotation = Quaternion.LookRotation(Target.position-About.position, Vector3.up);
        } else if (Positioning == Positioning.Absolute) {
            float CastDistance = (Target.position - About.position).magnitude;
            transform.position = About.position + Mathf.Min(Range, CastDistance)*(About.rotation * Vector3.forward);
            transform.rotation = About.rotation * transform.rotation;
        }
    }

    protected override void Tick() {
        Move();
        HandleCollisions();
    }

    protected override bool OnRecast(Transform target) {
        if (!Mathf.Approximately(RotationalControl, 0)) {
            Target.position = target.position;
            return true;
        } else {
            // nothing to recast
            return false;
        }
    }

    /* IMoves Methods */
    [SerializeField] private FieldExpression<Projectile, float> baseSpeedExpression = new("0");
    public float BaseSpeed { get { return baseSpeedExpression.Value; } set { baseSpeedExpression.Value = value; } }
    public Transform ForceMoveDestination { get; set; } = null;
    public int StunStack { get { return 0; } set {; } } // do I want these to be stunnable? doubt it
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
            Velocity = MovementUtils.inXZ(newRotation * Velocity.normalized * BaseSpeed * (240 - Quaternion.Angle(transform.rotation, targetRotation)) / 240);
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
            OnCollision();
        }
    }

    public Collider Collider { get { return _collider; } }

    /* IDamageable */
    [SerializeField] private int _hp;
    public int HP { get { return _hp; } }
    public List<Armor> Armors { get { return null; } set {; } }
    public int ParryWindow { get { return 0; } set {; } }
    public int InvulnerableStack { get { return 0; } set {; } }
    public int EncumberedTimer { get { return 0; } set {; } }

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
            foreach (Cast Castable in ConditionCastablesMap[CastableCondition.OnDeath]) {
                Initiate(
                    Castable,
                    Caster,
                    transform,
                    null,
                    Mirrored,
                    this
                );
            }
        }
        Destroy(gameObject);
    }

    public void TakeArmor(Armor armor) { ; }

    public override bool AppliesTo(GameObject go) {
        return true;
    }

    override protected void OnDestruction() {
        base.OnDestruction();
    }
}