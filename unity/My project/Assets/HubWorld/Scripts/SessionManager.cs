using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public string userId = "mock_user_001";
    public string displayName = "Mock Trainee";
    public List<AssignedTask> assignedTasks = new List<AssignedTask>();
    public string selectedTaskId = "";
    public Vector3 hubReturnPosition = Vector3.zero;
    public bool hasHubReturnPosition;

    bool _initialized;

    public static SessionManager EnsureExists()
    {
        if (Instance != null) return Instance;

        var go = new GameObject("SessionManager");
        return go.AddComponent<SessionManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(IAssignedTaskSource taskSource)
    {
        if (_initialized) return;

        assignedTasks = taskSource.GetAssignedTasks();
        _initialized = true;

        Debug.Log($"[SessionManager] Mock user: {displayName} ({userId})");
        foreach (var task in assignedTasks)
            Debug.Log($"[SessionManager] Assigned task: {task.taskId} | {task.displayName} | scene={task.sceneName} | anchor={task.anchorId}");
    }
}
