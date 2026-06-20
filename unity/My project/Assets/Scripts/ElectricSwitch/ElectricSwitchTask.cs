using UnityEngine;

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
    string _instruction = "握拳后拉动电闸，推动或拉下竖杆完成训练。";
    string _successMessage = "成功完成训练";

    readonly Color _panelColor = new Color(0.18f, 0.20f, 0.23f);
    readonly Color _edgeColor = new Color(0.42f, 0.46f, 0.50f);
    readonly Color _metalColor = new Color(0.62f, 0.65f, 0.68f);
    readonly Color _offRed = new Color(0.95f, 0.16f, 0.10f);
    readonly Color _onGreen = new Color(0.20f, 0.85f, 0.32f);
    readonly Color _lampDim = new Color(0.08f, 0.09f, 0.10f);

    void Start()
    {
        LoadSessionPrompt();
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
        SetColor(rod, _offRed);

        var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "HorizontalGripBar";
        handle.transform.parent = sliderRoot.transform;
        handle.transform.localPosition = new Vector3(0f, 0.12f, -0.02f);
        handle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        handle.transform.localScale = new Vector3(0.12f, 1.04f, 0.12f);
        SetColor(handle, _offRed);

        var leftCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftCap.name = "LeftGripCap";
        leftCap.transform.parent = sliderRoot.transform;
        leftCap.transform.localPosition = new Vector3(-1.04f, 0.12f, -0.02f);
        leftCap.transform.localRotation = Quaternion.identity;
        leftCap.transform.localScale = Vector3.one * 0.25f;
        SetColor(leftCap, _offRed);

        var rightCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightCap.name = "RightGripCap";
        rightCap.transform.parent = sliderRoot.transform;
        rightCap.transform.localPosition = new Vector3(1.04f, 0.12f, -0.02f);
        rightCap.transform.localRotation = Quaternion.identity;
        rightCap.transform.localScale = Vector3.one * 0.25f;
        SetColor(rightCap, _offRed);

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
        switchInteractable.SetUp(false);
    }

    void CreateStop(Transform parent, string name, Vector3 position, bool upper)
    {
        CreateBox(parent, name, position, new Vector3(0.82f, 0.14f, 0.16f), upper ? _onGreen : _offRed);

        var socket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        socket.name = name + "Socket";
        socket.transform.parent = parent;
        socket.transform.position = position + new Vector3(0f, upper ? -0.20f : 0.20f, -0.02f);
        socket.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        socket.transform.localScale = new Vector3(0.22f, 0.035f, 0.22f);
        SetColor(socket, _metalColor);

        var labelGo = new GameObject(name + "Label");
        labelGo.transform.parent = parent;
        labelGo.transform.position = position + new Vector3(0.78f, upper ? 0.06f : -0.18f, -0.12f);
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
        var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = name;
        lamp.transform.parent = parent;
        lamp.transform.position = position;
        lamp.transform.localRotation = Quaternion.identity;
        lamp.transform.localScale = Vector3.one * 0.28f;
        Destroy(lamp.GetComponent<Collider>());
        SetColor(lamp, _lampDim);
        return lamp.GetComponent<Renderer>();
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
        title.text = "电闸拉动训练";
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
        if (_line.enabled)
        {
            _line.SetPosition(0, grip);
            _line.SetPosition(1, switchInteractable.HandlePosition + new Vector3(0f, 0f, -0.22f));
        }
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
        if (!hand.IsActive) phase = "等待手势识别服务连接";
        else if (switchInteractable.IsGrabbed) phase = "已握住横杆：向上推或向下拉";
        else if (switchInteractable.IsHovering) phase = "握拳抓住横杆";
        else phase = "移动手部光标到横杆位置";

        string done = Time.time - _completedAt < 1.8f ? "\n" + _successMessage : "";
        _status.text =
            "目标：" + _instruction +
            "\n状态：" + (switchInteractable.IsUp ? "ON / 已推上" : "OFF / 已拉下") +
            "\n动作：" + phase +
            "\n握拳强度：" + switchInteractable.GripSignal.ToString("0.00") + " / " + switchInteractable.grabThreshold.ToString("0.00") +
            "\n行程：" + Mathf.RoundToInt(switchInteractable.CurrentTravel * 100f) + "%" +
            done +
            "\n按 Esc 返回新手训练大厅";
    }

    void LoadSessionPrompt()
    {
        var session = SessionManager.Instance;
        if (session == null) return;

        if (!string.IsNullOrEmpty(session.selectedInstruction))
            _instruction = session.selectedInstruction;
        if (!string.IsNullOrEmpty(session.selectedSuccessMessage))
            _successMessage = session.selectedSuccessMessage;
    }

    GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.parent = parent;
        box.transform.position = position;
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
