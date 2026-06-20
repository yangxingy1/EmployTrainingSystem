using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TrainingBackendClient : MonoBehaviour
{
    public static TrainingBackendClient Active { get; private set; }

    public string backendBaseUrl = "http://127.0.0.1:8000";
    public int studentId = 0;
    public int attemptId = 0;
    public int assignmentId = 0;
    public string launchSceneName = "";

    int _activeAttemptId;
    string _activeSceneName = "";
    bool _autoLoadLaunchSceneAttempted;

    [Serializable]
    class ActiveAttemptResponse
    {
        public bool active;
        public int attempt_id;
        public int assignment_id;
        public int student_id;
        public int task_id;
        public string scene_name;
        public string status;
    }

    [Serializable]
    class StartAttemptRequest
    {
        public int student_id;
        public int assignment_id;
    }

    [Serializable]
    class StartAttemptResponse
    {
        public int attempt_id;
        public int assignment_id;
        public int student_id;
        public int task_id;
        public string scene_name;
        public string status;
    }

    [Serializable]
    class LauncherTaskContext
    {
        public int student_id;
        public int assignment_id;
        public int task_id;
        public int attempt_id;
        public string scene_name;
        public string backend_url;
        public string username;
        public string status;
    }

    [Serializable]
    class TrainingResultRequest
    {
        public int attempt_id;
        public int student_id;
        public string scene_name;
        public string sub_task_id;
        public int score;
        public int train_time;
        public string started_at;
        public string finished_at;
        public TrainingStepReport[] steps;
        public TrainingErrorReport[] errors;
    }

    public static TrainingBackendClient EnsureExists()
    {
        if (Active != null) return Active;

        Active = FindObjectOfType<TrainingBackendClient>();
        if (Active != null) return Active;

        var go = new GameObject("TrainingBackendClient");
        DontDestroyOnLoad(go);
        Active = go.AddComponent<TrainingBackendClient>();
        return Active;
    }

    void Awake()
    {
        if (Active != null && Active != this)
        {
            Destroy(gameObject);
            return;
        }

        Active = this;
        DontDestroyOnLoad(gameObject);
        ApplyLaunchArgs();
    }

    IEnumerator Start()
    {
        yield return null;
        AutoLoadLaunchSceneIfRequested();
    }

    void ApplyLaunchArgs()
    {
        string[] args = Environment.GetCommandLineArgs();
        string backendArg = GetArg(args, "--backend-url");
        string studentArg = GetArg(args, "--student-id");
        string attemptArg = GetArg(args, "--attempt-id");
        string sceneArg = GetArg(args, "--scene-name");

        if (!string.IsNullOrEmpty(backendArg))
            backendBaseUrl = backendArg;

        if (int.TryParse(studentArg, out int parsedStudentId))
            studentId = parsedStudentId;

        if (int.TryParse(attemptArg, out int parsedAttemptId))
            attemptId = parsedAttemptId;

        string assignmentArg = GetArg(args, "--assignment-id");
        if (int.TryParse(assignmentArg, out int parsedAssignmentId))
            assignmentId = parsedAssignmentId;

        if (!string.IsNullOrEmpty(sceneArg))
            launchSceneName = sceneArg;

        ApplyLauncherTaskFallback();

        if (!string.IsNullOrEmpty(launchSceneName)
            && SceneNameAliases.IsLeadTrainScene(SceneNameAliases.ToPublicSceneName(launchSceneName)))
        {
            FactoryOneSceneController.ClearOneShotStartCameraReturnOverride();
        }

        if (studentId > 0)
            PlayerPrefs.SetInt("TrainingStudentId", studentId);

        if (attemptId > 0 && !string.IsNullOrEmpty(launchSceneName))
        {
            _activeAttemptId = attemptId;
            _activeSceneName = SceneNameAliases.ToPublicSceneName(launchSceneName);
            Debug.Log("[TrainingBackend] Launch context loaded: student=" + studentId + ", attempt=" + attemptId + ", scene=" + launchSceneName + ", backend=" + backendBaseUrl);
        }
    }

    void ApplyLauncherTaskFallback()
    {
        var context = LoadLauncherTaskContext();
        if (context == null) return;

        if (string.IsNullOrEmpty(GetArg(Environment.GetCommandLineArgs(), "--backend-url")) && !string.IsNullOrEmpty(context.backend_url))
            backendBaseUrl = context.backend_url;

        if (studentId <= 0 && context.student_id > 0)
            studentId = context.student_id;

        if (attemptId <= 0 && context.attempt_id > 0)
            attemptId = context.attempt_id;

        if (assignmentId <= 0 && context.assignment_id > 0)
            assignmentId = context.assignment_id;

        if (string.IsNullOrEmpty(launchSceneName) && !string.IsNullOrEmpty(context.scene_name))
            launchSceneName = context.scene_name;
    }

    public bool HasActiveAttemptForScene(string sceneName)
    {
        string normalized = SceneNameAliases.ToPublicSceneName(sceneName);
        return _activeAttemptId > 0 && _activeSceneName == normalized;
    }

    public IEnumerator StartTrainingAttemptRoutine(int sid, int aid)
    {
        if (sid <= 0 || aid <= 0)
        {
            Debug.LogWarning("[TrainingBackend] Skip start attempt: invalid student_id or assignment_id.");
            yield break;
        }

        string url = backendBaseUrl.TrimEnd('/') + "/training/start";
        var payload = new StartAttemptRequest
        {
            student_id = sid,
            assignment_id = aid
        };
        string json = JsonUtility.ToJson(payload);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        Debug.Log("[TrainingBackend] Start attempt: " + url + " | " + json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[TrainingBackend] Start attempt failed: "
                    + request.responseCode
                    + " | "
                    + request.error
                    + " | "
                    + request.downloadHandler.text);
                yield break;
            }

            var response = JsonUtility.FromJson<StartAttemptResponse>(request.downloadHandler.text);
            if (response == null || response.attempt_id <= 0)
            {
                Debug.LogWarning("[TrainingBackend] Start attempt returned invalid payload: " + request.downloadHandler.text);
                yield break;
            }

            studentId = sid;
            assignmentId = aid;
            attemptId = response.attempt_id;
            _activeAttemptId = response.attempt_id;
            _activeSceneName = SceneNameAliases.ToPublicSceneName(response.scene_name);
            PlayerPrefs.SetInt("TrainingStudentId", sid);
            Debug.Log("[TrainingBackend] Attempt started: #"
                + _activeAttemptId
                + " scene="
                + _activeSceneName
                + " assignment="
                + aid);
        }
    }

    LauncherTaskContext LoadLauncherTaskContext()
    {
        foreach (var path in LauncherTaskContextCandidates())
        {
            try
            {
                if (!File.Exists(path)) continue;

                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) continue;

                var context = JsonUtility.FromJson<LauncherTaskContext>(json);
                if (context != null)
                {
                    Debug.Log("[TrainingBackend] Launcher task context loaded: " + path);
                    return context;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[TrainingBackend] Failed to read launcher task context: " + path + " | " + ex.Message);
            }
        }

        return null;
    }

    string[] LauncherTaskContextCandidates()
    {
        string dataDir = Application.dataPath;
        string exeDir = Directory.GetParent(dataDir)?.FullName ?? dataDir;
        string launcherDir = Directory.GetParent(exeDir)?.FullName ?? exeDir;

        return new[]
        {
            Path.Combine(exeDir, "current_task.json"),
            Path.Combine(launcherDir, "current_task.json")
        };
    }

    void AutoLoadLaunchSceneIfRequested()
    {
        if (_autoLoadLaunchSceneAttempted) return;
        _autoLoadLaunchSceneAttempted = true;

        if (attemptId <= 0 || string.IsNullOrEmpty(launchSceneName)) return;

        string targetScene = SceneNameAliases.ToPublicSceneName(launchSceneName);
        if (!IsLauncherTrainingScene(targetScene)) return;

        if (SceneNameAliases.IsLeadTrainScene(targetScene))
            FactoryOneSceneController.ClearOneShotStartCameraReturnOverride();

        string activeScene = SceneNameAliases.ToPublicSceneName(SceneManager.GetActiveScene().name);
        if (activeScene == targetScene) return;

        Debug.Log("[TrainingBackend] Auto loading launch scene: " + targetScene);
        SceneFlow.EnsureExists().LoadScene(targetScene);
    }

    static bool IsLauncherTrainingScene(string sceneName)
    {
        return SceneNameAliases.IsLeadTrainScene(sceneName) || sceneName == "train2";
    }

    string GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return "";
    }

    public void PullActiveAttemptForCurrentScene()
    {
        int sid = ResolveStudentId();
        if (sid <= 0)
        {
            Debug.LogWarning("[TrainingBackend] studentId is not configured. Set it on TrainingBackendClient or PlayerPrefs key TrainingStudentId.");
            return;
        }

        string sceneName = SceneNameAliases.ToPublicSceneName(SceneManager.GetActiveScene().name);
        StartCoroutine(PullActiveAttemptRoutine(sid, sceneName));
    }

    public void UploadReport(TrainingReportPayload report)
    {
        int sid = ResolveStudentId();
        if (sid <= 0)
        {
            Debug.LogWarning("[TrainingBackend] Skip upload: studentId is not configured.");
            return;
        }

        string reportSceneName = SceneNameAliases.ToPublicSceneName(report.sceneName);
        if (_activeAttemptId <= 0 || _activeSceneName != reportSceneName)
        {
            StartCoroutine(PullThenUploadRoutine(sid, report));
            return;
        }

        StartCoroutine(UploadReportRoutine(sid, _activeAttemptId, report));
    }

    int ResolveStudentId()
    {
        if (studentId > 0) return studentId;
        return PlayerPrefs.GetInt("TrainingStudentId", 0);
    }

    IEnumerator PullThenUploadRoutine(int sid, TrainingReportPayload report)
    {
        yield return PullActiveAttemptRoutine(sid, SceneNameAliases.ToPublicSceneName(report.sceneName));
        if (_activeAttemptId <= 0)
        {
            Debug.LogWarning("[TrainingBackend] Skip upload: no running attempt found for student=" + sid + ", scene=" + SceneNameAliases.ToPublicSceneName(report.sceneName));
            yield break;
        }

        yield return UploadReportRoutine(sid, _activeAttemptId, report);
    }

    IEnumerator PullActiveAttemptRoutine(int sid, string sceneName)
    {
        string url = backendBaseUrl.TrimEnd('/') + "/training/active?student_id=" + sid + "&scene_name=" + UnityWebRequest.EscapeURL(sceneName);
        Debug.Log("[TrainingBackend] Pull active attempt: " + url);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[TrainingBackend] Pull active attempt failed: " + request.responseCode + " | " + request.error + " | " + request.downloadHandler.text);
                yield break;
            }

            var response = JsonUtility.FromJson<ActiveAttemptResponse>(request.downloadHandler.text);
            if (response == null || !response.active)
            {
                _activeAttemptId = 0;
                _activeSceneName = "";
                Debug.LogWarning("[TrainingBackend] No active attempt for student=" + sid + ", scene=" + sceneName);
                yield break;
            }

            _activeAttemptId = response.attempt_id;
            _activeSceneName = response.scene_name;
            Debug.Log("[TrainingBackend] Active attempt loaded: #" + _activeAttemptId + " scene=" + _activeSceneName);
        }
    }

    IEnumerator UploadReportRoutine(int sid, int attemptId, TrainingReportPayload report)
    {
        var payload = new TrainingResultRequest
        {
            attempt_id = attemptId,
            student_id = sid,
            scene_name = SceneNameAliases.ToPublicSceneName(report.sceneName),
            sub_task_id = report.taskId,
            score = report.score,
            train_time = report.trainTime,
            started_at = report.startedAt,
            finished_at = report.finishedAt,
            steps = report.steps != null ? report.steps.ToArray() : new TrainingStepReport[0],
            errors = report.errors != null ? report.errors.ToArray() : new TrainingErrorReport[0]
        };

        string json = JsonUtility.ToJson(payload);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        string url = backendBaseUrl.TrimEnd('/') + "/training/result";
        Debug.Log("[TrainingBackend] Upload report: " + url + " | " + json);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[TrainingBackend] Upload report failed: " + request.responseCode + " | " + request.error + " | " + request.downloadHandler.text);
                yield break;
            }

            Debug.Log("[TrainingBackend] Training report uploaded: attempt #" + attemptId + " | " + request.downloadHandler.text);
        }
    }
}
