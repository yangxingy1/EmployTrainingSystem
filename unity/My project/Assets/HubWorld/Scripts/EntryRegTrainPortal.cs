using UnityEngine;

[DisallowMultipleComponent]
public class EntryRegTrainPortal : MonoBehaviour
{
    public string regTrainSceneName = "regTrain";
    public float interactionDistance = 32f;
    public KeyCode interactKey = KeyCode.E;
    public Vector3 portalOffsetFromPlayer = new Vector3(7.5f, 0f, 10f);

    Transform _target;
    Transform _player;
    string _statusMessage = "";
    float _statusUntil;
    bool _loading;

    void Start()
    {
        SceneFlow.EnsureExists();
    }

    void Update()
    {
        if (_loading) return;

        _player = ResolvePlayerTransform();
        EnsurePortal();

        if (Input.GetKeyDown(interactKey))
            TryEnterRegTrain();
    }

    void OnGUI()
    {
        var text = BuildPromptText();
        if (string.IsNullOrEmpty(text)) return;

        var width = Mathf.Min(680f, Screen.width - 40f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 174f, width, 58f);
        GUI.Box(rect, text);
    }

    void EnsurePortal()
    {
        if (_target != null) return;

        var existing = GameObject.Find("Regular Training Entry");
        if (existing != null)
        {
            _target = existing.transform;
            return;
        }

        var root = new GameObject("Regular Training Entry");
        var basePosition = _player != null ? _player.position : Vector3.zero;
        root.transform.position = basePosition + portalOffsetFromPlayer;
        _target = root.transform;

        var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillar.name = "Regular Training Entry Highlight";
        pillar.transform.SetParent(root.transform, false);
        pillar.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        pillar.transform.localScale = new Vector3(4.4f, 2.5f, 0.32f);
        SetColor(pillar, new Color(0.18f, 0.54f, 0.88f));

        var labelGo = new GameObject("Regular Training Entry Label");
        labelGo.transform.SetParent(root.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 2.85f, -0.22f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = "按 E 进入常规训练区";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 48;
        label.characterSize = 0.16f;
        label.color = Color.white;
    }

    void TryEnterRegTrain()
    {
        if (_target == null)
        {
            ShowStatus("正在初始化常规训练入口，请稍后再试。", 1.4f);
            return;
        }

        if (!IsPlayerNearTarget())
        {
            ShowStatus("请先靠近“常规训练区”入口。", 1.4f);
            return;
        }

        var session = SessionManager.EnsureExists();
        session.selectedInstruction = "";
        session.selectedSuccessMessage = "训练完成";
        session.hasHubReturnPosition = false;
        session.returnSceneName = "entry";

        _loading = true;
        ShowStatus("正在进入常规训练区...", 0.8f);
        Debug.Log("[EntryRegTrainPortal] Enter " + regTrainSceneName);
        SceneFlow.EnsureExists().LoadScene(regTrainSceneName);
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

        return "按 E 进入：常规训练区";
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
}
