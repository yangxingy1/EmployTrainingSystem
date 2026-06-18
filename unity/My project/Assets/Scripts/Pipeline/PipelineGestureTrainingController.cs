using UnityEngine;

/// <summary>
/// 化工厂管道场景 — 手势训练迷你场景控制器。
/// 在玩家按下 F 键后进入，通过摄像头手势识别完成按钮点击操作。
/// 完全由脚本搭建（不依赖独立 .unity 场景文件），参考 LeadTrain 系列控制器。
///
/// 步骤 → 布局映射：
///   步骤 1 (PPECheck)          → 白板 + 圆形黄按钮 + 清单（安全帽/手套/护目镜）
///   步骤 3 (ReadInitialPressure)→ 精细压力表（表盘/刻度/指针/数字屏）+ 蓝色确认按钮
///   步骤 5 (MonitorFlowMeter)   → 深色面板 + 读数 + 矩形确认按钮
///   步骤 7 (CheckMidPressure)   → 精细压力表（表盘/刻度/指针/数字屏）+ 蓝色确认按钮
///   步骤 9 (EmergencyStopTest)  → 红色警示面板 + 圆形红色急停按钮 + 警告标语
///   步骤 10 (SystemShutdown)    → 方形桌子 + 工业控制台机器 + 绿色提交按钮
///
/// ── 如何自定义某步骤的布局 ──────────────────────────────────────
///   BuildStage()  / BuildButton()  / BuildTexts()  各自根据 _step 分发。
///   为你的步骤新增一个 BuildXXXStepName() 方法，在其中自由搭建几何体、
///   按钮样式和文字内容。可用工具方法：
///     CreateBox(parent, name, localPos, localScale, color)   → 矩形块
///     CreateCylinder(parent, name, localPos, scale, color)   → 圆柱/圆盘
///     CreateText(parent, name, localPos, text, fontSize, charSize, color, anchor)
///   FingertipTapButton 始终是交互核心；创建后挂到 _stageRoot 并订阅 Clicked。
///   按钮的 Build(center, size, label, color) 决定其位置与视觉尺寸。
/// </summary>
[DisallowMultipleComponent]
public class PipelineGestureTrainingController : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    //  配置（由 PipelineSceneRuntime 在 ConfigureAndBegin 中设置）
    // ═══════════════════════════════════════════════════════════════
    PipelineTrainingManager.PipelineStep _step;
    string _stepDisplayName;
    string _gaugeValueDisplay;          // null = 纯按钮步骤；非 null = 带读数显示的步骤
    Color _backgroundColor = new Color(0.07f, 0.09f, 0.11f);
    System.Action<bool> _onComplete;    // bool = success (true = 确认完成, false = 取消)
    PipelineSceneRuntime _runtime;

    // ═══════════════════════════════════════════════════════════════
    //  手势组件
    // ═══════════════════════════════════════════════════════════════
    HandInput _hand;
    HandVisual _handVisual;
    GameObject _cursor;
    Renderer _cursorRenderer;

    // ═══════════════════════════════════════════════════════════════
    //  场景组件
    // ═══════════════════════════════════════════════════════════════
    Transform _stageRoot;
    FingertipTapButton _confirmButton;
    TextMesh _titleText;
    TextMesh _statusText;
    TextMesh _gaugeText;
    Camera _trainingCamera;
    Camera _handOverlayCamera;              // 手部渲染专用相机（始终在最上层）
    const int HandOverlayLayer = 7;         // 手部独立渲染层（避免被场景物体遮挡）
    bool _handLayerApplied;                 // 是否已设置手部子对象的 layer

    // ═══════════════════════════════════════════════════════════════
    //  压力表专用组件（步骤 3 / 7）
    // ═══════════════════════════════════════════════════════════════
    float _gaugeNumericValue;               // 当前压力读数（MPa）
    float _gaugeMaxValue;                   // 表盘满量程（MPa）
    Transform _gaugePointerPivot;           // 指针旋转枢轴
    TextMesh _gaugeDisplayText;             // 数字读数屏文字
    const float GaugeMinAngle = -135f;      // 表盘 0 刻度角度（左下）
    const float GaugeMaxAngle = 135f;       // 表盘满量程角度（右下）
    const float GaugePointerSmooth = 8f;    // 指针平滑速度

    // ═══════════════════════════════════════════════════════════════
    //  阀门操作专用组件（步骤 4 / 6 / 8）
    // ═══════════════════════════════════════════════════════════════
    float _valveAngle;                      // 当前旋转角度（度）
    float _valveTargetAngle = 720f;         // 完成目标角度（默认 720°）
    Transform _valveHandwheelPivot;         // 手轮旋转枢轴
    bool _valveGrabbed;                     // 是否正在抓握手轮
    float _valveLastHandAngle;              // 上一帧手部角度（弧度）
    Vector3 _valveWheelCenter;              // 手轮中心世界坐标
    float _valveWheelRadius = 0.48f;        // 手轮交互半径
    Renderer[] _valveWheelAllRenderers;     // 手轮全部 Renderer（颜色反馈）
    Color _valveWheelIdleColor;             // 手轮默认颜色
    Color _valveWheelHoverColor;            // 手轮悬停颜色
    Color _valveWheelGrabbedColor;          // 手轮抓握颜色
    TextMesh _valveAngleText;               // 当前角度显示文字
    bool _valveCompleted;                   // 阀门是否已达到目标角度
    float _flowMaxValue;                    // 当前 V1 开度下的最大流量（步骤 6 用）
    TextMesh _flowDisplayText;              // 流量读数显示文字（步骤 6 用）

    // ═══════════════════════════════════════════════════════════════
    //  状态
    // ═══════════════════════════════════════════════════════════════
    bool _initialized;
    bool _completed;
    float _completeTime;
    const float ExitDelay = 1.8f;

    // ═══════════════════════════════════════════════════════════════
    //  OnGUI 提示样式（左上角 R/T 键说明）
    // ═══════════════════════════════════════════════════════════════
    GUIStyle _hintBoxStyle;
    GUIStyle _hintLabelStyle;
    Texture2D _hintBgTex;
    bool _hintStylesReady;

    // ═══════════════════════════════════════════════════════════════
    //  圆形按钮自定义交互（步骤 1 PPE 用，替代 FingertipTapButton）
    // ═══════════════════════════════════════════════════════════════
    Vector3 _circleBtnCenter;               // 按钮世界坐标
    float _circleBtnRadius;                 // 交互半径
    Renderer _circleBtnDiscRenderer;        // 黄色圆盘 Renderer（颜色反馈）
    Color _circleBtnIdleColor;              // 默认颜色
    Color _circleBtnHoverColor;             // 悬停颜色
    Color _circleBtnPressedColor;           // 按下颜色

    bool _circleHovering;
    bool _circleArmed;
    bool _circleReady;
    bool _circlePressed;
    float _circleHoverStartTime;
    Vector3 _circleLastPoint;
    bool _circleHasLastPoint;
    float _circleLastPressTime;

    const float CircleTapReadySeconds = 0.11f;
    const float CircleTapMinDownDelta = 0.028f;
    const float CircleTapMinDownSpeed = 0.18f;
    const float CircleTapCooldown = 0.45f;
    const float CircleTapStabilizeMaxDrift = 0.060f;

    // ═══════════════════════════════════════════════════════════════
    //  保存 / 恢复
    // ═══════════════════════════════════════════════════════════════
    Camera _previousMainCamera;
    string _previousMainCameraTag;
    bool _previousCursorVisible;
    CursorLockMode _previousCursorLock;

    // ═══════════════════════════════════════════════════════════════
    //  颜色预设（默认深色面板用）
    // ═══════════════════════════════════════════════════════════════
    readonly Color _panelColor = new Color(0.14f, 0.17f, 0.20f);
    readonly Color _panelEdgeColor = new Color(0.42f, 0.52f, 0.62f);
    readonly Color _confirmBtnColor = new Color(0.12f, 0.58f, 0.92f);

    // ═══════════════════════════════════════════════════════════════
    //  公开 API
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 配置并启动手势训练迷你场景。
    /// </summary>
    public void ConfigureAndBegin(
        PipelineTrainingManager.PipelineStep step,
        string stepName,
        string gaugeValue,
        float gaugeNumericValue,
        Color? bgColor,
        PipelineSceneRuntime runtime,
        System.Action<bool> onComplete,
        float initialValveAngle = 0f,
        float valveTargetAngle = 720f,
        float flowMaxValue = 0f)
    {
        _step = step;
        _stepDisplayName = stepName;
        _gaugeValueDisplay = gaugeValue;
        _gaugeNumericValue = gaugeNumericValue;
        _valveAngle = initialValveAngle;
        _valveTargetAngle = valveTargetAngle;
        _flowMaxValue = flowMaxValue;

        // 根据步骤确定表盘满量程（P1/P2 统一 0–1.0 MPa）
        _gaugeMaxValue = 1.0f;

        if (bgColor.HasValue) _backgroundColor = bgColor.Value;
        _runtime = runtime;
        _onComplete = onComplete;

        if (!_initialized)
            BeginTrainingScene();
        else
            RestartTraining();
    }

    /// <summary>阀门操作后的结果角度（供 PipelineSceneRuntime 读取）</summary>
    public float ValveAngle => _valveAngle;

    // ═══════════════════════════════════════════════════════════════
    //  场景搭建（仅执行一次）
    // ═══════════════════════════════════════════════════════════════

    void BeginTrainingScene()
    {
        if (_initialized) return;
        _initialized = true;

        SaveMainCameraState();
        SetupTrainingCamera();
        SetupTrainingLights();
        CreateVirtualHand();
        BuildStage();       // ★ 按 _step 分发
        BuildCursor();
        BuildButton();      // ★ 按 _step 分发
        BuildTexts();       // ★ 按 _step 分发
        RestartTraining();

        Debug.Log("[PipelineGestureTraining] 迷你场景搭建完成 — " + _stepDisplayName);
    }

    // ═══════════════════════════════════════════════════════════════
    //  保存 / 恢复主相机
    // ═══════════════════════════════════════════════════════════════

    void SaveMainCameraState()
    {
        _previousMainCamera = Camera.main;
        if (_previousMainCamera != null)
        {
            _previousMainCameraTag = _previousMainCamera.tag;
            _previousMainCamera.tag = "Untagged";
            _previousMainCamera.enabled = false;
        }
        _previousCursorVisible = Cursor.visible;
        _previousCursorLock = Cursor.lockState;
    }

    void RestoreMainCameraState()
    {
        if (_previousMainCamera != null)
        {
            _previousMainCamera.enabled = true;
            _previousMainCamera.tag = _previousMainCameraTag;
        }
        Cursor.visible = _previousCursorVisible;
        Cursor.lockState = _previousCursorLock;
    }

    void RestartTraining()
    {
        _completed = false;
        _completeTime = 0f;
        _valveGrabbed = false;
        _valveCompleted = false;

        if (_confirmButton != null)
            _confirmButton.interactable = true;

        if (_statusText != null)
            _statusText.color = new Color(0.78f, 0.90f, 1f);

        UpdateStatusText();
    }

    // ═══════════════════════════════════════════════════════════════
    //  相机 / 灯光 / 手势（所有步骤共用）
    // ═══════════════════════════════════════════════════════════════

    void SetupTrainingCamera()
    {
        // ── 主相机（渲染场景物体） ────────────────────────────
        GameObject camGo = new GameObject("PipelineGestureTrainingCamera");
        camGo.transform.SetParent(transform, false);

        _trainingCamera = camGo.AddComponent<Camera>();
        _trainingCamera.clearFlags = CameraClearFlags.SolidColor;
        _trainingCamera.backgroundColor = _backgroundColor;
        _trainingCamera.fieldOfView = 47f;
        _trainingCamera.nearClipPlane = 0.05f;
        _trainingCamera.farClipPlane = 100f;
        _trainingCamera.depth = 99f;
        // 排除手部渲染层，手部由 overlay 相机独立渲染（避免被场景物体遮挡）
        _trainingCamera.cullingMask &= ~(1 << HandOverlayLayer);

        camGo.tag = "MainCamera";

        camGo.transform.position = new Vector3(0f, 0.2f, -5.5f);
        camGo.transform.rotation = Quaternion.identity;

        // ── Overlay 相机（只渲染手部，始终在最上层） ────────────
        GameObject overlayGo = new GameObject("PipelineGestureHandOverlayCamera");
        overlayGo.transform.SetParent(transform, false);

        _handOverlayCamera = overlayGo.AddComponent<Camera>();
        _handOverlayCamera.clearFlags = CameraClearFlags.Depth;   // 保留主相机颜色缓冲
        _handOverlayCamera.cullingMask = 1 << HandOverlayLayer;   // 只渲染手部层
        _handOverlayCamera.depth = 100f;                          // 高于主相机
        _handOverlayCamera.fieldOfView = 47f;
        _handOverlayCamera.nearClipPlane = 0.05f;
        _handOverlayCamera.farClipPlane = 100f;

        overlayGo.transform.position = new Vector3(0f, 0.2f, -5.5f);
        overlayGo.transform.rotation = Quaternion.identity;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SetupTrainingLights()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.34f, 0.36f, 0.40f);

        GameObject keyGo = new GameObject("PipelineGesture_KeyLight");
        keyGo.transform.SetParent(transform, false);
        Light key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 0.92f;
        key.color = new Color(1f, 0.96f, 0.88f);
        keyGo.transform.rotation = Quaternion.Euler(38f, -30f, 0f);

        GameObject fillGo = new GameObject("PipelineGesture_FillLight");
        fillGo.transform.SetParent(transform, false);
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.intensity = 1.55f;
        fill.range = 8f;
        fill.color = new Color(0.46f, 0.72f, 1f);
        fillGo.transform.position = new Vector3(-2.7f, 2.4f, -2.2f);
    }

    void CreateVirtualHand()
    {
        GameObject handGo = new GameObject("PipelineGestureHand");
        handGo.transform.SetParent(transform, false);
        handGo.layer = HandOverlayLayer;        // 手部独立渲染层，避免被场景物体遮挡

        _hand = handGo.AddComponent<HandInput>();
        _hand.url = "ws://127.0.0.1:8765";
        _hand.planeWidth = 6.3f;
        _hand.planeHeight = 4.0f;
        _hand.planeOrigin = new Vector3(0f, 0f, -0.52f);
        _hand.gain = 1.28f;
        _hand.smoothing = 0.66f;
        _hand.graceTime = 0.45f;

        _handVisual = handGo.AddComponent<HandVisual>();
        _handVisual.jointRadius = 0.043f;
        _handVisual.skinColor = new Color(0.96f, 0.78f, 0.62f);
        _handVisual.gripColor = new Color(0.20f, 0.92f, 0.48f);
        _handVisual.enablePhysicalColliders = false;
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "PipelineGestureCursor";
        _cursor.transform.SetParent(transform, false);
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();
        SetColor(_cursorRenderer, new Color(0.18f, 0.66f, 1f));
    }

    // ═══════════════════════════════════════════════════════════════
    //  ★ 舞台搭建 — 按步骤分发（自定义布局请在此添加 case）
    // ═══════════════════════════════════════════════════════════════

    void BuildStage()
    {
        GameObject root = new GameObject("PipelineGestureStage");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;
        _stageRoot = root.transform;

        switch (_step)
        {
            case PipelineTrainingManager.PipelineStep.PPECheck:
                BuildPPEStage();
                break;
            case PipelineTrainingManager.PipelineStep.ReadInitialPressure:
            case PipelineTrainingManager.PipelineStep.CheckMidPressure:
                BuildGaugeStage();
                break;
            case PipelineTrainingManager.PipelineStep.MonitorFlowMeter:
                BuildFlowMeterStage();
                break;
            case PipelineTrainingManager.PipelineStep.EmergencyStopTest:
                BuildEStopStage();
                break;
            case PipelineTrainingManager.PipelineStep.SystemShutdown:
                BuildShutdownStage();
                break;
            case PipelineTrainingManager.PipelineStep.OpenInletValve:
            case PipelineTrainingManager.PipelineStep.AdjustControlValve:
            case PipelineTrainingManager.PipelineStep.OpenOutletValve:
                BuildValveStage();
                break;
            default:
                BuildDefaultStage();
                break;
        }
    }

    /// <summary>
    /// 步骤 4/6/8 舞台：水平黄色管道 + 红色阀门手轮。
    /// 管道横穿屏幕中央，阀门位于管道中间，手轮直径约为管道直径的 2 倍。
    /// </summary>
    void BuildValveStage()
    {
        // ══════════════════════════════════════════════════════════
        //  色彩定义
        // ══════════════════════════════════════════════════════════
        Color pipeYellow = new Color(0.92f, 0.78f, 0.15f);         // 管道黄色
        Color pipeDark = new Color(0.75f, 0.62f, 0.08f);           // 管道暗面（底部）
        Color valveBodyRed = new Color(0.72f, 0.10f, 0.08f);       // 阀体红色
        Color valveBodyDark = new Color(0.48f, 0.06f, 0.04f);      // 阀体暗红
        Color handwheelRed = new Color(0.85f, 0.12f, 0.09f);       // 手轮亮红
        Color handwheelBlue = new Color(0.12f, 0.45f, 0.85f);      // 手轮蓝色（步骤 6 控制阀）
        Color hubMetal = new Color(0.22f, 0.24f, 0.27f);           // 中心毂金属
        Color spokeMetal = new Color(0.30f, 0.32f, 0.35f);         // 轮辐金属
        Color flangeMetal = new Color(0.28f, 0.30f, 0.33f);        // 法兰金属

        // 步骤 6（调节控制阀）使用蓝色手轮；其余阀门步骤使用红色
        Color wheelColor = _step == PipelineTrainingManager.PipelineStep.AdjustControlValve
            ? handwheelBlue : handwheelRed;

        // ══════════════════════════════════════════════════════════
        //  背景遮罩
        // ══════════════════════════════════════════════════════════
        CreateBox(_stageRoot, "ValveStage_Backdrop",
            new Vector3(0f, 0.03f, 0.44f),
            new Vector3(7.3f, 4.6f, 0.18f),
            new Color(0.06f, 0.08f, 0.11f));

        // ══════════════════════════════════════════════════════════
        //  水平黄色管道（沿 X 轴，屏幕中央）
        //  Cylinder 原始：diameter=1(XZ), height=2(Y)
        //  旋转 Z=90° 使管道沿 X 轴
        //  scale: X=长度/2, Y=直径, Z=直径
        // ══════════════════════════════════════════════════════════
        float pipeDiameter = 0.55f;
        float pipeLength = 5.5f;
        float pipeY = 0f;
        float pipeZ = 0.05f;

        // 左段管道（阀门左侧）
        CreateCylinder(_stageRoot, "Valve_Pipe_Left",
            new Vector3(-1.55f, pipeY, pipeZ),
            new Vector3(pipeDiameter, 1.6f, pipeDiameter),
            pipeYellow,
            new Vector3(0f, 0f, 90f));

        // 右段管道（阀门右侧）
        CreateCylinder(_stageRoot, "Valve_Pipe_Right",
            new Vector3(1.55f, pipeY, pipeZ),
            new Vector3(pipeDiameter, 1.6f, pipeDiameter),
            pipeYellow,
            new Vector3(0f, 0f, 90f));

        // 管道上下高光线（模拟光照）
        float pipeTopY = pipeY + pipeDiameter * 0.5f;
        float pipeBotY = pipeY - pipeDiameter * 0.5f;
        CreateBox(_stageRoot, "Valve_PipeHighlight_Top",
            new Vector3(0f, pipeTopY - 0.02f, pipeZ - 0.02f),
            new Vector3(pipeLength, 0.025f, 0.02f),
            new Color(0.95f, 0.88f, 0.35f));
        CreateBox(_stageRoot, "Valve_PipeShadow_Bot",
            new Vector3(0f, pipeBotY + 0.02f, pipeZ + 0.02f),
            new Vector3(pipeLength, 0.025f, 0.02f),
            pipeDark);

        // ══════════════════════════════════════════════════════════
        //  法兰（管道与阀体连接处，左右各一）
        // ══════════════════════════════════════════════════════════
        float flangeDia = pipeDiameter * 1.55f;
        float flangeThick = 0.06f;
        float flangeX = pipeDiameter * 0.5f + 0.22f;

        CreateCylinder(_stageRoot, "Valve_Flange_Left",
            new Vector3(-flangeX, pipeY, pipeZ),
            new Vector3(flangeDia, flangeThick, flangeDia),
            flangeMetal,
            new Vector3(0f, 0f, 90f));
        CreateCylinder(_stageRoot, "Valve_Flange_Right",
            new Vector3(flangeX, pipeY, pipeZ),
            new Vector3(flangeDia, flangeThick, flangeDia),
            flangeMetal,
            new Vector3(0f, 0f, 90f));

        // 法兰螺栓（每个法兰 4 颗，均匀分布）
        Color boltColor = new Color(0.18f, 0.19f, 0.21f);
        float boltR = flangeDia * 0.38f;
        for (int side = -1; side <= 1; side += 2)
        {
            float bx = side * flangeX;
            for (int i = 0; i < 4; i++)
            {
                float ang = i * 90f * Mathf.Deg2Rad;
                float boltX = bx + Mathf.Cos(ang) * boltR * 0.5f;
                float boltY = pipeY + Mathf.Sin(ang) * boltR * 0.5f;
                CreateCylinder(_stageRoot, "Valve_Bolt_" + ((side + 1) / 2) + "_" + i,
                    new Vector3(boltX, boltY, pipeZ - 0.04f),
                    new Vector3(0.05f, 0.04f, 0.05f),
                    boltColor,
                    new Vector3(90f, 0f, 0f));
            }
        }

        // ══════════════════════════════════════════════════════════
        //  阀体（红色方形主体，位于管道中心）
        // ══════════════════════════════════════════════════════════
        float bodyW = 0.38f;
        float bodyH = pipeDiameter * 1.6f;
        float bodyD = 0.24f;
        Vector3 bodyCenter = new Vector3(0f, pipeY, pipeZ - 0.02f);

        // CreateBox(_stageRoot, "Valve_Body",
        //     bodyCenter,
        //     new Vector3(bodyW, bodyH, bodyD),
        //     valveBodyRed);

        // // 阀体顶/底高光（金属质感）
        // CreateBox(_stageRoot, "Valve_BodyTop",
        //     new Vector3(0f, bodyCenter.y + bodyH * 0.5f - 0.02f, bodyCenter.z - 0.01f),
        //     new Vector3(bodyW - 0.04f, 0.03f, bodyD - 0.02f),
        //     new Color(0.85f, 0.18f, 0.12f));
        // CreateBox(_stageRoot, "Valve_BodyBot",
        //     new Vector3(0f, bodyCenter.y - bodyH * 0.5f + 0.02f, bodyCenter.z + 0.01f),
        //     new Vector3(bodyW - 0.04f, 0.03f, bodyD - 0.02f),
        //     valveBodyDark);

        // ══════════════════════════════════════════════════════════
        //  阀杆（连接阀体到手轮的短圆柱）
        // ══════════════════════════════════════════════════════════
        float stemDia = 0.10f;
        float stemLen = 0.18f;
        float stemZ = bodyCenter.z - bodyD * 0.5f - stemLen * 0.5f;
        CreateCylinder(_stageRoot, "Valve_Stem",
            new Vector3(0f, pipeY, stemZ),
            new Vector3(stemDia, stemLen, stemDia),
            hubMetal,
            new Vector3(90f, 0f, 0f));

        // ══════════════════════════════════════════════════════════
        //  手轮（旋转部分，挂在 _valveHandwheelPivot 下）
        //  方向盘造型：外圈（分段圆环）+ 一根贯穿中心的横杆 + 中心毂
        //  其余地方镂空（不填充内圈/多余辐条）。
        // ══════════════════════════════════════════════════════════
        GameObject wheelPivotGo = new GameObject("Valve_HandwheelPivot");
        wheelPivotGo.transform.SetParent(_stageRoot, false);
        wheelPivotGo.transform.localPosition = new Vector3(0f, pipeY, stemZ - stemLen * 0.5f - 0.02f);
        wheelPivotGo.transform.localRotation = Quaternion.identity;
        _valveHandwheelPivot = wheelPivotGo.transform;

        float wheelDia = pipeDiameter * 2.0f;   // 手轮直径 ≈ 管道直径 × 2
        float wheelThick = 0.05f;               // 外圈厚度
        float wheelLocalZ = -0.02f;

        _valveWheelCenter = wheelPivotGo.transform.position;
        _valveWheelRadius = wheelDia * 0.5f + 0.04f; // 交互半径（略大于视觉半径）

        // ── 外圈：分段小方块排列成圆环（模拟方向盘/阀门手轮的空心外圈） ──
        //     每个小方块沿切线方向放置，28 段形成近似圆环。
        int ringSegments = 28;
        float ringRadius = wheelDia * 0.5f;
        float segTangential = Mathf.PI * wheelDia / ringSegments * 1.08f; // 切向长度（略重叠避免缝隙）
        float segRadial = 0.09f;       // 径向厚度
        float segThick = wheelThick;   // Z 向厚度

        for (int i = 0; i < ringSegments; i++)
        {
            float angleRad = (i / (float)ringSegments) * Mathf.PI * 2f;
            float sx = Mathf.Cos(angleRad) * ringRadius;
            float sy = Mathf.Sin(angleRad) * ringRadius;
            float zAngle = angleRad * Mathf.Rad2Deg + 90f; // 切线方向

            CreateBox(_valveHandwheelPivot, "Valve_RingSeg_" + i,
                new Vector3(sx, sy, wheelLocalZ),
                new Vector3(segTangential, segRadial, segThick),
                wheelColor,
                new Vector3(0f, 0f, zAngle));
        }

        // ── 横杆（单根长杆穿过中心，连接外圈左右两端，类似方向盘辐条） ──
        float barLen = wheelDia - segRadial * 2f;  // 从外圈内缘到内缘
        float barW = 0.07f;
        float barThick = wheelThick * 0.65f;
        CreateBox(_valveHandwheelPivot, "Valve_Crossbar",
            new Vector3(0f, 0f, wheelLocalZ),
            new Vector3(barLen, barW, barThick),
            spokeMetal);

        // ── 中心毂（横杆与中心轴交汇处） ────────────────────────────
        float hubDia = wheelDia * 0.16f;
        CreateCylinder(_valveHandwheelPivot, "Valve_Hub",
            new Vector3(0f, 0f, wheelLocalZ + 0.004f),
            new Vector3(hubDia, wheelThick * 1.5f, hubDia),
            hubMetal,
            new Vector3(90f, 0f, 0f));

        // 中心毂帽（小球）
        GameObject hubCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hubCap.name = "Valve_HubCap";
        hubCap.transform.SetParent(_valveHandwheelPivot, false);
        hubCap.transform.localPosition = new Vector3(0f, 0f, wheelLocalZ + 0.035f);
        hubCap.transform.localScale = Vector3.one * hubDia * 0.50f;
        Destroy(hubCap.GetComponent<Collider>());
        SetColor(hubCap.GetComponent<Renderer>(), new Color(0.35f, 0.37f, 0.40f));

        // ══════════════════════════════════════════════════════════
        //  手轮颜色状态预设（步骤 6 蓝色调，其余红色调）
        // ══════════════════════════════════════════════════════════
        _valveWheelIdleColor = wheelColor;
        _valveWheelHoverColor = _step == PipelineTrainingManager.PipelineStep.AdjustControlValve
            ? new Color(0.18f, 0.55f, 1f)           // 悬停：亮蓝
            : new Color(1f, 0.35f, 0.25f);          // 悬停：亮橙红
        _valveWheelGrabbedColor = new Color(0.2f, 0.85f, 0.35f);    // 抓握：绿色反馈（统一）

        // 收集手轮所有 Renderer
        _valveWheelAllRenderers = _valveHandwheelPivot.GetComponentsInChildren<Renderer>();

        // 设定初始手轮角度
        UpdateValveHandwheelVisual(instant: true);
    }

    /// <summary>
    /// 更新阀门手轮旋转角度（平滑过渡）。
    /// </summary>
    void UpdateValveHandwheelVisual(bool instant = false)
    {
        if (_valveHandwheelPivot == null) return;

        // 手轮绕 Z 轴旋转（面向相机），逆时针为正
        Quaternion targetRot = Quaternion.Euler(0f, 0f, _valveAngle);

        if (instant)
            _valveHandwheelPivot.localRotation = targetRot;
        else
            _valveHandwheelPivot.localRotation = Quaternion.Slerp(
                _valveHandwheelPivot.localRotation, targetRot, 12f * Time.deltaTime);
    }

    /// <summary>
    /// 步骤 1 舞台：黑边白板（模拟真实 PPE 确认面板）。
    /// </summary>
    void BuildPPEStage()
    {
        // ── 白板主体 ──────────────────────────────────────────
        CreateBox(_stageRoot, "WhiteBoard",
            new Vector3(0f, 0f, 0.08f),
            new Vector3(5.5f, 3.5f, 0.10f),
            new Color(0.94f, 0.95f, 0.96f));   // 米白色

        // ── 黑色边框（四边） ──────────────────────────────────
        Color frameBlack = new Color(0.08f, 0.08f, 0.10f);

        CreateBox(_stageRoot, "FrameTop",
            new Vector3(0f, 1.80f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f),
            frameBlack);
        CreateBox(_stageRoot, "FrameBottom",
            new Vector3(0f, -1.80f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f),
            frameBlack);
        CreateBox(_stageRoot, "FrameLeft",
            new Vector3(-2.80f, 0f, -0.04f),
            new Vector3(0.08f, 3.5f, 0.13f),
            frameBlack);
        CreateBox(_stageRoot, "FrameRight",
            new Vector3(2.80f, 0f, -0.04f),
            new Vector3(0.08f, 3.5f, 0.13f),
            frameBlack);
    }

    /// <summary>
    /// 步骤 9 舞台：深色面板 + 红色警示边框（急停测试）。
    /// </summary>
    void BuildEStopStage()
    {
        Color panelBg = new Color(0.13f, 0.08f, 0.08f);      // 暗红褐色面板
        Color frameRed = new Color(0.85f, 0.12f, 0.10f);     // 亮红色边框

        // ── 背景遮罩 ──────────────────────────────────────────
        CreateBox(_stageRoot, "EStop_Backdrop",
            new Vector3(0f, 0.03f, 0.44f),
            new Vector3(7.3f, 4.3f, 0.18f),
            new Color(0.08f, 0.03f, 0.03f));   // 深红黑

        // ── 主面板 ────────────────────────────────────────────
        CreateBox(_stageRoot, "EStop_Panel",
            new Vector3(0f, 0f, 0.08f),
            new Vector3(5.5f, 3.5f, 0.18f),
            panelBg);

        // ── 红色边框（四边） ──────────────────────────────────
        CreateBox(_stageRoot, "EStop_FrameTop",
            new Vector3(0f, 1.80f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f),
            frameRed);
        CreateBox(_stageRoot, "EStop_FrameBottom",
            new Vector3(0f, -1.80f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f),
            frameRed);
        CreateBox(_stageRoot, "EStop_FrameLeft",
            new Vector3(-2.80f, 0f, -0.04f),
            new Vector3(0.08f, 3.5f, 0.13f),
            frameRed);
        CreateBox(_stageRoot, "EStop_FrameRight",
            new Vector3(2.80f, 0f, -0.04f),
            new Vector3(0.08f, 3.5f, 0.13f),
            frameRed);
    }

    /// <summary>
    /// 步骤 10 舞台：方形桌子 + 绿色按钮（系统关停提交）。
    /// </summary>
    void BuildShutdownStage()
    {
        // ══════════════════════════════════════════════════════════
        //  步骤 10：方形控制台机器 + 桌子（正面视角）
        //  模拟一个工业控制终端，机器上有屏幕、指示灯、提交按钮。
        // ══════════════════════════════════════════════════════════

        Color tableTopColor = new Color(0.18f, 0.19f, 0.20f);    // 深灰桌面
        Color tableEdgeColor = new Color(0.12f, 0.13f, 0.14f);   // 桌沿
        Color legColor = new Color(0.10f, 0.11f, 0.12f);         // 桌腿深色
        Color machineBodyColor = new Color(0.20f, 0.22f, 0.25f); // 机器主体 — 深炭灰
        Color machinePanelColor = new Color(0.16f, 0.18f, 0.21f);// 前面板 — 略深
        Color screenBezelColor = new Color(0.06f, 0.07f, 0.09f); // 屏幕边框 — 纯黑
        Color screenColor = new Color(0.10f, 0.28f, 0.22f);      // 屏幕 — 暗绿色 CRT
        Color indicatorGreen = new Color(0.15f, 0.95f, 0.30f);   // 绿色指示灯
        Color indicatorYellow = new Color(1f, 0.85f, 0.15f);     // 黄色指示灯
        Color machineTopColor = new Color(0.24f, 0.26f, 0.29f);  // 顶部盖板

        // ── 背景墙（深色工业风） ──────────────────────────────
        CreateBox(_stageRoot, "Shutdown_BackWall",
            new Vector3(0f, 1.4f, 0.65f),
            new Vector3(7.3f, 5.5f, 0.12f),
            new Color(0.07f, 0.10f, 0.09f));

        // ── 方形桌面（宽 × 薄 × 深） ──────────────────────────
        float tableTopY = -0.22f;
        float tableTopHalfThick = 0.05f;
        float tableW = 4.5f;
        float tableD = 2.8f;

        CreateBox(_stageRoot, "Shutdown_TableTop",
            new Vector3(0f, tableTopY, 0f),
            new Vector3(tableW, tableTopHalfThick * 2f, tableD),
            tableTopColor);

        // 桌面边框（前后各一条，视觉加厚）
        CreateBox(_stageRoot, "Shutdown_TableEdgeFront",
            new Vector3(0f, tableTopY - 0.02f, -tableD * 0.5f + 0.04f),
            new Vector3(tableW + 0.06f, 0.04f, 0.08f),
            tableEdgeColor);
        CreateBox(_stageRoot, "Shutdown_TableEdgeBack",
            new Vector3(0f, tableTopY - 0.02f, tableD * 0.5f - 0.04f),
            new Vector3(tableW + 0.06f, 0.04f, 0.08f),
            tableEdgeColor);

        // ── 四条桌腿 ──────────────────────────────────────────
        float legH = 0.70f;
        float legY = tableTopY - tableTopHalfThick - legH * 0.5f;
        float halfW = tableW * 0.5f - 0.22f;
        float halfD = tableD * 0.5f - 0.18f;

        Vector3 legScale = new Vector3(0.14f, legH, 0.14f);
        CreateBox(_stageRoot, "Shutdown_Leg_FL", new Vector3(-halfW, legY, -halfD), legScale, legColor);
        CreateBox(_stageRoot, "Shutdown_Leg_FR", new Vector3( halfW, legY, -halfD), legScale, legColor);
        CreateBox(_stageRoot, "Shutdown_Leg_BL", new Vector3(-halfW, legY,  halfD), legScale, legColor);
        CreateBox(_stageRoot, "Shutdown_Leg_BR", new Vector3( halfW, legY,  halfD), legScale, legColor);

        // ── 机器主体（方形控制台，坐落在桌面上） ──────────────
        float machineBaseY = tableTopY + tableTopHalfThick;  // 桌面表面
        float machineH = 1.25f;
        float machineW = 2.2f;
        float machineD = 1.5f;
        Vector3 machineCenter = new Vector3(0f, machineBaseY + machineH * 0.5f, 0.12f);

        // 主箱体
        CreateBox(_stageRoot, "Shutdown_MachineBody",
            machineCenter,
            new Vector3(machineW, machineH, machineD),
            machineBodyColor);

        // ── 前面板（覆在机器正面的薄板，略深色） ──────────────
        float frontZ = machineCenter.z - machineD * 0.5f;
        CreateBox(_stageRoot, "Shutdown_FrontPanel",
            new Vector3(0f, machineCenter.y + 0.02f, frontZ - 0.025f),
            new Vector3(machineW - 0.06f, machineH - 0.06f, 0.03f),
            machinePanelColor);

        // ── 顶部盖板（机器上方略宽的板） ──────────────────────
        CreateBox(_stageRoot, "Shutdown_TopCover",
            new Vector3(0f, machineCenter.y + machineH * 0.5f + 0.02f, machineCenter.z),
            new Vector3(machineW + 0.10f, 0.04f, machineD + 0.10f),
            machineTopColor);

        // ── 屏幕区域（机器前面板上半部） ──────────────────────
        float screenPanelY = machineCenter.y + 0.18f;
        CreateBox(_stageRoot, "Shutdown_ScreenBezel",
            new Vector3(0f, screenPanelY, frontZ - 0.055f),
            new Vector3(1.7f, 0.55f, 0.03f),
            screenBezelColor);

        CreateBox(_stageRoot, "Shutdown_Screen",
            new Vector3(0f, screenPanelY, frontZ - 0.072f),
            new Vector3(1.45f, 0.42f, 0.02f),
            screenColor);

        // ── 屏幕上的模拟文字（系统状态） ──────────────────────
        CreateText(_stageRoot, "Shutdown_ScreenText",
            new Vector3(0f, screenPanelY + 0.04f, frontZ - 0.085f),
            "SYSTEM READY\nP2: 0.00 MPa",
            28, 0.028f,
            new Color(0.25f, 0.95f, 0.40f),
            TextAnchor.MiddleCenter);

        // ── 指示灯行（屏幕下方） ──────────────────────────────
        float indicatorY = screenPanelY - 0.35f;
        float indicatorZ = frontZ - 0.06f;
        float indicatorSpacing = 0.25f;

        CreateCylinder(_stageRoot, "Shutdown_LED_Green",
            new Vector3(-indicatorSpacing, indicatorY, indicatorZ),
            new Vector3(0.09f, 0.04f, 0.09f),
            indicatorGreen,
            new Vector3(90f, 0f, 0f));
        CreateCylinder(_stageRoot, "Shutdown_LED_Yellow",
            new Vector3(0f, indicatorY, indicatorZ),
            new Vector3(0.09f, 0.04f, 0.09f),
            indicatorYellow,
            new Vector3(90f, 0f, 0f));
        CreateCylinder(_stageRoot, "Shutdown_LED_Green2",
            new Vector3(indicatorSpacing, indicatorY, indicatorZ),
            new Vector3(0.09f, 0.04f, 0.09f),
            indicatorGreen,
            new Vector3(90f, 0f, 0f));
    }

    /// <summary>
    /// 默认舞台：深色面板 + 银色边框 + 按钮底座（步骤 5 流量计）。
    /// </summary>
    void BuildDefaultStage()
    {
        CreateBox(_stageRoot, "Backdrop",
            new Vector3(0f, 0.03f, 0.44f),
            new Vector3(7.3f, 4.3f, 0.18f),
            new Color(0.10f, 0.13f, 0.16f));

        CreateBox(_stageRoot, "MainPanel",
            new Vector3(0f, 0f, 0.08f),
            new Vector3(5.5f, 3.0f, 0.18f),
            _panelColor);

        CreateBox(_stageRoot, "PanelTopEdge",
            new Vector3(0f, 1.55f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f),
            _panelEdgeColor);
        CreateBox(_stageRoot, "PanelBottomEdge",
            new Vector3(0f, -1.55f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f),
            _panelEdgeColor);
        CreateBox(_stageRoot, "PanelLeftEdge",
            new Vector3(-2.8f, 0f, -0.04f),
            new Vector3(0.08f, 3.0f, 0.13f),
            _panelEdgeColor);
        CreateBox(_stageRoot, "PanelRightEdge",
            new Vector3(2.8f, 0f, -0.04f),
            new Vector3(0.08f, 3.0f, 0.13f),
            _panelEdgeColor);

        CreateBox(_stageRoot, "ButtonBasePlate",
            new Vector3(0f, -1.0f, 0.06f),
            new Vector3(2.6f, 0.18f, 0.08f),
            new Color(0.22f, 0.26f, 0.30f));
    }

    /// <summary>
    /// 步骤 5 舞台：长方形电子流量计 + 数字 LCD 显示屏（无指针/表盘/刻度）。
    /// 模拟真实工业电子流量计面板，中央是绿色 LCD 读数屏，四周是金属外壳。
    /// </summary>
    void BuildFlowMeterStage()
    {
        // ══════════════════════════════════════════════════════════
        //  色彩定义
        // ══════════════════════════════════════════════════════════
        Color meterBodyColor = new Color(0.18f, 0.20f, 0.23f);    // 流量计外壳 — 深枪灰
        Color meterBezelColor = new Color(0.10f, 0.11f, 0.13f);   // 屏幕边框 — 更深
        Color screenBgColor = new Color(0.03f, 0.07f, 0.05f);     // 屏幕背景 — 深绿黑 LCD
        Color screenTextColor = new Color(0.15f, 1f, 0.30f);      // 屏幕文字 — 亮绿色
        Color ledGreen = new Color(0.12f, 0.95f, 0.25f);          // 电源指示灯 — 绿色
        Color panelBgDarker = new Color(0.08f, 0.10f, 0.13f);     // 面板深色
        Color frameColor = new Color(0.35f, 0.45f, 0.55f);        // 边框银灰

        // ══════════════════════════════════════════════════════════
        //  背景遮罩 + 主面板 + 银色边框
        // ══════════════════════════════════════════════════════════
        CreateBox(_stageRoot, "FlowMeter_Backdrop",
            new Vector3(0f, 0.03f, 0.44f),
            new Vector3(7.3f, 4.6f, 0.18f),
            new Color(0.06f, 0.08f, 0.11f));

        CreateBox(_stageRoot, "FlowMeter_MainPanel",
            new Vector3(0f, 0f, 0.08f),
            new Vector3(5.5f, 3.6f, 0.18f),
            panelBgDarker);

        CreateBox(_stageRoot, "FlowMeter_FrameTop",
            new Vector3(0f, 1.85f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f), frameColor);
        CreateBox(_stageRoot, "FlowMeter_FrameBottom",
            new Vector3(0f, -1.85f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f), frameColor);
        CreateBox(_stageRoot, "FlowMeter_FrameLeft",
            new Vector3(-2.8f, 0f, -0.04f),
            new Vector3(0.08f, 3.6f, 0.13f), frameColor);
        CreateBox(_stageRoot, "FlowMeter_FrameRight",
            new Vector3(2.8f, 0f, -0.04f),
            new Vector3(0.08f, 3.6f, 0.13f), frameColor);

        // ══════════════════════════════════════════════════════════
        //  电子流量计主体（长方形金属箱体）
        // ══════════════════════════════════════════════════════════
        Vector3 meterCenter = new Vector3(0f, 0.35f, -0.10f);
        float meterW = 2.8f;
        float meterH = 1.6f;
        float meterD = 0.28f;

        // ── 外壳主体 ──────────────────────────────────────────
        CreateBox(_stageRoot, "FlowMeter_Body",
            meterCenter,
            new Vector3(meterW, meterH, meterD),
            meterBodyColor);

        // ── 前面板（略浅色的薄板，覆在箱体正面） ─────────────────
        Color frontPanelColor = new Color(0.22f, 0.24f, 0.27f);
        float frontZ = meterCenter.z - meterD * 0.5f;
        CreateBox(_stageRoot, "FlowMeter_FrontPanel",
            new Vector3(meterCenter.x, meterCenter.y, frontZ - 0.01f),
            new Vector3(meterW - 0.08f, meterH - 0.08f, 0.02f),
            frontPanelColor);

        // ══════════════════════════════════════════════════════════
        //  显示屏区域（在前面板上）
        // ══════════════════════════════════════════════════════════
        float screenFrontZ = frontZ - 0.03f;
        float screenW = 2.1f;
        float screenH = 0.90f;
        Vector3 screenCenter = meterCenter + new Vector3(0f, 0.05f, 0f);

        // ── 屏幕外框（深色金属边框，比屏幕略大） ─────────────────
        CreateBox(_stageRoot, "FlowMeter_ScreenBezel",
            new Vector3(screenCenter.x, screenCenter.y, screenFrontZ - 0.015f),
            new Vector3(screenW + 0.22f, screenH + 0.18f, 0.03f),
            meterBezelColor);

        // ── 屏幕背景（深绿黑 LCD 面板） ──────────────────────────
        CreateBox(_stageRoot, "FlowMeter_ScreenBg",
            new Vector3(screenCenter.x, screenCenter.y, screenFrontZ - 0.035f),
            new Vector3(screenW, screenH, 0.02f),
            screenBgColor);

        // ── 主读数（大字，亮绿色 LCD 风格） ──────────────────────
        string flowValueStr = _gaugeNumericValue.ToString("F1");
        CreateText(_stageRoot, "FlowMeter_ScreenValue",
            new Vector3(screenCenter.x, screenCenter.y + 0.10f, screenFrontZ - 0.05f),
            flowValueStr,
            76, 0.074f,
            screenTextColor,
            TextAnchor.MiddleCenter);
        _gaugeDisplayText = _stageRoot.Find("FlowMeter_ScreenValue")?.GetComponent<TextMesh>();

        // ── 单位文字（读数下方） ──────────────────────────────────
        CreateText(_stageRoot, "FlowMeter_ScreenUnit",
            new Vector3(screenCenter.x, screenCenter.y - 0.33f, screenFrontZ - 0.05f),
            "L/min",
            32, 0.032f,
            new Color(0.18f, 0.92f, 0.28f),
            TextAnchor.MiddleCenter);

        // ══════════════════════════════════════════════════════════
        //  "F1" 型号标签（外壳左上角） ──────────────────────────
        // ══════════════════════════════════════════════════════════
        CreateText(_stageRoot, "FlowMeter_LabelF1",
            new Vector3(meterCenter.x - meterW * 0.5f + 0.28f,
                meterCenter.y + meterH * 0.5f - 0.22f,
                screenFrontZ - 0.05f),
            "F1  电子流量计",
            26, 0.026f,
            new Color(0.62f, 0.68f, 0.74f),
            TextAnchor.MiddleLeft);

        // ══════════════════════════════════════════════════════════
        //  电源指示灯（外壳右上角，绿色 LED 小圆点） ─────────────
        // ══════════════════════════════════════════════════════════
        CreateCylinder(_stageRoot, "FlowMeter_PowerLED",
            new Vector3(meterCenter.x + meterW * 0.5f - 0.25f,
                meterCenter.y + meterH * 0.5f - 0.18f,
                screenFrontZ - 0.04f),
            new Vector3(0.10f, 0.04f, 0.10f),
            ledGreen,
            new Vector3(90f, 0f, 0f));

        // ── LED 标签 ────────────────────────────────────────────
        CreateText(_stageRoot, "FlowMeter_LEDLabel",
            new Vector3(meterCenter.x + meterW * 0.5f - 0.50f,
                meterCenter.y + meterH * 0.5f - 0.18f,
                screenFrontZ - 0.05f),
            "PWR",
            18, 0.018f,
            new Color(0.5f, 0.55f, 0.6f),
            TextAnchor.MiddleRight);

        // ══════════════════════════════════════════════════════════
        //  底座板（流量计下方，承接设备） ────────────────────────
        // ══════════════════════════════════════════════════════════
        CreateBox(_stageRoot, "FlowMeter_BasePlate",
            new Vector3(0f, meterCenter.y - meterH * 0.5f - 0.08f, meterCenter.z),
            new Vector3(meterW + 0.4f, 0.08f, meterD + 0.15f),
            new Color(0.22f, 0.26f, 0.30f));
    }

    /// <summary>
    /// 步骤 3/7 压力表舞台：深色面板 + 精细模拟压力表（表盘、刻度、指针、数字读数屏）。
    /// </summary>
    void BuildGaugeStage()
    {
        // ══════════════════════════════════════════════════════════
        //  色彩定义
        // ══════════════════════════════════════════════════════════
        Color bezelColor     = new Color(0.18f, 0.20f, 0.23f);   // 表圈 — 深枪灰
        Color faceColor      = new Color(0.96f, 0.94f, 0.89f);   // 表盘 — 暖米白
        Color majorTickColor = new Color(0.08f, 0.09f, 0.11f);   // 主刻度 — 深黑
        Color minorTickColor = new Color(0.25f, 0.26f, 0.29f);   // 次刻度 — 中灰
        Color needleColor    = new Color(0.92f, 0.10f, 0.08f);   // 指针 — 鲜红
        Color hubColor       = new Color(0.14f, 0.15f, 0.17f);   // 中心帽 — 深金属
        Color displayBg      = new Color(0.03f, 0.06f, 0.05f);   // 数字屏背景 — 深绿黑
        Color displayTextClr = new Color(0.18f, 1f, 0.32f);      // 数字屏文字 — 亮绿
        Color panelBgDarker  = new Color(0.08f, 0.10f, 0.13f);   // 面板深色
        Color frameColor     = new Color(0.35f, 0.45f, 0.55f);   // 边框银灰

        // ══════════════════════════════════════════════════════════
        //  背景遮罩 + 主面板
        // ══════════════════════════════════════════════════════════
        CreateBox(_stageRoot, "Gauge_Backdrop",
            new Vector3(0f, 0.03f, 0.44f),
            new Vector3(7.3f, 4.6f, 0.18f),
            new Color(0.06f, 0.08f, 0.11f));

        CreateBox(_stageRoot, "Gauge_MainPanel",
            new Vector3(0f, 0f, 0.08f),
            new Vector3(5.5f, 3.6f, 0.18f),
            panelBgDarker);

        // 银色边框
        CreateBox(_stageRoot, "Gauge_FrameTop",
            new Vector3(0f, 1.85f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f), frameColor);
        CreateBox(_stageRoot, "Gauge_FrameBottom",
            new Vector3(0f, -1.85f, -0.04f),
            new Vector3(5.68f, 0.08f, 0.13f), frameColor);
        CreateBox(_stageRoot, "Gauge_FrameLeft",
            new Vector3(-2.8f, 0f, -0.04f),
            new Vector3(0.08f, 3.6f, 0.13f), frameColor);
        CreateBox(_stageRoot, "Gauge_FrameRight",
            new Vector3(2.8f, 0f, -0.04f),
            new Vector3(0.08f, 3.6f, 0.13f), frameColor);

        // ══════════════════════════════════════════════════════════
        //  表圈（Bezel）—— 大直径暗色金属圆环底座，在表盘后面
        // ══════════════════════════════════════════════════════════
        Vector3 gaugeCenter = new Vector3(0f, 0.5f, -0.14f);
        float bezelDiam = 2.55f;
        float faceDiam = 2.1f;

        // 表圈底座：宽而薄，衬托表盘
        CreateCylinder(_stageRoot, "Gauge_Bezel",
            gaugeCenter + new Vector3(0f, 0f, 0.045f),
            new Vector3(bezelDiam, 0.14f, bezelDiam),
            bezelColor, new Vector3(90f, 0f, 0f));

        // 表圈外沿亮银细环
        CreateCylinder(_stageRoot, "Gauge_BezelRing",
            gaugeCenter + new Vector3(0f, 0f, 0.005f),
            new Vector3(bezelDiam + 0.08f, 0.04f, bezelDiam + 0.08f),
            new Color(0.50f, 0.54f, 0.58f), new Vector3(90f, 0f, 0f));

        // ══════════════════════════════════════════════════════════
        //  表盘面（Face）—— 暖米白色圆盘
        //  Cylinder 绕 X 转 90° 后两面分别朝 ±Z，相机在 Z=-5.5
        //  看到的可视面在 Z ≈ gaugeCenter.z - thickness/2 = -0.17
        // ══════════════════════════════════════════════════════════
        CreateCylinder(_stageRoot, "Gauge_Face",
            gaugeCenter,
            new Vector3(faceDiam, 0.06f, faceDiam),
            faceColor, new Vector3(90f, 0f, 0f));

        // ══════════════════════════════════════════════════════════
        //  刻度标记（Tick Marks）
        //  满量程 = _gaugeMaxValue，分为 10 大格，每格 5 等分
        // ══════════════════════════════════════════════════════════
        float majorStep = _gaugeMaxValue / 10f;
        int majorCount = 11; // 含 0 和满量程
        int subDivs = 5;     // 每大格细分数（含端点）

        float tickDepth = 0.006f;
        // 关键：tickZ 必须在表盘可视面之前（Z > -0.17），否则被表盘遮挡
        float tickZ = gaugeCenter.z + 0.025f;      // -0.115，远在表盘前面
        float tickOuterR = faceDiam * 0.5f - 0.06f;   // 刻度外缘半径 = 0.99
        float tickInnerMajorR = tickOuterR - 0.28f;    // 主刻度长 0.28
        float tickInnerMinorR = tickOuterR - 0.17f;    // 次刻度长 0.17

        for (int i = 0; i < majorCount; i++)
        {
            float val = i * majorStep;

            // ── 主刻度 ──────────────────────────────────────────
            float angleZ = Mathf.Lerp(GaugeMinAngle, GaugeMaxAngle, val / _gaugeMaxValue);
            float angleRad = angleZ * Mathf.Deg2Rad;
            float midR = (tickInnerMajorR + tickOuterR) * 0.5f;
            float tickLen = tickOuterR - tickInnerMajorR;

            float cx = gaugeCenter.x + midR * Mathf.Sin(angleRad);
            float cy = gaugeCenter.y + midR * Mathf.Cos(angleRad);

            CreateBox(_stageRoot, "Gauge_MajorTick_" + i,
                new Vector3(cx, cy, tickZ),
                new Vector3(0.06f, tickLen, tickDepth),
                majorTickColor,
                new Vector3(0f, 0f, angleZ));

            // ── 次刻度（不画在端点位置） ────────────────────────
            if (i < majorCount - 1)
            {
                for (int s = 1; s < subDivs; s++)
                {
                    float subVal = val + s * (majorStep / subDivs);
                    float subAngleZ = Mathf.Lerp(GaugeMinAngle, GaugeMaxAngle, subVal / _gaugeMaxValue);
                    float subAngleRad = subAngleZ * Mathf.Deg2Rad;
                    float subMidR = (tickInnerMinorR + tickOuterR) * 0.5f;
                    float subTickLen = tickOuterR - tickInnerMinorR;

                    float sx = gaugeCenter.x + subMidR * Mathf.Sin(subAngleRad);
                    float sy = gaugeCenter.y + subMidR * Mathf.Cos(subAngleRad);

                    CreateBox(_stageRoot, "Gauge_MinorTick_" + i + "_" + s,
                        new Vector3(sx, sy, tickZ),
                        new Vector3(0.035f, subTickLen, tickDepth),
                        minorTickColor,
                        new Vector3(0f, 0f, subAngleZ));
                }
            }
        }

        // ══════════════════════════════════════════════════════════
        //  数字标签 — 每隔一个主刻度（避免拥挤）
        // ══════════════════════════════════════════════════════════
        int labelStep = majorCount > 7 ? 2 : 1;
        float labelR = tickInnerMajorR - 0.16f;  // 数字在刻度内侧
        for (int i = 0; i < majorCount; i += labelStep)
        {
            float val = i * majorStep;
            float angleZ = Mathf.Lerp(GaugeMinAngle, GaugeMaxAngle, val / _gaugeMaxValue);
            float angleRad = angleZ * Mathf.Deg2Rad;
            float lx = gaugeCenter.x + labelR * Mathf.Sin(angleRad);
            float ly = gaugeCenter.y + labelR * Mathf.Cos(angleRad);

            string labelText;
            if (_gaugeMaxValue <= 0.3f)
                labelText = val.ToString("F2");       // 如 "0.04"
            else if (Mathf.Abs(val - Mathf.Round(val)) < 0.001f)
                labelText = val.ToString("F0");       // 如 "0", "1"
            else
                labelText = val.ToString("F1");       // 如 "0.5"

            CreateText(_stageRoot, "Gauge_Label_" + i,
                new Vector3(lx, ly, tickZ + 0.005f),
                labelText, 28, 0.028f,
                new Color(0.06f, 0.07f, 0.09f),
                TextAnchor.MiddleCenter);
        }

        // 单位标签（MPa）— 表盘中央下方
        CreateText(_stageRoot, "Gauge_UnitLabel",
            gaugeCenter + new Vector3(0f, -0.58f, tickZ + 0.005f),
            "MPa", 20, 0.020f,
            new Color(0.20f, 0.22f, 0.25f),
            TextAnchor.MiddleCenter);

        // ══════════════════════════════════════════════════════════
        //  指针（Pointer）—— 用 Cylinder 做针杆 + Sphere 做尖端
        //  全部使用 Cylinder/Sphere（已验证可渲染），避免 Box
        // ══════════════════════════════════════════════════════════
        GameObject pivotGo = new GameObject("Gauge_PointerPivot");
        pivotGo.transform.SetParent(_stageRoot, false);
        pivotGo.transform.localPosition = gaugeCenter;
        pivotGo.transform.localRotation = Quaternion.identity;
        _gaugePointerPivot = pivotGo.transform;

        float pointerLen = tickOuterR - 0.04f;     // 针尖到刻度外缘内侧 = 0.95
        float pointerTail = 0.18f;                 // 尾部（被中心帽盖住）
        float pointerTotal = pointerLen + pointerTail;  // 1.13
        float pointerZ = 0.11f;                    // world Z ≈ -0.03，最前层

        // ── 中心帽（Hub）先创建，在指针后面 ──────────────────
        float hubZ = pointerZ - 0.03f;             // hub 在指针后面 3cm

        CreateCylinder(_stageRoot, "Gauge_HubBase",
            gaugeCenter + new Vector3(0f, 0f, hubZ - 0.003f),
            new Vector3(0.26f, 0.06f, 0.26f),
            new Color(0.06f, 0.07f, 0.09f), new Vector3(90f, 0f, 0f));

        CreateCylinder(_stageRoot, "Gauge_Hub",
            gaugeCenter + new Vector3(0f, 0f, hubZ),
            new Vector3(0.20f, 0.08f, 0.20f),
            hubColor, new Vector3(90f, 0f, 0f));

        CreateCylinder(_stageRoot, "Gauge_HubTop",
            gaugeCenter + new Vector3(0f, 0f, hubZ + 0.02f),
            new Vector3(0.10f, 0.03f, 0.10f),
            new Color(0.22f, 0.24f, 0.26f), new Vector3(90f, 0f, 0f));

        // ── 指针：Cylinder 针杆（默认竖直，由 pivot 旋转带动） ──
        //     Cylinder 原始直径=1 高度=2；不旋转，保持竖直
        //     scale: X=直径  Z=直径  Y=高度/2
        float needleDiam = 0.10f;                  // 针杆直径 10cm
        CreateCylinder(_gaugePointerPivot, "Gauge_Needle",
            new Vector3(0f, pointerLen * 0.5f - pointerTail * 0.5f, pointerZ),
            new Vector3(needleDiam, pointerTotal, needleDiam),  // 无旋转→竖直圆柱
            needleColor);                                      // 亮红色

        // ── 针尖大球（Sphere，极其醒目，指向刻度） ────────────
        GameObject tipSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tipSphere.name = "Gauge_NeedleTip";
        tipSphere.transform.SetParent(_gaugePointerPivot, false);
        tipSphere.transform.localPosition = new Vector3(0f, pointerLen, pointerZ + 0.005f);
        tipSphere.transform.localScale = Vector3.one * 0.16f;  // 16cm 直径球
        Destroy(tipSphere.GetComponent<Collider>());
        SetColor(tipSphere.GetComponent<Renderer>(), new Color(1f, 0.25f, 0.10f));

        // ── 针尾小球（辅助判断反方向） ────────────────────────
        GameObject tailSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tailSphere.name = "Gauge_NeedleTail";
        tailSphere.transform.SetParent(_gaugePointerPivot, false);
        tailSphere.transform.localPosition = new Vector3(0f, -pointerTail, pointerZ);
        tailSphere.transform.localScale = Vector3.one * 0.08f;  // 8cm 直径球
        Destroy(tailSphere.GetComponent<Collider>());
        SetColor(tailSphere.GetComponent<Renderer>(), new Color(0.65f, 0.06f, 0.04f));

        // ══════════════════════════════════════════════════════════
        //  数字读数屏（Digital Display）—— 表盘下方绿色 LCD
        // ══════════════════════════════════════════════════════════
        Vector3 displayPos = new Vector3(0f, -0.82f, -0.10f);
        Vector3 displayScale = new Vector3(2.5f, 0.44f, 0.06f);

        // 屏外框（深灰色金属框）
        CreateBox(_stageRoot, "Gauge_DisplayBezel",
            displayPos + new Vector3(0f, 0f, 0.02f),
            new Vector3(displayScale.x + 0.2f, displayScale.y + 0.14f, 0.05f),
            new Color(0.12f, 0.14f, 0.16f));

        // 屏背景（深绿黑 LCD）
        CreateBox(_stageRoot, "Gauge_DisplayBg",
            displayPos,
            displayScale,
            displayBg);

        // 屏读数文字（亮绿色，大字）
        string gaugeId = _step == PipelineTrainingManager.PipelineStep.ReadInitialPressure
            ? "P1" : "P2";
        string displayStr = gaugeId + ": " + _gaugeNumericValue.ToString("F2") + " MPa";

        CreateText(_stageRoot, "Gauge_DisplayText",
            displayPos + new Vector3(0f, 0.04f, -0.035f),
            displayStr, 44, 0.044f,
            displayTextClr,
            TextAnchor.MiddleCenter);
        _gaugeDisplayText = _stageRoot.Find("Gauge_DisplayText")?.GetComponent<TextMesh>();

        // 设定初始指针角度
        UpdateGaugePointer(instant: true);
    }

    /// <summary>
    /// 更新压力表指针旋转角度（平滑过渡）。
    /// </summary>
    void UpdateGaugePointer(bool instant = false)
    {
        if (_gaugePointerPivot == null || _gaugeMaxValue <= 0f) return;

        float t = Mathf.Clamp01(_gaugeNumericValue / _gaugeMaxValue);
        float targetAngleZ = Mathf.Lerp(GaugeMinAngle, GaugeMaxAngle, t);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngleZ);

        if (instant)
            _gaugePointerPivot.localRotation = targetRot;
        else
            _gaugePointerPivot.localRotation = Quaternion.Slerp(
                _gaugePointerPivot.localRotation, targetRot,
                GaugePointerSmooth * Time.deltaTime);
    }

    // ═══════════════════════════════════════════════════════════════
    //  ★ 按钮搭建 — 按步骤分发
    // ═══════════════════════════════════════════════════════════════

    void BuildButton()
    {
        switch (_step)
        {
            case PipelineTrainingManager.PipelineStep.PPECheck:
                BuildPPEButton();
                break;
            case PipelineTrainingManager.PipelineStep.ReadInitialPressure:
            case PipelineTrainingManager.PipelineStep.CheckMidPressure:
                BuildGaugeButton();
                break;
            case PipelineTrainingManager.PipelineStep.MonitorFlowMeter:
                BuildFlowMeterButton();
                break;
            case PipelineTrainingManager.PipelineStep.EmergencyStopTest:
                BuildEStopButton();
                break;
            case PipelineTrainingManager.PipelineStep.SystemShutdown:
                BuildShutdownButton();
                break;
            case PipelineTrainingManager.PipelineStep.OpenInletValve:
            case PipelineTrainingManager.PipelineStep.AdjustControlValve:
            case PipelineTrainingManager.PipelineStep.OpenOutletValve:
                // 阀门步骤不需要确认按钮，通过手势抓握手轮旋转操作
                break;
            default:
                BuildDefaultButton();
                break;
        }
    }

    /// <summary>
    /// 步骤 1 按钮：圆形黄色按钮（黑边），位于白板中上方。
    /// 视觉 = 黑色大圆盘 + 黄色小圆盘叠加；交互 = 自定义圆形点击检测。
    /// </summary>
    void BuildPPEButton()
    {
        Vector3 btnPos = new Vector3(0f, 0.9f, -0.28f);
        float btnDiameter = 1.2f;
        float btnThickness = 0.06f;

        Color yellowBtn = new Color(0.95f, 0.78f, 0.12f);
        Color blackBorder = new Color(0.08f, 0.08f, 0.10f);

        // ── 黑色圆盘（边框，略大，在后面） ────────────────────
        CreateCylinder(_stageRoot, "PPE_ButtonBorder",
            btnPos + new Vector3(0f, 0f, 0.02f),
            new Vector3(btnDiameter + 0.25f, btnThickness * 0.7f, btnDiameter + 0.25f),
            blackBorder,
            new Vector3(90f, 0f, 0f));

        // ── 黄色圆盘（按钮主体，也是交互热区） ────────────────
        GameObject disc = CreateCylinder(_stageRoot, "PPE_ButtonDisc",
            btnPos,
            new Vector3(btnDiameter, btnThickness, btnDiameter),
            yellowBtn,
            new Vector3(90f, 0f, 0f));
        _circleBtnDiscRenderer = disc.GetComponent<Renderer>();

        // ── 记录圆形交互参数 ──────────────────────────────────
        _circleBtnCenter = btnPos;
        _circleBtnRadius = btnDiameter * 0.5f;   // 半径 = 直径 / 2
        _circleBtnIdleColor = yellowBtn;
        _circleBtnHoverColor = new Color(1f, 0.85f, 0.22f);
        _circleBtnPressedColor = new Color(0.2f, 0.85f, 0.35f);

        _circleHovering = false;
        _circleArmed = false;
        _circleReady = false;
        _circlePressed = false;
        _circleHasLastPoint = false;
        _circleLastPressTime = -99f;

        // ── 按钮中心文字 "确认" ───────────────────────────────
        CreateText(_stageRoot, "PPE_ButtonLabel",
            btnPos + new Vector3(0f, -0.03f, -0.14f),
            "确  认",
            40, 0.040f,
            new Color(0.1f, 0.1f, 0.1f, 1f),
            TextAnchor.MiddleCenter);
    }

    /// <summary>
    /// 步骤 9 按钮：圆形红色急停按钮（黑边），位于面板中央。
    /// 视觉 = 黑色大圆盘 + 红色圆盘叠加；交互 = 自定义圆形点击检测。
    /// </summary>
    void BuildEStopButton()
    {
        Vector3 btnPos = new Vector3(0f, 0.25f, -0.28f);
        float btnDiameter = 1.5f;          // 比 PPE 按钮更大，突出急停
        float btnThickness = 0.08f;

        Color eStopRed = new Color(0.88f, 0.10f, 0.08f);     // 急停红
        Color darkBorder = new Color(0.06f, 0.06f, 0.07f);

        // ── 黑色圆盘（边框，略大，在后面） ────────────────────
        CreateCylinder(_stageRoot, "EStop_Border",
            btnPos + new Vector3(0f, 0f, 0.02f),
            new Vector3(btnDiameter + 0.30f, btnThickness * 0.7f, btnDiameter + 0.30f),
            darkBorder,
            new Vector3(90f, 0f, 0f));

        // ── 红色圆盘（按钮主体，也是交互热区） ────────────────
        GameObject disc = CreateCylinder(_stageRoot, "EStop_Disc",
            btnPos,
            new Vector3(btnDiameter, btnThickness, btnDiameter),
            eStopRed,
            new Vector3(90f, 0f, 0f));
        _circleBtnDiscRenderer = disc.GetComponent<Renderer>();

        // ── 记录圆形交互参数 ──────────────────────────────────
        _circleBtnCenter = btnPos;
        _circleBtnRadius = btnDiameter * 0.5f;
        _circleBtnIdleColor = eStopRed;
        _circleBtnHoverColor = new Color(1f, 0.20f, 0.15f);      // 悬停：更亮的红色
        _circleBtnPressedColor = new Color(0.35f, 0.85f, 0.30f); // 按下：绿色反馈

        _circleHovering = false;
        _circleArmed = false;
        _circleReady = false;
        _circlePressed = false;
        _circleHasLastPoint = false;
        _circleLastPressTime = -99f;

        // ── 按钮中心文字 "急停" ───────────────────────────────
        CreateText(_stageRoot, "EStop_Label",
            btnPos + new Vector3(0f, -0.03f, -0.14f),
            "急  停",
            44, 0.044f,
            new Color(1f, 0.95f, 0.92f, 1f),
            TextAnchor.MiddleCenter);
    }

    /// <summary>
    /// 步骤 10 按钮：圆形绿色提交按钮，位于机器前面板上。
    /// 视觉 = 深绿底座 + 绿色圆盘叠加；交互 = 自定义圆形点击检测。
    /// </summary>
    void BuildShutdownButton()
    {
        // 按钮放在机器前面板上，指示灯下方
        Vector3 btnPos = new Vector3(0f, 0.1f, -0.70f);
        float btnDiameter = 0.5f;
        float btnThickness = 0.06f;

        Color greenBtn = new Color(0.15f, 0.78f, 0.25f);
        Color darkBorder = new Color(0.04f, 0.18f, 0.06f);

        // ── 按钮底座（深色圆盘，略大，在后面） ────────────────
        CreateCylinder(_stageRoot, "Shutdown_Border",
            btnPos + new Vector3(0f, 0f, 0.015f),
            new Vector3(btnDiameter + 0.20f, btnThickness * 0.8f, btnDiameter + 0.20f),
            darkBorder,
            new Vector3(90f, 0f, 0f));

        // ── 绿色按钮主体（交互热区） ──────────────────────────
        GameObject disc = CreateCylinder(_stageRoot, "Shutdown_Disc",
            btnPos,
            new Vector3(btnDiameter, btnThickness, btnDiameter),
            greenBtn,
            new Vector3(90f, 0f, 0f));
        _circleBtnDiscRenderer = disc.GetComponent<Renderer>();

        // ── 记录圆形交互参数 ──────────────────────────────────
        _circleBtnCenter = btnPos;
        _circleBtnRadius = btnDiameter * 0.5f;
        _circleBtnIdleColor = greenBtn;
        _circleBtnHoverColor = new Color(0.22f, 1f, 0.35f);
        _circleBtnPressedColor = new Color(0.2f, 0.5f, 0.95f);

        _circleHovering = false;
        _circleArmed = false;
        _circleReady = false;
        _circlePressed = false;
        _circleHasLastPoint = false;
        _circleLastPressTime = -99f;

        // ── 按钮上文字 "提交" ──────────────────────────────────
        CreateText(_stageRoot, "Shutdown_Label",
            btnPos + new Vector3(0f, -0.025f, -0.12f),
            "提  交",
            32, 0.032f,
            new Color(0.06f, 0.18f, 0.06f, 1f),
            TextAnchor.MiddleCenter);
    }

    /// <summary>
    /// 步骤 3/7 按钮：圆形蓝色确认按钮，位于数字屏下方。
    /// 使用自定义圆形点击检测（同步骤 1/9/10）。
    /// </summary>
    void BuildGaugeButton()
    {
        Vector3 btnPos = new Vector3(0f, -1.32f, -0.28f);
        float btnDiameter = 0.85f;
        float btnThickness = 0.06f;

        Color blueBtn = new Color(0.12f, 0.58f, 0.92f);
        Color darkBorder = new Color(0.04f, 0.14f, 0.28f);

        // ── 深色底座 ──────────────────────────────────────────
        CreateCylinder(_stageRoot, "Gauge_BtnBorder",
            btnPos + new Vector3(0f, 0f, 0.015f),
            new Vector3(btnDiameter + 0.20f, btnThickness * 0.8f, btnDiameter + 0.20f),
            darkBorder, new Vector3(90f, 0f, 0f));

        // ── 蓝色按钮主体（交互热区） ──────────────────────────
        GameObject disc = CreateCylinder(_stageRoot, "Gauge_BtnDisc",
            btnPos,
            new Vector3(btnDiameter, btnThickness, btnDiameter),
            blueBtn, new Vector3(90f, 0f, 0f));
        _circleBtnDiscRenderer = disc.GetComponent<Renderer>();

        // ── 记录圆形交互参数 ──────────────────────────────────
        _circleBtnCenter = btnPos;
        _circleBtnRadius = btnDiameter * 0.5f;
        _circleBtnIdleColor = blueBtn;
        _circleBtnHoverColor = new Color(0.20f, 0.72f, 1f);
        _circleBtnPressedColor = new Color(0.2f, 0.85f, 0.35f);  // 绿色按下反馈

        _circleHovering = false;
        _circleArmed = false;
        _circleReady = false;
        _circlePressed = false;
        _circleHasLastPoint = false;
        _circleLastPressTime = -99f;

        // ── 按钮文字 "确认" ───────────────────────────────────
        CreateText(_stageRoot, "Gauge_BtnLabel",
            btnPos + new Vector3(0f, -0.03f, -0.12f),
            "确  认",
            36, 0.036f,
            new Color(0.08f, 0.20f, 0.40f, 1f),
            TextAnchor.MiddleCenter);
    }

    /// <summary>
    /// 默认按钮：深色矩形按钮（步骤 5 流量计）。
    /// </summary>
    void BuildDefaultButton()
    {
        GameObject btnGo = new GameObject("PipelineGestureConfirmButton");
        btnGo.transform.SetParent(_stageRoot, false);

        _confirmButton = btnGo.AddComponent<FingertipTapButton>();
        _confirmButton.hand = _hand;
        _confirmButton.requireFreeHand = false;
        _confirmButton.showGuideLine = true;
        _confirmButton.Build(
            new Vector3(0f, -0.55f, -0.28f),
            new Vector3(2.2f, 0.55f, 0.12f),
            "确  认",
            _confirmBtnColor);
        _confirmButton.Clicked += HandleConfirmClicked;
    }

    /// <summary>
    /// 步骤 5 按钮：方形蓝色确认按钮（使用 FingertipTapButton，矩形交互热区）。
    /// 正方形外观（宽 = 高），与流量计下方的方形底座呼应。
    /// </summary>
    void BuildFlowMeterButton()
    {
        GameObject btnGo = new GameObject("PipelineGestureConfirmButton");
        btnGo.transform.SetParent(_stageRoot, false);

        _confirmButton = btnGo.AddComponent<FingertipTapButton>();
        _confirmButton.hand = _hand;
        _confirmButton.requireFreeHand = false;
        _confirmButton.showGuideLine = true;
        _confirmButton.Build(
            new Vector3(0f, -1.15f, -0.28f),
            new Vector3(1.15f, 1.15f, 0.12f),   // 正方形：宽 = 高
            "确  认",
            new Color(0.12f, 0.58f, 0.92f));     // 蓝色
        _confirmButton.Clicked += HandleConfirmClicked;
    }

    // ═══════════════════════════════════════════════════════════════
    //  ★ 文字搭建 — 按步骤分发
    // ═══════════════════════════════════════════════════════════════

    void BuildTexts()
    {
        switch (_step)
        {
            case PipelineTrainingManager.PipelineStep.PPECheck:
                BuildPPETexts();
                break;
            case PipelineTrainingManager.PipelineStep.ReadInitialPressure:
            case PipelineTrainingManager.PipelineStep.CheckMidPressure:
                BuildGaugeTexts();
                break;
            case PipelineTrainingManager.PipelineStep.MonitorFlowMeter:
                BuildFlowMeterTexts();
                break;
            case PipelineTrainingManager.PipelineStep.EmergencyStopTest:
                BuildEStopTexts();
                break;
            case PipelineTrainingManager.PipelineStep.SystemShutdown:
                BuildShutdownTexts();
                break;
            case PipelineTrainingManager.PipelineStep.OpenInletValve:
            case PipelineTrainingManager.PipelineStep.AdjustControlValve:
            case PipelineTrainingManager.PipelineStep.OpenOutletValve:
                BuildValveTexts();
                break;
            default:
                BuildDefaultTexts();
                break;
        }
    }

    /// <summary>
    /// 步骤 1 文字：标题（白色） + PPE 清单（黑色，在白板上） + 状态提示。
    /// </summary>
    void BuildPPETexts()
    {
        // ── 标题（白板顶部上方） ──────────────────────────────
        CreateText(_stageRoot, "PPE_Title",
            new Vector3(0f, 2.15f, -0.18f),
            _stepDisplayName + " — 手势操作训练",
            44, 0.042f,
            Color.white,
            TextAnchor.MiddleCenter);
        _titleText = _stageRoot.Find("PPE_Title")?.GetComponent<TextMesh>();

        // ── PPE 清单（白板中下部，黑色文字） ──────────────────
        //   在白色面板上以黑色文字列出待确认的装备项
        Color checklistColor = new Color(0.12f, 0.12f, 0.14f); // 近黑色
        string[] items = { "☐  安全帽", "☐  防护手套", "☐  护目镜", "☐  防护服", "☐  安全鞋" };
        float startY = -0.15f;
        float lineSpacing = 0.25f;

        for (int i = 0; i < items.Length; i++)
        {
            CreateText(_stageRoot, "PPE_CheckItem_" + i,
                new Vector3(0f, startY - i * lineSpacing, -0.22f),
                items[i],
                36, 0.036f,
                checklistColor,
                TextAnchor.MiddleCenter);
        }

        // ── 状态提示（白板下方） ──────────────────────────────
        CreateText(_stageRoot, "PPE_Status",
            new Vector3(0f, -1.65f, -0.22f),
            "",
            32, 0.032f,
            new Color(0.5f, 0.55f, 0.6f),
            TextAnchor.MiddleCenter);
        _statusText = _stageRoot.Find("PPE_Status")?.GetComponent<TextMesh>();
    }

    /// <summary>
    /// 步骤 9 文字：标题 + 警示标语 + 状态提示（红色警示风格）。
    /// </summary>
    void BuildEStopTexts()
    {
        // ── 标题（面板顶部上方） ──────────────────────────────
        CreateText(_stageRoot, "EStop_Title",
            new Vector3(0f, 2.15f, -0.18f),
            _stepDisplayName + " — 手势操作训练",
            44, 0.042f,
            new Color(1f, 0.85f, 0.80f),   // 暖白色标题
            TextAnchor.MiddleCenter);
        _titleText = _stageRoot.Find("EStop_Title")?.GetComponent<TextMesh>();

        // ── 警示标语（面板中上部，按钮上方） ──────────────────
        CreateText(_stageRoot, "EStop_Warning",
            new Vector3(0f, 1.45f, -0.22f),
            "⚠  紧 急 停 止  ⚠",
            52, 0.050f,
            new Color(1f, 0.25f, 0.18f),   // 亮红色警告
            TextAnchor.MiddleCenter);

        // ── 说明文字 ──────────────────────────────────────────
        CreateText(_stageRoot, "EStop_Instruction",
            new Vector3(0f, -0.82f, -0.22f),
            "请在摄像头前用手指点击下方红色急停按钮",
            32, 0.032f,
            new Color(0.85f, 0.80f, 0.78f),
            TextAnchor.MiddleCenter);

        // ── 状态提示（面板下方） ──────────────────────────────
        CreateText(_stageRoot, "EStop_Status",
            new Vector3(0f, -1.65f, -0.22f),
            "",
            32, 0.032f,
            new Color(0.85f, 0.70f, 0.65f),
            TextAnchor.MiddleCenter);
        _statusText = _stageRoot.Find("EStop_Status")?.GetComponent<TextMesh>();
    }

    /// <summary>
    /// 步骤 10 文字：标题 + 说明 + 状态提示（绿色关停风格）。
    /// </summary>
    void BuildShutdownTexts()
    {
        // ── 标题（机器后上方，背景墙前） ──────────────────────
        CreateText(_stageRoot, "Shutdown_Title",
            new Vector3(0f, 1.95f, -0.15f),
            _stepDisplayName + " — 手势操作训练",
            42, 0.040f,
            new Color(0.78f, 1f, 0.82f),
            TextAnchor.MiddleCenter);
        _titleText = _stageRoot.Find("Shutdown_Title")?.GetComponent<TextMesh>();

        // ── 机器上的 "提交记录" 标识（前面板右下） ────────────
        CreateText(_stageRoot, "Shutdown_MachineLabel",
            new Vector3(0.45f, -0.33f, -0.72f),
            "提交记录",
            30, 0.030f,
            new Color(0.75f, 0.90f, 0.78f),
            TextAnchor.MiddleLeft);

        // ── 操作说明（机器上方） ──────────────────────────────
        CreateText(_stageRoot, "Shutdown_Instruction",
            new Vector3(0f, 1.5f, -0.22f),
            "请用手指点击机器面板上的绿色按钮\n完成系统关停提交",
            32, 0.032f,
            new Color(0.80f, 0.92f, 0.82f),
            TextAnchor.MiddleCenter);

        // ── 状态提示（桌面下方） ──────────────────────────────
        CreateText(_stageRoot, "Shutdown_Status",
            new Vector3(0f, -1.2f, -0.22f),
            "",
            30, 0.030f,
            new Color(0.75f, 0.88f, 0.78f),
            TextAnchor.MiddleCenter);
        _statusText = _stageRoot.Find("Shutdown_Status")?.GetComponent<TextMesh>();
    }

    /// <summary>
    /// 步骤 3/7 文字：标题 + 读数说明 + 状态提示。
    /// </summary>
    void BuildGaugeTexts()
    {
        // ── 标题（面板顶部上方） ──────────────────────────────
        CreateText(_stageRoot, "Gauge_Title",
            new Vector3(0f, 2.15f, -0.18f),
            _stepDisplayName + " — 手势操作训练",
            44, 0.042f,
            new Color(0.85f, 0.90f, 0.95f),
            TextAnchor.MiddleCenter);
        _titleText = _stageRoot.Find("Gauge_Title")?.GetComponent<TextMesh>();

        // ── 操作说明 ──────────────────────────────────────────
        string gaugeId = _step == PipelineTrainingManager.PipelineStep.ReadInitialPressure
            ? "P1" : "P2";
        CreateText(_stageRoot, "Gauge_Instruction",
            new Vector3(0f, 1.52f, -0.18f),
            "请确认 " + gaugeId + " 压力表读数\n用手指点击下方蓝色【确认】按钮",
            32, 0.032f,
            new Color(0.72f, 0.80f, 0.88f),
            TextAnchor.MiddleCenter);

        // ── 状态提示 ──────────────────────────────────────────
        CreateText(_stageRoot, "Gauge_Status",
            new Vector3(0f, -1.90f, -0.22f),
            "",
            30, 0.030f,
            new Color(0.70f, 0.80f, 0.88f),
            TextAnchor.MiddleCenter);
        _statusText = _stageRoot.Find("Gauge_Status")?.GetComponent<TextMesh>();
    }

    /// <summary>
    /// 默认文字：标题 + 可选仪表读数 + 状态提示（步骤 5 流量计，深色面板用，文字亮色）。
    /// </summary>
    void BuildDefaultTexts()
    {
        CreateText(_stageRoot, "Default_Title",
            new Vector3(0f, 1.9f, -0.18f),
            _stepDisplayName + " — 手势操作训练",
            46, 0.044f,
            Color.white,
            TextAnchor.MiddleCenter);
        _titleText = _stageRoot.Find("Default_Title")?.GetComponent<TextMesh>();

        // 仪表读数（步骤 3/5/7）
        if (!string.IsNullOrEmpty(_gaugeValueDisplay))
        {
            CreateText(_stageRoot, "Default_GaugeValue",
                new Vector3(0f, 0.8f, -0.20f),
                "当前读数：" + _gaugeValueDisplay,
                42, 0.042f,
                new Color(1f, 0.85f, 0.1f),
                TextAnchor.MiddleCenter);
            _gaugeText = _stageRoot.Find("Default_GaugeValue")?.GetComponent<TextMesh>();
        }

        CreateText(_stageRoot, "Default_Status",
            new Vector3(0f, 1.2f, -0.20f),
            "",
            34, 0.034f,
            new Color(0.78f, 0.90f, 1f),
            TextAnchor.MiddleCenter);
        _statusText = _stageRoot.Find("Default_Status")?.GetComponent<TextMesh>();
    }

    /// <summary>
    /// 步骤 5 文字：标题 + 操作说明 + 状态提示（电子流量计面板风格）。
    /// </summary>
    void BuildFlowMeterTexts()
    {
        // ── 标题（面板顶部上方） ──────────────────────────────
        CreateText(_stageRoot, "FlowMeter_Title",
            new Vector3(0f, 2.15f, -0.18f),
            _stepDisplayName + " — 手势操作训练",
            44, 0.042f,
            new Color(0.85f, 0.90f, 0.95f),
            TextAnchor.MiddleCenter);
        _titleText = _stageRoot.Find("FlowMeter_Title")?.GetComponent<TextMesh>();

        // ── 操作说明 ──────────────────────────────────────────
        CreateText(_stageRoot, "FlowMeter_Instruction",
            new Vector3(0f, 1.52f, -0.18f),
            "请观察 F1 电子流量计的读数\n确认流量正常后，用手指点击下方方形【确认】按钮",
            32, 0.032f,
            new Color(0.72f, 0.80f, 0.88f),
            TextAnchor.MiddleCenter);

        // ── 状态提示 ──────────────────────────────────────────
        CreateText(_stageRoot, "FlowMeter_Status",
            new Vector3(0f, -1.65f, -0.22f),
            "",
            30, 0.030f,
            new Color(0.70f, 0.80f, 0.88f),
            TextAnchor.MiddleCenter);
        _statusText = _stageRoot.Find("FlowMeter_Status")?.GetComponent<TextMesh>();
    }

    /// <summary>
    /// 步骤 4/6/8 文字：标题 + 操作说明 + 状态提示（阀门手轮旋转风格）。
    /// 不创建确认按钮文字，角度显示由 OnGUI 左上角处理。
    /// </summary>
    void BuildValveTexts()
    {
        bool isStep6 = _step == PipelineTrainingManager.PipelineStep.AdjustControlValve;

        // ── 标题（管道上方） ──────────────────────────────────
        CreateText(_stageRoot, "Valve_Title",
            new Vector3(0f, 1.8f, -0.22f),
            _stepDisplayName + " — 手势操作训练",
            44, 0.042f,
            new Color(0.85f, 0.90f, 0.95f),
            TextAnchor.MiddleCenter);
        _titleText = _stageRoot.Find("Valve_Title")?.GetComponent<TextMesh>();

        // ── 操作说明（管道下方偏上） ──────────────────────────
        string valveName = _step == PipelineTrainingManager.PipelineStep.OpenInletValve ? "V1 (进口)"
            : _step == PipelineTrainingManager.PipelineStep.AdjustControlValve ? "V2 (控制)"
            : "V3 (出口)";

        if (isStep6)
        {
            // 步骤 6：无固定角度目标，目标是将流量调节至 35 L/min
            CreateText(_stageRoot, "Valve_Instruction",
                new Vector3(0f, 1.35f, -0.22f),
                "请用手势抓握 " + valveName + " 阀门手轮\n逆时针增大流量，顺时针减小流量\n步骤五：流量目标范围：20 ~ 50 L/min\n步骤六：流量目标范围：35 ± 5 L/min",
                30, 0.030f,
                new Color(0.72f, 0.80f, 0.88f),
                TextAnchor.MiddleCenter);

            // ── F1 流量读数（大字体，醒目，管道下方） ──────────
            float initFlow = _flowMaxValue > 0f
                ? Mathf.Clamp01(_valveAngle / 720f) * _flowMaxValue
                : 0f;
            CreateText(_stageRoot, "Valve_FlowDisplay",
                new Vector3(0f, -0.90f, -0.25f),
                "F1: " + initFlow.ToString("F1") + " L/min",
                56, 0.054f,
                new Color(0.18f, 1f, 0.35f),
                TextAnchor.MiddleCenter);
            _flowDisplayText = _stageRoot.Find("Valve_FlowDisplay")?.GetComponent<TextMesh>();

            // ── 当前角度文字（辅助信息，放在流量下方） ──────────
            CreateText(_stageRoot, "Valve_AngleDisplay",
                new Vector3(0f, -1.35f, -0.25f),
                _valveAngle.ToString("F1") + "°",
                36, 0.036f,
                new Color(0.65f, 0.70f, 0.78f),
                TextAnchor.MiddleCenter);
            _valveAngleText = _stageRoot.Find("Valve_AngleDisplay")?.GetComponent<TextMesh>();
        }
        else
        {
            CreateText(_stageRoot, "Valve_Instruction",
                new Vector3(0f, 1.35f, -0.22f),
                "请用手势抓握 " + valveName + " 阀门手轮\n逆时针旋转至 " + _valveTargetAngle.ToString("F0") + "° 目标",
                32, 0.032f,
                new Color(0.72f, 0.80f, 0.88f),
                TextAnchor.MiddleCenter);

            // ── 当前角度文字（屏幕中央偏下，大字体醒目） ──────────
            CreateText(_stageRoot, "Valve_AngleDisplay",
                new Vector3(0f, -1.25f, -0.25f),
                _valveAngle.ToString("F1") + "°",
                56, 0.054f,
                new Color(1f, 0.85f, 0.1f),
                TextAnchor.MiddleCenter);
            _valveAngleText = _stageRoot.Find("Valve_AngleDisplay")?.GetComponent<TextMesh>();
        }

        // ── 状态提示（底部） ──────────────────────────────────
        CreateText(_stageRoot, "Valve_Status",
            new Vector3(0f, -1.80f, -0.22f),
            "",
            28, 0.028f,
            new Color(0.70f, 0.80f, 0.88f),
            TextAnchor.MiddleCenter);
        _statusText = _stageRoot.Find("Valve_Status")?.GetComponent<TextMesh>();
    }

    // ═══════════════════════════════════════════════════════════════
    //  交互回调
    // ═══════════════════════════════════════════════════════════════

    void HandleConfirmClicked()
    {
        if (_completed) return;
        _completed = true;
        _completeTime = Time.time;

        if (_confirmButton != null)
            _confirmButton.interactable = false;

        if (_statusText != null)
        {
            _statusText.text = "✓ " + _stepDisplayName + " 已完成！\n即将返回训练场景...";
            _statusText.color = new Color(0.2f, 1f, 0.4f);
        }

        Debug.Log("[PipelineGestureTraining] " + _stepDisplayName + " 手势训练完成");
    }

    // ═══════════════════════════════════════════════════════════════
    //  Update
    // ═══════════════════════════════════════════════════════════════

    void Update()
    {
        if (!_initialized) return;

        // ── 首次：将手部所有子对象的 layer 同步为 HandOverlayLayer ──
        //     HandVisual.Start() → Build() 在首帧前创建关节/骨骼子对象，
        //     它们不继承父级 layer，需手动递归设置。
        if (!_handLayerApplied && _handVisual != null && _handVisual.transform.childCount > 0)
        {
            SetLayerRecursive(_handVisual.transform, HandOverlayLayer);
            _handLayerApplied = true;
        }

        // 紧急退出：按 R 键取消手势训练（返回初始位置，但保存阀门角度）
        if (Input.GetKeyDown(KeyCode.R) && !_completed)
        {
            Debug.Log("[PipelineGestureTraining] 用户按 R 取消手势训练（阀门角度: " + _valveAngle + "°）");
            RestoreMainCameraState();
            _onComplete?.Invoke(false);
            Destroy(gameObject);
            return;
        }

        // 按 T 键：取消手势训练（回到 F 交互前所在位置，但保存阀门角度）
        if (Input.GetKeyDown(KeyCode.T) && !_completed)
        {
            Debug.Log("[PipelineGestureTraining] 用户按 T 取消手势训练（阀门角度: " + _valveAngle + "°）");
            RestoreMainCameraState();
            _onComplete?.Invoke(false);
            Destroy(gameObject);
            return;
        }

        // ★ 步骤 1/3/7/9/10 用自定义圆形点击检测；其他步骤用 FingertipTapButton
        if (_step == PipelineTrainingManager.PipelineStep.PPECheck
            || _step == PipelineTrainingManager.PipelineStep.ReadInitialPressure
            || _step == PipelineTrainingManager.PipelineStep.CheckMidPressure
            || _step == PipelineTrainingManager.PipelineStep.EmergencyStopTest
            || _step == PipelineTrainingManager.PipelineStep.SystemShutdown)
            UpdateCircularButton();

        // 压力表步骤：持续更新指针（平滑旋转）
        if (_step == PipelineTrainingManager.PipelineStep.ReadInitialPressure
            || _step == PipelineTrainingManager.PipelineStep.CheckMidPressure)
            UpdateGaugePointer();

        // 阀门步骤：手势抓握旋转交互
        if (_step == PipelineTrainingManager.PipelineStep.OpenInletValve
            || _step == PipelineTrainingManager.PipelineStep.AdjustControlValve
            || _step == PipelineTrainingManager.PipelineStep.OpenOutletValve)
            UpdateValveInteraction();

        UpdateCursor();
        UpdateStatusText();

        if (_completed && Time.time - _completeTime >= ExitDelay)
        {
            ExitTraining();
        }
    }

    /// <summary>
    /// 阀门手势旋转交互（步骤 4/6/8）。
    /// 状态机：Idle → Hover（手靠近手轮）→ Grabbed（抓握旋转）→ Complete（达标）
    /// </summary>
    void UpdateValveInteraction()
    {
        if (_completed || _valveHandwheelPivot == null) return;

        bool isStep6 = _step == PipelineTrainingManager.PipelineStep.AdjustControlValve;
        bool hasTarget = _valveTargetAngle > 0f;

        // ── 步骤 6：实时计算并更新流量读数 ─────────────────
        if (isStep6 && _flowMaxValue > 0f)
        {
            float v2Open = Mathf.Clamp01(_valveAngle / 720f);
            float currentFlow = v2Open * _flowMaxValue;
            if (_flowDisplayText != null)
            {
                _flowDisplayText.text = "F1: " + currentFlow.ToString("F1") + " L/min";
                // 颜色：35±5 为绿色，偏低黄色，偏高红色
                if (currentFlow >= 30f && currentFlow <= 40f)
                    _flowDisplayText.color = new Color(0.2f, 1f, 0.4f);
                else if (currentFlow < 30f)
                    _flowDisplayText.color = new Color(1f, 0.85f, 0.2f);
                else
                    _flowDisplayText.color = new Color(1f, 0.35f, 0.25f);
            }
        }

        // 角度文本实时更新
        if (_valveAngleText != null)
        {
            if (isStep6)
                _valveAngleText.text = "V2 开度: " + _valveAngle.ToString("F1") + "°";
            else
                _valveAngleText.text = _valveAngle.ToString("F1") + "°";
        }

        // 已完成目标（仅当有目标时），显示 ✓ 反馈但仍可继续旋转
        if (_valveCompleted && hasTarget)
        {
            if (_valveAngleText != null)
            {
                _valveAngleText.text = "✓ " + _valveAngle.ToString("F1") + "° / " + _valveTargetAngle.ToString("F0") + "°";
                _valveAngleText.color = new Color(0.2f, 1f, 0.4f);
            }
            // 不 return —— 允许继续旋转，不设角度上限
        }

        // 无手时重置
        if (_hand == null || !_hand.IsActive)
        {
            if (_valveGrabbed)
            {
                _valveGrabbed = false;
                SetValveWheelColor(_valveWheelIdleColor);
            }
            return;
        }

        // 获取食指尖端 XY 坐标
        Vector3 point;
        if (_hand.Points != null && _hand.Points.Length > 8)
            point = _hand.Points[8];
        else
            point = _hand.GripPoint;

        // 计算指尖到手轮中心的 XY 距离
        Vector2 pointXY = new Vector2(point.x, point.y);
        Vector2 centerXY = new Vector2(_valveWheelCenter.x, _valveWheelCenter.y);
        float dist = Vector2.Distance(pointXY, centerXY);
        bool inWheel = dist <= _valveWheelRadius;

        // ── 状态机 ─────────────────────────────────────────────
        if (!_valveGrabbed)
        {
            // ── Idle / Hover ───────────────────────────────────
            if (!inWheel)
            {
                SetValveWheelColor(_valveWheelIdleColor);
                return;
            }

            // 手进入手轮范围 → Hover
            SetValveWheelColor(_valveWheelHoverColor);

            // 检测抓握：手在手轮范围内 + 捏合力度足够
            float pinch = _hand.PinchOnlyStrength;
            if (pinch > 0.35f)
            {
                _valveGrabbed = true;
                _valveLastHandAngle = Mathf.Atan2(point.y - _valveWheelCenter.y, point.x - _valveWheelCenter.x);
                SetValveWheelColor(_valveWheelGrabbedColor);
                Debug.Log("[PipelineGestureTraining] 抓握阀门手轮 角度=" + _valveAngle.ToString("F1") + "°");
            }
        }
        else
        {
            // ── Grabbed ─────────────────────────────────────────
            // 检测释放：捏合力度不足 或 手离开手轮范围太远
            float pinch = _hand.PinchOnlyStrength;
            if (pinch < 0.2f || dist > _valveWheelRadius * 1.6f)
            {
                _valveGrabbed = false;
                SetValveWheelColor(inWheel ? _valveWheelHoverColor : _valveWheelIdleColor);
                Debug.Log("[PipelineGestureTraining] 释放阀门手轮 角度=" + _valveAngle.ToString("F1") + "°");
                return;
            }

            // 计算手部当前角度（相对于手轮中心）
            float currentHandAngle = Mathf.Atan2(point.y - _valveWheelCenter.y, point.x - _valveWheelCenter.x);

            // 计算角度增量（逆时针 = 角度增加 = 正值）
            float delta = Mathf.DeltaAngle(_valveLastHandAngle * Mathf.Rad2Deg, currentHandAngle * Mathf.Rad2Deg);
            _valveLastHandAngle = currentHandAngle;

            // 累加角度（逆时针为正，顺时针为负）— 但不低于 0
            _valveAngle += delta;
            _valveAngle = Mathf.Max(0f, _valveAngle);

            // 更新手轮视觉旋转
            UpdateValveHandwheelVisual(instant: true);

            // 检查是否达到目标（仅当有明确目标角度时，仅作视觉标记）
            if (hasTarget && _valveAngle >= _valveTargetAngle && !_valveCompleted)
            {
                _valveCompleted = true;

                if (_statusText != null)
                {
                    _statusText.text = "✓ 已达到 " + _valveTargetAngle.ToString("F0") + "° 目标！\n可继续旋转或按 T/R 返回大场景";
                    _statusText.color = new Color(0.2f, 1f, 0.4f);
                }

                Debug.Log("[PipelineGestureTraining] 阀门已达到目标角度 " + _valveTargetAngle.ToString("F0") + "°！当前=" + _valveAngle.ToString("F1") + "°（可继续操作）");
            }
        }
    }

    /// <summary>设置手轮所有 Renderer 的颜色。</summary>
    void SetValveWheelColor(Color color)
    {
        if (_valveWheelAllRenderers == null) return;
        foreach (var r in _valveWheelAllRenderers)
        {
            if (r != null) SetColor(r, color);
        }
    }

    /// <summary>
    /// 自定义圆形按钮点击检测（步骤 1/3/7/9/10）。
    /// 完全替代 FingertipTapButton，整个圆形区域都是交互热区，
    /// 无隐藏的 Cube 或矩形判定。
    ///
    /// 状态机：Armed（进入圆形）→ Ready（稳定悬停）→ Pressed（手指下按）→ Click
    /// </summary>
    void UpdateCircularButton()
    {
        if (_completed || _hand == null || !_hand.IsActive
            || _circleBtnDiscRenderer == null || _circleBtnRadius <= 0f)
        {
            // 无手时重置 + 恢复默认颜色
            if (_circleArmed || _circleHovering)
            {
                _circleArmed = false;
                _circleReady = false;
                _circleHovering = false;
                SetColor(_circleBtnDiscRenderer, _circleBtnIdleColor);
            }
            return;
        }

        // ── 1. 获取食指尖端 XY 坐标（与 FingertipTapButton 一致，使用 Points[8]） ──
        Vector3 point;
        if (_hand.Points != null && _hand.Points.Length > 8)
            point = _hand.Points[8];
        else
            point = _hand.GripPoint;

        Vector2 pointXY = new Vector2(point.x, point.y);
        Vector2 centerXY = new Vector2(_circleBtnCenter.x, _circleBtnCenter.y);
        float dist = Vector2.Distance(pointXY, centerXY);
        bool inCircle = dist <= _circleBtnRadius;

        // ── 2. 状态机 ─────────────────────────────────────────
        if (!inCircle)
        {
            // 手指离开圆形区域 → 完全重置
            if (_circleArmed || _circleHovering)
            {
                ResetCircularTap();
                SetColor(_circleBtnDiscRenderer, _circleBtnIdleColor);
            }
            return;
        }

        // ── 进入圆形（Armed） ──────────────────────────────────
        if (!_circleArmed)
        {
            _circleArmed = true;
            _circleReady = false;
            _circlePressed = false;
            _circleHovering = true;
            _circleHasLastPoint = true;
            _circleLastPoint = point;
            _circleHoverStartTime = Time.time;
            SetColor(_circleBtnDiscRenderer, _circleBtnHoverColor);
            return;
        }

        _circleHovering = true;

        // ── 稳定检测（Ready） ─────────────────────────────────
        if (!_circleReady)
        {
            // 手指还在漂移 → 重置计时
            Vector3 settleDelta = point - _circleLastPoint;
            if (Mathf.Abs(settleDelta.x) > CircleTapStabilizeMaxDrift
                || Mathf.Abs(settleDelta.y) > CircleTapStabilizeMaxDrift)
            {
                _circleHoverStartTime = Time.time;
                _circleLastPoint = point;
                SetColor(_circleBtnDiscRenderer, _circleBtnHoverColor);
                return;
            }

            // 稳定时间不够
            if (Time.time - _circleHoverStartTime < CircleTapReadySeconds)
            {
                _circleLastPoint = point;
                SetColor(_circleBtnDiscRenderer, _circleBtnHoverColor);
                return;
            }

            // 稳定就绪！
            _circleReady = true;
            _circleLastPoint = point;
            SetColor(_circleBtnDiscRenderer,
                Color.Lerp(_circleBtnHoverColor, Color.white, 0.3f)); // 更亮，提示可以点按
            return;
        }

        // ── 按下检测（Pressed → Click） ───────────────────────
        float dt = Mathf.Max(Time.deltaTime, 1e-4f);

        // 如果手指上升，更新"最高点"（用于计算下按距离）
        if (!_circlePressed && point.y > _circleLastPoint.y)
            _circleLastPoint = point;

        float downDistance = _circleLastPoint.y - point.y;
        float downSpeed = Mathf.Max(0f, -(point.y - _circleLastPoint.y) / dt);

        // 正在按下中（还未释放）
        if (_circlePressed)
        {
            if (downDistance <= CircleTapMinDownDelta * 0.25f)
                _circlePressed = false;  // "释放"
            else
                return;                   // 仍在按着
        }

        bool tapDown = (downDistance >= CircleTapMinDownDelta
                        && downSpeed >= CircleTapMinDownSpeed)
                    || downDistance >= CircleTapMinDownDelta * 1.6f;  // 慢但够深也算
        bool notInCooldown = Time.time - _circleLastPressTime >= CircleTapCooldown;

        if (tapDown && !_circlePressed && notInCooldown)
        {
            _circlePressed = true;
            _circleLastPressTime = Time.time;

            // 视觉反馈：按下颜色
            SetColor(_circleBtnDiscRenderer, _circleBtnPressedColor);

            // ★ 触发点击！
            HandleConfirmClicked();
            return;
        }

        // 持续就绪状态
        SetColor(_circleBtnDiscRenderer,
            Color.Lerp(_circleBtnHoverColor, Color.white, 0.3f));
    }

    void ResetCircularTap()
    {
        _circleArmed = false;
        _circleReady = false;
        _circlePressed = false;
        _circleHovering = false;
        _circleHasLastPoint = false;
        _circleHoverStartTime = -99f;
    }

    void UpdateCursor()
    {
        if (_cursor == null || _hand == null) return;

        bool active = _hand.IsActive;
        _cursor.SetActive(active);
        if (!active) return;

        Vector3 grip = _hand.GripPoint + new Vector3(0f, 0f, -0.08f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.11f, 0.22f, _hand.PinchOnlyStrength);

        Color cursorColor = new Color(0.18f, 0.66f, 1f);

        // 步骤 1/3/7/9/10：圆形按钮距离判定
        if ((_step == PipelineTrainingManager.PipelineStep.PPECheck
             || _step == PipelineTrainingManager.PipelineStep.ReadInitialPressure
             || _step == PipelineTrainingManager.PipelineStep.CheckMidPressure
             || _step == PipelineTrainingManager.PipelineStep.EmergencyStopTest
             || _step == PipelineTrainingManager.PipelineStep.SystemShutdown)
            && _circleBtnRadius > 0f)
        {
            float dist = Vector2.Distance(
                new Vector2(grip.x, grip.y),
                new Vector2(_circleBtnCenter.x, _circleBtnCenter.y));
            if (dist < _circleBtnRadius)
            {
                // 按钮内光标颜色
                if (_step == PipelineTrainingManager.PipelineStep.EmergencyStopTest)
                    cursorColor = new Color(1f, 0.22f, 0.18f);     // 红色 — 急停
                else if (_step == PipelineTrainingManager.PipelineStep.SystemShutdown)
                    cursorColor = new Color(0.18f, 1f, 0.35f);     // 绿色 — 关停提交
                else if (_step == PipelineTrainingManager.PipelineStep.ReadInitialPressure
                      || _step == PipelineTrainingManager.PipelineStep.CheckMidPressure)
                    cursorColor = new Color(0.18f, 0.62f, 1f);     // 蓝色 — 压力表确认
                else
                    cursorColor = new Color(1f, 0.74f, 0.18f);     // 黄色 — PPE
            }
            else if (dist < _circleBtnRadius + 0.4f)
            {
                // 接近光标颜色
                if (_step == PipelineTrainingManager.PipelineStep.EmergencyStopTest)
                    cursorColor = new Color(1f, 0.55f, 0.50f);     // 浅红 — 接近急停
                else if (_step == PipelineTrainingManager.PipelineStep.SystemShutdown)
                    cursorColor = new Color(0.45f, 1f, 0.55f);     // 浅绿 — 接近关停
                else if (_step == PipelineTrainingManager.PipelineStep.ReadInitialPressure
                      || _step == PipelineTrainingManager.PipelineStep.CheckMidPressure)
                    cursorColor = new Color(0.35f, 0.72f, 1f);     // 浅蓝 — 接近压力表按钮
                else
                    cursorColor = new Color(0.6f, 0.85f, 1f);      // 浅蓝 — 接近 PPE
            }
        }
        // 阀门步骤：手轮距离判定
        else if ((_step == PipelineTrainingManager.PipelineStep.OpenInletValve
               || _step == PipelineTrainingManager.PipelineStep.AdjustControlValve
               || _step == PipelineTrainingManager.PipelineStep.OpenOutletValve)
              && _valveWheelRadius > 0f)
        {
            float dist = Vector2.Distance(
                new Vector2(grip.x, grip.y),
                new Vector2(_valveWheelCenter.x, _valveWheelCenter.y));
            if (dist < _valveWheelRadius)
            {
                // 手在手轮范围内
                bool isStep6 = _step == PipelineTrainingManager.PipelineStep.AdjustControlValve;
                cursorColor = _valveGrabbed
                    ? new Color(0.2f, 1f, 0.4f)                     // 绿色 — 正在抓握
                    : (isStep6
                        ? new Color(0.22f, 0.55f, 1f)               // 蓝色 — 步骤 6 可抓握
                        : new Color(1f, 0.45f, 0.22f));             // 橙红 — 可抓握
            }
            else if (dist < _valveWheelRadius + 0.5f)
            {
                bool isStep6 = _step == PipelineTrainingManager.PipelineStep.AdjustControlValve;
                cursorColor = isStep6
                    ? new Color(0.40f, 0.65f, 1f)                   // 浅蓝 — 步骤 6 接近手轮
                    : new Color(1f, 0.65f, 0.45f);                  // 浅橙 — 接近手轮
            }
        }
        // 其他步骤：FingertipTapButton 矩形距离判定
        else if (_confirmButton != null)
        {
            Vector3 btnCenter = _confirmButton.transform.position;
            float dist = Mathf.Abs(grip.x - btnCenter.x) + Mathf.Abs(grip.y - btnCenter.y);
            if (dist < 0.6f)
                cursorColor = new Color(1f, 0.74f, 0.18f);
            else if (dist < 1.2f)
                cursorColor = new Color(0.6f, 0.85f, 1f);
        }

        SetColor(_cursorRenderer, cursorColor);
    }

    void UpdateStatusText()
    {
        if (_statusText == null) return;
        if (_completed) return;

        string handState = (_hand != null && _hand.IsActive)
            ? "手势已连接 ✓"
            : "等待手势识别...（请检查摄像头与手势服务）";

        string instruction;
        bool isValve = _step == PipelineTrainingManager.PipelineStep.OpenInletValve
                    || _step == PipelineTrainingManager.PipelineStep.AdjustControlValve
                    || _step == PipelineTrainingManager.PipelineStep.OpenOutletValve;

        if (isValve)
        {
            bool isStep6 = _step == PipelineTrainingManager.PipelineStep.AdjustControlValve;
            bool hasTarget = _valveTargetAngle > 0f;

            if (isStep6)
            {
                // 步骤 6：无固定角度目标，以流量为准
                float v2Open = Mathf.Clamp01(_valveAngle / 720f);
                float currentFlow = _flowMaxValue > 0f ? v2Open * _flowMaxValue : 0f;
                string flowStatus = currentFlow >= 30f && currentFlow <= 40f
                    ? "✓ 流量正常" : "流量偏离目标范围（35 ± 5 L/min）";

                if (_valveGrabbed)
                    instruction = "正在旋转阀门... 当前流量: " + currentFlow.ToString("F1") + " L/min  " + flowStatus;
                else
                    instruction = " 当前: " + currentFlow.ToString("F1") + " L/min";
            }
            else if (_valveCompleted)
                instruction = "✓ 已达到 " + _valveTargetAngle.ToString("F0") + "° 目标！可继续操作或按 T/R 返回";
            else if (_valveGrabbed)
                instruction = "正在旋转阀门... 逆时针转动（目标: " + _valveTargetAngle.ToString("F0") + "°）";
            else
                instruction = "请用手势抓握红色手轮进行旋转（目标: " + _valveTargetAngle.ToString("F0") + "°）";
        }
        else if (string.IsNullOrEmpty(_gaugeValueDisplay))
        {
            instruction = "请用手指点击【确认】按钮";
        }
        else
        {
            instruction = "请确认读数后，用手指点击【确认】按钮";
        }

        _statusText.text = handState + "\n" + instruction;
    }

    // ═══════════════════════════════════════════════════════════════
    //  OnGUI — 左上角键盘提示
    // ═══════════════════════════════════════════════════════════════

    void OnGUI()
    {
        if (!_initialized || _completed) return;
        EnsureHintStyles();

        bool isValve = _step == PipelineTrainingManager.PipelineStep.OpenInletValve
                    || _step == PipelineTrainingManager.PipelineStep.AdjustControlValve
                    || _step == PipelineTrainingManager.PipelineStep.OpenOutletValve;

        float x = 18f;
        float y = 18f;
        float lineH = 28f;

        // ── 阀门步骤：当前状态信息（大字体，醒目） ────────────
        if (isValve)
        {
            bool isStep6 = _step == PipelineTrainingManager.PipelineStep.AdjustControlValve;
            bool hasTarget = _valveTargetAngle > 0f;

            GUIStyle infoBoxStyle = new GUIStyle(GUI.skin.box);
            infoBoxStyle.normal.background = MakeHintTexture(new Color(0.15f, 0.12f, 0.02f, 0.90f));
            infoBoxStyle.normal.textColor = Color.white;
            infoBoxStyle.padding = new RectOffset(0, 0, 0, 0);

            if (isStep6)
            {
                // 步骤 6：显示流量读数 + 角度
                float v2Open = Mathf.Clamp01(_valveAngle / 720f);
                float currentFlow = _flowMaxValue > 0f ? v2Open * _flowMaxValue : 0f;
                string flowColorTag = currentFlow >= 30f && currentFlow <= 40f ? "✓ " : "";

                string flowHint = "  F1 流量：" + flowColorTag + currentFlow.ToString("F1") + " L/min";
                Vector2 fSize = _hintLabelStyle.CalcSize(new GUIContent(flowHint));
                Rect fRect = new Rect(x, y, fSize.x + 20f, Mathf.Max(fSize.y + 8f, lineH + 6f));
                GUI.Box(fRect, "", infoBoxStyle);
                GUI.Label(fRect, flowHint, _hintLabelStyle);
                y += fRect.height + 4f;

                string angleHint = "  V2 开度：" + _valveAngle.ToString("F1") + "°";
                Vector2 aSize = _hintLabelStyle.CalcSize(new GUIContent(angleHint));
                Rect aRect = new Rect(x, y, aSize.x + 20f, Mathf.Max(aSize.y + 8f, lineH + 6f));
                GUI.Box(aRect, "", infoBoxStyle);
                GUI.Label(aRect, angleHint, _hintLabelStyle);
                y += aRect.height + 6f;
            }
            else
            {
                string angleHint = hasTarget
                    ? "  当前旋转角度：" + _valveAngle.ToString("F1") + "°  /  " + _valveTargetAngle.ToString("F0") + "°"
                    : "  当前旋转角度：" + _valveAngle.ToString("F1") + "°";
                Vector2 aSize = _hintLabelStyle.CalcSize(new GUIContent(angleHint));
                Rect aRect = new Rect(x, y, aSize.x + 20f, Mathf.Max(aSize.y + 8f, lineH + 6f));
                GUI.Box(aRect, "", infoBoxStyle);
                GUI.Label(aRect, angleHint, _hintLabelStyle);
                y += aRect.height + 6f;
            }
        }

        // ── R 键提示 ──────────────────────────────────────────
        string rHint = isValve
            ? "  R 键 — 保存角度，返回初始位置"
            : "  R 键 — 取消操作，返回初始位置";
        Vector2 rSize = _hintLabelStyle.CalcSize(new GUIContent(rHint));
        float rW = rSize.x + 20f;
        float rH = Mathf.Max(rSize.y + 8f, lineH);
        Rect rRect = new Rect(x, y, rW, rH);

        // 半透明深色背景
        GUI.Box(rRect, "", _hintBoxStyle);
        GUI.Label(rRect, rHint, _hintLabelStyle);

        // ── T 键提示 ──────────────────────────────────────────
        string tHint = isValve
            ? "  T 键 — 保存角度，返回训练场景"
            : "  T 键 — 取消操作，返回训练场景";
        Vector2 tSize = _hintLabelStyle.CalcSize(new GUIContent(tHint));
        float tW = tSize.x + 20f;
        float tH = Mathf.Max(tSize.y + 8f, lineH);
        Rect tRect = new Rect(x, y + rH + 6f, tW, tH);

        GUI.Box(tRect, "", _hintBoxStyle);
        GUI.Label(tRect, tHint, _hintLabelStyle);
    }

    void EnsureHintStyles()
    {
        if (_hintStylesReady) return;
        _hintStylesReady = true;

        _hintBgTex = MakeHintTexture(new Color(0.06f, 0.08f, 0.12f, 0.88f));

        _hintBoxStyle = new GUIStyle(GUI.skin.box);
        _hintBoxStyle.normal.background = _hintBgTex;
        _hintBoxStyle.normal.textColor = Color.white;
        _hintBoxStyle.padding = new RectOffset(0, 0, 0, 0);

        _hintLabelStyle = new GUIStyle(GUI.skin.label);
        _hintLabelStyle.normal.textColor = new Color(0.78f, 0.85f, 0.92f, 0.95f);
        _hintLabelStyle.fontSize = 15;
        _hintLabelStyle.fontStyle = FontStyle.Bold;
        _hintLabelStyle.alignment = TextAnchor.MiddleLeft;
        _hintLabelStyle.padding = new RectOffset(8, 8, 2, 2);
    }

    static Texture2D MakeHintTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    // ═══════════════════════════════════════════════════════════════
    //  退出
    // ═══════════════════════════════════════════════════════════════

    void ExitTraining()
    {
        RestoreMainCameraState();
        _onComplete?.Invoke(true);
        Destroy(gameObject);
        Debug.Log("[PipelineGestureTraining] 已退出迷你场景，返回管道训练");
    }

    void OnDestroy()
    {
        if (_previousMainCamera != null && !_previousMainCamera.enabled)
        {
            _previousMainCamera.enabled = true;
            _previousMainCamera.tag = _previousMainCameraTag;
        }
        Cursor.visible = _previousCursorVisible;
        Cursor.lockState = _previousCursorLock;
    }

    // ═══════════════════════════════════════════════════════════════
    //  ★ 布局工具方法（自定义布局时使用）
    // ═══════════════════════════════════════════════════════════════

    /// <summary>创建矩形块（Cube）。</summary>
    /// <param name="eulerAngles">可选旋转角度（欧拉角），null = 不旋转（默认）</param>
    GameObject CreateBox(Transform parent, string name, Vector3 localPos, Vector3 localScale, Color color, Vector3? eulerAngles = null)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPos;
        box.transform.localRotation = eulerAngles.HasValue
            ? Quaternion.Euler(eulerAngles.Value)
            : Quaternion.identity;
        box.transform.localScale = localScale;
        Destroy(box.GetComponent<Collider>());
        SetColor(box.GetComponent<Renderer>(), color);
        return box;
    }

    /// <summary>
    /// 创建圆柱体 / 圆盘（Cylinder）。
    /// Unity 默认 Cylinder 直径 1m、高 2m；通过 localScale 控制外观。
    ///   scale.x = scale.z = 直径（m）
    ///   scale.y = 厚度（m）/ 2
    /// </summary>
    /// <param name="eulerAngles">可选旋转角度（欧拉角），null = 不旋转（默认）</param>
    GameObject CreateCylinder(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color, Vector3? eulerAngles = null)
    {
        GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl.name = name;
        cyl.transform.SetParent(parent, false);
        cyl.transform.localPosition = localPos;
        cyl.transform.localRotation = eulerAngles.HasValue
            ? Quaternion.Euler(eulerAngles.Value)
            : Quaternion.identity;
        // Cylinder 原始高度=2，直径=1
        //   scale.x = 直径(m), scale.y = 厚度(m), scale.z = 直径(m)
        //   因为原始高度=2，所以 localScale.y = 目标厚度 / 2
        cyl.transform.localScale = new Vector3(scale.x, scale.y / 2f, scale.z);
        Destroy(cyl.GetComponent<Collider>());
        SetColor(cyl.GetComponent<Renderer>(), color);
        return cyl;
    }

    /// <summary>创建文字标签（TextMesh）。</summary>
    GameObject CreateText(Transform parent, string name, Vector3 localPos,
        string text, int fontSize, float charSize, Color color, TextAnchor anchor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;

        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = fontSize;
        tm.characterSize = charSize;
        tm.color = color;
        tm.anchor = anchor;
        tm.alignment = TextAlignment.Center;
        return go;
    }

    static void SetColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        Material mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    /// <summary>递归设置 Transform 及其所有子对象的 layer。</summary>
    static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursive(t.GetChild(i), layer);
    }
}
