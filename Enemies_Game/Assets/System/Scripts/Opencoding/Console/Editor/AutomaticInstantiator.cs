using System.IO;
using Opencoding.Console;
using Opencoding.Console.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad] 
class AutomaticInstantiator
{
	static AutomaticInstantiator()
	{
#if UNITY_2017_2_OR_NEWER
		EditorApplication.playModeStateChanged += PlaymodeStateChanged;
#else
        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
#endif
	}

#if UNITY_2017_2_OR_NEWER
    private static void PlaymodeStateChanged(PlayModeStateChange playModeStateChange)
    {
        if (playModeStateChange == PlayModeStateChange.EnteredPlayMode)
        {
            LoadConsolePrefab();
        }
    }
#else
    private static void PlaymodeStateChanged()
	{
		if (EditorApplication.isPlaying)
		{
		    LoadConsolePrefab();
		}
	}
#endif

    private static void LoadConsolePrefab()
    {
        if (!DebugConsoleEditorSettings.AutomaticallyLoadConsoleInEditor)
            return;

        if (Object.FindObjectOfType<DebugConsole>() == null)
        {
            var debugConsolePath = Path.Combine(DebugConsoleEditorSettings.OpencodingDirectoryLocation,
                "Console/Prefabs/DebugConsole.prefab");
            var prefab = AssetDatabase.LoadMainAssetAtPath(debugConsolePath);
            if (prefab == null)
            {
                Debug.LogWarning("Couldn't load DebugConsole as the DebugConsole prefab couldn't be found at " +
                                 debugConsolePath +
                                 ". If you have moved the OpenCoding folder, please update the location in DebugConsoleEditorSettings.");
                return;
            }

            var go = Object.Instantiate(prefab);
            go.name = "DebugConsole (Automatically Instantiated)";
        }
    }
}