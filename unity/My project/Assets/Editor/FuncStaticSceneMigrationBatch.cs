using UnityEditor;
using UnityEditor.SceneManagement;

public static class FuncStaticSceneMigrationBatch
{
    private const string Train1ScenePath = "Assets/HubWorld/Scenes/train1.unity";

    [MenuItem("Tools/Func/Regenerate All Func Statics In train1")]
    public static void RegenerateTrain1FuncStaticsMenu()
    {
        RegenerateTrain1FuncStatics();
    }

    public static void RegenerateTrain1FuncStatics()
    {
        EditorSceneManager.OpenScene(Train1ScenePath);
        CNCTrainingMachineStaticSceneBuilder.CreateStaticCNCTrainingMachineInScene();
        BreakerShutdownStationStaticSceneBuilder.CreateStaticBreakerShutdownStationInScene();
    }

    public static void RegenerateTrain1FuncStaticsBatch()
    {
        RegenerateTrain1FuncStatics();
        EditorApplication.Exit(0);
    }
}
