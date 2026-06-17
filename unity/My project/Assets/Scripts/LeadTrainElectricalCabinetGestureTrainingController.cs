using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LeadTrainElectricalCabinetGestureTrainingController : MonoBehaviour
{
    public const string TaskId = "lead_train1_electrical_cabinet_gesture";

    public HandInput hand;
    public HandVisual handVisual;

    enum TrainingPhase
    {
        WaitingStart,
        RotateOn,
        RotateOff,
        Completed
    }

    class ModeSlot
    {
        public string label;
        public float angle;
        public Renderer marker;
        public TextMesh text;
    }

    const int OffSlot = 0;
    const int OnSlot = 1;

    readonly ModeSlot[] _slots =
    {
        new ModeSlot { label = "OFF", angle = 145f },
        new ModeSlot { label = "ON", angle = 35f },
    };

    TrainingPhase _phase = TrainingPhase.WaitingStart;
    Transform _stageRoot;
    Transform _knobRoot;
    TextMesh _status;
    TextMesh _targetText;
    FingertipTapButton _startButton;
    FingertipTapButton _resetButton;
    GameObject _cursor;
    Renderer _cursorRenderer;
    LineRenderer _guideLine;

    Renderer _knobRenderer;
    Renderer _pointerRenderer;
    Renderer _powerLampRenderer;
    Renderer _powerHaloRenderer;
    Renderer _supplyLineRenderer;
    Renderer _loadLineRenderer;
    Light _powerLight;

    float _angle = 145f;
    float _lastHandAngle;
    float _holdTimer;
    float _lightChangedAt = -99f;
    bool _near;
    bool _rotating;
    bool _breakerOn;
    bool _initialized;
    string _feedback = "点击启动后开始配电柜主断路器手势训练";

    readonly Color _cabinetColor = new Color(0.64f, 0.67f, 0.65f);
    readonly Color _doorColor = new Color(0.48f, 0.52f, 0.51f);
    readonly Color _darkPanel = new Color(0.055f, 0.065f, 0.075f);
    readonly Color _metal = new Color(0.52f, 0.56f, 0.58f);
    readonly Color _blue = new Color(0.18f, 0.58f, 1f);
    readonly Color _green = new Color(0.14f, 0.92f, 0.34f);
    readonly Color _yellow = new Color(1f, 0.76f, 0.16f);
    readonly Color _red = new Color(0.86f, 0.09f, 0.06f);
    readonly Color _lampOff = new Color(0.035f, 0.090f, 0.045f);

    const float KnobRadius = 0.64f;
    const float GrabRadius = 0.86f;
    const float GripThreshold = 0.44f;
    const float ReleaseThreshold = 0.24f;
    const float AngleDeadZone = 0.20f;
    const float MaxStepDegrees = 8.0f;
    const float SnapSpeed = 7.5f;
    const float TargetTolerance = 13f;
    const float LightSwitchTolerance = 18f;
    const float HoldToConfirm = 0.32f;

    void Start()
    {
        BeginTrainingScene();
    }

    public void BeginTrainingScene()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        SetupTrainingCamera();
        SetupTrainingLights();
        CreateVirtualHand();
        BuildStage();
        BuildCursorAndGuide();
        BuildButtons();
        BuildTexts();
        TrainingFlowController.EnsureExists(TaskId);
        RestartTraining(false);
        EnsureReturnInput();
    }

    public void RestartTraining(bool resetTrainingFlow = true)
    {
        _phase = TrainingPhase.WaitingStart;
        _angle = _slots[OffSlot].angle;
        _holdTimer = 0f;
        _near = false;
        _rotating = false;
        _breakerOn = false;
        _lightChangedAt = -99f;
        _feedback = "点击启动后开始配电柜主断路器手势训练";

        if (resetTrainingFlow)
        {
            TrainingFlowController.EnsureExists(TaskId);
        }

        ApplyKnobRotation();
        RefreshButtons();
        RefreshVisuals();
        UpdateStatusText();
    }

    void Update()
    {
        if (!_initialized)
        {
            return;
        }

        if (_phase == TrainingPhase.RotateOn || _phase == TrainingPhase.RotateOff)
        {
            UpdateKnobInteraction();
            UpdateSnapAndCompletion();
        }

        UpdateCursorAndGuide();
        RefreshButtons();
        RefreshVisuals();
        UpdateStatusText();
        TrainingFlowController.Active?.ReportProgress(Progress01(), PhaseName(), CurrentInstruction());
    }

    void SetupTrainingCamera()
    {
        FactoryOneSceneController factoryController = FindObjectOfType<FactoryOneSceneController>();
        if (factoryController != null)
        {
            factoryController.enabled = false;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("LeadTrainElectricalCabinetGestureCamera");
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
        }

        cam.transform.SetParent(null, true);
        cam.transform.position = new Vector3(0f, 0.28f, -6.5f);
        cam.transform.rotation = Quaternion.identity;
        cam.fieldOfView = 45f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.055f, 0.070f, 0.080f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SetupTrainingLights()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.34f, 0.36f, 0.38f);

        GameObject keyGo = new GameObject("LeadElectricalCabinet_KeyLight");
        Light key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 0.95f;
        key.color = new Color(1f, 0.96f, 0.88f);
        keyGo.transform.rotation = Quaternion.Euler(38f, -24f, 0f);

        GameObject fillGo = new GameObject("LeadElectricalCabinet_FillLight");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.intensity = 1.35f;
        fill.range = 7.5f;
        fill.color = new Color(0.44f, 0.70f, 1f);
        fillGo.transform.position = new Vector3(-2.8f, 2.4f, -2.1f);
    }

    void CreateVirtualHand()
    {
        if (hand != null)
        {
            handVisual = hand.GetComponent<HandVisual>();
            return;
        }

        GameObject handGo = new GameObject("LeadTrainElectricalCabinetGestureHand");
        hand = handGo.AddComponent<HandInput>();
        hand.url = "ws://127.0.0.1:8765";
        hand.planeWidth = 6.1f;
        hand.planeHeight = 3.95f;
        hand.planeOrigin = new Vector3(0f, 0f, -0.52f);
        hand.gain = 1.25f;
        hand.smoothing = 0.66f;
        hand.graceTime = 0.45f;

        handVisual = handGo.AddComponent<HandVisual>();
        handVisual.jointRadius = 0.043f;
        handVisual.skinColor = new Color(0.96f, 0.78f, 0.62f);
        handVisual.gripColor = new Color(0.20f, 0.92f, 0.48f);
        handVisual.enablePhysicalColliders = false;
    }

    void BuildStage()
    {
        GameObject root = new GameObject("LeadElectricalCabinetGestureStage");
        root.transform.parent = transform;
        root.transform.position = Vector3.zero;
        _stageRoot = root.transform;

        CreateBox(_stageRoot, "TrainingBackdrop", new Vector3(0f, 0.02f, 0.44f), new Vector3(6.5f, 4.15f, 0.18f), new Color(0.09f, 0.12f, 0.15f));
        CreateBox(_stageRoot, "WorkbenchDeck", new Vector3(0f, -1.78f, 0.08f), new Vector3(6.2f, 0.24f, 0.58f), new Color(0.20f, 0.22f, 0.25f));

        CreateBox(_stageRoot, "CabinetBody", new Vector3(0f, -0.10f, 0.08f), new Vector3(3.65f, 3.02f, 0.22f), _cabinetColor);
        CreateBox(_stageRoot, "FrontDoorPanel", new Vector3(0f, -0.10f, -0.05f), new Vector3(3.35f, 2.76f, 0.12f), _doorColor);
        CreateBox(_stageRoot, "DoorCenterSeam", new Vector3(0f, -0.10f, -0.16f), new Vector3(0.035f, 2.58f, 0.08f), new Color(0.10f, 0.11f, 0.12f));
        CreateBox(_stageRoot, "ControlPanel", new Vector3(0f, 0.48f, -0.24f), new Vector3(2.72f, 1.58f, 0.12f), _darkPanel);

        BuildKnobAssembly();
        BuildPowerLamp();
        BuildPowerLines();
        BuildWarningLabel();
    }

    void BuildKnobAssembly()
    {
        GameObject ringGo = new GameObject("CabinetBreakerRing");
        ringGo.transform.parent = _stageRoot;
        LineRenderer ring = ringGo.AddComponent<LineRenderer>();
        ring.positionCount = 73;
        ring.startWidth = 0.022f;
        ring.endWidth = 0.022f;
        ring.material = MakeMaterial(new Color(0.76f, 0.82f, 0.86f));

        Vector3 center = new Vector3(-0.72f, 0.45f, -0.34f);
        for (int i = 0; i < ring.positionCount; i++)
        {
            float a = i / 72f * Mathf.PI * 2f;
            ring.SetPosition(i, center + new Vector3(Mathf.Cos(a) * KnobRadius, Mathf.Sin(a) * KnobRadius, 0f));
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            BuildSlotMarker(_stageRoot, center, _slots[i], i);
        }

        _knobRoot = new GameObject("CabinetModeSelectorKnob").transform;
        _knobRoot.parent = _stageRoot;
        _knobRoot.localPosition = center;

        GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        knob.name = "CabinetKnobBody";
        knob.transform.parent = _knobRoot;
        knob.transform.localPosition = Vector3.zero;
        knob.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        knob.transform.localScale = new Vector3(0.48f, 0.08f, 0.48f);
        DestroyCollider(knob);
        _knobRenderer = knob.GetComponent<Renderer>();
        SetColor(_knobRenderer, _blue);

        GameObject pointer = CreateBox(_knobRoot, "CabinetKnobPointer", new Vector3(0.31f, 0f, -0.08f), new Vector3(0.62f, 0.075f, 0.075f), _yellow);
        _pointerRenderer = pointer.GetComponent<Renderer>();

        CreateText(_stageRoot, "MainBreakerLabel", "MAIN BREAKER", new Vector3(-0.72f, 1.34f, -0.32f), 38, 0.036f, Color.white, TextAnchor.MiddleCenter);
    }

    void BuildSlotMarker(Transform parent, Vector3 center, ModeSlot slot, int index)
    {
        Vector3 direction = AngleToDirection(slot.angle);
        GameObject marker = CreateBox(parent, "CabinetModeSlot_" + index, center + direction * KnobRadius + new Vector3(0f, 0f, -0.02f), new Vector3(0.18f, 0.11f, 0.06f), _metal);
        marker.transform.localRotation = Quaternion.Euler(0f, 0f, slot.angle);
        slot.marker = marker.GetComponent<Renderer>();

        TextMesh text = CreateText(parent, "CabinetModeSlotLabel_" + index, slot.label, center + direction * 0.91f + new Vector3(0f, 0f, -0.02f), 34, 0.036f, new Color(0.86f, 0.93f, 1f), TextAnchor.MiddleCenter);
        slot.text = text;
    }

    void BuildPowerLamp()
    {
        CreateText(_stageRoot, "PowerText", "POWER", new Vector3(0.95f, 1.05f, -0.32f), 36, 0.037f, Color.white, TextAnchor.MiddleCenter);
        _powerHaloRenderer = CreateSphere(_stageRoot, "PowerLampHalo", new Vector3(0.95f, 0.57f, -0.36f), 0.48f, new Color(0.05f, 0.10f, 0.06f)).GetComponent<Renderer>();
        _powerLampRenderer = CreateSphere(_stageRoot, "PowerLampLens", new Vector3(0.95f, 0.57f, -0.43f), 0.24f, _lampOff).GetComponent<Renderer>();

        GameObject lightGo = new GameObject("PowerLampPointLight");
        lightGo.transform.parent = _stageRoot;
        lightGo.transform.localPosition = new Vector3(0.95f, 0.57f, -0.75f);
        _powerLight = lightGo.AddComponent<Light>();
        _powerLight.type = LightType.Point;
        _powerLight.color = _green;
        _powerLight.range = 2.5f;
        _powerLight.intensity = 0f;
        _powerLight.enabled = false;
    }

    void BuildPowerLines()
    {
        _supplyLineRenderer = CreateBox(_stageRoot, "SupplyLine", new Vector3(0.08f, 0.89f, -0.31f), new Vector3(0.92f, 0.055f, 0.055f), _lampOff).GetComponent<Renderer>();
        _loadLineRenderer = CreateBox(_stageRoot, "LoadLine", new Vector3(0.08f, 0.23f, -0.31f), new Vector3(0.92f, 0.055f, 0.055f), _lampOff).GetComponent<Renderer>();
        CreateBox(_stageRoot, "PowerBusLeft", new Vector3(-0.42f, 0.56f, -0.31f), new Vector3(0.06f, 0.72f, 0.055f), new Color(0.84f, 0.52f, 0.12f));
        CreateBox(_stageRoot, "PowerBusRight", new Vector3(0.58f, 0.56f, -0.31f), new Vector3(0.06f, 0.72f, 0.055f), new Color(0.84f, 0.52f, 0.12f));
    }

    void BuildWarningLabel()
    {
        CreateBox(_stageRoot, "WarningNameplate", new Vector3(0f, -0.95f, -0.26f), new Vector3(1.55f, 0.28f, 0.08f), new Color(1f, 0.76f, 0.08f));
        CreateText(_stageRoot, "WarningText", "ELECTRICAL CONTROL", new Vector3(0f, -0.96f, -0.34f), 34, 0.032f, new Color(0.06f, 0.06f, 0.05f), TextAnchor.MiddleCenter);
    }

    void BuildCursorAndGuide()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "LeadElectricalCabinetGripCursor";
        DestroyCollider(_cursor);
        _cursorRenderer = _cursor.GetComponent<Renderer>();
        SetColor(_cursorRenderer, _blue);

        GameObject lineGo = new GameObject("LeadElectricalCabinetGuideLine");
        _guideLine = lineGo.AddComponent<LineRenderer>();
        _guideLine.positionCount = 2;
        _guideLine.startWidth = 0.024f;
        _guideLine.endWidth = 0.012f;
        _guideLine.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _guideLine.enabled = false;
    }

    void BuildButtons()
    {
        _startButton = CreateButton("Start", new Vector3(-2.78f, -0.82f, -0.24f), new Vector3(0.72f, 0.36f, 0.10f), "START\n启动", new Color(0.10f, 0.62f, 0.26f));
        _resetButton = CreateButton("Reset", new Vector3(2.78f, -0.82f, -0.24f), new Vector3(0.72f, 0.36f, 0.10f), "RESET\n重置", new Color(0.10f, 0.62f, 0.26f));

        _startButton.Clicked += HandleStartClicked;
        _resetButton.Clicked += () => RestartTraining(true);
    }

    FingertipTapButton CreateButton(string name, Vector3 center, Vector3 size, string label, Color color)
    {
        GameObject go = new GameObject("LeadElectricalCabinetButton_" + name);
        go.transform.parent = _stageRoot;
        FingertipTapButton button = go.AddComponent<FingertipTapButton>();
        button.hand = hand;
        button.requireFreeHand = false;
        button.showGuideLine = false;
        button.Build(center, size, label, color);
        return button;
    }

    void BuildTexts()
    {
        CreateText(_stageRoot, "LeadElectricalCabinetTitle", "配电柜主断路器手势训练", new Vector3(0f, 2.02f, -0.18f), 48, 0.045f, Color.white, TextAnchor.MiddleCenter);

        _targetText = CreateText(_stageRoot, "LeadElectricalCabinetTarget", "", new Vector3(0f, 1.62f, -0.18f), 36, 0.036f, new Color(0.82f, 1f, 0.86f), TextAnchor.MiddleCenter);
        _status = CreateText(_stageRoot, "LeadElectricalCabinetStatus", "", new Vector3(0f, 1.42f, -0.18f), 30, 0.031f, new Color(0.76f, 0.88f, 1f), TextAnchor.MiddleCenter);
        _status.lineSpacing = 0.82f;
    }

    void HandleStartClicked()
    {
        if (_phase == TrainingPhase.Completed)
        {
            RestartTraining(true);
            return;
        }

        if (_phase != TrainingPhase.WaitingStart)
        {
            return;
        }

        _phase = TrainingPhase.RotateOn;
        _holdTimer = 0f;
        _feedback = "捏合旋钮并旋转到 ON，观察电源灯点亮";
        TrainingFlowController.Active?.ReportProgress(0f, "旋至 ON", CurrentInstruction());
    }

    void UpdateKnobInteraction()
    {
        if (hand == null || !hand.IsActive || _knobRoot == null)
        {
            _near = false;
            _rotating = false;
            return;
        }

        Vector3 grip = hand.GripPoint;
        Vector3 offset = grip - _knobRoot.position;
        float distance = new Vector2(offset.x, offset.y).magnitude;
        _near = distance <= GrabRadius;
        bool gripping = hand.PinchOnlyStrength >= GripThreshold;

        if (_near && gripping)
        {
            float handAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            if (!_rotating)
            {
                _rotating = true;
                _lastHandAngle = handAngle;
            }
            else
            {
                float step = Mathf.DeltaAngle(_lastHandAngle, handAngle);
                if (Mathf.Abs(step) >= AngleDeadZone)
                {
                    _angle = Mathf.Repeat(_angle + Mathf.Clamp(step, -MaxStepDegrees, MaxStepDegrees), 360f);
                }
                _lastHandAngle = handAngle;
            }
        }
        else
        {
            _rotating = false;
            if (hand.PinchOnlyStrength <= ReleaseThreshold)
            {
                _lastHandAngle = 0f;
            }
        }

        ApplyKnobRotation();
    }

    void UpdateSnapAndCompletion()
    {
        int nearest = NearestSlotIndex(_angle);
        if (!_rotating)
        {
            float nearestAngle = _slots[nearest].angle;
            float delta = Mathf.DeltaAngle(_angle, nearestAngle);
            float maxStep = SnapSpeed * Time.deltaTime * 60f;
            _angle = Mathf.Repeat(_angle + Mathf.Clamp(delta, -maxStep, maxStep), 360f);
            ApplyKnobRotation();
        }

        int targetSlot = TargetSlotIndex();
        bool correct = targetSlot >= 0
            && nearest == targetSlot
            && Mathf.Abs(Mathf.DeltaAngle(_angle, _slots[targetSlot].angle)) <= TargetTolerance;

        if (correct)
        {
            _holdTimer += Time.deltaTime;
        }
        else
        {
            _holdTimer = 0f;
        }

        if (correct && _holdTimer >= HoldToConfirm)
        {
            CompleteCurrentStep();
            return;
        }

    }

    void CompleteCurrentStep()
    {
        _holdTimer = 0f;

        if (_phase == TrainingPhase.RotateOn)
        {
            _phase = TrainingPhase.RotateOff;
            _feedback = "合闸完成，电源灯已点亮；继续旋回 OFF 完成分闸";
            TrainingFlowController.Active?.RecordSuccess("主断路器已旋至 ON，电源灯点亮");
            return;
        }

        if (_phase == TrainingPhase.RotateOff)
        {
            _phase = TrainingPhase.Completed;
            _feedback = "分闸完成，电源灯已熄灭";
            TrainingFlowController.Active?.RecordSuccess("主断路器已旋回 OFF，电源灯熄灭");
        }
    }

    void UpdateCursorAndGuide()
    {
        if (_cursor == null || _guideLine == null || hand == null)
        {
            return;
        }

        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            _guideLine.enabled = false;
            return;
        }

        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.14f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.11f, 0.22f, hand.PinchOnlyStrength);

        Color cursorColor = _rotating
            ? _green
            : _near ? _yellow : _blue;
        SetColor(_cursorRenderer, cursorColor);

        bool showGuide = (_near || _rotating) && (_phase == TrainingPhase.RotateOn || _phase == TrainingPhase.RotateOff);
        _guideLine.enabled = showGuide;
        if (!showGuide || _knobRoot == null)
        {
            return;
        }

        _guideLine.SetPosition(0, grip);
        _guideLine.SetPosition(1, _knobRoot.position + new Vector3(0f, 0f, -0.14f));
    }

    void RefreshButtons()
    {
        if (_startButton != null)
        {
            _startButton.interactable = _phase == TrainingPhase.WaitingStart || _phase == TrainingPhase.Completed;
        }

        if (_resetButton != null)
        {
            _resetButton.interactable = true;
        }
    }

    void RefreshVisuals()
    {
        int nearest = NearestSlotIndex(_angle);
        int target = TargetSlotIndex();
        bool atOn = Mathf.Abs(Mathf.DeltaAngle(_angle, _slots[OnSlot].angle)) <= LightSwitchTolerance;
        if (atOn != _breakerOn)
        {
            _breakerOn = atOn;
            _lightChangedAt = Time.time;
        }

        bool correct = target >= 0
            && nearest == target
            && Mathf.Abs(Mathf.DeltaAngle(_angle, _slots[target].angle)) <= TargetTolerance;

        SetColor(_knobRenderer, _rotating ? _green : _near ? _yellow : _blue);
        SetColor(_pointerRenderer, correct ? _green : _yellow);

        for (int i = 0; i < _slots.Length; i++)
        {
            Color markerColor = i == target
                ? _green
                : i == nearest ? _yellow : _metal;
            SetColor(_slots[i].marker, markerColor);
            if (_slots[i].text != null)
            {
                _slots[i].text.color = i == target ? new Color(0.82f, 1f, 0.86f) : new Color(0.86f, 0.93f, 1f);
            }
        }

        ApplyPowerLightVisuals();
    }

    void ApplyPowerLightVisuals()
    {
        float elapsed = Time.time - _lightChangedAt;
        float pulse = elapsed < 0.85f ? Mathf.PingPong(elapsed * 7f, 1f) : 0f;
        Color lineColor = _breakerOn ? Color.Lerp(_green, Color.white, pulse * 0.45f) : _lampOff;
        Color lampColor = _breakerOn
            ? Color.Lerp(_green, Color.white, pulse * 0.55f)
            : Color.Lerp(_lampOff, _red * 0.45f, pulse * 0.35f);

        SetEmissionColor(_powerLampRenderer, lampColor, _breakerOn ? 2.8f + pulse * 2.2f : 0.25f + pulse * 0.45f);
        SetEmissionColor(_powerHaloRenderer, _breakerOn ? _green * 0.55f : new Color(0.03f, 0.06f, 0.035f), _breakerOn ? 1.4f + pulse : 0.15f);
        SetEmissionColor(_supplyLineRenderer, lineColor, _breakerOn ? 1.25f + pulse : 0.20f);
        SetEmissionColor(_loadLineRenderer, lineColor, _breakerOn ? 1.25f + pulse : 0.20f);

        if (_powerLight != null)
        {
            _powerLight.enabled = _breakerOn || pulse > 0.05f;
            _powerLight.color = lampColor;
            _powerLight.intensity = _breakerOn ? 1.25f + pulse * 0.90f : pulse * 0.30f;
        }
    }

    void UpdateStatusText()
    {
        if (_targetText != null)
        {
            int target = TargetSlotIndex();
            _targetText.text = target >= 0 ? "目标档位：" + _slots[target].label : "训练完成，可重置或返回";
        }

        if (_status == null)
        {
            return;
        }

        string handState = hand != null && hand.IsActive ? "手势已连接" : "等待手势识别";
        int nearest = NearestSlotIndex(_angle);
        string rotateState = _rotating ? "正在旋转" : _near ? "靠近旋钮" : "移动到旋钮";
        _status.text =
            "状态：" + CurrentInstruction() +
            "    当前：" + _slots[nearest].label +
            "\n" + handState +
            "    " + rotateState +
            "    捏合：" + (hand != null ? hand.PinchOnlyStrength.ToString("0.00") : "0.00") +
            "\n" + _feedback;
    }

    void EnsureReturnInput()
    {
        ReturnToHubInput returnInput = FindObjectOfType<ReturnToHubInput>();
        if (returnInput == null)
        {
            returnInput = gameObject.AddComponent<ReturnToHubInput>();
        }

        string currentScene = SceneManager.GetActiveScene().name;
        returnInput.fallbackSceneName = string.IsNullOrEmpty(currentScene) ? "lead-train1" : currentScene;
        returnInput.preferFallbackScene = true;
    }

    int TargetSlotIndex()
    {
        if (_phase == TrainingPhase.RotateOn)
        {
            return OnSlot;
        }

        if (_phase == TrainingPhase.RotateOff)
        {
            return OffSlot;
        }

        return -1;
    }

    string PhaseName()
    {
        switch (_phase)
        {
            case TrainingPhase.WaitingStart:
                return "点击启动按钮";
            case TrainingPhase.RotateOn:
                return "旋至 ON";
            case TrainingPhase.RotateOff:
                return "旋回 OFF";
            case TrainingPhase.Completed:
                return "训练完成";
            default:
                return "准备中";
        }
    }

    string CurrentInstruction()
    {
        switch (_phase)
        {
            case TrainingPhase.WaitingStart:
                return "点击启动";
            case TrainingPhase.RotateOn:
                return "捏合旋钮旋至 ON";
            case TrainingPhase.RotateOff:
                return "捏合旋钮旋回 OFF";
            case TrainingPhase.Completed:
                return "已完成合闸与分闸";
            default:
                return "准备中";
        }
    }

    float Progress01()
    {
        if (_phase == TrainingPhase.Completed)
        {
            return 1f;
        }

        if (_phase == TrainingPhase.RotateOff)
        {
            return 0.55f;
        }

        if (_phase == TrainingPhase.RotateOn)
        {
            return 0.15f;
        }

        return 0f;
    }

    int NearestSlotIndex(float angle)
    {
        int best = 0;
        float bestDelta = float.MaxValue;
        for (int i = 0; i < _slots.Length; i++)
        {
            float delta = Mathf.Abs(Mathf.DeltaAngle(angle, _slots[i].angle));
            if (delta < bestDelta)
            {
                bestDelta = delta;
                best = i;
            }
        }
        return best;
    }

    void ApplyKnobRotation()
    {
        if (_knobRoot != null)
        {
            _knobRoot.localRotation = Quaternion.Euler(0f, 0f, _angle);
        }
    }

    static Vector3 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }

    GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.parent = parent;
        box.transform.localPosition = localPosition;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = localScale;
        DestroyCollider(box);
        SetColor(box.GetComponent<Renderer>(), color);
        return box;
    }

    GameObject CreateSphere(Transform parent, string name, Vector3 localPosition, float size, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.parent = parent;
        sphere.transform.localPosition = localPosition;
        sphere.transform.localRotation = Quaternion.identity;
        sphere.transform.localScale = Vector3.one * size;
        DestroyCollider(sphere);
        SetColor(sphere.GetComponent<Renderer>(), color);
        return sphere;
    }

    TextMesh CreateText(Transform parent, string name, string text, Vector3 localPosition, int fontSize, float characterSize, Color color, TextAnchor anchor)
    {
        GameObject textGo = new GameObject(name);
        textGo.transform.parent = parent;
        textGo.transform.localPosition = localPosition;
        TextMesh textMesh = textGo.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = anchor;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = characterSize;
        textMesh.color = color;
        return textMesh;
    }

    static void DestroyCollider(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    static void SetColor(Renderer renderer, Color color)
    {
        if (renderer == null)
        {
            return;
        }

        Material mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }
    }

    static void SetEmissionColor(Renderer renderer, Color color, float intensity)
    {
        if (renderer == null)
        {
            return;
        }

        Material mat = renderer.material;
        Color emission = color * Mathf.Max(0f, intensity);
        mat.color = color;
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
        }
    }

    static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        return material;
    }
}
