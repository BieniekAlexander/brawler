using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

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
    OnCollide,
    OnExpire,
    OnDeath
}

public interface ICastableMessage : IEventSystemHandler {
    void OnCast(CastId castId);
}

public class Castable : MonoBehaviour, ICasts, IHealingTree<Castable> {
    [SerializeField] public int Duration;
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
            .ToDictionary(t => t, t => new Castable[0])
        as ConditionCastablesDictionary
    );

    public void Awake() {
        if (Duration<0)
            Indefinite = true;
    }

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
        OnInitialize();
    }

    protected virtual void OnInitialize() {
        
    }

    /// <summary/>
    /// <param name="CastablePrefab"></param>
    /// <param name="caster"></param>
    /// <param name="about">The <typeparamref name="Transform"/> about which the cast is created</param>
    /// <param name="target"></param>
    /// <param name="mirrored"></param>
    /// <returns>An instance of the <typeparamref name="Castable"/>, instantiated and initialized</returns>
    public static Castable CreateCastable(Castable CastablePrefab, ICasts caster, Transform about, Transform target, bool mirrored, Castable parent) {
        Castable Castable = Instantiate(CastablePrefab);
        Castable.Initialize(caster, about, target, mirrored);
        Castable.Parent = parent;
        parent.AddChild(Castable);
        return Castable;
    }

    /// <summary>
    /// Update the Castable's behavior, according to some strategy decided by the Castable.
    /// </summary>
    /// <remarks>
    /// E.g. if the castable is a rocket, cast updating might involve reassigning the rocket's target destination.
    /// </remarks>
    /// <param name="target"></param>
    protected virtual bool OnRecast(Transform target) {
        return false;
    }

    public bool OnRecastCastables(Transform target) {
        bool ret = false;

        foreach (Castable child in Children) {
            ret |= child.OnRecastCastables(target);
        }

        ret |= OnRecast(target);
        return ret;
    }


    private void OnExpireCastables() {
        if (ConditionCastablesMap.TryGetValue(CastableCondition.OnExpire, out Castable[] value)) {
            foreach (Castable Castable in value) {
                CreateCastable(
                    Castable,
                    Caster,
                    transform,
                    null,
                    Mirrored,
                    this
                );
            }
        }

        foreach (Castable Castable in Children) {
            Castable.OnExpireCastables();
        }

        OnExpire();
    }

    protected virtual void OnExpire() {
        
    }

    /* ICasts Methods */
    public bool IsRotatingClockwise() {
        return true; // TODO actually implement this later :)
    }

    public Transform GetOriginTransform() {
        return transform;
    }

    public Transform GetTargetTransform() {
        return Target; // TODO this will probably depend on the cast, and I think this is a good default?
    }

    public virtual bool AppliesTo(MonoBehaviour mono) {
        return true;
    }

    protected virtual void Tick() {}

    private void FixedUpdate() {
        Tick();

        if (FrameCastablesMap.ContainsKey(Frame)) {
            foreach (Castable Castable in FrameCastablesMap[Frame]) {
                Castable newCast = CreateCastable(
                        Castable,
                        Caster,
                        About,
                        Caster.GetTargetTransform(),
                        Mirrored,
                        this
                    );

                Children.Add(newCast);

                if (newCast is CommandMovement) {
                    (About as IMoves).CommandMovement = (CommandMovement)newCast;
                }
            }
        }

        if (++Frame >= Duration && !Indefinite) {
            Destroy(gameObject);
        }
    }

    void OnDestroy() {
        OnExpireCastables();
        PropagateChildren();
    }

    /* IHealingTree Methods */
    public Castable Parent { get; set; } = null;
    public List<Castable> Children { get; private set; } = new List<Castable>();

    public void PropagateChildren() {
        if (Parent != null){
            foreach (var child in Children) {
                Parent.Children.Add(child);
            }
        }
    }

    public void AddChild(Castable newChild) {
        Children.Add(newChild);
    }

    public void PruneChildren() {
        Children.All(castable => { if (castable!=null) castable.Prune(); return true; });
    }

    public void Prune() {
        PruneChildren();
        Destroy(gameObject);
    }
}
