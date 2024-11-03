using System.Collections.Generic;
using UnityEngine;

public class Projectile : Cast, IMoves, ICollidable, IDamageable, ICasts {
    /* Movement */
    public Vector3 Velocity { get; set; } = new();
    public CommandMovement CommandMovement { get; set; }
    [SerializeField] float RotationalControl = 2f; // I'll just give this the rocket behavior - if it can't rotate, it'll be a normal projectile

    new public void Awake() {
        _collider = GetComponent<Collider>();
        base.Awake();
    }

    override protected void OnInitialize() {
        base.OnInitialize();
        FieldExpressionParser.instance.RenderValue(this, baseSpeedExpression);
        Velocity = Caster.GetOriginTransform().rotation*Vector3.forward*BaseSpeed;
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

            transform.rotation = Quaternion.RotateTowards(
                Quaternion.Euler(Velocity.normalized),
                targetRotation,
                RotationalControl);

            // force movement only in XZ
            Velocity = MovementUtils.inXZ(transform.rotation * Velocity.normalized * BaseSpeed * (240 - Quaternion.Angle(transform.rotation, targetRotation)) / 240);
        }

        transform.position += Velocity;
    }

    public Transform Transform { get { return transform; } }

    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, HitTier hitTier) {
        return 0f; // TODO I won't let this take knockback right now, but maybe!
    }

    /* ICollidable Methods */
    public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this, null, 0); // TODO I don't konw if it makes sense for this to have a collision log
    }

    override public bool OnCollideWith(ICollidable other, CollisionInfo info) {
        if (
            other is Character Character
            || (other is Projectile Projectile && Vector3.Dot(Velocity, Projectile.Velocity)<=0) // if the rockets are relatively antiparallel, make them collide
        ) {
            return base.OnCollideWith(other, info);
        } else if (other is StageTerrain terrain) {
            Velocity = MovementUtils.GetBounce(Velocity, info.Normal);
            return true;
        } else {
            return false;
        }
    }

    /* IDamageable */
    [SerializeField] private int _hp;
    public int HP { get { return _hp; } }
    public int AP { get { return 0; } }
    public List<Armor> Armors { get { return null; } set {; } }
    public int ParryWindow { get { return 0; } set {; } }
    public int InvulnerableStack { get { return 0; } set {; } }
    public int EncumberedStack { get { return 0; } set {; } }

    public int TakeDamage(Vector3 contactPoint, in int damage, HitTier hitTier) {
        int remainingDamage = damage;
        _hp -= damage;

        if (_hp <= 0)
            OnDeath();

        return damage-remainingDamage;
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

    override protected void OnDestruction() {
        base.OnDestruction();
    }
}