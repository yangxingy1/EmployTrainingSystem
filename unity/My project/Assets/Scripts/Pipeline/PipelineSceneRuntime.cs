using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 化工厂管道场景运行时控制器。
/// 处理键盘交互、模拟仪表读数、驱动 UI 更新。
/// 可与手势系统（RotaryValveInteractable）并存 —— 键盘用于测试，手势用于正式训练。
/// </summary>
public class PipelineSceneRuntime : MonoBehaviour
{
    [Header("References")]
    public PipelineTrainingManager trainingManager;
    public PipelineChemicalSceneBuilder sceneBuilder;

    [Header("Valve Handwheel Transforms")]
    public Transform inletValveWheel;      // V1
    public Transform controlValveWheel;    // V2
    public Transform outletValveWheel;     // V3

    [Header("Gauge Needle Transforms")]
    public Transform gaugeP1Needle;        // 压力表 P1
    public Transform gaugeP2Needle;        // 压力表 P2

    [Header("Flow Meter")]
    public TextMesh flowMeterDisplay;      // F1

    [Header("E-Stop")]
    public Transform eStopButtonTransform;
    public Renderer eStopButtonRenderer;

    [Header("Interaction Settings")]
    public float valveRotateSpeed = 180f;  // 度/秒（键盘操作时）
    public float playerInteractRadius = 5f;

    [Header("Player Movement")]
    public float moveSpeed = 20f;
    public float sprintMultiplier = 2.2f;
    public float mouseSensitivity = 2f;
    public float playerHeight = 10f;
    public float playerRadius = 2f;
    public bool useCollisionWalking = true;

    [Header("Simulation Parameters")]
    public float maxPressure = 1.0f;       // MPa
    public float maxFlow = 60f;            // L/min
    public float valveOpenRatio = 0.5f;    // 阀门开启对压力/流量的影响系数

    // ── 运行时状态 ─────────────────────────────────────────────
    private float _v1Angle;   // 进口阀累计角度
    private float _v2Angle;   // 控制阀累计角度
    private float _v3Angle;   // 出口阀累计角度

    // 阀门自动旋转动画目标角度
    private float _v1TargetAngle;
    private float _v2TargetAngle;
    private float _v3TargetAngle;
    private bool _v1HasTarget;
    private bool _v2HasTarget;
    private bool _v3HasTarget;
    private const float ValveAutoRotateSpeed = 360f;  // 自动旋转速度（度/秒）

    // 保存每个阀门手轮的初始 localRotation（避免首次交互时 Y/X 轴跳变）
    private Quaternion _v1InitialRotation;
    private Quaternion _v2InitialRotation;
    private Quaternion _v3InitialRotation;

    // 压力表指针初始 localRotation（避免手动调参后旋转轴偏移）
    private Quaternion _p1NeedleInitialRotation;
    private Quaternion _p2NeedleInitialRotation;
    private float _simPressure1; // P1 模拟压力
    private float _simPressure2; // P2 模拟压力
    private float _simFlow;      // F1 模拟流量

    private bool _eStopPressed = false;
    private bool _eStopResetting = false;
    private float _eStopPressDepth = 0f;
    private float _eStopTargetDepth = 0f;

    private Transform _playerTransform;
    private bool _playerInScene = false;

    // ── 第一人称玩家组件 ───────────────────────────────────────
    private CharacterController _playerController;
    private Camera _playerCamera;
    private float _cameraPitch;
    private float _cameraYaw;
    private bool _playerCreatedByUs;

    // 材质颜色缓存
    private Color _normalEStopColor = PipelineBuilder.EStopRed;
    private Color _pressedEStopColor = new Color(0.4f, 0.02f, 0.01f);
    private Material _eStopMatInstance;

    // 压力表数字读数（运行时动态创建）
    private TextMesh _gaugeP1Readout;
    private TextMesh _gaugeP2Readout;

    // 交互完成标记（用于 [√] 反馈）
    private HashSet<Transform> _markedInteractions = new HashSet<Transform>();

    // UI 信息面板容器（统一 Billboard）
    private Transform _uiInfoPanel;

    // F 键交互提示
    private bool _showInteractPrompt;

    // 手势训练迷你场景
    private bool _gestureTrainingActive;

    // ═══════════════════════════════════════════════════════════
    //  初始化
    // ═══════════════════════════════════════════════════════════

    void Start()
    {
        // ★ Prefab 自愈：sceneBuilder 指向父级组件，序列化时会丢失，运行时自动找回
        if (sceneBuilder == null && transform.parent != null)
            sceneBuilder = transform.parent.GetComponent<PipelineChemicalSceneBuilder>();

        // 尝试从 sceneBuilder 获取引用（如果未手动设置）
        if (sceneBuilder != null)
        {
            if (inletValveWheel == null)   inletValveWheel   = sceneBuilder.inletValveWheel;
            if (controlValveWheel == null) controlValveWheel = sceneBuilder.controlValveWheel;
            if (outletValveWheel == null)  outletValveWheel  = sceneBuilder.outletValveWheel;
            if (gaugeP1Needle == null)     gaugeP1Needle     = sceneBuilder.gaugeP1Needle;
            if (gaugeP2Needle == null)     gaugeP2Needle     = sceneBuilder.gaugeP2Needle;
            if (flowMeterDisplay == null)  flowMeterDisplay  = sceneBuilder.flowMeterDisplay;
            if (eStopButtonTransform == null) eStopButtonTransform = sceneBuilder.eStopButton;
            if (eStopButtonRenderer == null)  eStopButtonRenderer  = sceneBuilder.eStopRenderer;
        }

        // 从 TrainingManager 获取引用
        if (trainingManager == null)
            trainingManager = GetComponentInParent<PipelineTrainingManager>();
        if (trainingManager == null && GeneratedRoot != null)
            trainingManager = GeneratedRoot.GetComponent<PipelineTrainingManager>();
        if (trainingManager == null)
            trainingManager = FindObjectOfType<PipelineTrainingManager>();

        // 保存阀门初始 localRotation（用于后续仅修改 Z 轴旋转）
        if (inletValveWheel != null)   _v1InitialRotation = inletValveWheel.localRotation;
        if (controlValveWheel != null) _v2InitialRotation = controlValveWheel.localRotation;
        if (outletValveWheel != null)  _v3InitialRotation = outletValveWheel.localRotation;

        // 保存压力表指针初始 localRotation（防止手动调整后旋转轴偏移）
        if (gaugeP1Needle != null) _p1NeedleInitialRotation = gaugeP1Needle.localRotation;
        if (gaugeP2Needle != null) _p2NeedleInitialRotation = gaugeP2Needle.localRotation;

        // 初始化 E-Stop 材质
        if (eStopButtonRenderer != null)
        {
            _eStopMatInstance = eStopButtonRenderer.material;
        }

        // 修复压力表弧段材质（Prefab 加载后可能丢失颜色/自发光）
        FixGaugeArcMaterials();

        // ★ 修复 Cap 父级关系：Cap（中心轴盖）不应随指针旋转，应固定不动
        //   Cap 是 NeedlePivot 的子物体且 localRotation=(90,0,0)（Z轴圆柱），
        //   与父级 Z 轴旋转叠加产生万向节锁 → 拉伸变形
        FixGaugeCaps();

        // ★ 确保压力表数字读数存在（类似 F1 流量计的读数显示）
        EnsureGaugeReadouts();

        // 查找玩家
        FindPlayer();
    }

    Transform GeneratedRoot
    {
        get
        {
            if (sceneBuilder != null && sceneBuilder.GeneratedRoot != null)
                return sceneBuilder.GeneratedRoot.transform;
            return transform.Find(PipelineChemicalSceneBuilder.SceneRootName);
        }
    }

    void FindPlayer()
    {
        // 尝试查找已有玩家（从 Hub 进入或其他场景带入的）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            CharacterController cc = FindObjectOfType<CharacterController>();
            if (cc != null) player = cc.gameObject;
        }

        if (player != null)
        {
            _playerTransform = player.transform;
            _playerController = player.GetComponent<CharacterController>();
            _playerCamera = player.GetComponentInChildren<Camera>();
            _playerInScene = true;
            _playerCreatedByUs = false;

            if (_playerCamera == null)
                _playerCamera = Camera.main;

            // 初始化视角方向
            _cameraYaw = _playerTransform.eulerAngles.y;
            _cameraPitch = 0f;
            return;
        }

        // 没有找到玩家 —— 自动创建一个带 CharacterController 的第一人称玩家
        CreateFirstPersonPlayer();
    }

    /// <summary>
    /// 自动创建第一人称玩家（CharacterController + Camera + 碰撞）。
    /// 仅在场景中没有现成玩家时调用。
    /// </summary>
    void CreateFirstPersonPlayer()
    {
        // 确定出生位置：优先使用场景中的 PlayerSpawnPoint
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        Transform spawnPoint = transform.Find("PlayerSpawnPoint");
        if (spawnPoint == null && GeneratedRoot != null)
            spawnPoint = GeneratedRoot.Find("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
            spawnRot = spawnPoint.rotation;
        }
        else
        {
            // 回退：PPE 站前方的默认位置
            spawnPos = new Vector3(-50f, 1.7f, -27.5f); // ≈ (-10*S, 1.7*S, -5.5*S)  with S=5
            spawnRot = Quaternion.Euler(0f, 90f, 0f);  // 面向管道
        }

        // 创建玩家根 GameObject
        GameObject playerObj = new GameObject("First Person Player");
        playerObj.tag = "Player";
        playerObj.transform.position = spawnPos;
        playerObj.transform.rotation = spawnRot;

        // CharacterController（碰撞体）
        _playerController = playerObj.AddComponent<CharacterController>();
        _playerController.height = playerHeight;
        _playerController.radius = playerRadius;
        _playerController.center = Vector3.up * (playerHeight * 0.5f);
        _playerController.slopeLimit = 50f;
        _playerController.stepOffset = 0.35f;
        _playerController.detectCollisions = useCollisionWalking;

        // 主摄像机
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.SetParent(playerObj.transform, false);
        camObj.transform.localPosition = Vector3.up * (playerHeight - 0.15f);
        camObj.transform.localRotation = Quaternion.identity;

        _playerCamera = camObj.AddComponent<Camera>();
        _playerCamera.fieldOfView = 70f;
        _playerCamera.nearClipPlane = 0.1f;
        _playerCamera.farClipPlane = 500f;
        camObj.AddComponent<AudioListener>();

        _playerTransform = playerObj.transform;
        _playerInScene = true;
        _playerCreatedByUs = true;
        _cameraYaw = spawnRot.eulerAngles.y;
        _cameraPitch = 0f;

        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[PipelineSceneRuntime] 已自动创建第一人称玩家（CharacterController + Camera）。位置: " + spawnPos);
    }

    /// <summary>
    /// 设置 UI_InfoPanel 容器：如果场景中已有则复用，否则在运行时动态创建，
    /// 将 Step / Instruction / Status 三条字幕归入同一父节点，统一绕 Y 轴面向玩家。
    /// 同时缩小字号。
    /// </summary>
    void FindUIInfoPanel()
    {
        _uiInfoPanel = null;

        // 1) 场景中已有 UI_InfoPanel（重新生成过的场景）
        Transform genRoot = GeneratedRoot;
        if (genRoot != null)
        {
            Transform found = genRoot.Find("UI_InfoPanel");
            if (found != null)
            {
                _uiInfoPanel = found;
                return;
            }
        }

        // 2) stepText 的父节点已经是 UI_InfoPanel
        if (trainingManager != null && trainingManager.stepText != null)
        {
            Transform parent = trainingManager.stepText.transform.parent;
            if (parent != null && parent.name == "UI_InfoPanel")
            {
                _uiInfoPanel = parent;
                return;
            }
        }

        // 3) 运行时动态创建容器，把已有的三条字幕移入（不依赖重新生成场景）
        if (trainingManager == null) return;
        if (trainingManager.stepText == null || trainingManager.instructionText == null || trainingManager.statusText == null)
            return;

        Transform root = genRoot;
        if (root == null) root = transform;

        // 以三条字幕的世界坐标中心作为容器位置（同一 X/Z，仅 Y 不同）
        Vector3 pStep = trainingManager.stepText.transform.position;
        Vector3 pInst = trainingManager.instructionText.transform.position;
        Vector3 pStat = trainingManager.statusText.transform.position;
        Vector3 center = (pStep + pInst + pStat) / 3f;

        GameObject panel = new GameObject("UI_InfoPanel");
        panel.transform.SetParent(root, false);
        panel.transform.position = center;
        panel.transform.localRotation = Quaternion.identity;

        // 移入容器（worldPositionStays=true 保持世界位置不变）
        trainingManager.stepText.transform.SetParent(panel.transform, true);
        trainingManager.instructionText.transform.SetParent(panel.transform, true);
        trainingManager.statusText.transform.SetParent(panel.transform, true);

        // 缩小字号
        float S = 6f; // SCALE_FACTOR
        trainingManager.stepText.characterSize = 0.032f * S;
        trainingManager.instructionText.characterSize = 0.020f * S;
        trainingManager.statusText.characterSize = 0.018f * S;

        _uiInfoPanel = panel.transform;
        Debug.Log("[PipelineSceneRuntime] 运行时创建 UI_InfoPanel 容器，统一字幕 Billboard。");
    }

    // ═══════════════════════════════════════════════════════════
    //  Update
    // ═══════════════════════════════════════════════════════════

    void Update()
    {
        if (!_playerInScene)
        {
            FindPlayer();
            return;
        }

        // 手势训练期间：冻结玩家移动和键盘交互，仅维持模拟与视觉更新
        if (_gestureTrainingActive)
        {
            UpdateSimulation();
            UpdateVisuals();
            ReportToTrainingManager();
            return;
        }

        HandlePlayerMovement();
        HandleKeyboardInput();
        UpdateSimulation();
        UpdateVisuals();
        ReportToTrainingManager();
        UpdateInteractPrompt();
    }

    // ═══════════════════════════════════════════════════════════
    //  玩家移动与视角
    // ═══════════════════════════════════════════════════════════

    void HandlePlayerMovement()
    {
        // ── 鼠标视角 ─────────────────────────────────────────────
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _cameraYaw += mouseX;
        _cameraPitch -= mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -89f, 89f);

        _playerTransform.rotation = Quaternion.Euler(0f, _cameraYaw, 0f);
        if (_playerCamera != null)
            _playerCamera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

        // ── WASD 移动（使用 CharacterController 以触发碰撞） ──────
        float h = Input.GetAxis("Horizontal");  // A/D
        float v = Input.GetAxis("Vertical");    // W/S

        Vector3 moveDir = _playerTransform.right * h + _playerTransform.forward * v;
        moveDir.y = 0f;
        if (moveDir.magnitude > 1f) moveDir.Normalize();

        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= sprintMultiplier;

        Vector3 velocity = moveDir * speed;

        // 重力
        if (_playerController != null && _playerController.isGrounded)
            velocity.y = -2f; // 贴地
        else if (_playerController != null)
            velocity.y += Physics.gravity.y * Time.deltaTime;

        if (_playerController != null && _playerController.enabled)
            _playerController.Move(velocity * Time.deltaTime);

        // ── 地面吸附：防止踩上矮障碍物后离开时高度不回 ──────────
        if (_playerController != null && _playerController.isGrounded)
        {
            float checkDist = _playerController.stepOffset + 0.5f;
            Vector3 rayOrigin = _playerTransform.position + Vector3.up * 0.2f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, checkDist * 2f))
            {
                float gap = _playerTransform.position.y - hit.point.y;
                if (gap > _playerController.stepOffset + 0.05f)
                {
                    Vector3 pos = _playerTransform.position;
                    pos.y = hit.point.y;
                    _playerTransform.position = pos;
                }
            }
        }

        // ── 鼠标解锁（Esc）───────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // 点击重新锁定
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// 给交互对象添加绿色 [√] 完成标记（与巡检点反馈一致）。
    /// 会向上查找父级站区，在其标签上追加 [√] 并变绿。
    /// </summary>
    /// <param name="localOffset">[√] 相对 target 的世界偏移（仅新建 [√] 时生效，默认 Vector3.up*1.5f）</param>
    /// <param name="eulerAngles">[√] 的 XYZ 欧拉旋转角度（度），仅新建 [√] 时生效。默认 (0,0,0)=面朝±Z</param>
    void MarkInteractionComplete(Transform target, Vector3? localOffset = null, Vector3? eulerAngles = null)
    {
        Vector3 offset = localOffset ?? Vector3.up * 1.5f;
        Vector3 rotation = eulerAngles ?? Vector3.zero;
        if (target == null) return;
        if (_markedInteractions.Contains(target)) return;
        _markedInteractions.Add(target);

        // 收集候选标签：只搜索 target 自身 + parent 的直接子级（1 层深度），
        // 避免 GetComponentsInChildren 穿透到无关层级
        var candidates = new System.Collections.Generic.List<TextMesh>();

        // target 自身的 TextMesh
        TextMesh selfTm = target.GetComponent<TextMesh>();
        if (selfTm != null) candidates.Add(selfTm);

        // target 的直接子级（1 层）
        foreach (Transform child in target)
        {
            TextMesh tm = child.GetComponent<TextMesh>();
            if (tm != null) candidates.Add(tm);
        }

        // parent 的直接子级（即 target 的兄弟节点）—— 前提 parent 存在且不是场景根
        Transform genRoot = GeneratedRoot;
        if (target.parent != null && target.parent != genRoot)
        {
            TextMesh parentTm = target.parent.GetComponent<TextMesh>();
            if (parentTm != null) candidates.Add(parentTm);
            foreach (Transform sibling in target.parent)
            {
                TextMesh tm = sibling.GetComponent<TextMesh>();
                if (tm != null && tm != selfTm) candidates.Add(tm);
            }
        }

        bool found = false;
        foreach (var label in candidates)
        {
            if (label == null) continue;
            // 跳过动态读数（流量计、压力表）
            if (label == flowMeterDisplay || label == _gaugeP1Readout || label == _gaugeP2Readout)
                continue;
            // 跳过培训 UI 字幕
            if (trainingManager != null &&
                (label == trainingManager.titleText || label == trainingManager.stepText
                 || label == trainingManager.instructionText || label == trainingManager.statusText))
                continue;

            if (!label.text.EndsWith("[√]"))
            {
                label.text = label.text.TrimEnd() + " [√]";
                label.color = new Color(0.2f, 1f, 0.3f);
                found = true;
            }
        }

        // 如果对象本身没有标签，创建一个 [√]
        // ★ 挂到 target.parent（不旋转的父节点）下，避免随手轮/旋转部件一起转动
        if (!found)
        {
            GameObject feedbackObj = new GameObject("Interaction_Feedback_[√]");
            // 先设定世界位置（target 正上方 1.5 单位）
            feedbackObj.transform.position = target.position + offset;
            // 再挂到父节点，worldPositionStays=true 保持世界位置不变
            Transform feedbackParent = target.parent != null ? target.parent : target;
            feedbackObj.transform.SetParent(feedbackParent, true);
            // ★ 显式设置旋转（覆盖父节点继承的旋转），支持 XYZ 三轴
            feedbackObj.transform.localRotation = Quaternion.Euler(rotation);
            TextMesh tm = feedbackObj.AddComponent<TextMesh>();
            tm.text = "[√]";
            tm.fontSize = 48;
            tm.characterSize = 0.15f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.2f, 1f, 0.3f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  F 键交互提示 UI（OnGUI 屏幕贴图）
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 屏幕右上角 F 键交互提示。当玩家靠近任意 F 键可交互对象时显示。
    /// 使用 OnGUI 渲染，不依赖 Canvas / 动态字体。
    /// </summary>
    void OnGUI()
    {
        // 手势训练期间不显示 F 键提示（迷你场景有自己的 UI）
        if (_gestureTrainingActive) return;
        if (!_showInteractPrompt || _playerTransform == null)
            return;

        string promptText = "   请按 F 键进行交互";

        // ── 样式 ──────────────────────────────────────────────
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeSolidTexture(
            new Color(0.02f, 0.02f, 0.02f, 0.88f), ref _cachedBgTex);
        boxStyle.normal.textColor = Color.white;
        boxStyle.fontSize = 26;
        boxStyle.fontStyle = FontStyle.Bold;
        boxStyle.alignment = TextAnchor.MiddleCenter;
        boxStyle.padding = new RectOffset(18, 18, 10, 10);

        // ── 尺寸与位置（右上角） ──────────────────────────────
        Vector2 textSize = boxStyle.CalcSize(new GUIContent(promptText));
        float boxW = textSize.x + 44; // 留 F 键徽章空间
        float boxH = textSize.y + 20;
        float x = Screen.width - boxW - 30;
        float y = 30;

        Rect boxRect = new Rect(x, y, boxW, boxH);

        // ── 外发光边框 ────────────────────────────────────────
        GUIStyle borderStyle = new GUIStyle(GUI.skin.box);
        borderStyle.normal.background = MakeSolidTexture(
            new Color(1f, 0.6f, 0.05f, 0.9f), ref _cachedBorderTex);
        GUI.Box(new Rect(x - 2, y - 2, boxW + 4, boxH + 4), "", borderStyle);

        // ── 主体黑色背景 ──────────────────────────────────────
        GUI.Box(boxRect, promptText, boxStyle);

        // ── F 键徽章（覆盖在左侧） ─────────────────────────────
        GUIStyle keyBadgeStyle = new GUIStyle(GUI.skin.box);
        keyBadgeStyle.normal.background = MakeSolidTexture(
            new Color(1f, 0.85f, 0.1f, 1f), ref _cachedKeyTex);
        keyBadgeStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        keyBadgeStyle.fontSize = 22;
        keyBadgeStyle.fontStyle = FontStyle.Bold;
        keyBadgeStyle.alignment = TextAnchor.MiddleCenter;
        keyBadgeStyle.padding = new RectOffset(0, 0, 0, 0);

        float keyW = 36;
        float keyH = 30;
        Rect keyRect = new Rect(x + 12, y + (boxH - keyH) / 2, keyW, keyH);
        GUI.Box(keyRect, "F", keyBadgeStyle);
    }

    void UpdateInteractPrompt()
    {
        _showInteractPrompt = IsNearAnyFKeyInteractable();
    }

    bool IsNearAnyFKeyInteractable()
    {
        if (_playerTransform == null) return false;

        // 按钮（PPE 确认 / 关停提交）
        if (sceneBuilder != null)
        {
            foreach (var btn in sceneBuilder.confirmButtons)
            {
                if (btn != null && IsNearTarget(btn.transform.position))
                    return true;
            }
        }
        if (_cachedButtons != null)
        {
            foreach (var btn in _cachedButtons)
            {
                if (btn != null && IsNearTarget(btn.transform.position))
                    return true;
            }
        }

        // 巡检点
        if (_cachedInspectionZones != null)
        {
            foreach (var zone in _cachedInspectionZones)
            {
                if (zone != null && IsNearTarget(zone.transform.position))
                    return true;
            }
        }

        // 压力表 P1 / P2
        if (_cachedGaugeP1Transform != null && IsNearTarget(_cachedGaugeP1Transform.position))
            return true;
        if (_cachedGaugeP2Transform != null && IsNearTarget(_cachedGaugeP2Transform.position))
            return true;

        // 流量计 F1
        if (flowMeterDisplay != null && IsNearTarget(flowMeterDisplay.transform.position))
            return true;

        // 急停按钮
        if (eStopButtonTransform != null && IsNearTarget(eStopButtonTransform.position))
            return true;

        // 阀门（V1 / V2 / V3）
        if (inletValveWheel != null && IsNearTarget(inletValveWheel.position))
            return true;
        if (controlValveWheel != null && IsNearTarget(controlValveWheel.position))
            return true;
        if (outletValveWheel != null && IsNearTarget(outletValveWheel.position))
            return true;

        return false;
    }

    /// <summary>生成 1x1 纯色纹理（OnGUI 背景用）</summary>
    static Texture2D MakeSolidTexture(Color color, ref Texture2D cache)
    {
        if (cache == null)
        {
            cache = new Texture2D(1, 1);
            cache.hideFlags = HideFlags.HideAndDontSave;
        }
        cache.SetPixel(0, 0, color);
        cache.Apply();
        return cache;
    }
    static Texture2D _cachedBgTex;
    static Texture2D _cachedBorderTex;
    static Texture2D _cachedKeyTex;

    // ═══════════════════════════════════════════════════════════
    //  键盘输入处理（阀门/按钮交互）
    // ═══════════════════════════════════════════════════════════

    void HandleKeyboardInput()
    {
        // ── F 键：急停优先 → 阀门 → 仪表 → 按钮 → 巡检点 ──────────
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 优先级 1：靠近急停按钮 → 进入手势训练迷你场景
            if (eStopButtonTransform != null && IsNearTarget(eStopButtonTransform.position))
            {
                EnterGestureTraining(PipelineTrainingManager.PipelineStep.EmergencyStopTest);
            }
            // 优先级 2：靠近阀门 → 进入阀门手势训练（替代原 Q/E 键盘旋转）
            else if (TryEnterValveGestureTraining())
            {
                // 已在 TryEnterValveGestureTraining 内部处理
            }
            // 优先级 3：靠近压力表 / 流量计 → 进入对应的手势训练迷你场景
            else if (TryEnterGaugeGestureTraining())
            {
                // 已在 TryEnterGaugeGestureTraining 内部处理
            }
            else
            {
                CheckNearbyButtons();
                CheckNearbyZones();
            }
        }

        // ── R 键：急停复位（靠近时） ──────────────────────────────
        if (eStopButtonTransform != null && IsNearTarget(eStopButtonTransform.position))
        {
            if (Input.GetKeyDown(KeyCode.R) && _eStopPressed)
            {
                _eStopResetting = true;
                _eStopTargetDepth = 0f;
            }
        }
    }

    Transform GetNearestValve()
    {
        Transform nearest = null;
        float nearestDist = playerInteractRadius;

        TestValveDistance(inletValveWheel, ref nearest, ref nearestDist);
        TestValveDistance(controlValveWheel, ref nearest, ref nearestDist);
        TestValveDistance(outletValveWheel, ref nearest, ref nearestDist);

        return nearest;
    }

    void TestValveDistance(Transform valveWheel, ref Transform nearest, ref float nearestDist)
    {
        if (valveWheel == null || _playerTransform == null) return;
        float dist = Vector3.Distance(_playerTransform.position, valveWheel.position);
        if (dist < nearestDist)
        {
            nearestDist = dist;
            nearest = valveWheel;
        }
    }

    /// <summary>
    /// F 键按下时尝试进入阀门手势训练迷你场景（替代原 Q/E 键盘旋转）。
    /// </summary>
    /// <returns>true = 已进入手势训练</returns>
    bool TryEnterValveGestureTraining()
    {
        // V1 进口阀门（步骤 4）
        if (inletValveWheel != null && IsNearTarget(inletValveWheel.position))
        {
            EnterGestureTraining(PipelineTrainingManager.PipelineStep.OpenInletValve);
            return true;
        }
        // V2 控制阀门（步骤 6）
        if (controlValveWheel != null && IsNearTarget(controlValveWheel.position))
        {
            EnterGestureTraining(PipelineTrainingManager.PipelineStep.AdjustControlValve);
            return true;
        }
        // V3 出口阀门（步骤 8）
        if (outletValveWheel != null && IsNearTarget(outletValveWheel.position))
        {
            EnterGestureTraining(PipelineTrainingManager.PipelineStep.OpenOutletValve);
            return true;
        }
        return false;
    }

    bool IsNearTarget(Vector3 targetPos)
    {
        if (_playerTransform == null) return false;
        // 只判水平距离（XZ 平面），忽略 Y 轴高度差
        Vector3 playerXZ = _playerTransform.position;
        Vector3 targetXZ = targetPos;
        playerXZ.y = 0f;
        targetXZ.y = 0f;
        return Vector3.Distance(playerXZ, targetXZ) < playerInteractRadius;
    }

    // ═══════════════════════════════════════════════════════════════
    //  手势训练迷你场景调度
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 进入手势训练迷你场景。冻结玩家移动，创建由脚本搭建的手势操作面板。
    /// </summary>
    void EnterGestureTraining(PipelineTrainingManager.PipelineStep step)
    {
        if (_gestureTrainingActive) return;
        _gestureTrainingActive = true;

        var def = trainingManager != null ? trainingManager.GetStepDef(step) : default;
        string stepName = def.step != PipelineTrainingManager.PipelineStep.NotStarted
            ? def.name
            : step.ToString();

        string gaugeValue = null;
        float gaugeNumericValue = 0f;
        Color? bgColor = null;

        // 按步骤配置迷你场景
        switch (step)
        {
            case PipelineTrainingManager.PipelineStep.PPECheck:
                bgColor = new Color(0.05f, 0.10f, 0.18f); // 深蓝色调 — 安全装备
                break;
            case PipelineTrainingManager.PipelineStep.ReadInitialPressure:
                gaugeValue = _simPressure1.ToString("F2") + " MPa";
                gaugeNumericValue = _simPressure1;
                bgColor = new Color(0.07f, 0.09f, 0.11f);
                break;
            case PipelineTrainingManager.PipelineStep.MonitorFlowMeter:
                gaugeValue = _simFlow.ToString("F1") + " L/min";
                gaugeNumericValue = _simFlow;
                bgColor = new Color(0.07f, 0.09f, 0.11f);
                break;
            case PipelineTrainingManager.PipelineStep.CheckMidPressure:
                gaugeValue = _simPressure2.ToString("F2") + " MPa";
                gaugeNumericValue = _simPressure2;
                bgColor = new Color(0.07f, 0.09f, 0.11f);
                break;
            case PipelineTrainingManager.PipelineStep.EmergencyStopTest:
                bgColor = new Color(0.18f, 0.05f, 0.05f); // 深红色调 — 急停
                break;
            case PipelineTrainingManager.PipelineStep.SystemShutdown:
                bgColor = new Color(0.05f, 0.12f, 0.08f); // 深绿色调 — 关停
                break;
            case PipelineTrainingManager.PipelineStep.OpenInletValve:
                bgColor = new Color(0.06f, 0.08f, 0.12f); // 深色 — 阀门
                break;
            case PipelineTrainingManager.PipelineStep.AdjustControlValve:
                // 步骤 6：显示 F1 流量读数，无固定角度目标
                gaugeValue = _simFlow.ToString("F1") + " L/min";
                gaugeNumericValue = _simFlow;
                bgColor = new Color(0.06f, 0.08f, 0.12f);
                break;
            case PipelineTrainingManager.PipelineStep.OpenOutletValve:
                bgColor = new Color(0.06f, 0.08f, 0.12f);
                break;
        }

        // 阀门步骤获取当前角度和目标
        float initialValveAngle = 0f;
        float valveTargetAngle = 720f;
        float flowMaxValue = 0f;
        switch (step)
        {
            case PipelineTrainingManager.PipelineStep.OpenInletValve:
                initialValveAngle = _v1Angle;
                valveTargetAngle = 720f;
                break;
            case PipelineTrainingManager.PipelineStep.AdjustControlValve:
                initialValveAngle = _v2Angle;
                valveTargetAngle = 0f;  // 无固定角度目标，以流量为准
                // flowMaxValue = V1开度 × maxFlow（V2 全开时的理论最大流量）
                flowMaxValue = Mathf.Clamp01(_v1Angle / 720f) * maxFlow;
                break;
            case PipelineTrainingManager.PipelineStep.OpenOutletValve:
                initialValveAngle = _v3Angle;
                valveTargetAngle = 540f;
                break;
        }

        GameObject trainingObj = new GameObject("PipelineGestureTraining_" + step);
        var controller = trainingObj.AddComponent<PipelineGestureTrainingController>();
        controller.ConfigureAndBegin(step, stepName, gaugeValue, gaugeNumericValue, bgColor, this, (success) =>
        {
            float resultAngle = controller.ValveAngle;
            OnGestureTrainingComplete(step, success, resultAngle);
        }, initialValveAngle, valveTargetAngle, flowMaxValue);

        Debug.Log("[PipelineSceneRuntime] 进入手势训练迷你场景: " + stepName);
    }

    /// <summary>
    /// 手势训练完成回调。标记步骤完成并恢复玩家控制。
    /// </summary>
    void OnGestureTrainingComplete(PipelineTrainingManager.PipelineStep step, bool success, float valveAngle = 0f)
    {
        _gestureTrainingActive = false;

        // ── 阀门步骤：无论成功/取消，都保存旋转角度 ────────────────
        switch (step)
        {
            case PipelineTrainingManager.PipelineStep.OpenInletValve:
                _v1TargetAngle = valveAngle;
                _v1HasTarget = true;
                break;
            case PipelineTrainingManager.PipelineStep.AdjustControlValve:
                _v2TargetAngle = valveAngle;
                _v2HasTarget = true;
                break;
            case PipelineTrainingManager.PipelineStep.OpenOutletValve:
                _v3TargetAngle = valveAngle;
                _v3HasTarget = true;
                break;
        }

        // 用户取消训练 → 不标记步骤完成，仅恢复玩家控制
        if (!success)
        {
            Debug.Log("[PipelineSceneRuntime] 手势训练已取消: " + step + " (阀门角度已保存: " + valveAngle + "°)");
            return;
        }

        // 根据步骤类型向 TrainingManager 报告完成
        switch (step)
        {
            case PipelineTrainingManager.PipelineStep.PPECheck:
                trainingManager?.ReportButtonPress("PPE_Confirm");
                // 在所有 PPE 按钮上标记 [√]
                MarkPPEComplete();
                break;
            case PipelineTrainingManager.PipelineStep.ReadInitialPressure:
                trainingManager?.ReportGaugeRead("Gauge_P1");
                if (_cachedGaugeP1Transform != null)
                    MarkInteractionComplete(_cachedGaugeP1Transform,
                        new Vector3(0f, 1f, 0f), new Vector3(180f, 0f, -90f));
                break;
            case PipelineTrainingManager.PipelineStep.MonitorFlowMeter:
                trainingManager?.ReportGaugeRead("Flow_F1");
                if (flowMeterDisplay != null)
                    MarkInteractionComplete(flowMeterDisplay.transform,
                        new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 0f));
                break;
            case PipelineTrainingManager.PipelineStep.CheckMidPressure:
                trainingManager?.ReportGaugeRead("Gauge_P2");
                if (_cachedGaugeP2Transform != null)
                    MarkInteractionComplete(_cachedGaugeP2Transform,
                        new Vector3(0f, 1f, 0f), new Vector3(90f, 0f, 180f));
                break;
            case PipelineTrainingManager.PipelineStep.EmergencyStopTest:
                trainingManager?.ReportButtonPress("EStop_Button");
                if (eStopButtonTransform != null)
                    MarkInteractionComplete(eStopButtonTransform);
                break;
            case PipelineTrainingManager.PipelineStep.SystemShutdown:
                trainingManager?.ReportButtonPress("Shutdown_Confirm");
                // 标记所有 Shutdown 按钮
                if (sceneBuilder != null)
                {
                    foreach (var btn in sceneBuilder.confirmButtons)
                    {
                        if (btn != null && (btn.name.Contains("Shutdown") || btn.name.Contains("Confirm")))
                            MarkInteractionComplete(btn.transform);
                    }
                }
                break;
            case PipelineTrainingManager.PipelineStep.OpenInletValve:
                if (valveAngle >= 720f && inletValveWheel != null)
                    MarkInteractionComplete(inletValveWheel,
                        new Vector3(0f, 1.5f, 0f), new Vector3(0f, 0f, -90f));
                break;
            case PipelineTrainingManager.PipelineStep.AdjustControlValve:
                if (valveAngle >= 720f && controlValveWheel != null)
                    MarkInteractionComplete(controlValveWheel);
                break;
            case PipelineTrainingManager.PipelineStep.OpenOutletValve:
                if (valveAngle >= 540f && outletValveWheel != null)
                    MarkInteractionComplete(outletValveWheel,
                        new Vector3(0f, 1.5f, 0f), new Vector3(0f, 0f, -90f));
                break;
        }

        Debug.Log("[PipelineSceneRuntime] 手势训练完成: " + step);
    }

    /// <summary>
    /// 在所有 PPE 按钮上标记 [√]
    /// </summary>
    void MarkPPEComplete()
    {
        if (sceneBuilder != null)
        {
            foreach (var btn in sceneBuilder.confirmButtons)
            {
                if (btn != null && btn.name.Contains("PPE"))
                    MarkInteractionComplete(btn.transform);
            }
        }
        if (_cachedButtons != null)
        {
            foreach (var btn in _cachedButtons)
            {
                if (btn != null && btn.name.Contains("PPE"))
                    MarkInteractionComplete(btn.transform);
            }
        }
    }

    private GameObject[] _cachedButtons;

    void CheckNearbyButtons()
    {
        if (sceneBuilder == null) return;

        // 优先使用 sceneBuilder 的 confirmButtons 列表
        if (sceneBuilder.confirmButtons.Count > 0)
        {
            foreach (var btn in sceneBuilder.confirmButtons)
            {
                if (btn == null) continue;
                TryPressButton(btn);
            }
        }
        else
        {
            // ★ 运行时自愈：confirmButtons 为空时，扫描有碰撞体的可交互对象
            if (_cachedButtons == null)
            {
                var found = new System.Collections.Generic.List<GameObject>();
                foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
                {
                    // 优先组件检测：挂有特定名称模式的按钮对象
                    string n = t.name;
                    if (n.Contains("Confirm") || n.EndsWith("_Btn") || n.Contains("PPE") || n.Contains("Shutdown"))
                    {
                        // 确保是按钮根对象（有子物体），不是子物体本身
                        if (t.childCount > 1)
                            found.Add(t.gameObject);
                    }
                }
                _cachedButtons = found.ToArray();
                Debug.Log("[PipelineSceneRuntime] 按钮扫描完成：发现 " + _cachedButtons.Length + " 个");
                foreach (var b in _cachedButtons)
                    Debug.Log("  → 按钮: " + b.name + "  位置: " + b.transform.position);
            }
            foreach (var btn in _cachedButtons)
            {
                if (btn == null) continue;
                TryPressButton(btn);
            }
        }
    }

    void TryPressButton(GameObject btn)
    {
        if (!IsNearTarget(btn.transform.position)) return;
        string btnName = btn.name;

        if (btnName.Contains("PPE"))
        {
            EnterGestureTraining(PipelineTrainingManager.PipelineStep.PPECheck);
        }
        else if (btnName.Contains("Shutdown") || btnName.Contains("Confirm"))
        {
            EnterGestureTraining(PipelineTrainingManager.PipelineStep.SystemShutdown);
        }
    }

    private GameObject[] _cachedInspectionZones;
    private HashSet<GameObject> _visitedZones = new HashSet<GameObject>();
    private Transform _cachedGaugeP1Transform;
    private Transform _cachedGaugeP2Transform;
    private Transform _cachedFlowMeterTransform;
    private bool _gaugesScanned;

    void CheckNearbyZones()
    {
        // ★ 运行时自愈：通过 PipelineZoneTrigger 组件自动发现巡检点（不依赖命名）
        if (_cachedInspectionZones == null)
        {
            var found = new System.Collections.Generic.List<GameObject>();
            foreach (var trigger in transform.GetComponentsInChildren<PipelineZoneTrigger>(true))
            {
                if (trigger != null)
                    found.Add(trigger.gameObject);
            }
            _cachedInspectionZones = found.ToArray();

            // 同步巡检点总数给 TrainingManager
            if (trainingManager != null)
                trainingManager.totalInspectionZones = _cachedInspectionZones.Length;

            Debug.Log("[PipelineSceneRuntime] 巡检点扫描完成：发现 " + _cachedInspectionZones.Length + " 个");
            foreach (var z in _cachedInspectionZones)
                Debug.Log("  → 巡检点: " + z.name + "  位置: " + z.transform.position);
        }

        for (int i = 0; i < _cachedInspectionZones.Length; i++)
        {
            var zone = _cachedInspectionZones[i];
            if (zone == null) continue;
            if (IsNearTarget(zone.transform.position))
            {
                // ★ 每个巡检点用唯一 ID
                string zoneId = "InspectionZone_" + i;
                trainingManager?.ReportZoneVisit(zoneId);

                // 首次访问标记 [√]
                if (!_visitedZones.Contains(zone))
                {
                    _visitedZones.Add(zone);
                    TextMesh[] labels = zone.GetComponentsInChildren<TextMesh>();
                    foreach (var label in labels)
                    {
                        if (!label.text.EndsWith("[√]"))
                        {
                            label.text = label.text.TrimEnd() + " [√]";
                            label.color = new Color(0.2f, 1f, 0.3f);
                        }
                    }
                }
            }
        }

        // 仪表读数交互
        CheckNearbyGauges();
    }

    /// <summary>
    /// F 键按下时尝试进入仪表（压力表/流量计）手势训练迷你场景。
    /// </summary>
    /// <returns>true = 已进入手势训练（不再继续后续按钮/巡检点检查）</returns>
    bool TryEnterGaugeGestureTraining()
    {
        // 懒扫描仪表 Transform（与 CheckNearbyGauges 共享缓存）
        if (!_gaugesScanned)
        {
            foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains("GaugeP1") || (t.name.Contains("P1") && t.name.Contains("Gauge")))
                    _cachedGaugeP1Transform = t;
                if (t.name.Contains("GaugeP2") || (t.name.Contains("P2") && t.name.Contains("Gauge")))
                    _cachedGaugeP2Transform = t;
                if (t.name.Contains("FlowMeter") || t.name.Contains("flowMeter"))
                    _cachedFlowMeterTransform = t;
            }
            _gaugesScanned = true;
        }

        if (_cachedGaugeP1Transform != null && IsNearTarget(_cachedGaugeP1Transform.position))
        {
            EnterGestureTraining(PipelineTrainingManager.PipelineStep.ReadInitialPressure);
            return true;
        }
        if (_cachedGaugeP2Transform != null && IsNearTarget(_cachedGaugeP2Transform.position))
        {
            EnterGestureTraining(PipelineTrainingManager.PipelineStep.CheckMidPressure);
            return true;
        }

        // 流量计优先用缓存的 Transform；若为空或匹配到错误对象，用 sceneBuilder 的直接引用兜底
        Transform flowTarget = _cachedFlowMeterTransform;
        if (flowTarget == null && flowMeterDisplay != null)
            flowTarget = flowMeterDisplay.transform;
        // 如果缓存的 Transform 不是 FlowMeter 开头（如被 FlowArrow 误匹配），用 display 的父级修正
        if (_cachedFlowMeterTransform != null
            && !_cachedFlowMeterTransform.name.StartsWith("FlowMeter")
            && flowMeterDisplay != null)
            flowTarget = flowMeterDisplay.transform;

        if (flowTarget != null && IsNearTarget(flowTarget.position))
        {
            EnterGestureTraining(PipelineTrainingManager.PipelineStep.MonitorFlowMeter);
            return true;
        }

        return false;
    }

    void CheckNearbyGauges()
    {
        if (trainingManager == null) return;

        // 运行时扫描仪表位置
        if (!_gaugesScanned)
        {
            foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains("GaugeP1") || (t.name.Contains("P1") && t.name.Contains("Gauge")))
                    _cachedGaugeP1Transform = t;
                if (t.name.Contains("GaugeP2") || (t.name.Contains("P2") && t.name.Contains("Gauge")))
                    _cachedGaugeP2Transform = t;
                if (_cachedFlowMeterTransform == null &&
                    (t.name.Contains("FlowMeter") || t.name.Contains("flowMeter")))
                    _cachedFlowMeterTransform = t;
            }
            _gaugesScanned = true;
            if (_cachedGaugeP1Transform != null)
                Debug.Log("[PipelineSceneRuntime] 发现仪表 P1: " + _cachedGaugeP1Transform.name);
            if (_cachedGaugeP2Transform != null)
                Debug.Log("[PipelineSceneRuntime] 发现仪表 P2: " + _cachedGaugeP2Transform.name);
            if (_cachedFlowMeterTransform != null)
                Debug.Log("[PipelineSceneRuntime] 发现流量计: " + _cachedFlowMeterTransform.name);
        }

        if (_cachedGaugeP1Transform != null && IsNearTarget(_cachedGaugeP1Transform.position))
        {
            trainingManager.ReportGaugeRead("Gauge_P1");
            // P1 压力表 [√]  偏移(X,Y,Z)  旋转(X,Y,Z)
            MarkInteractionComplete(_cachedGaugeP1Transform,
                new Vector3(0f, 1f, 0f), new Vector3(180f, 0f, -90f));
        }
        if (_cachedGaugeP2Transform != null && IsNearTarget(_cachedGaugeP2Transform.position))
        {
            trainingManager.ReportGaugeRead("Gauge_P2");
            // P2 压力表 [√]  偏移(X,Y,Z)  旋转(X,Y,Z)
            MarkInteractionComplete(_cachedGaugeP2Transform,
                new Vector3(0f, 1f, 0f), new Vector3(90f, 0f, 180f));
        }
        if (flowMeterDisplay != null && IsNearTarget(flowMeterDisplay.transform.position))
        {
            trainingManager.ReportGaugeRead("Flow_F1");
            MarkInteractionComplete(flowMeterDisplay.transform,
                new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 0f));
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  物理模拟
    // ═══════════════════════════════════════════════════════════

    void UpdateSimulation()
    {
        if (_eStopPressed && !_eStopResetting)
        {
            // 急停状态：强制所有值为 0
            _simPressure1 = Mathf.Lerp(_simPressure1, 0f, Time.deltaTime * 3f);
            _simPressure2 = Mathf.Lerp(_simPressure2, 0f, Time.deltaTime * 3f);
            _simFlow      = Mathf.Lerp(_simFlow, 0f, Time.deltaTime * 3f);
            return;
        }

        // 模拟管道物理：
        // - V1 控制入口流量 / 压力
        // - V2 控制中间流量调节
        // - V3 控制出口 —— 打开 V3 会降低中段压力（流体排出）
        // - 流量 ≈ V1开度 × V2开度 × maxFlow
        // - P1 ≈ V1开度 × maxPressure
        // - P2 ≈ V1开度 × V2开度 × (1 - V3开度×0.5) × maxPressure × 0.7
        //   （V3 关闭时 P2 最高 ≈ 0.7 MPa，V3 全开时降至 ≈ 0.35 MPa）

        float v1Open = Mathf.Clamp01(_v1Angle / 720f); // 720° = 2圈全开
        float v2Open = Mathf.Clamp01(_v2Angle / 720f);   // 720° = 2圈
        float v3Open = Mathf.Clamp01(_v3Angle / 540f);  // 540° = 1.5圈

        float targetPressure1 = v1Open * maxPressure;
        float targetFlow = v1Open * v2Open * maxFlow;
        // P2 受 V1/V2 正影响，V3 负影响（V3 打开=泄压）
        float targetPressure2 = v1Open * v2Open * (1f - v3Open * 0.5f) * maxPressure * 0.7f;
        targetPressure2 = Mathf.Clamp(targetPressure2, 0f, maxPressure);

        // 平滑过渡
        _simPressure1 = Mathf.Lerp(_simPressure1, targetPressure1, Time.deltaTime * 2f);
        _simPressure2 = Mathf.Lerp(_simPressure2, targetPressure2, Time.deltaTime * 2f);
        _simFlow      = Mathf.Lerp(_simFlow, targetFlow, Time.deltaTime * 2f);
    }

    // ═══════════════════════════════════════════════════════════
    //  视觉更新
    // ═══════════════════════════════════════════════════════════

    void UpdateVisuals()
    {
        // ── 阀门手轮自动旋转动画（手势训练返回后的平滑过渡） ─────
        UpdateValveAutoRotate(inletValveWheel, ref _v1Angle, _v1TargetAngle, ref _v1HasTarget, _v1InitialRotation);
        UpdateValveAutoRotate(controlValveWheel, ref _v2Angle, _v2TargetAngle, ref _v2HasTarget, _v2InitialRotation);
        UpdateValveAutoRotate(outletValveWheel, ref _v3Angle, _v3TargetAngle, ref _v3HasTarget, _v3InitialRotation);

        // 压力表 P1 指针：-120° = 0 MPa, +120° = maxPressure
        UpdateGaugeNeedle(gaugeP1Needle, _simPressure1, maxPressure, _p1NeedleInitialRotation);

        // 压力表 P2 指针
        UpdateGaugeNeedle(gaugeP2Needle, _simPressure2, maxPressure, _p2NeedleInitialRotation);

        // 流量计显示
        if (flowMeterDisplay != null)
        {
            flowMeterDisplay.text = _simFlow.ToString("F1") + " L/min";
            // 颜色随流量变化
            if (_simFlow < 15f)
                flowMeterDisplay.color = new Color(0.9f, 0.7f, 0.2f); // 黄色 - 低流量
            else if (_simFlow > 50f)
                flowMeterDisplay.color = new Color(1f, 0.3f, 0.2f);  // 红色 - 高流量
            else
                flowMeterDisplay.color = new Color(0.2f, 0.95f, 0.65f); // 绿色 - 正常
        }

        // 压力表数字读数（类似 F1 流量计）
        if (_gaugeP1Readout != null)
        {
            _gaugeP1Readout.text = "P1 " + _simPressure1.ToString("F2") + " MPa";
            _gaugeP1Readout.color = _simPressure1 > 0.85f
                ? new Color(1f, 0.3f, 0.2f)   // 红色 - 压力过高
                : new Color(1f, 0.85f, 0.1f); // 亮金色 - 正常
        }
        if (_gaugeP2Readout != null)
        {
            _gaugeP2Readout.text = "P2 " + _simPressure2.ToString("F2") + " MPa";
            _gaugeP2Readout.color = _simPressure2 > 0.85f
                ? new Color(1f, 0.3f, 0.2f)   // 红色 - 压力过高
                : new Color(1f, 0.85f, 0.1f); // 亮金色 - 正常
        }

        // ★ 所有提示字幕始终水平面向玩家（Billboard）
        if (flowMeterDisplay != null)
            FacePlayer(flowMeterDisplay.transform);
        if (_gaugeP1Readout != null)
            FacePlayer(_gaugeP1Readout.transform);
        if (_gaugeP2Readout != null)
            FacePlayer(_gaugeP2Readout.transform);
        if (trainingManager != null)
        {
            // 标题独立 Billboard
            if (trainingManager.titleText != null)
                FacePlayer(trainingManager.titleText.transform);

            // Step / Instruction / Status 统一放在 UI_InfoPanel 容器中整体旋转
            if (_uiInfoPanel != null)
            {
                FacePlayer(_uiInfoPanel);
            }
            else
            {
                // 回退：旧 Prefab 没有 UI_InfoPanel，逐个 Billboard
                if (trainingManager.stepText != null)
                    FacePlayer(trainingManager.stepText.transform);
                if (trainingManager.instructionText != null)
                    FacePlayer(trainingManager.instructionText.transform);
                if (trainingManager.statusText != null)
                    FacePlayer(trainingManager.statusText.transform);
            }
        }

        // 巡检点标签也始终面向玩家
        if (_cachedInspectionZones != null)
        {
            foreach (var zone in _cachedInspectionZones)
            {
                if (zone == null) continue;
                TextMesh[] labels = zone.GetComponentsInChildren<TextMesh>();
                foreach (var label in labels)
                    FacePlayer(label?.transform);
            }
        }

        // 急停按钮动画
        UpdateEStopAnimation();
    }

    /// <summary>
    /// 使目标 Transform 水平面向玩家摄像头（仅 Y 轴旋转，不倾斜）。
    /// </summary>
    void FacePlayer(Transform t)
    {
        if (t == null || _playerCamera == null) return;
        Vector3 dir = t.position - _playerCamera.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            t.rotation = Quaternion.LookRotation(dir);
    }

    void UpdateGaugeNeedle(Transform needlePivot, float currentValue, float maxValue, Quaternion initialRotation)
    {
        if (needlePivot == null) return;

        // 指针范围：-120°（最小值）到 +120°（最大值）
        float ratio = Mathf.Clamp01(currentValue / maxValue);
        float angle = Mathf.Lerp(-120f, 120f, ratio);
        // 保留初始朝向，仅叠加 Z 轴旋转（防止手动调整后旋转轴偏移）
        needlePivot.localRotation = initialRotation * Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// 阀门手轮自动旋转动画：手势训练返回后，平滑旋转至目标角度。
    /// </summary>
    void UpdateValveAutoRotate(Transform handwheel, ref float currentAngle, float targetAngle, ref bool hasTarget, Quaternion initialRotation)
    {
        if (handwheel == null || !hasTarget) return;

        if (Mathf.Abs(currentAngle - targetAngle) < 0.5f)
        {
            // 已到达目标
            currentAngle = targetAngle;
            hasTarget = false;
        }
        else
        {
            // 平滑旋转（逆时针 = 正方向）
            float step = ValveAutoRotateSpeed * Time.deltaTime;
            if (targetAngle > currentAngle)
                currentAngle = Mathf.Min(currentAngle + step, targetAngle);
            else
                currentAngle = Mathf.Max(currentAngle - step, targetAngle);
        }

        handwheel.localRotation = initialRotation * Quaternion.Euler(0f, 0f, currentAngle);
    }

    /// <summary>
    /// 修复压力表弧段材质颜色/自发光（Prefab 序列化后可能丢失）。
    /// Mesh 旋转烘焙应在 Editor 中通过 Tools > Pipeline > Fix Gauge Arc Meshes & Materials 完成，
    /// 运行时仅做材质修复 + 安全回退烘焙（Instantiate 副本，不修改共享资产）。
    /// </summary>
    void FixGaugeArcMaterials()
    {
        Color green = new Color(0.18f, 0.82f, 0.30f);
        Color red   = new Color(0.90f, 0.15f, 0.10f);
        int fixedCount = 0;
        int bakedCount = 0;

        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            if (!n.Contains("GreenArc") && !n.Contains("RedArc")) continue;

            // ── 1. 材质修复（总是执行） ──────────────────────────
            MeshRenderer mr = t.GetComponent<MeshRenderer>();
            if (mr == null) continue;

            Material mat = mr.sharedMaterial;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                mr.sharedMaterial = mat;
            }

            Color targetColor = n.Contains("GreenArc") ? green : red;

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_BaseColor", targetColor);
            mat.SetColor("_Color", targetColor);
            mat.SetColor("_EmissionColor", targetColor * 0.6f);
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Glossiness", 0.55f);

            // ── 2. Mesh 旋转烘焙（安全回退：仅在确实需要时执行） ──
            MeshFilter mf = t.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Vector3 euler = t.localRotation.eulerAngles;
                // 仅当 X 轴有明显旋转时才考虑烘焙
                bool hasXRotation = Mathf.Abs(euler.x) > 1f;

                if (hasXRotation)
                {
                    Mesh srcMesh = mf.sharedMesh;
                    Vector3[] testVerts = srcMesh.vertices;

                    // 检测 mesh 是否已被烘焙过（顶点 Y 分量全 ≈ 0 说明已在 XZ 平面）
                    bool alreadyBaked = true;
                    for (int i = 0; i < testVerts.Length; i++)
                    {
                        if (Mathf.Abs(testVerts[i].y) > 0.001f)
                        {
                            alreadyBaked = false;
                            break;
                        }
                    }

                    if (!alreadyBaked)
                    {
                        // ★ 使用 Instantiate 创建运行时副本，绝不修改共享资产 mesh
                        Mesh mesh = Object.Instantiate(srcMesh);
                        mesh.name = srcMesh.name + "_RuntimeBaked";

                        Vector3[] verts = mesh.vertices;
                        Vector3[] norms = mesh.normals;

                        for (int i = 0; i < verts.Length; i++)
                        {
                            // 原始：(x, y, 0) 在 XY 平面 → 烘焙到 XZ 平面 → (x, 0, y)
                            verts[i] = new Vector3(verts[i].x, 0f, verts[i].y);
                            norms[i] = Vector3.up;
                        }

                        mesh.vertices = verts;
                        mesh.normals = norms;
                        mesh.RecalculateBounds();

                        mf.sharedMesh = mesh;
                        bakedCount++;
                    }

                    // 无论是否烘焙，归零 localRotation
                    t.localRotation = Quaternion.identity;
                }
            }

            fixedCount++;
        }

        if (fixedCount > 0 || bakedCount > 0)
            Debug.Log("[PipelineSceneRuntime] 已修复 " + fixedCount + " 个压力表弧段材质"
                + (bakedCount > 0 ? "，安全烘焙 " + bakedCount + " 个 mesh（运行时副本）" : "（无需烘焙）"));
    }

    /// <summary>
    /// 修复 Cap（中心轴盖）父级关系。
    /// Cap 是 NeedlePivot 的子物体，且因 Axis.Z 圆柱体带了 localRotation=(90,0,0)。
    /// 父级 Z 轴旋转 + 子级 90° X 旋转 = 万向节锁 → Cap 拉伸变形/异常位移。
    /// 修复：将 Cap 移到 GaugeRoot（Pivot 的父级）下，保持世界位姿不变。
    /// </summary>
    void FixGaugeCaps()
    {
        Transform[] needlePivots = { gaugeP1Needle, gaugeP2Needle };
        int fixedCount = 0;

        foreach (var pivot in needlePivots)
        {
            if (pivot == null) continue;
            Transform gaugeRoot = pivot.parent;
            if (gaugeRoot == null) continue;

            // 倒序遍历，因为要移除子物体
            for (int i = pivot.childCount - 1; i >= 0; i--)
            {
                Transform child = pivot.GetChild(i);
                if (child.name.Contains("_Cap") || child.name.Contains("Cap"))
                {
                    child.SetParent(gaugeRoot, true); // worldPositionStays=true，保持世界位姿
                    fixedCount++;
                }
            }
        }

        if (fixedCount > 0)
            Debug.Log("[PipelineSceneRuntime] 已修复 " + fixedCount + " 个 Cap 父级关系（移出 NeedlePivot，避免万向节锁）");
    }

    /// <summary>
    /// 确保压力表旁有数字读数显示（类似 F1 流量计）。
    /// 优先查找已有对象，未找到则动态创建。
    /// </summary>
    void EnsureGaugeReadouts()
    {
        if (gaugeP1Needle != null && _gaugeP1Readout == null)
            _gaugeP1Readout = FindOrCreateReadout("GaugeP1_Readout", gaugeP1Needle);
        if (gaugeP2Needle != null && _gaugeP2Readout == null)
            _gaugeP2Readout = FindOrCreateReadout("GaugeP2_Readout", gaugeP2Needle);
    }

    TextMesh FindOrCreateReadout(string objName, Transform needleTransform)
    {
        // 先查找是否已存在（Prefab 重建后可能有）
        Transform genRoot = GeneratedRoot;
        Transform gaugeRoot = needleTransform.parent;
        Transform existing = genRoot != null ? genRoot.Find(objName) : null;
        if (existing == null && gaugeRoot != null)
            existing = gaugeRoot.Find(objName);
        if (existing != null)
        {
            TextMesh tm = existing.GetComponent<TextMesh>();
            if (tm != null) return tm;
            tm = existing.gameObject.AddComponent<TextMesh>();
            ConfigureReadoutTextMesh(tm);
            return tm;
        }

        // 动态创建
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(genRoot != null ? genRoot : gaugeRoot, false);

        // 放置在仪表盘旁边（右侧 + 上方）
        Vector3 gaugeWorldPos = needleTransform.position;
        obj.transform.position = gaugeWorldPos + Vector3.up * 1.5f;
        obj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = obj.AddComponent<TextMesh>();
        ConfigureReadoutTextMesh(textMesh);
        textMesh.text = "0.00 MPa";

        return textMesh;
    }

    void ConfigureReadoutTextMesh(TextMesh tm)
    {
        tm.fontSize = 48;
        tm.characterSize = 0.025f * 6f; // SCALE_FACTOR = 6
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1f, 0.85f, 0.1f); // 亮金色，醒目可见
    }

    void UpdateEStopAnimation()
    {
        if (eStopButtonTransform == null) return;

        // 平滑移动按钮深度
        _eStopPressDepth = Mathf.Lerp(_eStopPressDepth, _eStopTargetDepth, Time.deltaTime * 8f);

        Vector3 localPos = eStopButtonTransform.localPosition;
        localPos.z = 0.11f + _eStopPressDepth; // 原位置 + 按下深度
        eStopButtonTransform.localPosition = localPos;

        // 颜色过渡
        if (_eStopMatInstance != null)
        {
            Color targetColor = (_eStopPressed && !_eStopResetting) ? _pressedEStopColor : _normalEStopColor;
            Color currentColor = _eStopMatInstance.HasProperty("_BaseColor")
                ? _eStopMatInstance.GetColor("_BaseColor")
                : _eStopMatInstance.GetColor("_Color");
            Color newColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 5f);

            if (_eStopMatInstance.HasProperty("_BaseColor"))
                _eStopMatInstance.SetColor("_BaseColor", newColor);
            if (_eStopMatInstance.HasProperty("_Color"))
                _eStopMatInstance.SetColor("_Color", newColor);
        }

        // 复位完成
        if (_eStopResetting && _eStopPressDepth < 0.005f)
        {
            _eStopPressed = false;
            _eStopResetting = false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  向 TrainingManager 报告状态
    // ═══════════════════════════════════════════════════════════

    void ReportToTrainingManager()
    {
        if (trainingManager == null) return;

        trainingManager.ReportValveAngle("valve_V1", _v1Angle);
        trainingManager.ReportValveAngle("valve_V2", _v2Angle);
        trainingManager.ReportValveAngle("valve_V3", _v3Angle);
        trainingManager.ReportGaugeValue("gauge_P1", _simPressure1);
        trainingManager.ReportGaugeValue("gauge_P2", _simPressure2);
        trainingManager.ReportGaugeValue("flow_F1", _simFlow);
    }

    // ═══════════════════════════════════════════════════════════
    //  调试
    // ═══════════════════════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        if (_playerTransform == null) return;

        // 绘制交互范围
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(_playerTransform.position, playerInteractRadius);

        // 绘制阀门位置
        DrawValveGizmo(inletValveWheel, "V1");
        DrawValveGizmo(controlValveWheel, "V2");
        DrawValveGizmo(outletValveWheel, "V3");
    }

    void DrawValveGizmo(Transform valve, string label)
    {
        if (valve == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(valve.position, 0.2f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(valve.position + Vector3.up * 0.3f, label);
#endif
    }
}
