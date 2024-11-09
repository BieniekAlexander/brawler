// TODO document this - https://www.reddit.com/r/csharp/comments/195y1ag/c_scripting_with_dotnetscript_in_vscode/kir5871/
#r "Library/ScriptAssemblies/Main.dll" // path to local DLL, compiled from came project
// TODO find a way to get references to the system's installed Unity stuff, not hardcoded smh
#r "C:/Program Files/Unity/Hub/Editor/6000.0.25f1/Editor/Data/Managed/UnityEngine/UnityEngine.dll"
#r "C:/Program Files/Unity/Hub/Editor/6000.0.25f1/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll"


using UnityEngine;

Console.WriteLine(
        Vector2.Dot(new Vector2(.5f, 1.3f), Vector2.one)
);