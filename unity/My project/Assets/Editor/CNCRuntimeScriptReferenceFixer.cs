using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CNCRuntimeScriptReferenceFixer
{
    private const string MigrationKey = "EmployTraining.CNCRuntimeScriptRef.v1";
    private const string RuntimeScriptPath = "Assets/Scripts/CNCTrainingMachineRuntime.cs";
    private const string Train1ScenePath = "Assets/HubWorld/Scenes/train1.unity";

    static CNCRuntimeScriptReferenceFixer()
    {
        EditorApplication.delayCall += RunIfNeeded;
    }

    [MenuItem("Tools/Func/Fix CNC Runtime Script References In train1")]
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
            Debug.LogWarning("[CNCFix] Runtime script not found: " + RuntimeScriptPath);
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath);
        int fixedCount = 0;

        foreach (CNCTrainingMachineRuntime runtime in Object.FindObjectsByType<CNCTrainingMachineRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (FixComponentScript(runtime, runtimeScript))
            {
                fixedCount++;
            }
        }

        foreach (CNCInteractablePart interactable in Object.FindObjectsByType<CNCInteractablePart>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (FixComponentScript(interactable, runtimeScript))
            {
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scene.path, false);
            Debug.Log("[CNCFix] Repaired " + fixedCount + " CNC runtime script reference(s) in " + scenePath + ".");
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
