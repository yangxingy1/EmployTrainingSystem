using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptSceneValidator
{
    private const string Train1ScenePath = "Assets/HubWorld/Scenes/train1.unity";
    private const string LeadTrain1ScenePath = "Assets/HubWorld/Scenes/leadTrain1.unity";

    [MenuItem("Tools/Validation/Validate train1 and leadTrain1 Missing Scripts")]
    public static void ValidateTrainingScenesMenu()
    {
        ValidateTrainingScenes();
    }

    public static bool ValidateTrainingScenes()
    {
        bool trainOk = ValidateSceneAtPath(Train1ScenePath);
        bool leadOk = ValidateSceneAtPath(LeadTrain1ScenePath);
        return trainOk && leadOk;
    }

    public static void ValidateTrainingScenesBatch()
    {
        EditorApplication.Exit(ValidateTrainingScenes() ? 0 : 1);
    }

    public static bool ValidateSceneAtPath(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        return ValidateOpenScene(scene);
    }

    public static bool ValidateOpenScene(Scene scene)
    {
        int missingCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            missingCount += LogMissingScriptsRecursive(root);
        }

        if (missingCount == 0)
        {
            Debug.Log("[MissingScript] Validation passed: " + scene.path);
            return true;
        }

        Debug.LogError("[MissingScript] Validation failed: " + scene.path + " has " + missingCount + " missing script component(s).");
        return false;
    }

    private static int LogMissingScriptsRecursive(GameObject obj)
    {
        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj);
        if (count > 0)
        {
            Debug.LogError("[MissingScript] " + obj.name + " has " + count + " missing script component(s).", obj);
        }

        foreach (Transform child in obj.transform)
        {
            count += LogMissingScriptsRecursive(child.gameObject);
        }

        return count;
    }
}
