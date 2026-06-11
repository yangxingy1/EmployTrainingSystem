using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow : MonoBehaviour
{
    public static SceneFlow Instance { get; private set; }

    bool _isLoading;

    public static SceneFlow EnsureExists()
    {
        if (Instance != null) return Instance;

        var go = new GameObject("SceneFlow");
        return go.AddComponent<SceneFlow>();
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

    public void LoadScene(string sceneName)
    {
        if (_isLoading) return;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void EnterTraining(AssignedTask task, Vector3 hubReturnPosition)
    {
        if (task == null)
        {
            Debug.LogWarning("[SceneFlow] Cannot enter training because task is null.");
            return;
        }

        var session = SessionManager.EnsureExists();
        session.selectedTaskId = task.taskId;
        session.hubReturnPosition = hubReturnPosition;
        session.hasHubReturnPosition = true;

        Debug.Log($"[SceneFlow] Enter training: task={task.taskId}, scene={task.sceneName}, return={hubReturnPosition}");
        LoadScene(task.sceneName);
    }

    public void ReturnToHub()
    {
        Debug.Log("[SceneFlow] Return to HubWorld");
        LoadScene("HubWorld");
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isLoading = true;
        Debug.Log($"[SceneFlow] Loading scene: {sceneName}");

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            Debug.Log($"[SceneFlow] Loading {sceneName}: {op.progress:P0}");
            yield return null;
        }

        Debug.Log($"[SceneFlow] Loaded scene: {sceneName}");
        _isLoading = false;
    }
}
