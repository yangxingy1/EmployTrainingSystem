using UnityEngine;

/// <summary>
/// 推拉开关训练: 捏合横杆，上下推拉到 ON/OFF 档位，松手后自动吸附到最近档位。
/// </summary>
public class ElectricSwitchTask : MonoBehaviour
{
    public HandInput hand;
    public ElectricSwitchInteractable switchInteractable;

    TextMesh _status;
    GameObject _cursor;
    Renderer _cursorRenderer;
    LineRenderer _line;
    Renderer _powerLamp;
    Renderer _offLamp;
    Renderer _panelRenderer;
    float _completedAt = -999f;
    bool _lastUp;

    readonly Color _panelColor = new Color(0.18f, 0.20f, 0.23f);
    readonly Color _edgeColor = new Color(0.42f, 0.46f, 0.50f);
    readonly Color _metalColor = new Color(0.62f, 0.65f, 0.68f);
    readonly Color _offRed = new Color(0.95f, 0.16f, 0.10f);
    readonly Color _onGreen = new Color(0.20f, 0.85f, 0.32f);
    readonly Color _lampDim = new Color(0.08f, 0.09f, 0.10f);

    void Start()
    {
        BuildSwitch();
        BuildCursor();
        BuildText();
        _lastUp = switchInteractable != null && switchInteractable.IsUp;
        UpdateLamps();
        UpdateStatus();
    }

    void Update()
    {
        UpdateCursor();
        UpdateLamps();
        UpdateStatus();

        if (switchInteractable != null && switchInteractable.IsUp != _lastUp)
        {
            _lastUp = switchInteractable.IsUp;
            _completedAt = Time.time;
        }
    }

    void BuildSwitch()
    {
        var root = new GameObject("ElectricSwitch");
        root.transform.parent = transform;
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        var panel = CreateBox(root.transform, "BreakerPanel", new Vector3(0f, 0.05f, 0.18f), new Vector3(2.9f, 3.25f, 0.20f), _panelColor);
        _panelRenderer = panel.GetComponent<Renderer>();

        CreateBox(root.transform, "PanelTopEdge", new Vector3(0f, 1.72f, 0.04f), new Vector3(3.05f, 0.08f, 0.16f), _edgeColor);
        CreateBox(root.transform, "PanelBottomEdge", new Vector3(0f, -1.62f, 0.04f), new Vector3(3.05f, 0.08f, 0.16f), _edgeColor);
        CreateBox(root.transform, "PanelLeftEdge", new Vector3(-1.50f, 0.05f, 0.04f), new Vector3(0.08f, 3.25f, 0.16f), _edgeColor);
        CreateBox(root.transform, "PanelRightEdge", new Vector3(1.50f, 0.05f, 0.04f), new Vector3(0.08f, 3.25f, 0.16f), _edgeColor);

        CreateBox(root.transform, "VerticalSlot", new Vector3(0f, 0f, -0.04f), new Vector3(0.34f, 2.35f, 0.06f), new Color(0.045f, 0.050f, 0.055f));
        CreateBox(root.transform, "LeftGuideRail", new Vector3(-0.30f, 0f, -0.13f), new Vector3(0.07f, 2.45f, 0.12f), _metalColor);
        CreateBox(root.transform, "RightGuideRail", new Vector3(0.30f, 0f, -0.13f), new Vector3(0.07f, 2.45f, 0.12f), _metalColor);
        CreateStop(root.transform, "UpperStop", new Vector3(0f, 1.02f, -0.18f), true);
        CreateStop(root.transform, "LowerStop", new Vector3(0f, -1.02f, -0.18f), false);

        var sliderRoot = new GameObject("VerticalPullRodAssembly");
        sliderRoot.transform.parent = root.transform;
        sliderRoot.transform.localPosition = new Vector3(0f, -0.82f, -0.22f);
        sliderRoot.transform.localRotation = Quaternion.identity;

        var rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rod.name = "VerticalPullRod";
        rod.transform.parent = sliderRoot.transform;
        rod.transform.localPosition = Vector3.zero;
        rod.transform.localRotation = Quaternion.identity;
        rod.transform.localScale = new Vector3(0.085f, 0.58f, 0.085f);
        Destroy(rod.GetComponent<Collider>());
        SetColor(rod, _offRed);

        var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "HorizontalGripBar";
        handle.transform.parent = sliderRoot.transform;
        handle.transform.localPosition = new Vector3(0f, 0.12f, -0.02f);
        handle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        handle.transform.localScale = new Vector3(0.12f, 1.04f, 0.12f);
        Destroy(handle.GetComponent<Collider>());
        SetColor(handle, _offRed);

        var leftCap = CreateSphere(sliderRoot.transform, "LeftGripCap", new Vector3(-1.04f, 0.12f, -0.02f), 0.25f, _offRed);
        var rightCap = CreateSphere(sliderRoot.transform, "RightGripCap", new Vector3(1.04f, 0.12f, -0.02f), 0.25f, _offRed);

        _powerLamp = CreateLamp(root.transform, "PowerLamp", new Vector3(-0.95f, 1.05f, -0.16f));
        _offLamp = CreateLamp(root.transform, "OffLamp", new Vector3(-0.95f, -1.05f, -0.16f));

        switchInteractable = root.AddComponent<ElectricSwitchInteractable>();
        switchInteractable.hand = hand;
        switchInteractable.sliderRoot = sliderRoot.transform;
        switchInteractable.handle = handle.transform;
        switchInteractable.highlightRenderers = new[]
        {
            rod.GetComponent<Renderer>(),
            handle.GetComponent<Renderer>(),
            leftCap.GetComponent<Renderer>(),
            rightCap.GetComponent<Renderer>()
        };
        switchInteractable.downY = -0.82f;
        switchInteractable.upY = 0.82f;
        switchInteractable.grabRadius = 0.66f;
        switchInteractable.grabThreshold = 0.45f;
        switchInteractable.releaseThreshold = 0.24f;
        switchInteractable.SetUp(false);
    }

    void CreateStop(Transform parent, string name, Vector3 position, bool upper)
    {
        CreateBox(parent, name, position, new Vector3(0.82f, 0.14f, 0.16f), upper ? _onGreen : _offRed);

        var socket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        socket.name = name + "Socket";
        socket.transform.parent = parent;
        socket.transform.localPosition = position + new Vector3(0f, upper ? -0.20f : 0.20f, -0.02f);
        socket.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        socket.transform.localScale = new Vector3(0.22f, 0.035f, 0.22f);
        Destroy(socket.GetComponent<Collider>());
        SetColor(socket, _metalColor);

        var labelGo = new GameObject(name + "Label");
        labelGo.transform.parent = parent;
        labelGo.transform.localPosition = position + new Vector3(0.78f, upper ? 0.06f : -0.18f, -0.12f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = upper ? "ON" : "OFF";
        label.anchor = TextAnchor.MiddleLeft;
        label.alignment = TextAlignment.Left;
        label.fontSize = 38;
        label.characterSize = 0.046f;
        label.color = upper ? _onGreen : _offRed;
    }

    Renderer CreateLamp(Transform parent, string name, Vector3 position)
    {
        var lamp = CreateSphere(parent, name, position, 0.28f, _lampDim);
        return lamp.GetComponent<Renderer>();
    }

    GameObject CreateSphere(Transform parent, string name, Vector3 position, float size, Color color)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.parent = parent;
        sphere.transform.localPosition = position;
        sphere.transform.localRotation = Quaternion.identity;
        sphere.transform.localScale = Vector3.one * size;
        Destroy(sphere.GetComponent<Collider>());
        SetColor(sphere, color);
        return sphere;
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "SwitchGripCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("SwitchGripLine");
        _line = lineGo.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.025f;
        _line.endWidth = 0.012f;
        _line.material = MakeMaterial(new Color(0.82f, 0.94f, 1f));
        _line.enabled = false;
    }

    void BuildText()
    {
        var titleGo = new GameObject("SwitchTaskTitle");
        titleGo.transform.position = new Vector3(0f, 2.05f, -0.18f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "推拉开关训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 58;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var statusGo = new GameObject("SwitchTaskStatus");
        statusGo.transform.position = new Vector3(-2.90f, 1.50f, -0.18f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperLeft;
        _status.alignment = TextAlignment.Left;
        _status.fontSize = 40;
        _status.characterSize = 0.041f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void UpdateCursor()
    {
        if (_cursor == null || hand == null || switchInteractable == null) return;

        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            _line.enabled = false;
            return;
        }

        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.22f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.22f, switchInteractable.GripSignal);

        Color color = switchInteractable.IsGrabbed
            ? new Color(0.20f, 0.90f, 0.38f)
            : switchInteractable.IsHovering ? new Color(1f, 0.78f, 0.18f) : new Color(0.18f, 0.66f, 1f);
        SetRendererColor(_cursorRenderer, color);

        _line.enabled = switchInteractable.IsHovering || switchInteractable.IsGrabbed;
        if (!_line.enabled) return;

        _line.SetPosition(0, grip);
        _line.SetPosition(1, switchInteractable.HandlePosition + new Vector3(0f, 0f, -0.22f));
    }

    void UpdateLamps()
    {
        if (switchInteractable == null) return;

        bool up = switchInteractable.IsUp;
        SetRendererColor(_powerLamp, up ? _onGreen : _lampDim);
        SetRendererColor(_offLamp, up ? _lampDim : _offRed);
        SetRendererColor(_panelRenderer, up ? new Color(0.16f, 0.23f, 0.18f) : _panelColor);
    }

    void UpdateStatus()
    {
        if (_status == null || hand == null || switchInteractable == null) return;

        string phase;
        if (!hand.IsActive) phase = "等待手势识别";
        else if (switchInteractable.IsGrabbed) phase = "已抓住横杆, 上下推拉";
        else if (switchInteractable.IsHovering) phase = "捏合横杆开始操作";
        else phase = "移动光标到横向把手";

        string done = Time.time - _completedAt < 1.2f ? "\n开关已切换" : "";
        _status.text =
            "状态: " + (switchInteractable.IsUp ? "ON / 已上推" : "OFF / 已下拉") +
            "\n操作: " + phase +
            "\n捏合: " + switchInteractable.GripSignal.ToString("0.00") + " / " + switchInteractable.grabThreshold.ToString("0.00") +
            "\n行程: " + Mathf.RoundToInt(switchInteractable.CurrentTravel * 100f) + "%" +
            done;
    }

    GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.parent = parent;
        box.transform.localPosition = position;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = scale;
        SetColor(box, color);
        return box;
    }

    static void SetColor(GameObject go, Color color)
    {
        SetRendererColor(go.GetComponent<Renderer>(), color);
    }

    static void SetRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        return material;
    }
}
