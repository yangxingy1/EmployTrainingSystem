using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LeadTrainGestureTrainingController : MonoBehaviour
{
    public const string TaskId = "lead_train1_gesture";

    public HandInput hand;
    public HandVisual handVisual;

    enum TrainingPhase
    {
        WaitingStart,
        SwitchSequence,
        ReadyToConfirm,
        Completed
    }

    readonly int[] _sequence = { 1, 3, 0, 2 };
    readonly ElectricSwitchInteractable[] _switches = new ElectricSwitchInteractable[4];
    readonly Renderer[] _switchLamps = new Renderer[4];
    readonly bool[] _switchCompleted = new bool[4];

    TrainingPhase _phase = TrainingPhase.WaitingStart;
    Transform _stageRoot;
    TextMesh _status;
    FingertipTapButton _startButton;
    FingertipTapButton _confirmButton;
    FingertipTapButton _resetButton;
    GameObject _cursor;
    Renderer _cursorRenderer;
    int _sequenceIndex;
    int _lastWrongSwitch = -1;
    bool _initialized;

    readonly Color _panelColor = new Color(0.14f, 0.17f, 0.20f);
    readonly Color _panelEdgeColor = new Color(0.42f, 0.47f, 0.52f);
    readonly Color _busBarColor = new Color(0.92f, 0.62f, 0.08f);
    readonly Color _switchIdle = new Color(0.78f, 0.11f, 0.08f);
    readonly Color _switchDone = new Color(0.18f, 0.78f, 0.34f);
    readonly Color _metalColor = new Color(0.56f, 0.61f, 0.66f);
    readonly Color _lampDim = new Color(0.06f, 0.07f, 0.08f);
    readonly Color _lampGreen = new Color(0.18f, 0.90f, 0.38f);
    readonly Color _lampYellow = new Color(1f, 0.74f, 0.18f);

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

        ReturnToHubInput returnInput = FindObjectOfType<ReturnToHubInput>();
        if (returnInput == null)
        {
            returnInput = gameObject.AddComponent<ReturnToHubInput>();
        }

        string currentScene = SceneManager.GetActiveScene().name;
        returnInput.fallbackSceneName = string.IsNullOrEmpty(currentScene) ? "lead-train1" : currentScene;
        returnInput.preferFallbackScene = true;
    }

    public void RestartTraining(bool resetTrainingFlow = true)
    {
        _phase = TrainingPhase.WaitingStart;
        _sequenceIndex = 0;
        _lastWrongSwitch = -1;

        for (int i = 0; i < _switchCompleted.Length; i++)
        {
            _switchCompleted[i] = false;
            if (_switches[i] != null)
            {
                _switches[i].SetUp(true);
            }
        }

        if (resetTrainingFlow)
        {
            TrainingFlowController.EnsureExists(TaskId);
        }

        RefreshButtonStates();
        RefreshSwitchColors();
        UpdateStatusText();
    }

    void Update()
    {
        if (!_initialized)
        {
            return;
        }

        UpdateTaskProgress();
        UpdateCursor();
        RefreshSwitchColors();
        RefreshButtonStates();
        UpdateStatusText();
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
            GameObject camGo = new GameObject("LeadTrainGestureCamera");
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
        }

        cam.transform.SetParent(null, true);
        cam.transform.position = new Vector3(0f, 0.34f, -6.6f);
        cam.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        cam.fieldOfView = 47f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.09f, 0.11f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SetupTrainingLights()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.34f, 0.36f, 0.40f);

        GameObject keyGo = new GameObject("LeadGesture_KeyLight");
        Light key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 0.92f;
        key.color = new Color(1f, 0.96f, 0.88f);
        keyGo.transform.rotation = Quaternion.Euler(38f, -30f, 0f);

        GameObject fillGo = new GameObject("LeadGesture_FillLight");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.intensity = 1.55f;
        fill.range = 8f;
        fill.color = new Color(0.46f, 0.72f, 1f);
        fillGo.transform.position = new Vector3(-2.7f, 2.4f, -2.2f);
    }

    void CreateVirtualHand()
    {
        if (hand != null)
        {
            handVisual = hand.GetComponent<HandVisual>();
            return;
        }

        GameObject handGo = new GameObject("LeadTrainGestureHand");
        hand = handGo.AddComponent<HandInput>();
        hand.url = "ws://127.0.0.1:8765";
        hand.planeWidth = 6.3f;
        hand.planeHeight = 4.0f;
        hand.planeOrigin = new Vector3(0f, 0f, -0.52f);
        hand.gain = 1.28f;
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
        GameObject root = new GameObject("LeadTrainGestureStage");
        root.transform.parent = transform;
        root.transform.position = Vector3.zero;
        _stageRoot = root.transform;

        CreateBox(_stageRoot, "TrainingBackdrop", new Vector3(0f, 0.03f, 0.44f), new Vector3(7.3f, 4.3f, 0.18f), new Color(0.10f, 0.13f, 0.16f));
        CreateBox(_stageRoot, "WorkbenchDeck", new Vector3(0f, -1.78f, 0.08f), new Vector3(7.0f, 0.24f, 0.58f), new Color(0.20f, 0.22f, 0.25f));
        CreateBox(_stageRoot, "SwitchPanel", new Vector3(0f, -0.10f, 0.06f), new Vector3(6.30f, 2.34f, 0.18f), _panelColor);
        CreateBox(_stageRoot, "SwitchPanelTopEdge", new Vector3(0f, 1.10f, -0.04f), new Vector3(6.48f, 0.08f, 0.13f), _panelEdgeColor);
        CreateBox(_stageRoot, "SwitchPanelBottomEdge", new Vector3(0f, -1.30f, -0.04f), new Vector3(6.48f, 0.08f, 0.13f), _panelEdgeColor);
        CreateBox(_stageRoot, "SwitchPanelLeftEdge", new Vector3(-3.20f, -0.10f, -0.04f), new Vector3(0.08f, 2.34f, 0.13f), _panelEdgeColor);
        CreateBox(_stageRoot, "SwitchPanelRightEdge", new Vector3(3.20f, -0.10f, -0.04f), new Vector3(0.08f, 2.34f, 0.13f), _panelEdgeColor);

        CreateBusBar("UpperCopperBus", new Vector3(0f, 0.78f, -0.15f), 5.7f);
        CreateBusBar("LowerCopperBus", new Vector3(0f, -0.78f, -0.15f), 5.7f);

        float[] xPositions = { -2.28f, -0.76f, 0.76f, 2.28f };
        for (int i = 0; i < xPositions.Length; i++)
        {
            BuildSwitch(i, new Vector3(xPositions[i], -0.02f, -0.28f));
        }
    }

    void BuildSwitch(int index, Vector3 center)
    {
        GameObject root = new GameObject("LeadGestureSwitch_" + (index + 1));
        root.transform.parent = _stageRoot;
        root.transform.localPosition = center;

        CreateBox(root.transform, "SwitchBayPlate", new Vector3(0f, 0f, 0.24f), new Vector3(1.02f, 1.72f, 0.12f), new Color(0.18f, 0.21f, 0.24f));
        CreateBox(root.transform, "SwitchSlot", new Vector3(0f, 0f, 0.05f), new Vector3(0.18f, 1.30f, 0.06f), new Color(0.045f, 0.050f, 0.055f));
        CreateBox(root.transform, "LeftGuideRail", new Vector3(-0.19f, 0f, -0.04f), new Vector3(0.045f, 1.36f, 0.08f), _metalColor);
        CreateBox(root.transform, "RightGuideRail", new Vector3(0.19f, 0f, -0.04f), new Vector3(0.045f, 1.36f, 0.08f), _metalColor);
        CreateBox(root.transform, "UpperStop", new Vector3(0f, 0.61f, -0.06f), new Vector3(0.56f, 0.08f, 0.08f), new Color(0.18f, 0.62f, 0.32f));
        CreateBox(root.transform, "LowerStop", new Vector3(0f, -0.61f, -0.06f), new Vector3(0.56f, 0.08f, 0.08f), new Color(0.78f, 0.11f, 0.08f));

        GameObject sliderRoot = new GameObject("PullRodAssembly_" + (index + 1));
        sliderRoot.transform.parent = root.transform;
        sliderRoot.transform.localPosition = new Vector3(0f, 0.48f, -0.16f);

        GameObject rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rod.name = "VerticalPullRod";
        rod.transform.parent = sliderRoot.transform;
        rod.transform.localPosition = Vector3.zero;
        rod.transform.localRotation = Quaternion.identity;
        rod.transform.localScale = new Vector3(0.055f, 0.38f, 0.055f);
        Destroy(rod.GetComponent<Collider>());
        SetColor(rod.GetComponent<Renderer>(), _switchIdle);

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "HorizontalGripBar";
        handle.transform.parent = sliderRoot.transform;
        handle.transform.localPosition = new Vector3(0f, 0f, -0.02f);
        handle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        handle.transform.localScale = new Vector3(0.085f, 0.34f, 0.085f);
        Destroy(handle.GetComponent<Collider>());
        SetColor(handle.GetComponent<Renderer>(), _switchIdle);

        GameObject leftCap = CreateCap(sliderRoot.transform, "LeftGripCap", new Vector3(-0.68f, 0f, -0.02f));
        GameObject rightCap = CreateCap(sliderRoot.transform, "RightGripCap", new Vector3(0.68f, 0f, -0.02f));

        ElectricSwitchInteractable switchInteractable = root.AddComponent<ElectricSwitchInteractable>();
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
        switchInteractable.downY = -0.48f;
        switchInteractable.upY = 0.48f;
        switchInteractable.grabRadius = 0.62f;
        switchInteractable.grabThreshold = 0.46f;
        switchInteractable.releaseThreshold = 0.24f;
        switchInteractable.SetUp(true);

        _switches[index] = switchInteractable;
        _switchLamps[index] = CreateLamp(root.transform, "SwitchLamp_" + (index + 1), new Vector3(-0.32f, 0.78f, -0.07f));
        CreateSwitchNumber(root.transform, index + 1, new Vector3(0.34f, 0.78f, -0.12f));
    }

    GameObject CreateCap(Transform parent, string name, Vector3 localPosition)
    {
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cap.name = name;
        cap.transform.parent = parent;
        cap.transform.localPosition = localPosition;
        cap.transform.localScale = Vector3.one * 0.18f;
        Destroy(cap.GetComponent<Collider>());
        SetColor(cap.GetComponent<Renderer>(), _switchIdle);
        return cap;
    }

    void CreateSwitchNumber(Transform parent, int number, Vector3 localPosition)
    {
        GameObject labelGo = new GameObject("SwitchNumber_" + number);
        labelGo.transform.parent = parent;
        labelGo.transform.localPosition = localPosition;
        TextMesh label = labelGo.AddComponent<TextMesh>();
        label.text = number.ToString();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 38;
        label.characterSize = 0.040f;
        label.color = Color.white;
    }

    void BuildButtons()
    {
        _startButton = CreateButton("Start", new Vector3(-3.24f, -0.78f, -0.24f), new Vector3(0.72f, 0.36f, 0.10f), "START\n启动", new Color(0.10f, 0.62f, 0.26f));
        _confirmButton = CreateButton("Confirm", new Vector3(3.24f, -0.58f, -0.24f), new Vector3(0.72f, 0.36f, 0.10f), "CONFIRM\n确认", new Color(0.10f, 0.62f, 0.26f));
        _resetButton = CreateButton("Reset", new Vector3(3.24f, -1.16f, -0.24f), new Vector3(0.72f, 0.36f, 0.10f), "RESET\n重置", new Color(0.10f, 0.62f, 0.26f));

        _startButton.Clicked += HandleStartClicked;
        _confirmButton.Clicked += HandleConfirmClicked;
        _resetButton.Clicked += () => RestartTraining(true);
        RefreshButtonStates();
    }

    FingertipTapButton CreateButton(string name, Vector3 center, Vector3 size, string label, Color color)
    {
        GameObject go = new GameObject("LeadGestureButton_" + name);
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
        GameObject titleGo = new GameObject("LeadGestureTitle");
        titleGo.transform.parent = _stageRoot;
        titleGo.transform.localPosition = new Vector3(0f, 2.05f, -0.18f);
        TextMesh title = titleGo.AddComponent<TextMesh>();
        title.text = "四电闸手势真实训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 48;
        title.characterSize = 0.046f;
        title.color = Color.white;

        GameObject statusGo = new GameObject("LeadGestureStatus");
        statusGo.transform.parent = _stageRoot;
        statusGo.transform.localPosition = new Vector3(0f, 1.55f, -0.20f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.MiddleCenter;
        _status.alignment = TextAlignment.Center;
        _status.fontSize = 34;
        _status.characterSize = 0.034f;
        _status.lineSpacing = 0.82f;
        _status.color = new Color(0.78f, 0.90f, 1f);
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "LeadGestureGripCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();
        SetColor(_cursorRenderer, new Color(0.18f, 0.66f, 1f));
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

        _phase = TrainingPhase.SwitchSequence;
        TrainingFlowController.Active?.ReportProgress(0f, "四电闸真实训练", CurrentInstruction());
    }

    void HandleConfirmClicked()
    {
        if (_phase != TrainingPhase.ReadyToConfirm)
        {
            TrainingFlowController.Active?.RecordMistake("尚未完成四个电闸，不能确认");
            return;
        }

        _phase = TrainingPhase.Completed;
        TrainingFlowController.Active?.RecordSuccess("流程确认完成");
    }

    void UpdateTaskProgress()
    {
        if (_phase != TrainingPhase.SwitchSequence)
        {
            TrainingFlowController.Active?.ReportProgress(Progress01(), PhaseName(), CurrentInstruction());
            return;
        }

        int targetIndex = _sequence[_sequenceIndex];
        ElectricSwitchInteractable targetSwitch = _switches[targetIndex];
        if (targetSwitch != null && !targetSwitch.IsUp && !_switchCompleted[targetIndex])
        {
            _switchCompleted[targetIndex] = true;
            _sequenceIndex++;
            _lastWrongSwitch = -1;
            TrainingFlowController.Active?.RecordSuccess("电闸 " + (targetIndex + 1) + " 已拉下");

            if (_sequenceIndex >= _sequence.Length)
            {
                _phase = TrainingPhase.ReadyToConfirm;
            }
        }

        for (int i = 0; i < _switches.Length; i++)
        {
            ElectricSwitchInteractable current = _switches[i];
            if (i == targetIndex || _switchCompleted[i] || current == null)
            {
                continue;
            }

            if (!current.IsUp)
            {
                TrainingFlowController.Active?.RecordMistake("顺序错误，请回忆引导演示；误拉电闸 " + (i + 1));
                current.SetUp(true);
                _lastWrongSwitch = -1;
                continue;
            }

            if (current.IsGrabbed && _lastWrongSwitch != i)
            {
                _lastWrongSwitch = i;
                TrainingFlowController.Active?.RecordMistake("顺序错误，请回忆引导演示；误碰电闸 " + (i + 1));
            }
        }

        TrainingFlowController.Active?.ReportProgress(Progress01(), PhaseName(), CurrentInstruction());
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
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.11f, 0.22f, hand.PinchOnlyStrength);

        bool grabbed = false;
        bool hovering = false;
        for (int i = 0; i < _switches.Length; i++)
        {
            ElectricSwitchInteractable current = _switches[i];
            if (current == null)
            {
                continue;
            }

            grabbed |= current.IsGrabbed;
            hovering |= current.IsHovering;
        }

        Color color = grabbed ? _lampGreen : hovering ? _lampYellow : new Color(0.18f, 0.66f, 1f);
        SetColor(_cursorRenderer, color);
    }

    void RefreshSwitchColors()
    {
        for (int i = 0; i < _switches.Length; i++)
        {
            ElectricSwitchInteractable current = _switches[i];
            if (current == null)
            {
                continue;
            }

            current.hand = _phase == TrainingPhase.SwitchSequence ? hand : null;

            if (_switchCompleted[i] || _phase == TrainingPhase.Completed)
            {
                current.idleColor = _switchDone;
                current.hoverColor = Color.Lerp(_switchDone, Color.white, 0.25f);
                current.grabbedColor = _switchDone;
                SetColor(_switchLamps[i], _lampGreen);
            }
            else
            {
                current.idleColor = _switchIdle;
                current.hoverColor = _phase == TrainingPhase.SwitchSequence
                    ? new Color(1f, 0.54f, 0.16f)
                    : _switchIdle;
                current.grabbedColor = _lampGreen;
                SetColor(_switchLamps[i], _lampDim);
            }
        }
    }

    void RefreshButtonStates()
    {
        if (_startButton != null)
        {
            _startButton.interactable = _phase == TrainingPhase.WaitingStart || _phase == TrainingPhase.Completed;
        }

        if (_confirmButton != null)
        {
            _confirmButton.interactable = _phase == TrainingPhase.ReadyToConfirm;
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
            "状态：" + CurrentInstruction() + "    完成 " + CompletedSwitchCount() + "/4" +
            "\n" + handState + "    请根据引导演示完成操作";
    }

    string PhaseName()
    {
        switch (_phase)
        {
            case TrainingPhase.WaitingStart:
                return "点击启动按钮";
            case TrainingPhase.SwitchSequence:
                return "根据演示拉下电闸";
            case TrainingPhase.ReadyToConfirm:
                return "点击确认按钮";
            case TrainingPhase.Completed:
                return "训练完成";
            default:
                return "准备中";
        }
    }

    string CurrentInstruction()
    {
        if (_phase == TrainingPhase.WaitingStart)
        {
            return "点击启动";
        }

        if (_phase == TrainingPhase.ReadyToConfirm)
        {
            return "点击确认";
        }

        if (_phase == TrainingPhase.Completed)
        {
            return "可重新训练或返回";
        }

        if (CurrentTargetSwitchIndex() < 0)
        {
            return "等待确认";
        }

        return "根据演示拉下电闸";
    }

    int CurrentTargetSwitchIndex()
    {
        if (_phase != TrainingPhase.SwitchSequence || _sequenceIndex < 0 || _sequenceIndex >= _sequence.Length)
        {
            return -1;
        }

        return _sequence[_sequenceIndex];
    }

    int CompletedSwitchCount()
    {
        int count = 0;
        for (int i = 0; i < _switchCompleted.Length; i++)
        {
            if (_switchCompleted[i])
            {
                count++;
            }
        }

        return count;
    }

    float Progress01()
    {
        float count = CompletedSwitchCount();
        if (_phase == TrainingPhase.ReadyToConfirm)
        {
            count = 4f;
        }
        else if (_phase == TrainingPhase.Completed)
        {
            count = 5f;
        }

        return Mathf.Clamp01(count / 5f);
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

    void CreateBusBar(string name, Vector3 localPosition, float length)
    {
        GameObject busBar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        busBar.name = name;
        busBar.transform.parent = _stageRoot;
        busBar.transform.localPosition = localPosition;
        busBar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        busBar.transform.localScale = new Vector3(0.055f, length * 0.5f, 0.055f);
        Destroy(busBar.GetComponent<Collider>());
        SetColor(busBar.GetComponent<Renderer>(), _busBarColor);
    }

    Renderer CreateLamp(Transform parent, string name, Vector3 localPosition)
    {
        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = name;
        lamp.transform.parent = parent;
        lamp.transform.localPosition = localPosition;
        lamp.transform.localScale = Vector3.one * 0.105f;
        Destroy(lamp.GetComponent<Collider>());
        Renderer renderer = lamp.GetComponent<Renderer>();
        SetColor(renderer, _lampDim);
        return renderer;
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
