using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BreakerShutdownStationRuntime : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode autoStartKey = KeyCode.J;
    public float autoSequenceDelay = 0.65f;

    [Header("Scene Parts")]
    public BreakerSwitchRuntime[] breakers;
    public Renderer[] circuitLightRenderers;
    public Renderer startButtonRenderer;
    public Transform startButtonTransform;
    public Vector3 startButtonReadyLocalPosition;
    public Vector3 startButtonPressedLocalPosition;
    public Renderer confirmButtonRenderer;

    public Transform voltageNeedlePivot;
    public Renderer gaugeZoneRenderer;

    public Transform beaconPivot;
    public Renderer beaconLensRenderer;
    public Light beaconLight;
    public Light topWorkLight;

    public TextMesh statusText;

    [Header("Materials")]
    public Material lightOnMaterial;
    public Material lightOffMaterial;
    public Material confirmDisabledMaterial;
    public Material confirmEnabledMaterial;
    public Material gaugeGreenMaterial;
    public Material gaugeOffMaterial;
    public Material beaconStandbyMaterial;
    public Material beaconActiveMaterial;
    public Material beaconCompleteMaterial;
    public Material startButtonReadyMaterial;
    public Material startButtonPressedMaterial;

    [Header("Voltage Gauge")]
    public float fullVoltageAngle = 330f;
    public float zeroVoltageAngle = 210f;

    private readonly int[] correctSequence = { 2, 4, 1, 3 };

    private int sequenceProgress = 0;
    private int errorCount = 0;

    private bool stationStarted = false;
    private bool completed = false;
    private bool autoSequenceRunning = false;

    private float startTime;
    private Coroutine voltageCoroutine;
    private Coroutine shutdownCoroutine;
    private Coroutine beaconFlashCoroutine;
    private Coroutine workLightFlickerCoroutine;
    private Coroutine startButtonCoroutine;

    private enum StationPhase
    {
        Standby,
        ShuttingDown,
        Complete
    }

    private enum BeaconState
    {
        Standby,
        Active,
        Complete
    }

    private StationPhase phase = StationPhase.Standby;

    private void Awake()
    {
        EnsureSceneBindings();
        RebindBreakerReferences();
        EnsureStartButtonInteractable();
        RefreshSurfaceLabels();
        MarkDynamicParts();
    }

    private void EnsureSceneBindings()
    {
        if (statusText == null)
        {
            Transform statusTransform = FindChildRecursive(transform, "Status_Text");
            if (statusTransform != null)
            {
                statusText = statusTransform.GetComponent<TextMesh>();
            }
        }

        if (startButtonTransform == null)
        {
            startButtonTransform = FindChildRecursive(transform, "Auto_Start_Button_Cap_Clickable");
        }

        if (startButtonRenderer == null && startButtonTransform != null)
        {
            startButtonRenderer = startButtonTransform.GetComponent<Renderer>();
        }

        if (beaconPivot == null)
        {
            beaconPivot = FindChildRecursive(transform, "Red_Rotating_Beacon_Pivot");
        }

        if (voltageNeedlePivot == null)
        {
            voltageNeedlePivot = FindChildRecursive(transform, "Voltage_Gauge_Needle_Pivot");
        }

        if (circuitLightRenderers == null || circuitLightRenderers.Length == 0)
        {
            circuitLightRenderers = new Renderer[4];
            for (int i = 1; i <= 4; i++)
            {
                Transform lightTransform = FindChildRecursive(transform, "Circuit_LED_" + i);
                if (lightTransform != null)
                {
                    circuitLightRenderers[i - 1] = lightTransform.GetComponent<Renderer>();
                }
            }
        }
    }

    private void RefreshSurfaceLabels()
    {
        ApplySurfaceLabel("Emergency_Zone_Label", "紧急停机作业区", BreakerShutdownUiFont.Positions.EmergencyZone, BreakerShutdownUiFont.Sizes.EmergencyZone, new Color(1f, 0.28f, 0.22f));
        ApplySurfaceLabel("Procedure_Sign_Text", "顺序 ②→④→①→③", BreakerShutdownUiFont.Positions.Procedure, BreakerShutdownUiFont.Sizes.Procedure, new Color(1f, 0.92f, 0.55f));
        ApplySurfaceLabel("Auto_Start_Label", "启动(J)", BreakerShutdownUiFont.Positions.AutoStart, BreakerShutdownUiFont.Sizes.ButtonHint, new Color(0.92f, 0.92f, 0.88f));
        ApplySurfaceLabel("Confirm_Label", "确认", BreakerShutdownUiFont.Positions.Confirm, BreakerShutdownUiFont.Sizes.ButtonHint, new Color(0.92f, 0.92f, 0.88f));
        ApplySurfaceLabel("Voltage_Gauge_Label", "电压", BreakerShutdownUiFont.Positions.GaugeTitle, BreakerShutdownUiFont.Sizes.GaugeTitle, Color.white);
        ApplySurfaceLabel("Voltage_Gauge_Rated", "正常", BreakerShutdownUiFont.Positions.GaugeRated, BreakerShutdownUiFont.Sizes.GaugeHint, new Color(0f, 0.55f, 0.1f));
        ApplySurfaceLabel("Voltage_Gauge_Zero", "0", BreakerShutdownUiFont.Positions.GaugeZero, BreakerShutdownUiFont.Sizes.GaugeHint, Color.black);

        for (int breakerNumber = 1; breakerNumber <= 4; breakerNumber++)
        {
            Transform numberLabel = FindChildRecursive(transform, "Breaker_Number_" + breakerNumber);
            if (numberLabel == null)
            {
                continue;
            }

            TextMesh numberMesh = numberLabel.GetComponent<TextMesh>();
            if (numberMesh == null)
            {
                continue;
            }

            Vector3 position = numberLabel.localPosition;
            position.z = BreakerShutdownUiFont.Positions.PanelTextZ;
            numberLabel.localPosition = position;
            numberLabel.localRotation = BreakerShutdownUiFont.PanelFaceRotation;
            numberMesh.characterSize = BreakerShutdownUiFont.Sizes.BreakerNumber;
            BreakerShutdownUiFont.Apply(numberMesh, BreakerShutdownUiFont.Sizes.BreakerNumber);
        }

        if (statusText != null)
        {
            statusText.transform.localPosition = BreakerShutdownUiFont.Positions.Status;
            statusText.transform.localRotation = BreakerShutdownUiFont.PanelFaceRotation;
            statusText.characterSize = BreakerShutdownUiFont.Sizes.Status;
            statusText.lineSpacing = 0.82f;
            BreakerShutdownUiFont.Apply(statusText, BreakerShutdownUiFont.Sizes.Status);
        }
    }

    private void ApplySurfaceLabel(string objectName, string text, Vector3 position, float characterSize, Color color)
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
        labelTransform.localRotation = BreakerShutdownUiFont.PanelFaceRotation;

        textMesh.text = text;
        textMesh.characterSize = characterSize;
        textMesh.color = color;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.lineSpacing = 0.82f;

        BreakerShutdownUiFont.Apply(textMesh, characterSize);
    }

    private void EnsureStartButtonInteractable()
    {
        if (startButtonTransform == null)
        {
            Transform found = FindChildRecursive(transform, "Auto_Start_Button_Cap_Clickable");
            if (found != null)
            {
                startButtonTransform = found;
            }
        }

        if (startButtonTransform == null)
        {
            return;
        }

        BreakerStartButtonInteractable interactable = startButtonTransform.GetComponent<BreakerStartButtonInteractable>();
        if (interactable == null)
        {
            interactable = startButtonTransform.gameObject.AddComponent<BreakerStartButtonInteractable>();
        }

        interactable.station = this;
    }

    private void Start()
    {
        InitializeStandby();
    }

    private void Update()
    {
        if (phase != StationPhase.ShuttingDown && WasAutoStartPressed())
        {
            PressStartButton();
        }

        if (stationStarted && !completed && beaconPivot != null)
        {
            beaconPivot.Rotate(Vector3.up, 70f * Time.deltaTime, Space.Self);
        }
    }

    public void PressStartButton()
    {
        if (phase == StationPhase.ShuttingDown)
        {
            return;
        }

        if (phase == StationPhase.Complete)
        {
            InitializeStandby();
        }

        if (shutdownCoroutine != null)
        {
            StopCoroutine(shutdownCoroutine);
        }

        shutdownCoroutine = StartCoroutine(ShutdownRoutine());
    }

    public void TriggerAutomaticShutdown()
    {
        PressStartButton();
    }

    private void InitializeStandby()
    {
        phase = StationPhase.Standby;
        stationStarted = false;
        completed = false;
        autoSequenceRunning = false;
        sequenceProgress = 0;
        errorCount = 0;

        StopLightEffects();
        ResetBreakersToOn();
        SetStartButtonPressed(false);
        SetConfirmEnabled(false);
        SetBeaconState(BeaconState.Standby);

        if (topWorkLight != null)
        {
            topWorkLight.enabled = true;
            topWorkLight.intensity = 1.7f;
        }

        UpdateStatusText("待机 | 按启动或J");
        Debug.Log("[Breaker] 待机：请按启动按钮或 J 键开始断电");
    }

    private bool WasAutoStartPressed()
    {
        if (Input.GetKeyDown(autoStartKey))
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return false;
        }

        Key? systemKey = AutoStartKeyToInputSystemKey(autoStartKey);
        if (systemKey.HasValue && Keyboard.current[systemKey.Value].wasPressedThisFrame)
        {
            return true;
        }
#endif

        return false;
    }

#if ENABLE_INPUT_SYSTEM
    private static Key? AutoStartKeyToInputSystemKey(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.J: return Key.J;
            case KeyCode.K: return Key.K;
            case KeyCode.Space: return Key.Space;
            default: return null;
        }
    }
#endif

    private void MarkDynamicParts()
    {
        if (voltageNeedlePivot != null)
        {
            MarkTransformHierarchyDynamic(voltageNeedlePivot);
        }

        if (beaconPivot != null)
        {
            MarkTransformHierarchyDynamic(beaconPivot);
        }

        if (startButtonTransform != null)
        {
            startButtonTransform.gameObject.isStatic = false;
        }

        if (breakers == null)
        {
            return;
        }

        foreach (BreakerSwitchRuntime breaker in breakers)
        {
            if (breaker == null)
            {
                continue;
            }

            breaker.gameObject.isStatic = false;

            if (breaker.leverPivot != null)
            {
                MarkTransformHierarchyDynamic(breaker.leverPivot);
            }
        }
    }

    private void MarkTransformHierarchyDynamic(Transform root)
    {
        root.gameObject.isStatic = false;

        foreach (Transform child in root)
        {
            MarkTransformHierarchyDynamic(child);
        }
    }

    private void StartStation()
    {
        if (stationStarted)
        {
            return;
        }

        stationStarted = true;
        startTime = Time.time;

        if (topWorkLight != null)
            topWorkLight.enabled = true;

        if (beaconLight != null)
            beaconLight.enabled = true;

        SetBeaconState(BeaconState.Active);
        UpdateStatusText("紧急停机中");
    }

    private IEnumerator ShutdownRoutine()
    {
        phase = StationPhase.ShuttingDown;
        autoSequenceRunning = true;
        completed = false;
        sequenceProgress = 0;
        errorCount = 0;

        RebindBreakerReferences();
        MarkDynamicParts();

        SetStartButtonPressed(true);
        UpdateStatusText("启动中...");
        Debug.Log("[Breaker] 已按下启动，开始断电流程。");

        yield return new WaitForSeconds(0.2f);

        StartStation();
        StartLightEffects();

        yield return new WaitForSeconds(0.35f);

        UpdateStatusText("拉闸 2→4→1→3");

        for (int i = 0; i < correctSequence.Length; i++)
        {
            yield return PullBreakerInAutoSequence(correctSequence[i]);

            if (i < correctSequence.Length - 1)
            {
                yield return new WaitForSeconds(autoSequenceDelay);
            }
        }

        OnAllBreakersOff();
        StopLightEffects();
        SetStartButtonPressed(false);

        phase = StationPhase.Complete;
        autoSequenceRunning = false;
        shutdownCoroutine = null;
    }

    private void StartLightEffects()
    {
        StopLightEffects();
        beaconFlashCoroutine = StartCoroutine(BeaconFlashRoutine());
        workLightFlickerCoroutine = StartCoroutine(WorkLightFlickerRoutine());
    }

    private void StopLightEffects()
    {
        if (beaconFlashCoroutine != null)
        {
            StopCoroutine(beaconFlashCoroutine);
            beaconFlashCoroutine = null;
        }

        if (workLightFlickerCoroutine != null)
        {
            StopCoroutine(workLightFlickerCoroutine);
            workLightFlickerCoroutine = null;
        }
    }

    private IEnumerator BeaconFlashRoutine()
    {
        const float brightIntensity = 2.4f;
        const float dimIntensity = 0.6f;
        const float flashInterval = 0.28f;
        bool brightPhase = true;

        while (stationStarted && !completed)
        {
            if (beaconLight != null)
            {
                beaconLight.intensity = brightPhase ? brightIntensity : dimIntensity;
            }

            brightPhase = !brightPhase;
            yield return new WaitForSeconds(flashInterval);
        }
    }

    private IEnumerator WorkLightFlickerRoutine()
    {
        const float baseIntensity = 1.7f;
        const float flickerIntensity = 2.3f;
        const float flickerInterval = 0.18f;
        bool brightPhase = true;

        while (stationStarted && !completed && topWorkLight != null)
        {
            topWorkLight.intensity = brightPhase ? flickerIntensity : baseIntensity;
            brightPhase = !brightPhase;
            yield return new WaitForSeconds(flickerInterval);
        }

        if (topWorkLight != null)
        {
            topWorkLight.intensity = baseIntensity;
        }
    }

    private IEnumerator PullBreakerInAutoSequence(int breakerNumber)
    {
        BreakerSwitchRuntime breaker = FindBreaker(breakerNumber);

        if (breaker == null)
        {
            Debug.LogWarning("[Breaker] Breaker " + breakerNumber + " not found.");
            yield break;
        }

        Debug.Log("[Breaker] Pulling breaker " + breakerNumber + ".");
        yield return breaker.SetOffRoutine();

        int index = breakerNumber - 1;

        if (index >= 0 && index < circuitLightRenderers.Length && circuitLightRenderers[index] != null)
        {
            circuitLightRenderers[index].sharedMaterial = lightOffMaterial;
            Light pointLight = circuitLightRenderers[index].GetComponent<Light>();
            if (pointLight != null)
            {
                pointLight.enabled = false;
            }
        }

        sequenceProgress++;
        UpdateVoltageGauge();
        UpdateStatusText("电闸" + breakerNumber + "已拉下 " + sequenceProgress + "/4");
        Debug.Log("[Breaker] 电闸 " + breakerNumber + " 已拉下，进度 " + sequenceProgress + "/4。");
    }

    public void TryToggleBreaker(int breakerNumber)
    {
        if (completed) return;

        if (!stationStarted)
        {
            StartStation();
        }

        BreakerSwitchRuntime breaker = FindBreaker(breakerNumber);

        if (breaker == null || breaker.isOff)
            return;

        int expected = correctSequence[sequenceProgress];

        if (breakerNumber != expected)
        {
            errorCount++;

            UpdateStatusText("顺序错误 应拉" + expected + "号");
            Debug.LogWarning("[Breaker] 顺序错误，下一步应拉 " + expected + " 号。错误次数：" + errorCount);

            breaker.BounceBack();
            return;
        }

        breaker.SetOff();

        int index = breakerNumber - 1;

        if (index >= 0 && index < circuitLightRenderers.Length && circuitLightRenderers[index] != null)
        {
            circuitLightRenderers[index].sharedMaterial = lightOffMaterial;
            Light pointLight = circuitLightRenderers[index].GetComponent<Light>();
            if (pointLight != null) pointLight.enabled = false;
        }

        sequenceProgress++;

        UpdateVoltageGauge();

        Debug.Log("断路器 " + breakerNumber + " 已断开。进度：" + sequenceProgress + "/4");

        if (sequenceProgress >= 4)
        {
            OnAllBreakersOff();
        }
        else
        {
            UpdateStatusText("正确 下一步" + correctSequence[sequenceProgress] + "号");
        }
    }

    private BreakerSwitchRuntime FindBreaker(int breakerNumber)
    {
        if (breakers != null)
        {
            foreach (BreakerSwitchRuntime breaker in breakers)
            {
                if (breaker != null && breaker.breakerNumber == breakerNumber)
                    return breaker;
            }
        }

        Transform pivot = FindBreakerPivotByName(breakerNumber);
        if (pivot == null)
        {
            return null;
        }

        BreakerSwitchRuntime fallbackBreaker = pivot.GetComponent<BreakerSwitchRuntime>();
        if (fallbackBreaker == null)
        {
            fallbackBreaker = pivot.gameObject.AddComponent<BreakerSwitchRuntime>();
        }

        ConfigureBreakerRuntime(fallbackBreaker, breakerNumber, pivot);
        return fallbackBreaker;
    }

    private void RebindBreakerReferences()
    {
        BreakerSwitchRuntime[] reboundBreakers = new BreakerSwitchRuntime[4];

        for (int breakerNumber = 1; breakerNumber <= 4; breakerNumber++)
        {
            Transform pivot = FindBreakerPivotByName(breakerNumber);
            if (pivot == null)
            {
                Debug.LogWarning("自动停机流程绑定失败：找不到 Breaker_" + breakerNumber + "_Lever_Pivot。");
                continue;
            }

            BreakerSwitchRuntime breaker = pivot.GetComponent<BreakerSwitchRuntime>();
            if (breaker == null)
            {
                breaker = pivot.gameObject.AddComponent<BreakerSwitchRuntime>();
            }

            ConfigureBreakerRuntime(breaker, breakerNumber, pivot);
            reboundBreakers[breakerNumber - 1] = breaker;
        }

        breakers = reboundBreakers;
        Debug.Log("J 键自动流程：已绑定 " + CountBoundBreakers() + " 个电闸。");
    }

    private int CountBoundBreakers()
    {
        int count = 0;

        if (breakers == null)
        {
            return count;
        }

        foreach (BreakerSwitchRuntime breaker in breakers)
        {
            if (breaker != null)
            {
                count++;
            }
        }

        return count;
    }

    private Transform FindBreakerPivotByName(int breakerNumber)
    {
        string targetName = "Breaker_" + breakerNumber + "_Lever_Pivot";
        Transform found = FindChildRecursive(transform, targetName);

        if (found != null)
        {
            return found;
        }

        GameObject globalFound = GameObject.Find(targetName);
        return globalFound != null ? globalFound.transform : null;
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

    private void ConfigureBreakerRuntime(BreakerSwitchRuntime breaker, int breakerNumber, Transform pivot)
    {
        breaker.station = this;
        breaker.breakerNumber = breakerNumber;
        breaker.leverPivot = pivot;
        breaker.onRotation = Quaternion.Euler(0f, 0f, 0f);
        breaker.offRotation = Quaternion.Euler(68f, 0f, 0f);
        breaker.pullDuration = 0.75f;
        breaker.gameObject.isStatic = false;

        if (pivot != null)
        {
            MarkTransformHierarchyDynamic(breaker.leverPivot);
        }
    }

    private void ResetBreakersToOn()
    {
        if (breakers == null)
        {
            return;
        }

        foreach (BreakerSwitchRuntime breaker in breakers)
        {
            if (breaker != null)
            {
                breaker.ResetToOn();
            }
        }

        if (circuitLightRenderers != null)
        {
            for (int i = 0; i < circuitLightRenderers.Length; i++)
            {
                if (circuitLightRenderers[i] == null)
                {
                    continue;
                }

                circuitLightRenderers[i].sharedMaterial = lightOnMaterial;
                Light pointLight = circuitLightRenderers[i].GetComponent<Light>();
                if (pointLight != null) pointLight.enabled = true;
            }
        }

        if (voltageNeedlePivot != null)
        {
            voltageNeedlePivot.localRotation = Quaternion.Euler(0f, 0f, fullVoltageAngle);
        }

        if (gaugeZoneRenderer != null && gaugeGreenMaterial != null)
        {
            gaugeZoneRenderer.sharedMaterial = gaugeGreenMaterial;
        }
    }

    private void UpdateVoltageGauge()
    {
        if (voltageNeedlePivot == null) return;

        int remainingOn = 4 - sequenceProgress;
        float ratio = Mathf.Clamp01(remainingOn / 4f);
        float targetAngle = Mathf.Lerp(zeroVoltageAngle, fullVoltageAngle, ratio);

        if (voltageCoroutine != null)
        {
            StopCoroutine(voltageCoroutine);
        }

        voltageCoroutine = StartCoroutine(RotateVoltageNeedle(targetAngle));
    }

    private IEnumerator RotateVoltageNeedle(float targetAngle)
    {
        Quaternion startRot = voltageNeedlePivot.localRotation;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);

        float t = 0f;
        float duration = 0.45f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float smooth = k * k * (3f - 2f * k);

            voltageNeedlePivot.localRotation = Quaternion.Slerp(startRot, targetRot, smooth);
            yield return null;
        }

        voltageNeedlePivot.localRotation = targetRot;
    }

    private void OnAllBreakersOff()
    {
        completed = true;
        autoSequenceRunning = false;
        SetBeaconState(BeaconState.Complete);

        if (topWorkLight != null)
        {
            topWorkLight.intensity = 1.2f;
        }

        if (gaugeZoneRenderer != null && gaugeOffMaterial != null)
        {
            gaugeZoneRenderer.sharedMaterial = gaugeOffMaterial;
        }

        float elapsed = Time.time - startTime;
        UpdateStatusText("断电完成 | 按J重置");
        Debug.Log("[Breaker] 断电完成，用时 " + elapsed.ToString("F1") + " 秒。顺序：2 → 4 → 1 → 3。");
    }

    private void SetConfirmEnabled(bool enabled)
    {
        if (confirmButtonRenderer != null)
        {
            confirmButtonRenderer.sharedMaterial = enabled ? confirmEnabledMaterial : confirmDisabledMaterial;
        }
    }

    private void SetStartButtonPressed(bool pressed)
    {
        if (startButtonTransform != null)
        {
            Vector3 target = pressed ? startButtonPressedLocalPosition : startButtonReadyLocalPosition;

            if (startButtonCoroutine != null)
            {
                StopCoroutine(startButtonCoroutine);
            }

            startButtonCoroutine = StartCoroutine(AnimateStartButton(target));
        }

        if (startButtonRenderer != null)
        {
            startButtonRenderer.sharedMaterial = pressed ? startButtonPressedMaterial : startButtonReadyMaterial;
        }
    }

    private IEnumerator AnimateStartButton(Vector3 target)
    {
        Vector3 start = startButtonTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.18f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = t * t * (3f - 2f * t);
            startButtonTransform.localPosition = Vector3.Lerp(start, target, smooth);
            yield return null;
        }

        startButtonTransform.localPosition = target;
        startButtonCoroutine = null;
    }

    private void SetBeaconState(BeaconState state)
    {
        Material targetMaterial = beaconStandbyMaterial;
        Color targetColor = new Color(1f, 0.72f, 0.02f);
        float targetIntensity = 0.75f;

        if (state == BeaconState.Active)
        {
            targetMaterial = beaconActiveMaterial;
            targetColor = Color.red;
            targetIntensity = 2.2f;
        }
        else if (state == BeaconState.Complete)
        {
            targetMaterial = beaconCompleteMaterial;
            targetColor = new Color(0.05f, 1f, 0.22f);
            targetIntensity = 1.6f;
        }

        if (beaconLensRenderer != null && targetMaterial != null)
        {
            beaconLensRenderer.sharedMaterial = targetMaterial;
        }

        if (beaconLight != null)
        {
            beaconLight.enabled = true;
            beaconLight.color = targetColor;
            beaconLight.intensity = targetIntensity;
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.transform.localRotation = BreakerShutdownUiFont.PanelFaceRotation;
            BreakerShutdownUiFont.Apply(statusText, BreakerShutdownUiFont.Sizes.Status);
        }
    }
}

public class BreakerStartButtonInteractable : MonoBehaviour
{
    public BreakerShutdownStationRuntime station;

    private void OnMouseDown()
    {
        if (station != null)
        {
            station.PressStartButton();
        }
    }
}

public class BreakerSwitchRuntime : MonoBehaviour
{
    public BreakerShutdownStationRuntime station;
    public int breakerNumber;

    public Transform leverPivot;

    public Quaternion onRotation;
    public Quaternion offRotation;
    public float pullDuration = 0.42f;

    public bool isOff = false;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (leverPivot == null)
        {
            leverPivot = transform;
        }

        gameObject.isStatic = false;
        leverPivot.gameObject.isStatic = false;
    }

    public void SetOff()
    {
        if (isOff) return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(SetOffRoutine());
    }

    public IEnumerator SetOffRoutine()
    {
        if (isOff) yield break;

        isOff = true;
        yield return AnimateTo(offRotation, pullDuration);
        animationCoroutine = null;
    }

    public void ResetToOn()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        isOff = false;

        if (leverPivot == null)
        {
            leverPivot = transform;
        }

        leverPivot.localRotation = onRotation;
    }

    public void BounceBack()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        Quaternion start = leverPivot.localRotation;
        Quaternion wrongPull = Quaternion.Slerp(onRotation, offRotation, 0.45f);

        yield return AnimateToInternal(start, wrongPull, 0.08f);
        yield return AnimateToInternal(wrongPull, onRotation, 0.14f);

        leverPivot.localRotation = onRotation;
    }

    private IEnumerator AnimateTo(Quaternion target, float duration)
    {
        Quaternion start = leverPivot.localRotation;
        yield return AnimateToInternal(start, target, duration);
        leverPivot.localRotation = target;
    }

    private IEnumerator AnimateToInternal(Quaternion start, Quaternion target, float duration)
    {
        if (leverPivot == null)
        {
            leverPivot = transform;
        }

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float smooth = k * k * (3f - 2f * k);

            leverPivot.localRotation = Quaternion.Slerp(start, target, smooth);
            yield return null;
        }

        leverPivot.localRotation = target;
    }
}
