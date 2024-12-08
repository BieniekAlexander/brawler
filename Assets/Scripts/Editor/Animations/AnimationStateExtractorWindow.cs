#if UNITY_EDITOR
using UnityEditor;

public class AnimationStateExtractorWindow: EditorWindow {
    [MenuItem("Custom/Generate Walls %g")] // in the editor, press "CTRL+g" to open this window
    public static void OpenWindow() {
       GetWindow<AnimationStateExtractorWindow>();
    }

    void OnGUI() {
        if(EditorGUILayout.LinkButton("Generate Animation States Class")) {
            AnimationStateExtractor.generateClass();
        }
    }
}
#endif