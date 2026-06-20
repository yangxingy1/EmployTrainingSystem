using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class LeadTrainGuideController : MonoBehaviour
{
    const string RetiredFireExtinguisherObjectName = "Fire Extinguisher Static";

    [Serializable]
    public class GuideStep
    {
        public string machineObjectName;
        public string title;
        public string approachHint;
        public string demoDescription;
    }

    [Header("Interaction")]
    public float interactionDistance = 24f;
    public KeyCode deviceInteractKey = KeyCode.Return;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode gestureTrainingKey = KeyCode.T;

    [Header("Screen GUI")]
    public string heightAdjustHint = TrainingNavigationShortcuts.HeightAdjustHint;
    public string entryInstruction = "设备交互（回车）   E 引导式教学   T 手势训练";
    public Color panelBackgroundColor = new Color(0.55f, 0.82f, 1f, 0.78f);
    public Color panelTextColor = new Color(0.06f, 0.14f, 0.26f, 1f);
    public int panelFontSize = 17;
    public float panelRightMargin = 16f;
    public float panelTopMargin = 16f;
    public float panelWidth = 460f;

    [Header("Guide Flow")]
    public bool enforceCanonicalOrderOnAwake = true;
    public GuideStep[] steps = CreateDefaultSteps();

    [Header("Guide Visuals")]
    public bool showPathWhenOutOfRange = true;
    public float movementAvatarScale = 0.42f;

    FactoryOneSceneController _factoryController;
    Transform _player;
    Transform[] _stepMachineTransforms;
    int _currentStepIndex;
    bool _demoRunning;
    bool _guideCompleted;
    bool _gestureTrainingStarted;
    string _postInteractPrompt;
    LeadTrainGuidePath _guidePath;
    GameObject _movementAvatar;

    GUIStyle _panelStyle;
    GUIStyle _postPanelStyle;
    Texture2D _panelTexture;

    static bool _hasRememberedMovementAvatarScale;
    static Vector3 _rememberedMovementAvatarLocalScale;
    static bool _hasRememberedGuideReturnProgress;
    static int _rememberedGuideReturnStepIndex;

    public static GuideStep[] CreateDefaultSteps()
    {
        return new[]
        {
            new GuideStep
            {
                machineObjectName = ElectricalControlCabinetBuilder.StaticCabinetName,
                title = "步骤 1：配电柜",
                approachHint = "靠近配电柜，按 E 开始学习主断路器合闸与分闸操作。",
                demoDescription = "演示：合闸送电 → 分闸断电"
            },
            new GuideStep
            {
                machineObjectName = BreakerShutdownStationBuilder.StaticStationName,
                title = "步骤 2：紧急停机作业区",
                approachHint = "靠近断路器停机站，按 E 开始学习紧急断电顺序。",
                demoDescription = "演示：按顺序拉闸 ②→④→①→③"
            },
            new GuideStep
            {
                machineObjectName = CNCTrainingMachineBuilder.StaticMachineName,
                title = "步骤 3：CNC 实训机床",
                approachHint = "靠近 CNC 机床，按 E 开始学习标准开机与加工流程。",
                demoDescription = "演示：上电 → 开门 → 夹紧 → 启动 → 急停 → 复位"
            }
        };
    }

    public void ApplyCanonicalGuideOrder()
    {
        steps = CreateDefaultSteps();
    }

    void Awake()
    {
        _factoryController = FindObjectOfType<FactoryOneSceneController>();
        if (_factoryController != null)
        {
            _factoryController.allowInteractFly = false;
        }

        RemoveRetiredFireExtinguisherFromScene();

        _guidePath = GetComponent<LeadTrainGuidePath>();
        if (_guidePath == null)
        {
            _guidePath = gameObject.AddComponent<LeadTrainGuidePath>();
        }

        if (enforceCanonicalOrderOnAwake)
        {
            ApplyCanonicalGuideOrder();
        }

        _currentStepIndex = 0;
        _guideCompleted = false;
        _demoRunning = false;
        _gestureTrainingStarted = false;
        _postInteractPrompt = null;
        RestoreGuideReturnProgressIfNeeded();

        RemoveGuideSceneReturnInput();
    }

    IEnumerator Start()
    {
        yield return null;
        LeadTrainRegularTrainingController.EnsureExists();

        if (_factoryController != null)
        {
            _factoryController.RepositionCameraToTrainingDevice();
        }

        _player = ResolvePlayerTransform();
        EnsureMovementAvatar();
        CacheStepMachineTransforms();
        LogCurrentStepTarget();
    }

    void CacheStepMachineTransforms()
    {
        if (steps == null || steps.Length == 0)
        {
            _stepMachineTransforms = Array.Empty<Transform>();
            return;
        }

        _stepMachineTransforms = new Transform[steps.Length];
        for (int i = 0; i < steps.Length; i++)
        {
            GuideStep step = steps[i];
            if (step == null || string.IsNullOrEmpty(step.machineObjectName))
            {
                Debug.LogWarning("[LeadTrain] Step " + (i + 1) + " has no machine name.");
                continue;
            }

            GameObject machineRoot = GameObject.Find(step.machineObjectName);
            if (machineRoot == null)
            {
                Debug.LogWarning("[LeadTrain] Missing machine for step " + (i + 1) + ": " + step.machineObjectName);
                continue;
            }

            _stepMachineTransforms[i] = machineRoot.transform;
        }
    }

    void LogCurrentStepTarget()
    {
        if (_currentStepIndex >= steps.Length)
        {
            return;
        }

        GuideStep step = steps[_currentStepIndex];
        Transform machine = GetCurrentStepMachineTransform();
        if (machine == null)
        {
            Debug.LogWarning("[LeadTrain] Step " + (_currentStepIndex + 1) + "/" + steps.Length
                + " -> " + step.machineObjectName + " (not found in scene)");
            return;
        }

        Vector3 position = machine.position;
        Debug.Log("[LeadTrain] Step " + (_currentStepIndex + 1) + "/" + steps.Length
            + " -> " + step.machineObjectName
            + " @ (" + position.x.ToString("F2") + ", " + position.y.ToString("F2") + ", " + position.z.ToString("F2") + ")");
    }

    void Update()
    {
        _player = ResolvePlayerTransform();
        EnsureMovementAvatar();
        UpdateGuideVisuals();

        TrainingNavigationShortcuts.HandleCtrlQ();

        if (_gestureTrainingStarted)
        {
            return;
        }

        if (!_demoRunning && WasGestureTrainingPressed())
        {
            StartGestureTrainingMode();
            return;
        }

        if (_guideCompleted || _demoRunning)
        {
            return;
        }

        if (WasInteractPressed())
        {
            TryStartCurrentStepDemo();
        }

        if (WasDeviceInteractPressed())
        {
            Debug.Log("[LeadTrain] 设备交互功能暂未开放。");
        }
    }

    void OnGUI()
    {
        if (_gestureTrainingStarted)
        {
            return;
        }

        DrawEntryPanel();

        if (!string.IsNullOrEmpty(_postInteractPrompt))
        {
            DrawPostInteractPanel(_postInteractPrompt);
        }
    }

    void StartGestureTrainingMode()
    {
        if (_gestureTrainingStarted)
        {
            return;
        }

        string machineName = ResolveGestureTrainingMachineName();
        RememberGuideReturnProgress(machineName);
        FactoryOneSceneController.ClearOneShotStartCameraReturnOverride();
        RememberGestureTrainingReturnPose();
        _gestureTrainingStarted = true;
        _demoRunning = false;
        _postInteractPrompt = null;
        HideGuideVisuals();
        if (_movementAvatar != null)
        {
            RememberMovementAvatarScale(_movementAvatar.transform);
            _movementAvatar.SetActive(false);
        }

        if (machineName == ElectricalControlCabinetBuilder.StaticCabinetName)
        {
            StartElectricalCabinetGestureTraining();
            return;
        }

        if (machineName == CNCTrainingMachineBuilder.StaticMachineName)
        {
            StartCNCGestureTraining();
            return;
        }

        StartBreakerGestureTraining();
    }

    void RememberGestureTrainingReturnPose()
    {
        Camera camera = _factoryController != null && _factoryController.playerCamera != null
            ? _factoryController.playerCamera
            : Camera.main;

        if (camera == null)
        {
            Debug.LogWarning("[LeadTrain] 进入手势训练前未找到相机，无法记录返回位置。");
            return;
        }

        Transform rig = camera.transform.parent;
        if (rig == null && _factoryController != null)
        {
            rig = _factoryController.transform;
        }
        if (rig == null)
        {
            rig = ResolvePlayerTransform();
        }

        if (rig == null)
        {
            Debug.LogWarning("[LeadTrain] 进入手势训练前未找到玩家 Rig，无法记录返回位置。");
            return;
        }

        Vector3 rigPosition = rig.position;
        Vector3 cameraLocalPosition = camera.transform.localPosition;
        FactoryOneSceneController.RememberOneShotStartCameraPose(
            rigPosition,
            rig.rotation,
            cameraLocalPosition,
            camera.transform.localRotation,
            camera.transform.position,
            camera.transform.rotation);

        Vector3 position = rigPosition;
        Debug.Log("[LeadTrain] 已记录手势训练返回位置：("
            + position.x.ToString("F2") + ", "
            + position.y.ToString("F2") + ", "
            + position.z.ToString("F2") + ")");
    }

    void RememberGuideReturnProgress(string machineName)
    {
        int stepIndex = ResolveStepIndexForMachine(machineName);
        if (stepIndex < 0)
        {
            stepIndex = _currentStepIndex;
        }

        _rememberedGuideReturnStepIndex = Mathf.Clamp(stepIndex + 1, 0, steps != null ? steps.Length : 0);
        _hasRememberedGuideReturnProgress = true;

        int stepCount = steps != null ? steps.Length : 0;
        if (_rememberedGuideReturnStepIndex >= stepCount)
        {
            Debug.Log("[LeadTrain] 已记录返回后的引导进度：返回后完成全部步骤。");
            return;
        }

        Debug.Log("[LeadTrain] 已记录返回后的引导进度：下一步 "
            + (_rememberedGuideReturnStepIndex + 1)
            + "/"
            + stepCount);
    }

    void RestoreGuideReturnProgressIfNeeded()
    {
        if (!_hasRememberedGuideReturnProgress)
        {
            return;
        }

        _currentStepIndex = Mathf.Clamp(_rememberedGuideReturnStepIndex, 0, steps != null ? steps.Length : 0);
        _guideCompleted = steps == null || _currentStepIndex >= steps.Length;
        _hasRememberedGuideReturnProgress = false;

        if (_guideCompleted)
        {
            Debug.Log("[LeadTrain] 已恢复引导进度：全部步骤已完成。");
            return;
        }

        Debug.Log("[LeadTrain] 已恢复引导进度：继续步骤 "
            + (_currentStepIndex + 1)
            + "/"
            + steps.Length);
    }

    int ResolveStepIndexForMachine(string machineName)
    {
        if (steps == null || string.IsNullOrEmpty(machineName))
        {
            return -1;
        }

        for (int i = 0; i < steps.Length; i++)
        {
            if (steps[i] != null && steps[i].machineObjectName == machineName)
            {
                return i;
            }
        }

        return -1;
    }

    void StartElectricalCabinetGestureTraining()
    {
        GameObject trainingObject = GameObject.Find("LeadTrain1ElectricalCabinetGestureTraining");
        if (trainingObject == null)
        {
            trainingObject = new GameObject("LeadTrain1ElectricalCabinetGestureTraining");
        }

        LeadTrainElectricalCabinetGestureTrainingController training = trainingObject.GetComponent<LeadTrainElectricalCabinetGestureTrainingController>();
        if (training == null)
        {
            training = trainingObject.AddComponent<LeadTrainElectricalCabinetGestureTrainingController>();
        }

        training.BeginTrainingScene();
        Debug.Log("[LeadTrain] 已进入配电柜主断路器手势训练。");
    }

    void StartCNCGestureTraining()
    {
        GameObject trainingObject = GameObject.Find("LeadTrain1CNCGestureTraining");
        if (trainingObject == null)
        {
            trainingObject = new GameObject("LeadTrain1CNCGestureTraining");
        }

        LeadTrainCNCGestureTrainingController training = trainingObject.GetComponent<LeadTrainCNCGestureTrainingController>();
        if (training == null)
        {
            training = trainingObject.AddComponent<LeadTrainCNCGestureTrainingController>();
        }

        training.BeginTrainingScene();
        Debug.Log("[LeadTrain] 已进入 CNC 8 步手势真实训练。");
    }

    void StartBreakerGestureTraining()
    {
        GameObject trainingObject = GameObject.Find("LeadTrain1GestureTraining");
        if (trainingObject == null)
        {
            trainingObject = new GameObject("LeadTrain1GestureTraining");
        }

        LeadTrainGestureTrainingController training = trainingObject.GetComponent<LeadTrainGestureTrainingController>();
        if (training == null)
        {
            training = trainingObject.AddComponent<LeadTrainGestureTrainingController>();
        }

        training.BeginTrainingScene();
        Debug.Log("[LeadTrain] 已进入四电闸手势真实训练。");
    }

    string ResolveGestureTrainingMachineName()
    {
        if (_currentStepIndex >= 0 && steps != null && _currentStepIndex < steps.Length && steps[_currentStepIndex] != null)
        {
            Transform currentMachine = GetStepMachineTransform(_currentStepIndex);
            if (currentMachine != null && IsInRange(currentMachine))
            {
                return steps[_currentStepIndex].machineObjectName;
            }
        }

        Transform nearest = null;
        string nearestName = "";
        float nearestDistance = float.MaxValue;
        Vector3 probe = ResolvePlayerProbePosition();

        if (steps != null)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                GuideStep step = steps[i];
                if (step == null || string.IsNullOrEmpty(step.machineObjectName))
                {
                    continue;
                }

                Transform machine = GetStepMachineTransform(i);
                if (machine == null)
                {
                    continue;
                }

                float distance = HorizontalDistance(probe, machine.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = machine;
                    nearestName = step.machineObjectName;
                }
            }
        }

        if (nearest != null && nearestDistance <= interactionDistance)
        {
            return nearestName;
        }

        if (_currentStepIndex >= 0 && steps != null && _currentStepIndex < steps.Length && steps[_currentStepIndex] != null)
        {
            return steps[_currentStepIndex].machineObjectName;
        }

        return BreakerShutdownStationBuilder.StaticStationName;
    }

    void EnsureMovementAvatar()
    {
        if (_gestureTrainingStarted || _player == null)
        {
            return;
        }

        Vector3 localPosition = ResolveMovementAvatarLocalPosition();

        if (_movementAvatar != null)
        {
            ApplyMovementAvatarPose(_movementAvatar.transform, localPosition);
            return;
        }

        Transform existing = _player.Find("LeadTrain Safety Trainee Avatar");
        if (existing != null)
        {
            _movementAvatar = existing.gameObject;
            ApplyMovementAvatarPose(existing, localPosition);
            return;
        }

        _movementAvatar = SafetyTraineeAvatar.Create(
            _player,
            "LeadTrain Safety Trainee Avatar",
            localPosition,
            movementAvatarScale);
        ApplyMovementAvatarPose(_movementAvatar.transform, localPosition);
        Debug.Log("[LeadTrain] 已创建移动小人：" + localPosition);
    }

    void RememberMovementAvatarScale(Transform avatar)
    {
        if (avatar == null || !IsValidVector(avatar.localScale))
        {
            return;
        }

        _rememberedMovementAvatarLocalScale = avatar.localScale;
        _hasRememberedMovementAvatarScale = true;
    }

    void ApplyMovementAvatarPose(Transform avatar, Vector3 localPosition)
    {
        if (avatar == null)
        {
            return;
        }

        avatar.localPosition = localPosition;
        avatar.localRotation = Quaternion.identity;
        avatar.localScale = ResolveMovementAvatarLocalScale();
    }

    Vector3 ResolveMovementAvatarLocalScale()
    {
        if (_hasRememberedMovementAvatarScale && IsValidVector(_rememberedMovementAvatarLocalScale))
        {
            return _rememberedMovementAvatarLocalScale;
        }

        return Vector3.one * Mathf.Max(0.1f, movementAvatarScale);
    }

    void RemoveGuideSceneReturnInput()
    {
        ReturnToHubInput[] returnInputs = FindObjectsOfType<ReturnToHubInput>();
        for (int i = 0; i < returnInputs.Length; i++)
        {
            if (returnInputs[i] != null)
            {
                Destroy(returnInputs[i]);
            }
        }
    }

    Vector3 ResolveMovementAvatarLocalPosition()
    {
        return Vector3.zero;
    }

    void RemoveRetiredFireExtinguisherFromScene()
    {
        GameObject extinguisher = GameObject.Find(RetiredFireExtinguisherObjectName);
        if (extinguisher == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(extinguisher);
        }
        else
        {
            DestroyImmediate(extinguisher);
        }

        Debug.Log("[LeadTrain] 已移除灭火器组件，当前引导训练仅保留配电柜、紧急停机作业区、CNC。");
    }

    void DrawEntryPanel()
    {
        EnsurePanelStyle();
        const float lineHeight = 22f;
        const int lineCount = 3;
        float height = lineCount * lineHeight + 24f;
        Rect rect = BuildTopRightRect(height);
        GUI.Box(rect, BuildEntryPanelText(), _panelStyle);
    }

    string BuildEntryPanelText()
    {
        return heightAdjustHint + "\n"
            + TrainingNavigationShortcuts.GetCtrlQHint() + "\n"
            + entryInstruction;
    }

    void DrawPostInteractPanel(string message)
    {
        EnsurePanelStyle();
        EnsurePostPanelStyle();
        const float height = 84f;
        Rect rect = BuildTopRightRect(height, 72f);
        GUI.Box(rect, message, _postPanelStyle);
    }

    void EnsurePostPanelStyle()
    {
        if (_postPanelStyle != null)
        {
            return;
        }

        _postPanelStyle = new GUIStyle(_panelStyle)
        {
            fontStyle = FontStyle.Normal,
            fontSize = panelFontSize - 1,
            alignment = TextAnchor.UpperLeft
        };
    }

    Rect BuildTopRightRect(float height, float topOffset = 0f)
    {
        float x = Screen.width - panelWidth - panelRightMargin;
        float y = panelTopMargin + topOffset;
        return new Rect(x, y, panelWidth, height);
    }

    void EnsurePanelStyle()
    {
        if (_panelTexture == null)
        {
            _panelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _panelTexture.SetPixel(0, 0, panelBackgroundColor);
            _panelTexture.Apply();
        }

        if (_panelStyle == null)
        {
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                wordWrap = true,
                fontSize = panelFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                richText = true
            };
            _panelStyle.normal.textColor = panelTextColor;
            _panelStyle.padding = new RectOffset(14, 14, 12, 12);
        }

        _panelStyle.fontSize = panelFontSize;
        _panelStyle.normal.textColor = panelTextColor;
        _panelStyle.normal.background = _panelTexture;
    }

    void TryStartCurrentStepDemo()
    {
        if (_currentStepIndex >= steps.Length)
        {
            return;
        }

        Transform machine = GetCurrentStepMachineTransform();
        if (!IsInRange(machine))
        {
            return;
        }

        StartCoroutine(RunStepDemo(steps[_currentStepIndex]));
    }

    IEnumerator RunStepDemo(GuideStep step)
    {
        _demoRunning = true;
        _postInteractPrompt = step.title + "\n" + step.demoDescription;

        Transform machineTransform = GetStepMachineTransform(_currentStepIndex);
        if (machineTransform == null)
        {
            Debug.LogWarning("[LeadTrain] 未找到设备：" + step.machineObjectName);
            _demoRunning = false;
            _postInteractPrompt = null;
            yield break;
        }

        if (!TryPlayCurrentMachineGuidedSequence(machineTransform.gameObject, step, out IEnumerator guidedSequence))
        {
            Debug.LogWarning("[LeadTrain] 设备上未找到可演示的运行时组件：" + step.machineObjectName);
            _demoRunning = false;
            _postInteractPrompt = null;
            yield break;
        }

        yield return guidedSequence;

        int completedStepIndex = _currentStepIndex;
        GuideStep completedStep = steps[completedStepIndex];
        _currentStepIndex++;
        _demoRunning = false;
        _postInteractPrompt = null;

        LeadTrainRegularTrainingController regularTraining = FindObjectOfType<LeadTrainRegularTrainingController>();
        if (regularTraining != null && completedStep != null)
        {
            regularTraining.NotifyGuideStepCompleted(completedStep.machineObjectName);
        }

        if (_currentStepIndex >= steps.Length)
        {
            _guideCompleted = true;
            regularTraining?.NotifyGuideFlowCompleted();
            HideGuideVisuals();
            yield break;
        }

        LogCurrentStepTarget();
    }

    bool TryPlayCurrentMachineGuidedSequence(GameObject machineRoot, GuideStep step, out IEnumerator guidedSequence)
    {
        guidedSequence = null;

        if (step.machineObjectName == ElectricalControlCabinetBuilder.StaticCabinetName)
        {
            ElectricalCabinetEInteraction cabinet = machineRoot.GetComponentInChildren<ElectricalCabinetEInteraction>();
            if (cabinet != null)
            {
                guidedSequence = cabinet.PlayGuidedSequence();
                return true;
            }

            return false;
        }

        if (step.machineObjectName == CNCTrainingMachineBuilder.StaticMachineName)
        {
            CNCTrainingMachineRuntime cnc = machineRoot.GetComponentInChildren<CNCTrainingMachineRuntime>();
            if (cnc != null)
            {
                guidedSequence = cnc.PlayGuidedSequence();
                return true;
            }

            return false;
        }

        if (step.machineObjectName == BreakerShutdownStationBuilder.StaticStationName)
        {
            BreakerShutdownStationRuntime breaker = machineRoot.GetComponentInChildren<BreakerShutdownStationRuntime>();
            if (breaker != null)
            {
                guidedSequence = breaker.PlayGuidedSequence();
                return true;
            }

            return false;
        }

        return false;
    }

    void UpdateGuideVisuals()
    {
        if (_gestureTrainingStarted || _guideCompleted || _currentStepIndex >= steps.Length || _demoRunning)
        {
            HideGuideVisuals();
            return;
        }

        Transform machine = GetCurrentStepMachineTransform();
        if (machine == null || _player == null)
        {
            HideGuideVisuals();
            return;
        }

        bool inRange = IsInRange(machine);
        bool showPath = showPathWhenOutOfRange ? !inRange : !_demoRunning;
        if (_guidePath != null)
        {
            _guidePath.SetVisible(showPath);
            if (showPath)
            {
                Vector3 pathStart = ResolvePlayerProbePosition();
                _guidePath.UpdatePath(pathStart, machine.position);
            }
        }
    }

    void HideGuideVisuals()
    {
        if (_guidePath != null)
        {
            _guidePath.SetVisible(false);
        }
    }

    Transform GetCurrentStepMachineTransform()
    {
        return GetStepMachineTransform(_currentStepIndex);
    }

    Transform GetStepMachineTransform(int stepIndex)
    {
        if (_stepMachineTransforms != null
            && stepIndex >= 0
            && stepIndex < _stepMachineTransforms.Length
            && _stepMachineTransforms[stepIndex] != null)
        {
            return _stepMachineTransforms[stepIndex];
        }

        if (steps == null || stepIndex < 0 || stepIndex >= steps.Length)
        {
            return null;
        }

        GameObject found = GameObject.Find(steps[stepIndex].machineObjectName);
        return found != null ? found.transform : null;
    }

    Vector3 ResolvePlayerProbePosition()
    {
        if (_player != null)
        {
            return _player.position;
        }

        if (_factoryController != null && _factoryController.playerCamera != null)
        {
            return _factoryController.playerCamera.transform.position;
        }

        return transform.position;
    }

    bool IsInRange(Transform machine)
    {
        if (machine == null)
        {
            return false;
        }

        Vector3 probe = ResolvePlayerProbePosition();
        if (_player == null && _factoryController == null)
        {
            return false;
        }

        return HorizontalDistance(probe, machine.position) <= interactionDistance;
    }

    Transform ResolvePlayerTransform()
    {
        if (_factoryController != null && _factoryController.playerCamera != null)
        {
            return _factoryController.playerCamera.transform.parent != null
                ? _factoryController.playerCamera.transform.parent
                : _factoryController.playerCamera.transform;
        }

        CharacterController controller = FindObjectOfType<CharacterController>();
        if (controller != null)
        {
            return controller.transform;
        }

        if (Camera.main != null && Camera.main.transform.parent != null)
        {
            return Camera.main.transform.parent;
        }

        return Camera.main != null ? Camera.main.transform : transform;
    }

    bool WasInteractPressed()
    {
        if (Input.GetKeyDown(interactKey))
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

        return false;
    }

    bool WasGestureTrainingPressed()
    {
        if (Input.GetKeyDown(gestureTrainingKey))
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

        return false;
    }

    bool WasDeviceInteractPressed()
    {
        if (Input.GetKeyDown(deviceInteractKey) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                return true;
            }
        }
#endif

        return false;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    static bool IsValidVector(Vector3 value)
    {
        return IsValidFloat(value.x)
            && IsValidFloat(value.y)
            && IsValidFloat(value.z)
            && value.sqrMagnitude > 0.0001f;
    }

    static bool IsValidFloat(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
