using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LeadTrainFireExtinguisherGestureTrainingController : MonoBehaviour
{
    public const string TaskId = "lead_train1_fire_extinguisher_gesture";

    public HandInput hand;
    public HandVisual handVisual;

    enum TrainingPhase
    {
        WaitingStart,
        Training,
        Completed
    }

    enum ExtinguisherStep
    {
        Label = 0,
        Gauge = 1,
        Pin = 2,
        Nozzle = 3,
        Handle = 4
    }

    readonly string[] _stepNames =
    {
        "目视标签",
        "读压力表",
        "查保险销",
        "查喷管",
        "试压把"
    };

    readonly string[] _stepHints =
    {
        "用食指点按白色标签区，确认标识清晰且在有效期内",
        "用食指点按压力表，确认指针位于绿色区域",
        "捏住黄色保险销，轻轻向左拉一下，不要拔出",
        "捏住喷嘴或软管，沿喷管方向滑动检查是否顺畅",
        "捏住压把，收紧后松开，确认压把能回弹"
    };

    TrainingPhase _phase = TrainingPhase.WaitingStart;
    Transform _stageRoot;
    TextMesh _title;
    TextMesh _status;
    TextMesh _feedbackText;
    FingertipTapButton _startButton;
    FingertipTapButton _resetButton;
    GameObject _cursor;
    Renderer _cursorRenderer;

    Transform _labelTarget;
    Transform _gaugeTarget;
    Transform _pinTarget;
    Transform _nozzleTarget;
    Transform _handleTarget;
    Transform _gaugeGroup;
    Transform _gaugeNeedle;
    Transform _pinVisual;
    Transform _nozzleVisual;
    Transform _handleVisual;

    Renderer[] _labelRenderers;
    Renderer[] _gaugeRenderers;
    Renderer[] _pinRenderers;
    Renderer[] _nozzleRenderers;
    Renderer[] _handleRenderers;

    readonly bool[] _completedSteps = new bool[5];
    int _stepIndex;
    bool _initialized;
    string _feedback = "点击启动后开始 5 步点检";
    float _lastMistakeAt = -99f;
    float _flashUntil;
    int _flashStep = -1;
    Color _flashColor = Color.white;

    bool _tapArmed;
    bool _tapReady;
    bool _tapPressed;
    bool _hasLastTapPoint;
    Vector3 _lastTapPoint;
    float _tapHoverStartAt = -99f;
    float _lastTapAt = -99f;

    int _grabZone = -1;
    Vector3 _grabStart;
    bool _squeezeReached;

    readonly Color _red = new Color(0.86f, 0.05f, 0.03f);
    readonly Color _darkRed = new Color(0.45f, 0.01f, 0.01f);
    readonly Color _black = new Color(0.025f, 0.026f, 0.028f);
    readonly Color _metal = new Color(0.46f, 0.48f, 0.49f);
    readonly Color _label = new Color(0.92f, 0.90f, 0.82f);
    readonly Color _green = new Color(0.16f, 0.86f, 0.30f);
    readonly Color _yellow = new Color(1f, 0.76f, 0.10f);
    readonly Color _blue = new Color(0.22f, 0.62f, 1f);
    readonly Color _done = new Color(0.20f, 0.90f, 0.44f);
    readonly Color _warning = new Color(1f, 0.30f, 0.18f);

    const int ZoneLabel = 0;
    const int ZoneGauge = 1;
    const int ZonePin = 2;
    const int ZoneNozzle = 3;
    const int ZoneHandle = 4;

    const float TapReadySeconds = 0.10f;
    const float TapMinDownDelta = 0.030f;
    const float TapMinDownSpeed = 0.16f;
    const float TapCooldownSeconds = 0.38f;
    const float MistakeCooldownSeconds = 0.65f;

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
        BuildCursor();
        BuildButtons();
        BuildTexts();
        TrainingFlowController.EnsureExists(TaskId);
        RestartTraining(false);
        EnsureReturnInput();
    }

    public void RestartTraining(bool resetTrainingFlow = true)
    {
        _phase = TrainingPhase.WaitingStart;
        _stepIndex = 0;
        _feedback = "点击启动后开始 5 步点检";
        _flashStep = -1;
        _grabZone = -1;
        _squeezeReached = false;
        ResetTap();

        for (int i = 0; i < _completedSteps.Length; i++)
        {
            _completedSteps[i] = false;
        }

        if (resetTrainingFlow)
        {
            TrainingFlowController.EnsureExists(TaskId);
        }

        ResetMovableParts();
        RefreshButtons();
        RefreshRegionColors();
        UpdateStatusText();
    }

    void Update()
    {
        if (!_initialized)
        {
            return;
        }

        UpdateCursor();
        RefreshRegionColors();
        RefreshButtons();
        UpdateStatusText();

        if (_phase != TrainingPhase.Training)
        {
            TrainingFlowController.Active?.ReportProgress(Progress01(), PhaseName(), CurrentHint());
            return;
        }

        UpdateTapInteraction();
        UpdateGrabInteraction();
        TrainingFlowController.Active?.ReportProgress(Progress01(), PhaseName(), CurrentHint());
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
            GameObject camGo = new GameObject("LeadTrainFireGestureCamera");
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
        }

        cam.transform.SetParent(null, true);
        cam.transform.position = new Vector3(0f, 0.34f, -6.4f);
        cam.transform.rotation = Quaternion.identity;
        cam.fieldOfView = 46f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.075f, 0.085f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SetupTrainingLights()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.36f, 0.36f, 0.34f);

        GameObject keyGo = new GameObject("LeadFire_KeyLight");
        Light key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 1.0f;
        key.color = new Color(1f, 0.95f, 0.86f);
        keyGo.transform.rotation = Quaternion.Euler(38f, -28f, 0f);

        GameObject fillGo = new GameObject("LeadFire_FillLight");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.intensity = 1.45f;
        fill.range = 8f;
        fill.color = new Color(0.62f, 0.78f, 1f);
        fillGo.transform.position = new Vector3(-2.6f, 2.2f, -2.1f);
    }

    void CreateVirtualHand()
    {
        if (hand != null)
        {
            handVisual = hand.GetComponent<HandVisual>();
            return;
        }

        GameObject handGo = new GameObject("LeadTrainFireGestureHand");
        hand = handGo.AddComponent<HandInput>();
        hand.url = "ws://127.0.0.1:8765";
        hand.planeWidth = 5.9f;
        hand.planeHeight = 4.0f;
        hand.planeOrigin = new Vector3(0f, 0f, -0.58f);
        hand.gain = 1.24f;
        hand.smoothing = 0.66f;
        hand.graceTime = 0.45f;

        handVisual = handGo.AddComponent<HandVisual>();
        handVisual.jointRadius = 0.041f;
        handVisual.skinColor = new Color(0.96f, 0.78f, 0.62f);
        handVisual.gripColor = new Color(0.20f, 0.92f, 0.48f);
        handVisual.enablePhysicalColliders = false;
    }

    void BuildStage()
    {
        GameObject root = new GameObject("LeadTrainFireGestureStage");
        root.transform.parent = transform;
        root.transform.localPosition = Vector3.zero;
        _stageRoot = root.transform;

        CreateBox(_stageRoot, "TrainingBackdrop", new Vector3(0f, 0.03f, 0.44f), new Vector3(7.2f, 4.25f, 0.18f), new Color(0.10f, 0.12f, 0.14f));
        CreateBox(_stageRoot, "WorkbenchDeck", new Vector3(0f, -1.78f, 0.08f), new Vector3(6.7f, 0.24f, 0.58f), new Color(0.20f, 0.22f, 0.24f));
        CreateBox(_stageRoot, "InspectionWallPanel", new Vector3(0f, -0.12f, 0.05f), new Vector3(3.6f, 3.28f, 0.16f), new Color(0.17f, 0.19f, 0.21f));
        CreateBox(_stageRoot, "InspectionWallTop", new Vector3(0f, 1.58f, -0.04f), new Vector3(3.78f, 0.08f, 0.12f), _metal);
        CreateBox(_stageRoot, "InspectionWallBottom", new Vector3(0f, -1.72f, -0.04f), new Vector3(3.78f, 0.08f, 0.12f), _metal);

        BuildExtinguisherModel();
    }

    void BuildExtinguisherModel()
    {
        Transform root = _stageRoot;

        CreateBox(root, "WallBackPlate", new Vector3(0f, -0.16f, -0.10f), new Vector3(1.36f, 2.55f, 0.06f), new Color(0.38f, 0.40f, 0.41f));
        CreateBox(root, "UpperBracket", new Vector3(0f, 0.68f, -0.25f), new Vector3(1.02f, 0.10f, 0.20f), _metal);
        CreateBox(root, "LowerBracket", new Vector3(0f, -0.62f, -0.25f), new Vector3(1.02f, 0.10f, 0.20f), _metal);
        CreateBox(root, "BottomShelf", new Vector3(0f, -1.22f, -0.20f), new Vector3(1.08f, 0.12f, 0.34f), _metal);

        CreateCylinder(root, "DryPowderBody", new Vector3(0f, -0.34f, -0.38f), 0.86f, 2.0f, _red, Axis.Y);
        CreateCylinder(root, "BottomBlackBase", new Vector3(0f, -1.34f, -0.38f), 0.78f, 0.16f, _black, Axis.Y);
        CreateCylinder(root, "TopNeck", new Vector3(0f, 0.74f, -0.38f), 0.26f, 0.24f, _darkRed, Axis.Y);
        CreateBox(root, "ValveBlock", new Vector3(0f, 0.93f, -0.42f), new Vector3(0.38f, 0.12f, 0.20f), _metal);

        GameObject upperHandle = CreateBox(root, "UpperHandle", new Vector3(0f, 1.15f, -0.56f), new Vector3(0.78f, 0.08f, 0.15f), _black);
        GameObject lowerHandle = CreateBox(root, "LowerHandle", new Vector3(0f, 1.01f, -0.58f), new Vector3(0.68f, 0.07f, 0.14f), _black);
        _handleVisual = upperHandle.transform;
        _handleTarget = CreateTarget("HandleTarget", new Vector3(0f, 1.09f, -0.66f));
        _handleRenderers = RenderersOf(upperHandle, lowerHandle);

        GameObject pin = CreateCylinder(root, "SafetyPinYellowRing", new Vector3(-0.50f, 0.93f, -0.60f), 0.20f, 0.045f, _yellow, Axis.X);
        _pinVisual = pin.transform;
        _pinTarget = CreateTarget("PinTarget", pin.transform.localPosition + new Vector3(0f, 0f, -0.06f));
        _pinRenderers = RenderersOf(pin);

        GameObject labelPanel = CreateBox(root, "InspectionLabelPanel", new Vector3(0f, -0.42f, -0.86f), new Vector3(0.62f, 0.58f, 0.035f), _label);
        CreateText(root, "InspectionLabelText", "2026 检验合格\n有效期内", new Vector3(0f, -0.42f, -0.885f), 0.060f, new Color(0.12f, 0.12f, 0.11f), TextAnchor.MiddleCenter);
        _labelTarget = CreateTarget("LabelTarget", labelPanel.transform.localPosition + new Vector3(0f, 0f, -0.05f));
        _labelRenderers = RenderersOf(labelPanel);

        BuildGauge(root);
        BuildHoseAndNozzle(root);

        CreateText(root, "FirePartLabel_Label", "标签", new Vector3(-0.86f, -0.42f, -0.80f), 0.052f, Color.white, TextAnchor.MiddleCenter);
        CreateText(root, "FirePartLabel_Gauge", "压力表", new Vector3(0.80f, 0.44f, -0.80f), 0.052f, Color.white, TextAnchor.MiddleCenter);
        CreateText(root, "FirePartLabel_Pin", "保险销", new Vector3(-0.98f, 0.94f, -0.78f), 0.052f, Color.white, TextAnchor.MiddleCenter);
        CreateText(root, "FirePartLabel_Nozzle", "喷管", new Vector3(1.05f, 0.02f, -0.78f), 0.052f, Color.white, TextAnchor.MiddleCenter);
        CreateText(root, "FirePartLabel_Handle", "压把", new Vector3(0.82f, 1.15f, -0.78f), 0.052f, Color.white, TextAnchor.MiddleCenter);
    }

    void BuildGauge(Transform parent)
    {
        GameObject gaugeRoot = new GameObject("PressureGaugeGroup");
        gaugeRoot.transform.parent = parent;
        gaugeRoot.transform.localPosition = new Vector3(0f, 0.43f, -0.88f);
        _gaugeGroup = gaugeRoot.transform;

        GameObject ring = CreateCylinder(_gaugeGroup, "PressureGaugeMetalRing", Vector3.zero, 0.48f, 0.055f, _metal, Axis.Z);
        GameObject face = CreateCylinder(_gaugeGroup, "PressureGaugeFace", new Vector3(0f, 0f, -0.028f), 0.40f, 0.030f, new Color(0.96f, 0.96f, 0.91f), Axis.Z);
        GameObject zone = CreateGaugeArc(_gaugeGroup, "PressureGaugeGreenZone", new Vector3(0f, 0f, -0.058f), 0.08f, 0.18f, 205f, 330f, _green);

        GameObject pivot = new GameObject("PressureGaugeNeedlePivot");
        pivot.transform.parent = _gaugeGroup;
        pivot.transform.localPosition = new Vector3(0f, 0f, -0.08f);
        pivot.transform.localRotation = Quaternion.Euler(0f, 0f, 265f);
        _gaugeNeedle = pivot.transform;

        GameObject needle = CreateBox(pivot.transform, "PressureGaugeNeedle", new Vector3(0.12f, 0f, 0f), new Vector3(0.22f, 0.022f, 0.018f), _warning);
        GameObject hub = CreateCylinder(_gaugeGroup, "PressureGaugeHub", new Vector3(0f, 0f, -0.10f), 0.055f, 0.018f, _black, Axis.Z);

        CreateText(_gaugeGroup, "GaugeNormalText", "绿区", new Vector3(0f, -0.28f, -0.11f), 0.035f, new Color(0.08f, 0.30f, 0.10f), TextAnchor.MiddleCenter);
        _gaugeTarget = CreateTarget("GaugeTarget", _gaugeGroup.localPosition + new Vector3(0f, 0f, -0.12f));
        _gaugeRenderers = RenderersOf(ring, face, zone, needle, hub);
    }

    void BuildHoseAndNozzle(Transform parent)
    {
        GameObject hoseRoot = new GameObject("HoseAndNozzleGroup");
        hoseRoot.transform.parent = parent;
        hoseRoot.transform.localPosition = Vector3.zero;
        _nozzleVisual = hoseRoot.transform;

        Vector3[] points =
        {
            new Vector3(0.28f, 0.84f, -0.62f),
            new Vector3(0.72f, 0.58f, -0.74f),
            new Vector3(0.78f, 0.18f, -0.78f),
            new Vector3(0.56f, -0.14f, -0.76f)
        };

        GameObject[] parts = new GameObject[points.Length];
        for (int i = 0; i < points.Length - 1; i++)
        {
            parts[i] = CreateCylinderBetween(hoseRoot.transform, "BlackRubberHose_" + i, points[i], points[i + 1], 0.055f, _black);
        }

        GameObject nozzle = CreateCylinder(hoseRoot.transform, "Nozzle", new Vector3(0.52f, -0.22f, -0.76f), 0.095f, 0.46f, _black, Axis.X);
        parts[points.Length - 1] = nozzle;
        _nozzleTarget = CreateTarget("NozzleTarget", new Vector3(0.62f, -0.16f, -0.88f));
        _nozzleRenderers = RenderersOf(parts);
    }

    void BuildButtons()
    {
        _startButton = CreateButton("Start", new Vector3(-1.28f, -1.78f, -0.24f), new Vector3(1.18f, 0.34f, 0.08f), "启动", new Color(0.15f, 0.55f, 0.92f));
        _resetButton = CreateButton("Reset", new Vector3(1.28f, -1.78f, -0.24f), new Vector3(1.18f, 0.34f, 0.08f), "重置", new Color(0.86f, 0.35f, 0.24f));

        _startButton.Clicked += HandleStartClicked;
        _resetButton.Clicked += () => RestartTraining(true);
        RefreshButtons();
    }

    FingertipTapButton CreateButton(string name, Vector3 center, Vector3 size, string label, Color color)
    {
        GameObject go = new GameObject("LeadFireGestureButton_" + name);
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
        GameObject titleGo = new GameObject("LeadFireGestureTitle");
        titleGo.transform.parent = _stageRoot;
        titleGo.transform.localPosition = new Vector3(0f, 2.02f, -0.18f);
        _title = titleGo.AddComponent<TextMesh>();
        _title.text = "灭火器 5 步点检手势训练";
        _title.anchor = TextAnchor.MiddleCenter;
        _title.alignment = TextAlignment.Center;
        _title.fontSize = 48;
        _title.characterSize = 0.044f;
        _title.color = Color.white;

        GameObject statusGo = new GameObject("LeadFireGestureStatus");
        statusGo.transform.parent = _stageRoot;
        statusGo.transform.localPosition = new Vector3(0f, 1.62f, -0.20f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.MiddleCenter;
        _status.alignment = TextAlignment.Center;
        _status.fontSize = 33;
        _status.characterSize = 0.032f;
        _status.lineSpacing = 0.84f;
        _status.color = new Color(0.78f, 0.90f, 1f);

        GameObject feedbackGo = new GameObject("LeadFireGestureFeedback");
        feedbackGo.transform.parent = _stageRoot;
        feedbackGo.transform.localPosition = new Vector3(0f, -1.46f, -0.20f);
        _feedbackText = feedbackGo.AddComponent<TextMesh>();
        _feedbackText.anchor = TextAnchor.MiddleCenter;
        _feedbackText.alignment = TextAlignment.Center;
        _feedbackText.fontSize = 32;
        _feedbackText.characterSize = 0.032f;
        _feedbackText.color = new Color(1f, 0.88f, 0.55f);
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "LeadFireGestureCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();
        SetColor(_cursorRenderer, _blue);
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

        _phase = TrainingPhase.Training;
        _feedback = "步骤 1/5：请先目视检查标签";
        TrainingFlowController.Active?.ReportProgress(0f, "灭火器点检", CurrentHint());
    }

    void UpdateTapInteraction()
    {
        if (hand == null || !hand.IsActive)
        {
            ResetTap();
            return;
        }

        Vector3 point = TapPoint();
        int zone = HitTestTapZone(point);
        bool tap = UpdateTapState(point, zone >= 0);
        if (!tap || zone < 0)
        {
            return;
        }

        HandleZoneAction(zone, "tap");
    }

    void UpdateGrabInteraction()
    {
        if (hand == null || !hand.IsActive)
        {
            _grabZone = -1;
            _squeezeReached = false;
            ResetMovableParts();
            return;
        }

        Vector3 grip = hand.GripPoint;
        float signal = hand.PinchOnlyStrength;

        if (_grabZone < 0)
        {
            if (signal < 0.48f)
            {
                return;
            }

            int zone = HitTestGrabZone(grip);
            if (zone < 0)
            {
                return;
            }

            _grabZone = zone;
            _grabStart = grip;
            _squeezeReached = false;
            HandleGrabStart(zone);
            return;
        }

        if (signal < 0.22f)
        {
            if (_grabZone == ZoneHandle && _squeezeReached && CurrentStep() == ExtinguisherStep.Handle)
            {
                CompleteCurrentStep("压把回弹正常");
            }

            _grabZone = -1;
            _squeezeReached = false;
            ResetMovableParts();
            return;
        }

        if (_grabZone == ZonePin)
        {
            UpdatePinGrab(grip);
        }
        else if (_grabZone == ZoneNozzle)
        {
            UpdateNozzleGrab(grip);
        }
        else if (_grabZone == ZoneHandle)
        {
            UpdateHandleGrab(signal);
        }
    }

    void HandleGrabStart(int zone)
    {
        if (_phase != TrainingPhase.Training)
        {
            return;
        }

        if (zone == ZoneHandle && CurrentStep() != ExtinguisherStep.Handle)
        {
            RecordMistake("点检阶段请勿提前按压把，请先完成前面步骤");
            return;
        }

        if (zone == ZonePin && CurrentStep() != ExtinguisherStep.Pin)
        {
            HandleZoneAction(zone, "grab");
            return;
        }

        if (zone == ZoneNozzle && CurrentStep() != ExtinguisherStep.Nozzle)
        {
            HandleZoneAction(zone, "grab");
        }
    }

    void UpdatePinGrab(Vector3 grip)
    {
        if (CurrentStep() != ExtinguisherStep.Pin)
        {
            return;
        }

        float pullLeft = Mathf.Max(0f, _grabStart.x - grip.x);
        Vector3 local = _pinVisual.localPosition;
        local.x = -0.50f - Mathf.Min(pullLeft, 0.34f);
        _pinVisual.localPosition = local;

        if (pullLeft > 0.54f)
        {
            RecordMistake("误拔保险销！点检时仅做轻拉确认");
            _grabZone = -1;
            ResetMovableParts();
            return;
        }

        if (pullLeft > 0.18f)
        {
            CompleteCurrentStep("保险销完好、在位");
            _grabZone = -1;
            ResetMovableParts();
        }
    }

    void UpdateNozzleGrab(Vector3 grip)
    {
        if (CurrentStep() != ExtinguisherStep.Nozzle)
        {
            return;
        }

        Vector3 delta = grip - _grabStart;
        Vector2 slideAxis = new Vector2(0.72f, -0.28f).normalized;
        float slide = Vector2.Dot(new Vector2(delta.x, delta.y), slideAxis);
        Vector3 local = _nozzleVisual.localPosition;
        local.x = Mathf.Clamp(slide * 0.16f, -0.05f, 0.12f);
        local.y = Mathf.Clamp(-slide * 0.05f, -0.04f, 0.03f);
        _nozzleVisual.localPosition = local;

        if (slide > 0.36f)
        {
            CompleteCurrentStep("喷管无折弯、无堵塞");
            _grabZone = -1;
            ResetMovableParts();
        }
    }

    void UpdateHandleGrab(float signal)
    {
        if (CurrentStep() != ExtinguisherStep.Handle)
        {
            return;
        }

        float press = Mathf.Clamp01((signal - 0.46f) / 0.34f);
        Vector3 local = _handleVisual.localPosition;
        local.y = 1.15f - press * 0.10f;
        _handleVisual.localPosition = local;

        if (signal > 0.74f)
        {
            _squeezeReached = true;
            _feedback = "已压下压把，请松开观察回弹";
        }
    }

    void HandleZoneAction(int zone, string action)
    {
        if (_phase != TrainingPhase.Training)
        {
            return;
        }

        ExtinguisherStep expected = CurrentStep();
        ExtinguisherStep actual = (ExtinguisherStep)zone;

        if ((action == "tap" && (actual == ExtinguisherStep.Pin || actual == ExtinguisherStep.Nozzle || actual == ExtinguisherStep.Handle))
            || (action == "grab" && (actual == ExtinguisherStep.Label || actual == ExtinguisherStep.Gauge)))
        {
            RecordMistake("请使用正确手势：" + CurrentHint());
            return;
        }

        if (actual != expected)
        {
            if (_completedSteps[zone])
            {
                RecordMistake("该步骤已完成，请继续：" + CurrentHint());
            }
            else
            {
                RecordMistake("步骤 " + (_stepIndex + 1) + "/5：请先" + _stepNames[_stepIndex]);
            }
            return;
        }

        switch (expected)
        {
            case ExtinguisherStep.Label:
                CompleteCurrentStep("标识清晰、在检验有效期内");
                break;
            case ExtinguisherStep.Gauge:
                StartCoroutine(PlayGaugeInspectFeedback());
                CompleteCurrentStep("压力正常，指针位于绿色区域");
                break;
        }
    }

    void CompleteCurrentStep(string message)
    {
        if (_phase != TrainingPhase.Training || _stepIndex < 0 || _stepIndex >= _completedSteps.Length)
        {
            return;
        }

        int completedIndex = _stepIndex;
        _completedSteps[completedIndex] = true;
        _feedback = message;
        _flashStep = completedIndex;
        _flashColor = _done;
        _flashUntil = Time.time + 0.34f;
        TrainingFlowController.Active?.RecordSuccess(_stepNames[completedIndex] + "完成：" + message);

        _stepIndex++;
        if (_stepIndex >= _completedSteps.Length)
        {
            _phase = TrainingPhase.Completed;
            _feedback = "灭火器 5 步点检完成";
            TrainingFlowController.Active?.CompleteTraining(_feedback);
        }
    }

    System.Collections.IEnumerator PlayGaugeInspectFeedback()
    {
        if (_gaugeGroup == null)
        {
            yield break;
        }

        Vector3 originalScale = _gaugeGroup.localScale;
        Vector3 originalPosition = _gaugeGroup.localPosition;
        Vector3 enlargedScale = Vector3.one * 1.22f;
        Vector3 enlargedPosition = originalPosition + new Vector3(0f, 0.05f, -0.10f);

        float elapsed = 0f;
        const float duration = 0.16f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _gaugeGroup.localScale = Vector3.Lerp(originalScale, enlargedScale, t);
            _gaugeGroup.localPosition = Vector3.Lerp(originalPosition, enlargedPosition, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.55f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _gaugeGroup.localScale = Vector3.Lerp(enlargedScale, originalScale, t);
            _gaugeGroup.localPosition = Vector3.Lerp(enlargedPosition, originalPosition, t);
            yield return null;
        }

        _gaugeGroup.localScale = originalScale;
        _gaugeGroup.localPosition = originalPosition;
    }

    void RecordMistake(string message)
    {
        if (Time.time - _lastMistakeAt < MistakeCooldownSeconds)
        {
            return;
        }

        _lastMistakeAt = Time.time;
        _feedback = message;
        _flashStep = Mathf.Clamp(_stepIndex, 0, 4);
        _flashColor = _warning;
        _flashUntil = Time.time + 0.42f;
        TrainingFlowController.Active?.RecordMistake(message);
    }

    int HitTestTapZone(Vector3 point)
    {
        if (IsInsideRect(point, _labelTarget.position, new Vector2(0.74f, 0.66f))) return ZoneLabel;
        if (IsInsideCircle(point, _gaugeTarget.position, 0.34f)) return ZoneGauge;
        if (IsInsideCircle(point, _pinTarget.position, 0.24f)) return ZonePin;
        if (IsInsideRect(point, _nozzleTarget.position, new Vector2(0.70f, 0.45f))) return ZoneNozzle;
        if (IsInsideRect(point, _handleTarget.position, new Vector2(0.94f, 0.34f))) return ZoneHandle;
        return -1;
    }

    int HitTestGrabZone(Vector3 grip)
    {
        if (IsInsideCircle(grip, _pinTarget.position, 0.30f)) return ZonePin;
        if (IsInsideRect(grip, _nozzleTarget.position, new Vector2(0.82f, 0.52f))) return ZoneNozzle;
        if (IsInsideRect(grip, _handleTarget.position, new Vector2(1.05f, 0.44f))) return ZoneHandle;
        if (IsInsideRect(grip, _labelTarget.position, new Vector2(0.76f, 0.68f))) return ZoneLabel;
        if (IsInsideCircle(grip, _gaugeTarget.position, 0.36f)) return ZoneGauge;
        return -1;
    }

    bool UpdateTapState(Vector3 point, bool inHover)
    {
        if (!inHover)
        {
            ResetTap();
            return false;
        }

        if (!_tapArmed)
        {
            _tapArmed = true;
            _tapReady = false;
            _tapPressed = false;
            _hasLastTapPoint = true;
            _lastTapPoint = point;
            _tapHoverStartAt = Time.time;
            return false;
        }

        if (!_hasLastTapPoint)
        {
            _hasLastTapPoint = true;
            _lastTapPoint = point;
            _tapHoverStartAt = Time.time;
            return false;
        }

        if (!_tapReady)
        {
            if (Time.time - _tapHoverStartAt < TapReadySeconds)
            {
                if (Vector2.Distance(new Vector2(point.x, point.y), new Vector2(_lastTapPoint.x, _lastTapPoint.y)) > 0.07f)
                {
                    _tapHoverStartAt = Time.time;
                    _lastTapPoint = point;
                }
                return false;
            }

            _tapReady = true;
            _lastTapPoint = point;
            return false;
        }

        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        Vector3 frameDelta = point - _lastTapPoint;
        if (!_tapPressed && point.y > _lastTapPoint.y)
        {
            _lastTapPoint = point;
        }

        float downDistance = _lastTapPoint.y - point.y;
        float downSpeed = Mathf.Max(0f, -frameDelta.y / dt);
        if (_tapPressed)
        {
            if (downDistance <= TapMinDownDelta * 0.25f)
            {
                _tapPressed = false;
            }
            return false;
        }

        bool canTap = ((downDistance >= TapMinDownDelta && downSpeed >= TapMinDownSpeed) || downDistance >= TapMinDownDelta * 1.6f)
            && Time.time - _lastTapAt >= TapCooldownSeconds;
        if (!canTap)
        {
            return false;
        }

        _lastTapAt = Time.time;
        _tapPressed = true;
        return true;
    }

    void ResetTap()
    {
        _tapArmed = false;
        _tapReady = false;
        _tapPressed = false;
        _hasLastTapPoint = false;
        _tapHoverStartAt = -99f;
    }

    void UpdateCursor()
    {
        if (_cursor == null || hand == null)
        {
            return;
        }

        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            return;
        }

        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.08f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.11f, 0.23f, hand.PinchOnlyStrength);

        int zone = HitTestGrabZone(hand.GripPoint);
        bool near = zone >= 0;
        Color color = _grabZone >= 0 ? _green : near ? _yellow : _blue;
        SetColor(_cursorRenderer, color);
    }

    void RefreshRegionColors()
    {
        ApplyRegionColor(ZoneLabel, _labelRenderers, _label);
        ApplyRegionColor(ZoneGauge, _gaugeRenderers, new Color(0.95f, 0.95f, 0.90f));
        ApplyRegionColor(ZonePin, _pinRenderers, _yellow);
        ApplyRegionColor(ZoneNozzle, _nozzleRenderers, _black);
        ApplyRegionColor(ZoneHandle, _handleRenderers, _black);
    }

    void ApplyRegionColor(int zone, Renderer[] renderers, Color baseColor)
    {
        if (renderers == null)
        {
            return;
        }

        Color color = baseColor;
        if (_completedSteps[zone] || _phase == TrainingPhase.Completed)
        {
            color = Color.Lerp(baseColor, _done, 0.55f);
        }
        else if (_phase == TrainingPhase.Training && zone == _stepIndex)
        {
            color = Color.Lerp(baseColor, _yellow, 0.50f + Mathf.PingPong(Time.time * 2.8f, 0.25f));
        }

        if (zone == _flashStep && Time.time < _flashUntil)
        {
            color = Color.Lerp(color, _flashColor, 0.75f);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            SetColor(renderers[i], color);
        }
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

    void UpdateStatusText()
    {
        if (_status == null)
        {
            return;
        }

        string handState = hand != null && hand.IsActive ? "手势已连接" : "等待手势识别";
        _status.text =
            "状态：" + PhaseName() + "    步骤 " + Mathf.Min(_stepIndex + 1, 5) + "/5" +
            "\n" + handState + "    " + CurrentHint();

        if (_feedbackText != null)
        {
            _feedbackText.text = _feedback;
            _feedbackText.color = Time.time < _flashUntil && _flashColor == _warning
                ? new Color(1f, 0.42f, 0.30f)
                : new Color(1f, 0.88f, 0.55f);
        }
    }

    string PhaseName()
    {
        switch (_phase)
        {
            case TrainingPhase.WaitingStart:
                return "点击启动按钮";
            case TrainingPhase.Training:
                return _stepIndex < _stepNames.Length ? _stepNames[_stepIndex] : "点检完成";
            case TrainingPhase.Completed:
                return "训练完成";
            default:
                return "准备中";
        }
    }

    string CurrentHint()
    {
        if (_phase == TrainingPhase.WaitingStart)
        {
            return "点击启动按钮开始灭火器点检";
        }

        if (_phase == TrainingPhase.Completed)
        {
            return "点检完成，可重新训练或返回";
        }

        return _stepIndex >= 0 && _stepIndex < _stepHints.Length ? _stepHints[_stepIndex] : "";
    }

    ExtinguisherStep CurrentStep()
    {
        return (ExtinguisherStep)Mathf.Clamp(_stepIndex, 0, 4);
    }

    float Progress01()
    {
        return Mathf.Clamp01(CompletedStepCount() / 5f);
    }

    int CompletedStepCount()
    {
        int count = 0;
        for (int i = 0; i < _completedSteps.Length; i++)
        {
            if (_completedSteps[i])
            {
                count++;
            }
        }

        return count;
    }

    void ResetMovableParts()
    {
        if (_pinVisual != null)
        {
            _pinVisual.localPosition = new Vector3(-0.50f, 0.93f, -0.60f);
        }

        if (_nozzleVisual != null)
        {
            _nozzleVisual.localPosition = Vector3.zero;
        }

        if (_handleVisual != null)
        {
            _handleVisual.localPosition = new Vector3(0f, 1.15f, -0.56f);
        }
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

    Vector3 TapPoint()
    {
        return hand != null && hand.Points != null && hand.Points.Length > 8 ? hand.Points[8] : hand.GripPoint;
    }

    bool IsInsideRect(Vector3 point, Vector3 center, Vector2 size)
    {
        return Mathf.Abs(point.x - center.x) <= size.x * 0.5f
            && Mathf.Abs(point.y - center.y) <= size.y * 0.5f;
    }

    bool IsInsideCircle(Vector3 point, Vector3 center, float radius)
    {
        Vector2 delta = new Vector2(point.x - center.x, point.y - center.y);
        return delta.sqrMagnitude <= radius * radius;
    }

    Transform CreateTarget(string name, Vector3 localPosition)
    {
        GameObject target = new GameObject(name);
        target.transform.parent = _stageRoot;
        target.transform.localPosition = localPosition;
        return target.transform;
    }

    GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.parent = parent;
        box.transform.localPosition = localPosition;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = localScale;
        Destroy(box.GetComponent<Collider>());
        SetColor(box.GetComponent<Renderer>(), color);
        return box;
    }

    enum Axis
    {
        X,
        Y,
        Z
    }

    GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition, float diameter, float length, Color color, Axis axis)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.parent = parent;
        cylinder.transform.localPosition = localPosition;
        cylinder.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);
        if (axis == Axis.X)
        {
            cylinder.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else if (axis == Axis.Z)
        {
            cylinder.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            cylinder.transform.localRotation = Quaternion.identity;
        }

        Destroy(cylinder.GetComponent<Collider>());
        SetColor(cylinder.GetComponent<Renderer>(), color);
        return cylinder;
    }

    GameObject CreateCylinderBetween(Transform parent, string name, Vector3 start, Vector3 end, float diameter, Color color)
    {
        Vector3 mid = (start + end) * 0.5f;
        Vector3 direction = end - start;
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.parent = parent;
        cylinder.transform.localPosition = mid;
        cylinder.transform.localRotation = direction.sqrMagnitude > 0.0001f
            ? Quaternion.FromToRotation(Vector3.up, direction.normalized)
            : Quaternion.identity;
        cylinder.transform.localScale = new Vector3(diameter, direction.magnitude * 0.5f, diameter);
        Destroy(cylinder.GetComponent<Collider>());
        SetColor(cylinder.GetComponent<Renderer>(), color);
        return cylinder;
    }

    GameObject CreateGaugeArc(Transform parent, string name, Vector3 localPosition, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = parent;
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.material = MakeMaterial(color);

        int segments = 24;
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(startAngleDeg, endAngleDeg, t) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            int index = i * 2;
            vertices[index] = new Vector3(cos * innerRadius, sin * innerRadius, 0f);
            vertices[index + 1] = new Vector3(cos * outerRadius, sin * outerRadius, 0f);
            normals[index] = Vector3.back;
            normals[index + 1] = Vector3.back;
            uvs[index] = new Vector2(t, 0f);
            uvs[index + 1] = new Vector2(t, 1f);
        }

        for (int i = 0; i < segments; i++)
        {
            int v = i * 2;
            int tri = i * 6;
            triangles[tri] = v;
            triangles[tri + 1] = v + 2;
            triangles[tri + 2] = v + 1;
            triangles[tri + 3] = v + 2;
            triangles[tri + 4] = v + 3;
            triangles[tri + 5] = v + 1;
        }

        Mesh mesh = new Mesh();
        mesh.name = name + "_Mesh";
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;

        return obj;
    }

    TextMesh CreateText(Transform parent, string name, string text, Vector3 localPosition, float characterSize, Color color, TextAnchor anchor)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent;
        go.transform.localPosition = localPosition;
        TextMesh mesh = go.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.anchor = anchor;
        mesh.alignment = TextAlignment.Center;
        mesh.fontSize = 42;
        mesh.characterSize = characterSize;
        mesh.color = color;
        return mesh;
    }

    Renderer[] RenderersOf(params GameObject[] objects)
    {
        int count = 0;
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null && objects[i].GetComponent<Renderer>() != null)
            {
                count++;
            }
        }

        Renderer[] renderers = new Renderer[count];
        int index = 0;
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
            {
                continue;
            }

            Renderer renderer = objects[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderers[index++] = renderer;
            }
        }

        return renderers;
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
    }

    static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
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
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.34f);
        }
        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", 0.34f);
        }

        return material;
    }
}
