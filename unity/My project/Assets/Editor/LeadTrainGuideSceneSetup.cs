using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LeadTrainGuideSceneSetup
{
    const string AddMenuPath = "Tools/LeadTrain/Add Guide Controller To Current Scene";
    const string ResetMenuPath = "Tools/LeadTrain/Reset Guide Step Order In Current Scene";
    const string RebuildMenuPath = "Tools/LeadTrain/Rebuild Clean leadTrain1 From train1";
    const string Train1ScenePath = "Assets/HubWorld/Scenes/train1.unity";
    const string LeadTrain1ScenePath = "Assets/HubWorld/Scenes/leadTrain1.unity";

    [MenuItem(AddMenuPath)]
    public static void AddGuideControllerToCurrentScene()
    {
        LeadTrainGuideController existing = Object.FindObjectOfType<LeadTrainGuideController>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("[LeadTrain] Guide controller already exists on: " + existing.gameObject.name);
            return;
        }

        FactoryOneSceneController factoryController = Object.FindObjectOfType<FactoryOneSceneController>();
        GameObject host = factoryController != null ? factoryController.gameObject : new GameObject("LeadTrainGuide");

        if (factoryController == null)
        {
            Undo.RegisterCreatedObjectUndo(host, "Create Lead Train Guide");
        }

        LeadTrainGuideController guide = Undo.AddComponent<LeadTrainGuideController>(host);
        guide.ApplyCanonicalGuideOrder();
        Selection.activeGameObject = host;
        MarkSceneDirty(host);
        Debug.Log("[LeadTrain] Added LeadTrainGuideController to: " + host.name);
    }

    [MenuItem(ResetMenuPath)]
    public static void ResetGuideStepOrderInCurrentScene()
    {
        LeadTrainGuideController guide = Object.FindObjectOfType<LeadTrainGuideController>();
        if (guide == null)
        {
            Debug.LogWarning("[LeadTrain] No LeadTrainGuideController found in the current scene.");
            return;
        }

        Undo.RecordObject(guide, "Reset Lead Train Guide Step Order");
        guide.enforceCanonicalOrderOnAwake = true;
        guide.ApplyCanonicalGuideOrder();
        EditorUtility.SetDirty(guide);

        ValidateRequiredMachines();
        Selection.activeGameObject = guide.gameObject;
        MarkSceneDirty(guide.gameObject);
        Debug.Log("[LeadTrain] Reset guide step order to: Cabinet -> Breaker -> CNC");
    }

    [MenuItem(RebuildMenuPath)]
    public static void RebuildCleanLeadTrain1FromTrain1Menu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("[LeadTrain] Rebuild leadTrain1 canceled because open scene changes were not saved.");
            return;
        }

        RebuildCleanLeadTrain1FromTrain1();
    }

    public static bool RebuildCleanLeadTrain1FromTrain1()
    {
        if (!System.IO.File.Exists(Train1ScenePath))
        {
            Debug.LogError("[LeadTrain] Cannot rebuild leadTrain1. Source scene is missing: " + Train1ScenePath);
            return false;
        }

        var scene = EditorSceneManager.OpenScene(Train1ScenePath, OpenSceneMode.Single);
        FuncStaticSceneMigrationBatch.RegenerateFuncStaticsInCurrentScene();

        if (!MissingScriptSceneValidator.ValidateOpenScene(scene))
        {
            Debug.LogError("[LeadTrain] Cannot rebuild leadTrain1 because train1 still has missing scripts.");
            return false;
        }

        if (!EditorSceneManager.SaveScene(scene, Train1ScenePath))
        {
            Debug.LogError("[LeadTrain] Failed to save repaired source scene: " + Train1ScenePath);
            return false;
        }

        AddGuideControllerToCurrentScene();
        ResetGuideStepOrderInCurrentScene();

        if (!MissingScriptSceneValidator.ValidateOpenScene(scene))
        {
            Debug.LogError("[LeadTrain] Cannot save leadTrain1 because the rebuilt scene still has missing scripts.");
            return false;
        }

        if (!EditorSceneManager.SaveScene(scene, LeadTrain1ScenePath))
        {
            Debug.LogError("[LeadTrain] Failed to save rebuilt scene: " + LeadTrain1ScenePath);
            return false;
        }

        AssetDatabase.ImportAsset(LeadTrain1ScenePath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        var leadScene = EditorSceneManager.OpenScene(LeadTrain1ScenePath, OpenSceneMode.Single);
        if (!MissingScriptSceneValidator.ValidateOpenScene(leadScene))
        {
            Debug.LogError("[LeadTrain] Rebuilt leadTrain1 failed missing-script validation after import.");
            return false;
        }

        Debug.Log("[LeadTrain] Rebuilt clean leadTrain1 scene from train1: " + LeadTrain1ScenePath);
        return true;
    }

    public static void RebuildCleanLeadTrain1FromTrain1Batch()
    {
        EditorApplication.Exit(RebuildCleanLeadTrain1FromTrain1() ? 0 : 1);
    }

    static void ValidateRequiredMachines()
    {
        LeadTrainGuideController.GuideStep[] steps = LeadTrainGuideController.CreateDefaultSteps();
        for (int i = 0; i < steps.Length; i++)
        {
            string machineName = steps[i].machineObjectName;
            if (GameObject.Find(machineName) == null)
            {
                Debug.LogWarning("[LeadTrain] Missing machine in scene for step " + (i + 1) + ": " + machineName);
            }
        }
    }

    static void MarkSceneDirty(GameObject host)
    {
        if (host != null)
        {
            EditorSceneManager.MarkSceneDirty(host.scene);
        }
    }
}
