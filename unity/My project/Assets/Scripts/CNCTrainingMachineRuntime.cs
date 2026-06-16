using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum CNCInteractionType
{
    TogglePower,
    ToggleDoor,
    ToggleClamp,
    CycleStart,
    EmergencyStop,
    Reset,
    ToggleMode
}

public class CNCInteractablePart : MonoBehaviour
{
    public CNCTrainingMachineRuntime station;
    public CNCInteractionType interactionType;
}

public class CNCTrainingMachineRuntime : MonoBehaviour
{
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

    private void Awake()
    {
        RefreshSurfaceLabels();
        MarkRuntimePartsDynamic();
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
        if (leftClampJaw != null) leftClampOpenPos = leftClampJaw.localPosition;
        if (rightClampJaw != null) rightClampOpenPos = rightClampJaw.localPosition;
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
        float duration = 0.35f;

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
        float duration = 0.25f;

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

        if (modeKnob != null)
        {
            modeKnob.localRotation = autoMode ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f);
        }

        UpdateStatus(autoMode ? "模式：自动" : "模式：手动");
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

        yield return MoveLocalPosition(button, button.localPosition, pressedPosition, 0.08f);
        yield return new WaitForSeconds(0.08f);
        yield return MoveLocalPosition(button, button.localPosition, readyPosition, 0.12f);
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
            screenRenderer.sharedMaterial = powerOn ? matScreenOn : matScreenOff;

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
            statusText.transform.localRotation = CNCUiFont.PanelFaceRotation;
            CNCUiFont.Apply(statusText, CNCUiFont.Sizes.Status);
        }

        Debug.Log("[CNC] " + message);
    }

    public IEnumerator PlayGuidedSequence()
    {
        suppressGuidedInput = true;
        ApplyInitialState();

        UpdateStatus("引导：1 上电");
        HandleInteraction(CNCInteractionType.TogglePower);
        yield return new WaitForSeconds(0.8f);

        if (doorClosed)
        {
            UpdateStatus("引导：2 打开安全门");
            HandleInteraction(CNCInteractionType.ToggleDoor);
            yield return new WaitForSeconds(0.5f);
        }

        UpdateStatus("引导：3 夹紧工件");
        HandleInteraction(CNCInteractionType.ToggleClamp);
        yield return new WaitForSeconds(0.5f);

        UpdateStatus("引导：4 启动加工");
        HandleInteraction(CNCInteractionType.CycleStart);
        yield return new WaitForSeconds(2.5f);

        UpdateStatus("引导：5 急停");
        HandleInteraction(CNCInteractionType.EmergencyStop);
        yield return new WaitForSeconds(0.8f);

        UpdateStatus("引导：6 复位");
        HandleInteraction(CNCInteractionType.Reset);
        yield return new WaitForSeconds(0.8f);

        UpdateStatus("引导演示完成");
        suppressGuidedInput = false;
    }
}
