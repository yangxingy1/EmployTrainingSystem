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
        session.returnSceneName = SceneManager.GetActiveScene().name;

        Debug.Log($"[SceneFlow] Enter training: task={task.taskId}, scene={task.sceneName}, return={hubReturnPosition}");
        LoadScene(task.sceneName);
    }

    public void ReturnToHub()
    {
        var session = SessionManager.EnsureExists();
        var targetScene = string.IsNullOrEmpty(session.returnSceneName) ? "HubWorld" : session.returnSceneName;
        Debug.Log($"[SceneFlow] Return to {targetScene}");
        LoadScene(targetScene);
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isLoading = true;
        string buildSceneName = SceneNameAliases.ToBuildSceneName(sceneName);
        string buildScenePath = SceneNameAliases.ToBuildScenePath(buildSceneName);
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(buildScenePath);
        bool canLoad = Application.CanStreamedLevelBeLoaded(buildSceneName);

        Debug.Log(
            $"[SceneSwitch] About to load: requested={sceneName}, build={buildSceneName}, " +
            $"path={buildScenePath}, buildIndex={buildIndex}, canLoad={canLoad}");

        if (!canLoad && buildIndex < 0)
        {
            Debug.LogError(
                $"[SceneSwitch] Scene is not in Build Settings or cannot be streamed: " +
                $"requested={sceneName}, build={buildSceneName}, path={buildScenePath}");
            _isLoading = false;
            yield break;
        }

        Debug.Log(buildSceneName == sceneName
            ? $"[SceneFlow] Loading scene: {sceneName}"
            : $"[SceneFlow] Loading scene: {sceneName} -> {buildSceneName}");

        if (SceneNameAliases.IsLeadTrainScene(sceneName) || SceneNameAliases.IsLeadTrainScene(buildSceneName))
        {
            FactoryOneSceneController.ClearOneShotStartCameraReturnOverride();
        }

        var op = SceneManager.LoadSceneAsync(buildSceneName);
        while (!op.isDone)
        {
            Debug.Log($"[SceneFlow] Loading {buildSceneName}: {op.progress:P0}");
            yield return null;
        }

        Debug.Log($"[SceneFlow] Loaded scene: {buildSceneName}");
        _isLoading = false;
    }
}
