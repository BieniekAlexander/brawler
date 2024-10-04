using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum Positioning {
    Directional,
    Absolute
}

/// <summary>
/// A set of potential conditions that might be met for a <typeparamref name="Castable"/>
/// </summary>
[Serializable]
public enum CastableCondition {
    OnRecast,
    OnFinishStartup,
    OnCollide,
    OnExpire,
    OnDeath
}

public abstract class Castable : MonoBehaviour, ICasts {
    [SerializeField] public int Duration;
    [HideInInspector] public ICasts Caster;
    [HideInInspector] public Transform Origin;
    [HideInInspector] public Transform Target; // TODO do I need this?
    [HideInInspector] public bool Mirror = false;
    [HideInInspector] public int Frame = 0;
    [HideInInspector] public bool indefinite = false;
    [HideInInspector] public IEnumerable<Castable> ActiveCastables { get; set; } = new List<Castable>();
    [SerializeField] public FrameCastablesDictionary FrameCastablesMap = new();
    [SerializeField]
    public ConditionCastablesDictionary ConditionCastablesMap = (
        Enum.GetValues(typeof(CastableCondition))
            .Cast<CastableCondition>()
            .ToDictionary(t => t, t => new Castable[0])
        as ConditionCastablesDictionary
    );

    public void Awake() {
        if (Duration<0)
            indefinite = true;
    }

    /// <summary>
    /// To be run right when the Castable is casted by a caster.
    /// </summary>
    /// <param name="_caster">The GameObject that created the castable</param>
    /// <param name="_origin">The Transform about which the castable originated.</param>
    /// <param name="_target">The Transform towards which the castable was initially sent.</param>
    /// <param name="_rotatingClockwise">Whether, according the castable's movement, the trajcetory needs to be mirrored.</param>
    public virtual void Initialize(ICasts _caster, Transform _origin, Transform _target, bool _mirrored) {
        Caster = _caster;
        Origin = _origin;
        Target = _target;
        Mirror = _mirrored;
    }

    /// <summary>
    /// To be run right when the Castable is casted by a caster.
    /// </summary>
    /// <param name="_caster">The GameObject that created the castable</param>
    /// <param name="_origin">The Transform about which the castable originated.</param>
    /// <param name="_target">The Vector3 world position towards which the castable was initially sent.</param>
    /// <param name="_rotatingClockwise">Whether, according the castable's movement, the trajcetory needs to be mirrored.</param>
    public virtual void Initialize(ICasts _caster, Transform _origin, Vector3 _target, bool _mirrored) {
        GameObject go = new("Castable Target");
        go.transform.parent = transform;
        go.transform.position = _target;
        Initialize(_caster, _origin, go.transform, _mirrored);
    }

    /// <summary/>
    /// <param name="CastablePrefab"></param>
    /// <param name="_caster"></param>
    /// <param name="_origin">The <typeparamref name="Transform"/> about which the cast is created</param>
    /// <param name="_target"></param>
    /// <param name="_mirrored"></param>
    /// <returns>An instance of the <typeparamref name="Castable"/>, instantiated and initialized</returns>
    public static Castable CreateCast(Castable CastablePrefab, ICasts _caster, Transform _origin, Transform _target, bool _mirrored) {
        Castable Castable = Instantiate(CastablePrefab);
        Castable.Initialize(_caster, _origin, _target, _mirrored);
        return Castable;
    }

    /// <summary>
    /// Update the Castable's behavior, according to some strategy decided by the Castable.
    /// </summary>
    /// <remarks>
    /// E.g. if the castable is a rocket, cast updating might involve reassigning the rocket's target destination.
    /// </remarks>
    /// <param name="_target"></param>
    public virtual void Recast(Transform _target) {; }

    /* ICasts Methods */
    public bool IsRotatingClockwise() {
        return true; // TODO actually implement this later :)
    }

    public Transform GetOriginTransform() {
        return transform;
    }

    public Transform GetTargetTransform() {
        return Caster.GetTargetTransform(); // TODO this will probably depend on the cast, and I think this is a good default?
    }

    public virtual void FixedUpdate() {
        if (FrameCastablesMap.ContainsKey(Frame)) {
            foreach (Castable Castable in FrameCastablesMap[Frame]) {
                (ActiveCastables as List<Castable>).Add(
                    CreateCast(
                        Castable,
                        Caster,
                        Origin,
                        Caster.GetTargetTransform(),
                        Mirror
                    )
                );
            }
        }

        if (++Frame == Duration && !indefinite) {
            Destroy(gameObject);
        }
    }
}
