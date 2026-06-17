using System.Collections;
using UnityEngine;

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
        public Color color;

        public RegularTrainingOption(string taskId, string displayName, string sceneName, string instruction, Color color)
        {
            this.taskId = taskId;
            this.displayName = displayName;
            this.sceneName = sceneName;
            this.instruction = instruction;
            this.color = color;
        }
    }

    const float TileSize = 2.2f;
    const float InteractionDistance = 12.0f;

    readonly RegularTrainingOption[] _options =
    {
        new RegularTrainingOption(
            "lead_train1_regular",
            "常规训练一\nlead-train1",
            "lead-train1",
            "进入 lead-train1：完成引导式教学后，可选择正式手势识别训练。",
            new Color(0.18f, 0.54f, 0.88f)),
        new RegularTrainingOption(
            "train2_regular",
            "常规训练二\ntrain2",
            "train2",
            "进入 train2：进行第二套常规训练内容。",
            new Color(0.20f, 0.70f, 0.46f)),
    };

    Transform _player;
    RegularTrainingOption _nearestOption;
    Transform _nearestTransform;
    bool _enteringTraining;
    string _statusMessage = "靠近训练入口，按 E 开始常规训练。";
    float _statusUntil;

    IEnumerator Start()
    {
        SceneFlow.EnsureExists();
        var session = SessionManager.EnsureExists();
        session.returnSceneName = "entry";
        session.hasHubReturnPosition = false;

        var returnInput = gameObject.AddComponent<ReturnToHubInput>();
        returnInput.fallbackSceneName = "entry";
        returnInput.preferFallbackScene = true;

        SetupCamera();
        SetupLighting();
        BuildFloor();
        BuildTrainingOptions();

        yield return null;

        _player = ResolvePlayerTransform();
        ShowStatus("常规训练区已准备：靠近入口，按 E 进入训练。", 3.0f);
    }

    void Update()
    {
        if (_enteringTraining) return;

        _player = ResolvePlayerTransform();
        UpdateNearestOption();

        if (Input.GetKeyDown(KeyCode.E))
            TryEnterNearestTraining();
    }

    void OnGUI()
    {
        var message = BuildPromptText();
        if (string.IsNullOrEmpty(message)) return;

        var width = Mathf.Min(760f, Screen.width - 36f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 116f, width, 72f);
        GUI.Box(rect, message);
    }

    void BuildTrainingOptions()
    {
        var root = new GameObject("Regular Training Entries");
        for (int i = 0; i < _options.Length; i++)
        {
            float x = (i - 0.5f) * 4.0f;
            CreateTrainingTile(root.transform, _options[i], i, new Vector3(x, TileSize * 0.5f, 2.4f));
        }
    }

    void CreateTrainingTile(Transform parent, RegularTrainingOption option, int index, Vector3 position)
    {
        var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = "RegTrainTile_" + (index + 1) + "_" + option.taskId;
        tile.transform.SetParent(parent, true);
        tile.transform.position = position;
        tile.transform.localScale = new Vector3(2.8f, TileSize, 1.4f);
        SetColor(tile, option.color);

        var labelGo = new GameObject("Tile Label");
        labelGo.transform.SetParent(tile.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 0.62f, -0.52f);
        labelGo.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);

        var label = labelGo.AddComponent<TextMesh>();
        label.text = (index + 1) + "\n" + option.displayName;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 44;
        label.characterSize = 0.075f;
        label.color = Color.white;
    }

    void UpdateNearestOption()
    {
        _nearestOption = null;
        _nearestTransform = null;
        if (_player == null) return;

        float nearestDistance = float.MaxValue;
        for (int i = 0; i < _options.Length; i++)
        {
            var tile = GameObject.Find("RegTrainTile_" + (i + 1) + "_" + _options[i].taskId);
            if (tile == null) continue;

            float distance = HorizontalDistance(_player.position, tile.transform.position);
            if (distance > InteractionDistance || distance >= nearestDistance) continue;

            nearestDistance = distance;
            _nearestOption = _options[i];
            _nearestTransform = tile.transform;
        }
    }

    void TryEnterNearestTraining()
    {
        if (_nearestOption == null || _nearestTransform == null)
        {
            ShowStatus("请先靠近一个常规训练入口，再按 E。", 1.6f);
            return;
        }

        StartCoroutine(EnterTrainingRoutine(_nearestOption, _nearestTransform.position));
    }

    IEnumerator EnterTrainingRoutine(RegularTrainingOption option, Vector3 tilePosition)
    {
        _enteringTraining = true;

        var session = SessionManager.EnsureExists();
        session.selectedInstruction = option.instruction;
        session.selectedSuccessMessage = "训练完成";

        var task = new AssignedTask(option.taskId, option.displayName.Replace("\n", " "), option.sceneName, 0);
        ShowStatus(option.instruction + "\n正在进入：" + option.sceneName, 0.8f);
        yield return new WaitForSeconds(0.35f);

        if (option.sceneName == "lead-train1")
        {
            FactoryOneSceneController.ClearOneShotStartCameraReturnOverride();
        }

        SceneFlow.EnsureExists().EnterTraining(task, ResolveReturnPosition(tilePosition));
    }

    Vector3 ResolveReturnPosition(Vector3 tilePosition)
    {
        if (_player != null) return _player.position;
        return tilePosition + Vector3.back * 2f + Vector3.up;
    }

    Transform ResolvePlayerTransform()
    {
        var controller = FindObjectOfType<CharacterController>();
        if (controller != null) return controller.transform;

        if (Camera.main != null && Camera.main.transform.parent != null)
            return Camera.main.transform.parent;

        return Camera.main != null ? Camera.main.transform : transform;
    }

    string BuildPromptText()
    {
        if (Time.time < _statusUntil && !string.IsNullOrEmpty(_statusMessage))
            return _statusMessage;

        if (_nearestOption == null)
            return "常规训练区：靠近训练入口，按 E 开始。按 R / Esc 返回游戏大厅。";

        return "按 E 开始：" + _nearestOption.displayName.Replace("\n", " ") + "\n" + _nearestOption.instruction;
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

        cam.transform.position = new Vector3(0f, 2.4f, -5.8f);
        cam.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
        cam.fieldOfView = 52f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.11f, 0.14f);
    }

    static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.44f, 0.46f, 0.48f);

        var keyGo = new GameObject("RegTrain Key Light");
        var key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 0.9f;
        key.color = new Color(1f, 0.96f, 0.88f);
        keyGo.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
    }

    static void BuildFloor()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "RegTrain Floor";
        floor.transform.position = new Vector3(0f, -0.08f, 2.5f);
        floor.transform.localScale = new Vector3(9.5f, 0.16f, 6.5f);
        SetColor(floor, new Color(0.16f, 0.18f, 0.20f));

        var titleGo = new GameObject("RegTrain Title");
        titleGo.transform.position = new Vector3(0f, 3.0f, 1.4f);
        titleGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "常规训练区";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 72;
        title.characterSize = 0.075f;
        title.color = new Color(0.92f, 0.97f, 1f);
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }
}
