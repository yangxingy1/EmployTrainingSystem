using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

internal static class BreakerShutdownUiFont
{
    private static Font _cachedFont;

    // 面板朝 +Z 建造，玩家从作业区正面（+Z 侧）观察，文字需绕 Y 轴翻转 180° 避免镜像。
    public static readonly Quaternion PanelFaceRotation = Quaternion.Euler(0f, 180f, 0f);

    public static class Sizes
    {
        public const float EmergencyZone = 0.036f;
        public const float Procedure = 0.020f;
        public const float BreakerNumber = 0.040f;
        public const float ButtonHint = 0.018f;
        public const float Status = 0.017f;
        public const float GaugeTitle = 0.020f;
        public const float GaugeHint = 0.016f;
    }

    public static class Positions
    {
        public const float PanelTextZ = -9.496f;
        public static readonly Vector3 EmergencyZone = new Vector3(0f, 0.04f, -7.88f);
        public static readonly Vector3 Procedure = new Vector3(0f, 1.89f, PanelTextZ);
        public static readonly Vector3 AutoStart = new Vector3(-0.24f, 0.82f, PanelTextZ);
        public static readonly Vector3 Confirm = new Vector3(0f, 0.89f, PanelTextZ);
        public static readonly Vector3 Status = new Vector3(0f, 0.555f, PanelTextZ);
        public static readonly Vector3 GaugeTitle = new Vector3(0f, 1.565f, PanelTextZ);
        public static readonly Vector3 GaugeRated = new Vector3(0.055f, 1.64f, PanelTextZ);
        public static readonly Vector3 GaugeZero = new Vector3(-0.055f, 1.64f, PanelTextZ);
    }

    public static Font Resolve()
    {
        if (_cachedFont != null)
        {
            return _cachedFont;
        }

        string[] preferredFonts =
        {
            "YouYuan",
            "幼圆",
            "STYuanti",
            "华文圆体",
            "FZCuYuan",
            "Comic Sans MS",
            "Microsoft YaHei UI",
            "Microsoft YaHei",
            "SimHei"
        };

        _cachedFont = Font.CreateDynamicFontFromOSFont(preferredFonts, 48);

        if (_cachedFont == null)
        {
            try
            {
                _cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                // Built-in font fallback when OS fonts are unavailable.
            }
        }

        return _cachedFont;
    }

    public static void Apply(TextMesh textMesh, float characterSize)
    {
        if (textMesh == null)
        {
            return;
        }

        Font font = Resolve();
        if (font == null)
        {
            return;
        }

        textMesh.font = font;
        textMesh.fontSize = Mathf.Clamp(Mathf.RoundToInt(characterSize * 1200f), 32, 72);
        textMesh.fontStyle = FontStyle.Bold;

        Renderer renderer = textMesh.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = font.material;
        }
    }
}

public class BreakerShutdownStationBuilder : MonoBehaviour
{
    public const string StaticStationName = "Breaker Shutdown Station Static";
    public const string GeneratedModelName = "__Generated_BreakerShutdown_Station";

    [Header("Placement")]
    public Vector3 stationWorldPosition = Vector3.zero;

    [Header("Build Options")]
    public bool buildOnStart = false;
    public bool addColliders = true;

    [Header("Runtime Interaction")]
    public KeyCode autoStartKey = KeyCode.J;

    private const string RootName = GeneratedModelName;

    private void Start()
    {
        if (buildOnStart)
        {
            BuildStation();
        }
    }

    [ContextMenu("Build / Rebuild Breaker Shutdown Station")]
    public void BuildStation()
    {
        ClearOldModel();

        transform.position = stationWorldPosition;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        Material matCabinet = CreateLitMaterial("MAT_Cabinet_Mechanical_Gray", new Color(0.42f, 0.45f, 0.46f), 0.25f, 0.35f);
        Material matPanel = CreateLitMaterial("MAT_Recessed_Dark_Panel", new Color(0.08f, 0.09f, 0.10f), 0.25f, 0.45f);
        Material matFrame = CreateLitMaterial("MAT_Cabinet_Frame_Dark", new Color(0.18f, 0.19f, 0.20f), 0.35f, 0.35f);
        Material matBreakerBase = CreateLitMaterial("MAT_Breaker_Base", new Color(0.035f, 0.035f, 0.038f), 0.2f, 0.35f);
        Material matBreakerLever = CreateLitMaterial("MAT_Breaker_Lever_White", new Color(0.82f, 0.82f, 0.76f), 0.05f, 0.32f);
        Material matGreenOn = CreateEmissionMaterial("MAT_Circuit_LED_Green_ON", new Color(0.05f, 1.0f, 0.22f), 2.0f);
        Material matLightOff = CreateLitMaterial("MAT_Circuit_LED_OFF_Gray", new Color(0.08f, 0.09f, 0.08f), 0.0f, 0.2f);
        Material matConfirmDisabled = CreateLitMaterial("MAT_Confirm_Disabled_Gray", new Color(0.22f, 0.23f, 0.23f), 0.0f, 0.25f);
        Material matConfirmEnabled = CreateEmissionMaterial("MAT_Confirm_Enabled_Green", new Color(0.05f, 1.0f, 0.25f), 1.8f);
        Material matRedEmission = CreateEmissionMaterial("MAT_Red_Beacon", new Color(1.0f, 0.05f, 0.02f), 2.5f);
        Material matBeaconStandby = CreateEmissionMaterial("MAT_Beacon_Standby_Yellow", new Color(1.0f, 0.72f, 0.02f), 1.5f);
        Material matBeaconComplete = CreateEmissionMaterial("MAT_Beacon_Complete_Green", new Color(0.05f, 1.0f, 0.22f), 2.0f);
        Material matStartButton = CreateEmissionMaterial("MAT_Auto_Start_Button_Red", new Color(1.0f, 0.06f, 0.03f), 1.8f);
        Material matKnifeBlade = CreateLitMaterial("MAT_Ceramic_Knife_Blade", new Color(0.88f, 0.78f, 0.56f), 0.05f, 0.42f);
        Material matCopperContact = CreateLitMaterial("MAT_Copper_Contact", new Color(0.82f, 0.42f, 0.16f), 0.75f, 0.45f);
        Material matAgedSteel = CreateLitMaterial("MAT_Aged_Steel_Caps", new Color(0.58f, 0.54f, 0.48f), 0.8f, 0.38f);
        Material matYellow = CreateLitMaterial("MAT_Warning_Yellow", new Color(1.0f, 0.78f, 0.02f), 0f, 0.35f);
        Material matBlack = CreateLitMaterial("MAT_Warning_Black", new Color(0.02f, 0.02f, 0.018f), 0f, 0.25f);
        Material matWhite = CreateLitMaterial("MAT_Label_White", new Color(0.92f, 0.92f, 0.86f), 0f, 0.3f);
        Material matGaugeFace = CreateLitMaterial("MAT_Gauge_Face", new Color(0.93f, 0.93f, 0.88f), 0f, 0.38f);
        Material matGaugeGreen = CreateEmissionMaterial("MAT_Gauge_Green_Rated", new Color(0.04f, 0.9f, 0.18f), 1.2f);
        Material matGaugeOff = CreateLitMaterial("MAT_Gauge_Off_Gray", new Color(0.22f, 0.23f, 0.22f), 0f, 0.25f);

        GameObject root = new GameObject(RootName);
        root.transform.SetParent(transform, false);

        BreakerShutdownStationRuntime runtime = root.AddComponent<BreakerShutdownStationRuntime>();
        runtime.autoStartKey = autoStartKey;

        BuildWorkArea(root.transform, matYellow, matBlack);
        BuildEmergencyLights(root.transform, runtime, matRedEmission, matBeaconStandby, matBeaconComplete, matBlack, matYellow);
        BuildCabinet(root.transform, runtime, matCabinet, matPanel, matFrame, matBreakerBase, matBreakerLever,
            matGreenOn, matLightOff, matGaugeFace, matGaugeGreen, matGaugeOff,
            matConfirmDisabled, matConfirmEnabled, matStartButton, matKnifeBlade, matCopperContact, matAgedSteel, matWhite, matBlack);

        runtime.lightOnMaterial = matGreenOn;
        runtime.lightOffMaterial = matLightOff;
        runtime.confirmDisabledMaterial = matConfirmDisabled;
        runtime.confirmEnabledMaterial = matConfirmEnabled;
        runtime.gaugeGreenMaterial = matGaugeGreen;
        runtime.gaugeOffMaterial = matGaugeOff;
        runtime.beaconStandbyMaterial = matBeaconStandby;
        runtime.beaconActiveMaterial = matRedEmission;
        runtime.beaconCompleteMaterial = matBeaconComplete;
        runtime.startButtonReadyMaterial = matStartButton;
        runtime.startButtonPressedMaterial = matConfirmEnabled;

        Debug.Log("BreakerShutdown station generated. Correct sequence: 2 → 4 → 1 → 3.");
    }

    private void BuildWorkArea(Transform parent, Material matYellow, Material matBlack)
    {
        // 工位中心约为 (0,0,-9)，作业区 2m × 2m
        CreateCube("WorkArea_Floor_Dim_Panel", parent, new Vector3(0f, 0.006f, -9f), new Vector3(2.05f, 0.012f, 2.05f),
            CreateLitMaterial("MAT_WorkArea_Dark_Floor", new Color(0.09f, 0.10f, 0.11f), 0f, 0.3f));

        float y = 0.025f;
        float zCenter = -9f;
        float half = 1f;
        float strip = 0.06f;

        CreateCube("WarningFrame_Front_Yellow", parent, new Vector3(0f, y, zCenter + half), new Vector3(2.0f, 0.035f, strip), matYellow);
        CreateCube("WarningFrame_Back_Yellow", parent, new Vector3(0f, y, zCenter - half), new Vector3(2.0f, 0.035f, strip), matYellow);
        CreateCube("WarningFrame_Left_Yellow", parent, new Vector3(-half, y, zCenter), new Vector3(strip, 0.035f, 2.0f), matYellow);
        CreateCube("WarningFrame_Right_Yellow", parent, new Vector3(half, y, zCenter), new Vector3(strip, 0.035f, 2.0f), matYellow);

        for (int i = 0; i < 8; i++)
        {
            float x = -0.82f + i * 0.24f;
            CreateCube("WarningFrame_BlackSlash_Front_" + i, parent, new Vector3(x, y + 0.012f, zCenter + half), new Vector3(0.12f, 0.038f, strip * 1.2f), matBlack)
                .transform.localRotation = Quaternion.Euler(0f, 35f, 0f);

            CreateCube("WarningFrame_BlackSlash_Back_" + i, parent, new Vector3(x, y + 0.012f, zCenter - half), new Vector3(0.12f, 0.038f, strip * 1.2f), matBlack)
                .transform.localRotation = Quaternion.Euler(0f, -35f, 0f);
        }
    }

    private void BuildEmergencyLights(
        Transform parent,
        BreakerShutdownStationRuntime runtime,
        Material matRed,
        Material matStandby,
        Material matComplete,
        Material matBlack,
        Material matYellow)
    {
        // 顶部暖光
        GameObject topLightObj = new GameObject("Top_Warm_Work_Light");
        topLightObj.transform.SetParent(parent, false);
        topLightObj.transform.localPosition = new Vector3(0f, 2.7f, -9f);

        Light topLight = topLightObj.AddComponent<Light>();
        topLight.type = LightType.Point;
        topLight.color = new Color(1f, 0.76f, 0.45f);
        topLight.intensity = 1.7f;
        topLight.range = 3.0f;
        topLight.enabled = true;
        runtime.topWorkLight = topLight;

        CreateCube("Top_Light_Housing", parent, new Vector3(0f, 2.55f, -9f), new Vector3(0.45f, 0.06f, 0.25f), matBlack);

        // 红色旋转警示灯
        GameObject beaconPivot = new GameObject("Red_Rotating_Beacon_Pivot");
        beaconPivot.transform.SetParent(parent, false);
        beaconPivot.transform.localPosition = new Vector3(0.72f, 2.12f, -9.62f);
        runtime.beaconPivot = beaconPivot.transform;

        CreateCylinder("Beacon_Base", beaconPivot.transform, new Vector3(0f, -0.06f, 0f), 0.16f, 0.05f, matBlack, CylinderAxis.Y);
        GameObject beaconLens = CreateSphere("Beacon_Status_Lens", beaconPivot.transform, new Vector3(0f, 0.02f, 0f), 0.16f, matStandby);
        runtime.beaconLensRenderer = beaconLens.GetComponent<Renderer>();

        GameObject beaconLightObj = new GameObject("Beacon_Red_Point_Light");
        beaconLightObj.transform.SetParent(beaconPivot.transform, false);
        beaconLightObj.transform.localPosition = new Vector3(0f, 0.03f, 0f);

        Light beaconLight = beaconLightObj.AddComponent<Light>();
        beaconLight.type = LightType.Point;
        beaconLight.color = new Color(1f, 0.72f, 0.02f);
        beaconLight.intensity = 0.75f;
        beaconLight.range = 1.8f;
        runtime.beaconLight = beaconLight;
        runtime.beaconStandbyMaterial = matStandby;
        runtime.beaconActiveMaterial = matRed;
        runtime.beaconCompleteMaterial = matComplete;

        CreateTextLabel(
            "Emergency_Zone_Label",
            parent,
            "紧急停机作业区",
            BreakerShutdownUiFont.Positions.EmergencyZone,
            BreakerShutdownUiFont.Sizes.EmergencyZone,
            new Color(1f, 0.28f, 0.22f));
    }

    private void BuildCabinet(
        Transform parent,
        BreakerShutdownStationRuntime runtime,
        Material matCabinet,
        Material matPanel,
        Material matFrame,
        Material matBreakerBase,
        Material matBreakerLever,
        Material matGreenOn,
        Material matLightOff,
        Material matGaugeFace,
        Material matGaugeGreen,
        Material matGaugeOff,
        Material matConfirmDisabled,
        Material matConfirmEnabled,
        Material matStartButton,
        Material matKnifeBlade,
        Material matCopperContact,
        Material matAgedSteel,
        Material matWhite,
        Material matBlack)
    {
        // 配电柜位置与尺寸
        // 柜体中心 (0,1,-9.7)，尺寸 0.9m(W=X) × 1.6m(H=Y) × 0.3m(D=Z)
        CreateCube("North_Wall_Backdrop", parent, new Vector3(0f, 1.2f, -10.05f), new Vector3(2.6f, 2.4f, 0.08f),
            CreateLitMaterial("MAT_Cold_North_Wall", new Color(0.18f, 0.20f, 0.23f), 0f, 0.3f));

        CreateCube("Cabinet_Body", parent, new Vector3(0f, 1.0f, -9.7f), new Vector3(0.9f, 1.6f, 0.3f), matCabinet);

        CreateCube("Cabinet_Recessed_Panel", parent, new Vector3(0f, 1.0f, -9.535f), new Vector3(0.80f, 1.50f, 0.025f), matPanel);

        // 边框
        CreateCube("Panel_Frame_Top", parent, new Vector3(0f, 1.765f, -9.515f), new Vector3(0.90f, 0.035f, 0.035f), matFrame);
        CreateCube("Panel_Frame_Bottom", parent, new Vector3(0f, 0.235f, -9.515f), new Vector3(0.90f, 0.035f, 0.035f), matFrame);
        CreateCube("Panel_Frame_Left", parent, new Vector3(-0.45f, 1.0f, -9.515f), new Vector3(0.035f, 1.55f, 0.035f), matFrame);
        CreateCube("Panel_Frame_Right", parent, new Vector3(0.45f, 1.0f, -9.515f), new Vector3(0.035f, 1.55f, 0.035f), matFrame);

        // 操作规程牌
        CreateCube("Procedure_Sign_Board", parent, new Vector3(0f, 1.88f, -9.53f), new Vector3(0.62f, 0.16f, 0.025f), matBlack);
        CreateTextLabel(
            "Procedure_Sign_Text",
            parent,
            "顺序 ②→④→①→③",
            BreakerShutdownUiFont.Positions.Procedure,
            BreakerShutdownUiFont.Sizes.Procedure,
            new Color(1f, 0.92f, 0.55f));

        // 总电压表
        Transform gaugeNeedle = BuildVoltageGauge(parent, matGaugeFace, matGaugeGreen, matBlack, out Renderer gaugeZoneRenderer);

        runtime.voltageNeedlePivot = gaugeNeedle;
        runtime.gaugeZoneRenderer = gaugeZoneRenderer;
        runtime.fullVoltageAngle = 330f;
        runtime.zeroVoltageAngle = 210f;

        // 断路器与指示灯
        float[] xs = { -0.27f, -0.09f, 0.09f, 0.27f };

        List<BreakerSwitchRuntime> breakers = new List<BreakerSwitchRuntime>();
        List<Renderer> lights = new List<Renderer>();

        for (int i = 0; i < 4; i++)
        {
            int breakerNumber = i + 1;

            BreakerSwitchRuntime breaker = BuildBreaker(
                parent,
                breakerNumber,
                new Vector3(xs[i], 1.26f, -9.50f),
                matBreakerBase,
                matKnifeBlade,
                matCopperContact,
                matAgedSteel,
                matWhite
            );

            breakers.Add(breaker);

            Renderer lightRenderer = BuildCircuitLight(
                parent,
                "Circuit_LED_" + breakerNumber,
                new Vector3(xs[i], 1.49f, -9.50f),
                matGreenOn
            );

            lights.Add(lightRenderer);

            CreateTextLabel(
                "Breaker_Number_" + breakerNumber,
                parent,
                breakerNumber.ToString(),
                new Vector3(xs[i], 1.12f, BreakerShutdownUiFont.Positions.PanelTextZ),
                BreakerShutdownUiFont.Sizes.BreakerNumber,
                Color.white);
        }

        runtime.breakers = breakers.ToArray();
        runtime.circuitLightRenderers = lights.ToArray();

        foreach (BreakerSwitchRuntime breaker in runtime.breakers)
        {
            breaker.station = runtime;
        }

        Transform startButton = BuildStartButton(parent, matStartButton, matBlack, out Renderer startButtonRenderer);
        runtime.startButtonTransform = startButton;
        runtime.startButtonRenderer = startButtonRenderer;
        runtime.startButtonReadyLocalPosition = startButton.localPosition;
        runtime.startButtonPressedLocalPosition = startButton.localPosition + new Vector3(0f, 0f, -0.025f);

        BreakerStartButtonInteractable startButtonInteractable = startButton.gameObject.AddComponent<BreakerStartButtonInteractable>();
        startButtonInteractable.station = runtime;

        CreateTextLabel(
            "Auto_Start_Label",
            parent,
            "启动(J)",
            BreakerShutdownUiFont.Positions.AutoStart,
            BreakerShutdownUiFont.Sizes.ButtonHint,
            new Color(0.92f, 0.92f, 0.88f));

        // 确认按钮
        runtime.confirmButtonRenderer = BuildConfirmButton(parent, matConfirmDisabled, matBlack);

        CreateTextLabel(
            "Confirm_Label",
            parent,
            "确认",
            BreakerShutdownUiFont.Positions.Confirm,
            BreakerShutdownUiFont.Sizes.ButtonHint,
            new Color(0.92f, 0.92f, 0.88f));

        // 底部小状态牌
        CreateCube("Status_Strip", parent, new Vector3(0f, 0.55f, -9.505f), new Vector3(0.62f, 0.11f, 0.018f), matFrame);
        CreateTextLabel(
            "Status_Text",
            parent,
            "电闸全部合上",
            BreakerShutdownUiFont.Positions.Status,
            BreakerShutdownUiFont.Sizes.Status,
            new Color(0.75f, 0.98f, 0.82f));

        runtime.statusText = parent.Find("Status_Text")?.GetComponent<TextMesh>();
    }

    private Transform BuildVoltageGauge(Transform parent, Material matGaugeFace, Material matGaugeGreen, Material matNeedle, out Renderer gaugeZoneRenderer)
    {
        Vector3 center = new Vector3(0f, 1.68f, -9.50f);

        CreateCylinder("Voltage_Gauge_Metal_Ring", parent, center, 0.17f, 0.025f,
            CreateLitMaterial("MAT_Gauge_Metal_Ring", new Color(0.32f, 0.33f, 0.33f), 0.75f, 0.45f),
            CylinderAxis.Z);

        CreateCylinder("Voltage_Gauge_Face", parent, center + new Vector3(0f, 0f, 0.016f), 0.145f, 0.012f, matGaugeFace, CylinderAxis.Z);

        gaugeZoneRenderer = CreateGaugeArc("Voltage_Gauge_Green_Rated_Zone", parent, center + new Vector3(0f, 0f, 0.030f), 0.045f, 0.065f, 285f, 350f, matGaugeGreen);

        Transform needlePivot = CreateGaugeNeedle(parent, center + new Vector3(0f, 0f, 0.040f), 330f, matNeedle);

        CreateTextLabel("Voltage_Gauge_Label", parent, "电压", BreakerShutdownUiFont.Positions.GaugeTitle, BreakerShutdownUiFont.Sizes.GaugeTitle, Color.white);
        CreateTextLabel("Voltage_Gauge_Zero", parent, "0", BreakerShutdownUiFont.Positions.GaugeZero, BreakerShutdownUiFont.Sizes.GaugeHint, Color.black);
        CreateTextLabel("Voltage_Gauge_Rated", parent, "正常", BreakerShutdownUiFont.Positions.GaugeRated, BreakerShutdownUiFont.Sizes.GaugeHint, new Color(0f, 0.55f, 0.1f));

        return needlePivot;
    }

    private BreakerSwitchRuntime BuildBreaker(
        Transform parent,
        int breakerNumber,
        Vector3 position,
        Material matBase,
        Material matLever,
        Material matCopper,
        Material matRedHandle,
        Material matWhite)
    {
        string prefix = "Breaker_" + breakerNumber;

        CreateCube(prefix + "_Knife_Backplate", parent, position + new Vector3(0f, 0.02f, -0.018f), new Vector3(0.155f, 0.46f, 0.030f), matBase);
        CreateCube(prefix + "_Knife_Panel_Bevel_Top", parent, position + new Vector3(0f, 0.25f, 0.001f), new Vector3(0.135f, 0.012f, 0.012f), matWhite);
        CreateCube(prefix + "_Knife_Panel_Bevel_Bottom", parent, position + new Vector3(0f, -0.205f, 0.001f), new Vector3(0.135f, 0.012f, 0.012f), matWhite);
        CreateCube(prefix + "_Knife_Panel_Bevel_Left", parent, position + new Vector3(-0.074f, 0.02f, 0.001f), new Vector3(0.010f, 0.42f, 0.012f), matWhite);
        CreateCube(prefix + "_Knife_Panel_Bevel_Right", parent, position + new Vector3(0.074f, 0.02f, 0.001f), new Vector3(0.010f, 0.42f, 0.012f), matWhite);

        Vector3 upperPlate = position + new Vector3(0f, 0.145f, 0.020f);
        Vector3 lowerPlate = position + new Vector3(0f, -0.145f, 0.020f);

        CreateCube(prefix + "_Upper_Brass_Mount_Plate", parent, upperPlate, new Vector3(0.105f, 0.070f, 0.020f), matCopper);
        CreateCube(prefix + "_Lower_Brass_Mount_Plate", parent, lowerPlate, new Vector3(0.112f, 0.074f, 0.020f), matCopper);

        CreateCube(prefix + "_Upper_Hinge_Block", parent, upperPlate + new Vector3(0f, 0f, 0.018f), new Vector3(0.088f, 0.035f, 0.028f), matRedHandle);
        CreateCylinder(prefix + "_Upper_Hinge_Dome", parent, upperPlate + new Vector3(0f, 0.012f, 0.038f), 0.052f, 0.088f, matRedHandle, CylinderAxis.X);
        CreateCube(prefix + "_Lower_Hinge_Block", parent, lowerPlate + new Vector3(0f, 0f, 0.018f), new Vector3(0.092f, 0.035f, 0.030f), matRedHandle);
        CreateCylinder(prefix + "_Lower_Hinge_Dome", parent, lowerPlate + new Vector3(0f, 0.012f, 0.040f), 0.056f, 0.092f, matRedHandle, CylinderAxis.X);

        CreateSphere(prefix + "_Screw_Top_Left", parent, position + new Vector3(-0.060f, 0.225f, 0.018f), 0.012f, matRedHandle);
        CreateSphere(prefix + "_Screw_Top_Right", parent, position + new Vector3(0.060f, 0.225f, 0.018f), 0.012f, matRedHandle);
        CreateSphere(prefix + "_Screw_Bottom_Left", parent, position + new Vector3(-0.060f, -0.185f, 0.018f), 0.012f, matRedHandle);
        CreateSphere(prefix + "_Screw_Bottom_Right", parent, position + new Vector3(0.060f, -0.185f, 0.018f), 0.012f, matRedHandle);

        CreateCube(prefix + "_On_Arrow", parent, position + new Vector3(0.030f, 0.060f, 0.018f), new Vector3(0.010f, 0.075f, 0.010f), matWhite)
            .transform.localRotation = Quaternion.Euler(0f, 0f, -34f);
        CreateCube(prefix + "_Off_Arrow", parent, position + new Vector3(-0.030f, -0.070f, 0.018f), new Vector3(0.010f, 0.075f, 0.010f), matWhite)
            .transform.localRotation = Quaternion.Euler(0f, 0f, 34f);

        GameObject pivotObj = new GameObject("Breaker_" + breakerNumber + "_Lever_Pivot");
        pivotObj.transform.SetParent(parent, false);
        pivotObj.transform.localPosition = lowerPlate + new Vector3(0f, 0.010f, 0.070f);
        BoxCollider pivotCollider = pivotObj.AddComponent<BoxCollider>();
        pivotCollider.center = new Vector3(0f, 0.16f, 0.01f);
        pivotCollider.size = new Vector3(0.18f, 0.46f, 0.13f);

        CreateCylinderBetween(prefix + "_Lever_Rod_Left", pivotObj.transform, new Vector3(-0.045f, 0.015f, 0f), new Vector3(-0.064f, 0.305f, 0f), 0.015f, matRedHandle);
        CreateCylinderBetween(prefix + "_Lever_Rod_Right", pivotObj.transform, new Vector3(0.045f, 0.015f, 0f), new Vector3(0.064f, 0.305f, 0f), 0.015f, matRedHandle);
        CreateCylinder(prefix + "_Lever_Clickable", pivotObj.transform, new Vector3(0f, 0.330f, 0f), 0.046f, 0.170f, matLever, CylinderAxis.X);
        CreateCylinder(prefix + "_Lever_End_Left", pivotObj.transform, new Vector3(-0.100f, 0.330f, 0f), 0.050f, 0.040f, matRedHandle, CylinderAxis.X);
        CreateCylinder(prefix + "_Lever_End_Right", pivotObj.transform, new Vector3(0.100f, 0.330f, 0f), 0.050f, 0.040f, matRedHandle, CylinderAxis.X);

        BreakerSwitchRuntime breaker = pivotObj.AddComponent<BreakerSwitchRuntime>();
        breaker.breakerNumber = breakerNumber;
        breaker.leverPivot = pivotObj.transform;
        breaker.onRotation = Quaternion.Euler(0f, 0f, 0f);
        breaker.offRotation = Quaternion.Euler(68f, 0f, 0f);
        breaker.pullDuration = 0.75f;
        breaker.leverPivot.localRotation = breaker.onRotation;

        return breaker;
    }

    private Renderer BuildCircuitLight(Transform parent, string name, Vector3 position, Material matOn)
    {
        GameObject lightObj = CreateSphere(name, parent, position + new Vector3(0f, 0f, 0.025f), 0.035f, matOn);

        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(0.05f, 1f, 0.2f);
        pointLight.intensity = 0.5f;
        pointLight.range = 0.35f;

        return lightObj.GetComponent<Renderer>();
    }

    private Transform BuildStartButton(Transform parent, Material matReady, Material matBlack, out Renderer buttonRenderer)
    {
        Vector3 pos = new Vector3(-0.24f, 0.98f, -9.50f);

        CreateCube("Auto_Start_Button_Base", parent, pos + new Vector3(0f, 0f, -0.005f), new Vector3(0.17f, 0.09f, 0.035f), matBlack);

        GameObject cap = CreateCylinder("Auto_Start_Button_Cap_Clickable", parent, pos + new Vector3(0f, 0f, 0.036f), 0.075f, 0.038f, matReady, CylinderAxis.Z);
        buttonRenderer = cap.GetComponent<Renderer>();
        return cap.transform;
    }

    private Renderer BuildConfirmButton(Transform parent, Material matDisabled, Material matBlack)
    {
        Vector3 pos = new Vector3(0f, 1.05f, -9.50f);

        CreateCube("Confirm_Button_Base", parent, pos + new Vector3(0f, 0f, -0.005f), new Vector3(0.15f, 0.08f, 0.035f), matBlack);

        GameObject cap = CreateCylinder("Confirm_Button_Cap_Clickable", parent, pos + new Vector3(0f, 0f, 0.035f), 0.07f, 0.035f, matDisabled, CylinderAxis.Z);
        return cap.GetComponent<Renderer>();
    }

    private Transform CreateGaugeNeedle(Transform parent, Vector3 localPosition, float angleDeg, Material material)
    {
        GameObject pivot = new GameObject("Voltage_Gauge_Needle_Pivot");
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = localPosition;
        pivot.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);

        GameObject needle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        needle.name = "Voltage_Gauge_Needle";
        needle.transform.SetParent(pivot.transform, false);
        needle.transform.localPosition = new Vector3(0.045f, 0f, 0f);
        needle.transform.localRotation = Quaternion.identity;
        needle.transform.localScale = new Vector3(0.09f, 0.008f, 0.008f);

        needle.GetComponent<Renderer>().sharedMaterial = material;

        if (!addColliders)
        {
            DestroySafe(needle.GetComponent<Collider>());
        }

        return pivot.transform;
    }

    private Renderer CreateGaugeArc(string name, Transform parent, Vector3 localPosition, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

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

            normals[index] = Vector3.forward;
            normals[index + 1] = Vector3.forward;

            uvs[index] = new Vector2(t, 0f);
            uvs[index + 1] = new Vector2(t, 1f);
        }

        for (int i = 0; i < segments; i++)
        {
            int v = i * 2;
            int tri = i * 6;

            triangles[tri] = v;
            triangles[tri + 1] = v + 1;
            triangles[tri + 2] = v + 2;

            triangles[tri + 3] = v + 2;
            triangles[tri + 4] = v + 1;
            triangles[tri + 5] = v + 3;
        }

        Mesh mesh = new Mesh();
        mesh.name = name + "_Mesh";
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        return meshRenderer;
    }

    private enum CylinderAxis
    {
        X,
        Y,
        Z
    }

    private GameObject CreateCube(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = size;

        obj.GetComponent<Renderer>().sharedMaterial = material;

        if (!addColliders)
        {
            DestroySafe(obj.GetComponent<Collider>());
        }

        return obj;
    }

    private GameObject CreateSphere(string name, Transform parent, Vector3 localPosition, float diameter, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one * diameter;

        obj.GetComponent<Renderer>().sharedMaterial = material;

        if (!addColliders)
        {
            DestroySafe(obj.GetComponent<Collider>());
        }

        return obj;
    }

    private GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, float diameter, float length, Material material, CylinderAxis axis)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);

        if (axis == CylinderAxis.X)
            obj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        else if (axis == CylinderAxis.Z)
            obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        else
            obj.transform.localRotation = Quaternion.identity;

        obj.GetComponent<Renderer>().sharedMaterial = material;

        if (!addColliders)
        {
            DestroySafe(obj.GetComponent<Collider>());
        }

        return obj;
    }

    private GameObject CreateCylinderBetween(string name, Transform parent, Vector3 startLocal, Vector3 endLocal, float diameter, Material material)
    {
        Vector3 direction = endLocal - startLocal;
        float length = direction.magnitude;

        if (length <= 0.0001f)
        {
            return CreateCylinder(name, parent, startLocal, diameter, 0.001f, material, CylinderAxis.Y);
        }

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = (startLocal + endLocal) * 0.5f;
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);

        obj.GetComponent<Renderer>().sharedMaterial = material;

        if (!addColliders)
        {
            DestroySafe(obj.GetComponent<Collider>());
        }

        return obj;
    }

    private void CreateTextLabel(string name, Transform parent, string text, Vector3 localPosition, float size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = BreakerShutdownUiFont.PanelFaceRotation;

        TextMesh textMesh = obj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = size;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.lineSpacing = 0.82f;
        textMesh.richText = false;

        BreakerShutdownUiFont.Apply(textMesh, size);
    }

    private Material CreateLitMaterial(string name, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader == null)
            shader = Shader.Find("Diffuse");

        Material mat = new Material(shader);
        mat.name = name;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);

        return mat;
    }

    private Material CreateEmissionMaterial(string name, Color color, float intensity)
    {
        Material mat = CreateLitMaterial(name, color, 0f, 0.6f);

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * intensity);
        }

        return mat;
    }

    private void ClearOldModel()
    {
        Transform old = transform.Find(RootName);

        if (old != null)
        {
            DestroySafe(old.gameObject);
        }
    }

    private void DestroySafe(Object obj)
    {
        if (obj == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(obj);
        else
            Destroy(obj);
#else
        Destroy(obj);
#endif
    }
}

