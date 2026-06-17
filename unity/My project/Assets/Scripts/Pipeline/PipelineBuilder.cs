using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管道模型构建器 —— 程序化生成化工管道元件。
/// 所有管道统一管径等参数，使用 Unity Primitive + 少量自定义 Mesh 拼装。
/// 用法：挂载到 GameObject 上，配置参数后在 Editor 中调用 Build()，或通过代码调用各 Create 方法。
/// </summary>
public class PipelineBuilder : MonoBehaviour
{
    // ── 统一尺寸常量 ───────────────────────────────────────────
    public const float SCALE_FACTOR        = 6f;      // 整体缩放倍率（修改此值统一缩放整个模型）
    public const float PIPE_DIAMETER      = 0.30f * SCALE_FACTOR;   // 管道外径
    public const float PIPE_RADIUS        = 0.15f * SCALE_FACTOR;   // 管道半径
    public const float FLANGE_DIAMETER    = 0.50f * SCALE_FACTOR;   // 法兰外径
    public const float FLANGE_THICKNESS   = 0.08f * SCALE_FACTOR;   // 法兰厚度
    public const float BEND_RADIUS        = 0.40f * SCALE_FACTOR;   // 弯头中心线弯曲半径
    public const float WALL_THICKNESS     = 0.02f * SCALE_FACTOR;   // 壁厚（视觉用）
    public const float VALVE_BODY_WIDTH   = 0.55f * SCALE_FACTOR;   // 阀门主体宽度
    public const float VALVE_BODY_HEIGHT  = 0.80f * SCALE_FACTOR;   // 阀门主体高度
    public const float HANDWHEEL_DIAMETER = 0.70f * SCALE_FACTOR;   // 手轮直径
    public const float HANDWHEEL_THICK    = 0.06f * SCALE_FACTOR;   // 手轮厚度
    public const float GAUGE_DIAMETER     = 0.25f * SCALE_FACTOR;   // 压力表表盘直径
    public const float GAUGE_THICKNESS    = 0.06f * SCALE_FACTOR;   // 压力表厚度
    public const float FLOWMETER_WIDTH    = 0.35f * SCALE_FACTOR;   // 流量计本体宽度
    public const float FLOWMETER_HEIGHT   = 0.50f * SCALE_FACTOR;   // 流量计本体高度
    public const float SUPPORT_BASE_SIZE  = 0.40f * SCALE_FACTOR;   // 支架底板尺寸
    public const float SUPPORT_POLE_DIA   = 0.10f * SCALE_FACTOR;   // 支架立柱直径
    public const float ESTOP_BODY_DIAMETER = 0.40f * SCALE_FACTOR;  // 急停按钮箱体宽度
    public const int   ELBOW_SEGMENTS     = 12;      // 弯头分段数
    public const int   FLANGE_BOLTS       = 8;       // 法兰螺栓数量

    // ── 细节零件尺寸（受 SCALE_FACTOR 影响）─────────────────────
    public const float BOLT_DIAMETER     = 0.05f * SCALE_FACTOR;   // 法兰螺栓直径
    public const float NUT_SIZE          = 0.065f * SCALE_FACTOR;  // 螺母边长
    public const float NUT_THICKNESS     = 0.04f * SCALE_FACTOR;   // 螺母厚度
    public const float STEM_DIAMETER     = 0.08f * SCALE_FACTOR;   // 阀杆直径
    public const float STEM_LENGTH       = 0.25f * SCALE_FACTOR;   // 阀杆长度
    public const float STEM_WHEEL_GAP    = 0.20f * SCALE_FACTOR;   // 阀杆顶到手轮的距离
    public const float LAMP_DIAMETER     = 0.15f * SCALE_FACTOR;   // 状态指示灯直径
    public const float BTN_BASE_DIAMETER = 0.18f * SCALE_FACTOR;   // 按钮底座直径
    public const float BTN_BASE_THICK    = 0.04f * SCALE_FACTOR;   // 按钮底座厚度
    public const float BTN_DIAMETER      = 0.14f * SCALE_FACTOR;   // 按钮直径
    public const float BTN_THICK         = 0.05f * SCALE_FACTOR;   // 按钮厚度
    public const float ESTOP_PANEL_W     = 0.55f * SCALE_FACTOR;   // 急停面板宽度
    public const float ESTOP_PANEL_H     = 0.70f * SCALE_FACTOR;   // 急停面板高度
    public const float ESTOP_PANEL_D     = 0.12f * SCALE_FACTOR;   // 急停面板深度
    public const float DESK_WIDTH        = 1.0f * SCALE_FACTOR;   // 记录台宽度
    public const float DESK_DEPTH        = 0.6f * SCALE_FACTOR;   // 记录台深度
    public const float DESK_THICKNESS    = 0.06f * SCALE_FACTOR;   // 记录台厚度
    public const float DESK_LEG_DIA      = 0.05f * SCALE_FACTOR;   // 桌腿直径
    public const float DESK_HEIGHT       = 0.5f * SCALE_FACTOR;   // 桌腿/台面高度
    public const float INSPECT_POLE_DIA  = 0.04f * SCALE_FACTOR;   // 巡检柱直径
    public const float INSPECT_POLE_H    = 0.7f * SCALE_FACTOR;   // 巡检柱高度
    public const float INSPECT_BULB_DIA  = 0.12f * SCALE_FACTOR;   // 巡检灯泡直径
    public const float INSPECT_MARKER_DIA = 0.6f * SCALE_FACTOR;   // 巡检地面标记直径
    public const float PPE_PANEL_W       = 1.2f * SCALE_FACTOR;   // PPE面板宽度
    public const float PPE_PANEL_H       = 1.4f * SCALE_FACTOR;   // PPE面板高度
    public const float PPE_PANEL_D       = 0.08f * SCALE_FACTOR;   // PPE面板深度
    public const float PPE_POLE_DIA      = 0.08f * SCALE_FACTOR;   // PPE立柱直径
    public const float PPE_POLE_H        = 0.5f * SCALE_FACTOR;   // PPE立柱高度

    // ── 材质颜色 ───────────────────────────────────────────────
    public static readonly Color PipeYellow     = new Color(0.94f, 0.62f, 0.05f);
    public static readonly Color PipeSilver     = new Color(0.58f, 0.60f, 0.62f);
    public static readonly Color MetalDark      = new Color(0.22f, 0.24f, 0.27f);
    public static readonly Color MetalMid       = new Color(0.38f, 0.40f, 0.41f);
    public static readonly Color MetalLight     = new Color(0.58f, 0.60f, 0.62f);
    public static readonly Color WheelRed       = new Color(0.84f, 0.08f, 0.05f);
    public static readonly Color BoltDark       = new Color(0.12f, 0.13f, 0.14f);
    public static readonly Color GaugeFace      = new Color(0.94f, 0.95f, 0.96f);
    public static readonly Color GaugeGreenZone = new Color(0.18f, 0.82f, 0.30f);
    public static readonly Color GaugeRedZone   = new Color(0.90f, 0.15f, 0.10f);
    public static readonly Color EStopRed       = new Color(1.0f, 0.04f, 0.02f);
    public static readonly Color ButtonYellow   = new Color(1.0f, 0.72f, 0.04f);
    public static readonly Color LampGreen      = new Color(0.20f, 0.85f, 0.32f);
    public static readonly Color LampDim        = new Color(0.08f, 0.09f, 0.10f);
    public static readonly Color PanelWhite     = new Color(0.88f, 0.90f, 0.91f);
    public static readonly Color Black          = new Color(0.02f, 0.02f, 0.02f);

    // ── 配置 ───────────────────────────────────────────────────
    [Header("Build Options")]
    public bool buildOnStart = false;
    public bool addColliders = true;
    public bool faceSouth = true;          // 整个场景朝向

    private readonly List<Material> _runtimeMaterials = new List<Material>();
    private Shader _litShader;

    // 生成的根节点
    public GameObject GeneratedRoot { get; private set; }

    private void Start()
    {
        if (buildOnStart)
            Build();
    }

    // ── 材质辅助 ───────────────────────────────────────────────

    protected Shader ResolveLitShader()
    {
        if (_litShader != null) return _litShader;
        _litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (_litShader == null) _litShader = Shader.Find("Standard");
        if (_litShader == null) _litShader = Shader.Find("Diffuse");
        return _litShader;
    }

    protected Material CreateMaterial(string name, Color color, float metallic = 0.3f, float smoothness = 0.5f)
    {
        Material mat = new Material(ResolveLitShader());
        mat.name = name;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        _runtimeMaterials.Add(mat);
        return mat;
    }

    protected Material CreateEmissionMaterial(string name, Color color, float intensity = 1.5f)
    {
        Material mat = CreateMaterial(name, color, 0f, 0.55f);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * intensity);
        }
        return mat;
    }

    // ── 基础几何辅助 ───────────────────────────────────────────

    public enum Axis { X, Y, Z }

    /// <summary>创建 Cube，指定世界尺寸（非缩放）</summary>
    protected GameObject CreateCube(string objName, Transform parent, Vector3 localPos, Vector3 size, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = objName;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = size;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        if (!addColliders) DestroySafe(obj.GetComponent<Collider>());
        return obj;
    }

    /// <summary>创建 Cylinder，指定直径和长度。axis 为圆柱体的长轴方向。</summary>
    protected GameObject CreateCylinder(string objName, Transform parent, Vector3 localPos, float diameter, float length, Material mat, Axis axis = Axis.Y)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = objName;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);

        switch (axis)
        {
            case Axis.X: obj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); break;
            case Axis.Z: obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); break;
            default:     obj.transform.localRotation = Quaternion.identity; break;
        }

        obj.GetComponent<Renderer>().sharedMaterial = mat;
        if (!addColliders) DestroySafe(obj.GetComponent<Collider>());
        return obj;
    }

    /// <summary>创建两端点之间的圆柱</summary>
    protected GameObject CreateCylinderBetween(string objName, Transform parent, Vector3 start, Vector3 end, float diameter, Material mat)
    {
        Vector3 mid = (start + end) * 0.5f;
        Vector3 dir = end - start;
        float length = dir.magnitude;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = objName;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = mid;
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        if (!addColliders) DestroySafe(obj.GetComponent<Collider>());
        return obj;
    }

    /// <summary>创建球体</summary>
    protected GameObject CreateSphere(string objName, Transform parent, Vector3 localPos, float diameter, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = objName;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one * diameter;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        if (!addColliders) DestroySafe(obj.GetComponent<Collider>());
        return obj;
    }

    /// <summary>设置 GameObject 及其所有子 Renderer 的颜色</summary>
    public static void SetColor(GameObject go, Color color)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            var mat = r.sharedMaterial;
            if (mat == null) continue;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        }
    }

    protected void DestroySafe(Object obj)
    {
        if (obj == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying) DestroyImmediate(obj);
        else Destroy(obj);
#else
        Destroy(obj);
#endif
    }

    // ═══════════════════════════════════════════════════════════
    //  公开 API —— 管道元件创建方法
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 创建直管段。
    /// 直管沿 local Y 轴（垂直）或 local X 轴（水平）取决于 isVertical。
    /// 默认水平：沿 X 轴，从 localPos 向 +X 延伸 length。
    /// </summary>
    public GameObject CreateStraightPipe(string objName, Transform parent, Vector3 localPos, float length,
        bool isVertical = false, Material overrideMat = null)
    {
        Material mat = overrideMat ?? CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        Axis axis = isVertical ? Axis.Y : Axis.X;
        return CreateCylinder(objName, parent, localPos, PIPE_DIAMETER, length, mat, axis);
    }

    /// <summary>
    /// 创建 90° 弯头（直角弯头）。
    /// 入口方向为 +X，出口方向为 +Y（向上弯）或 +Z（水平弯）。
    /// bendPlane: "XZ" 为水平面弯头，"XY" 为垂直面弯头。
    /// </summary>
    public GameObject CreateElbow90(string objName, Transform parent, Vector3 localPos,
        string bendPlane = "XZ", Material overrideMat = null)
    {
        Material mat = overrideMat ?? CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        GameObject elbowRoot = new GameObject(objName);
        elbowRoot.transform.SetParent(parent, false);
        elbowRoot.transform.localPosition = localPos;
        elbowRoot.transform.localRotation = Quaternion.identity;

        // 弯头弧线：以 (BEND_RADIUS, 0, 0) 为起点（入口，方向 +X），
        // 终点取决于弯曲平面
        float totalAngle = 90f;
        float segAngle = totalAngle / ELBOW_SEGMENTS;
        float segArcLen = BEND_RADIUS * Mathf.Deg2Rad * segAngle;

        for (int i = 0; i < ELBOW_SEGMENTS; i++)
        {
            float a0 = i * segAngle * Mathf.Deg2Rad;
            float a1 = (i + 1) * segAngle * Mathf.Deg2Rad;
            float aMid = (a0 + a1) * 0.5f;

            Vector3 segPos;
            Vector3 tangent;

            if (bendPlane == "XZ")
            {
                // 水平弯头：从 +X 方向弯到 +Z 方向
                // entry at (R, 0, 0), exit at (0, 0, R)
                segPos = new Vector3(BEND_RADIUS * Mathf.Cos(aMid), 0f, BEND_RADIUS * Mathf.Sin(aMid));
                tangent = new Vector3(-Mathf.Sin(aMid), 0f, Mathf.Cos(aMid));
            }
            else // "XY"
            {
                // 垂直弯头：从 +X 方向弯到 +Y 方向
                // entry at (R, 0, 0), exit at (0, R, 0)
                segPos = new Vector3(BEND_RADIUS * Mathf.Cos(aMid), BEND_RADIUS * Mathf.Sin(aMid), 0f);
                tangent = new Vector3(-Mathf.Sin(aMid), Mathf.Cos(aMid), 0f);
            }

            CreateCylinder(objName + "_Seg" + i, elbowRoot.transform,
                segPos, PIPE_DIAMETER, segArcLen * 1.05f, mat,
                Axis.Y); // Y 轴对齐切线

            // 手动旋转以对齐切线
            Transform seg = elbowRoot.transform.GetChild(elbowRoot.transform.childCount - 1);
            seg.localRotation = Quaternion.FromToRotation(Vector3.up, tangent.normalized);
        }

        return elbowRoot;
    }

    /// <summary>
    /// 创建 T 形三通。
    /// 主管沿 X 轴，分支管沿 Y 轴（向上）或 Z 轴。
    /// </summary>
    public GameObject CreateTeeJunction(string objName, Transform parent, Vector3 localPos,
        bool branchUp = true, Material overrideMat = null)
    {
        Material mat = overrideMat ?? CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        float halfJunction = PIPE_DIAMETER * 1.2f;

        // 主管（X 方向穿过）
        CreateCylinder(objName + "_Main", root.transform, Vector3.zero,
            PIPE_DIAMETER, halfJunction * 2f, mat, Axis.X);

        // 分支管
        if (branchUp)
        {
            CreateCylinder(objName + "_Branch", root.transform,
                new Vector3(0f, halfJunction * 0.7f, 0f),
                PIPE_DIAMETER, halfJunction, mat, Axis.Y);
        }
        else
        {
            CreateCylinder(objName + "_Branch", root.transform,
                new Vector3(0f, 0f, halfJunction * 0.7f),
                PIPE_DIAMETER, halfJunction, mat, Axis.Z);
        }

        // 过渡块
        CreateCube(objName + "_Block", root.transform, Vector3.zero,
            new Vector3(halfJunction * 2f, PIPE_DIAMETER * 0.95f, PIPE_DIAMETER * 0.95f),
            mat);

        return root;
    }

    /// <summary>
    /// 创建十字形四通。
    /// 主管沿 X 轴，两个分支分别沿 ±Y（或 ±Z）。
    /// </summary>
    public GameObject CreateCrossJunction(string objName, Transform parent, Vector3 localPos,
        Material overrideMat = null)
    {
        Material mat = overrideMat ?? CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        float halfJunction = PIPE_DIAMETER * 1.3f;

        // 主管（X 方向）
        CreateCylinder(objName + "_MainX", root.transform, Vector3.zero,
            PIPE_DIAMETER, halfJunction * 2f, mat, Axis.X);

        // 上分支（+Y）
        CreateCylinder(objName + "_BranchUp", root.transform,
            new Vector3(0f, halfJunction * 0.7f, 0f),
            PIPE_DIAMETER, halfJunction, mat, Axis.Y);

        // 下分支（-Y）
        CreateCylinder(objName + "_BranchDown", root.transform,
            new Vector3(0f, -halfJunction * 0.7f, 0f),
            PIPE_DIAMETER, halfJunction, mat, Axis.Y);

        // 过渡块
        CreateCube(objName + "_Block", root.transform, Vector3.zero,
            new Vector3(halfJunction * 2f, PIPE_DIAMETER * 2f, PIPE_DIAMETER * 0.95f),
            mat);

        return root;
    }

    /// <summary>
    /// 创建 U 形管（倒 U 形）。
    /// 两个垂直支腿 + 顶部水平段 + 两个弯头。
    /// 底部两端在同一水平面上（X 方向跨度 width，Y 方向高度）。
    /// </summary>
    public GameObject CreateUPipe(string objName, Transform parent, Vector3 localPos,
        float width, float legHeight, Material overrideMat = null)
    {
        Material mat = overrideMat ?? CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        float halfW = width * 0.5f;
        float bendR = BEND_RADIUS;
        float topY = legHeight;

        // 左支腿（从底部到弯头起点）
        CreateCylinder(objName + "_LegLeft", root.transform,
            new Vector3(-halfW, (legHeight - bendR) * 0.5f, 0f),
            PIPE_DIAMETER, legHeight - bendR, mat, Axis.Y);

        // 右支腿
        CreateCylinder(objName + "_LegRight", root.transform,
            new Vector3(halfW, (legHeight - bendR) * 0.5f, 0f),
            PIPE_DIAMETER, legHeight - bendR, mat, Axis.Y);

        // 顶部水平段
        float topLen = width - bendR * 2f;
        if (topLen > 0.01f)
        {
            CreateCylinder(objName + "_Top", root.transform,
                new Vector3(0f, topY, 0f),
                PIPE_DIAMETER, topLen, mat, Axis.X);
        }

        // 左上弯头（XY 平面）
        CreateElbow90(objName + "_ElbowTL", root.transform,
            new Vector3(-halfW + bendR, topY - bendR, 0f), "XY", mat);

        // 右上弯头（XY 平面，需要镜像）
        GameObject elbowTR = CreateElbow90(objName + "_ElbowTR_Temp", root.transform,
            new Vector3(0f, 0f, 0f), "XY", mat);
        elbowTR.transform.localPosition = new Vector3(halfW - bendR, topY - bendR, 0f);
        // 镜像 X 方向：对弯头的子对象做 scale.x = -1
        elbowTR.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        return root;
    }

    /// <summary>
    /// 创建法兰接头。
    /// 法兰为宽扁圆柱体，沿 axis 方向。默认沿 X 轴（管道方向）。
    /// </summary>
    public GameObject CreateFlange(string objName, Transform parent, Vector3 localPos,
        Axis pipeAxis = Axis.X, Material overrideMat = null)
    {
        Material flangeMat = overrideMat ?? CreateMaterial("MAT_Flange", MetalLight, 0.7f, 0.45f);
        Material boltMat = CreateMaterial("MAT_Bolt", BoltDark, 0.85f, 0.35f);

        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        // 法兰盘
        CreateCylinder(objName + "_Disc", root.transform, Vector3.zero,
            FLANGE_DIAMETER, FLANGE_THICKNESS, flangeMat, pipeAxis);

        // 螺栓（环绕法兰盘）
        float boltCircleR = FLANGE_DIAMETER * 0.38f;
        for (int i = 0; i < FLANGE_BOLTS; i++)
        {
            float a = i * Mathf.PI * 2f / FLANGE_BOLTS;
            Vector3 boltOffset;

            if (pipeAxis == Axis.X)
                boltOffset = new Vector3(0f, Mathf.Cos(a) * boltCircleR, Mathf.Sin(a) * boltCircleR);
            else if (pipeAxis == Axis.Y)
                boltOffset = new Vector3(Mathf.Cos(a) * boltCircleR, 0f, Mathf.Sin(a) * boltCircleR);
            else
                boltOffset = new Vector3(Mathf.Cos(a) * boltCircleR, Mathf.Sin(a) * boltCircleR, 0f);

            GameObject bolt = CreateCylinder(objName + "_Bolt" + i, root.transform,
                boltOffset, BOLT_DIAMETER, FLANGE_THICKNESS * 1.3f, boltMat, pipeAxis);

            // 螺母（两端各一个）
            float nutOffset = FLANGE_THICKNESS * 0.65f;
            Vector3 nutAxis = Vector3.zero;
            if (pipeAxis == Axis.X) nutAxis = Vector3.right;
            else if (pipeAxis == Axis.Y) nutAxis = Vector3.up;
            else nutAxis = Vector3.forward;

            CreateCube(objName + "_NutA" + i, root.transform,
                boltOffset + nutAxis * nutOffset,
                new Vector3(NUT_SIZE, NUT_SIZE, NUT_THICKNESS), boltMat);
            CreateCube(objName + "_NutB" + i, root.transform,
                boltOffset - nutAxis * nutOffset,
                new Vector3(NUT_SIZE, NUT_SIZE, NUT_THICKNESS), boltMat);
        }

        return root;
    }

    /// <summary>
    /// 创建带旋转阀门的管道段。
    /// 管道沿 X 轴，阀体在中间，手轮朝 +Z（玩家侧）。
    /// 返回根节点，其中 handwheelRoot 子节点用于旋转控制。
    /// </summary>
    public GameObject CreateValvePipeSegment(string objName, Transform parent, Vector3 localPos,
        out Transform handwheelRoot, out Transform valveStem,
        Material pipeMat = null, Material bodyMat = null, Material wheelMat = null)
    {
        pipeMat  = pipeMat  ?? CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        bodyMat  = bodyMat  ?? CreateMaterial("MAT_Valve_Body", MetalDark, 0.75f, 0.40f);
        wheelMat = wheelMat ?? CreateMaterial("MAT_Handwheel_Red", WheelRed, 0.05f, 0.30f);

        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        float bodyHalfW = VALVE_BODY_WIDTH * 0.5f;
        float pipeHalfLen = PIPE_DIAMETER * 0.8f;

        // 左侧管道
        CreateCylinder(objName + "_PipeL", root.transform,
            new Vector3(-bodyHalfW - pipeHalfLen * 0.5f, 0f, 0f),
            PIPE_DIAMETER, pipeHalfLen, pipeMat, Axis.X);

        // 右侧管道
        CreateCylinder(objName + "_PipeR", root.transform,
            new Vector3(bodyHalfW + pipeHalfLen * 0.5f, 0f, 0f),
            PIPE_DIAMETER, pipeHalfLen, pipeMat, Axis.X);

        // 左法兰
        CreateFlange(objName + "_FlangeL", root.transform,
            new Vector3(-bodyHalfW, 0f, 0f), Axis.X);

        // 右法兰
        CreateFlange(objName + "_FlangeR", root.transform,
            new Vector3(bodyHalfW, 0f, 0f), Axis.X);

        // 阀体（球形主体）
        CreateSphere(objName + "_Body", root.transform,
            new Vector3(0f, 0.05f, 0f),
            VALVE_BODY_WIDTH * 0.9f, bodyMat);

        // 阀体上盖
        CreateCylinder(objName + "_Bonnet", root.transform,
            new Vector3(0f, VALVE_BODY_HEIGHT * 0.25f, 0f),
            VALVE_BODY_WIDTH * 0.5f, VALVE_BODY_HEIGHT * 0.35f, bodyMat, Axis.Y);

        // 阀杆（旋转轴）
        GameObject stemObj = new GameObject(objName + "_Stem");
        stemObj.transform.SetParent(root.transform, false);
        stemObj.transform.localPosition = new Vector3(0f, VALVE_BODY_HEIGHT * 0.45f, 0f);
        stemObj.transform.localRotation = Quaternion.identity;
        valveStem = stemObj.transform;

        CreateCylinder(objName + "_StemRod", stemObj.transform,
            Vector3.zero, STEM_DIAMETER, STEM_LENGTH, bodyMat, Axis.Y);

        // 手轮根节点（可旋转）
        GameObject wheelRootObj = new GameObject(objName + "_HandwheelRoot");
        wheelRootObj.transform.SetParent(stemObj.transform, false);
        wheelRootObj.transform.localPosition = new Vector3(0f, STEM_WHEEL_GAP, 0f);
        wheelRootObj.transform.localRotation = Quaternion.identity;
        handwheelRoot = wheelRootObj.transform;

        // 构建手轮
        BuildHandWheel(wheelRootObj.transform, HANDWHEEL_DIAMETER, HANDWHEEL_THICK, wheelMat, bodyMat);

        return root;
    }

    /// <summary>构建阀门手轮（外环、辐条、轮毂、握柄）</summary>
    protected void BuildHandWheel(Transform parent, float diameter, float thickness,
        Material wheelMat, Material hubMat)
    {
        float radius = diameter * 0.5f;
        float ringThick = thickness * 1.5f;
        int ringSegs = 18;

        // 外环（分段圆柱）
        float segArcLen = 2f * Mathf.PI * radius / ringSegs * 0.94f;
        for (int i = 0; i < ringSegs; i++)
        {
            float a = i * Mathf.PI * 2f / ringSegs;
            Vector3 pos = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
            CreateCylinder("RingSeg" + i, parent, pos, ringThick, segArcLen, wheelMat, Axis.Z);
            // 旋转对齐切线
            Transform t = parent.GetChild(parent.childCount - 1);
            t.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg);
        }

        // 轮毂
        CreateCylinder("WheelHub", parent,
            new Vector3(0f, 0f, -0.02f),
            diameter * 0.22f, thickness * 2f, hubMat, Axis.Z);

        // 4 根辐条
        for (int i = 0; i < 4; i++)
        {
            float a = i * 90f + 22.5f;
            CreateCylinder("Spoke" + i, parent,
                Vector3.zero, ringThick * 0.7f, radius * 0.85f, wheelMat, Axis.X);
            Transform t = parent.GetChild(parent.childCount - 1);
            t.localRotation = Quaternion.Euler(0f, 0f, a + 90f);
        }

        // 握柄球
        CreateSphere("GripKnob", parent,
            new Vector3(radius * 0.9f, radius * 0.35f, 0f),
            diameter * 0.14f, wheelMat);
    }

    /// <summary>
    /// 创建带压力表的管道段。
    /// 管道沿 X 轴，压力表通过 T 形分支向上伸出。
    /// 返回 (root, gaugeNeedlePivot) —— gaugeNeedlePivot 用于运行时旋转指针。
    /// </summary>
    public GameObject CreatePressureGaugeSegment(string objName, Transform parent, Vector3 localPos,
        out Transform gaugeNeedlePivot,
        Material pipeMat = null, Material gaugeBodyMat = null)
    {
        pipeMat      = pipeMat      ?? CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        gaugeBodyMat = gaugeBodyMat ?? CreateMaterial("MAT_Gauge_Body", MetalDark, 0.75f, 0.40f);
        Material faceMat = CreateMaterial("MAT_Gauge_Face", GaugeFace, 0.05f, 0.10f);
        Material greenMat = CreateEmissionMaterial("MAT_Gauge_Green", GaugeGreenZone, 0.6f);
        Material redMat   = CreateEmissionMaterial("MAT_Gauge_Red", GaugeRedZone, 0.6f);
        Material needleMat = CreateMaterial("MAT_Needle", WheelRed, 0.05f, 0.20f);

        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        // 主管段
        CreateCylinder(objName + "_Pipe", root.transform, Vector3.zero,
            PIPE_DIAMETER, PIPE_DIAMETER * 2.5f, pipeMat, Axis.X);

        // 法兰
        CreateFlange(objName + "_FlangeL", root.transform,
            new Vector3(-PIPE_DIAMETER * 1.1f, 0f, 0f), Axis.X);
        CreateFlange(objName + "_FlangeR", root.transform,
            new Vector3(PIPE_DIAMETER * 1.1f, 0f, 0f), Axis.X);

        // 分支管（向上）
        float branchH = GAUGE_DIAMETER * 0.9f;
        CreateCylinder(objName + "_Branch", root.transform,
            new Vector3(0f, branchH * 0.5f, 0f),
            PIPE_DIAMETER * 0.5f, branchH, gaugeBodyMat, Axis.Y);

        // 表头外壳
        float gaugeY = branchH + GAUGE_THICKNESS * 0.5f;
        CreateCylinder(objName + "_Housing", root.transform,
            new Vector3(0f, gaugeY, 0f),
            GAUGE_DIAMETER * 1.1f, GAUGE_THICKNESS, gaugeBodyMat, Axis.Y);

        // 表盘面（白色）
        CreateCylinder(objName + "_Face", root.transform,
            new Vector3(0f, gaugeY, 0.001f),
            GAUGE_DIAMETER, GAUGE_THICKNESS * 0.3f, faceMat, Axis.Y);

        // 绿色安全区域弧（扇形）
        CreateGaugeArc(objName + "_GreenArc", root.transform,
            new Vector3(0f, gaugeY, 0.005f),
            GAUGE_DIAMETER * 0.28f, GAUGE_DIAMETER * 0.38f,
            -60f, 60f, greenMat);

        // 红色危险区域弧
        CreateGaugeArc(objName + "_RedArcL", root.transform,
            new Vector3(0f, gaugeY, 0.005f),
            GAUGE_DIAMETER * 0.28f, GAUGE_DIAMETER * 0.38f,
            60f, 120f, redMat);
        CreateGaugeArc(objName + "_RedArcR", root.transform,
            new Vector3(0f, gaugeY, 0.005f),
            GAUGE_DIAMETER * 0.28f, GAUGE_DIAMETER * 0.38f,
            -120f, -60f, redMat);

        // 刻度标记（小方块表示刻度线）
        for (int i = 0; i <= 12; i++)
        {
            float a = Mathf.Lerp(-120f, 120f, i / 12f) * Mathf.Deg2Rad;
            float r = GAUGE_DIAMETER * 0.40f;
            Material tickMat = CreateMaterial("MAT_Tick", BoltDark, 0.5f, 0.3f);
            CreateCube(objName + "_Tick" + i, root.transform,
                new Vector3(Mathf.Cos(a) * r, gaugeY, Mathf.Sin(a) * r + 0.005f),
                new Vector3(0.005f, 0.005f, 0.018f), tickMat);
        }

        // 指针枢轴
        GameObject pivotObj = new GameObject(objName + "_NeedlePivot");
        pivotObj.transform.SetParent(root.transform, false);
        pivotObj.transform.localPosition = new Vector3(0f, gaugeY, 0.008f);
        pivotObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        gaugeNeedlePivot = pivotObj.transform;

        // 指针
        CreateCube(objName + "_Needle", pivotObj.transform,
            new Vector3(GAUGE_DIAMETER * 0.20f, 0f, 0f),
            new Vector3(GAUGE_DIAMETER * 0.38f, 0.006f, 0.01f), needleMat);

        // 中心帽
        // Cap 作为 root 子物体（而非 pivotObj），确保不随指针旋转/万向节锁
        CreateCylinder(objName + "_Cap", root.transform,
            new Vector3(0f, gaugeY, 0.008f - 0.003f),
            GAUGE_DIAMETER * 0.12f, GAUGE_THICKNESS * 0.25f, gaugeBodyMat, Axis.Z);

        return root;
    }

    /// <summary>
    /// 创建仪表弧形区域的自定义 Mesh。
    /// 在 XY 平面生成环形扇面。
    /// </summary>
    protected void CreateGaugeArc(string objName, Transform parent, Vector3 localPos,
        float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, Material mat)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        // ★ 不再使用 Euler(90,0,0)，直接在 XZ 平面构建 mesh，避免万向节锁
        obj.transform.localRotation = Quaternion.identity;

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        int segs = 24;
        Vector3[] verts = new Vector3[(segs + 1) * 2];
        Vector3[] norms = new Vector3[verts.Length];
        Vector2[] uvs   = new Vector2[verts.Length];
        int[]     tris  = new int[segs * 6];

        for (int i = 0; i <= segs; i++)
        {
            float t = (float)i / segs;
            float angle = Mathf.Lerp(startAngleDeg, endAngleDeg, t) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            int idx = i * 2;
            // ★ 直接在 XZ 平面构建（Y=0），不再经 XY→XZ 旋转
            verts[idx]     = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
            verts[idx + 1] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);
            norms[idx]     = Vector3.up;
            norms[idx + 1] = Vector3.up;
            uvs[idx]       = new Vector2(t, 0f);
            uvs[idx + 1]   = new Vector2(t, 1f);
        }

        for (int i = 0; i < segs; i++)
        {
            int v = i * 2;
            int t = i * 6;
            tris[t]     = v;
            tris[t + 1] = v + 1;
            tris[t + 2] = v + 2;
            tris[t + 3] = v + 2;
            tris[t + 4] = v + 1;
            tris[t + 5] = v + 3;
        }

        Mesh mesh = new Mesh();
        mesh.name = objName + "_Mesh";
        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        mf.sharedMesh = mesh;
    }

    /// <summary>
    /// 创建带流量计的管道段。
    /// 管道沿 X 轴，流量计本体在管道上方。
    /// 返回 (root, displayTextMesh) —— displayTextMesh 用于运行时更新读数。
    /// </summary>
    public GameObject CreateFlowMeterSegment(string objName, Transform parent, Vector3 localPos,
        out TextMesh displayTextMesh,
        Material pipeMat = null, Material meterBodyMat = null)
    {
        pipeMat      = pipeMat      ?? CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        meterBodyMat = meterBodyMat ?? CreateMaterial("MAT_Meter_Body", MetalDark, 0.70f, 0.40f);

        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        // 主管段
        CreateCylinder(objName + "_Pipe", root.transform, Vector3.zero,
            PIPE_DIAMETER, PIPE_DIAMETER * 2.5f, pipeMat, Axis.X);

        // 法兰
        CreateFlange(objName + "_FlangeL", root.transform,
            new Vector3(-PIPE_DIAMETER * 1.1f, 0f, 0f), Axis.X);
        CreateFlange(objName + "_FlangeR", root.transform,
            new Vector3(PIPE_DIAMETER * 1.1f, 0f, 0f), Axis.X);

        // 流量计本体（在管道上方）
        float meterY = PIPE_RADIUS + FLOWMETER_HEIGHT * 0.5f;
        CreateCube(objName + "_Body", root.transform,
            new Vector3(0f, meterY, 0f),
            new Vector3(FLOWMETER_WIDTH, FLOWMETER_HEIGHT, PIPE_DIAMETER * 0.7f),
            meterBodyMat);

        // 连接颈部
        CreateCylinder(objName + "_Neck", root.transform,
            new Vector3(0f, PIPE_RADIUS + 0.04f, 0f),
            PIPE_DIAMETER * 0.35f, 0.08f, meterBodyMat, Axis.Y);

        // 显示面板
        Material panelMat = CreateMaterial("MAT_Meter_Panel", new Color(0.15f, 0.18f, 0.20f), 0.1f, 0.15f);
        CreateCube(objName + "_Panel", root.transform,
            new Vector3(0f, meterY, PIPE_DIAMETER * 0.4f),
            new Vector3(FLOWMETER_WIDTH * 0.75f, FLOWMETER_HEIGHT * 0.55f, 0.015f),
            panelMat);

        // 文本显示
        GameObject textObj = new GameObject(objName + "_DisplayText");
        textObj.transform.SetParent(root.transform, false);
        textObj.transform.localPosition = new Vector3(0f, meterY, PIPE_DIAMETER * 0.42f);
        textObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // 面朝 +Z（玩家方向）

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = "0.0 L/min";
        tm.fontSize = 72;
        tm.characterSize = 0.025f * SCALE_FACTOR;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.2f, 0.95f, 0.65f);
        displayTextMesh = tm;

        return root;
    }

    /// <summary>
    /// 创建管道支架。
    /// 底板 + 立柱 + 管托（U 形抱箍）。
    /// </summary>
    public GameObject CreatePipeSupport(string objName, Transform parent, Vector3 localPos,
        float supportHeight, Material overrideMat = null)
    {
        Material mat = overrideMat ?? CreateMaterial("MAT_Support", MetalDark, 0.70f, 0.38f);

        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        // 底板
        CreateCube(objName + "_Base", root.transform,
            new Vector3(0f, 0.03f, 0f),
            new Vector3(SUPPORT_BASE_SIZE, 0.06f, SUPPORT_BASE_SIZE), mat);

        // 立柱
        float poleH = supportHeight - PIPE_RADIUS;
        CreateCylinder(objName + "_Pole", root.transform,
            new Vector3(0f, poleH * 0.5f + 0.03f, 0f),
            SUPPORT_POLE_DIA, poleH, mat, Axis.Y);

        // 管托底座
        CreateCube(objName + "_Saddle", root.transform,
            new Vector3(0f, supportHeight - PIPE_RADIUS * 0.3f, 0f),
            new Vector3(PIPE_DIAMETER * 1.1f, PIPE_RADIUS * 0.5f, PIPE_DIAMETER * 0.6f), mat);

        // 抱箍（左右两个半环 - 用弯曲的立方体近似）
        for (int side = -1; side <= 1; side += 2)
        {
            CreateCube(objName + "_Clamp" + (side > 0 ? "R" : "L"), root.transform,
                new Vector3(side * PIPE_DIAMETER * 0.45f, supportHeight, 0f),
                new Vector3(0.04f, PIPE_DIAMETER * 0.7f, PIPE_DIAMETER * 0.25f), mat);
        }

        return root;
    }

    /// <summary>
    /// 创建急停按钮面板。
    /// 面板 + 红色蘑菇头按钮 + 标签。
    /// 返回 (root, buttonTransform) —— buttonTransform 用于运行时按下/复位动画。
    /// </summary>
    public GameObject CreateEmergencyStopPanel(string objName, Transform parent, Vector3 localPos,
        out Transform buttonTransform, out Renderer buttonRenderer,
        Material panelMat = null)
    {
        panelMat = panelMat ?? CreateMaterial("MAT_ESTOP_Panel", PanelWhite, 0.10f, 0.20f);
        Material redMat    = CreateEmissionMaterial("MAT_ESTOP_Button", EStopRed, 2.0f);
        Material yellowMat = CreateEmissionMaterial("MAT_ESTOP_Ring", ButtonYellow, 1.3f);
        Material darkMat   = CreateMaterial("MAT_ESTOP_Dark", MetalDark, 0.5f, 0.3f);

        float panelCenterY = ESTOP_PANEL_H * 0.5f;
        float frameDepth = ESTOP_PANEL_D * 0.33f;
        float buttonCenterY = ESTOP_PANEL_H * 0.74f;
        float guardRingZ = ESTOP_PANEL_D * 0.67f;
        float buttonZ = ESTOP_PANEL_D * 0.92f;
        float buttonBaseZ = ESTOP_PANEL_D * 0.58f;
        float labelY = ESTOP_PANEL_H * 0.17f;
        float labelZ = ESTOP_PANEL_D * 0.75f;

        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        // 面板
        CreateCube(objName + "_Panel", root.transform,
            new Vector3(0f, panelCenterY, 0f),
            new Vector3(ESTOP_PANEL_W, ESTOP_PANEL_H, ESTOP_PANEL_D), panelMat);

        // 面板边框
        CreateCube(objName + "_PanelFrame", root.transform,
            new Vector3(0f, panelCenterY, -frameDepth),
            new Vector3(ESTOP_PANEL_W + 0.05f * SCALE_FACTOR, ESTOP_PANEL_H + 0.05f * SCALE_FACTOR, 0.04f * SCALE_FACTOR), darkMat);

        // 黄色警示环
        CreateCylinder(objName + "_GuardRing", root.transform,
            new Vector3(0f, buttonCenterY, guardRingZ),
            ESTOP_BODY_DIAMETER * 0.85f, 0.015f * SCALE_FACTOR, yellowMat, Axis.Z);

        // 急停按钮（红色蘑菇头）
        GameObject btnObj = CreateCylinder(objName + "_Button", root.transform,
            new Vector3(0f, buttonCenterY, buttonZ),
            ESTOP_BODY_DIAMETER * 0.55f, 0.06f * SCALE_FACTOR, redMat, Axis.Z);
        buttonTransform = btnObj.transform;
        buttonRenderer = btnObj.GetComponent<Renderer>();

        // 按钮底座
        CreateCylinder(objName + "_ButtonBase", root.transform,
            new Vector3(0f, buttonCenterY, buttonBaseZ),
            ESTOP_BODY_DIAMETER * 0.40f, 0.04f * SCALE_FACTOR, darkMat, Axis.Z);

        // 标签文字
        GameObject labelObj = new GameObject(objName + "_Label");
        labelObj.transform.SetParent(root.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, labelY, labelZ);
        labelObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        TextMesh label = labelObj.AddComponent<TextMesh>();
        label.text = "紧急停止\nEMERGENCY STOP";
        label.fontSize = 36;
        label.characterSize = 0.018f * SCALE_FACTOR;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = EStopRed;
        label.lineSpacing = 0.9f;

        return root;
    }

    /// <summary>
    /// 创建独立式压力表（安装在面板或支架上，非管道嵌入式）。
    /// </summary>
    public GameObject CreateStandaloneGauge(string objName, Transform parent, Vector3 localPos,
        out Transform needlePivot, string labelText = "压力表",
        Material bodyMat = null)
    {
        bodyMat = bodyMat ?? CreateMaterial("MAT_Gauge_Body", MetalDark, 0.75f, 0.40f);
        Material faceMat  = CreateMaterial("MAT_Gauge_Face", GaugeFace, 0.05f, 0.10f);
        Material greenMat = CreateEmissionMaterial("MAT_Gauge_Green", GaugeGreenZone, 0.6f);
        Material redMat   = CreateEmissionMaterial("MAT_Gauge_Red", GaugeRedZone, 0.6f);
        Material needleMat = CreateMaterial("MAT_Needle", WheelRed, 0.05f, 0.20f);

        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        // 外壳
        CreateCylinder(objName + "_Housing", root.transform,
            Vector3.zero, GAUGE_DIAMETER * 1.15f, GAUGE_THICKNESS, bodyMat, Axis.Z);

        // 表盘
        CreateCylinder(objName + "_Face", root.transform,
            new Vector3(0f, 0f, 0.003f),
            GAUGE_DIAMETER, GAUGE_THICKNESS * 0.25f, faceMat, Axis.Z);

        // 绿色弧
        CreateGaugeArc(objName + "_Green", root.transform,
            new Vector3(0f, 0f, 0.006f),
            GAUGE_DIAMETER * 0.28f, GAUGE_DIAMETER * 0.38f,
            -60f, 60f, greenMat);

        // 红色弧
        CreateGaugeArc(objName + "_RedL", root.transform,
            new Vector3(0f, 0f, 0.006f),
            GAUGE_DIAMETER * 0.28f, GAUGE_DIAMETER * 0.38f,
            60f, 120f, redMat);
        CreateGaugeArc(objName + "_RedR", root.transform,
            new Vector3(0f, 0f, 0.006f),
            GAUGE_DIAMETER * 0.28f, GAUGE_DIAMETER * 0.38f,
            -120f, -60f, redMat);

        // 指针枢轴
        GameObject pivotObj = new GameObject(objName + "_NeedlePivot");
        pivotObj.transform.SetParent(root.transform, false);
        pivotObj.transform.localPosition = new Vector3(0f, 0f, 0.009f);
        pivotObj.transform.localRotation = Quaternion.identity;
        needlePivot = pivotObj.transform;

        // 指针
        CreateCube(objName + "_Needle", pivotObj.transform,
            new Vector3(GAUGE_DIAMETER * 0.20f, 0f, 0f),
            new Vector3(GAUGE_DIAMETER * 0.38f, 0.006f, 0.008f), needleMat);

        // 中心帽
        // Cap 作为 root 子物体（而非 pivotObj），确保不随指针旋转/万向节锁
        CreateCylinder(objName + "_Cap", root.transform,
            new Vector3(0f, 0f, 0.009f - 0.002f),
            GAUGE_DIAMETER * 0.10f, GAUGE_THICKNESS * 0.2f, bodyMat, Axis.Z);

        // 连接管
        CreateCylinder(objName + "_Stem", root.transform,
            new Vector3(0f, -GAUGE_DIAMETER * 0.7f, 0f),
            PIPE_DIAMETER * 0.3f, GAUGE_DIAMETER * 0.4f, bodyMat, Axis.Y);

        // 标签
        if (!string.IsNullOrEmpty(labelText))
        {
            GameObject lblObj = new GameObject(objName + "_Label");
            lblObj.transform.SetParent(root.transform, false);
            lblObj.transform.localPosition = new Vector3(0f, -GAUGE_DIAMETER * 0.95f, 0f);
            lblObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh tm = lblObj.AddComponent<TextMesh>();
            tm.text = labelText;
            tm.fontSize = 32;
            tm.characterSize = 0.015f * SCALE_FACTOR;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
        }

        return root;
    }

    /// <summary>
    /// 创建状态指示灯。
    /// </summary>
    public GameObject CreateStatusLamp(string objName, Transform parent, Vector3 localPos,
        out Renderer lampRenderer, Color? onColor = null)
    {
        Color c = onColor ?? LampGreen;
        Material offMat = CreateMaterial("MAT_Lamp_Off", LampDim, 0f, 0.10f);

        GameObject lamp = CreateSphere(objName, parent, localPos, LAMP_DIAMETER, offMat);
        lampRenderer = lamp.GetComponent<Renderer>();
        return lamp;
    }

    /// <summary>
    /// 创建确认/交互按钮。
    /// </summary>
    public GameObject CreatePushButton(string objName, Transform parent, Vector3 localPos,
        out Transform buttonTransform, string label = "确认",
        Color? btnColor = null)
    {
        Color c = btnColor ?? ButtonYellow;
        Material btnMat = CreateEmissionMaterial("MAT_Btn_" + objName, c, 1.2f);
        Material baseMat = CreateMaterial("MAT_BtnBase", MetalDark, 0.5f, 0.3f);

        GameObject root = new GameObject(objName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = Quaternion.identity;

        // 底座
        CreateCylinder(objName + "_Base", root.transform,
            Vector3.zero, BTN_BASE_DIAMETER, BTN_BASE_THICK, baseMat, Axis.Z);

        // 按钮
        float btnZ = BTN_BASE_THICK + 0.01f * SCALE_FACTOR;
        GameObject btn = CreateCylinder(objName + "_Btn", root.transform,
            new Vector3(0f, 0f, btnZ), BTN_DIAMETER, BTN_THICK, btnMat, Axis.Z);
        buttonTransform = btn.transform;

        // 标签
        if (!string.IsNullOrEmpty(label))
        {
            GameObject lbl = new GameObject(objName + "_Label");
            lbl.transform.SetParent(root.transform, false);
            lbl.transform.localPosition = new Vector3(0f, -BTN_DIAMETER, btnZ);
            lbl.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh tm = lbl.AddComponent<TextMesh>();
            tm.text = label;
            tm.fontSize = 36;
            tm.characterSize = 0.015f * SCALE_FACTOR;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
        }

        return root;
    }

    /// <summary>
    /// 创建设备标签（TextMesh，面朝 +Z / 玩家方向）。
    /// </summary>
    public GameObject CreateLabel(string objName, Transform parent, Vector3 localPos,
        string text, float charSize = -1f, Color? textColor = null, bool bold = false)
    {
        // 默认字符大小随缩放因子调整
        float cs = charSize > 0f ? charSize : (0.03f * SCALE_FACTOR);
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // 面朝 -Z

        TextMesh tm = obj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = Mathf.Clamp(Mathf.RoundToInt(cs * 1200f), 24, 128);
        tm.characterSize = cs;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = textColor ?? Color.white;
        tm.lineSpacing = 0.85f;

        if (bold)
            tm.fontStyle = FontStyle.Bold;

        return obj;
    }

    /// <summary>
    /// 创建地面区域标记（半透明平面，用于标记训练区域）。
    /// </summary>
    public GameObject CreateGroundMarker(string objName, Transform parent, Vector3 localPos,
        float width, float depth, Color? markerColor = null)
    {
        Color c = markerColor ?? new Color(1f, 0.84f, 0.1f, 0.25f);
        Material markerMat = new Material(ResolveLitShader());
        markerMat.name = "MAT_GroundMarker";
        if (markerMat.HasProperty("_BaseColor")) markerMat.SetColor("_BaseColor", c);
        if (markerMat.HasProperty("_Color")) markerMat.SetColor("_Color", c);
        // 尝试透明
        if (markerMat.HasProperty("_Surface"))
        {
            markerMat.SetFloat("_Surface", 1f);
            markerMat.SetFloat("_Blend", 0f);
            markerMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            markerMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        _runtimeMaterials.Add(markerMat);

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        obj.name = objName;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = new Vector3(width * 0.1f, 1f, depth * 0.1f); // Plane 默认 10x10
        obj.GetComponent<Renderer>().sharedMaterial = markerMat;
        if (!addColliders) DestroySafe(obj.GetComponent<Collider>());
        return obj;
    }

    /// <summary>
    /// 清除之前生成的模型。
    /// </summary>
    public void ClearModel(string rootName = "__Generated_Pipeline_Scene__")
    {
        Transform old = transform.Find(rootName);
        if (old != null)
            DestroySafe(old.gameObject);

        for (int i = 0; i < _runtimeMaterials.Count; i++)
            DestroySafe(_runtimeMaterials[i]);
        _runtimeMaterials.Clear();

        GeneratedRoot = null;
    }

    /// <summary>
    /// 主构建入口 —— 子类重写此方法来实现具体场景搭建。
    /// 基类保证根节点创建和朝向设置。
    /// </summary>
    public GameObject CreateRoot(string rootName = "__Generated_Pipeline_Scene__")
    {
        ClearModel(rootName);

        transform.position = Vector3.zero;
        transform.rotation = faceSouth ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        transform.localScale = Vector3.one;

        GameObject root = new GameObject(rootName);
        root.transform.SetParent(transform, false);
        GeneratedRoot = root;
        return root;
    }

    [ContextMenu("Build / Rebuild Pipeline Scene")]
    public virtual void Build()
    {
        // 基类空实现 —— 子类覆盖
        Debug.Log("[PipelineBuilder] Base Build() called. Override this in subclass or use specific scene builder.");
    }
}
