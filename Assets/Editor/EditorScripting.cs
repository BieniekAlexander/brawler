using System;
using System.Reflection;
using UnityEditor;
public class InspectorLockToggle {
    /// <summary>
    /// Source: https://discussions.unity.com/t/shortcut-key-for-lock-inspector/449578/18
    /// </summary>
    [MenuItem("Tools/Toggle Lock ^l")]
    static void ToggleInspectorLock() // Inspector must be inspecting something to be locked
    {
        EditorWindow inspectorToBeLocked = EditorWindow.mouseOverWindow; // "EditorWindow.focusedWindow" can be used instead

        if (inspectorToBeLocked != null  && inspectorToBeLocked.GetType().Name == "InspectorWindow") {
            Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
            PropertyInfo propertyInfo = type.GetProperty("isLocked");
            bool value = (bool)propertyInfo.GetValue(inspectorToBeLocked, null);
            propertyInfo.SetValue(inspectorToBeLocked, !value, null);
            inspectorToBeLocked.Repaint();
        }
    }
}