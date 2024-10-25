using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.Equals(object o)

public class VectorLabelsAttribute : PropertyAttribute {
    public readonly string[] Labels;

    public VectorLabelsAttribute(params string[] labels) {
        Labels = labels;
    }
}

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

    public static TransformCoordinates FromTransform(Transform t) {
        return new(
            t.position,
            t.rotation,
            t.localScale
        );
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
    private Vector3 axis;

    public static Func<Quaternion, Quaternion> inv = x => Quaternion.Inverse(x);

    public static int NegativeFactor(bool isNegative) {
        return isNegative ? -1 : 1;
    }

    public static Quaternion MirrorQuaternion(Quaternion q, Vector3 axis) {
        // ref: https://discussions.unity.com/t/mirroring-a-quaternion/189704/2
        q.ToAngleAxis(out float angle, out Vector3 qAxis); // get angle and axis
        qAxis = Vector3.Scale(-1*axis, qAxis); // mirror the axis about the plane through the supplied axis
        return Quaternion.AngleAxis(angle, qAxis); // assign it back

    }

    /// <summary>
    /// Mirrors the transformation
    /// </summary>
    public void Mirror() {
        PolarPosition.x *= -1;
        Rotation = MirrorQuaternion(Rotation, Vector3.up);
    }

    public TriggerTransformation(Vector2 _polarPosition, Quaternion _rotation, Vector3 _dimension) {
        axis = Vector3.up;
        PolarPosition = _polarPosition;
        Dimension = _dimension;
        Rotation = _rotation;
    }

    public override string ToString() {
        return $"PolarPosition: {PolarPosition}\nDimension: {Dimension}\nRotation: {Rotation}\nAxis: {axis}";
    }

    public TransformCoordinates ToTransformCoordinates(
        Vector3 position,
        Quaternion rotation,
        bool mirror
    ) {
        axis = Vector3.up;
        Quaternion rot = mirror ? MirrorQuaternion(Rotation, axis) : Rotation;

        Quaternion orientation = Quaternion.AngleAxis( // TODO maybe generalize this to different axes? I hardcoded it here because I think the serialized instances were set to 0, breaking this function
            NegativeFactor(mirror)*PolarPosition.x,
            axis
        );

        return new(
            position + rotation*orientation*Vector3.forward*PolarPosition.y,
            rotation*orientation*rot,
            Dimension
        );
    }

    public TransformCoordinates ToTransformCoordinates(TransformCoordinates origin, bool mirror) {
        return ToTransformCoordinates(
            origin.Position,
            origin.Rotation,
            mirror
        );
    }

    public TransformCoordinates ToTransformCoordinates(Transform origin, bool mirror) {
        return ToTransformCoordinates(
            origin.position,
            origin.rotation,
            mirror
        );
    }

    public static TriggerTransformation FromTransformCoordinates(Transform transform, Transform origin, bool mirror) {
        return FromTransformCoordinates(
            new TransformCoordinates(transform.position, transform.rotation, transform.localScale),
            origin,
            mirror
        );
    }

    public static TriggerTransformation FromTransformCoordinates(TransformCoordinates coordinates, Transform origin, bool mirror) {
        Vector3 axis = Vector3.up; // TODO hardcoded
        Vector3 globalOrientedRelativePosition = coordinates.Position-origin.position;
        Vector3 relativelyOrientedRelativePosition = inv(origin.rotation)*globalOrientedRelativePosition;

        Vector2 PolarPosition = new Vector2(
            Vector3.SignedAngle(Vector3.forward, relativelyOrientedRelativePosition, axis),
            relativelyOrientedRelativePosition.magnitude
        );

        Quaternion orientation = Quaternion.AngleAxis(
            PolarPosition.x,
            axis
        );

        Quaternion transformationRotation =
            inv(origin.rotation)
            * inv(orientation)
            * coordinates.Rotation;

        if (mirror) {
            PolarPosition.x *= -1;
            transformationRotation = MirrorQuaternion(transformationRotation, Vector3.up);
        }

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

    public TriggerTransformation GetHardCopy() {
        return new TriggerTransformation(
            this.PolarPosition,
            this.Rotation,
            this.Dimension
        );
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
public class Trigger : Cast, ICollidable, ICasts {
    /* Transform */
    [SerializeField] public List<TriggerTransformation> TriggerTransformations;
    private TransformCoordinates _staticCoordinates = null;

    /* Utility Functions */
    public static GameObject GetClosestGameObject(GameObject reference, params GameObject[] others) {
        Array.Sort(
            others,
            (o1, o2) => { return ((reference.transform.position - o1.transform.position).sqrMagnitude).CompareTo((reference.transform.position - o2.transform.position).sqrMagnitude); }
        );

        return others[0];
    }

    /* State Handling */
    new public void Awake() {
        if (_collider is CapsuleCollider cc) {
            // https://discussions.unity.com/t/capsule-collider-how-direction-change-to-z-axis-c/225672/2
            Assert.IsTrue(cc.direction==0);
        }

        base.Awake();
    }

    override protected void OnInitialize() {
        base.OnInitialize();
        transform.position = About.position;
        transform.rotation = About.rotation;
    }

    public void Start() {
        // Evaluate the position of the Trigger before it's accounted for in game logic
        if (RetriggerDuration>0) {
            CollisionLog = new();
        }

        _staticCoordinates = new TransformCoordinates(transform.position, transform.rotation, transform.localScale);
        UpdateTransform(0);
    }

    protected override void Tick() {
        // TODO gonna leave out UpdateTransform because these might not be moving, but maybe they 
        if (CollisionLog != null) {
            foreach (ICollidable c in CollisionLog.Keys.ToList()) {
                CollisionLog[c]--;
            }
        }

        UpdateTransform(Frame);
        HandleCollisions();
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
        int index = Math.Min(_frame, TriggerTransformations.Count-1);
        TransformCoordinates tc = (About==null)
            ? TriggerTransformations[index].ToTransformCoordinates(_staticCoordinates, Mirrored)
            : TriggerTransformations[index].ToTransformCoordinates(About, Mirrored);

        tc.Apply(transform);
    }

    /// <summary>
    /// No-op for Triggers
    /// </summary>
    public float TakeKnockBack(Vector3 contactPoint, int hitLagDuration, Vector3 knockBackVector, int hitStunDuration, int hitTier) { return 0f; }

    /* ICollidable */
    [SerializeField] public int RetriggerDuration = 60;
    [SerializeField] public bool HitsEnemies = true;
    [SerializeField] public bool HitsFriendlies = false;
    private Dictionary<ICollidable, int> CollisionLog = null;
    

    override public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this, (RetriggerDuration==0) ? null : CollisionLog, RetriggerDuration);
    }

    public virtual bool OnCollideWith(ICollidable collided, CollisionInfo info) {
        if (
            (collided is not IDamageable)
            || (Caster==collided && !HitsFriendlies)
            || (Caster!=collided && !HitsEnemies)
        ) {
            return false;
        } else {
            if (collided.Transform.gameObject.activeSelf) {
                // TODO why did I check this? when will a collision happen with something inactive?
                return base.OnCollideWith(collided, info);
            } else {
                return false;
            }
        }
    }

    public void UpdatePrefabTransformations(Trigger prefab) {
        for (int i = 0; i < Duration; i++) {
            UpdateTransform(i);
            prefab.TriggerTransformations[i] = TriggerTransformation.FromTransformCoordinates(transform, About, Mirrored);
            // TODO hardcoding mirrored=false - I'm worried that the Caster is getting rotated in the CastablePlayer edit process
        }
    }

    override protected void OnDestruction() {}
}
