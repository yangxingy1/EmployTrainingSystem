using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CNCTrainingMachineBuilder : MonoBehaviour
{
    public const string StaticMachineName = "CNC Training Machine Static";
    public const string GeneratedModelName = "__Generated_CNC_Training_Machine";

    [Header("Placement")]
    public Vector3 machineWorldPosition = new Vector3(0f, 0f, -6f);

    [Tooltip("机床正面默认朝 +Z，玩家从南侧靠近。")]
    public bool faceSouth = true;

    [Header("Size, meters")]
    public float width = 2.2f;
    public float height = 2.1f;
    public float depth = 1.5f;

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
    private Font _uiFont;

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

        Material matMachineWhite = CreateLitMaterial("MAT_CNC_Machine_White", new Color(0.78f, 0.80f, 0.78f), 0.15f, 0.38f);
        Material matDarkMetal = CreateLitMaterial("MAT_CNC_Dark_Metal", new Color(0.12f, 0.13f, 0.14f), 0.75f, 0.42f);
        Material matLightMetal = CreateLitMaterial("MAT_CNC_Light_Metal", new Color(0.42f, 0.44f, 0.44f), 0.8f, 0.5f);
        Material matBlack = CreateLitMaterial("MAT_CNC_Black_Rubber", new Color(0.02f, 0.02f, 0.02f), 0.1f, 0.25f);
        Material matGlass = CreateTransparentMaterial("MAT_CNC_Blue_Glass", new Color(0.25f, 0.55f, 0.85f, 0.34f));
        Material matScreenOn = CreateEmissionMaterial("MAT_CNC_Screen_ON", new Color(0.05f, 0.8f, 1.0f), 1.5f);
        Material matScreenOff = CreateLitMaterial("MAT_CNC_Screen_OFF", new Color(0.01f, 0.015f, 0.018f), 0f, 0.15f);
        Material matButtonGreen = CreateEmissionMaterial("MAT_Button_Green", new Color(0.05f, 1.0f, 0.25f), 1.7f);
        Material matButtonRed = CreateEmissionMaterial("MAT_Button_Red", new Color(1.0f, 0.04f, 0.02f), 2.0f);
        Material matButtonYellow = CreateEmissionMaterial("MAT_Button_Yellow", new Color(1.0f, 0.72f, 0.04f), 1.4f);
        Material matButtonGray = CreateLitMaterial("MAT_Button_Gray", new Color(0.22f, 0.23f, 0.23f), 0f, 0.25f);
        Material matLedOff = CreateLitMaterial("MAT_LED_Off_Dark", new Color(0.04f, 0.04f, 0.04f), 0f, 0.15f);
        Material matWorkpiece = CreateLitMaterial("MAT_Workpiece_Aluminum", new Color(0.65f, 0.66f, 0.62f), 0.9f, 0.45f);
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

        BuildMachineBody(root.transform, matMachineWhite, matDarkMetal, matGlass, matWarningYellow);
        BuildWorkChamber(root.transform, runtime, matDarkMetal, matLightMetal, matWorkpiece, matBlack);
        BuildControlPanel(root.transform, runtime, matDarkMetal, matScreenOn, matScreenOff, matButtonGreen, matButtonRed, matButtonYellow, matButtonGray, matBlack);
        BuildStackLight(root.transform, runtime, matButtonGreen, matButtonRed, matButtonYellow, matLedOff, matBlack);
        BuildLabels(root.transform);

        runtime.ApplyInitialState();

        Debug.Log("CNC training machine generated. Keyboard sequence: 1 Power -> 2 Door -> 3 Clamp -> 2 Door -> 4 Cycle Start -> 5 Emergency Stop -> 6 Reset.");
    }

    private void BuildMachineBody(Transform parent, Material matWhite, Material matDarkMetal, Material matGlass, Material matWarningYellow)
    {
        // 坐标约定：
        // X = 机床宽度
        // Y = 高度
        // Z = 深度，正面朝 +Z

        // 底座
        CreateCube(
            "CNC_Base_Platform",
            parent,
            new Vector3(0f, 0.15f, 0f),
            new Vector3(width, 0.30f, depth),
            matDarkMetal
        );

        // 主机外壳
        CreateCube(
            "CNC_Main_Enclosure",
            parent,
            new Vector3(0f, 1.05f, -0.05f),
            new Vector3(width, 1.75f, depth),
            matWhite
        );

        // 内部加工腔黑色背景
        CreateCube(
            "CNC_Work_Chamber_Dark_Back",
            parent,
            new Vector3(-0.28f, 1.05f, 0.46f),
            new Vector3(1.22f, 1.05f, 0.08f),
            matDarkMetal
        );

        // 前部开口边框
        CreateCube("CNC_Front_Frame_Top", parent, new Vector3(-0.28f, 1.61f, 0.79f), new Vector3(1.35f, 0.08f, 0.10f), matDarkMetal);
        CreateCube("CNC_Front_Frame_Bottom", parent, new Vector3(-0.28f, 0.48f, 0.79f), new Vector3(1.35f, 0.08f, 0.10f), matDarkMetal);
        CreateCube("CNC_Front_Frame_Left", parent, new Vector3(-0.98f, 1.05f, 0.79f), new Vector3(0.08f, 1.15f, 0.10f), matDarkMetal);
        CreateCube("CNC_Front_Frame_Right", parent, new Vector3(0.42f, 1.05f, 0.79f), new Vector3(0.08f, 1.15f, 0.10f), matDarkMetal);

        // 左右滑动安全门
        GameObject doorLeft = CreateCube(
            "Safety_Door_Left_Glass",
            parent,
            new Vector3(-0.57f, 1.05f, 0.86f),
            new Vector3(0.58f, 0.95f, 0.035f),
            matGlass
        );

        GameObject doorRight = CreateCube(
            "Safety_Door_Right_Glass",
            parent,
            new Vector3(0.02f, 1.05f, 0.865f),
            new Vector3(0.58f, 0.95f, 0.035f),
            matGlass
        );

        runtimeAssignDoor(doorLeft.transform, doorRight.transform);

        // 门把手，作为可点击交互对象
        GameObject handle = CreateCube(
            "Safety_Door_Handle_Clickable",
            doorRight.transform,
            new Vector3(0.27f, 0f, 0.065f),
            new Vector3(0.045f, 0.36f, 0.055f),
            matDarkMetal
        );

        AddInteractable(handle, CNCInteractionType.ToggleDoor);

        // 黄黑安全警示条
        CreateCube("Door_Warning_Yellow_Strip", parent, new Vector3(-0.28f, 0.42f, 0.91f), new Vector3(1.25f, 0.035f, 0.035f), matWarningYellow);
    }

    private void runtimeAssignDoor(Transform doorLeft, Transform doorRight)
    {
        _runtime.leftDoor = doorLeft;
        _runtime.rightDoor = doorRight;
    }

    private void BuildWorkChamber(Transform parent, CNCTrainingMachineRuntime runtime, Material matDarkMetal, Material matLightMetal, Material matWorkpiece, Material matBlack)
    {
        // 工作台
        CreateCube(
            "Machine_Table",
            parent,
            new Vector3(-0.28f, 0.64f, 0.25f),
            new Vector3(0.88f, 0.12f, 0.48f),
            matLightMetal
        );

        // T 型槽
        for (int i = 0; i < 3; i++)
        {
            CreateCube(
                "Machine_Table_TSlot_" + i,
                parent,
                new Vector3(-0.28f, 0.712f, 0.08f + i * 0.16f),
                new Vector3(0.80f, 0.018f, 0.025f),
                matDarkMetal
            );
        }

        // 工件
        GameObject workpiece = CreateCube(
            "Workpiece_Aluminum_Block",
            parent,
            new Vector3(-0.28f, 0.80f, 0.25f),
            new Vector3(0.32f, 0.18f, 0.24f),
            matWorkpiece
        );

        runtime.workpiece = workpiece.transform;

        // 左右夹爪
        GameObject leftJaw = CreateCube(
            "Clamp_Jaw_Left",
            parent,
            new Vector3(-0.53f, 0.82f, 0.25f),
            new Vector3(0.08f, 0.16f, 0.28f),
            matDarkMetal
        );

        GameObject rightJaw = CreateCube(
            "Clamp_Jaw_Right",
            parent,
            new Vector3(-0.03f, 0.82f, 0.25f),
            new Vector3(0.08f, 0.16f, 0.28f),
            matDarkMetal
        );

        runtime.leftClampJaw = leftJaw.transform;
        runtime.rightClampJaw = rightJaw.transform;

        // 夹具手柄
        GameObject clampHandlePivot = new GameObject("Clamp_Handle_Pivot");
        clampHandlePivot.transform.SetParent(parent, false);
        clampHandlePivot.transform.localPosition = new Vector3(0.19f, 0.82f, 0.47f);

        GameObject clampHandle = CreateCube(
            "Clamp_Handle_Clickable",
            clampHandlePivot.transform,
            new Vector3(0f, 0.11f, 0f),
            new Vector3(0.035f, 0.22f, 0.035f),
            matBlack
        );

        runtime.clampHandlePivot = clampHandlePivot.transform;
        AddInteractable(clampHandle, CNCInteractionType.ToggleClamp);

        // 主轴头
        CreateCube(
            "Spindle_Head_Housing",
            parent,
            new Vector3(-0.28f, 1.33f, 0.25f),
            new Vector3(0.32f, 0.26f, 0.26f),
            matLightMetal
        );

        GameObject spindlePivot = new GameObject("Spindle_Rotator");
        spindlePivot.transform.SetParent(parent, false);
        spindlePivot.transform.localPosition = new Vector3(-0.28f, 1.15f, 0.25f);
        runtime.spindleRotator = spindlePivot.transform;

        GameObject spindle = CreateCylinder(
            "Spindle_Tool",
            spindlePivot.transform,
            new Vector3(0f, -0.07f, 0f),
            0.08f,
            0.28f,
            matDarkMetal,
            CylinderAxis.Y
        );

        runtime.tool = spindle.transform;

        // 冷却液软管，用几段圆柱模拟
        CreateCylinderBetween("Coolant_Hose_0", parent, new Vector3(0.05f, 1.35f, 0.12f), new Vector3(0.15f, 1.22f, 0.20f), 0.035f, matBlack);
        CreateCylinderBetween("Coolant_Hose_1", parent, new Vector3(0.15f, 1.22f, 0.20f), new Vector3(0.05f, 1.08f, 0.28f), 0.035f, matBlack);
        CreateCylinder("Coolant_Nozzle", parent, new Vector3(0.04f, 1.06f, 0.29f), 0.045f, 0.12f, matDarkMetal, CylinderAxis.Y);
    }

    private void BuildControlPanel(
        Transform parent,
        CNCTrainingMachineRuntime runtime,
        Material matPanel,
        Material matScreenOn,
        Material matScreenOff,
        Material matGreen,
        Material matRed,
        Material matYellow,
        Material matGray,
        Material matBlack)
    {
        // 右侧控制台
        CreateCube(
            "Control_Panel_Body",
            parent,
            new Vector3(0.82f, 1.08f, 0.83f),
            new Vector3(0.42f, 1.05f, 0.16f),
            matPanel
        );

        // 屏幕
        GameObject screen = CreateCube(
            "CNC_Status_Screen",
            parent,
            new Vector3(0.82f, 1.43f, 0.925f),
            new Vector3(0.32f, 0.22f, 0.025f),
            matScreenOff
        );

        runtime.screenRenderer = screen.GetComponent<Renderer>();

        GameObject screenTextObj = new GameObject("CNC_Status_Text");
        screenTextObj.transform.SetParent(parent, false);
        screenTextObj.transform.localPosition = new Vector3(0.82f, 1.43f, 0.945f);
        screenTextObj.transform.localRotation = Quaternion.identity;

        TextMesh screenText = screenTextObj.AddComponent<TextMesh>();
        screenText.text = "POWER OFF";
        screenText.characterSize = 0.038f;
        screenText.anchor = TextAnchor.MiddleCenter;
        screenText.alignment = TextAlignment.Center;
        screenText.color = Color.cyan;
        ApplyUiFont(screenText);
        runtime.statusText = screenText;

        // 主电源开关
        GameObject powerSwitchPivot = new GameObject("Main_Power_Switch_Pivot");
        powerSwitchPivot.transform.SetParent(parent, false);
        powerSwitchPivot.transform.localPosition = new Vector3(0.70f, 1.18f, 0.945f);

        GameObject powerSwitch = CreateCube(
            "Main_Power_Switch_Clickable",
            powerSwitchPivot.transform,
            new Vector3(0f, 0f, 0.015f),
            new Vector3(0.055f, 0.16f, 0.04f),
            matGray
        );

        runtime.powerSwitchPivot = powerSwitchPivot.transform;
        runtime.powerSwitchRenderer = powerSwitch.GetComponent<Renderer>();
        AddInteractable(powerSwitch, CNCInteractionType.TogglePower);

        CreateTextLabel("Label_MainPower", parent, "1 POWER", new Vector3(0.70f, 1.04f, 0.955f), 0.031f, Color.white);

        // 循环启动按钮
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
        CreateTextLabel("Label_CycleStart", parent, "4 START", new Vector3(0.93f, 1.04f, 0.955f), 0.031f, Color.white);

        // 急停按钮
        GameObject emergency = CreateCylinder(
            "Emergency_Stop_Button_Clickable",
            parent,
            new Vector3(0.82f, 0.87f, 0.95f),
            0.12f,
            0.055f,
            matRed,
            CylinderAxis.Z
        );

        runtime.emergencyStopRenderer = emergency.GetComponent<Renderer>();
        runtime.emergencyStopButtonTransform = emergency.transform;
        AddInteractable(emergency, CNCInteractionType.EmergencyStop);
        CreateTextLabel("Label_EStop", parent, "5 E-STOP", new Vector3(0.82f, 0.74f, 0.955f), 0.034f, Color.red);

        // 复位按钮
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
        CreateTextLabel("Label_Reset", parent, "6 RESET", new Vector3(0.70f, 0.53f, 0.955f), 0.029f, Color.white);

        // 模式旋钮，第一版只建模，暂不强制流程
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
        CreateTextLabel("Label_Mode", parent, "7 MODE", new Vector3(0.94f, 0.53f, 0.955f), 0.029f, Color.white);
    }

    private void BuildStackLight(Transform parent, CNCTrainingMachineRuntime runtime, Material matGreen, Material matRed, Material matYellow, Material matOff, Material matBlack)
    {
        CreateCylinder(
            "Stack_Light_Pole",
            parent,
            new Vector3(0.80f, 2.20f, -0.25f),
            0.035f,
            0.42f,
            matBlack,
            CylinderAxis.Y
        );

        GameObject red = CreateSphere("Stack_Light_Red", parent, new Vector3(0.80f, 2.48f, -0.25f), 0.13f, matOff);
        GameObject yellow = CreateSphere("Stack_Light_Yellow", parent, new Vector3(0.80f, 2.34f, -0.25f), 0.13f, matOff);
        GameObject green = CreateSphere("Stack_Light_Green", parent, new Vector3(0.80f, 2.20f, -0.25f), 0.13f, matOff);

        runtime.stackRedRenderer = red.GetComponent<Renderer>();
        runtime.stackYellowRenderer = yellow.GetComponent<Renderer>();
        runtime.stackGreenRenderer = green.GetComponent<Renderer>();
    }

    private void BuildLabels(Transform parent)
    {
        CreateTextLabel("Machine_Name_Label", parent, "CNC TRAINING MACHINE", new Vector3(-0.28f, 1.88f, 0.84f), 0.075f, Color.white);
        CreateTextLabel("Door_Instruction_Label", parent, "SAFETY DOOR", new Vector3(-0.28f, 0.32f, 0.93f), 0.045f, Color.yellow);
        CreateTextLabel("Teaching_Sequence_Label", parent, "1 POWER  2 DOOR  3 CLAMP\n4 START  5 E-STOP  6 RESET  7 MODE", new Vector3(-0.28f, 0.22f, 0.93f), 0.030f, Color.cyan);
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

    private void ApplyUiFont(TextMesh textMesh)
    {
        Font font = ResolveUiFont();
        if (font == null) return;
        textMesh.font = font;
        Renderer r = textMesh.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = font.material;
    }

    private Font ResolveUiFont()
    {
        if (_uiFont != null) return _uiFont;
        try { _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (_uiFont == null) { try { _uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }
        return _uiFont;
    }

    private void CreateTextLabel(string name, Transform parent, string text, Vector3 localPosition, float size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = obj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = size;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        ApplyUiFont(textMesh);
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

    private void Awake()
    {
        if (station == null)
        {
            station = GetComponentInParent<CNCTrainingMachineRuntime>();
        }
    }

    private void OnMouseDown()
    {
        if (station == null)
        {
            station = GetComponentInParent<CNCTrainingMachineRuntime>();
        }

        if (station != null)
        {
            station.HandleInteraction(interactionType);
        }
    }
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

    private void Start()
    {
        if (leftDoor != null) leftDoorClosedPos = leftDoor.localPosition;
        if (rightDoor != null) rightDoorClosedPos = rightDoor.localPosition;
        if (leftClampJaw != null) leftClampOpenPos = leftClampJaw.localPosition;
        if (rightClampJaw != null) rightClampOpenPos = rightClampJaw.localPosition;
        if (cycleStartButtonTransform != null) cycleStartButtonReadyPos = cycleStartButtonTransform.localPosition;
        if (emergencyStopButtonTransform != null) emergencyStopButtonReadyPos = emergencyStopButtonTransform.localPosition;
        if (resetButtonTransform != null) resetButtonReadyPos = resetButtonTransform.localPosition;

        MarkRuntimePartsDynamic();
        ApplyInitialState();
        Debug.Log("[CNC] Keyboard input ready: 1 Power, 2 Door, 3 Clamp, 4 Start, 5 Emergency Stop, 6 Reset, 7 Mode.");
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
        UpdateStatus("POWER OFF");
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
        return Input.GetKeyDown(primaryKey) || Input.GetKeyDown(keypadKey);
    }

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
            UpdateStatus("RESET REQUIRED");
            return;
        }

        powerOn = !powerOn;

        if (!powerOn)
        {
            running = false;
            UpdateStatus("POWER OFF");
        }
        else
        {
            UpdateStatus("POWER ON: OPEN DOOR AND CLAMP");
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
            UpdateStatus("DOOR LOCKED DURING RUN");
            return;
        }

        doorClosed = !doorClosed;

        if (doorCoroutine != null)
        {
            StopCoroutine(doorCoroutine);
        }

        doorCoroutine = StartCoroutine(AnimateDoor(doorClosed));

        UpdateStatus(doorClosed ? "DOOR CLOSED" : "DOOR OPEN: CLAMP WORKPIECE");
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
            UpdateStatus("CANNOT CLAMP WHILE RUNNING");
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

        UpdateStatus(workpieceClamped ? "WORKPIECE CLAMPED: CLOSE DOOR" : "WORKPIECE UNCLAMPED");
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
            UpdateStatus("TURN POWER ON FIRST");
            return;
        }

        if (emergencyStopped)
        {
            UpdateStatus("RESET REQUIRED");
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
            UpdateStatus("CLAMP WORKPIECE FIRST");
            return;
        }

        running = true;
        UpdateStatus("MACHINING RUNNING");
        UpdateVisuals();
    }

    private void EmergencyStop()
    {
        PushButton(emergencyStopButtonTransform, emergencyStopButtonReadyPos, ref emergencyStopButtonCoroutine);
        running = false;
        emergencyStopped = true;
        UpdateStatus("EMERGENCY STOPPED");
        UpdateVisuals();
    }

    private void ResetMachine()
    {
        PushButton(resetButtonTransform, resetButtonReadyPos, ref resetButtonCoroutine);
        emergencyStopped = false;
        running = false;

        if (powerOn)
            UpdateStatus("RESET COMPLETE: READY");
        else
            UpdateStatus("RESET COMPLETE: POWER OFF");

        UpdateVisuals();
    }

    private void ToggleMode()
    {
        autoMode = !autoMode;

        if (modeKnob != null)
        {
            modeKnob.localRotation = autoMode ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f);
        }

        UpdateStatus(autoMode ? "MODE: AUTO" : "MODE: MANUAL");
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
        }

        Debug.Log("[CNC] " + message);
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
