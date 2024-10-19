using ServiceStack.Script;
using System;
using UnityEditor;
using UnityEngine;


// specifies that C must subclass Castable - I'm not sure that this is what I want yet
[Serializable]
public class FieldExpression<C, T> where C: Castable {
    [SerializeField] string expression;

    public FieldExpression(string initialExpression) {
        expression = initialExpression;
    }

    public T GetValue(
        C context,
        Lisp.Interpreter interpreter,
        ScriptContext scriptContext
    ) {
        try {
            scriptContext.Args["this"] = context; // I'm just having the supplied "C" implicitly called "this"
            return (T)Convert.ChangeType(interpreter.ReplEval(scriptContext, null, expression), typeof(T));
        } catch (Exception e) {
            Debug.LogError($"Exception on Lisp expression: {expression}");
            throw e;
        }
    }
}


[CustomPropertyDrawer(typeof(FieldExpression<,>))]
public class FieldExpressionPropertyDrawer: PropertyDrawer {
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