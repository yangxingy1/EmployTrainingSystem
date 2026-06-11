using System;

[Serializable]
public class AssignedTask
{
    public string taskId;
    public string displayName;
    public string sceneName;
    public int anchorId;

    public AssignedTask(string taskId, string displayName, string sceneName, int anchorId)
    {
        this.taskId = taskId;
        this.displayName = displayName;
        this.sceneName = sceneName;
        this.anchorId = anchorId;
    }
}
