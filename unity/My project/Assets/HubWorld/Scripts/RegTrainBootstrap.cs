using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class RegTrainBootstrap : MonoBehaviour
{
    [System.Serializable]
    class RegularTrainingOption
    {
        public string taskId;
        public string displayName;
        public string sceneName;
        public string instruction;
        public string[] texturePaths;
        public bool unlocked;
        public bool assigned;
        public int assignmentId;
        public string assignmentStatus;

        public RegularTrainingOption(string taskId, string displayName, string sceneName, string instruction, string[] texturePaths)
        {
            this.taskId = taskId;
            this.displayName = displayName;
            this.sceneName = sceneName;
            this.instruction = instruction;
            this.texturePaths = texturePaths;
        }
    }

    [System.Serializable]
    class StudentAccessResponse
    {
        public int student_id;
        public int company_id;
        public StudentAccessItem[] items;
    }

    [System.Serializable]
    class StudentAccessItem
    {
        public string scene_name;
        public int task_id;
        public string title;
        public string description;
        public bool unlocked;
        public bool assigned;
        public int assignment_id;
        public string status;
    }

    readonly RegularTrainingOption[] _options =
    {
        new RegularTrainingOption(
            "lead_train1_regular",
            "leadTrain1",
            "lead-train1",
            "正式训练一：电闸、CNC、储罐相关手势项目。",
            new[] { "HubPics/Breaker", "HubPics/cnc", "HubPics/tank" }),
        new RegularTrainingOption(
            "train2_regular",
            "train2",
            "train2",
            "正式训练二：管线流程训练项目。",
            new[] { "HubPics/pipe1", "HubPics/pipe2", "HubPics/pipe3" }),
    };

    readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

    Vector2 _scroll;
    int _selectedIndex;
    bool _enteringTraining;
    string _statusMessage = "正在同步训练权限...";
    float _statusUntil;
    GUIStyle _titleStyle;
    GUIStyle _subtitleStyle;
    GUIStyle _rowStyle;
    GUIStyle _rowSelectedStyle;
    GUIStyle _nameStyle;
    GUIStyle _bodyStyle;
    GUIStyle _stampAssignedStyle;
    GUIStyle _stampBlockedStyle;
    GUIStyle _buttonStyle;
    GUIStyle _disabledButtonStyle;
    GUIStyle _smallStyle;
    Texture2D _rowTexture;
    Texture2D _rowSelectedTexture;
    Texture2D _buttonTexture;
    Texture2D _disabledTexture;

    IEnumerator Start()
    {
        SceneFlow.EnsureExists();
        TrainingBackendClient.EnsureExists();

        var session = SessionManager.EnsureExists();
        session.returnSceneName = "entry";
        session.hasHubReturnPosition = false;

        var returnInput = gameObject.AddComponent<ReturnToHubInput>();
        returnInput.fallbackSceneName = "entry";
        returnInput.preferFallbackScene = true;

        SetupCamera();
        SetupLighting();
        BuildQuietBackdrop();
        LoadTextures();

        yield return RefreshAccessRoutine();
        ShowStatus("鼠标滚轮浏览项目，点击进入已分配训练。", 3.0f);
    }

    void Update()
    {
        if (_enteringTraining) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            _selectedIndex = Mathf.Max(0, _selectedIndex - 1);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            _selectedIndex = Mathf.Min(_options.Length - 1, _selectedIndex + 1);
        if (Input.GetKeyDown(KeyCode.E) && _selectedIndex >= 0 && _selectedIndex < _options.Length)
            TryEnterTraining(_options[_selectedIndex]);
    }

    void OnGUI()
    {
        EnsureGuiStyles();

        float margin = Mathf.Clamp(Screen.width * 0.055f, 38f, 86f);
        float headerHeight = 106f;
        var titleRect = new Rect(margin, 22f, Screen.width - margin * 2f, 52f);
        GUI.Label(titleRect, "Regular Training", _titleStyle);
        GUI.Label(new Rect(margin, 74f, Screen.width - margin * 2f, 26f), "按公司解锁和学员分配显示训练入口", _subtitleStyle);

        float listTop = headerHeight;
        float listHeight = Screen.height - listTop - 38f;
        var outer = new Rect(margin, listTop, Screen.width - margin * 2f, listHeight);
        float rowHeight = 162f;
        float contentHeight = (_options.Length + 1) * (rowHeight + 14f) + 12f;

        _scroll = GUI.BeginScrollView(outer, _scroll, new Rect(0f, 0f, outer.width - 22f, contentHeight));
        for (int i = 0; i < _options.Length; i++)
        {
            var row = new Rect(0f, i * (rowHeight + 14f), outer.width - 34f, rowHeight);
            DrawTrainingRow(row, _options[i], i);
        }

        var waitRow = new Rect(0f, _options.Length * (rowHeight + 14f), outer.width - 34f, rowHeight);
        DrawWaitingRow(waitRow);
        GUI.EndScrollView();

        if (Time.time < _statusUntil && !string.IsNullOrEmpty(_statusMessage))
        {
            var statusRect = new Rect(margin, Screen.height - 34f, Screen.width - margin * 2f, 24f);
            GUI.Label(statusRect, _statusMessage, _smallStyle);
        }
    }

    void DrawTrainingRow(Rect rect, RegularTrainingOption option, int index)
    {
        bool selected = index == _selectedIndex;
        GUI.Box(rect, GUIContent.none, selected ? _rowSelectedStyle : _rowStyle);

        var imageY = rect.y + 22f;
        float imageSize = 104f;
        for (int i = 0; i < 3; i++)
        {
            var imageRect = new Rect(rect.x + 22f + i * (imageSize + 10f), imageY, imageSize, imageSize);
            DrawTextureCard(imageRect, GetTexture(option.texturePaths, i));
        }

        float textX = rect.x + 22f + 3f * (imageSize + 10f) + 22f;
        float rightWidth = 172f;
        var nameRect = new Rect(textX, rect.y + 24f, rect.width - textX - rightWidth, 34f);
        GUI.Label(nameRect, option.displayName, _nameStyle);
        GUI.Label(new Rect(textX, rect.y + 60f, 260f, 24f), option.sceneName, _smallStyle);
        GUI.Label(new Rect(textX, rect.y + 90f, rect.width - textX - rightWidth, 48f), option.instruction, _bodyStyle);

        string stampText = option.assigned ? "已分配" : option.unlocked ? "未分配" : "未解锁";
        DrawStamp(new Rect(rect.x + rect.width - 154f, rect.y + 26f, 114f, 42f), stampText, option.assigned);

        var buttonRect = new Rect(rect.x + rect.width - 156f, rect.y + 96f, 120f, 42f);
        bool canEnter = option.unlocked && option.assigned && !_enteringTraining;
        bool clicked = GUI.Button(buttonRect, canEnter ? "进入" : option.unlocked ? "未分配" : "未解锁", canEnter ? _buttonStyle : _disabledButtonStyle);
        if (clicked)
        {
            _selectedIndex = index;
            TryEnterTraining(option);
        }

        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            _selectedIndex = index;
    }

    void DrawWaitingRow(Rect rect)
    {
        GUI.Box(rect, GUIContent.none, _rowStyle);
        for (int i = 0; i < 3; i++)
        {
            var boxRect = new Rect(rect.x + 22f + i * 114f, rect.y + 32f, 104f, 104f);
            GUI.Box(boxRect, GUIContent.none, _disabledButtonStyle);
        }

        float textX = rect.x + 22f + 3f * 114f + 22f;
        GUI.Label(new Rect(textX, rect.y + 34f, 360f, 38f), "等待解锁", _nameStyle);
        GUI.Label(new Rect(textX, rect.y + 76f, 460f, 26f), "Root 为公司开放新项目后会显示在这里。", _bodyStyle);
        DrawStamp(new Rect(rect.x + rect.width - 154f, rect.y + 50f, 114f, 42f), "Waiting", false);
    }

    void DrawTextureCard(Rect rect, Texture2D texture)
    {
        GUI.Box(rect, GUIContent.none, _disabledButtonStyle);
        if (texture != null)
            GUI.DrawTexture(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f), texture, ScaleMode.ScaleAndCrop);
    }

    void DrawStamp(Rect rect, string text, bool assigned)
    {
        var pivot = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(-8f, pivot);
        GUI.Label(rect, text, assigned ? _stampAssignedStyle : _stampBlockedStyle);
        GUI.matrix = matrix;
    }

    void TryEnterTraining(RegularTrainingOption option)
    {
        if (option == null) return;

        if (!option.unlocked)
        {
            ShowStatus("该项目尚未对当前公司解锁。", 2.0f);
            return;
        }

        if (!option.assigned)
        {
            ShowStatus("管理员还未给你分配该任务。", 2.0f);
            return;
        }

        StartCoroutine(EnterTrainingRoutine(option));
    }

    IEnumerator EnterTrainingRoutine(RegularTrainingOption option)
    {
        _enteringTraining = true;

        var session = SessionManager.EnsureExists();
        session.selectedInstruction = option.instruction;
        session.selectedSuccessMessage = "Training complete";

        var task = new AssignedTask(option.taskId, option.displayName, option.sceneName, 0);
        ShowStatus("正在进入: " + option.sceneName, 0.8f);
        yield return new WaitForSeconds(0.25f);

        var backend = TrainingBackendClient.EnsureExists();
        int studentId = ResolveStudentId(backend);
        if (studentId > 0 && option.assignmentId > 0)
        {
            yield return backend.StartTrainingAttemptRoutine(studentId, option.assignmentId);
            if (!backend.HasActiveAttemptForScene(option.sceneName))
            {
                ShowStatus("训练记录创建失败，完成后成绩可能无法上报。", 2.5f);
            }
        }
        else if (studentId <= 0)
        {
            ShowStatus("未绑定学员账号，完成后成绩可能无法上报。", 2.0f);
        }
        else if (option.assignmentId <= 0)
        {
            ShowStatus("当前任务未分配 assignment，完成后成绩可能无法上报。", 2.0f);
        }

        if (SceneNameAliases.IsLeadTrainScene(option.sceneName))
            FactoryOneSceneController.ClearOneShotStartCameraReturnOverride();

        SceneFlow.EnsureExists().EnterTraining(task, Vector3.zero);
    }

    IEnumerator RefreshAccessRoutine()
    {
        ResetAccess();
        ApplyLaunchContextFallback();

        var backend = TrainingBackendClient.EnsureExists();
        int studentId = ResolveStudentId(backend);
        if (studentId <= 0)
        {
            ShowStatus("未绑定学员账号，训练项目暂不开放。", 2.5f);
            yield break;
        }

        string url = backend.backendBaseUrl.TrimEnd('/') + "/task/student-access/" + studentId;
        Debug.Log("[RegTrain] Access query: " + url);
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[RegTrain] Access query failed: " + request.error + " | " + url);
                ShowStatus("权限同步失败，已保留启动上下文中的训练入口。", 2.5f);
                ApplyLaunchContextFallback();
                yield break;
            }

            var response = JsonUtility.FromJson<StudentAccessResponse>(request.downloadHandler.text);
            Debug.Log("[RegTrain] Access response: " + request.downloadHandler.text);
            ApplyAccessResponse(response);
            ApplyLaunchContextFallback();
            ShowStatus("训练权限已同步。", 2.0f);
        }
    }

    static int ResolveStudentId(TrainingBackendClient backend)
    {
        if (backend != null && backend.studentId > 0) return backend.studentId;
        return PlayerPrefs.GetInt("TrainingStudentId", 0);
    }

    void ResetAccess()
    {
        for (int i = 0; i < _options.Length; i++)
        {
            _options[i].unlocked = false;
            _options[i].assigned = false;
            _options[i].assignmentId = 0;
            _options[i].assignmentStatus = "";
        }
    }

    void ApplyAccessResponse(StudentAccessResponse response)
    {
        if (response == null || response.items == null) return;

        var backend = TrainingBackendClient.EnsureExists();
        if (response.student_id > 0)
        {
            backend.studentId = response.student_id;
            PlayerPrefs.SetInt("TrainingStudentId", response.student_id);
        }

        for (int i = 0; i < response.items.Length; i++)
        {
            var item = response.items[i];
            if (item == null) continue;

            var option = FindOptionByScene(item.scene_name);
            if (option == null) continue;

            option.unlocked = item.unlocked;
            option.assigned = item.unlocked && item.assigned;
            option.assignmentId = item.assignment_id;
            option.assignmentStatus = item.status;
            if (!string.IsNullOrEmpty(item.title)) option.displayName = item.title;
        }
    }

    void ApplyLaunchContextFallback()
    {
        var backend = TrainingBackendClient.Active;
        if (backend == null || backend.attemptId <= 0 || string.IsNullOrEmpty(backend.launchSceneName)) return;

        string launchScene = SceneNameAliases.ToPublicSceneName(backend.launchSceneName);
        var option = FindOptionByScene(launchScene);
        if (option == null) return;

        option.unlocked = true;
        option.assigned = true;
        option.assignmentStatus = "running";
        if (backend.assignmentId > 0)
        {
            option.assignmentId = backend.assignmentId;
        }
    }

    RegularTrainingOption FindOptionByScene(string sceneName)
    {
        string normalized = SceneNameAliases.ToPublicSceneName(sceneName);
        for (int i = 0; i < _options.Length; i++)
        {
            if (_options[i].sceneName == normalized)
                return _options[i];
        }
        return null;
    }

    void LoadTextures()
    {
        for (int i = 0; i < _options.Length; i++)
        {
            for (int j = 0; j < _options[i].texturePaths.Length; j++)
                GetTexture(_options[i].texturePaths, j);
        }
    }

    Texture2D GetTexture(string[] paths, int index)
    {
        if (paths == null || index < 0 || index >= paths.Length) return null;
        string path = paths[index];
        if (_textureCache.TryGetValue(path, out var cached)) return cached;

        var texture = Resources.Load<Texture2D>(path);
        if (texture == null)
            Debug.LogWarning("[RegTrain] Missing texture: Resources/" + path);

        _textureCache[path] = texture;
        return texture;
    }

    void ShowStatus(string message, float seconds)
    {
        _statusMessage = message;
        _statusUntil = Time.time + seconds;
    }

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
        }

        cam.transform.position = new Vector3(0f, 2.8f, -7.2f);
        cam.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
        cam.fieldOfView = 52f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.035f, 0.045f, 0.055f);
    }

    static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.32f, 0.34f, 0.37f);

        var keyGo = new GameObject("RegTrain GUI Key Light");
        var key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 0.55f;
        key.color = new Color(1f, 0.96f, 0.88f);
        keyGo.transform.rotation = Quaternion.Euler(44f, -32f, 0f);
    }

    static void BuildQuietBackdrop()
    {
        Cube("RegTrain Floor", new Vector3(0f, -0.08f, 1.4f), new Vector3(16.5f, 0.16f, 9.2f), new Color(0.10f, 0.12f, 0.14f));
        Cube("RegTrain Back Wall", new Vector3(0f, 1.55f, 3.9f), new Vector3(16.5f, 3.1f, 0.22f), new Color(0.075f, 0.085f, 0.10f));
    }

    static void Cube(string name, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        var renderer = cube.GetComponent<Renderer>();
        if (renderer == null) return;
        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    void EnsureGuiStyles()
    {
        if (_titleStyle != null) return;

        _rowTexture = MakeTexture(new Color(0.075f, 0.088f, 0.105f, 0.94f));
        _rowSelectedTexture = MakeTexture(new Color(0.10f, 0.13f, 0.16f, 0.98f));
        _buttonTexture = MakeTexture(new Color(0.10f, 0.55f, 0.46f, 1f));
        _disabledTexture = MakeTexture(new Color(0.18f, 0.19f, 0.21f, 1f));

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 46,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        _subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            normal = { textColor = new Color(0.78f, 0.86f, 0.90f) }
        };
        _rowStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = _rowTexture },
            border = new RectOffset(8, 8, 8, 8)
        };
        _rowSelectedStyle = new GUIStyle(_rowStyle)
        {
            normal = { background = _rowSelectedTexture }
        };
        _nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        _bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            wordWrap = true,
            normal = { textColor = new Color(0.78f, 0.86f, 0.88f) }
        };
        _smallStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            normal = { textColor = new Color(0.72f, 0.80f, 0.84f) }
        };
        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { background = _buttonTexture, textColor = Color.white },
            hover = { background = _buttonTexture, textColor = Color.white },
            active = { background = _buttonTexture, textColor = Color.white }
        };
        _disabledButtonStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { background = _disabledTexture, textColor = new Color(0.70f, 0.74f, 0.78f) }
        };
        _stampAssignedStyle = CreateStampStyle(new Color(0.12f, 0.78f, 0.42f));
        _stampBlockedStyle = CreateStampStyle(new Color(0.90f, 0.24f, 0.18f));
    }

    static GUIStyle CreateStampStyle(Color color)
    {
        var style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            border = new RectOffset(4, 4, 4, 4),
            normal = { textColor = color }
        };
        return style;
    }

    static Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
