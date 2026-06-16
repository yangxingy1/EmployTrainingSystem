using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class BreakerRuntimeScriptReferenceFixer
{
    private const string MigrationKey = "EmployTraining.BreakerRuntimeScriptRef.v1";
    private const string RuntimeScriptPath = "Assets/Scripts/BreakerShutdownStationRuntime.cs";
    private const string Train1ScenePath = "Assets/HubWorld/Scenes/train1.unity";

    static BreakerRuntimeScriptReferenceFixer()
    {
        EditorApplication.delayCall += RunIfNeeded;
    }

    [MenuItem("Tools/Func/Fix Breaker Runtime Script References In train1")]
    public static void FixTrain1Menu()
    {
        FixSceneReferences(Train1ScenePath);
    }

    private static void RunIfNeeded()
    {
        if (EditorPrefs.GetBool(MigrationKey, false))
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        FixSceneReferences(Train1ScenePath);
        EditorPrefs.SetBool(MigrationKey, true);
    }

    private static void FixSceneReferences(string scenePath)
    {
        MonoScript runtimeScript = AssetDatabase.LoadAssetAtPath<MonoScript>(RuntimeScriptPath);
        if (runtimeScript == null)
        {
            Debug.LogWarning("[BreakerFix] Runtime script not found: " + RuntimeScriptPath);
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath);
        int fixedCount = 0;

        foreach (BreakerShutdownStationRuntime runtime in UnityEngine.Object.FindObjectsByType<BreakerShutdownStationRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (FixComponentScript(runtime, runtimeScript))
            {
                fixedCount++;
            }
        }

        foreach (BreakerStartButtonInteractable interactable in UnityEngine.Object.FindObjectsByType<BreakerStartButtonInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (FixComponentScript(interactable, runtimeScript))
            {
                fixedCount++;
            }
        }

        foreach (BreakerSwitchRuntime breaker in UnityEngine.Object.FindObjectsByType<BreakerSwitchRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (FixComponentScript(breaker, runtimeScript))
            {
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scene.path, false);
            Debug.Log("[BreakerFix] Repaired " + fixedCount + " breaker runtime script reference(s) in " + scenePath + ".");
        }
    }

    private static bool FixComponentScript(Component component, MonoScript targetScript)
    {
        if (component == null || targetScript == null)
        {
            return false;
        }

        MonoScript currentScript = MonoScript.FromMonoBehaviour(component as MonoBehaviour);
        if (currentScript == targetScript)
        {
            return false;
        }

        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
        if (scriptProperty == null)
        {
            return false;
        }

        scriptProperty.objectReferenceValue = targetScript;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }
}
