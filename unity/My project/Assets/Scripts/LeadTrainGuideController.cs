using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class LeadTrainGuideController : MonoBehaviour
{
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

    FactoryOneSceneController _factoryController;
    Transform _player;
    Transform[] _stepMachineTransforms;
    int _currentStepIndex;
    bool _demoRunning;
    bool _guideCompleted;
    string _postInteractPrompt;
    LeadTrainGuidePath _guidePath;

    GUIStyle _panelStyle;
    GUIStyle _postPanelStyle;
    Texture2D _panelTexture;

    public static GuideStep[] CreateDefaultSteps()
    {
        return new[]
        {
            new GuideStep
            {
                machineObjectName = FireExtinguisherBuilder.StaticExtinguisherName,
                title = "步骤 1：灭火器检查",
                approachHint = "靠近灭火器，按 E 开始学习压力表检查方法。",
                demoDescription = "演示：检查压力表指针是否位于绿色区域"
            },
            new GuideStep
            {
                machineObjectName = ElectricalControlCabinetBuilder.StaticCabinetName,
                title = "步骤 2：配电柜",
                approachHint = "靠近配电柜，按 E 开始学习主断路器合闸与分闸操作。",
                demoDescription = "演示：合闸送电 → 分闸断电"
            },
            new GuideStep
            {
                machineObjectName = BreakerShutdownStationBuilder.StaticStationName,
                title = "步骤 3：紧急停机作业区",
                approachHint = "靠近断路器停机站，按 E 开始学习紧急断电顺序。",
                demoDescription = "演示：按顺序拉闸 ②→④→①→③"
            },
            new GuideStep
            {
                machineObjectName = CNCTrainingMachineBuilder.StaticMachineName,
                title = "步骤 4：CNC 实训机床",
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
        _postInteractPrompt = null;
    }

    IEnumerator Start()
    {
        yield return null;
        _player = ResolvePlayerTransform();
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
        UpdateGuideVisuals();

        if (_guideCompleted || _demoRunning)
        {
            return;
        }

        if (WasInteractPressed())
        {
            TryStartCurrentStepDemo();
        }

        if (WasGestureTrainingPressed())
        {
            Debug.Log("[LeadTrain] 手势训练功能暂未开放。");
        }

        if (WasDeviceInteractPressed())
        {
            Debug.Log("[LeadTrain] 设备交互功能暂未开放。");
        }
    }

    void OnGUI()
    {
        DrawEntryPanel();

        if (!string.IsNullOrEmpty(_postInteractPrompt))
        {
            DrawPostInteractPanel(_postInteractPrompt);
        }
    }

    void DrawEntryPanel()
    {
        EnsurePanelStyle();
        const float height = 64f;
        Rect rect = BuildTopRightRect(height);
        GUI.Box(rect, entryInstruction, _panelStyle);
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
                alignment = TextAnchor.MiddleCenter,
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

        _currentStepIndex++;
        _demoRunning = false;
        _postInteractPrompt = null;

        if (_currentStepIndex >= steps.Length)
        {
            _guideCompleted = true;
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

        if (step.machineObjectName == FireExtinguisherBuilder.StaticExtinguisherName)
        {
            FireExtinguisherGaugeInteraction extinguisher = machineRoot.GetComponentInChildren<FireExtinguisherGaugeInteraction>();
            if (extinguisher != null)
            {
                guidedSequence = extinguisher.PlayGuidedSequence();
                return true;
            }

            return false;
        }

        return false;
    }

    void UpdateGuideVisuals()
    {
        if (_guideCompleted || _currentStepIndex >= steps.Length || _demoRunning)
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
}
