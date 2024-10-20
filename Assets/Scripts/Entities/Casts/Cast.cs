using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum TargetingMethod {
    ContinuousTargeting,
    DiscreteTargeting,
    SnapTarget,
    SelfTarget
}

[Serializable]
public enum Positioning {
    Relative,
    Absolute
}

[Serializable]
public enum CastableCondition {
    OnRecast,
    OnCollision,
    OnDestruction,
    OnDeath
}

[Serializable]
public class CastableCosts {
    [Range(0, 1)] public int charges = 0;
    [Range(0, 100)] public int energy = 0;
}

public static class CastUtils {
    public static Character GetSnapTarget(Vector3 worldPosition, bool hitsFriendlies, bool hitsEnemies, float maxDistance) {
        List<Character> characters = (from go in GameObject.FindGameObjectsWithTag("Character") select go.GetComponent<Character>()).ToList();
        List<float> characterDists = (from ch in characters select (worldPosition-ch.transform.position).magnitude).ToList();

        float minDist = Mathf.Infinity;
        int minIndex = -1;

        for (int i = 0; i < characterDists.Count; i++) {
            float dist = characterDists[i];
            if (dist < minDist && dist<maxDistance) {
                minDist = dist;
                minIndex = i;
            }
        }

        if (minIndex>=0) {
            return characters[minIndex];
        } else {
            return null;
        }
    }

    public static Transform GetTransformDeepCopy(Transform transform) {
        GameObject go = new("Projectile Target");
        go.transform.position = transform.position;
        go.transform.rotation = transform.rotation;
        return go.transform;
    }
}

public class Cast : MonoBehaviour, ICasts, IHealingTree<Cast> {
    [SerializeField] public Positioning Positioning = Positioning.Relative;
    [SerializeField] public TargetingMethod TargetingMethod = TargetingMethod.ContinuousTargeting;
    [SerializeField] public int Duration;
    [SerializeField] public int EncumberTime;
    [SerializeField] public float Range = 0;
    [SerializeField] public bool DestroyOnRecast = false;
    [SerializeField] public bool DestroyOnCollision = false;
    [SerializeField] public bool DestroyWithParent = false;
    [SerializeField] private FieldExpression<Cast, string> DataExpression = new("0");
    public string Data { get { return DataExpression.Value; } }

    [HideInInspector] public ICasts Caster;
    [HideInInspector] public Transform About;
    [HideInInspector] public Transform Target;
    [HideInInspector] public bool Mirrored = false;
    [HideInInspector] public bool Indefinite = false;
    public int Frame { get; set; } = 0;
    public int MaimStack { get { return 0; } set {; } }
    public int SilenceStack { get { return 0; } set {; } }

    [SerializeField] public FrameCastablesDictionary FrameCastablesMap = new();
    [SerializeField]
    public ConditionCastablesDictionary ConditionCastablesMap = (
        Enum.GetValues(typeof(CastableCondition))
            .Cast<CastableCondition>()
            .ToDictionary(t => t, t => new Cast[0])
        as ConditionCastablesDictionary
    );

    public void Awake() => Indefinite = (Duration<0);

    /// <summary>
    /// To be run right when the Castable is casted by a caster.
    /// </summary>
    /// <param name="caster">The GameObject that created the castable</param>
    /// <param name="about">The Transform about which the castable originates.</param>
    /// <param name="target">The Transform towards which the castable is directed.</param>
    /// <param name="_rotatingClockwise">Whether the castable's trajcetory needs to be mirrored.</param>
    private void Initialize(ICasts caster, Transform about, Transform target, bool mirrored) {
        Caster = caster;
        About = about;
        Target = target;
        Mirrored = mirrored;

        if (TargetingMethod == TargetingMethod.DiscreteTargeting) {
            Target = CastUtils.GetTransformDeepCopy(Target);
        }

        OnInitialize();
    }

    [HideInInspector]
    public Quaternion InitialRotation; // TODO I think I don't need this - remove

    protected virtual void OnInitialize() {
        FieldExpressionParser.instance.RenderValue(this, DataExpression);
        InitialRotation = transform.rotation;
        
        if (Positioning == Positioning.Relative) {
            transform.position = About.position + About.rotation * Vector3.forward * Range;
            transform.rotation = About.rotation;
        } else if (Positioning == Positioning.Absolute) { // Positioning == Positioning.Absolute
            float CastDistance = (Target.position - About.position).magnitude;
            transform.position = About.position + Mathf.Min(Range, CastDistance)*(About.rotation * Vector3.forward);
            transform.rotation = About.rotation * transform.rotation;
        }
    }

    /// <summary/>
    /// <param name="CastablePrefab"></param>
    /// <param name="caster"></param>
    /// <param name="about">The <typeparamref name="Transform"/> about which the cast is created</param>
    /// <param name="target"></param>
    /// <param name="mirrored"></param>
    /// <returns>An instance of the <typeparamref name="Castable"/>, instantiated and initialized</returns>
    public static Cast Instantiate(Cast CastablePrefab, ICasts caster, Transform about, Transform target, bool mirrored, Cast parent) {
        Cast Castable = Instantiate(CastablePrefab);
        Castable.Parent = parent;
        parent.AddChild(Castable);
        Castable.Initialize(caster, about, target, mirrored);
        return Castable;
    }

    /// <summary>
    /// Update the Castable's behavior, according to some strategy decided by the Castable.
    /// </summary>
    /// <remarks>
    /// E.g. if the castable is a rocket, cast updating might involve reassigning the rocket's target destination.
    /// </remarks>
    /// <param name="target"></param>
    protected virtual bool OnRecast(Transform target) => false;

    public bool OnRecastCastables(Transform target) {
        bool ret = false;

        foreach (Cast child in CastableChlidren) {
            ret |= child.OnRecastCastables(target);
        }

        ret |= _castConditionalCastables(CastableCondition.OnRecast);
        ret |= OnRecast(target);

        if (DestroyOnRecast) {
            Destroy(gameObject);
        }

        return ret;
    }

    private bool _castConditionalCastables(CastableCondition castableCondition) {
        if (ConditionCastablesMap.TryGetValue(castableCondition, out Cast[] value)) {
            if (value.Count() == 0) {
                return false;
            } else {
                foreach (Cast Castable in value) {
                    Instantiate(
                        Castable,
                        Caster,
                        transform,
                        Target,
                        Mirrored,
                        this
                    );
                }
            }

            return true;
        } else {
            return false;
        }
    }

    /* ICasts Methods */
    public bool IsRotatingClockwise() => !Mirrored;
    public Transform GetOriginTransform() => transform;
    public Transform GetTargetTransform() => Target; // TODO this will probably depend on the cast, and I think this is a good default?
    public virtual bool AppliesTo(MonoBehaviour mono) => true;

    protected virtual void Tick() { }

    private void FixedUpdate() {
        Tick();

        if (FrameCastablesMap.ContainsKey(Frame)) {
            foreach (Cast Castable in FrameCastablesMap[Frame]) {
                Cast newCast = Instantiate(
                    Castable,
                    Caster,
                    About,
                    Target,
                    Mirrored,
                    this
                );

                CastableChlidren.Add(newCast);

                if (newCast is CommandMovement) {
                    // TODO should this be the caster or the Castable?
                    (About as IMoves).CommandMovement = (CommandMovement)newCast;
                }
            }
        }

        if (++Frame >= Duration && !Indefinite) {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestruction() {
    }

    protected virtual void OnCollision() {
        _castConditionalCastables(CastableCondition.OnCollision);

        if (DestroyOnCollision) {
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if (TargetingMethod == TargetingMethod.DiscreteTargeting) {
            Destroy(Target.gameObject);
        }
        
        _castConditionalCastables(CastableCondition.OnDestruction);
        OnDestruction();
        PropagateChildren();
    }

    /* IHealingTree Methods */
    public Cast Parent { get; set; } = null;
    public List<Cast> CastableChlidren { get; private set; } = new();

    public void PropagateChildren() {
        if (Parent != null) {
            foreach (var child in CastableChlidren) {
                if (child != null) {
                    if (child.DestroyWithParent) {
                        Destroy(child.gameObject);
                    } else {
                        Parent.CastableChlidren.Add(child);
                        child.Parent = Parent;
                    }
                }
            }

            Parent.CastableChlidren.Remove(this);
        }
    }

    public void AddChild(Cast newChild) {
        CastableChlidren.Add(newChild);
    }

    public void PruneChildren() {
        CastableChlidren.All(castable => { if (castable!=null) castable.Prune(); return true; });
    }

    public void Prune() {
        PruneChildren();
        Destroy(gameObject);
    }
}
