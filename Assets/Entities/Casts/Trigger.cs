using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that generically handles the changing of object states when the object collides with it
/// </summary>
public class Trigger : Castable, ICollidable {
    /* Transform */
    public enum CoordinateSystem { Polar, Cartesian };
    [SerializeField] public CoordinateSystem positionCoordinateSystem;
    [SerializeField] public Vector3[] positions;
    [SerializeField] public Quaternion[] rotations;
    [SerializeField] public Vector3[] dimensions;

    /* Collision */
    [HideInInspector] public Collider Collider;
    [SerializeField] public List<Effect> effects = new();
    [SerializeField] public bool RedundantCollisions = false;
    [SerializeField] public bool DissapearOnTrigger = false;
    private HashSet<ICollidable> CollisionLog = new();

    /* Utility Functions */
    public static GameObject GetClosestGameObject(GameObject reference, params GameObject[] others) {
        Array.Sort(
            others,
            (o1, o2) => { return ((reference.transform.position - o1.transform.position).sqrMagnitude).CompareTo((reference.transform.position - o2.transform.position).sqrMagnitude); }
        );

        return others[0];
    }

    /* State Handling */
    public void Awake() {
        Collider=GetComponent<Collider>();
    }

    public void Initiate(ICasts _caster, Transform _origin, Transform _target, bool _mirrored) {
        Caster = _caster;
        Origin = _origin;
        Target = _target;
        Mirror = _mirrored;
    }

    public void Start() {
        // Evaluate the position of the Trigger before it's accounted for in game logic
        Move();
    }

    public void FixedUpdate() {
        // TODO gonna leave out UpdateTransform because these might not be moving, but maybe they will
        if (Frame>=Duration) {
            Destroy(gameObject);
        }

        Move();
        HandleCollisions();
        Frame++;
    }
   
    /* IMoves Methods */
    public virtual void Move() {
        if (Origin==null) return; // TODO if a character dies during grab, this hits null refs, so this is my cheap fix

        int positionFrame = Mathf.Min(Frame, positions.Length-1);
        int rotationFrame = Mathf.Min(Frame, rotations.Length-1);
        int dimensionFrame = Mathf.Min(Frame, dimensions.Length-1);

        Vector3 offset = (positionCoordinateSystem==CoordinateSystem.Cartesian)
            ? positions[positionFrame]
            : Quaternion.Euler(0, positions[positionFrame].x, 0)*Vector3.forward*positions[positionFrame].z;

        Quaternion orientation = (positionCoordinateSystem==CoordinateSystem.Cartesian)
            ? Quaternion.identity
            : Quaternion.Euler(0, positions[positionFrame].x, 0);

        // TODO make sure that the calculations without an origin are correct
        if (Mirror) {
            offset.x *= -1;
            orientation.y *= -1;
        }

        transform.localScale = dimensions[dimensionFrame];
        transform.position = Origin.position+Origin.rotation*offset;
        transform.rotation = Origin.rotation*orientation*rotations[rotationFrame];
    }

    /// <summary>
    /// No-op for Triggers
    /// </summary>
    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, int hitTier) { return 0f;}

    /* ICollidable Methods */
    public Collider GetCollider() { return Collider; }

    public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this, CollisionLog);
    }

    public virtual void OnCollideWith(ICollidable other) {
        if (other is MonoBehaviour mono) {
            for (int i = 0; i < effects.Count; i++) {
                // TODO what if I don't want the status effect to stack?
                // maybe check if an effect of the same type is active, and if so, do some sort of resolution
                // e.g. if two slows are applied, refresh slow
                Effect effect = Instantiate(effects[i]);
                effect.Initialize(mono);
            }
        }
    }
}
