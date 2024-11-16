using ServiceStack.Script;
using System;
using UnityEditor;
using UnityEngine;

// specifies that C must subclass Castable - I'm not sure that this is what I want yet
[Serializable]
public class FieldExpression<C, T> where C : Cast {
    [SerializeField] public string expression;
    public T Value { get; set; }

    public FieldExpression(string initialExpression) {
        expression = initialExpression;
    }

    public void RenderValue(
        C context,
        Lisp.Interpreter interpreter,
        ScriptContext scriptContext
    ) {
        try {
            scriptContext.Args["this"] = context; // I'm just having the supplied "C" implicitly called "this"
            Value = (T)Convert.ChangeType(interpreter.ReplEval(scriptContext, null, expression), typeof(T));
        } catch (Exception e) {
            Debug.LogError($"Exception on Lisp expression: {expression}");
            throw e;
        }
    }
}

public static class FieldExpressionParserFactory {
    private static FieldExpressionParser _fieldExpressionParser;
    public static FieldExpressionParser singleton { get {
            if (_fieldExpressionParser==null) {
                _fieldExpressionParser = new();
            }

            return _fieldExpressionParser;
        }
    }
}

public class FieldExpressionParser {
    // ref: https://sharpscript.net/lisp/unity#annotated-unity-repl-transcript
    private Lisp.Interpreter interpreter;
    private ScriptContext scriptContext;

    public class CastableLispAccessors : ScriptMethods {
        public int HP(IDamageable d) => d.HP;
        public int AP(IDamageable d) => d.AP;
        public float Speed(ICasts c) => (c is IMoves moves) ? moves.Velocity.magnitude : 0f;
        public ICasts Caster(Cast c) => c.Caster;
        public string Data(Cast c) => c.Data;
        public int Duration(Cast c) => c.Duration;
        public int Frame(Cast c) => c.Frame;
        public Cast Parent(Cast c) => c.Parent;
    }

    public FieldExpressionParser() {
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

    public void RenderValue<C, T>(C context, FieldExpression<C, T> fieldExpression) where C : Cast {
        fieldExpression.RenderValue(context, interpreter, scriptContext);
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FieldExpression<,>))]
public class FieldExpressionPropertyDrawer : PropertyDrawer {
    // reference: https://www.youtube.com/watch?v=ur-qy6SjVQw
    private SerializedProperty expression;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (expression== null) {
            expression = property.FindPropertyRelative("expression");
        }

        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PropertyField(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            expression,
            new GUIContent(property.displayName)
        );

        EditorGUI.EndProperty();
    }
}
#endif