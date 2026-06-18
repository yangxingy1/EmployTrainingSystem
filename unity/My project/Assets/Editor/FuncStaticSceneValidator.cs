using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FuncStaticSceneValidator
{
    private const string Train1ScenePath = "Assets/HubWorld/Scenes/train1.unity";

    [MenuItem("Tools/Func/Validate Func Statics In train1")]
    public static void ValidateTrain1FuncStatics()
    {
        EditorSceneManager.OpenScene(Train1ScenePath);

        bool hasCnc = GameObject.Find(CNCTrainingMachineBuilder.StaticMachineName) != null;
        bool hasBreaker = GameObject.Find(BreakerShutdownStationBuilder.StaticStationName) != null;
        bool hasCabinet = GameObject.Find(ElectricalControlCabinetBuilder.StaticCabinetName) != null;
        bool hasExtinguisher = GameObject.Find(FireExtinguisherBuilder.StaticExtinguisherName) != null;

        Transform cncRuntime = FindGeneratedRoot(CNCTrainingMachineBuilder.StaticMachineName, CNCTrainingMachineBuilder.GeneratedModelName);
        Transform breakerRuntime = FindGeneratedRoot(BreakerShutdownStationBuilder.StaticStationName, BreakerShutdownStationBuilder.GeneratedModelName);
        Transform cabinetRuntime = FindGeneratedRoot(ElectricalControlCabinetBuilder.StaticCabinetName, ElectricalControlCabinetBuilder.GeneratedModelName);
        Transform extinguisherRuntime = FindGeneratedRoot(FireExtinguisherBuilder.StaticExtinguisherName, FireExtinguisherBuilder.GeneratedModelName);

        bool cncRuntimeReady = cncRuntime != null && cncRuntime.GetComponent<CNCTrainingMachineRuntime>() != null;
        bool breakerRuntimeReady = breakerRuntime != null && breakerRuntime.GetComponent<BreakerShutdownStationRuntime>() != null;
        bool cabinetRuntimeReady = cabinetRuntime != null && cabinetRuntime.GetComponentInChildren<MainBreakerToggle>(true) != null;
        bool extinguisherRuntimeReady = extinguisherRuntime != null && extinguisherRuntime.GetComponentInChildren<FireExtinguisherGaugeInteraction>(true) != null;

        if (hasCnc && hasBreaker && hasCabinet && hasExtinguisher && cncRuntimeReady && breakerRuntimeReady && cabinetRuntimeReady && extinguisherRuntimeReady)
        {
            Debug.Log("[FuncStatics] Validation passed: cabinet, extinguisher, CNC, and breaker stations are present with runtime components.");
            return;
        }

        Debug.LogWarning(
            "[FuncStatics] Validation failed. "
            + "CNC root=" + hasCnc + ", CNC runtime=" + cncRuntimeReady + ", "
            + "Breaker root=" + hasBreaker + ", Breaker runtime=" + breakerRuntimeReady + ", "
            + "Cabinet root=" + hasCabinet + ", Cabinet runtime=" + cabinetRuntimeReady + ", "
            + "Extinguisher root=" + hasExtinguisher + ", Extinguisher runtime=" + extinguisherRuntimeReady + ". "
            + "Run Tools/Func/Regenerate All Func Statics In train1.");
    }

    private static Transform FindGeneratedRoot(string staticRootName, string generatedRootName)
    {
        GameObject staticRoot = GameObject.Find(staticRootName);
        if (staticRoot == null)
        {
            return null;
        }

        return staticRoot.transform.Find(generatedRootName);
    }
}
