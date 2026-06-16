using UnityEngine;

public class FreshTrainCubeMarker : MonoBehaviour
{
    public string taskId;
    public string displayName;
    public string sceneName;
    public string instruction;

    public void Configure(string taskId, string displayName, string sceneName, string instruction)
    {
        this.taskId = taskId;
        this.displayName = displayName;
        this.sceneName = sceneName;
        this.instruction = instruction;
    }
}
