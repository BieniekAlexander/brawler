using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Class that generically handles the changing of object states when the object collides with it
/// </summary>
public class Trigger : MonoBehaviour {
    /* Origin */
    public Character Caster;
    public Transform Origin; // origin of the cast, allowing for movement during animation

    /* Transform */
    public enum CoordinateSystem { Polar, Cartesian };
    [HideInInspector] public bool Mirror = false;
    [SerializeField] public CoordinateSystem positionCoordinateSystem;
    [SerializeField] public Vector3[] positions;
    [SerializeField] public Quaternion[] rotations;
    [SerializeField] public Vector3[] dimensions;

    /* Collision */
    [HideInInspector] public Collider Collider;
    [HideInInspector] public HashSet<Character> TriggeredColliderIds = new();
    [SerializeField] public List<EffectBase> effects = new();
    [SerializeField] public bool RedundantCollisions = false;
    [SerializeField] public bool DissapearOnTrigger = false;

    /* Duration */
    [SerializeField] public int duration;
    [HideInInspector] public int frame = 0;

    /* Utility Functions */
    public static IEnumerable<Collider> GetOverlappingColliders(Collider collider) {
        Collider[] colliders = FindObjectsOfType<Collider>();
        return (from c in colliders where collider.bounds.Intersects(c.bounds) select c);
    }

    public static GameObject GetClosestGameObject(GameObject reference, params GameObject[] others) {
        Array.Sort(
            others,
            (o1, o2) => { return ((reference.transform.position - o1.transform.position).sqrMagnitude).CompareTo((reference.transform.position - o2.transform.position).sqrMagnitude); }
        );

        return others[0];
    }

    /* State Handling */
    public void Awake() {
        // ensure that the hitbox information comports with the duration
        Assert.IsTrue(1<=positions.Length&&positions.Length<=duration);
        Assert.IsTrue(1<=rotations.Length&&rotations.Length<=duration);
        Assert.IsTrue(1<=dimensions.Length&&dimensions.Length<=duration);

        Collider=GetComponent<Collider>();
    }

    public void Start() {
        UpdateTransform();
    }

    public void FixedUpdate() {
        // TODO gonna leave out UpdateTransform because these might not be moving, but maybe they will
        HandleCollisions();
    }

    public virtual void UpdateTransform() {
        if (Origin==null) return; // TODO if a character dies during grab, this hits null refs, so this is my cheap fix

        int positionFrame = Mathf.Min(frame, positions.Length-1);
        int rotationFrame = Mathf.Min(frame, rotations.Length-1);
        int dimensionFrame = Mathf.Min(frame, dimensions.Length-1);

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

    public virtual void HandleCollisions() {
        List<Collider> otherColliders = GetOverlappingColliders(Collider).ToList();

        foreach (Collider otherCollider in GetOverlappingColliders(Collider)) {
            GameObject go = otherCollider.gameObject;
            Character target = go.GetComponent<Character>();

            if (target != null) {
                for (int i = 0; i < effects.Count; i++) {
                    // TODO what if I don't want the status effect to stack?
                    // maybe check if an effect of the same type is active, and if so, do some sort of resolution
                    // e.g. if two slows are applied, refresh slow
                    EffectBase effect = Instantiate(effects[i]);
                    effect.Initialize(target);
                }
            }
        }
    }
}
