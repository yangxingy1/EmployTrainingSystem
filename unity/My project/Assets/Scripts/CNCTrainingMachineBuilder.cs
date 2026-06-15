using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

internal static class CNCUiFont
{
    private static Font _cachedFont;

    public static readonly Quaternion PanelFaceRotation = Quaternion.Euler(0f, 180f, 0f);

    private const int FontRequestSize = 128;

    private static readonly string[] PreferredFonts =
    {
        "DengXian",
        "等线",
        "Microsoft YaHei UI",
        "Microsoft YaHei",
        "微软雅黑",
        "Source Han Sans SC",
        "Noto Sans CJK SC",
        "SimHei",
        "黑体",
        "YouYuan",
        "幼圆",
        "STYuanti",
        "华文圆体"
    };

    public static class Sizes
    {
        public const float MachineName = 0.040f;
        public const float DoorInstruction = 0.024f;
        public const float TeachingSequence = 0.032f;
        public const float ButtonHint = 0.017f;
        public const float Status = 0.020f;
        public const float PowerOnOff = 0.011f;
    }

    public static class Positions
    {
        public const float FrontTextZ = 0.935f;
        public const float PanelTextZ = 0.955f;
        public static readonly Vector3 MachineName = new Vector3(-0.28f, 0.06f, 1.08f);
        public static readonly Vector3 DoorInstruction = new Vector3(-0.28f, 0.32f, FrontTextZ);
        public static readonly Vector3 TeachingSequence = new Vector3(-0.28f, 0.16f, FrontTextZ);
        public static readonly Vector3 Status = new Vector3(0.82f, 1.43f, PanelTextZ);
        public static readonly Vector3 MainPower = new Vector3(0.70f, 1.04f, PanelTextZ);
        public static readonly Vector3 CycleStart = new Vector3(0.93f, 1.04f, PanelTextZ);
        public static readonly Vector3 EStop = new Vector3(0.82f, 0.74f, PanelTextZ);
        public static readonly Vector3 Reset = new Vector3(0.70f, 0.53f, PanelTextZ);
        public static readonly Vector3 Mode = new Vector3(0.94f, 0.53f, PanelTextZ);
        public static readonly Vector3 PowerOn = new Vector3(0.58f, 1.18f, PanelTextZ);
        public static readonly Vector3 PowerOff = new Vector3(0.58f, 1.10f, PanelTextZ);
    }

    public static Font Resolve()
    {
        if (_cachedFont != null)
        {
            return _cachedFont;
        }

        foreach (string fontName in PreferredFonts)
        {
            Font candidate = TryCreateFont(fontName);
            if (candidate != null && SupportsChinese(candidate))
            {
                _cachedFont = candidate;
                return _cachedFont;
            }
        }

        _cachedFont = Font.CreateDynamicFontFromOSFont(PreferredFonts, FontRequestSize);

        if (_cachedFont == null)
        {
            try
            {
                _cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch
            {
                // Built-in font fallback when OS fonts are unavailable.
            }
        }

        return _cachedFont;
    }

    private static Font TryCreateFont(string fontName)
    {
        try
        {
            return Font.CreateDynamicFontFromOSFont(new[] { fontName }, FontRequestSize);
        }
        catch
        {
            return null;
        }
    }

    private static bool SupportsChinese(Font font)
    {
        return font != null
            && font.HasCharacter('\u5B9E')
            && font.HasCharacter('\u673A')
            && font.HasCharacter('\u7535');
    }

    public static void Apply(TextMesh textMesh, float characterSize, bool bold = false)
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
        textMesh.fontSize = Mathf.Clamp(Mathf.RoundToInt(characterSize * 1800f), 56, FontRequestSize);
        textMesh.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

        Renderer renderer = textMesh.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = font.material;
        }
    }
}

public class CNCTrainingMachineBuilder : MonoBehaviour
{
    public const string StaticMachineName = "CNC Training Machine Static";
    public const string GeneratedModelName = "__Generated_CNC_Training_Machine";

    [Header("Placement")]
    public Vector3 machineWorldPosition = new Vector3(0f, 0f, -6f);

    [Tooltip("机床正面默认朝 +Z，玩家从南侧靠近。")]
    public bool faceSouth = true;

    [Header("Size, meters")]
    public float width = 2.4f;
    public float height = 2.2f;
    public float depth = 1.6f;

    [Header("Keyboard Interaction")]
    public KeyCode powerKey = KeyCode.Alpha1;
    public KeyCode doorKey = KeyCode.Alpha2;
    public KeyCode clampKey = KeyCode.Alpha3;
    public KeyCode cycleStartKey = KeyCode.Alpha4;
    public KeyCode emergencyStopKey = KeyCode.Alpha5;
    public KeyCode resetKey = KeyCode.Alpha6;
    public KeyCode modeKey = KeyCode.Alpha7;

    [Header("Build Options")]
    public bool buildOnStart = false;
    public bool addColliders = true;

    private const string RootName = GeneratedModelName;

    // 运行时生成的材质 + 缓存，用于重建时回收，避免材质泄漏
    private readonly List<Material> _runtimeMaterials = new List<Material>();
    private CNCTrainingMachineRuntime _runtime;
    private Shader _litShader;

    private void Start()
    {
        if (buildOnStart)
        {
            BuildMachine();
        }
    }

    [ContextMenu("Build / Rebuild CNC Training Machine")]
    public void BuildMachine()
    {
        ClearOldModel();

        transform.position = machineWorldPosition;
        transform.rotation = faceSouth ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        transform.localScale = Vector3.one;

        Material matMachineWhite = CreateLitMaterial("MAT_CNC_Machine_White", new Color(0.72f, 0.74f, 0.76f), 0.22f, 0.42f);
        Material matDarkMetal = CreateLitMaterial("MAT_CNC_Dark_Metal", new Color(0.10f, 0.11f, 0.12f), 0.82f, 0.52f);
        Material matLightMetal = CreateLitMaterial("MAT_CNC_Light_Metal", new Color(0.38f, 0.40f, 0.41f), 0.85f, 0.58f);
        Material matBlack = CreateLitMaterial("MAT_CNC_Black_Rubber", new Color(0.02f, 0.02f, 0.02f), 0.15f, 0.28f);
        Material matGlass = CreateTransparentMaterial("MAT_CNC_Blue_Glass", new Color(0.18f, 0.42f, 0.72f, 0.22f));
        Material matScreenOn = CreateEmissionMaterial("MAT_CNC_Screen_ON", new Color(0.04f, 0.72f, 0.62f), 1.6f);
        Material matScreenOff = CreateLitMaterial("MAT_CNC_Screen_OFF", new Color(0.008f, 0.012f, 0.015f), 0f, 0.12f);
        Material matButtonGreen = CreateEmissionMaterial("MAT_Button_Green", new Color(0.05f, 1.0f, 0.25f), 1.7f);
        Material matButtonRed = CreateEmissionMaterial("MAT_Button_Red", new Color(1.0f, 0.04f, 0.02f), 2.0f);
        Material matButtonYellow = CreateEmissionMaterial("MAT_Button_Yellow", new Color(1.0f, 0.72f, 0.04f), 1.6f);
        Material matButtonGray = CreateLitMaterial("MAT_Button_Gray", new Color(0.20f, 0.21f, 0.21f), 0.35f, 0.32f);
        Material matLedOff = CreateLitMaterial("MAT_LED_Off_Dark", new Color(0.04f, 0.04f, 0.04f), 0f, 0.15f);
        Material matWorkpiece = CreateLitMaterial("MAT_Workpiece_Aluminum", new Color(0.65f, 0.66f, 0.62f), 0.9f, 0.48f);
        Material matWarningYellow = CreateLitMaterial("MAT_Warning_Yellow", new Color(1.0f, 0.78f, 0.03f), 0f, 0.35f);

        GameObject root = new GameObject(RootName);
        root.transform.SetParent(transform, false);

        CNCTrainingMachineRuntime runtime = root.AddComponent<CNCTrainingMachineRuntime>();
        _runtime = runtime;
        runtime.powerKey = powerKey;
        runtime.doorKey = doorKey;
        runtime.clampKey = clampKey;
        runtime.cycleStartKey = cycleStartKey;
        runtime.emergencyStopKey = emergencyStopKey;
        runtime.resetKey = resetKey;
        runtime.modeKey = modeKey;

        runtime.matScreenOn = matScreenOn;
        runtime.matScreenOff = matScreenOff;
        runtime.matGreen = matButtonGreen;
        runtime.matRed = matButtonRed;
        runtime.matYellow = matButtonYellow;
        runtime.matGray = matButtonGray;
        runtime.matLedOff = matLedOff;

        BuildMachineBody(root.transform, matMachineWhite, matDarkMetal, matGlass, matWarningYellow, matBlack);
        BuildWorkChamber(root.transform, runtime, matDarkMetal, matLightMetal, matWorkpiece, matBlack);
        BuildControlPanel(root.transform, runtime, matDarkMetal, matLightMetal, matScreenOn, matScreenOff, matButtonGreen, matButtonRed, matButtonYellow, matButtonGray, matBlack);
        BuildStackLight(root.transform, runtime, matButtonGreen, matButtonRed, matButtonYellow, matLedOff, matBlack);
        BuildLabels(root.transform);

        runtime.ApplyInitialState();

        Debug.Log("CNC 实训机床已生成。键盘顺序：1电源 → 2安全门 → 3夹具 → 4启动 → 5急停 → 6复位 → 7模式。");
    }

    private void BuildMachineBody(Transform parent, Material matWhite, Material matDarkMetal, Material matGlass, Material matWarningYellow, Material matBlack)
    {
        float enclosureHeight = height * 0.82f;
        float enclosureCenterY = 0.15f + enclosureHeight * 0.5f;

        CreateCube(
            "CNC_Base_Platform",
            parent,
            new Vector3(0f, 0.15f, 0f),
            new Vector3(width, 0.30f, depth),
            matDarkMetal
        );

        float footInsetX = width * 0.42f;
        float footInsetZ = depth * 0.38f;
        CreateCube("CNC_Foot_Pad_FL", parent, new Vector3(-footInsetX, 0.04f, footInsetZ), new Vector3(0.14f, 0.08f, 0.14f), matDarkMetal);
        CreateCube("CNC_Foot_Pad_FR", parent, new Vector3(footInsetX, 0.04f, footInsetZ), new Vector3(0.14f, 0.08f, 0.14f), matDarkMetal);
        CreateCube("CNC_Foot_Pad_BL", parent, new Vector3(-footInsetX, 0.04f, -footInsetZ), new Vector3(0.14f, 0.08f, 0.14f), matDarkMetal);
        CreateCube("CNC_Foot_Pad_BR", parent, new Vector3(footInsetX, 0.04f, -footInsetZ), new Vector3(0.14f, 0.08f, 0.14f), matDarkMetal);

        CreateCube(
            "CNC_Main_Enclosure",
            parent,
            new Vector3(0f, enclosureCenterY, -0.05f),
            new Vector3(width, enclosureHeight, depth),
            matWhite
        );

        for (int i = 0; i < 4; i++)
        {
            float ventY = enclosureCenterY - 0.35f + i * 0.22f;
            CreateCube("CNC_Side_Vent_Slot_" + i, parent, new Vector3(width * 0.49f, ventY, -0.12f), new Vector3(0.018f, 0.08f, 0.55f), matDarkMetal);
        }

        CreateCube(
            "CNC_Work_Chamber_Dark_Back",
            parent,
            new Vector3(-0.28f, enclosureCenterY, 0.46f),
            new Vector3(1.28f, 1.08f, 0.08f),
            matDarkMetal
        );

        CreateCube("CNC_Front_Frame_Top", parent, new Vector3(-0.28f, enclosureCenterY + 0.56f, 0.79f), new Vector3(1.42f, 0.10f, 0.12f), matDarkMetal);
        CreateCube("CNC_Front_Frame_Bottom", parent, new Vector3(-0.28f, enclosureCenterY - 0.57f, 0.79f), new Vector3(1.42f, 0.10f, 0.12f), matDarkMetal);
        CreateCube("CNC_Front_Frame_Left", parent, new Vector3(-1.02f, enclosureCenterY, 0.79f), new Vector3(0.10f, 1.18f, 0.12f), matDarkMetal);
        CreateCube("CNC_Front_Frame_Right", parent, new Vector3(0.46f, enclosureCenterY, 0.79f), new Vector3(0.10f, 1.18f, 0.12f), matDarkMetal);

        CreateCube("Door_Track_Left", parent, new Vector3(-0.86f, enclosureCenterY - 0.52f, 0.875f), new Vector3(0.04f, 0.025f, 1.30f), matDarkMetal);
        CreateCube("Door_Track_Right", parent, new Vector3(0.30f, enclosureCenterY - 0.52f, 0.875f), new Vector3(0.04f, 0.025f, 1.30f), matDarkMetal);

        GameObject doorLeft = CreateCube(
            "Safety_Door_Left_Glass",
            parent,
            new Vector3(-0.57f, enclosureCenterY, 0.86f),
            new Vector3(0.60f, 0.98f, 0.032f),
            matGlass
        );

        GameObject doorRight = CreateCube(
            "Safety_Door_Right_Glass",
            parent,
            new Vector3(0.02f, enclosureCenterY, 0.865f),
            new Vector3(0.60f, 0.98f, 0.032f),
            matGlass
        );

        runtimeAssignDoor(doorLeft.transform, doorRight.transform);

        GameObject handle = CreateCube(
            "Safety_Door_Handle_Clickable",
            doorRight.transform,
            new Vector3(0.27f, 0f, 0.065f),
            new Vector3(0.045f, 0.36f, 0.055f),
            matDarkMetal
        );

        AddInteractable(handle, CNCInteractionType.ToggleDoor);

        float warningY = enclosureCenterY - 0.63f;
        CreateCube("Door_Warning_Yellow_Strip", parent, new Vector3(-0.28f, warningY, 0.915f), new Vector3(1.32f, 0.035f, 0.035f), matWarningYellow);
        CreateCube("Door_Warning_Yellow_Strip_Left", parent, new Vector3(-1.02f, enclosureCenterY, 0.915f), new Vector3(0.035f, 1.05f, 0.035f), matWarningYellow);
        CreateCube("Door_Warning_Yellow_Strip_Right", parent, new Vector3(0.46f, enclosureCenterY, 0.915f), new Vector3(0.035f, 1.05f, 0.035f), matWarningYellow);

        for (int i = 0; i < 7; i++)
        {
            float x = -0.78f + i * 0.24f;
            CreateCube("Door_Warning_BlackSlash_" + i, parent, new Vector3(x, warningY + 0.012f, 0.918f), new Vector3(0.11f, 0.038f, 0.04f), matBlack)
                .transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
        }
    }

    private void runtimeAssignDoor(Transform doorLeft, Transform doorRight)
    {
        _runtime.leftDoor = doorLeft;
        _runtime.rightDoor = doorRight;
    }

    private void BuildWorkChamber(Transform parent, CNCTrainingMachineRuntime runtime, Material matDarkMetal, Material matLightMetal, Material matWorkpiece, Material matBlack)
    {
        CreateCube(
            "Machine_Table",
            parent,
            new Vector3(-0.28f, 0.64f, 0.25f),
            new Vector3(0.92f, 0.12f, 0.50f),
            matLightMetal
        );

        CreateCube("Machine_Chip_Tray", parent, new Vector3(-0.28f, 0.58f, 0.48f), new Vector3(0.86f, 0.04f, 0.12f), matDarkMetal);

        for (int i = 0; i < 3; i++)
        {
            CreateCube(
                "Machine_Table_TSlot_" + i,
                parent,
                new Vector3(-0.28f, 0.712f, 0.08f + i * 0.16f),
                new Vector3(0.84f, 0.022f, 0.028f),
                matDarkMetal
            );
        }

        GameObject workpiece = CreateCube(
            "Workpiece_Aluminum_Block",
            parent,
            new Vector3(-0.28f, 0.80f, 0.25f),
            new Vector3(0.32f, 0.18f, 0.24f),
            matWorkpiece
        );

        runtime.workpiece = workpiece.transform;

        GameObject leftJaw = CreateCube(
            "Clamp_Jaw_Left",
            parent,
            new Vector3(-0.53f, 0.82f, 0.25f),
            new Vector3(0.08f, 0.16f, 0.28f),
            matDarkMetal
        );
        CreateCube("Clamp_Jaw_Left_Bevel", parent, new Vector3(-0.49f, 0.82f, 0.38f), new Vector3(0.04f, 0.12f, 0.06f), matLightMetal);

        GameObject rightJaw = CreateCube(
            "Clamp_Jaw_Right",
            parent,
            new Vector3(-0.03f, 0.82f, 0.25f),
            new Vector3(0.08f, 0.16f, 0.28f),
            matDarkMetal
        );
        CreateCube("Clamp_Jaw_Right_Bevel", parent, new Vector3(-0.07f, 0.82f, 0.38f), new Vector3(0.04f, 0.12f, 0.06f), matLightMetal);

        runtime.leftClampJaw = leftJaw.transform;
        runtime.rightClampJaw = rightJaw.transform;

        GameObject clampHandlePivot = new GameObject("Clamp_Handle_Pivot");
        clampHandlePivot.transform.SetParent(parent, false);
        clampHandlePivot.transform.localPosition = new Vector3(0.19f, 0.82f, 0.47f);

        GameObject clampHandleStem = CreateCube(
            "Clamp_Handle_Clickable",
            clampHandlePivot.transform,
            new Vector3(0f, 0.08f, 0f),
            new Vector3(0.035f, 0.16f, 0.035f),
            matBlack
        );
        CreateCube("Clamp_Handle_Grip", clampHandlePivot.transform, new Vector3(0f, 0.18f, 0.03f), new Vector3(0.12f, 0.035f, 0.035f), matBlack);

        runtime.clampHandlePivot = clampHandlePivot.transform;
        AddInteractable(clampHandleStem, CNCInteractionType.ToggleClamp);

        CreateCube(
            "Spindle_Head_Housing",
            parent,
            new Vector3(-0.28f, 1.33f, 0.25f),
            new Vector3(0.34f, 0.28f, 0.28f),
            matLightMetal
        );

        for (int i = 0; i < 5; i++)
        {
            CreateCube("Spindle_Fin_" + i, parent, new Vector3(-0.42f + i * 0.07f, 1.33f, 0.12f), new Vector3(0.025f, 0.18f, 0.06f), matDarkMetal);
        }

        GameObject spindlePivot = new GameObject("Spindle_Rotator");
        spindlePivot.transform.SetParent(parent, false);
        spindlePivot.transform.localPosition = new Vector3(-0.28f, 1.15f, 0.25f);
        runtime.spindleRotator = spindlePivot.transform;

        CreateCylinder("Spindle_Tool_Shank", spindlePivot.transform, new Vector3(0f, -0.02f, 0f), 0.06f, 0.12f, matLightMetal, CylinderAxis.Y);
        GameObject spindle = CreateCylinder(
            "Spindle_Tool",
            spindlePivot.transform,
            new Vector3(0f, -0.12f, 0f),
            0.05f,
            0.22f,
            matDarkMetal,
            CylinderAxis.Y
        );

        runtime.tool = spindle.transform;

        CreateCube("Coolant_Joint_Base", parent, new Vector3(0.08f, 1.30f, 0.14f), new Vector3(0.06f, 0.06f, 0.06f), matDarkMetal);
        CreateCylinderBetween("Coolant_Hose_0", parent, new Vector3(0.08f, 1.30f, 0.14f), new Vector3(0.16f, 1.20f, 0.22f), 0.032f, matBlack);
        CreateCylinderBetween("Coolant_Hose_1", parent, new Vector3(0.16f, 1.20f, 0.22f), new Vector3(0.06f, 1.06f, 0.30f), 0.032f, matBlack);
        CreateCylinder("Coolant_Nozzle", parent, new Vector3(0.05f, 1.04f, 0.31f), 0.04f, 0.14f, matDarkMetal, CylinderAxis.Y);
    }

    private void BuildControlPanel(
        Transform parent,
        CNCTrainingMachineRuntime runtime,
        Material matPanel,
        Material matFrame,
        Material matScreenOn,
        Material matScreenOff,
        Material matGreen,
        Material matRed,
        Material matYellow,
        Material matGray,
        Material matBlack)
    {
        CreateCube(
            "Control_Panel_Outer_Frame",
            parent,
            new Vector3(0.84f, 1.08f, 0.83f),
            new Vector3(0.46f, 1.10f, 0.18f),
            matFrame
        );

        CreateCube(
            "Control_Panel_Body",
            parent,
            new Vector3(0.84f, 1.08f, 0.825f),
            new Vector3(0.40f, 1.02f, 0.14f),
            matPanel
        );

        CreateCube("Screen_Bezel", parent, new Vector3(0.84f, 1.43f, 0.918f), new Vector3(0.36f, 0.26f, 0.03f), matFrame);

        GameObject screen = CreateCube(
            "CNC_Status_Screen",
            parent,
            new Vector3(0.84f, 1.43f, 0.928f),
            new Vector3(0.32f, 0.22f, 0.022f),
            matScreenOff
        );

        runtime.screenRenderer = screen.GetComponent<Renderer>();

        GameObject screenTextObj = new GameObject("CNC_Status_Text");
        screenTextObj.transform.SetParent(parent, false);
        screenTextObj.transform.localPosition = CNCUiFont.Positions.Status;
        screenTextObj.transform.localRotation = CNCUiFont.PanelFaceRotation;

        TextMesh screenText = screenTextObj.AddComponent<TextMesh>();
        screenText.text = "电源关闭";
        screenText.characterSize = CNCUiFont.Sizes.Status;
        screenText.anchor = TextAnchor.MiddleCenter;
        screenText.alignment = TextAlignment.Center;
        screenText.color = new Color(0.2f, 0.95f, 0.85f);
        screenText.lineSpacing = 0.82f;
        CNCUiFont.Apply(screenText, CNCUiFont.Sizes.Status);
        runtime.statusText = screenText;

        GameObject powerSwitchPivot = new GameObject("Main_Power_Switch_Pivot");
        powerSwitchPivot.transform.SetParent(parent, false);
        powerSwitchPivot.transform.localPosition = new Vector3(0.70f, 1.18f, 0.945f);

        GameObject powerSwitch = CreateCube(
            "Main_Power_Switch_Clickable",
            powerSwitchPivot.transform,
            new Vector3(0f, 0f, 0.015f),
            new Vector3(0.055f, 0.20f, 0.04f),
            matGray
        );

        runtime.powerSwitchPivot = powerSwitchPivot.transform;
        runtime.powerSwitchRenderer = powerSwitch.GetComponent<Renderer>();
        AddInteractable(powerSwitch, CNCInteractionType.TogglePower);

        CreateTextLabel("Label_MainPower", parent, "1 电源", CNCUiFont.Positions.MainPower, CNCUiFont.Sizes.ButtonHint, Color.white);
        CreateTextLabel("Label_PowerOn", parent, "开", CNCUiFont.Positions.PowerOn, CNCUiFont.Sizes.PowerOnOff, new Color(0.2f, 0.9f, 0.3f));
        CreateTextLabel("Label_PowerOff", parent, "关", CNCUiFont.Positions.PowerOff, CNCUiFont.Sizes.PowerOnOff, new Color(0.75f, 0.75f, 0.75f));

        CreateCylinder("Cycle_Start_Button_Pedestal", parent, new Vector3(0.93f, 1.18f, 0.938f), 0.09f, 0.02f, matGray, CylinderAxis.Z);

        GameObject cycleStart = CreateCylinder(
            "Cycle_Start_Button_Clickable",
            parent,
            new Vector3(0.93f, 1.18f, 0.945f),
            0.075f,
            0.035f,
            matGreen,
            CylinderAxis.Z
        );

        runtime.cycleStartRenderer = cycleStart.GetComponent<Renderer>();
        runtime.cycleStartButtonTransform = cycleStart.transform;
        AddInteractable(cycleStart, CNCInteractionType.CycleStart);
        CreateTextLabel("Label_CycleStart", parent, "4 启动", CNCUiFont.Positions.CycleStart, CNCUiFont.Sizes.ButtonHint, Color.white);

        CreateCylinder("Emergency_Stop_Guard_Ring", parent, new Vector3(0.84f, 0.87f, 0.942f), 0.155f, 0.012f, matYellow, CylinderAxis.Z);

        GameObject emergency = CreateCylinder(
            "Emergency_Stop_Button_Clickable",
            parent,
            new Vector3(0.84f, 0.87f, 0.95f),
            0.12f,
            0.055f,
            matRed,
            CylinderAxis.Z
        );

        runtime.emergencyStopRenderer = emergency.GetComponent<Renderer>();
        runtime.emergencyStopButtonTransform = emergency.transform;
        AddInteractable(emergency, CNCInteractionType.EmergencyStop);
        CreateTextLabel("Label_EStop", parent, "5 急停", CNCUiFont.Positions.EStop, CNCUiFont.Sizes.ButtonHint, Color.red);

        CreateCylinder("Reset_Button_Pedestal", parent, new Vector3(0.70f, 0.63f, 0.938f), 0.08f, 0.02f, matGray, CylinderAxis.Z);

        GameObject reset = CreateCylinder(
            "Reset_Button_Clickable",
            parent,
            new Vector3(0.70f, 0.63f, 0.945f),
            0.065f,
            0.035f,
            matYellow,
            CylinderAxis.Z
        );

        runtime.resetButtonTransform = reset.transform;
        AddInteractable(reset, CNCInteractionType.Reset);
        CreateTextLabel("Label_Reset", parent, "6 复位", CNCUiFont.Positions.Reset, CNCUiFont.Sizes.ButtonHint, Color.white);

        GameObject modeKnob = CreateCylinder(
            "Mode_Select_Knob_Clickable",
            parent,
            new Vector3(0.94f, 0.63f, 0.945f),
            0.075f,
            0.035f,
            matBlack,
            CylinderAxis.Z
        );

        runtime.modeKnob = modeKnob.transform;
        AddInteractable(modeKnob, CNCInteractionType.ToggleMode);
        CreateTextLabel("Label_Mode", parent, "7 模式", CNCUiFont.Positions.Mode, CNCUiFont.Sizes.ButtonHint, Color.white);
    }

    private void BuildStackLight(Transform parent, CNCTrainingMachineRuntime runtime, Material matGreen, Material matRed, Material matYellow, Material matOff, Material matBlack)
    {
        CreateCylinder(
            "Stack_Light_Pole",
            parent,
            new Vector3(0.82f, 2.18f, -0.25f),
            0.045f,
            0.48f,
            matBlack,
            CylinderAxis.Y
        );

        CreateCylinder(
            "Stack_Light_Housing",
            parent,
            new Vector3(0.82f, 2.42f, -0.25f),
            0.16f,
            0.06f,
            matBlack,
            CylinderAxis.Y
        );

        CreateSphere("Stack_Light_Cap", parent, new Vector3(0.82f, 2.58f, -0.25f), 0.10f, matBlack);

        GameObject red = CreateSphere("Stack_Light_Red", parent, new Vector3(0.82f, 2.50f, -0.25f), 0.12f, matOff);
        GameObject yellow = CreateSphere("Stack_Light_Yellow", parent, new Vector3(0.82f, 2.36f, -0.25f), 0.12f, matOff);
        GameObject green = CreateSphere("Stack_Light_Green", parent, new Vector3(0.82f, 2.22f, -0.25f), 0.12f, matOff);

        runtime.stackRedRenderer = red.GetComponent<Renderer>();
        runtime.stackYellowRenderer = yellow.GetComponent<Renderer>();
        runtime.stackGreenRenderer = green.GetComponent<Renderer>();
    }

    private void BuildLabels(Transform parent)
    {
        CreateTextLabel("Machine_Name_Label", parent, "CNC 实训机床", CNCUiFont.Positions.MachineName, CNCUiFont.Sizes.MachineName, Color.white, bold: true);
        CreateTextLabel("Door_Instruction_Label", parent, "安全门", CNCUiFont.Positions.DoorInstruction, CNCUiFont.Sizes.DoorInstruction, Color.yellow);
        CreateTextLabel(
            "Teaching_Sequence_Label",
            parent,
            "1电源 2安全门 3夹具 4启动 5急停 6复位 7模式",
            CNCUiFont.Positions.TeachingSequence,
            CNCUiFont.Sizes.TeachingSequence,
            new Color(0.55f, 0.92f, 1f),
            bold: true);
    }

    private void AddInteractable(GameObject obj, CNCInteractionType type)
    {
        CNCInteractablePart part = obj.AddComponent<CNCInteractablePart>();
        part.interactionType = type;
        part.station = _runtime;

        // 交互依赖碰撞体：即使 addColliders=false 把装饰碰撞体剥掉，
        // 也必须保证可点击部件自身始终有碰撞体，否则射线/点击全部失效。
        if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>();
        }
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

    private void CreateCylinderBetween(string name, Transform parent, Vector3 start, Vector3 end, float diameter, Material material)
    {
        Vector3 mid = (start + end) * 0.5f;
        Vector3 dir = end - start;
        float length = dir.magnitude;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = mid;
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);
        obj.GetComponent<Renderer>().sharedMaterial = material;

        if (!addColliders)
        {
            DestroySafe(obj.GetComponent<Collider>());
        }
    }

    private void CreateTextLabel(string name, Transform parent, string text, Vector3 localPosition, float size, Color color, bool bold = false)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = CNCUiFont.PanelFaceRotation;

        TextMesh textMesh = obj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = size;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.lineSpacing = 0.82f;
        CNCUiFont.Apply(textMesh, size, bold);
    }

    private Shader ResolveLitShader()
    {
        if (_litShader != null) return _litShader;
        _litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (_litShader == null) _litShader = Shader.Find("Standard");
        if (_litShader == null) _litShader = Shader.Find("Diffuse");
        return _litShader;
    }

    private Material CreateLitMaterial(string name, Color color, float metallic, float smoothness)
    {
        Material mat = new Material(ResolveLitShader());
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

        _runtimeMaterials.Add(mat);
        return mat;
    }

    private Material CreateEmissionMaterial(string name, Color color, float intensity)
    {
        Material mat = CreateLitMaterial(name, color, 0f, 0.55f);

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * intensity);
        }

        return mat;
    }

    private Material CreateTransparentMaterial(string name, Color color)
    {
        Material mat = new Material(ResolveLitShader());
        mat.name = name;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        // URP 透明设置
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        // Built-in Standard 透明设置
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        _runtimeMaterials.Add(mat);
        return mat;
    }

    private void ClearOldModel()
    {
        Transform old = transform.Find(RootName);

        if (old != null)
        {
            DestroySafe(old.gameObject);
        }

        // 回收上一次生成的材质，避免反复 Build 时材质数量无限增长
        for (int i = 0; i < _runtimeMaterials.Count; i++)
        {
            DestroySafe(_runtimeMaterials[i]);
        }
        _runtimeMaterials.Clear();
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


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(CNCTrainingMachineBuilder))]
public class CNCTrainingMachineBuilderEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CNCTrainingMachineBuilder builder = (CNCTrainingMachineBuilder)target;

        GUILayout.Space(12);

        if (GUILayout.Button("Build / Rebuild CNC Training Machine", GUILayout.Height(38)))
        {
            builder.BuildMachine();
        }
    }
}
#endif
