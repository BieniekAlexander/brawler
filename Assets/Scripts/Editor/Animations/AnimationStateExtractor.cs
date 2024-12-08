#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using System.CodeDom;
using System.CodeDom.Compiler;
using ServiceStack;
using System.Linq;

public static class AnimationStateExtractor {
    private static Dictionary<string, int> getAnimatorNameHashMap(AnimatorController _controller) {
        Dictionary<string, int> stateNamHashMap = new();
        AnimatorControllerLayer[] allLayer = _controller.layers;

        for (int i = 0; i < allLayer.Length; i++) {
            ChildAnimatorState[] states = allLayer[i].stateMachine.states;

            for (int j = 0; j < states.Length; j++) {
                stateNamHashMap[states[j].state.name] = states[j].state.GetHashCode();
            }
        }

        return stateNamHashMap;
    }

    /// <summary>
    /// Generates a class that represents animation states
    /// </summary>
    /// <remarks>I'm fudging this together - see <see href="https://stackoverflow.com/a/41728640/3600382">here</see></remarks>
    public static void generateClass() {
        AnimatorController animatorController = Resources.Load<AnimatorController>("Animations/animatorController");
        var targetUnit = new CodeCompileUnit();
        CodeNamespace codeNamespace = new CodeNamespace("");
        var AnimationStateMapClass = new CodeTypeDeclaration {
            Name = "AnimationStateMap",
            IsClass = true,
            TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Sealed
        };
        
        targetUnit.Namespaces.Add(codeNamespace);
        codeNamespace.Types.Add(AnimationStateMapClass);
        codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

        Dictionary<string, int> animatorNameHashMap = getAnimatorNameHashMap(animatorController);
        foreach (KeyValuePair<string, int> pair in animatorNameHashMap)
            Debug.Log(pair);

        // initializer lists not supported, so I hacked together this very ugly implementation
        // https://stackoverflow.com/a/53135875/3600382
        CodeMemberField map = new CodeMemberField(
            "Dictionary<string, int> nameHashMap",
            @$" = new Dictionary<string, int>(){{
                {
                    (
                        from i in animatorNameHashMap
                        select $"{{\"{i.Key}\", {i.Value}}}"
                    ).ToArray().Join(",\n")
                }
            }}"
        );

        map.Attributes = MemberAttributes.Public;
        AnimationStateMapClass.Members.Add(
            map
        );

        CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
        CodeGeneratorOptions options = new CodeGeneratorOptions {BracingStyle = "C"};
        const string outputFileName = "AnimationStateMap.cs";
        var path = System.IO.Path.Combine(Application.dataPath, "Scripts/Visuals/Models");
        path = System.IO.Path.Combine(path, outputFileName);
        using (System.IO.StreamWriter sourceWriter = new System.IO.StreamWriter(path)) {
            provider.GenerateCodeFromCompileUnit(
                targetUnit, sourceWriter, options);
        }
    }
}
#endif