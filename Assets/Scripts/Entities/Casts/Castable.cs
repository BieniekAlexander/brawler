using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using ServiceStack.Script;
using UnityEditor;

[Serializable]
public enum Positioning {
    Directional,
    Absolute
}

[Serializable]
public enum CastableCondition {
    OnRecast,
    OnCollision,
    OnDestruction,
    OnDeath
}

public interface ICastableMessage : IEventSystemHandler {
    void OnCast(CastId castId);
}

public class FieldExpressionParser : ScriptableSingleton<FieldExpressionParser> {
    // ref: https://sharpscript.net/lisp/unity#annotated-unity-repl-transcript
    private Lisp.Interpreter interpreter;
    private ScriptContext scriptContext;

    public class CastableLispAccessors : ScriptMethods {
        public string Data(Castable c) => c.Data;
        public int Duration(Castable c) => c.Duration;
        public int Frame(Castable c) => c.Frame;
        public Castable Parent(Castable c) => c.Parent;
    }

    public void OnEnable() {
        scriptContext = new ScriptContext {
            ScriptLanguages = { ScriptLisp.Language },
            AllowScriptingOfAllTypes = true,
            ScriptNamespaces = { nameof(UnityEngine) },
            Args = { },
            ScriptMethods = {
                new ProtectedScripts(),
                new CastableLispAccessors(),
            }
        }.Init();

        interpreter = Lisp.CreateInterpreter();

        // NOTE: running an empty call on interpreter because it seems to be lazily setting itself up,
        // leading to a lot of overhead on the first call of ReplEval
        interpreter.ReplEval(scriptContext, null, "\"wtf\"");
    }

    public T RenderValue<C, T>(C context, FieldExpression<C, T> fieldExpression) where C : Castable {
        return fieldExpression.GetValue(context, interpreter, scriptContext);
    }
}

public class Castable : MonoBehaviour, ICasts, IHealingTree<Castable> {
    [SerializeField] public int Duration;
    [SerializeField] public bool ExpiresOnNewCast = false;
    [SerializeField] public bool ExpiresOnRecast = false;
    [SerializeField] private FieldExpression<Castable, string> DataExpression = new("0");

    [HideInInspector] public ICasts Caster;
    [HideInInspector] public Transform About;
    [HideInInspector] public Transform Target;
    [HideInInspector] public bool Mirrored = false;
    [HideInInspector] public bool Indefinite = false;
    public string Data { get; set; } = "";
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
        OnInitialize();
    }

    protected virtual void OnInitialize() => Data = FieldExpressionParser.instance.RenderValue(this, DataExpression);

    /// <summary/>
    /// <param name="CastablePrefab"></param>
    /// <param name="caster"></param>
    /// <param name="about">The <typeparamref name="Transform"/> about which the cast is created</param>
    /// <param name="target"></param>
    /// <param name="mirrored"></param>
    /// <returns>An instance of the <typeparamref name="Castable"/>, instantiated and initialized</returns>
    public static Castable CreateCastable(Castable CastablePrefab, ICasts caster, Transform about, Transform target, bool mirrored, Castable parent) {
        Castable Castable = Instantiate(CastablePrefab);
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

        foreach (Castable child in CastableChlidren) {
            ret |= child.OnRecastCastables(target);
        }

        ret |= _castConditionalCastables(CastableCondition.OnRecast);
        ret |= OnRecast(target);

        if (ExpiresOnRecast) {
            Destroy(gameObject);
        }

        return ret;
    }

    private bool _castConditionalCastables(CastableCondition castableCondition) {
        if (ConditionCastablesMap.TryGetValue(castableCondition, out Castable[] value)) {
            if (value.Count() == 0) {
                return false;
            } else {
                foreach (Castable Castable in value) {
                    CreateCastable(
                        Castable,
                        Caster,
                        About,
                        null,
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
            foreach (Castable Castable in FrameCastablesMap[Frame]) {
                Castable newCast = CreateCastable(
                        Castable,
                        Caster,
                        About,
                        Caster.GetTargetTransform(),
                        Mirrored,
                        this
                    );

                CastableChlidren.Add(newCast);

                if (newCast is CommandMovement) {
                    (About as IMoves).CommandMovement = (CommandMovement)newCast;
                }
            }
        }

        if (++Frame >= Duration && !Indefinite) {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestruction() { }

    private void OnDestroy() {
        _castConditionalCastables(CastableCondition.OnDestruction);
        OnDestruction();
        PropagateChildren();
    }

    /* IHealingTree Methods */
    public Castable Parent { get; set; } = null;
    public List<Castable> CastableChlidren { get; private set; } = new List<Castable>();

    public void PropagateChildren() {
        if (Parent != null) {
            foreach (var child in CastableChlidren) {
                if (!child.ExpiresOnNewCast) {
                    Parent.CastableChlidren.Add(child);
                } else {
                    Destroy(child.gameObject);
                }
            }

            Parent.CastableChlidren.Remove(this);
        }
    }

    public void AddChild(Castable newChild) {
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
