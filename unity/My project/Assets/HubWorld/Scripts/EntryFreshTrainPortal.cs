using UnityEngine;

[DisallowMultipleComponent]
public class EntryFreshTrainPortal : MonoBehaviour
{
    public string targetObjectName = "Image Overlay - 9g2pd_142";
    public string targetNameContains = "9g2pd_142";
    public string freshTrainSceneName = "freshTrain";
    public float interactionDistance = 28f;
    public KeyCode interactKey = KeyCode.E;

    Transform _target;
    Transform _player;
    GameObject _marker;
    float _nextFindTime;
    string _statusMessage = "";
    float _statusUntil;
    bool _loading;

    void Update()
    {
        if (_loading) return;

        ResolveTarget();
        _player = ResolvePlayerTransform();

        if (Input.GetKeyDown(interactKey))
            TryEnterFreshTrain();
    }

    void OnGUI()
    {
        var text = BuildPromptText();
        if (string.IsNullOrEmpty(text)) return;

        var width = Mathf.Min(420f, Screen.width - 40f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 104f, width, 48f);
        GUI.Box(rect, text);
    }

    void ResolveTarget()
    {
        if (_target != null) return;
        if (Time.time < _nextFindTime) return;

        _nextFindTime = Time.time + 0.35f;
        var targetGo = GameObject.Find(targetObjectName);
        if (targetGo != null)
        {
            _target = targetGo.transform;
            CreateMarker();
            return;
        }

        if (string.IsNullOrEmpty(targetNameContains)) return;

        foreach (var renderer in FindObjectsOfType<Renderer>())
        {
            if (renderer == null || !renderer.name.Contains(targetNameContains)) continue;

            _target = renderer.transform;
            CreateMarker();
            return;
        }
    }

    void TryEnterFreshTrain()
    {
        if (_target == null)
        {
            ShowStatus("正在初始化新手教学区。", 1.4f);
            return;
        }

        if (!IsPlayerNearTarget())
        {
            ShowStatus("新手教学区", 1.4f);
            return;
        }

        var session = SessionManager.EnsureExists();
        session.selectedInstruction = "";
        session.selectedSuccessMessage = "成功完成训练";
        session.hasHubReturnPosition = false;
        session.returnSceneName = "";

        _loading = true;
        ShowStatus("新手教学区", 0.8f);
        Debug.Log($"[EntryFreshTrainPortal] Enter {freshTrainSceneName} from {targetObjectName}");
        SceneFlow.EnsureExists().LoadScene(freshTrainSceneName);
    }

    bool IsPlayerNearTarget()
    {
        if (_player == null || _target == null) return false;
        var playerPosition = _player.position;
        var targetPosition = _target.position;
        playerPosition.y = 0f;
        targetPosition.y = 0f;
        return Vector3.Distance(playerPosition, targetPosition) <= interactionDistance;
    }

    string BuildPromptText()
    {
        if (Time.time < _statusUntil && !string.IsNullOrEmpty(_statusMessage))
            return _statusMessage;

        if (_target == null) return "";
        if (!IsPlayerNearTarget()) return "";

        return "新手教学区";
    }

    void CreateMarker()
    {
        if (_target == null || _marker != null) return;

        _marker = new GameObject("FreshTrain Entry Marker");
        _marker.transform.SetParent(_target, false);
        _marker.transform.localPosition = new Vector3(0f, 1.75f, -0.05f);

        CreateStyledLabel(_marker.transform, "新手教学区", new Color(0.10f, 0.95f, 1.0f));
    }

    void ShowStatus(string message, float seconds)
    {
        _statusMessage = message;
        _statusUntil = Time.time + seconds;
    }

    Transform ResolvePlayerTransform()
    {
        var controller = FindObjectOfType<CharacterController>();
        if (controller != null) return controller.transform;

        if (Camera.main != null && Camera.main.transform.parent != null)
            return Camera.main.transform.parent;

        return Camera.main != null ? Camera.main.transform : transform;
    }

    static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static void CreateStyledLabel(Transform parent, string text, Color color)
    {
        CreateLabelLayer(parent, text, new Vector3(0.035f, -0.035f, 0.012f), Color.black, 0.165f);
        CreateLabelLayer(parent, text, Vector3.zero, color, 0.155f);
    }

    static void CreateLabelLayer(Transform parent, string text, Vector3 localPosition, Color color, float characterSize)
    {
        var labelGo = new GameObject("FreshTrain Entry Label");
        labelGo.transform.SetParent(parent, false);
        labelGo.transform.localPosition = localPosition;
        var label = labelGo.AddComponent<TextMesh>();
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 42;
        label.characterSize = characterSize;
        label.fontStyle = FontStyle.Bold;
        label.color = color;
    }
}
