using System.Collections.Generic;
using UnityEngine;

public class ElectricalControlCabinetBuilder : MonoBehaviour
{
    public const string StaticCabinetName = "Electrical Control Cabinet Static";
    public const string GeneratedModelName = "__Generated_Electrical_Cabinet_Model";
    public const float SwitchOnAngle = -55f;
    public const float SwitchOffAngle = 55f;

    [Header("Cabinet Placement")]
    public Vector3 cabinetWorldPosition = new Vector3(10f, 0f, -10f);

    [Tooltip("柜门面板默认朝向世界 -X，也就是西侧。")]
    public bool faceWest = true;

    [Header("Cabinet Size, meters")]
    public float width = 0.8f;   // 沿 Z 方向
    public float height = 2.0f;  // 沿 Y 方向
    public float depth = 0.4f;   // 沿 X 方向

    [Header("Control State")]
    public bool breakerOn = true;

    [Header("Build Options")]
    public bool buildOnStart = false;
    public bool addColliders = true;
    public bool addEInteraction = true;

    private readonly List<Material> generatedMaterials = new List<Material>();

    private const string RootName = GeneratedModelName;

    private void Start()
    {
        if (buildOnStart)
        {
            BuildCabinet();
        }
    }

    [ContextMenu("Build / Rebuild Cabinet")]
    public void BuildCabinet()
    {
        ClearOldModel();

        transform.position = cabinetWorldPosition;
        transform.rotation = faceWest ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
        transform.localScale = Vector3.one;

        Material matCabinet = CreateLitMaterial("MAT_Cabinet_OffWhite", new Color(0.78f, 0.80f, 0.78f), 0.15f, 0.35f);
        Material matDoor = CreateLitMaterial("MAT_Door_Panel_LightGray", new Color(0.62f, 0.65f, 0.64f), 0.2f, 0.38f);
        Material matDarkPanel = CreateLitMaterial("MAT_Dark_Control_Panel", new Color(0.08f, 0.09f, 0.10f), 0.4f, 0.55f);
        Material matRed = CreateLitMaterial("MAT_Red_Main_Switch", new Color(0.85f, 0.05f, 0.03f), 0.1f, 0.45f);
        Material matGreenLED = CreateEmissionMaterial("MAT_Green_LED_On", new Color(0.05f, 1f, 0.25f), 1.8f);
        Material matLEDOff = CreateLitMaterial("MAT_LED_Off", new Color(0.05f, 0.18f, 0.08f), 0f, 0.2f);
        Material matBlack = CreateLitMaterial("MAT_Black_Handle", new Color(0.02f, 0.02f, 0.02f), 0.2f, 0.35f);
        Material matMetal = CreateLitMaterial("MAT_Dark_Metal", new Color(0.28f, 0.30f, 0.32f), 0.8f, 0.45f);
        Material matYellow = CreateLitMaterial("MAT_Warning_Yellow", new Color(1.0f, 0.75f, 0.05f), 0f, 0.35f);
        Material matWhite = CreateLitMaterial("MAT_Label_White", new Color(0.9f, 0.9f, 0.85f), 0f, 0.3f);

        GameObject root = new GameObject(RootName);
        root.transform.SetParent(transform, false);

        // 约定：
        // X = 深度方向，柜门在 -X 面，朝西
        // Y = 高度方向
        // Z = 宽度方向

        float frontX = -depth * 0.5f;
        float backX = depth * 0.5f;

        // 主柜体
        GameObject body = CreateCube(
            "Cabinet_Body",
            root.transform,
            new Vector3(0f, height * 0.5f, 0f),
            new Vector3(depth, height, width),
            matCabinet
        );

        // 前门面板，比柜体稍微突出一点
        GameObject door = CreateCube(
            "Front_Door_Panel",
            root.transform,
            new Vector3(frontX - 0.012f, height * 0.5f, 0f),
            new Vector3(0.024f, height * 0.92f, width * 0.88f),
            matDoor
        );

        // 门缝线，模拟左右门边
        CreateCube(
            "Door_Center_Seam",
            root.transform,
            new Vector3(frontX - 0.026f, height * 0.5f, 0f),
            new Vector3(0.01f, height * 0.86f, 0.012f),
            matBlack
        );

        // 顶部控制面板区域
        CreateCube(
            "Top_Control_Panel",
            root.transform,
            new Vector3(frontX - 0.035f, 1.55f, 0f),
            new Vector3(0.025f, 0.42f, 0.62f),
            matDarkPanel
        );

        // 红色主开关底座
        GameObject switchBase = CreateCylinder(
            "Main_Breaker_Red_Dial_Base",
            root.transform,
            new Vector3(frontX - 0.06f, 1.48f, -0.12f),
            0.16f,
            0.04f,
            matRed,
            CylinderAxis.X
        );

        // 主开关旋钮手柄，点击这个对象可以切换 ON/OFF
        GameObject switchHandle = CreateCube(
            "Main_Breaker_Handle_Clickable",
            root.transform,
            new Vector3(frontX - 0.095f, 1.48f, -0.12f),
            new Vector3(0.035f, 0.055f, 0.22f),
            matBlack
        );

        float handleAngle = breakerOn ? SwitchOnAngle : SwitchOffAngle;
        switchHandle.transform.localRotation = Quaternion.Euler(handleAngle, 0f, 0f);

        // 给开关加 BoxCollider，保证可以点击
        BoxCollider handleCollider = switchHandle.GetComponent<BoxCollider>();
        if (handleCollider == null)
        {
            handleCollider = switchHandle.AddComponent<BoxCollider>();
        }

        // LED 指示灯
        GameObject led = CreateSphere(
            "Power_LED_Green",
            root.transform,
            new Vector3(frontX - 0.065f, 1.68f, 0.18f),
            0.055f,
            breakerOn ? matGreenLED : matLEDOff
        );

        // LED 外圈
        CreateCylinder(
            "Power_LED_Metal_Ring",
            root.transform,
            new Vector3(frontX - 0.058f, 1.68f, 0.18f),
            0.075f,
            0.018f,
            matMetal,
            CylinderAxis.X
        );

        // ON / OFF 文本标签
        CreateTextLabel("Label_ON", root.transform, "ON", new Vector3(frontX - 0.08f, 1.34f, -0.25f), 0.07f, matWhite);
        CreateTextLabel("Label_OFF", root.transform, "OFF", new Vector3(frontX - 0.08f, 1.34f, 0.02f), 0.07f, matWhite);
        CreateTextLabel("Label_Main", root.transform, "MAIN BREAKER", new Vector3(frontX - 0.08f, 1.78f, -0.10f), 0.055f, matWhite);
        CreateTextLabel("Label_Power", root.transform, "POWER", new Vector3(frontX - 0.08f, 1.78f, 0.18f), 0.055f, matWhite);

        // 门把手
        CreateCube(
            "Door_Handle",
            root.transform,
            new Vector3(frontX - 0.07f, 1.00f, 0.31f),
            new Vector3(0.05f, 0.38f, 0.045f),
            matBlack
        );

        // 铰链，放在门另一侧
        CreateCylinder(
            "Upper_Hinge",
            root.transform,
            new Vector3(frontX - 0.045f, 1.55f, -0.37f),
            0.045f,
            0.18f,
            matMetal,
            CylinderAxis.Y
        );

        CreateCylinder(
            "Middle_Hinge",
            root.transform,
            new Vector3(frontX - 0.045f, 1.00f, -0.37f),
            0.045f,
            0.18f,
            matMetal,
            CylinderAxis.Y
        );

        CreateCylinder(
            "Lower_Hinge",
            root.transform,
            new Vector3(frontX - 0.045f, 0.45f, -0.37f),
            0.045f,
            0.18f,
            matMetal,
            CylinderAxis.Y
        );

        // 黄色警示铭牌
        CreateCube(
            "Warning_Nameplate",
            root.transform,
            new Vector3(frontX - 0.045f, 0.32f, 0f),
            new Vector3(0.018f, 0.16f, 0.44f),
            matYellow
        );

        CreateTextLabel("Warning_Text", root.transform, "ELECTRICAL", new Vector3(frontX - 0.07f, 0.345f, -0.14f), 0.045f, matBlack);
        CreateTextLabel("Warning_Text_2", root.transform, "CONTROL", new Vector3(frontX - 0.07f, 0.285f, -0.12f), 0.045f, matBlack);

        // 顶部小通风槽
        for (int i = 0; i < 5; i++)
        {
            float z = -0.24f + i * 0.12f;
            CreateCube(
                "Vent_Slot_" + i,
                root.transform,
                new Vector3(frontX - 0.05f, 1.88f, z),
                new Vector3(0.012f, 0.035f, 0.08f),
                matBlack
            );
        }

        // 底部支脚
        CreateCube("Foot_FL", root.transform, new Vector3(frontX + 0.05f, 0.045f, -width * 0.35f), new Vector3(0.12f, 0.09f, 0.10f), matMetal);
        CreateCube("Foot_FR", root.transform, new Vector3(frontX + 0.05f, 0.045f, width * 0.35f), new Vector3(0.12f, 0.09f, 0.10f), matMetal);
        CreateCube("Foot_BL", root.transform, new Vector3(backX - 0.05f, 0.045f, -width * 0.35f), new Vector3(0.12f, 0.09f, 0.10f), matMetal);
        CreateCube("Foot_BR", root.transform, new Vector3(backX - 0.05f, 0.045f, width * 0.35f), new Vector3(0.12f, 0.09f, 0.10f), matMetal);

        // 顶部线缆接头
        CreateCylinder(
            "Cable_Conduit_Top",
            root.transform,
            new Vector3(0f, height + 0.09f, 0.22f),
            0.055f,
            0.28f,
            matMetal,
            CylinderAxis.Y
        );

        // 给开关添加交互脚本
        MainBreakerToggle toggle = switchHandle.AddComponent<MainBreakerToggle>();
        toggle.handle = switchHandle.transform;
        toggle.ledRenderer = led.GetComponent<Renderer>();
        toggle.onMaterial = matGreenLED;
        toggle.offMaterial = matLEDOff;
        toggle.isOn = breakerOn;
        toggle.onAngle = SwitchOnAngle;
        toggle.offAngle = SwitchOffAngle;

        if (addEInteraction)
        {
            ConfigureEInteraction(switchHandle.transform, led.GetComponent<Renderer>(), breakerOn);
        }

        Debug.Log("Electrical control cabinet generated at " + cabinetWorldPosition);
    }

    public void ConfigureEInteraction(Transform switchHandle, Renderer ledRenderer, bool initialState)
    {
        ElectricalCabinetEInteraction interaction = GetComponent<ElectricalCabinetEInteraction>();
        if (interaction == null)
        {
            interaction = gameObject.AddComponent<ElectricalCabinetEInteraction>();
        }

        interaction.switchHandle = switchHandle;
        interaction.ledRenderer = ledRenderer;
        interaction.requireInteractionDistance = false;
        interaction.interactKey = KeyCode.G;
        interaction.isOn = initialState;
        interaction.useAbsoluteAngles = true;
        interaction.onAngle = SwitchOnAngle;
        interaction.offAngle = SwitchOffAngle;
        interaction.localRotationAxis = Vector3.right;
        interaction.rotateDuration = 0.6f;
    }

    private GameObject CreateCube(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = size;

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        if (!addColliders)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null) DestroySafe(col);
        }

        return obj;
    }

    private enum CylinderAxis
    {
        X,
        Y,
        Z
    }

    private GameObject CreateCylinder(
        string name,
        Transform parent,
        Vector3 localPosition,
        float diameter,
        float length,
        Material material,
        CylinderAxis axis
    )
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;

        // Unity 默认 Cylinder：高度沿 Y 轴，半径约 0.5，高度约 2。
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);

        if (axis == CylinderAxis.X)
            obj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        else if (axis == CylinderAxis.Z)
            obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        else
            obj.transform.localRotation = Quaternion.identity;

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        if (!addColliders)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null) DestroySafe(col);
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

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        if (!addColliders)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null) DestroySafe(col);
        }

        return obj;
    }

    private void CreateTextLabel(string name, Transform parent, string text, Vector3 localPosition, float size, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;

        // 让文字贴在朝西的柜门上。
        // 如果你发现文字反了，把 90 改成 -90。
        obj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        TextMesh textMesh = obj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = size;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
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

        generatedMaterials.Add(mat);
        return mat;
    }

    private Material CreateEmissionMaterial(string name, Color color, float intensity)
    {
        Material mat = CreateLitMaterial(name, color, 0f, 0.65f);

        Color emissionColor = color * intensity;

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor);
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

public class MainBreakerToggle : MonoBehaviour
{
    public bool enableMouseClick = false;

    public Transform handle;
    public Renderer ledRenderer;

    public Material onMaterial;
    public Material offMaterial;

    public bool isOn = true;

    public float onAngle = -35f;
    public float offAngle = 35f;

    private void OnMouseDown()
    {
        if (!enableMouseClick)
        {
            return;
        }

        Toggle();
    }

    public void Toggle()
    {
        isOn = !isOn;

        if (handle != null)
        {
            float angle = isOn ? onAngle : offAngle;
            handle.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        if (ledRenderer != null)
        {
            ledRenderer.sharedMaterial = isOn ? onMaterial : offMaterial;
        }

        Debug.Log(isOn ? "Main breaker switched ON." : "Main breaker switched OFF.");
    }
}
