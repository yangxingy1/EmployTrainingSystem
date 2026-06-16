using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LeadTrainGuideSceneSetup
{
    const string AddMenuPath = "Tools/LeadTrain/Add Guide Controller To Current Scene";
    const string ResetMenuPath = "Tools/LeadTrain/Reset Guide Step Order In Current Scene";

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
        Debug.Log("[LeadTrain] Reset guide step order to: Fire -> Cabinet -> Breaker -> CNC");
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
