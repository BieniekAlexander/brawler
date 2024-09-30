using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class VectorLabelsAttribute : PropertyAttribute {
    public readonly string[] Labels;

    public VectorLabelsAttribute(params string[] labels) {
        Labels = labels;
    }
}

public enum CoordinateSystem { Polar, Cartesian };

[Serializable]
public class TransformCoordinates : IEquatable<TransformCoordinates> {
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Dimension;

    public TransformCoordinates(Vector3 _position, Quaternion _rotation, Vector3 _dimension) {
        Position = _position;
        Rotation = _rotation;
        Dimension = _dimension;
    }
    public override string ToString() {
        return $"Position: {Position}\nDimension: {Dimension}\nRotation {Rotation}";
    }

    public void Apply(Transform transform) {
        transform.position = Position;
        transform.rotation = Rotation;
        transform.localScale = Dimension;
    }

    public bool Equals(TransformCoordinates other) {
        return 
            Position == other.Position
            && Rotation == other.Rotation
            && Dimension == other.Dimension;
    }

    public static bool operator !=(TransformCoordinates obj1, TransformCoordinates obj2) => !(obj1 == obj2);
    public static bool operator ==(TransformCoordinates obj1, TransformCoordinates obj2) {
        if (ReferenceEquals(obj1, obj2))
            return true;
        else if (ReferenceEquals(obj1, null))
            return false;
        else if (ReferenceEquals(obj2, null))
            return false;
        else
            return obj1.Equals(obj2);
    }
}

[Serializable]
public class TriggerTransformation : IEquatable<TriggerTransformation> {
    public Vector2 PolarPosition;
    public Vector3 Dimension;
    public Quaternion Rotation;
    private Vector3 axis = Vector3.up;

    public TriggerTransformation(Vector2 _polarPosition, Quaternion _rotation, Vector3 _dimension) {
        PolarPosition = _polarPosition;
        Dimension = _dimension;
        Rotation = _rotation;
    }

    public override string ToString() {
        return $"PolarPosition: {PolarPosition}\nDimension: {Dimension}\nRotation: {Rotation}Axis: {axis}";
    }

    public TransformCoordinates ToTransformCoordinates(Transform origin, bool mirror) {
        Quaternion orientation = Quaternion.Euler(0, PolarPosition.x, 0);
        Vector3 offset = Quaternion.AngleAxis( // TODO maybe generalize this to different axes?
            PolarPosition.x,
            axis
        )*Vector3.forward*PolarPosition.y;

        // TODO make sure that the calculations without an origin are correct
        if (mirror) {
            offset.x *= -1;
            orientation.y *= -1;
        }

        return new(
            origin.position+origin.rotation*offset,
            origin.rotation*orientation*Rotation,
            Dimension
        );
    }

    public static TriggerTransformation FromTransformCoordinates(TransformCoordinates coordinates, Transform origin, bool mirror) {
        Vector3 globalOrientedRelativePosition = coordinates.Position-origin.position;
        Vector3 relativelyOrientedRelativePosition = Quaternion.Inverse(origin.rotation)*globalOrientedRelativePosition;
        Vector2 PolarPosition = new Vector2(
            Vector3.Angle(Vector3.forward, relativelyOrientedRelativePosition),
            relativelyOrientedRelativePosition.magnitude
        );

        Quaternion orientation = Quaternion.Euler(0, PolarPosition.x, 0);
        Quaternion transformationRotation =
            Quaternion.Inverse(orientation)
            *Quaternion.Inverse(origin.rotation)
            *coordinates.Rotation;

        return new(
            PolarPosition,
            transformationRotation,
            coordinates.Dimension
        );
    }

    public bool Equals(TriggerTransformation other) {
        bool samePolarPosition = PolarPosition == other.PolarPosition;
        bool sameRotation = Rotation == other.Rotation;
        bool sameDimension = Dimension == other.Dimension;
        return samePolarPosition && sameRotation && sameDimension;
    }

    public static bool operator !=(TriggerTransformation obj1, TriggerTransformation obj2) => !(obj1 == obj2);
    public static bool operator ==(TriggerTransformation obj1, TriggerTransformation obj2) {
        if (ReferenceEquals(obj1, obj2))
            return true;
        else if (ReferenceEquals(obj1, null))
            return false;
        else if (ReferenceEquals(obj2, null))
            return false;
        else 
            return obj1.Equals(obj2);
    }
}

/// <summary>
/// Class that generically handles the changing of object states when the object collides with it
/// </summary>
public class Trigger : Castable, ICollidable, ISerializationCallbackReceiver {
    /* Transform */
    [SerializeField] List<TriggerTransformation> TriggerTransformations;

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

        if (Collider is CapsuleCollider cc) {
            // https://discussions.unity.com/t/capsule-collider-how-direction-change-to-z-axis-c/225672/2
            Assert.IsTrue(cc.direction==0);
        }
    }

    public void Initiate(ICasts _caster, Transform _origin, Transform _target, bool _mirrored) {
        Caster = _caster;
        Origin = _origin;
        Target = _target;
        Mirror = _mirrored;
    }

    public void Start() {
        // Evaluate the position of the Trigger before it's accounted for in game logic
        UpdateTransform(0);
    }

    public void FixedUpdate() {
        // TODO gonna leave out UpdateTransform because these might not be moving, but maybe they will
        if (Frame>=Duration) {
            Destroy(gameObject);
        }

        UpdateTransform(Frame);
        HandleCollisions();
        Frame++;
    }

    /* IMoves Methods */
    public virtual void UpdateTransform(int _frame) {
        /*
         * I almost pulled my hair out over trying to understand the Collider handling again, so
         * I'll save the following notes here. I don't know that my current Collision implementation is great,
         * but it seems okay for now. One of the big difficulties has been handling hitbox scale,
         * and to keep the implementation clean, I need to consider the following:
         * - A Collider's shape is only an approximation of the transform's scale, given the following:
         *   - A sphere collider's shape is a centered circle with r = Max(localScale)
         *   - a Capsule Collider's shape:
         *     - h = localScale along a specified major axis
         *     - r = Max(localScale of minor axes)
         *     - then, rotation is only meaningful on minor axes
         * - Additionally, confusing things happen when the collider size fields are nondefault:
         *   - make sure sphere r=.5
         *   - make sure capsule h=1, r=.5
         */
        // maybe by convention, X will be major axis? and then the magnitude of minor axes must be smaller
        // I'm considering enforcing this with an assertion, and additionally maybe specifying different parameter types
        if (Origin==null) {
            return; // If the origin disappears, stop moving
        }

        int index = Math.Min(_frame, TriggerTransformations.Count-1);
        TriggerTransformations[index].ToTransformCoordinates(Origin, Mirror).Apply(transform);
    }

    /// <summary>
    /// No-op for Triggers
    /// </summary>
    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, int hitTier) { return 0f; }

    /* ICollidable Methods */
    public Collider GetCollider() { return Collider; }

    public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this, RedundantCollisions ? null : CollisionLog);
    }

    public virtual void OnCollideWith(ICollidable other, CollisionInfo info) {
        if (other is MonoBehaviour mono && mono.enabled) {
            for (int i = 0; i < effects.Count; i++) {
                if (effects[i].CanEffect(mono)) {
                    // TODO what if I don't want the status effect to stack?
                    // maybe check if an effect of the same type is active, and if so, do some sort of resolution
                    // e.g. if two slows are applied, refresh slow
                    Effect effect = Instantiate(effects[i]);
                    effect.Initialize(mono);
                }
            }
        }
    }

    public void OnBeforeSerialize() {
        /*
        if (TriggerTransformations!=null && TriggerTransformations.Count > 0) {
            transform.position = TriggerTransformations[0].Position;
            transform.rotation = TriggerTransformations[0].Rotation;
            transform.localScale = TriggerTransformations[0].Dimension;
        }*/
    }

    public void OnAfterDeserialize() {
    }
}
