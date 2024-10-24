using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum AboutResolution {
    ShallowCopyAbout,
    ShallowCopyCaster,
    ShallowCopyTarget,
    HardCopyAbout,
    HardCopyCaster,
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
    OnAttack,
    OnRecast,
    OnCollision,
    OnDestruction,
    OnRecastDestruction,
    OnAttackDestruction,
    OnDeath
}

[Serializable]
public class CastCosts {
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
    [SerializeField] public AboutResolution AboutResolution = AboutResolution.ShallowCopyAbout;
    [SerializeField] public TargetResolution TargetResolution = TargetResolution.ShallowCopy;
    [SerializeField] public int Duration;
    [SerializeField] public int BusyTimer;
    [SerializeField] public float RotationCap = 180f;
    [SerializeField] public float Range = 0;
    [SerializeField] public bool Encumbering;
    [SerializeField] public float RecastCooldown = 0; // TODO make use of implementation
    [SerializeField] public float AttackCooldown = 0; // TODO make use of implementation
    [SerializeField] public bool DestroyOnRecast = false;
    [SerializeField] public bool DestroyOnAttack = false;
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
        if (AboutResolution == AboutResolution.HardCopyTarget) {
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

        if (AboutResolution == AboutResolution.HardCopyAbout) {
            transform.position = About.position + transform.rotation*Vector3.forward*Range;
            About = transform;
        } else if (AboutResolution == AboutResolution.HardCopyTarget) {
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
    /// Cast an attack, based on the active castable's behavior
    /// </summary>
    /// <remarks>E.g. dash attack</remarks>
    /// <param name="target"></param>
    /// <returns>cast busying time</returns>
    protected virtual int OnAttack(Transform target) => -1;

    public int OnAttackCastables(Transform target) {
        int ret = -1;

        foreach (Cast child in _children) {
            ret = Math.Max(ret, child.OnAttackCastables(target));
        }

        ret = Math.Max(ret, OnAttack(target));
        ret = Math.Max(
            ret,
            _castConditionalCastables(
                DestroyOnAttack
                ? CastableCondition.OnAttackDestruction
                : CastableCondition.OnAttack   
            )
        );

        if (DestroyOnAttack) {
            Destroy(gameObject);
        }

        return ret;
    }

    /// <summary>
    /// Update the Castable's behavior, according to some strategy decided by the Castable.
    /// </summary>
    /// <remarks>
    /// E.g. if the castable is a rocket, cast updating might involve reassigning the rocket's target destination.
    /// </remarks>
    /// <param name="target"></param>
    protected virtual int OnRecast(Transform target) => -1;

    public int OnRecastCastables(Transform target) {
        int ret = -1;

        foreach (Cast child in _children) {
            ret = Math.Max(ret, child.OnRecastCastables(target));
        }

        ret = Math.Max(ret, OnRecast(target));
        ret = Math.Max(
            ret,
            _castConditionalCastables(
                DestroyOnRecast
                ? CastableCondition.OnRecastDestruction
                : CastableCondition.OnRecast
            )
        );

        if (DestroyOnRecast) {
            Destroy(gameObject);
        }

        return ret;
    }

    private int _castCastables(Cast[] casts) {
        if (casts==null && casts.Count() == 0) {
                return -1;
        } else {
            int ret = 0;

            foreach (Cast Castable in casts) {
                Cast newCast = Initiate(
                    Castable,
                    Caster,
                    About,
                    Target,
                    Mirrored,
                    this
                );

                ret = Math.Max(ret, newCast.BusyTimer);
            }

            return ret;
        }
    }

    private int _castConditionalCastables(CastableCondition castableCondition) {
        if (ConditionCastablesMap.ContainsKey(castableCondition)) {
            return _castCastables(ConditionCastablesMap[castableCondition]);
        } else {
            return -1;
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

    private int f(int x) {
        Debug.Log($"hello {x}");
        return x;
    }

    protected virtual void OnCollision(ICollidable collidable) {
        Cast[] casts = ConditionCastablesMap.ContainsKey(CastableCondition.OnCollision)
            ? ConditionCastablesMap[CastableCondition.OnCollision]
            : null;
        if (casts==null || casts.Count() == 0) {
            return;
        } else {
            foreach (Cast Castable in casts) {
                Transform castableAbout = null;
                
                switch (Castable.AboutResolution) {
                    case AboutResolution.ShallowCopyCaster:
                        castableAbout = Caster.GetOriginTransform();
                        break;
                    case AboutResolution.ShallowCopyAbout:
                        castableAbout = About;
                        break;
                    case AboutResolution.ShallowCopyTarget:
                        castableAbout = collidable.Transform;
                        break;
                    default:
                        throw new Exception($"unhandled Effect About evaluation for {Castable.AboutResolution}");
                }

                if (!(Castable is Effect effect) || effect.AppliesTo(castableAbout.gameObject)) {
                    Cast newCast = Initiate(
                        Castable,
                        Caster,
                        castableAbout,
                        Target,
                        Mirrored,
                        this
                    );
                }
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
