using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EntryRegTrainPortal : MonoBehaviour
{
    [System.Serializable]
    public class RegularTrainingBoxBinding
    {
        public string targetNameContains;
    }

    public string regTrainSceneName = "regTrain";
    public float interactionDistance = 32f;
    public KeyCode interactKey = KeyCode.E;
    public Vector3 portalOffsetFromPlayer = new Vector3(7.5f, 0f, 10f);

    [Header("Regular Training Boxes")]
    [SerializeField] RegularTrainingBoxBinding[] boxBindings;
    [SerializeField] Color highlightColor = new Color(0.18f, 0.54f, 0.88f);

    readonly List<Transform> _targets = new List<Transform>();
    Transform _nearestTarget;
    Transform _player;
    float _nextResolveTime;
    string _statusMessage = "";
    float _statusUntil;
    bool _loading;
    bool _resolvedBoxes;
    bool _createdAreaLabel;

    void Start()
    {
        SceneFlow.EnsureExists();
    }

    void Update()
    {
        if (_loading) return;

        _player = ResolvePlayerTransform();
        ResolveTrainingBoxes();
        RefreshNearestTarget();

        if (Input.GetKeyDown(interactKey))
            TryEnterRegTrain();
    }

    void OnGUI()
    {
        var text = BuildPromptText();
        if (string.IsNullOrEmpty(text)) return;

        var width = Mathf.Min(420f, Screen.width - 40f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 174f, width, 48f);
        GUI.Box(rect, text);
    }

    void ResolveTrainingBoxes()
    {
        if (_resolvedBoxes) return;
        if (Time.time < _nextResolveTime) return;

        _nextResolveTime = Time.time + 0.35f;

        var bindings = GetBoxBindings();
        var renderers = FindObjectsOfType<Renderer>();
        foreach (var binding in bindings)
        {
            if (binding == null || string.IsNullOrWhiteSpace(binding.targetNameContains))
                continue;

            var renderer = FindRenderer(renderers, binding.targetNameContains);
            if (renderer == null)
                continue;

            if (!_targets.Contains(renderer.transform))
            {
                _targets.Add(renderer.transform);
                CreateMarker(renderer.transform, binding.targetNameContains, !_createdAreaLabel);
                _createdAreaLabel = true;
            }
        }

        _resolvedBoxes = _targets.Count >= bindings.Length;
    }

    RegularTrainingBoxBinding[] GetBoxBindings()
    {
        if (boxBindings != null && boxBindings.Length > 0)
            return boxBindings;

        return new[]
        {
            new RegularTrainingBoxBinding { targetNameContains = "9g2pd_133" },
            new RegularTrainingBoxBinding { targetNameContains = "9g2pd_132" },
            new RegularTrainingBoxBinding { targetNameContains = "9g2pd_128" },
            new RegularTrainingBoxBinding { targetNameContains = "9g2pd_129" }
        };
    }

    static Renderer FindRenderer(Renderer[] renderers, string nameContains)
    {
        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;
            if (!renderer.name.StartsWith("Image Overlay -")) continue;
            if (renderer.name.Contains(nameContains))
                return renderer;
        }

        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;
            if (renderer.name.StartsWith("Regular Training Entry")) continue;
            if (renderer.name.Contains(nameContains))
                return renderer;
        }

        return null;
    }

    void RefreshNearestTarget()
    {
        _nearestTarget = null;
        if (_player == null || _targets.Count == 0) return;

        var bestDistance = float.MaxValue;
        foreach (var target in _targets)
        {
            if (target == null) continue;

            var distance = HorizontalDistance(_player.position, target.position);
            if (distance > interactionDistance || distance >= bestDistance)
                continue;

            bestDistance = distance;
            _nearestTarget = target;
        }
    }

    void TryEnterRegTrain()
    {
        if (_targets.Count == 0)
        {
            ShowStatus("正在初始化常规训练区。", 1.4f);
            return;
        }

        if (_nearestTarget == null)
        {
            ShowStatus("常规训练区", 1.4f);
            return;
        }

        var session = SessionManager.EnsureExists();
        session.selectedInstruction = "";
        session.selectedSuccessMessage = "训练完成";
        session.hasHubReturnPosition = false;
        session.returnSceneName = "entry";

        _loading = true;
        ShowStatus("常规训练区", 0.8f);
        Debug.Log("[EntryRegTrainPortal] Enter " + regTrainSceneName + " from " + _nearestTarget.name);
        SceneFlow.EnsureExists().LoadScene(regTrainSceneName);
    }

    string BuildPromptText()
    {
        if (Time.time < _statusUntil && !string.IsNullOrEmpty(_statusMessage))
            return _statusMessage;

        return _nearestTarget == null ? "" : "常规训练区";
    }

    void CreateMarker(Transform target, string label, bool createLabel)
    {
        var markerRoot = new GameObject("Regular Training Entry Marker - " + label);
        markerRoot.transform.SetParent(target, false);
        markerRoot.transform.localPosition = new Vector3(0f, 1.75f, -0.05f);

        if (!createLabel) return;

        CreateStyledLabel(markerRoot.transform, "常规训练区", Color.white);
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

    static void CreateStyledLabel(Transform parent, string text, Color color)
    {
        CreateLabelLayer(parent, text, new Vector3(0.035f, -0.035f, 0.012f), Color.black, 0.165f);
        CreateLabelLayer(parent, text, Vector3.zero, color, 0.155f);
    }

    static void CreateLabelLayer(Transform parent, string text, Vector3 localPosition, Color color, float characterSize)
    {
        var labelGo = new GameObject("Regular Training Entry Label");
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
