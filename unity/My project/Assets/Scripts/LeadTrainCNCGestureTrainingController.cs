using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LeadTrainCNCGestureTrainingController : MonoBehaviour
{
    public const string TaskId = "lead_train1_cnc_gesture";

    public HandInput hand;
    public HandVisual handVisual;

    enum TrainingPhase
    {
        WaitingStart,
        Training,
        Completed
    }

    enum CNCAction
    {
        PowerTap,
        ModeRotate,
        DoorSlide,
        ClampRotate,
        StartTap,
        EmergencyStopTap,
        ResetTap
    }

    readonly string[] _stepNames =
    {
        "按下电源按钮",
        "确认自动模式",
        "打开安全门",
        "夹紧工件",
        "关闭安全门",
        "按下启动按钮",
        "按下急停按钮",
        "按下复位按钮"
    };

    readonly string[] _stepHints =
    {
        "用食指点按 POWER 电源按钮上电",
        "捏住模式旋钮并旋转，最终停在 AUTO",
        "捏住安全门把手，水平滑动打开安全门",
        "捏住夹具手柄旋转，夹紧工件",
        "捏住安全门把手，水平滑动关闭安全门",
        "用食指点按 CYCLE START 启动自动加工",
        "点按红色急停按钮，让运行中的机床停止",
        "点按 RESET 复位急停，回到就绪状态"
    };

    readonly string[] _expectedActions =
    {
        "PointTap: POWER",
        "GrabRotate: AUTO mode knob",
        "GrabSlide: open safety door",
        "GrabRotate: clamp workpiece",
        "GrabSlide: close safety door",
        "PointTap: CYCLE START",
        "PointTap: EMERGENCY STOP",
        "PointTap: RESET"
    };

    const int ZonePower = 0;
    const int ZoneMode = 1;
    const int ZoneDoor = 2;
    const int ZoneClamp = 3;
    const int ZoneStart = 4;
    const int ZoneEStop = 5;
    const int ZoneReset = 6;
    const int ZoneCount = 7;

    const float GestureCameraHeight = 0.34f;
    const float TextSizeScale = 0.72f;
    const float ButtonPressDistance = 0.050f;
    const float ButtonPressSeconds = 0.18f;
    const float TapHoverWidthFactor = 1.12f;
    const float TapHoverHeightFactor = 1.18f;
    const float TapClickWidthFactor = 0.90f;
    const float TapClickHeightFactor = 0.88f;
    const float TapReadySeconds = 0.11f;
    const float TapMinDownDelta = 0.028f;
    const float TapMinDownSpeed = 0.18f;
    const float TapMaxSideOffsetFactor = 0.62f;
    const float TapStabilizeMaxDrift = 0.060f;
    const float TapCooldownSeconds = 0.45f;
    const float MistakeCooldownSeconds = 0.62f;
    const int StepHintMistakeThreshold = 2;

    TrainingPhase _phase = TrainingPhase.WaitingStart;
    Transform _stageRoot;
    TextMesh _screenText;
    TextMesh _modeValueText;
    FingertipTapButton _startTrainingButton;
    FingertipTapButton _resetTrainingButton;
    GameObject _cursor;
    Renderer _cursorRenderer;

    readonly Transform[] _targets = new Transform[ZoneCount];
    readonly Renderer[][] _zoneRenderers = new Renderer[ZoneCount][];
    readonly Transform[][] _buttonPressVisuals = new Transform[ZoneCount][];
    readonly Vector3[][] _buttonPressRestPositions = new Vector3[ZoneCount][];
    readonly float[] _buttonPressUntil = new float[ZoneCount];
    readonly bool[] _completedSteps = new bool[8];
    readonly int[] _mistakesByStep = new int[8];
    TrainingReportRecorder _reportRecorder;

    Transform _leftDoor;
    Transform _rightDoor;
    Transform _clampLever;
    Transform _clampJaw;
    Transform _modePointer;
    Renderer _screenRenderer;
    Renderer _workpieceRenderer;
    Renderer _towerGreen;
    Renderer _towerYellow;
    Renderer _towerRed;
    Renderer _powerLamp;
    Renderer _autoLamp;

    int _stepIndex;
    bool _initialized;
    bool _powerOn;
    bool _autoMode;
    bool _doorOpen;
    bool _workpieceClamped;
    bool _running;
    bool _emergencyStopped;
    bool _everManualAfterAuto;
    string _feedback = "点击启动后开始 CNC 8 步手势训练";

    float _lastMistakeAt = -99f;
    float _flashUntil;
    int _flashZone = -1;
    Color _flashColor = Color.white;

    bool _tapArmed;
    bool _tapReady;
    bool _tapPressed;
    bool _hasLastTapPoint;
    Vector3 _lastTapPoint;
    float _tapHoverStartAt = -99f;
    float _lastTapAt = -99f;
    int _tapZone = -1;

    int _grabZone = -1;
    Vector3 _grabStart;
    float _grabStartAngle;
    bool _grabActionFired;

    readonly Color _panelDark = new Color(0.11f, 0.13f, 0.15f);
    readonly Color _panelMid = new Color(0.20f, 0.23f, 0.25f);
    readonly Color _metal = new Color(0.50f, 0.53f, 0.54f);
    readonly Color _darkMetal = new Color(0.08f, 0.09f, 0.10f);
    readonly Color _glass = new Color(0.18f, 0.43f, 0.68f);
    readonly Color _green = new Color(0.16f, 0.88f, 0.36f);
    readonly Color _yellow = new Color(1f, 0.76f, 0.12f);
    readonly Color _red = new Color(0.90f, 0.08f, 0.05f);
    readonly Color _blue = new Color(0.18f, 0.58f, 1f);
    readonly Color _orange = new Color(1f, 0.42f, 0.12f);
    readonly Color _done = new Color(0.22f, 0.92f, 0.46f);

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
        BuildTrainingButtons();
        BuildTexts();
        TrainingFlowController.EnsureExists(TaskId);
        RestartTraining(false);
        EnsureReturnInput();
    }

    public void RestartTraining(bool resetTrainingFlow = true)
    {
        _phase = TrainingPhase.WaitingStart;
        _stepIndex = 0;
        _powerOn = false;
        _autoMode = false;
        _doorOpen = false;
        _workpieceClamped = false;
        _running = false;
        _emergencyStopped = false;
        _everManualAfterAuto = false;
        _feedback = "点击启动后开始 CNC 8 步手势训练";
        _flashZone = -1;
        _grabZone = -1;
        _grabActionFired = false;
        ResetTap();
        ResetButtonPressTimers();

        for (int i = 0; i < _completedSteps.Length; i++)
        {
            _completedSteps[i] = false;
            _mistakesByStep[i] = 0;
        }

        _reportRecorder = new TrainingReportRecorder();
        _reportRecorder.Begin(TaskId, SceneManager.GetActiveScene().name, _stepNames, _expectedActions);

        if (resetTrainingFlow)
        {
            TrainingFlowController.EnsureExists(TaskId);
        }

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

        UpdateCursor();
        RefreshButtons();
        RefreshVisuals();
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
            GameObject camGo = new GameObject("LeadTrainCNCGestureCamera");
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
        }

        cam.transform.SetParent(null, true);
        cam.transform.position = new Vector3(0f, GestureCameraHeight, -6.75f);
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

        GameObject keyGo = new GameObject("LeadCNC_KeyLight");
        Light key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 1.05f;
        key.color = new Color(1f, 0.96f, 0.88f);
        keyGo.transform.rotation = Quaternion.Euler(38f, -32f, 0f);

        GameObject fillGo = new GameObject("LeadCNC_FillLight");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.intensity = 1.55f;
        fill.range = 8.5f;
        fill.color = new Color(0.48f, 0.72f, 1f);
        fillGo.transform.position = new Vector3(-2.8f, 2.5f, -2.2f);
    }

    void CreateVirtualHand()
    {
        if (hand != null)
        {
            handVisual = hand.GetComponent<HandVisual>();
            return;
        }

        GameObject handGo = new GameObject("LeadTrainCNCGestureHand");
        hand = handGo.AddComponent<HandInput>();
        hand.url = "ws://127.0.0.1:8765";
        hand.planeWidth = 6.35f;
        hand.planeHeight = 4.05f;
        hand.planeOrigin = new Vector3(0f, 0f, -0.58f);
        hand.gain = 1.24f;
        hand.smoothing = 0.66f;
        hand.graceTime = 0.45f;

        handVisual = handGo.AddComponent<HandVisual>();
        handVisual.jointRadius = 0.025f;
        handVisual.skinColor = new Color(0.96f, 0.78f, 0.62f);
        handVisual.gripColor = new Color(0.20f, 0.92f, 0.48f);
        handVisual.enablePhysicalColliders = false;
    }

    void BuildStage()
    {
        GameObject root = new GameObject("LeadTrainCNCGestureStage");
        root.transform.parent = transform;
        root.transform.localPosition = Vector3.zero;
        _stageRoot = root.transform;

        CreateBox(_stageRoot, "TrainingBackdrop", new Vector3(0f, 0.02f, 0.46f), new Vector3(8.35f, 4.35f, 0.18f), new Color(0.09f, 0.11f, 0.13f));
        CreateBox(_stageRoot, "WorkbenchDeck", new Vector3(0f, -1.80f, 0.08f), new Vector3(7.55f, 0.24f, 0.58f), new Color(0.20f, 0.22f, 0.24f));

        BuildMachineBody();
        BuildControlPanel();
    }

    void BuildMachineBody()
    {
        CreateBox(_stageRoot, "CNCMachineFrame", new Vector3(-1.35f, -0.18f, 0.04f), new Vector3(3.55f, 2.70f, 0.18f), new Color(0.60f, 0.64f, 0.66f));
        CreateBox(_stageRoot, "CNCMachineInnerCavity", new Vector3(-1.35f, -0.24f, -0.08f), new Vector3(2.55f, 1.72f, 0.12f), _darkMetal);
        CreateBox(_stageRoot, "MachineTopRail", new Vector3(-1.35f, 1.20f, -0.10f), new Vector3(3.70f, 0.12f, 0.13f), _metal);
        CreateBox(_stageRoot, "MachineBottomRail", new Vector3(-1.35f, -1.57f, -0.10f), new Vector3(3.70f, 0.12f, 0.13f), _metal);

        GameObject leftDoor = CreateBox(_stageRoot, "SafetyDoor_Left", new Vector3(-1.86f, -0.23f, -0.30f), new Vector3(1.12f, 1.56f, 0.08f), new Color(0.16f, 0.34f, 0.52f));
        GameObject rightDoor = CreateBox(_stageRoot, "SafetyDoor_Right", new Vector3(-0.84f, -0.23f, -0.30f), new Vector3(1.12f, 1.56f, 0.08f), new Color(0.16f, 0.34f, 0.52f));
        _leftDoor = leftDoor.transform;
        _rightDoor = rightDoor.transform;
        _zoneRenderers[ZoneDoor] = RenderersOf(leftDoor, rightDoor);

        CreateBox(_leftDoor, "LeftDoorGlass", new Vector3(0f, 0f, -0.08f), new Vector3(0.84f, 1.12f, 0.04f), _glass);
        CreateBox(_rightDoor, "RightDoorGlass", new Vector3(0f, 0f, -0.08f), new Vector3(0.84f, 1.12f, 0.04f), _glass);
        CreateBox(_rightDoor, "SafetyDoorHandle", new Vector3(-0.42f, 0f, -0.17f), new Vector3(0.10f, 0.78f, 0.09f), _yellow);
        _targets[ZoneDoor] = CreateTarget("DoorSlideTarget", new Vector3(-1.27f, -0.22f, -0.66f));

        CreateBox(_stageRoot, "MachineBed", new Vector3(-1.35f, -1.04f, -0.38f), new Vector3(2.10f, 0.18f, 0.35f), _metal);
        GameObject workpiece = CreateBox(_stageRoot, "Workpiece", new Vector3(-1.32f, -0.78f, -0.52f), new Vector3(0.66f, 0.30f, 0.22f), new Color(0.74f, 0.76f, 0.70f));
        _workpieceRenderer = workpiece.GetComponent<Renderer>();
        GameObject clampBase = CreateBox(_stageRoot, "ClampBase", new Vector3(-1.98f, -0.80f, -0.50f), new Vector3(0.36f, 0.18f, 0.18f), _metal);
        GameObject jaw = CreateBox(_stageRoot, "ClampJaw", new Vector3(-1.58f, -0.78f, -0.54f), new Vector3(0.12f, 0.36f, 0.20f), _orange);
        _clampJaw = jaw.transform;

        GameObject leverPivot = new GameObject("ClampLeverPivot");
        leverPivot.transform.parent = _stageRoot;
        leverPivot.transform.localPosition = new Vector3(-2.20f, -0.61f, -0.58f);
        leverPivot.transform.localRotation = Quaternion.Euler(0f, 0f, -38f);
        _clampLever = leverPivot.transform;
        GameObject lever = CreateBox(_clampLever, "ClampLever", new Vector3(0.36f, 0f, 0f), new Vector3(0.78f, 0.08f, 0.10f), _orange);
        GameObject leverGrip = CreateCylinder(_clampLever, "ClampLeverGrip", new Vector3(0.75f, 0f, -0.02f), 0.18f, 0.28f, _darkMetal, Axis.Y);
        _zoneRenderers[ZoneClamp] = RenderersOf(clampBase, jaw, lever, leverGrip);
        _targets[ZoneClamp] = CreateTarget("ClampRotateTarget", new Vector3(-1.72f, -0.58f, -0.70f));

        CreateText(_stageRoot, "DoorLabel", "安全门", new Vector3(-2.64f, 0.72f, -0.34f), 0.042f, Color.white, TextAnchor.MiddleCenter, false);
        CreateText(_stageRoot, "ClampLabel", "夹具", new Vector3(-2.33f, -0.28f, -0.58f), 0.042f, Color.white, TextAnchor.MiddleCenter, false);
    }

    void BuildControlPanel()
    {
        CreateBox(_stageRoot, "ControlPanelBack", new Vector3(2.08f, -0.15f, 0.02f), new Vector3(2.28f, 2.86f, 0.18f), _panelMid);
        CreateBox(_stageRoot, "ControlPanelFace", new Vector3(2.08f, -0.15f, -0.12f), new Vector3(2.08f, 2.62f, 0.10f), _panelDark);
        CreateBox(_stageRoot, "ControlPanelTopLip", new Vector3(2.08f, 1.26f, -0.20f), new Vector3(2.24f, 0.08f, 0.13f), _metal);

        GameObject screen = CreateBox(_stageRoot, "CNCStatusScreen", new Vector3(2.08f, 0.79f, -0.26f), new Vector3(1.62f, 0.58f, 0.08f), new Color(0.02f, 0.09f, 0.11f));
        _screenRenderer = screen.GetComponent<Renderer>();
        _screenText = CreateText(_stageRoot, "CNCStatusScreenText", "", new Vector3(2.08f, 0.80f, -0.34f), 0.030f, new Color(0.78f, 1f, 0.88f), TextAnchor.MiddleCenter, false);
        _screenText.lineSpacing = 0.82f;

        BuildTowerLights();
        BuildPanelButton(ZonePower, "POWER\n电源", new Vector3(1.42f, 0.20f, -0.30f), new Vector3(0.52f, 0.34f, 0.10f), new Color(0.14f, 0.58f, 0.28f));
        BuildPanelButton(ZoneStart, "START\n启动", new Vector3(2.16f, 0.20f, -0.30f), new Vector3(0.52f, 0.34f, 0.10f), new Color(0.10f, 0.62f, 0.26f));
        BuildRoundPanelButton(ZoneEStop, "急停", new Vector3(2.74f, 0.18f, -0.34f), 0.44f, _red);
        BuildPanelButton(ZoneReset, "RESET\n复位", new Vector3(2.42f, -0.53f, -0.30f), new Vector3(0.78f, 0.40f, 0.10f), new Color(0.15f, 0.46f, 0.86f));
        BuildModeSelector();

        _powerLamp = CreateLamp(_stageRoot, "PowerLamp", new Vector3(1.42f, 0.50f, -0.34f), 0.095f);
        _autoLamp = CreateLamp(_stageRoot, "AutoLamp", new Vector3(1.74f, -0.18f, -0.34f), 0.075f);
        CreateText(_stageRoot, "ModeLabel", "MODE", new Vector3(1.74f, -0.84f, -0.34f), 0.034f, Color.white, TextAnchor.MiddleCenter, true);
        _modeValueText = CreateText(_stageRoot, "ModeValue", "MANUAL", new Vector3(1.74f, -1.08f, -0.34f), 0.036f, _yellow, TextAnchor.MiddleCenter, true);
    }

    void BuildTowerLights()
    {
        CreateBox(_stageRoot, "TowerStem", new Vector3(3.38f, 0.75f, -0.20f), new Vector3(0.06f, 0.86f, 0.06f), _metal);
        _towerRed = CreateLamp(_stageRoot, "TowerRed", new Vector3(3.38f, 1.28f, -0.30f), 0.16f);
        _towerYellow = CreateLamp(_stageRoot, "TowerYellow", new Vector3(3.38f, 1.05f, -0.30f), 0.16f);
        _towerGreen = CreateLamp(_stageRoot, "TowerGreen", new Vector3(3.38f, 0.82f, -0.30f), 0.16f);
    }

    void BuildModeSelector()
    {
        GameObject knob = CreateCylinder(_stageRoot, "ModeSelectorKnob", new Vector3(1.74f, -0.54f, -0.34f), 0.46f, 0.12f, new Color(0.42f, 0.44f, 0.45f), Axis.Z);
        GameObject pointerRoot = new GameObject("ModeSelectorPointerPivot");
        pointerRoot.transform.parent = _stageRoot;
        pointerRoot.transform.localPosition = new Vector3(1.74f, -0.54f, -0.42f);
        _modePointer = pointerRoot.transform;
        GameObject pointer = CreateBox(_modePointer, "ModeSelectorPointer", new Vector3(0f, 0.13f, 0f), new Vector3(0.08f, 0.28f, 0.05f), _yellow);
        _targets[ZoneMode] = CreateTarget("ModeRotateTarget", new Vector3(1.74f, -0.54f, -0.54f));
        _zoneRenderers[ZoneMode] = RenderersOf(knob, pointer);

        CreateText(_stageRoot, "ManualText", "MANUAL", new Vector3(1.28f, -0.46f, -0.34f), 0.030f, Color.white, TextAnchor.MiddleCenter, false);
        CreateText(_stageRoot, "AutoText", "AUTO", new Vector3(2.20f, -0.46f, -0.34f), 0.030f, Color.white, TextAnchor.MiddleCenter, false);
    }

    void BuildPanelButton(int zone, string label, Vector3 center, Vector3 size, Color color)
    {
        GameObject baseBox = CreateBox(_stageRoot, "CNCButtonBase_" + zone, center + new Vector3(0f, -0.04f, 0.045f), new Vector3(size.x * 1.10f, size.y * 0.40f, size.z), Color.Lerp(color, Color.black, 0.35f));
        GameObject button = CreateBox(_stageRoot, "CNCButtonCap_" + zone, center, size, color);
        _targets[zone] = CreateTarget("CNCTapTarget_" + zone, center + new Vector3(0f, 0f, -0.11f));
        _zoneRenderers[zone] = RenderersOf(baseBox, button);
        RegisterButtonPressVisuals(zone, button.transform);
        CreateText(_stageRoot, "CNCButtonLabel_" + zone, label, center + new Vector3(0f, -0.01f, -0.10f), 0.029f, Color.white, TextAnchor.MiddleCenter, true);
    }

    void BuildRoundPanelButton(int zone, string label, Vector3 center, float diameter, Color color)
    {
        GameObject baseDisc = CreateCylinder(_stageRoot, "CNCRoundButtonBase_" + zone, center + new Vector3(0f, -0.02f, 0.05f), diameter * 1.12f, 0.10f, Color.Lerp(color, Color.black, 0.35f), Axis.Z);
        GameObject button = CreateCylinder(_stageRoot, "CNCRoundButtonCap_" + zone, center, diameter, 0.12f, color, Axis.Z);
        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "CNCRoundButtonDome_" + zone;
        dome.transform.parent = _stageRoot;
        dome.transform.localPosition = center + new Vector3(0f, 0f, -0.08f);
        dome.transform.localScale = new Vector3(diameter * 0.82f, diameter * 0.82f, diameter * 0.28f);
        Destroy(dome.GetComponent<Collider>());
        SetColor(dome.GetComponent<Renderer>(), Color.Lerp(color, Color.white, 0.08f));

        _targets[zone] = CreateTarget("CNCTapTarget_" + zone, center + new Vector3(0f, 0f, -0.13f));
        _zoneRenderers[zone] = RenderersOf(baseDisc, button, dome);
        RegisterButtonPressVisuals(zone, button.transform, dome.transform);
        CreateText(_stageRoot, "CNCRoundButtonLabel_" + zone, label, center + new Vector3(0f, -0.42f, -0.08f), 0.032f, Color.white, TextAnchor.MiddleCenter, true);
    }

    void RegisterButtonPressVisuals(int zone, params Transform[] visuals)
    {
        if (zone < 0 || zone >= ZoneCount || visuals == null || visuals.Length == 0)
        {
            return;
        }

        _buttonPressVisuals[zone] = visuals;
        _buttonPressRestPositions[zone] = new Vector3[visuals.Length];
        for (int i = 0; i < visuals.Length; i++)
        {
            _buttonPressRestPositions[zone][i] = visuals[i] != null ? visuals[i].localPosition : Vector3.zero;
        }
    }

    void BuildTrainingButtons()
    {
        _startTrainingButton = CreateTrainingButton("StartTraining", new Vector3(-3.55f, -0.92f, -0.24f), new Vector3(0.72f, 0.36f, 0.10f), "START\n启动", new Color(0.10f, 0.62f, 0.26f));
        _resetTrainingButton = CreateTrainingButton("ResetTraining", new Vector3(3.58f, -0.92f, -0.24f), new Vector3(0.72f, 0.36f, 0.10f), "RESET\n重置", new Color(0.10f, 0.62f, 0.26f));

        _startTrainingButton.Clicked += HandleStartTrainingClicked;
        _resetTrainingButton.Clicked += () => RestartTraining(true);
        RefreshButtons();
    }

    FingertipTapButton CreateTrainingButton(string name, Vector3 center, Vector3 size, string label, Color color)
    {
        GameObject go = new GameObject("LeadCNCGestureButton_" + name);
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
        CreateText(_stageRoot, "LeadCNCGestureTitle", "CNC 8 步手势真实训练", new Vector3(0f, 2.05f, -0.18f), 0.052f, Color.white, TextAnchor.MiddleCenter, true);
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "LeadCNCGestureCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();
        SetColor(_cursorRenderer, _blue);
    }

    void HandleStartTrainingClicked()
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
        _feedback = "步骤 1/8：请先按下电源按钮";
        TrainingFlowController.Active?.ReportProgress(0f, "CNC 真实训练", CurrentHint());
    }

    void UpdateTapInteraction()
    {
        if (hand == null || !hand.IsActive)
        {
            ResetTap();
            return;
        }

        Vector3 point = TapPoint();
        int zone = HitTestTapZone(point, out bool inHover, out bool inClick);
        bool tap = UpdateTapState(point, zone, inHover, inClick, out int clickedZone);
        if (!tap || clickedZone < 0)
        {
            return;
        }

        TriggerButtonPress(clickedZone);

        if (clickedZone == ZonePower)
        {
            HandleCNCAction(CNCAction.PowerTap);
        }
        else if (clickedZone == ZoneStart)
        {
            HandleCNCAction(CNCAction.StartTap);
        }
        else if (clickedZone == ZoneEStop)
        {
            HandleCNCAction(CNCAction.EmergencyStopTap);
        }
        else if (clickedZone == ZoneReset)
        {
            HandleCNCAction(CNCAction.ResetTap);
        }
        else
        {
            RecordMistake("请使用正确手势：" + GestureHintForZone(clickedZone), clickedZone);
        }
    }

    void UpdateGrabInteraction()
    {
        if (hand == null || !hand.IsActive)
        {
            EndGrab();
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
            _grabStartAngle = hand.PalmAngle;
            _grabActionFired = false;
            HandleGrabStart(zone);
            return;
        }

        if (signal < 0.22f)
        {
            EndGrab();
            return;
        }

        UpdateGrabAction(grip);
    }

    void HandleGrabStart(int zone)
    {
        if (_phase != TrainingPhase.Training)
        {
            return;
        }

        if (zone == ZonePower)
        {
            RecordMistake("请用食指点按电源按钮，而非旋转", zone);
            _grabActionFired = true;
            return;
        }

        if (zone == ZoneStart || zone == ZoneEStop || zone == ZoneReset)
        {
            RecordMistake("按钮类操作请使用 Point + Tap 点按手势", zone);
            _grabActionFired = true;
            return;
        }

        _feedback = "已抓住" + ZoneName(zone) + "，请完成" + GestureHintForZone(zone);
    }

    void UpdateGrabAction(Vector3 grip)
    {
        if (_grabActionFired || _phase != TrainingPhase.Training)
        {
            return;
        }

        Vector3 delta = grip - _grabStart;
        float angleDelta = Mathf.Abs(Mathf.DeltaAngle(_grabStartAngle, hand.PalmAngle));

        if (_grabZone == ZoneMode)
        {
            AnimateModePreview(delta, angleDelta);
            if (angleDelta >= 26f || Mathf.Abs(delta.x) >= 0.34f || Mathf.Abs(delta.y) >= 0.32f)
            {
                _grabActionFired = true;
                HandleCNCAction(CNCAction.ModeRotate);
            }
        }
        else if (_grabZone == ZoneDoor)
        {
            AnimateDoorPreview(delta.x);
            if (Mathf.Abs(delta.x) >= 0.42f)
            {
                _grabActionFired = true;
                HandleCNCAction(CNCAction.DoorSlide);
            }
        }
        else if (_grabZone == ZoneClamp)
        {
            AnimateClampPreview(delta, angleDelta);
            if (angleDelta >= 22f || delta.magnitude >= 0.34f)
            {
                _grabActionFired = true;
                HandleCNCAction(CNCAction.ClampRotate);
            }
        }
    }

    void EndGrab()
    {
        _grabZone = -1;
        _grabActionFired = false;
    }

    void HandleCNCAction(CNCAction action)
    {
        if (_phase != TrainingPhase.Training)
        {
            return;
        }

        int actionZone = ZoneForAction(action);
        if (!PassSafetyGuard(action, out string safetyMessage))
        {
            RecordMistake(safetyMessage, actionZone);
            return;
        }

        if (HandlePowerToggleOutsideSequence(action))
        {
            return;
        }

        if (HandleModeChangeOutsideAutoStep(action))
        {
            return;
        }

        CNCAction expected = ExpectedAction();
        if (action != expected)
        {
            RecordMistake(WrongStepMessage(), actionZone);
            return;
        }

        ExecuteExpectedAction(action);
    }

    bool PassSafetyGuard(CNCAction action, out string message)
    {
        message = "";

        if (!_powerOn && action != CNCAction.PowerTap)
        {
            if (action == CNCAction.ModeRotate)
            {
                message = "上电后再确认加工模式";
            }
            else if (_stepIndex == 0)
            {
                message = "步骤 1/8：请先按下电源按钮";
            }
            else
            {
                message = "请先按下电源按钮上电";
            }
            return false;
        }

        if (_emergencyStopped && action != CNCAction.ResetTap)
        {
            message = action == CNCAction.PowerTap ? "请先复位后再操作电源" : "急停激活，请先复位";
            return false;
        }

        if (_running
            && (action == CNCAction.PowerTap
                || action == CNCAction.DoorSlide
                || action == CNCAction.ClampRotate
                || action == CNCAction.ModeRotate))
        {
            message = action == CNCAction.PowerTap ? "运行中禁止关电，请先急停" : "运行中禁止该操作";
            return false;
        }

        if (action == CNCAction.StartTap && _powerOn && !_autoMode)
        {
            message = "手动模式下禁止自动启动，请切回 AUTO";
            return false;
        }

        if (action == CNCAction.ResetTap && !_emergencyStopped && !_running)
        {
            message = "无需复位，请继续";
            return false;
        }

        return true;
    }

    bool HandlePowerToggleOutsideSequence(CNCAction action)
    {
        if (action != CNCAction.PowerTap || _stepIndex == 0)
        {
            return false;
        }

        if (_powerOn)
        {
            _powerOn = false;
            _running = false;
            _feedback = "电源已关闭，请重新上电";
            RecordMistake(_feedback, ZonePower);
            return true;
        }

        _powerOn = true;
        _feedback = "已重新上电，请继续：" + CurrentHint();
        FlashZone(ZonePower, _done, 0.32f);
        return true;
    }

    bool HandleModeChangeOutsideAutoStep(CNCAction action)
    {
        if (action != CNCAction.ModeRotate || _stepIndex <= 1)
        {
            return false;
        }

        ToggleMode();
        if (_autoMode)
        {
            _feedback = "已切回 AUTO，可继续当前步骤";
            FlashZone(ZoneMode, _done, 0.32f);
        }
        else
        {
            _everManualAfterAuto = true;
            RecordMistake("已改为手动模式，启动将被锁定，请切回 AUTO", ZoneMode);
        }

        return true;
    }

    CNCAction ExpectedAction()
    {
        switch (_stepIndex)
        {
            case 0: return CNCAction.PowerTap;
            case 1: return CNCAction.ModeRotate;
            case 2: return CNCAction.DoorSlide;
            case 3: return CNCAction.ClampRotate;
            case 4: return CNCAction.DoorSlide;
            case 5: return CNCAction.StartTap;
            case 6: return CNCAction.EmergencyStopTap;
            case 7: return CNCAction.ResetTap;
            default: return CNCAction.ResetTap;
        }
    }

    void ExecuteExpectedAction(CNCAction action)
    {
        switch (action)
        {
            case CNCAction.PowerTap:
                _powerOn = true;
                CompleteCurrentStep("已上电，请确认加工模式");
                break;
            case CNCAction.ModeRotate:
                ToggleMode();
                if (_autoMode)
                {
                    CompleteCurrentStep("AUTO 模式确认完成");
                }
                else
                {
                    RecordMistake("请切回自动模式后再继续", ZoneMode);
                }
                break;
            case CNCAction.DoorSlide:
                if (_stepIndex == 2)
                {
                    _doorOpen = true;
                    CompleteCurrentStep("安全门已打开，可以装夹工件");
                }
                else if (_stepIndex == 4)
                {
                    _doorOpen = false;
                    CompleteCurrentStep("安全门已关闭，可以启动");
                }
                break;
            case CNCAction.ClampRotate:
                _workpieceClamped = true;
                CompleteCurrentStep("工件已夹紧");
                break;
            case CNCAction.StartTap:
                if (_doorOpen)
                {
                    RecordMistake("安全门未关闭，禁止启动", ZoneStart);
                    return;
                }
                if (!_workpieceClamped)
                {
                    RecordMistake("工件未夹紧，禁止启动", ZoneStart);
                    return;
                }

                _running = true;
                CompleteCurrentStep("自动加工已启动");
                break;
            case CNCAction.EmergencyStopTap:
                _running = false;
                _emergencyStopped = true;
                CompleteCurrentStep("急停已触发，请复位");
                break;
            case CNCAction.ResetTap:
                _emergencyStopped = false;
                _running = false;
                CompleteCurrentStep("急停已复位，机床回到就绪状态");
                break;
        }
    }

    void CompleteCurrentStep(string message)
    {
        if (_stepIndex < 0 || _stepIndex >= _completedSteps.Length)
        {
            return;
        }

        int completedIndex = _stepIndex;
        _completedSteps[completedIndex] = true;
        _reportRecorder?.MarkStepCompleted(completedIndex);
        _feedback = message;
        FlashZone(ZoneForStep(completedIndex), _done, 0.36f);
        TrainingFlowController.Active?.RecordSuccess(_stepNames[completedIndex] + "完成：" + message, false);

        _stepIndex++;
        if (_stepIndex >= _completedSteps.Length)
        {
            _phase = TrainingPhase.Completed;
            _feedback = _everManualAfterAuto
                ? "CNC 8 步训练完成；注意：训练中曾切到 MANUAL"
                : "CNC 8 步训练完成，机床处于 AUTO 就绪状态";
            if (_reportRecorder != null)
            {
                float targetSeconds = TrainingFlowController.Active != null ? TrainingFlowController.Active.targetSeconds : 180f;
                int score = _reportRecorder.Complete(targetSeconds);
                TrainingFlowController.Active?.AttachReportResult(score, _reportRecorder.LastSavedPath, _reportRecorder.ErrorSummary);
            }
            TrainingFlowController.Active?.CompleteTraining(_feedback);
        }
    }

    void ToggleMode()
    {
        _autoMode = !_autoMode;
    }

    void RecordMistake(string message, int zone = -1)
    {
        if (Time.time - _lastMistakeAt < MistakeCooldownSeconds)
        {
            return;
        }

        _lastMistakeAt = Time.time;
        _feedback = message;
        RegisterStepMistake();
        _reportRecorder?.RecordError(
            _stepIndex,
            message,
            ConsequenceForMistake(message),
            SeverityForMistake(message));
        if (ShouldShowCurrentStepHint())
        {
            FlashZone(ZoneForStep(_stepIndex), _orange, 0.42f);
        }
        TrainingFlowController.Active?.RecordMistake(message);
    }

    string SeverityForMistake(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "normal";
        }

        if (message.Contains("安全门未关闭")
            || message.Contains("工件未夹紧")
            || message.Contains("手动模式")
            || message.Contains("MANUAL")
            || message.Contains("启动将被锁定"))
        {
            return "safety";
        }

        return "normal";
    }

    string ConsequenceForMistake(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "当前动作无效，需要回到正确步骤继续。";
        }

        if (message.Contains("安全门未关闭"))
        {
            return "启动被锁定；真实场景存在飞溅和夹伤风险。";
        }

        if (message.Contains("工件未夹紧"))
        {
            return "加工可能导致工件移位、刀具损坏。";
        }

        if (message.Contains("手动模式") || message.Contains("MANUAL") || message.Contains("启动将被锁定"))
        {
            return "自动加工链路中断，需要切回 AUTO。";
        }

        if (message.Contains("步骤") || message.Contains("请先"))
        {
            return "前置条件未满足，当前动作无效。";
        }

        if (message.Contains("正确手势") || message.Contains("点按") || message.Contains("旋转"))
        {
            return "手势类型不匹配，设备不会响应该操作。";
        }

        return "当前动作无效，需要按提示恢复正确流程。";
    }

    void RegisterStepMistake()
    {
        if (_stepIndex >= 0 && _stepIndex < _mistakesByStep.Length)
        {
            _mistakesByStep[_stepIndex]++;
        }
    }

    void FlashZone(int zone, Color color, float duration)
    {
        _flashZone = zone;
        _flashColor = color;
        _flashUntil = Time.time + duration;
    }

    string WrongStepMessage()
    {
        if (_stepIndex == 0)
        {
            return "步骤 1/8：请先按下电源按钮";
        }

        if (_stepIndex == 1)
        {
            return "步骤 2/8：请确认自动模式";
        }

        return "步骤 " + (_stepIndex + 1) + "/8：请先" + _stepNames[_stepIndex];
    }

    int HitTestTapZone(Vector3 point, out bool inHover, out bool inClick)
    {
        inHover = false;
        inClick = false;

        int[] priority = (_emergencyStopped || _stepIndex == 7)
            ? new[] { ZoneReset, ZonePower, ZoneStart, ZoneEStop, ZoneMode, ZoneDoor, ZoneClamp }
            : new[] { ZonePower, ZoneStart, ZoneEStop, ZoneReset, ZoneMode, ZoneDoor, ZoneClamp };

        for (int i = 0; i < priority.Length; i++)
        {
            int zone = priority[i];
            Vector2 baseSize = TapBaseSizeForZone(zone);
            Vector2 hoverSize = new Vector2(baseSize.x * TapHoverWidthFactor, baseSize.y * TapHoverHeightFactor);
            Vector2 clickSize = new Vector2(baseSize.x * TapClickWidthFactor, baseSize.y * TapClickHeightFactor);
            Vector3 center = TargetPosition(zone);

            if (!IsInsideRect(point, center, hoverSize))
            {
                continue;
            }

            inHover = true;
            inClick = IsInsideRect(point, center, clickSize);
            return zone;
        }

        return -1;
    }

    Vector2 TapBaseSizeForZone(int zone)
    {
        switch (zone)
        {
            case ZonePower:
                return new Vector2(0.86f, 0.62f);
            case ZoneStart:
                return new Vector2(0.86f, 0.62f);
            case ZoneEStop:
                return new Vector2(0.94f, 0.94f);
            case ZoneReset:
                return (_emergencyStopped || _stepIndex == 7)
                    ? new Vector2(1.22f, 0.86f)
                    : new Vector2(1.12f, 0.78f);
            case ZoneMode:
                return new Vector2(0.80f, 0.80f);
            case ZoneDoor:
                return new Vector2(1.15f, 1.30f);
            case ZoneClamp:
                return new Vector2(0.88f, 0.72f);
            default:
                return new Vector2(0.6f, 0.6f);
        }
    }

    int HitTestGrabZone(Vector3 grip)
    {
        if (IsInsideCircle(grip, TargetPosition(ZoneMode), 0.46f)) return ZoneMode;
        if (IsInsideRect(grip, TargetPosition(ZoneDoor), new Vector2(1.20f, 1.34f))) return ZoneDoor;
        if (IsInsideRect(grip, TargetPosition(ZoneClamp), new Vector2(0.96f, 0.78f))) return ZoneClamp;
        if (IsInsideRect(grip, TargetPosition(ZonePower), new Vector2(0.70f, 0.52f))) return ZonePower;
        if (IsInsideCircle(grip, TargetPosition(ZoneStart), 0.38f)) return ZoneStart;
        if (IsInsideCircle(grip, TargetPosition(ZoneEStop), 0.38f)) return ZoneEStop;
        if (IsInsideRect(grip, TargetPosition(ZoneReset), new Vector2(1.02f, 0.70f))) return ZoneReset;
        return -1;
    }

    bool UpdateTapState(Vector3 point, int zone, bool inHover, bool inClick, out int clickedZone)
    {
        clickedZone = -1;
        if (zone < 0 || !inHover)
        {
            ResetTap();
            return false;
        }

        if (!_tapArmed || _tapZone != zone)
        {
            _tapArmed = true;
            _tapZone = zone;
            _tapReady = false;
            _tapPressed = false;
            _hasLastTapPoint = true;
            _lastTapPoint = point;
            _tapHoverStartAt = Time.time;
            return false;
        }

        if (!inClick)
        {
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
                Vector3 settleDelta = point - _lastTapPoint;
                if (Mathf.Abs(settleDelta.x) > TapStabilizeMaxDrift || Mathf.Abs(settleDelta.y) > TapStabilizeMaxDrift)
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
        float sideOffset = Mathf.Abs(point.x - _lastTapPoint.x);
        if (_tapPressed)
        {
            if (downDistance <= TapMinDownDelta * 0.25f)
            {
                _tapPressed = false;
            }
            return false;
        }

        bool canTap = ((downDistance >= TapMinDownDelta && downSpeed >= TapMinDownSpeed) || downDistance >= TapMinDownDelta * 1.6f)
            && sideOffset <= TapBaseSizeForZone(_tapZone).x * 0.5f * TapMaxSideOffsetFactor
            && Time.time - _lastTapAt >= TapCooldownSeconds;
        if (!canTap)
        {
            return false;
        }

        _lastTapAt = Time.time;
        _tapPressed = true;
        clickedZone = _tapZone;
        return true;
    }

    void ResetTap()
    {
        _tapArmed = false;
        _tapReady = false;
        _tapPressed = false;
        _hasLastTapPoint = false;
        _tapHoverStartAt = -99f;
        _tapZone = -1;
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
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.075f, 0.16f, hand.PinchOnlyStrength);

        int zone = HitTestGrabZone(hand.GripPoint);
        Color color = _grabZone >= 0 ? _green : zone >= 0 ? _yellow : _blue;
        SetColor(_cursorRenderer, color);
    }

    void RefreshButtons()
    {
        if (_startTrainingButton != null)
        {
            _startTrainingButton.interactable = _phase == TrainingPhase.WaitingStart || _phase == TrainingPhase.Completed;
        }

        if (_resetTrainingButton != null)
        {
            _resetTrainingButton.interactable = true;
        }
    }

    void RefreshVisuals()
    {
        RefreshMechanicalState();
        RefreshZoneColors();
        RefreshStatusScreen();
        RefreshTowerLights();
    }

    void RefreshMechanicalState()
    {
        float door = _doorOpen ? 1f : 0f;
        if (_leftDoor != null)
        {
            _leftDoor.localPosition = Vector3.Lerp(new Vector3(-1.86f, -0.23f, -0.30f), new Vector3(-2.27f, -0.23f, -0.30f), door);
        }
        if (_rightDoor != null)
        {
            _rightDoor.localPosition = Vector3.Lerp(new Vector3(-0.84f, -0.23f, -0.30f), new Vector3(-0.43f, -0.23f, -0.30f), door);
        }

        if (_clampLever != null)
        {
            _clampLever.localRotation = Quaternion.Euler(0f, 0f, _workpieceClamped ? 30f : -38f);
        }
        if (_clampJaw != null)
        {
            _clampJaw.localPosition = _workpieceClamped
                ? new Vector3(-1.45f, -0.78f, -0.54f)
                : new Vector3(-1.58f, -0.78f, -0.54f);
        }

        if (_modePointer != null)
        {
            _modePointer.localRotation = Quaternion.Euler(0f, 0f, _autoMode ? -48f : 48f);
        }

        SetButtonPress(ZonePower, false);
        SetButtonPress(ZoneStart, false);
        SetButtonPress(ZoneEStop, _emergencyStopped);
        SetButtonPress(ZoneReset, false);
    }

    void AnimateModePreview(Vector3 delta, float angleDelta)
    {
        if (_modePointer == null)
        {
            return;
        }

        float preview = Mathf.Clamp(delta.x * 65f + Mathf.Sign(delta.y) * angleDelta * 0.35f, -30f, 30f);
        float baseAngle = _autoMode ? -48f : 48f;
        _modePointer.localRotation = Quaternion.Euler(0f, 0f, baseAngle + preview);
    }

    void AnimateDoorPreview(float deltaX)
    {
        if (_leftDoor == null || _rightDoor == null)
        {
            return;
        }

        float preview = Mathf.Clamp01(Mathf.Abs(deltaX) / 0.42f) * 0.28f;
        float sign = _doorOpen ? -1f : 1f;
        _leftDoor.localPosition += new Vector3(-preview * sign * Time.deltaTime * 5f, 0f, 0f);
        _rightDoor.localPosition += new Vector3(preview * sign * Time.deltaTime * 5f, 0f, 0f);
    }

    void AnimateClampPreview(Vector3 delta, float angleDelta)
    {
        if (_clampLever == null)
        {
            return;
        }

        float preview = Mathf.Clamp(delta.x * 70f + angleDelta * 0.28f, -24f, 24f);
        float baseAngle = _workpieceClamped ? 30f : -38f;
        _clampLever.localRotation = Quaternion.Euler(0f, 0f, baseAngle + preview);
    }

    void TriggerButtonPress(int zone)
    {
        if (zone < 0 || zone >= ZoneCount || _buttonPressVisuals[zone] == null)
        {
            return;
        }

        _buttonPressUntil[zone] = Mathf.Max(_buttonPressUntil[zone], Time.time + ButtonPressSeconds);
    }

    void ResetButtonPressTimers()
    {
        for (int i = 0; i < _buttonPressUntil.Length; i++)
        {
            _buttonPressUntil[i] = 0f;
        }
    }

    void SetButtonPress(int zone, bool heldPressed)
    {
        if (zone < 0 || zone >= ZoneCount)
        {
            return;
        }

        Transform[] visuals = _buttonPressVisuals[zone];
        Vector3[] restPositions = _buttonPressRestPositions[zone];
        if (visuals == null || restPositions == null)
        {
            return;
        }

        bool pressed = heldPressed || Time.time < _buttonPressUntil[zone];
        for (int i = 0; i < visuals.Length; i++)
        {
            Transform visual = visuals[i];
            if (visual == null)
            {
                continue;
            }

            Vector3 local = restPositions[i];
            if (pressed)
            {
                local.z += ButtonPressDistance;
            }

            visual.localPosition = local;
        }
    }

    void RefreshZoneColors()
    {
        ApplyZoneColor(ZonePower, _powerOn ? _green : new Color(0.14f, 0.58f, 0.28f));
        ApplyZoneColor(ZoneMode, _autoMode ? _green : _yellow);
        ApplyZoneColor(ZoneDoor, _doorOpen ? _blue : _glass);
        ApplyZoneColor(ZoneClamp, _workpieceClamped ? _done : _orange);
        ApplyZoneColor(ZoneStart, _running ? _green : new Color(0.10f, 0.62f, 0.26f));
        ApplyZoneColor(ZoneEStop, _emergencyStopped ? _red : new Color(0.88f, 0.08f, 0.05f));
        ApplyZoneColor(ZoneReset, new Color(0.15f, 0.46f, 0.86f));

        SetColor(_workpieceRenderer, _workpieceClamped ? Color.Lerp(new Color(0.74f, 0.76f, 0.70f), _done, 0.45f) : new Color(0.74f, 0.76f, 0.70f));
        SetColor(_powerLamp, _powerOn ? _green : new Color(0.04f, 0.05f, 0.05f));
        SetColor(_autoLamp, _autoMode ? _green : _yellow);
    }

    void ApplyZoneColor(int zone, Color baseColor)
    {
        Renderer[] renderers = _zoneRenderers[zone];
        if (renderers == null)
        {
            return;
        }

        Color color = baseColor;
        if (ShouldShowCurrentStepHint() && zone == ZoneForStep(_stepIndex))
        {
            color = Color.Lerp(baseColor, _yellow, 0.45f + Mathf.PingPong(Time.time * 2.8f, 0.25f));
        }
        else if (IsZoneCompleted(zone) || _phase == TrainingPhase.Completed)
        {
            color = Color.Lerp(baseColor, _done, 0.38f);
        }

        if (zone == _flashZone && Time.time < _flashUntil)
        {
            color = Color.Lerp(color, _flashColor, 0.70f);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            SetColor(renderers[i], color);
        }
    }

    bool ShouldShowCurrentStepHint()
    {
        return _phase == TrainingPhase.Training
            && _stepIndex >= 0
            && _stepIndex < _mistakesByStep.Length
            && _mistakesByStep[_stepIndex] >= StepHintMistakeThreshold;
    }

    bool IsZoneCompleted(int zone)
    {
        if (zone == ZonePower) return _completedSteps[0];
        if (zone == ZoneMode) return _completedSteps[1];
        if (zone == ZoneDoor) return _completedSteps[2] && (_stepIndex < 4 || _completedSteps[4]);
        if (zone == ZoneClamp) return _completedSteps[3];
        if (zone == ZoneStart) return _completedSteps[5];
        if (zone == ZoneEStop) return _completedSteps[6];
        if (zone == ZoneReset) return _completedSteps[7];
        return false;
    }

    void RefreshStatusScreen()
    {
        if (_screenText != null)
        {
            _screenText.text =
                "POWER " + (_powerOn ? "ON" : "OFF") +
                "\nMODE  " + (_autoMode ? "AUTO" : "MANUAL") +
                "\nDOOR  " + (_doorOpen ? "OPEN" : "CLOSED") +
                "\nCLAMP " + (_workpieceClamped ? "LOCK" : "OPEN") +
                "\nRUN   " + (_running ? "YES" : "NO") +
                "\nESTOP " + (_emergencyStopped ? "ACTIVE" : "CLEAR");
        }

        SetColor(_screenRenderer, _powerOn ? new Color(0.02f, 0.20f, 0.18f) : new Color(0.02f, 0.04f, 0.05f));

        if (_modeValueText != null)
        {
            _modeValueText.text = _autoMode ? "AUTO" : "MANUAL";
            _modeValueText.color = _autoMode ? _green : _yellow;
        }
    }

    void RefreshTowerLights()
    {
        Color dim = new Color(0.04f, 0.04f, 0.04f);
        SetColor(_towerGreen, _running ? _green : dim);
        SetColor(_towerYellow, _powerOn && !_running && !_emergencyStopped ? _yellow : dim);
        SetColor(_towerRed, _emergencyStopped ? _red : dim);
    }

    void UpdateStatusText()
    {
    }

    string PhaseName()
    {
        switch (_phase)
        {
            case TrainingPhase.WaitingStart:
                return "点击启动按钮";
            case TrainingPhase.Training:
                return _stepIndex >= 0 && _stepIndex < _stepNames.Length ? _stepNames[_stepIndex] : "训练完成";
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
            return "点击启动训练，按电源 → AUTO → 开门 → 夹紧 → 关门 → 启动 → 急停 → 复位";
        }

        if (_phase == TrainingPhase.Completed)
        {
            return "训练完成，可重新训练或返回";
        }

        return _stepIndex >= 0 && _stepIndex < _stepHints.Length ? _stepHints[_stepIndex] : "";
    }

    float Progress01()
    {
        return Mathf.Clamp01(CompletedStepCount() / 8f);
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

    int ZoneForAction(CNCAction action)
    {
        switch (action)
        {
            case CNCAction.PowerTap: return ZonePower;
            case CNCAction.ModeRotate: return ZoneMode;
            case CNCAction.DoorSlide: return ZoneDoor;
            case CNCAction.ClampRotate: return ZoneClamp;
            case CNCAction.StartTap: return ZoneStart;
            case CNCAction.EmergencyStopTap: return ZoneEStop;
            case CNCAction.ResetTap: return ZoneReset;
            default: return -1;
        }
    }

    int ZoneForStep(int step)
    {
        switch (step)
        {
            case 0: return ZonePower;
            case 1: return ZoneMode;
            case 2: return ZoneDoor;
            case 3: return ZoneClamp;
            case 4: return ZoneDoor;
            case 5: return ZoneStart;
            case 6: return ZoneEStop;
            case 7: return ZoneReset;
            default: return -1;
        }
    }

    string ZoneName(int zone)
    {
        switch (zone)
        {
            case ZonePower: return "电源按钮";
            case ZoneMode: return "模式旋钮";
            case ZoneDoor: return "安全门";
            case ZoneClamp: return "夹具手柄";
            case ZoneStart: return "启动按钮";
            case ZoneEStop: return "急停按钮";
            case ZoneReset: return "复位按钮";
            default: return "目标部件";
        }
    }

    string GestureHintForZone(int zone)
    {
        switch (zone)
        {
            case ZonePower:
            case ZoneStart:
            case ZoneEStop:
            case ZoneReset:
                return "Point + Tap 点按";
            case ZoneMode:
            case ZoneClamp:
                return "Grab + Rotate 旋转";
            case ZoneDoor:
                return "Grab + Slide 水平滑动";
            default:
                return "当前步骤提示";
        }
    }

    Vector3 TapPoint()
    {
        return hand != null && hand.Points != null && hand.Points.Length > 8 ? hand.Points[8] : hand.GripPoint;
    }

    Vector3 TargetPosition(int zone)
    {
        return _targets[zone] != null ? _targets[zone].position : Vector3.one * 999f;
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
        cylinder.transform.localRotation = axis == Axis.Z ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;

        Destroy(cylinder.GetComponent<Collider>());
        SetColor(cylinder.GetComponent<Renderer>(), color);
        return cylinder;
    }

    Renderer CreateLamp(Transform parent, string name, Vector3 localPosition, float diameter)
    {
        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = name;
        lamp.transform.parent = parent;
        lamp.transform.localPosition = localPosition;
        lamp.transform.localScale = Vector3.one * diameter;
        Destroy(lamp.GetComponent<Collider>());
        Renderer renderer = lamp.GetComponent<Renderer>();
        SetColor(renderer, new Color(0.04f, 0.04f, 0.04f));
        return renderer;
    }

    TextMesh CreateText(Transform parent, string name, string text, Vector3 localPosition, float characterSize, Color color, TextAnchor anchor, bool bold)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent;
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        TextMesh mesh = go.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.anchor = anchor;
        mesh.alignment = TextAlignment.Center;
        mesh.fontSize = 42;
        float scaledCharacterSize = characterSize * TextSizeScale;
        mesh.characterSize = scaledCharacterSize;
        mesh.color = color;
        CNCUiFont.Apply(mesh, scaledCharacterSize, bold);
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
}
