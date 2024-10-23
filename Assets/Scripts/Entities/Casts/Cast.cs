using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum AboutResolution {
    ShallowCopyAbout,
    ShallowCopyTarget,
    HardCopyAbout,
    HardCopyTarget
}

[Serializable]
public enum TargetResolution {
    ShallowCopy,
    HardCopy,
    SnapTarget,
    SelfTarget
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
    public static Character GetSnapTarget(Vector3 worldPosition, int teamMask, float maxDistance) {
        List<Character> characters = (from go in GameObject.FindGameObjectsWithTag("Character") select go.GetComponent<Character>()).ToList();
        List<float> characterDists = (from ch in characters select (worldPosition-ch.transform.position).magnitude).ToList();

        float minDist = Mathf.Infinity;
        int minIndex = -1;

        for (int i = 0; i < characterDists.Count; i++) {
            if ((characters[i].TeamBit & teamMask)>0) {
                float dist = characterDists[i];
                if (dist < minDist && dist<maxDistance) {
                    minDist = dist;
                    minIndex = i;
                }
            }        
        }

        if (minIndex>=0) {
            return characters[minIndex];
        } else {
            return null;
        }
    }

    public static Transform GetTransformDeepCopy(Transform transform, string name, Transform parentTransform) {
        GameObject go = new(name);
        go.transform.parent = parentTransform;
        go.transform.position = transform.position;
        go.transform.rotation = transform.rotation;
        return go.transform;
    }
}

public class Cast : MonoBehaviour, ICasts, IHealingTree<Cast> {
    [SerializeField] public AboutResolution Positioning = AboutResolution.ShallowCopyAbout;
    [SerializeField] public TargetResolution TargetResolution = TargetResolution.ShallowCopy;
    [SerializeField] public int Duration;
    [SerializeField] public int EncumberTime;
    [SerializeField] public float Range = 0;
    [SerializeField] public bool DestroyOnRecast = false;
    [SerializeField] public bool DestroyOnCollision = false;
    [SerializeField] public bool DestroyWithParent = false;
    [SerializeField] private FieldExpression<Cast, string> DataExpression = new("0");
    public string Data { get { return DataExpression.Value; } }

    [HideInInspector] public ICasts Caster;
    [HideInInspector] protected Transform About;
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

        // TODO clean these configs up
        if (Positioning == AboutResolution.HardCopyTarget) {
            About = CastUtils.GetTransformDeepCopy(target, "Cast About", null);
        }

        if (TargetResolution == TargetResolution.HardCopy) {
            Target = CastUtils.GetTransformDeepCopy(target, "Projectile About", null);
        }

        OnInitialize();
    }

    protected virtual void OnInitialize() {
        FieldExpressionParser.instance.RenderValue(this, DataExpression);
        transform.rotation = Quaternion.LookRotation(Target.position-About.position, Vector3.up);

        if (Positioning == AboutResolution.HardCopyAbout) {
            transform.position = About.position + transform.rotation*Vector3.forward*Range;
            About = transform;
        } else if (Positioning == AboutResolution.HardCopyTarget) {
            float CastDistance = (Target.position - About.position).magnitude;
            transform.position = About.position + Mathf.Min(Range, CastDistance)*(transform.rotation * Vector3.forward);
            About = transform;
        } 
    }

    /// <summary/>
    /// <param name="CastablePrefab"></param>
    /// <param name="caster"></param>
    /// <param name="about">The <typeparamref name="Transform"/> about which the cast is created</param>
    /// <param name="target"></param>
    /// <param name="mirrored"></param>
    /// <returns>An instance of the <typeparamref name="Castable"/>, instantiated and initialized</returns>
    public static Cast Initiate(Cast CastablePrefab, ICasts caster, Transform about, Transform target, bool mirrored, Cast parent) {
        Cast Castable = Instantiate(CastablePrefab);
        Castable.Parent = parent;
        parent._children.Add(Castable);
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

        foreach (Cast child in _children) {
            ret |= child.OnRecastCastables(target);
        }

        ret |= OnRecast(target);
        ret |= _castConditionalCastables( // sinister bug here - if set to destroy on recast, both will happen, but I only want one
            DestroyOnRecast
            ? CastableCondition.OnDestruction
            : CastableCondition.OnRecast
        );

        if (DestroyOnRecast) {
            Destroy(gameObject);
        }

        return ret;
    }

    private bool _castCastables(Cast[] casts) {
        if (casts==null && casts.Count() == 0) {
                return false;
        } else {
            foreach (Cast Castable in casts) {
                Cast newCast = Initiate(
                    Castable,
                    Caster,
                    About,
                    Target,
                    Mirrored,
                    this
                );
            }

            return true;
        }
    }

    private bool _castConditionalCastables(CastableCondition castableCondition) {
        if (ConditionCastablesMap.ContainsKey(castableCondition)) {
            return _castCastables(ConditionCastablesMap[castableCondition]);
        } else {
            return false;
        }
    }

    /* ICasts Methods */
    public bool IsRotatingClockwise() => !Mirrored;
    public Transform GetOriginTransform() => transform;
    public Transform GetTargetTransform() => Target; // TODO this will probably depend on the cast, and I think this is a good default?

    protected virtual void Tick() { }

    private void FixedUpdate() {
        Tick();

        if (FrameCastablesMap.ContainsKey(Frame)) {
            _castCastables(FrameCastablesMap[Frame]);
        }

        if (++Frame >= Duration && !Indefinite) {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestruction() {
    }

    protected virtual void OnCollision(ICollidable collidable) {
        Cast[] casts = ConditionCastablesMap.ContainsKey(CastableCondition.OnCollision)
            ? ConditionCastablesMap[CastableCondition.OnCollision]
            : null;
        if (casts==null || casts.Count() == 0) {
            return;
        } else {
            foreach (Cast Castable in casts) {
                if (Castable is Effect effect && !effect.AppliesTo(collidable.Transform.gameObject)) {
                    continue;
                }

                Cast newCast = Initiate(
                    Castable,
                    Caster,
                    (Castable is Effect)?collidable.Transform:About,
                    Target,
                    Mirrored,
                    this
                );
            }

            
        }

        if (DestroyOnCollision) {
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if (TargetResolution == TargetResolution.HardCopy) {
            Destroy(Target.gameObject);
        }
        
        _castConditionalCastables(CastableCondition.OnDestruction);
        OnDestruction();
        PropagateChildren();
    }

    /* IHealingTree Methods */
    public Cast Parent { get; set; } = null;
    public List<Cast> Children => _children;
    private List<Cast> _children = new();

    public void PropagateChildren() {
        if (Parent != null) {
            foreach (var child in _children) {
                if (child != null) {
                    if (child.DestroyWithParent) {
                        Destroy(child.gameObject);
                    } else {
                        Parent._children.Add(child);
                        child.Parent = Parent;
                    }
                }
            }

            Parent._children.Remove(this);
        }
    }
}
