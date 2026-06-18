public static class SceneNameAliases
{
    public const string LeadTrainPublicSceneName = "lead-train1";
    public const string LeadTrainBuildSceneName = "leadTrain1";

    public static string ToBuildSceneName(string sceneName)
    {
        return sceneName == LeadTrainPublicSceneName ? LeadTrainBuildSceneName : sceneName;
    }

    public static string ToPublicSceneName(string sceneName)
    {
        return sceneName == LeadTrainBuildSceneName ? LeadTrainPublicSceneName : sceneName;
    }

    public static string ToBuildScenePath(string sceneName)
    {
        string buildSceneName = ToBuildSceneName(sceneName);
        switch (buildSceneName)
        {
            case "RotaryValve":
                return "Assets/Scenes/RotaryValve/RotaryValve.unity";
            case "ElectricSwitch":
                return "Assets/Scenes/ElectricSwitch/ElectricSwitch.unity";
            case "SampleScene":
                return "Assets/Scenes/SampleScene.unity";
            default:
                return "Assets/HubWorld/Scenes/" + buildSceneName + ".unity";
        }
    }

    public static bool IsLeadTrainScene(string sceneName)
    {
        return sceneName == LeadTrainPublicSceneName || sceneName == LeadTrainBuildSceneName;
    }
}
