using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CNCTrainingMachineRuntime : MonoBehaviour
{
    public static readonly Color StatusTextPoweredOn = new Color(0.78f, 1f, 0.88f);
    static readonly Color ModeKnobGreen = new Color(0.14f, 0.78f, 0.32f);
    static readonly Color ClampAccentOrange = new Color(0.88f, 0.42f, 0.10f);
    static readonly Color ScreenOnLightBlue = new Color(0.58f, 0.82f, 0.96f);
    public static readonly Color StatusTextIdle = new Color(0.82f, 0.90f, 0.98f);
    public static readonly Color StatusTextGuide = new Color(1f, 0.92f, 0.38f);
    public static readonly Color StatusTextAlert = new Color(1f, 0.58f, 0.34f);

    [Header("Keyboard Interaction")]
    public KeyCode powerKey = KeyCode.Alpha1;
    public KeyCode doorKey = KeyCode.Alpha2;
    public KeyCode clampKey = KeyCode.Alpha3;
    public KeyCode cycleStartKey = KeyCode.Alpha4;
    public KeyCode emergencyStopKey = KeyCode.Alpha5;
    public KeyCode resetKey = KeyCode.Alpha6;
    public KeyCode modeKey = KeyCode.Alpha7;

    [Header("Machine Parts")]
    public Transform leftDoor;
    public Transform rightDoor;
    public Transform leftClampJaw;
    public Transform rightClampJaw;
    public Transform clampHandlePivot;
    public Transform powerSwitchPivot;
    public Transform modeKnob;
    public Transform spindleRotator;
    public Transform tool;
    public Transform workpiece;

    [Header("Renderers")]
    public Renderer screenRenderer;
    public Renderer powerSwitchRenderer;
    public Renderer cycleStartRenderer;
    public Renderer emergencyStopRenderer;
    public Renderer stackRedRenderer;
    public Renderer stackYellowRenderer;
    public Renderer stackGreenRenderer;

    [Header("Animated Buttons")]
    public Transform cycleStartButtonTransform;
    public Transform emergencyStopButtonTransform;
    public Transform resetButtonTransform;

    [Header("Text")]
    public TextMesh statusText;

    [Header("Materials")]
    public Material matScreenOn;
    public Material matScreenOff;
    public Material matGreen;
    public Material matRed;
    public Material matYellow;
    public Material matGray;
    public Material matLedOff;

    [Header("State")]
    public bool powerOn = false;
    public bool doorClosed = true;
    public bool workpieceClamped = false;
    public bool running = false;
    public bool emergencyStopped = false;
    public bool autoMode = true;

    [Header("Presentation")]
    public bool enableFrontFillLight = true;
    public float frontFillLightIntensity = 1.45f;
    public float machineSurfaceBrightness = 1.24f;

    [Header("Guided Teaching")]
    public float guidedStepPause = 1.8f;
    public float guidedDoorPause = 1.3f;
    public float guidedClampPause = 1.3f;
    public float guidedRunPause = 4.0f;
    public float guidedActionPause = 1.6f;
    public float guidedAnimationDurationScale = 1.75f;

    private Vector3 leftDoorClosedPos;
    private Vector3 rightDoorClosedPos;
    private Vector3 leftClampOpenPos;
    private Vector3 rightClampOpenPos;
    private Vector3 cycleStartButtonReadyPos;
    private Vector3 emergencyStopButtonReadyPos;
    private Vector3 resetButtonReadyPos;

    private Coroutine doorCoroutine;
    private Coroutine clampCoroutine;
    private Coroutine cycleStartButtonCoroutine;
    private Coroutine emergencyStopButtonCoroutine;
    private Coroutine resetButtonCoroutine;

    private bool suppressGuidedInput;
    private Material _resolvedScreenOnMaterial;
    private Material _modeKnobBodyMaterial;
    private Material _clampAccentMaterial;
    private Light _frontFillLight;
    private Light _chamberLight;
    private Vector3 _workpieceRestLocalPos;
    private bool _workpieceRestCaptured;

    private float AnimationDurationScale => suppressGuidedInput ? guidedAnimationDurationScale : 1f;

    private void Awake()
    {
        RefreshSurfaceLabels();
        MarkRuntimePartsDynamic();
        EnsurePresentation();
    }

    private void RefreshSurfaceLabels()
    {
        ApplySurfaceLabel("Machine_Name_Label", "CNC 实训机床", CNCUiFont.Positions.MachineName, CNCUiFont.Sizes.MachineName, Color.white, bold: true);
        ApplySurfaceLabel("Door_Instruction_Label", "安全门", CNCUiFont.Positions.DoorInstruction, CNCUiFont.Sizes.DoorInstruction, Color.yellow);
        ApplySurfaceLabel(
            "Teaching_Sequence_Label",
            "1电源 2安全门 3夹具 4启动 5急停 6复位 7模式",
            CNCUiFont.Positions.TeachingSequence,
            CNCUiFont.Sizes.TeachingSequence,
            new Color(0.55f, 0.92f, 1f),
            bold: true);
        ApplySurfaceLabel("Label_MainPower", "1 电源", CNCUiFont.Positions.MainPower, CNCUiFont.Sizes.ButtonHint, Color.white);
        ApplySurfaceLabel("Label_CycleStart", "4 启动", CNCUiFont.Positions.CycleStart, CNCUiFont.Sizes.ButtonHint, Color.white);
        ApplySurfaceLabel("Label_EStop", "5 急停", CNCUiFont.Positions.EStop, CNCUiFont.Sizes.ButtonHint, Color.red);
        ApplySurfaceLabel("Label_Reset", "6 复位", CNCUiFont.Positions.Reset, CNCUiFont.Sizes.ButtonHint, Color.white);
        ApplySurfaceLabel("Label_Mode", "7 模式", CNCUiFont.Positions.Mode, CNCUiFont.Sizes.ButtonHint, Color.white);
        ApplySurfaceLabel("Clamp_Label", "夹具", ResolveDoorWindowCenter() + new Vector3(0f, 0.44f, 0.06f), CNCUiFont.Sizes.DoorInstruction, Color.white);
        ApplySurfaceLabel("Label_PowerOn", "开", CNCUiFont.Positions.PowerOn, CNCUiFont.Sizes.PowerOnOff, new Color(0.2f, 0.9f, 0.3f));
        ApplySurfaceLabel("Label_PowerOff", "关", CNCUiFont.Positions.PowerOff, CNCUiFont.Sizes.PowerOnOff, new Color(0.75f, 0.75f, 0.75f));

        if (statusText != null)
        {
            statusText.transform.localPosition = CNCUiFont.Positions.Status;
            statusText.transform.localRotation = CNCUiFont.PanelFaceRotation;
            statusText.characterSize = CNCUiFont.Sizes.Status;
            statusText.lineSpacing = 0.82f;
            CNCUiFont.Apply(statusText, CNCUiFont.Sizes.Status);
        }

        RemoveModeSelectorTextLabels();
    }

    private void ApplySurfaceLabel(string objectName, string text, Vector3 position, float characterSize, Color color, bool bold = false)
    {
        Transform labelTransform = FindChildRecursive(transform, objectName);
        if (labelTransform == null)
        {
            return;
        }

        TextMesh textMesh = labelTransform.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            return;
        }

        labelTransform.localPosition = position;
        labelTransform.localRotation = CNCUiFont.PanelFaceRotation;
        textMesh.text = text;
        textMesh.characterSize = characterSize;
        textMesh.color = color;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.lineSpacing = 0.82f;
        CNCUiFont.Apply(textMesh, characterSize, bold);
    }

    private Transform FindChildRecursive(Transform root, string targetName)
    {
        if (root.name == targetName)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            Transform found = FindChildRecursive(child, targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void Start()
    {
        if (leftDoor != null) leftDoorClosedPos = leftDoor.localPosition;
        if (rightDoor != null) rightDoorClosedPos = rightDoor.localPosition;
        EnsurePresentation();
        if (leftClampJaw != null) leftClampOpenPos = leftClampJaw.localPosition;
        if (rightClampJaw != null) rightClampOpenPos = rightClampJaw.localPosition;
        if (workpiece != null)
        {
            _workpieceRestLocalPos = workpiece.localPosition;
            _workpieceRestCaptured = true;
        }
        if (cycleStartButtonTransform != null) cycleStartButtonReadyPos = cycleStartButtonTransform.localPosition;
        if (emergencyStopButtonTransform != null) emergencyStopButtonReadyPos = emergencyStopButtonTransform.localPosition;
        if (resetButtonTransform != null) resetButtonReadyPos = resetButtonTransform.localPosition;

        ApplyInitialState();
        Debug.Log("[CNC] 键盘就绪：1电源 2安全门 3夹具 4启动 5急停 6复位 7模式。");
    }

    private void Update()
    {
        if (running && spindleRotator != null)
        {
            spindleRotator.Rotate(Vector3.up, 900f * Time.deltaTime, Space.Self);
        }

        UpdateChamberDynamics();
        HandleKeyboardInput();
    }

    public void ApplyInitialState()
    {
        UpdateVisuals();
        UpdateStatus("电源关闭");
    }

    private void MarkRuntimePartsDynamic()
    {
        MarkTransformHierarchyDynamic(leftDoor);
        MarkTransformHierarchyDynamic(rightDoor);
        MarkTransformHierarchyDynamic(leftClampJaw);
        MarkTransformHierarchyDynamic(rightClampJaw);
        MarkTransformHierarchyDynamic(clampHandlePivot);
        MarkTransformHierarchyDynamic(powerSwitchPivot);
        MarkTransformHierarchyDynamic(modeKnob);
        MarkTransformHierarchyDynamic(spindleRotator);
        MarkTransformHierarchyDynamic(tool);

        MarkRendererDynamic(screenRenderer);
        MarkRendererDynamic(powerSwitchRenderer);
        MarkRendererDynamic(cycleStartRenderer);
        MarkRendererDynamic(emergencyStopRenderer);
        MarkRendererDynamic(stackRedRenderer);
        MarkRendererDynamic(stackYellowRenderer);
        MarkRendererDynamic(stackGreenRenderer);
        MarkTransformHierarchyDynamic(cycleStartButtonTransform);
        MarkTransformHierarchyDynamic(emergencyStopButtonTransform);
        MarkTransformHierarchyDynamic(resetButtonTransform);

        if (statusText != null)
        {
            statusText.gameObject.isStatic = false;
        }
    }

    private void MarkTransformHierarchyDynamic(Transform root)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.isStatic = false;

        foreach (Transform child in root)
        {
            MarkTransformHierarchyDynamic(child);
        }
    }

    private static void MarkRendererDynamic(Renderer renderer)
    {
        if (renderer != null)
        {
            renderer.gameObject.isStatic = false;
        }
    }

    private void HandleKeyboardInput()
    {
        if (suppressGuidedInput)
        {
            return;
        }

        if (WasPressed(powerKey, KeyCode.Keypad1)) HandleKeyInteraction("1", CNCInteractionType.TogglePower);
        else if (WasPressed(doorKey, KeyCode.Keypad2)) HandleKeyInteraction("2", CNCInteractionType.ToggleDoor);
        else if (WasPressed(clampKey, KeyCode.Keypad3)) HandleKeyInteraction("3", CNCInteractionType.ToggleClamp);
        else if (WasPressed(cycleStartKey, KeyCode.Keypad4)) HandleKeyInteraction("4", CNCInteractionType.CycleStart);
        else if (WasPressed(emergencyStopKey, KeyCode.Keypad5)) HandleKeyInteraction("5", CNCInteractionType.EmergencyStop);
        else if (WasPressed(resetKey, KeyCode.Keypad6)) HandleKeyInteraction("6", CNCInteractionType.Reset);
        else if (WasPressed(modeKey, KeyCode.Keypad7)) HandleKeyInteraction("7", CNCInteractionType.ToggleMode);
    }

    private static bool WasPressed(KeyCode primaryKey, KeyCode keypadKey)
    {
        if (Input.GetKeyDown(primaryKey) || Input.GetKeyDown(keypadKey))
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return false;
        }

        Key? primarySystemKey = KeyCodeToInputSystemKey(primaryKey);
        if (primarySystemKey.HasValue && Keyboard.current[primarySystemKey.Value].wasPressedThisFrame)
        {
            return true;
        }

        Key? keypadSystemKey = KeyCodeToInputSystemKey(keypadKey);
        if (keypadSystemKey.HasValue && Keyboard.current[keypadSystemKey.Value].wasPressedThisFrame)
        {
            return true;
        }
#endif

        return false;
    }

#if ENABLE_INPUT_SYSTEM
    private static Key? KeyCodeToInputSystemKey(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.Alpha1: return Key.Digit1;
            case KeyCode.Alpha2: return Key.Digit2;
            case KeyCode.Alpha3: return Key.Digit3;
            case KeyCode.Alpha4: return Key.Digit4;
            case KeyCode.Alpha5: return Key.Digit5;
            case KeyCode.Alpha6: return Key.Digit6;
            case KeyCode.Alpha7: return Key.Digit7;
            case KeyCode.Keypad1: return Key.Numpad1;
            case KeyCode.Keypad2: return Key.Numpad2;
            case KeyCode.Keypad3: return Key.Numpad3;
            case KeyCode.Keypad4: return Key.Numpad4;
            case KeyCode.Keypad5: return Key.Numpad5;
            case KeyCode.Keypad6: return Key.Numpad6;
            case KeyCode.Keypad7: return Key.Numpad7;
            default: return null;
        }
    }
#endif

    private void HandleKeyInteraction(string keyLabel, CNCInteractionType type)
    {
        Debug.Log("[CNC] Key " + keyLabel + " detected -> " + type);
        HandleInteraction(type);
    }

    public void HandleInteraction(CNCInteractionType type)
    {
        switch (type)
        {
            case CNCInteractionType.TogglePower:
                TogglePower();
                break;

            case CNCInteractionType.ToggleDoor:
                ToggleDoor();
                break;

            case CNCInteractionType.ToggleClamp:
                ToggleClamp();
                break;

            case CNCInteractionType.CycleStart:
                TryCycleStart();
                break;

            case CNCInteractionType.EmergencyStop:
                EmergencyStop();
                break;

            case CNCInteractionType.Reset:
                ResetMachine();
                break;

            case CNCInteractionType.ToggleMode:
                ToggleMode();
                break;
        }
    }

    private void TogglePower()
    {
        if (emergencyStopped)
        {
            UpdateStatus("请先复位");
            return;
        }

        powerOn = !powerOn;

        if (!powerOn)
        {
            running = false;
            UpdateStatus("电源关闭");
        }
        else
        {
            UpdateStatus("已上电：请开门并夹紧");
        }

        if (powerSwitchPivot != null)
        {
            powerSwitchPivot.localRotation = powerOn ? Quaternion.Euler(0f, 0f, -35f) : Quaternion.identity;
        }

        UpdateVisuals();
    }

    private void ToggleDoor()
    {
        if (running)
        {
            UpdateStatus("运行中禁止开门");
            return;
        }

        doorClosed = !doorClosed;

        if (doorCoroutine != null)
        {
            StopCoroutine(doorCoroutine);
        }

        doorCoroutine = StartCoroutine(AnimateDoor(doorClosed));

        UpdateStatus(doorClosed ? "安全门已关" : "安全门已开：请夹紧工件");
        UpdateVisuals();
    }

    private IEnumerator AnimateDoor(bool close)
    {
        Vector3 leftTarget = close ? leftDoorClosedPos : leftDoorClosedPos + new Vector3(-0.35f, 0f, 0f);
        Vector3 rightTarget = close ? rightDoorClosedPos : rightDoorClosedPos + new Vector3(0.35f, 0f, 0f);

        Vector3 leftStart = leftDoor != null ? leftDoor.localPosition : Vector3.zero;
        Vector3 rightStart = rightDoor != null ? rightDoor.localPosition : Vector3.zero;

        float t = 0f;
        float duration = 0.35f * AnimationDurationScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float smooth = k * k * (3f - 2f * k);

            if (leftDoor != null)
                leftDoor.localPosition = Vector3.Lerp(leftStart, leftTarget, smooth);

            if (rightDoor != null)
                rightDoor.localPosition = Vector3.Lerp(rightStart, rightTarget, smooth);

            yield return null;
        }

        if (leftDoor != null) leftDoor.localPosition = leftTarget;
        if (rightDoor != null) rightDoor.localPosition = rightTarget;
        doorCoroutine = null;
    }

    private void ToggleClamp()
    {
        if (running)
        {
            UpdateStatus("运行中禁止夹紧");
            return;
        }

        workpieceClamped = !workpieceClamped;

        if (clampCoroutine != null)
        {
            StopCoroutine(clampCoroutine);
        }

        clampCoroutine = StartCoroutine(AnimateClamp(workpieceClamped));

        if (clampHandlePivot != null)
        {
            clampHandlePivot.localRotation = workpieceClamped ? Quaternion.Euler(0f, 0f, -55f) : Quaternion.identity;
        }

        UpdateStatus(workpieceClamped ? "工件已夹紧：请关门" : "工件已松开");
        UpdateVisuals();
    }

    private IEnumerator AnimateClamp(bool clamp)
    {
        Vector3 leftTarget = clamp ? leftClampOpenPos + new Vector3(0.13f, 0f, 0f) : leftClampOpenPos;
        Vector3 rightTarget = clamp ? rightClampOpenPos + new Vector3(-0.13f, 0f, 0f) : rightClampOpenPos;

        Vector3 leftStart = leftClampJaw != null ? leftClampJaw.localPosition : Vector3.zero;
        Vector3 rightStart = rightClampJaw != null ? rightClampJaw.localPosition : Vector3.zero;

        float t = 0f;
        float duration = 0.25f * AnimationDurationScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float smooth = k * k * (3f - 2f * k);

            if (leftClampJaw != null)
                leftClampJaw.localPosition = Vector3.Lerp(leftStart, leftTarget, smooth);

            if (rightClampJaw != null)
                rightClampJaw.localPosition = Vector3.Lerp(rightStart, rightTarget, smooth);

            yield return null;
        }

        if (leftClampJaw != null) leftClampJaw.localPosition = leftTarget;
        if (rightClampJaw != null) rightClampJaw.localPosition = rightTarget;
        clampCoroutine = null;
    }

    private void TryCycleStart()
    {
        PushButton(cycleStartButtonTransform, cycleStartButtonReadyPos, ref cycleStartButtonCoroutine);

        if (!powerOn)
        {
            UpdateStatus("请先上电");
            return;
        }

        if (emergencyStopped)
        {
            UpdateStatus("请先复位");
            return;
        }

        if (!doorClosed)
        {
            doorClosed = true;

            if (doorCoroutine != null)
            {
                StopCoroutine(doorCoroutine);
            }

            doorCoroutine = StartCoroutine(AnimateDoor(true));
        }

        if (!workpieceClamped)
        {
            UpdateStatus("请先夹紧工件");
            return;
        }

        running = true;
        UpdateStatus("加工运行中");
        UpdateVisuals();
    }

    private void EmergencyStop()
    {
        PushButton(emergencyStopButtonTransform, emergencyStopButtonReadyPos, ref emergencyStopButtonCoroutine);
        running = false;
        emergencyStopped = true;
        UpdateStatus("急停激活");
        UpdateVisuals();
    }

    private void ResetMachine()
    {
        PushButton(resetButtonTransform, resetButtonReadyPos, ref resetButtonCoroutine);
        emergencyStopped = false;
        running = false;

        if (powerOn)
            UpdateStatus("复位完成：就绪");
        else
            UpdateStatus("复位完成：电源关闭");

        UpdateVisuals();
    }

    private void ToggleMode()
    {
        autoMode = !autoMode;
        ApplyModeKnobRotation();
        UpdateStatus(autoMode ? "模式：自动" : "模式：手动");
    }

    void ApplyModeKnobRotation()
    {
        if (modeKnob != null)
        {
            modeKnob.localRotation = autoMode ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f);
        }
    }

    private void PushButton(Transform button, Vector3 readyPosition, ref Coroutine activeCoroutine)
    {
        if (button == null)
        {
            return;
        }

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(PushButtonRoutine(button, readyPosition));
    }

    private IEnumerator PushButtonRoutine(Transform button, Vector3 readyPosition)
    {
        Vector3 pressedPosition = readyPosition + new Vector3(0f, 0f, -0.022f);

        yield return MoveLocalPosition(button, button.localPosition, pressedPosition, 0.08f * AnimationDurationScale);
        yield return new WaitForSeconds(0.08f * AnimationDurationScale);
        yield return MoveLocalPosition(button, button.localPosition, readyPosition, 0.12f * AnimationDurationScale);
    }

    private IEnumerator MoveLocalPosition(Transform target, Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = t * t * (3f - 2f * t);
            target.localPosition = Vector3.Lerp(start, end, smooth);
            yield return null;
        }

        target.localPosition = end;
    }

    private void UpdateVisuals()
    {
        if (screenRenderer != null)
            screenRenderer.sharedMaterial = powerOn ? ResolveScreenOnMaterial() : matScreenOff;

        if (powerSwitchRenderer != null)
            powerSwitchRenderer.sharedMaterial = powerOn ? matYellow : matGray;

        SetStackLight();

        if (cycleStartRenderer != null)
        {
            bool canStart = powerOn && doorClosed && workpieceClamped && !running && !emergencyStopped;
            cycleStartRenderer.sharedMaterial = canStart ? matGreen : matGray;
        }

        // 急停按钮本就是常红实体，状态反馈交给三色塔灯(SetStackLight)，
        // 这里不再做无意义的同色切换。
    }

    private void SetStackLight()
    {
        if (stackRedRenderer != null)
            stackRedRenderer.sharedMaterial = emergencyStopped ? matRed : matLedOff;

        if (stackYellowRenderer != null)
            stackYellowRenderer.sharedMaterial = powerOn && !running && !emergencyStopped ? matYellow : matLedOff;

        if (stackGreenRenderer != null)
            stackGreenRenderer.sharedMaterial = running ? matGreen : matLedOff;
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = ResolveStatusTextColor(message);
            statusText.transform.localRotation = CNCUiFont.PanelFaceRotation;
            CNCUiFont.Apply(statusText, CNCUiFont.Sizes.Status);
        }

        Debug.Log("[CNC] " + message);
    }

    Color ResolveStatusTextColor(string message)
    {
        if (!string.IsNullOrEmpty(message) && message.StartsWith("引导："))
        {
            return StatusTextGuide;
        }

        if (!string.IsNullOrEmpty(message)
            && (message.Contains("请先") || message.Contains("急停") || message.Contains("禁止")))
        {
            return StatusTextAlert;
        }

        if (powerOn)
        {
            return StatusTextPoweredOn;
        }

        return StatusTextIdle;
    }

    Material ResolveScreenOnMaterial()
    {
        if (_resolvedScreenOnMaterial != null)
        {
            return _resolvedScreenOnMaterial;
        }

        if (matScreenOn == null)
        {
            return null;
        }

        _resolvedScreenOnMaterial = new Material(matScreenOn);
        if (_resolvedScreenOnMaterial.HasProperty("_BaseColor"))
        {
            _resolvedScreenOnMaterial.SetColor("_BaseColor", ScreenOnLightBlue);
        }

        if (_resolvedScreenOnMaterial.HasProperty("_Color"))
        {
            _resolvedScreenOnMaterial.SetColor("_Color", ScreenOnLightBlue);
        }

        if (_resolvedScreenOnMaterial.HasProperty("_EmissionColor"))
        {
            _resolvedScreenOnMaterial.EnableKeyword("_EMISSION");
            _resolvedScreenOnMaterial.SetColor("_EmissionColor", new Color(0.42f, 0.72f, 0.92f) * 0.38f);
        }

        return _resolvedScreenOnMaterial;
    }

    void EnsurePresentation()
    {
        EnsureFrontFillLight();
        EnsureModeSelectorVisuals();
        EnsureWorkChamberVisuals();
        BrightenMachineSurfaces();
        ApplyModeKnobRotation();
    }

    void EnsureFrontFillLight()
    {
        if (!enableFrontFillLight)
        {
            return;
        }

        Transform existing = transform.Find("CNC_Front_Fill_Light");
        if (existing != null)
        {
            _frontFillLight = existing.GetComponent<Light>();
            if (_frontFillLight != null)
            {
                _frontFillLight.intensity = frontFillLightIntensity;
            }

            return;
        }

        GameObject lightObject = new GameObject("CNC_Front_Fill_Light");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = new Vector3(-0.05f, 1.18f, 2.65f);
        lightObject.transform.localRotation = Quaternion.Euler(10f, 180f, 0f);

        _frontFillLight = lightObject.AddComponent<Light>();
        _frontFillLight.type = LightType.Spot;
        _frontFillLight.color = new Color(1f, 0.98f, 0.94f);
        _frontFillLight.intensity = frontFillLightIntensity;
        _frontFillLight.range = 9f;
        _frontFillLight.spotAngle = 72f;
        _frontFillLight.shadows = LightShadows.None;
    }

    void BrightenMachineSurfaces()
    {
        if (machineSurfaceBrightness <= 1.001f)
        {
            return;
        }

        BrightenRenderer(FindChildRecursive(transform, "CNC_Main_Enclosure")?.GetComponent<Renderer>());
        BrightenRenderer(FindChildRecursive(transform, "CNC_Base_Platform")?.GetComponent<Renderer>());
        BrightenRenderer(FindChildRecursive(transform, "Control_Panel_Body")?.GetComponent<Renderer>());
        BrightenRenderer(FindChildRecursive(transform, "Machine_Table")?.GetComponent<Renderer>());
    }

    void BrightenRenderer(Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        Material source = renderer.sharedMaterial;
        if (source == null)
        {
            return;
        }

        Material instance = new Material(source);
        MultiplyMaterialColor(instance, "_BaseColor", machineSurfaceBrightness);
        MultiplyMaterialColor(instance, "_Color", machineSurfaceBrightness);
        renderer.sharedMaterial = instance;
    }

    static void MultiplyMaterialColor(Material material, string propertyName, float multiplier)
    {
        if (material == null || !material.HasProperty(propertyName))
        {
            return;
        }

        Color color = material.GetColor(propertyName);
        color.r = Mathf.Clamp01(color.r * multiplier);
        color.g = Mathf.Clamp01(color.g * multiplier);
        color.b = Mathf.Clamp01(color.b * multiplier);
        material.SetColor(propertyName, color);
    }

    void EnsureModeSelectorVisuals()
    {
        Transform existingPivot = FindChildRecursive(transform, "Mode_Select_Knob_Pivot");
        if (existingPivot != null)
        {
            modeKnob = existingPivot;
            RemoveModeSelectorTextLabels();
            return;
        }

        Transform knobClickable = FindChildRecursive(transform, "Mode_Select_Knob_Clickable");
        if (knobClickable == null)
        {
            return;
        }

        Transform panelParent = knobClickable.parent;
        Vector3 knobPosition = knobClickable.localPosition;
        Quaternion knobRotation = knobClickable.localRotation;

        GameObject pivotObject = new GameObject("Mode_Select_Knob_Pivot");
        pivotObject.transform.SetParent(panelParent, false);
        pivotObject.transform.localPosition = knobPosition;
        pivotObject.transform.localRotation = knobRotation;

        knobClickable.SetParent(pivotObject.transform, false);
        knobClickable.localPosition = Vector3.zero;
        knobClickable.localRotation = Quaternion.identity;
        knobClickable.localScale = new Vector3(0.088f, 0.014f, 0.088f);

        Renderer knobRenderer = knobClickable.GetComponent<Renderer>();
        if (knobRenderer != null)
        {
            knobRenderer.sharedMaterial = ResolveModeKnobBodyMaterial();
        }

        CreateRuntimeCylinder(
            pivotObject.transform,
            "Mode_Select_Knob_Base",
            new Vector3(0f, 0f, -0.004f),
            0.095f,
            0.012f,
            matGray != null ? matGray : ResolveModeKnobBodyMaterial(),
            RuntimeCylinderAxis.Z);

        CreateRuntimeCube(
            pivotObject.transform,
            "Mode_Select_Knob_Pointer",
            new Vector3(0f, 0.042f, 0.012f),
            new Vector3(0.018f, 0.048f, 0.012f),
            matYellow != null ? matYellow : ResolveModeKnobBodyMaterial());

        RemoveModeSelectorTextLabels();
        modeKnob = pivotObject.transform;
        MarkTransformHierarchyDynamic(modeKnob);
    }

    void RemoveModeSelectorTextLabels()
    {
        DestroyChildLabel("Label_Mode_Auto");
        DestroyChildLabel("Label_Mode_Manual");
        DestroyChildLabel("Label_Mode_Value");
    }

    void DestroyChildLabel(string objectName)
    {
        Transform label = FindChildRecursive(transform, objectName);
        if (label != null)
        {
            Destroy(label.gameObject);
        }
    }

    Vector3 ResolveDoorWindowCenter()
    {
        if (leftDoor != null && rightDoor != null)
        {
            Vector3 left = leftDoor.localPosition;
            Vector3 right = rightDoor.localPosition;
            return new Vector3((left.x + right.x) * 0.5f, (left.y + right.y) * 0.5f, 0.78f);
        }

        return new Vector3(-0.28f, 1.052f, 0.78f);
    }

    void RepositionClampAssemblyToDoorCenter()
    {
        Vector3 doorCenter = ResolveDoorWindowCenter();

        SetTransformLocalPosition(workpiece, doorCenter + new Vector3(0f, 0.02f, 0f));

        Transform clampBase = FindChildRecursive(transform, "Clamp_Base");
        if (clampBase != null)
        {
            clampBase.localPosition = doorCenter + new Vector3(0f, -0.08f, 0f);
        }

        SetTransformLocalPosition(leftClampJaw, doorCenter + new Vector3(-0.25f, 0.04f, 0f));
        SetTransformLocalPosition(rightClampJaw, doorCenter + new Vector3(0.25f, 0.04f, 0f));

        Transform leftBevel = FindChildRecursive(transform, "Clamp_Jaw_Left_Bevel");
        if (leftBevel != null)
        {
            leftBevel.localPosition = doorCenter + new Vector3(-0.21f, 0.04f, 0.10f);
        }

        Transform rightBevel = FindChildRecursive(transform, "Clamp_Jaw_Right_Bevel");
        if (rightBevel != null)
        {
            rightBevel.localPosition = doorCenter + new Vector3(0.21f, 0.04f, 0.10f);
        }

        if (clampHandlePivot != null)
        {
            clampHandlePivot.localPosition = doorCenter + new Vector3(0.47f, 0.04f, 0.06f);
        }

        if (leftClampJaw != null)
        {
            leftClampOpenPos = leftClampJaw.localPosition;
        }

        if (rightClampJaw != null)
        {
            rightClampOpenPos = rightClampJaw.localPosition;
        }

        if (workpiece != null)
        {
            _workpieceRestLocalPos = workpiece.localPosition;
            _workpieceRestCaptured = true;
        }

        ApplySurfaceLabel(
            "Clamp_Label",
            "夹具",
            doorCenter + new Vector3(0f, 0.44f, 0.06f),
            CNCUiFont.Sizes.DoorInstruction,
            Color.white);
    }

    static void SetTransformLocalPosition(Transform target, Vector3 localPosition)
    {
        if (target != null)
        {
            target.localPosition = localPosition;
        }
    }

    void EnsureWorkChamberVisuals()
    {
        Material accentMaterial = ResolveClampAccentMaterial();
        Material metalMaterial = ResolveChamberMetalMaterial();

        SetRendererMaterial(leftClampJaw, accentMaterial);
        SetRendererMaterial(rightClampJaw, accentMaterial);

        Transform chamberParent = leftClampJaw != null ? leftClampJaw.parent : transform;
        Vector3 doorCenter = ResolveDoorWindowCenter();
        if (FindChildRecursive(transform, "Clamp_Base") == null)
        {
            CreateRuntimeCube(
                chamberParent,
                "Clamp_Base",
                doorCenter + new Vector3(0f, -0.08f, 0f),
                new Vector3(0.62f, 0.08f, 0.30f),
                metalMaterial);
        }

        if (clampHandlePivot != null && FindChildRecursive(clampHandlePivot, "Clamp_Lever_Bar") == null)
        {
            CreateRuntimeCube(
                clampHandlePivot,
                "Clamp_Lever_Bar",
                new Vector3(0.06f, 0.14f, 0.02f),
                new Vector3(0.14f, 0.025f, 0.025f),
                accentMaterial);
        }

        RepositionClampAssemblyToDoorCenter();
        EnsureChamberLight();
    }

    void EnsureChamberLight()
    {
        Vector3 doorCenter = ResolveDoorWindowCenter();

        Transform existing = transform.Find("CNC_Chamber_Work_Light");
        if (existing != null)
        {
            _chamberLight = existing.GetComponent<Light>();
            existing.localPosition = doorCenter + new Vector3(0f, 0.05f, -0.08f);
            return;
        }

        GameObject lightObject = new GameObject("CNC_Chamber_Work_Light");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = doorCenter + new Vector3(0f, 0.05f, -0.08f);
        lightObject.transform.localRotation = Quaternion.Euler(72f, 180f, 0f);

        _chamberLight = lightObject.AddComponent<Light>();
        _chamberLight.type = LightType.Point;
        _chamberLight.color = new Color(0.92f, 0.96f, 1f);
        _chamberLight.range = 2.4f;
        _chamberLight.intensity = 0f;
        _chamberLight.shadows = LightShadows.None;
        _chamberLight.enabled = false;
    }

    void UpdateChamberDynamics()
    {
        if (_chamberLight != null)
        {
            bool chamberActive = powerOn && (running || workpieceClamped);
            _chamberLight.enabled = chamberActive;

            if (chamberActive)
            {
                float pulse = running ? 1.35f + Mathf.Sin(Time.time * 8f) * 0.15f : 0.82f;
                _chamberLight.intensity = pulse;
            }
        }

        if (workpiece != null)
        {
            if (!_workpieceRestCaptured)
            {
                _workpieceRestLocalPos = workpiece.localPosition;
                _workpieceRestCaptured = true;
            }

            if (running)
            {
                float bob = Mathf.Sin(Time.time * 12f) * 0.0025f;
                workpiece.localPosition = _workpieceRestLocalPos + new Vector3(0f, bob, 0f);
            }
            else
            {
                workpiece.localPosition = _workpieceRestLocalPos;
            }
        }
    }

    Material ResolveModeKnobBodyMaterial()
    {
        if (_modeKnobBodyMaterial != null)
        {
            return _modeKnobBodyMaterial;
        }

        Material source = matGreen != null ? matGreen : matGray;
        _modeKnobBodyMaterial = source != null ? new Material(source) : new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        SetMaterialBaseColor(_modeKnobBodyMaterial, ModeKnobGreen);
        return _modeKnobBodyMaterial;
    }

    Material ResolveClampAccentMaterial()
    {
        if (_clampAccentMaterial != null)
        {
            return _clampAccentMaterial;
        }

        Material source = matYellow != null ? matYellow : matGray;
        _clampAccentMaterial = source != null ? new Material(source) : new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        SetMaterialBaseColor(_clampAccentMaterial, ClampAccentOrange);
        return _clampAccentMaterial;
    }

    Material ResolveChamberMetalMaterial()
    {
        Transform table = FindChildRecursive(transform, "Machine_Table");
        Renderer tableRenderer = table != null ? table.GetComponent<Renderer>() : null;
        if (tableRenderer != null && tableRenderer.sharedMaterial != null)
        {
            return tableRenderer.sharedMaterial;
        }

        return matGray;
    }

    static void SetMaterialBaseColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    static void SetRendererMaterial(Transform target, Material material)
    {
        if (target == null || material == null)
        {
            return;
        }

        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    enum RuntimeCylinderAxis
    {
        X,
        Y,
        Z
    }

    static GameObject CreateRuntimeCube(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = size;
        obj.GetComponent<Renderer>().sharedMaterial = material;

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        return obj;
    }

    static GameObject CreateRuntimeCylinder(
        Transform parent,
        string name,
        Vector3 localPosition,
        float diameter,
        float length,
        Material material,
        RuntimeCylinderAxis axis)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);

        if (axis == RuntimeCylinderAxis.X)
        {
            obj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else if (axis == RuntimeCylinderAxis.Z)
        {
            obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        obj.GetComponent<Renderer>().sharedMaterial = material;

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        return obj;
    }

    TextMesh EnsureRuntimeTextLabel(
        Transform parent,
        string objectName,
        string text,
        Vector3 position,
        float characterSize,
        Color color,
        bool bold = false)
    {
        Transform existing = FindChildRecursive(parent, objectName);
        if (existing != null)
        {
            TextMesh existingText = existing.GetComponent<TextMesh>();
            if (existingText != null)
            {
                existing.localPosition = position;
                existing.localRotation = CNCUiFont.PanelFaceRotation;
                existingText.text = text;
                existingText.characterSize = characterSize;
                existingText.color = color;
                existingText.anchor = TextAnchor.MiddleCenter;
                existingText.alignment = TextAlignment.Center;
                CNCUiFont.Apply(existingText, characterSize, bold);
                return existingText;
            }
        }

        GameObject labelObject = new GameObject(objectName);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = position;
        labelObject.transform.localRotation = CNCUiFont.PanelFaceRotation;

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = characterSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.lineSpacing = 0.82f;
        CNCUiFont.Apply(textMesh, characterSize, bold);
        return textMesh;
    }

    IEnumerator WaitGuidedSeconds(float seconds)
    {
        if (seconds <= 0f)
        {
            yield break;
        }

        yield return new WaitForSeconds(seconds);
    }

    IEnumerator WaitForDoorAnimation()
    {
        while (doorCoroutine != null)
        {
            yield return null;
        }
    }

    IEnumerator WaitForClampAnimation()
    {
        while (clampCoroutine != null)
        {
            yield return null;
        }
    }

    public IEnumerator PlayGuidedSequence()
    {
        suppressGuidedInput = true;
        ApplyInitialState();

        UpdateStatus("引导：1 上电");
        HandleInteraction(CNCInteractionType.TogglePower);
        yield return WaitGuidedSeconds(guidedStepPause);

        UpdateStatus("引导：2 确认自动模式");
        if (!autoMode)
        {
            HandleInteraction(CNCInteractionType.ToggleMode);
        }
        else
        {
            ApplyModeKnobRotation();
        }
        yield return WaitGuidedSeconds(guidedActionPause);

        if (doorClosed)
        {
            UpdateStatus("引导：3 打开安全门");
            HandleInteraction(CNCInteractionType.ToggleDoor);
            yield return WaitForDoorAnimation();
            yield return WaitGuidedSeconds(guidedDoorPause);
        }

        UpdateStatus("引导：4 夹紧工件");
        HandleInteraction(CNCInteractionType.ToggleClamp);
        yield return WaitForClampAnimation();
        yield return WaitGuidedSeconds(guidedClampPause);

        if (!doorClosed)
        {
            UpdateStatus("引导：5 关闭安全门");
            HandleInteraction(CNCInteractionType.ToggleDoor);
            yield return WaitForDoorAnimation();
            yield return WaitGuidedSeconds(guidedDoorPause);
        }

        UpdateStatus("引导：6 启动加工");
        HandleInteraction(CNCInteractionType.CycleStart);
        yield return WaitGuidedSeconds(guidedRunPause);

        UpdateStatus("引导：7 急停");
        HandleInteraction(CNCInteractionType.EmergencyStop);
        yield return WaitGuidedSeconds(guidedActionPause);

        UpdateStatus("引导：8 复位");
        HandleInteraction(CNCInteractionType.Reset);
        yield return WaitGuidedSeconds(guidedActionPause);

        UpdateStatus("引导演示完成");
        suppressGuidedInput = false;
    }
}
