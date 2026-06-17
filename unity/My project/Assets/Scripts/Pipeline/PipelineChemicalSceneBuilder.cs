using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 化工厂管道培训场景搭建器。
/// 使用 PipelineBuilder 按培训步骤布局完整的管道场景。
/// 包含：管道线路、阀门、压力表、流量计、急停面板、巡检点、地面标记等。
/// </summary>
public class PipelineChemicalSceneBuilder : PipelineBuilder
{
    public const string SceneRootName = "__Generated_Chemical_Pipeline_Scene__";
    public const string StaticSceneName = "Chemical Pipeline Training Static";

    [Header("Scene Layout")]
    public Vector3 sceneOrigin = Vector3.zero;

    // ── 场景中生成的交互对象引用（供 TrainingManager 使用）──
    [HideInInspector] public Transform inletValveWheel;      // V1 手轮
    [HideInInspector] public Transform controlValveWheel;    // V2 手轮
    [HideInInspector] public Transform outletValveWheel;     // V3 手轮
    [HideInInspector] public Transform gaugeP1Needle;        // P1 指针
    [HideInInspector] public Transform gaugeP2Needle;        // P2 指针
    [HideInInspector] public TextMesh flowMeterDisplay;      // F1 显示
    [HideInInspector] public Transform eStopButton;          // 急停按钮
    [HideInInspector] public Renderer eStopRenderer;         // 急停按钮渲染器
    [HideInInspector] public List<GameObject> inspectionZones = new List<GameObject>();
    [HideInInspector] public List<GameObject> confirmButtons = new List<GameObject>();

    // ── 场景具体尺寸参数（受 SCALE_FACTOR 影响）─────────────────
    private const float GROUND_WIDTH  = 22f * SCALE_FACTOR;
    private const float GROUND_DEPTH  = 16f * SCALE_FACTOR;
    private const float PIPE_HEIGHT   = 1.2f * SCALE_FACTOR;   // 管道中心线离地高度
    private const float SUPPORT_SPACING = 2.5f * SCALE_FACTOR; // 支架间距

    // ═══════════════════════════════════════════════════════════
    //  主构建入口
    // ═══════════════════════════════════════════════════════════

    [ContextMenu("Build / Rebuild Chemical Pipeline Scene")]
    public override void Build()
    {
        GameObject root = CreateRoot(SceneRootName);
        if (root == null) return;

        BuildGroundAndEnvironment(root.transform);
        BuildPipelineRoute(root.transform);
        BuildTrainingStations(root.transform);
        BuildLabelsAndMarkers(root.transform);
        AttachTrainingManager(root);

        Debug.Log("[PipelineChemicalSceneBuilder] 化工厂管道培训场景已生成。共 10 个培训步骤。");
    }

    // ═══════════════════════════════════════════════════════════
    //  地面与环境
    // ═══════════════════════════════════════════════════════════

    void BuildGroundAndEnvironment(Transform root)
    {
        // 地面
        Material groundMat = CreateMaterial("MAT_Ground", new Color(0.28f, 0.30f, 0.32f), 0f, 0.15f);
        float groundThick = 0.1f * SCALE_FACTOR;
        float groundY = -0.05f * SCALE_FACTOR;
        CreateCube("Ground", root,
            new Vector3(sceneOrigin.x, groundY, sceneOrigin.z + GROUND_DEPTH * 0.5f - 1f * SCALE_FACTOR),
            new Vector3(GROUND_WIDTH, groundThick, GROUND_DEPTH), groundMat);

        // 地面网格线（视觉参考）
        float gridSpacing = 2f * SCALE_FACTOR;
        float gridOffset = 1f * SCALE_FACTOR;
        for (float x = -GROUND_WIDTH * 0.5f + gridOffset; x < GROUND_WIDTH * 0.5f; x += gridSpacing)
        {
            CreateCube("GridLine_X_" + x, root,
                new Vector3(x, 0.001f, GROUND_DEPTH * 0.5f - 1f * SCALE_FACTOR),
                new Vector3(0.02f * SCALE_FACTOR, 0.002f, GROUND_DEPTH), CreateMaterial("MAT_Grid", new Color(0.35f, 0.37f, 0.38f), 0f, 0.1f));
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  管道线路
    // ═══════════════════════════════════════════════════════════

    void BuildPipelineRoute(Transform root)
    {
        // 管道线路设计（XZ 平面俯视图）：
        //
        // 所有坐标值已乘以 SCALE_FACTOR，原始设计坐标见注释。
        //
        //   Z=25 [V3出口阀]═══[E-Stop]═══[关停记录站]
        //           ‖                        (第8-10步区域)
        //   Z=20   ‖
        //   Z=15 [P2压力表]
        //           ‖
        //   Z=10   ‖
        //   Z=5  [V2控制阀]
        //           ‖
        //   Z=0    ‖
        //   Z=-5 [F1流量计]
        //           ‖
        //   Z=-10   ‖
        //   Z=-15 [P1压力表]══[V1进口阀]══[PPE确认站]
        //         X=-25     X=-35      X=-45       (第1-4步区域)
        //
        // 管道走向：水平段1 (Z=-15, X=-25→-15) → 90°弯 → 垂直段 (X=-15→25, Z方向)
        //           → 90°弯 → 水平段2 (Z=25, X=-15→25)

        float py = PIPE_HEIGHT;
        float S = SCALE_FACTOR; // 缩写，方便阅读

        // ── 水平段 1：PPE → V1 → P1 → 弯头（Z=-15, X 从 -45 到 -15）──
        float seg1EndX = -3f * S;
        float seg1Z = -3f * S;

        // 直管：X=-45 到 X=-25（PPE 到 V1 之间），长度 = 20
        CreateStraightPipe("Pipe_Seg1_Left", root,
            new Vector3(-7f * S, py, seg1Z), 4f * S, false);

        // 进口阀门 V1 在 X=-35
        BuildValveStation(root, "V1", new Vector3(-7f * S, py, seg1Z),
            out inletValveWheel, out _);

        // 直管：X=-35 到 X=-25（V1 到 P1 之间），长度 = 10
        CreateStraightPipe("Pipe_Seg1_Mid", root,
            new Vector3(-6f * S, py, seg1Z), 2f * S, false);

        // 压力表 P1 在 X=-25
        BuildGaugeStation(root, "P1", new Vector3(-5f * S, py, seg1Z),
            out gaugeP1Needle, "压力表 P1\n(起始端)");

        // 直管：X=-25 到 X=-15
        CreateStraightPipe("Pipe_Seg1_Right", root,
            new Vector3(-4f * S, py, seg1Z), 2f * S, false);

        // 弯头 1：在 X=-15, Z=-15，从 +X 方向弯到 +Z 方向
        CreateElbow90("Elbow_1", root,
            new Vector3(seg1EndX, py, seg1Z), "XZ");

        // ── 垂直段：Z=-15 → Z=25（X=-15）──
        float seg2EndZ = 5f * S;

        // 直管：Z=-15 到 Z=-5（弯头到 F1），沿 Z 轴
        CreatePipeAlongZ("Pipe_Seg2_Z1", root,
            new Vector3(seg1EndX, py, -2f * S), 2f * S);

        // 流量计 F1 在 Z=-5
        BuildFlowMeterStation(root, "F1", new Vector3(seg1EndX, py, -1f * S),
            out flowMeterDisplay);

        // 直管：Z=-5 到 Z=5（F1 到 V2）
        CreatePipeAlongZ("Pipe_Seg2_Z2", root,
            new Vector3(seg1EndX, py, 0f * S), 2f * S);

        // 控制阀 V2 在 Z=5
        BuildValveStation(root, "V2", new Vector3(seg1EndX, py, 1f * S),
            out controlValveWheel, out _, isControlValve: true);

        // 直管：Z=5 到 Z=15（V2 到 P2）
        CreatePipeAlongZ("Pipe_Seg2_Z3", root,
            new Vector3(seg1EndX, py, 2f * S), 2f * S);

        // 压力表 P2 在 Z=15
        BuildGaugeStation(root, "P2", new Vector3(seg1EndX, py, 3f * S),
            out gaugeP2Needle, "压力表 P2\n(中段)");

        // 直管：Z=15 到 Z=25
        CreatePipeAlongZ("Pipe_Seg2_Z4", root,
            new Vector3(seg1EndX, py, 4f * S), 2f * S);

        // 弯头 2：在 X=-15, Z=25，从 +Z 方向弯到 +X 方向
        CreateElbow90("Elbow_2", root,
            new Vector3(seg1EndX, py, seg2EndZ), "XZ");

        // ── 水平段 2：X=-15 → X=25（Z=25）──
        float seg3Z = 5f * S;

        // 直管：X=-15 到 X=0（弯头到 V3）
        CreateStraightPipe("Pipe_Seg3_Left", root,
            new Vector3(-1.5f * S, py, seg3Z), 3f * S, false);

        // 出口阀 V3 在 X=0
        BuildValveStation(root, "V3", new Vector3(0f * S, py, seg3Z),
            out outletValveWheel, out _);

        // 直管：X=0 到 X=25（V3 到 E-Stop/终点）
        CreateStraightPipe("Pipe_Seg3_Right", root,
            new Vector3(2.5f * S, py, seg3Z), 5f * S, false);

        // ── 管道支架 ───────────────────────────────────────────
        BuildPipeSupports(root, py);
    }

    /// <summary>创建沿 Z 轴的管道段（垂直走向的管道）</summary>
    void CreatePipeAlongZ(string objName, Transform parent, Vector3 centerPos, float length)
    {
        Material pipeMat = CreateMaterial("MAT_Pipe_Yellow_Z", PipeYellow, 0.15f, 0.35f);
        // 使用 Axis.Z 让圆柱沿 Z 轴
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = objName;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = centerPos;
        obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Cylinder 默认 Y-up, 旋转使沿 Z
        obj.transform.localScale = new Vector3(PIPE_DIAMETER, length * 0.5f, PIPE_DIAMETER);
        obj.GetComponent<Renderer>().sharedMaterial = pipeMat;
        if (!addColliders) DestroySafe(obj.GetComponent<Collider>());
    }

    /// <summary>构建阀门工位（管道段 + 法兰 + 阀门）</summary>
    void BuildValveStation(Transform root, string valveId, Vector3 pos,
        out Transform handwheelRoot, out Transform valveStem, bool isControlValve = false)
    {
        Material wheelMat = isControlValve
            ? CreateMaterial("MAT_CtrlValve_Wheel", new Color(0.2f, 0.5f, 0.9f), 0.05f, 0.30f) // 蓝色手轮区分控制阀
            : CreateMaterial("MAT_Handwheel_Red", WheelRed, 0.05f, 0.30f);

        Material pipeMat = CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        Material bodyMat = CreateMaterial("MAT_Valve_Body", MetalDark, 0.75f, 0.40f);

        // 创建阀门管道段（手轮朝向 +Z，即玩家侧）
        // 水平管沿 X 轴，手轮朝 +Z
        GameObject valveObj = CreateValvePipeSegment(
            "ValveStation_" + valveId, root, pos,
            out handwheelRoot, out valveStem,
            pipeMat, bodyMat, wheelMat);

        // 手轮默认朝 +Y（上方），旋转使其朝 +Z（玩家方向）
        if (handwheelRoot != null)
        {
            handwheelRoot.parent.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }

    /// <summary>构建压力表工位（管道段 + 法兰 + 压力表）</summary>
    void BuildGaugeStation(Transform root, string gaugeId, Vector3 pos,
        out Transform needlePivot, string labelText)
    {
        Material pipeMat = CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        Material bodyMat = CreateMaterial("MAT_Gauge_Body", MetalDark, 0.75f, 0.40f);

        GameObject gaugeObj = CreatePressureGaugeSegment(
            "GaugeStation_" + gaugeId, root, pos,
            out needlePivot, pipeMat, bodyMat);

        // 添加标签
        CreateLabel("Label_" + gaugeId, root,
            new Vector3(pos.x, pos.y + GAUGE_DIAMETER * 0.9f, pos.z + PIPE_DIAMETER * 0.6f),
            labelText, 0.025f * SCALE_FACTOR, Color.white);
    }

    /// <summary>构建流量计工位</summary>
    void BuildFlowMeterStation(Transform root, string meterId, Vector3 pos,
        out TextMesh displayText)
    {
        Material pipeMat = CreateMaterial("MAT_Pipe_Yellow", PipeYellow, 0.15f, 0.35f);
        Material bodyMat = CreateMaterial("MAT_Meter_Body", MetalDark, 0.70f, 0.40f);

        CreateFlowMeterSegment("FlowMeter_" + meterId, root, pos,
            out displayText, pipeMat, bodyMat);

        // 标签
        CreateLabel("Label_" + meterId, root,
            new Vector3(pos.x, pos.y + FLOWMETER_HEIGHT * 0.8f, pos.z + PIPE_DIAMETER * 0.5f),
            "流量计 F1\n(中段)", 0.025f * SCALE_FACTOR, Color.white);
    }

    /// <summary>沿线放置管道支架</summary>
    void BuildPipeSupports(Transform root, float pipeY)
    {
        Material supportMat = CreateMaterial("MAT_Support", MetalDark, 0.70f, 0.38f);
        float S = SCALE_FACTOR;

        // 水平段 1 支架（Z=-15, X 从 -40 到 -17.5）
        for (float x = -8f * S; x <= -3.5f * S; x += SUPPORT_SPACING)
            CreatePipeSupport("Support_S1_" + x, root,
                new Vector3(x, 0f, -3f * S), pipeY, supportMat);

        // 垂直段支架（X=-15, Z 从 -10 到 22.5）
        for (float z = -2f * S; z <= 4.5f * S; z += SUPPORT_SPACING)
            CreatePipeSupport("Support_S2_" + z, root,
                new Vector3(-3f * S, 0f, z), pipeY, supportMat);

        // 水平段 2 支架（Z=25, X 从 -10 到 22.5）
        for (float x = -2f * S; x <= 4.5f * S; x += SUPPORT_SPACING)
            CreatePipeSupport("Support_S3_" + x, root,
                new Vector3(x, 0f, 5f * S), pipeY, supportMat);
    }

    // ═══════════════════════════════════════════════════════════
    //  培训工位（非管道元件：PPE 站、急停、关停记录、巡检点）
    // ═══════════════════════════════════════════════════════════

    void BuildTrainingStations(Transform root)
    {
        float S = SCALE_FACTOR;

        // ── 1. PPE 确认站 ──────────────────────────────────────
        BuildPPEStation(root, new Vector3(-9.5f * S, 0f, -5f * S));

        // ── 巡检点（沿途 3 个）─────────────────────────────────
        inspectionZones.Clear();
        inspectionZones.Add(BuildInspectionPoint(root, "InspectionPoint_1",
            new Vector3(-8f * S, 0f, -2.5f * S), "巡检点 ①\n检查管道外观"));
        inspectionZones.Add(BuildInspectionPoint(root, "InspectionPoint_2",
            new Vector3(-3f * S, 0f, 0f * S), "巡检点 ②\n检查法兰螺栓"));
        inspectionZones.Add(BuildInspectionPoint(root, "InspectionPoint_3",
            new Vector3(2f * S, 0f, 5.5f * S), "巡检点 ③\n检查出口端"));

        // ── 9. 急停面板 ────────────────────────────────────────
        BuildEStopStation(root, new Vector3(3.5f * S, 0f, 6.5f * S));

        // ── 10. 关停记录站 ─────────────────────────────────────
        BuildShutdownStation(root, new Vector3(5.5f * S, 0f, 6.5f * S));
    }

    void BuildPPEStation(Transform root, Vector3 pos)
    {
        GameObject station = new GameObject("PPE_Station");
        station.transform.SetParent(root, false);
        station.transform.localPosition = pos;

        float S = SCALE_FACTOR;

        // 信息面板
        Material panelMat = CreateMaterial("MAT_PPE_Panel", PanelWhite, 0.10f, 0.20f);
        CreateCube("PPE_Panel", station.transform,
            new Vector3(0f, 1.0f * S, 0f),
            new Vector3(PPE_PANEL_W, PPE_PANEL_H, PPE_PANEL_D), panelMat);

        // 面板框架
        Material frameMat = CreateMaterial("MAT_PPE_Frame", MetalDark, 0.5f, 0.3f);
        CreateCube("PPE_Frame", station.transform,
            new Vector3(0f, 1.0f * S, -0.05f * S),
            new Vector3(PPE_PANEL_W + 0.1f * S, PPE_PANEL_H + 0.1f * S, 0.04f * S), frameMat);

        // 支柱
        CreateCylinder("PPE_Pole", station.transform,
            new Vector3(0f, 0.25f * S, 0f), PPE_POLE_DIA, PPE_POLE_H, frameMat, Axis.Y);

        // 确认按钮
        confirmButtons.Clear();
        GameObject btn = CreatePushButton("PPE_Confirm_Btn", station.transform,
            new Vector3(0f, 0.4f * S, 0.1f * S), out Transform btnTrans, "确认PPE");
        confirmButtons.Add(btn);

        // 状态灯
        CreateStatusLamp("PPE_Lamp", station.transform,
            new Vector3(0f, 1.8f * S, 0.05f * S), out Renderer lampR);

        // 说明文字
        CreateLabel("PPE_Label", station.transform,
            new Vector3(0f, 2.0f * S, 0.07f * S),
            "安全装备确认\nPPE CHECK", 0.028f * S, Color.white, true);
    }

    GameObject BuildInspectionPoint(Transform root, string name, Vector3 pos, string labelText)
    {
        float S = SCALE_FACTOR;
        GameObject point = new GameObject(name);
        point.transform.SetParent(root, false);
        point.transform.localPosition = pos;

        // 地面标记
        Material markerMat = CreateMaterial("MAT_Inspect_Marker",
            new Color(0.2f, 0.6f, 1f, 0.3f), 0f, 0.1f);
        // 使用透明
        if (markerMat.HasProperty("_Surface"))
        {
            markerMat.SetFloat("_Surface", 1f);
            markerMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            markerMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        CreateCylinder("Inspect_Marker_" + name, point.transform,
            new Vector3(0f, 0.01f, 0f), INSPECT_MARKER_DIA, 0.01f, markerMat, Axis.Y);

        // 标签柱
        Material poleMat = CreateMaterial("MAT_Inspect_Pole", MetalDark, 0.5f, 0.3f);
        CreateCylinder("Inspect_Pole_" + name, point.transform,
            new Vector3(0f, 0.35f * S, 0f), INSPECT_POLE_DIA, INSPECT_POLE_H, poleMat, Axis.Y);

        // 顶部球
        Material bulbMat = CreateEmissionMaterial("MAT_Inspect_Bulb",
            new Color(0.2f, 0.55f, 1f), 0.8f);
        CreateSphere("Inspect_Bulb_" + name, point.transform,
            new Vector3(0f, 0.75f * S, 0f), INSPECT_BULB_DIA, bulbMat);

        // 标签
        CreateLabel("Label_" + name, point.transform,
            new Vector3(0f, 0.95f * S, 0f),
            labelText, 0.018f * S, new Color(0.6f, 0.85f, 1f));

        // 触发器碰撞体
        BoxCollider trigger = point.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(0.6f * S, 0.6f * S, 0.6f * S);
        trigger.center = new Vector3(0f, 0.3f * S, 0f);

        // 区域触发器组件（自动向 TrainingManager 报告）
        PipelineZoneTrigger zoneTrigger = point.AddComponent<PipelineZoneTrigger>();
        zoneTrigger.zoneId = "InspectionZone";
        zoneTrigger.autoComplete = true;

        return point;
    }

    void BuildEStopStation(Transform root, Vector3 pos)
    {
        CreateEmergencyStopPanel("EStop_Station", root, pos,
            out eStopButton, out eStopRenderer);

        float S = SCALE_FACTOR;
        // 标签
        CreateLabel("Label_EStop", root,
            new Vector3(pos.x, pos.y + 1.0f * S, pos.z),
            "急停功能测试\nEMERGENCY STOP TEST", 0.022f * S, EStopRed, true);
    }

    void BuildShutdownStation(Transform root, Vector3 pos)
    {
        float S = SCALE_FACTOR;
        GameObject station = new GameObject("Shutdown_Station");
        station.transform.SetParent(root, false);
        station.transform.localPosition = pos;

        // 记录台
        Material deskMat = CreateMaterial("MAT_Desk", MetalMid, 0.60f, 0.35f);
        CreateCube("Shutdown_Desk", station.transform,
            new Vector3(0f, DESK_HEIGHT, 0f),
            new Vector3(DESK_WIDTH, DESK_THICKNESS, DESK_DEPTH), deskMat);

        // 桌腿
        Material legMat = CreateMaterial("MAT_Leg", MetalDark, 0.7f, 0.3f);
        for (int ix = -1; ix <= 1; ix += 2)
        {
            for (int iz = -1; iz <= 1; iz += 2)
            {
                CreateCylinder("DeskLeg_" + ix + "_" + iz, station.transform,
                    new Vector3(ix * 0.4f * S, 0.25f * S, iz * 0.2f * S),
                    DESK_LEG_DIA, DESK_HEIGHT, legMat, Axis.Y);
            }
        }

        // 提交按钮
        GameObject submitBtn = CreatePushButton("Shutdown_Confirm_Btn", station.transform,
            new Vector3(0f, 0.6f * S, 0f), out Transform btnTrans, "提交记录",
            new Color(0.2f, 0.8f, 0.3f));
        confirmButtons.Add(submitBtn);

        // 记录标签
        CreateLabel("Label_Shutdown", station.transform,
            new Vector3(0f, 0.85f * S, 0f),
            "操作记录台\nLOG STATION", 0.022f * S, Color.white, true);
    }

    // ═══════════════════════════════════════════════════════════
    //  标签与地面标记
    // ═══════════════════════════════════════════════════════════

    void BuildLabelsAndMarkers(Transform root)
    {
        float S = SCALE_FACTOR;
        // 管道流向箭头标记
        // 水平段 1 流向指示（→ +X）
        CreateFlowArrow(root, new Vector3(-8f * S, 0.02f, -3f * S), 0f);
        CreateFlowArrow(root, new Vector3(-6f * S, 0.02f, -3f * S), 0f);
        CreateFlowArrow(root, new Vector3(-4f * S, 0.02f, -3f * S), 0f);

        // 垂直段流向指示（→ +Z）
        CreateFlowArrow(root, new Vector3(-3f * S, 0.02f, -2f * S), 90f);
        CreateFlowArrow(root, new Vector3(-3f * S, 0.02f, 0f * S), 90f);
        CreateFlowArrow(root, new Vector3(-3f * S, 0.02f, 2f * S), 90f);
        CreateFlowArrow(root, new Vector3(-3f * S, 0.02f, 4f * S), 90f);

        // 水平段 2 流向指示（→ +X）
        CreateFlowArrow(root, new Vector3(-1f * S, 0.02f, 5f * S), 0f);
        CreateFlowArrow(root, new Vector3(1f * S, 0.02f, 5f * S), 0f);
        CreateFlowArrow(root, new Vector3(3f * S, 0.02f, 5f * S), 0f);

        // 场景标题
        CreateLabel("SceneTitle", root,
            new Vector3(-1f * S, 2.2f * S, -5.5f * S),
            "化工厂管道阀门操作培训", 0.06f * S, Color.white, true);

        // 图例
        string legend = "图例：  ● 阀门    ◎ 压力表    ▣ 流量计    ■ 急停";
        CreateLabel("Legend", root,
            new Vector3(-1f * S, 1.8f * S, -5.5f * S),
            legend, 0.02f * S, new Color(0.7f, 0.8f, 0.9f));
    }

    void CreateFlowArrow(Transform root, Vector3 pos, float yaw)
    {
        float S = SCALE_FACTOR;
        Material arrowMat = CreateEmissionMaterial("MAT_Arrow", new Color(0.3f, 0.8f, 0.4f), 0.5f);
        // 简单的三角形箭头用 Cube 近似
        GameObject arrow = new GameObject("FlowArrow");
        arrow.transform.SetParent(root, false);
        arrow.transform.localPosition = pos;
        arrow.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        CreateCube("ArrowHead", arrow.transform,
            new Vector3(0.15f * S, 0f, 0f),
            new Vector3(0.25f * S, 0.01f, 0.12f * S), arrowMat);
        CreateCube("ArrowTail", arrow.transform,
            new Vector3(-0.05f * S, 0f, 0.06f * S),
            new Vector3(0.12f * S, 0.01f, 0.04f * S), arrowMat);
        CreateCube("ArrowTail2", arrow.transform,
            new Vector3(-0.05f * S, 0f, -0.06f * S),
            new Vector3(0.12f * S, 0.01f, 0.04f * S), arrowMat);
    }

    // ═══════════════════════════════════════════════════════════
    //  TrainingManager 挂载与配置
    // ═══════════════════════════════════════════════════════════

    void AttachTrainingManager(GameObject root)
    {
        PipelineTrainingManager mgr = root.AddComponent<PipelineTrainingManager>();
        mgr.mode = TrainingMode.Guide;

        // 创建 UI 文本
        CreateTrainingUI(root, mgr);

        // 挂载运行时控制器（键盘交互 + 物理模拟）
        PipelineSceneRuntime runtime = root.AddComponent<PipelineSceneRuntime>();
        runtime.trainingManager = mgr;
        runtime.sceneBuilder = this;
        runtime.inletValveWheel = inletValveWheel;
        runtime.controlValveWheel = controlValveWheel;
        runtime.outletValveWheel = outletValveWheel;
        runtime.gaugeP1Needle = gaugeP1Needle;
        runtime.gaugeP2Needle = gaugeP2Needle;
        runtime.flowMeterDisplay = flowMeterDisplay;
        runtime.eStopButtonTransform = eStopButton;
        runtime.eStopButtonRenderer = eStopRenderer;

        // 创建玩家出生点
        CreatePlayerSpawn(root);

        Debug.Log("[PipelineChemicalSceneBuilder] TrainingManager + SceneRuntime 已配置（Guide 模式）。" +
                  "\n可通过 Inspector 切换为 Practice 模式。\n" +
                  "键盘操作：WASD 移动, Q/E 旋转阀门, F 交互, Space 急停, R 急停复位。");
    }

    void CreatePlayerSpawn(GameObject root)
    {
        float S = SCALE_FACTOR;
        // 在场景入口处创建玩家出生点标记
        GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.SetParent(root.transform, false);
        spawnPoint.transform.localPosition = new Vector3(-10f * S, 1.7f * S, -5.5f * S); // PPE 站前方

        // 视觉标记（半透明胶囊体）
        Material spawnMat = CreateMaterial("MAT_SpawnMarker", new Color(0f, 1f, 0.5f, 0.4f), 0f, 0.2f);
        if (spawnMat.HasProperty("_Surface"))
        {
            spawnMat.SetFloat("_Surface", 1f);
            spawnMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            spawnMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        CreateSphere("SpawnMarker", spawnPoint.transform,
            Vector3.zero, 0.3f * S, spawnMat);

        // 标签
        CreateLabel("SpawnLabel", spawnPoint.transform,
            new Vector3(0f, 0.5f * S, 0f),
            "▼ 入口 ▼", 0.03f * S, new Color(0f, 1f, 0.5f), true);
    }

    void CreateTrainingUI(GameObject root, PipelineTrainingManager mgr)
    {
        float S = SCALE_FACTOR;
        // 标题（独立，不放入面板）
        GameObject titleObj = CreateLabel("UI_Title", root.transform,
            new Vector3(-1f * S, 2.8f * S, -5f * S),
            "化工厂管道操作培训", 0.055f * S, Color.white, true);
        mgr.titleText = titleObj.GetComponent<TextMesh>();

        // ── UI 信息面板（Step + Instruction + Status 的父容器）──
        //     三条字幕统一放在同一平面上，整体绕 Y 轴面向玩家
        GameObject infoPanel = new GameObject("UI_InfoPanel");
        infoPanel.transform.SetParent(root.transform, false);
        infoPanel.transform.localPosition = new Vector3(5.5f * S, 1.5f * S, 2f * S);
        infoPanel.transform.localRotation = Quaternion.identity;

        // 步骤名（尺寸缩小：0.040 → 0.032）
        GameObject stepObj = CreateLabel("UI_Step", infoPanel.transform,
            new Vector3(0f, 0.9f * S, 0f),
            "", 0.032f * S, new Color(1f, 0.9f, 0.4f), true);
        mgr.stepText = stepObj.GetComponent<TextMesh>();

        // 操作说明（尺寸缩小：0.025 → 0.020）
        GameObject instObj = CreateLabel("UI_Instruction", infoPanel.transform,
            new Vector3(0f, 0.1f * S, 0f),
            "", 0.020f * S, new Color(0.8f, 0.9f, 1f));
        TextMesh instTm = instObj.GetComponent<TextMesh>();
        instTm.anchor = TextAnchor.UpperLeft;
        instTm.alignment = TextAlignment.Left;
        mgr.instructionText = instTm;

        // 状态（尺寸缩小：0.022 → 0.018）
        GameObject statusObj = CreateLabel("UI_Status", infoPanel.transform,
            new Vector3(0f, -1.0f * S, 0f),
            "", 0.018f * S, new Color(0.6f, 0.85f, 1f));
        TextMesh statusTm = statusObj.GetComponent<TextMesh>();
        statusTm.anchor = TextAnchor.UpperLeft;
        statusTm.alignment = TextAlignment.Left;
        mgr.statusText = statusTm;

        // OperationLogger
        OperationLogger logger = root.AddComponent<OperationLogger>();
        mgr.logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    //  Prefab 恢复：从模型层级中重新扫描交互对象引用
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 从已实例化的模型 GameObject 层级中，按命名规则找到所有交互对象，
    /// 重新填充 builder 的引用字段。用于 Prefab 加载后恢复引用。
    /// </summary>
    public void ReWireFromModel(GameObject modelRoot)
    {
        if (modelRoot == null) return;

        Transform[] allChildren = modelRoot.GetComponentsInChildren<Transform>(true);

        // ── 阀门手轮 ──────────────────────────────────────────
        inletValveWheel   = FindTransform(allChildren, "V1", "HandwheelRoot");
        controlValveWheel = FindTransform(allChildren, "V2", "HandwheelRoot");
        outletValveWheel  = FindTransform(allChildren, "V3", "HandwheelRoot");

        // ── 压力表指针 ────────────────────────────────────────
        gaugeP1Needle = FindTransform(allChildren, "P1", "NeedlePivot");
        gaugeP2Needle = FindTransform(allChildren, "P2", "NeedlePivot");

        // ── 流量计显示 ────────────────────────────────────────
        flowMeterDisplay = FindTextMesh(allChildren, "F1", "DisplayText");

        // ── 急停按钮 ──────────────────────────────────────────
        foreach (Transform t in allChildren)
        {
            string n = t.name;
            if (n.Contains("EStop") && n.Contains("_Button") && !n.Contains("ButtonBase"))
            {
                eStopButton = t;
                eStopRenderer = t.GetComponent<Renderer>();
                break;
            }
        }

        // ── 巡检点 ────────────────────────────────────────────
        inspectionZones.Clear();
        foreach (Transform t in allChildren)
        {
            if (t.name.StartsWith("InspectionPoint_"))
                inspectionZones.Add(t.gameObject);
        }

        // ── 确认按钮 ──────────────────────────────────────────
        confirmButtons.Clear();
        foreach (Transform t in allChildren)
        {
            if (t.name.Contains("Confirm") || t.name.EndsWith("_Btn"))
                confirmButtons.Add(t.gameObject);
        }

        Debug.Log("[PipelineChemicalSceneBuilder] 已从模型重新扫描交互对象引用。" +
            "\n  V1手轮: " + (inletValveWheel != null ? "√" : "✗") +
            "\n  V2手轮: " + (controlValveWheel != null ? "√" : "✗") +
            "\n  V3手轮: " + (outletValveWheel != null ? "√" : "✗") +
            "\n  P1指针: " + (gaugeP1Needle != null ? "√" : "✗") +
            "\n  P2指针: " + (gaugeP2Needle != null ? "√" : "✗") +
            "\n  流量计: " + (flowMeterDisplay != null ? "√" : "✗") +
            "\n  急停: " + (eStopButton != null ? "√" : "✗") +
            "\n  巡检点: " + inspectionZones.Count + " 个" +
            "\n  确认按钮: " + confirmButtons.Count + " 个");
    }

    private static Transform FindTransform(Transform[] all, string key1, string key2)
    {
        foreach (Transform t in all)
        {
            if (t.name.Contains(key1) && t.name.Contains(key2))
                return t;
        }
        return null;
    }

    private static TextMesh FindTextMesh(Transform[] all, string key1, string key2)
    {
        foreach (Transform t in all)
        {
            if (t.name.Contains(key1) && t.name.Contains(key2))
                return t.GetComponent<TextMesh>();
        }
        return null;
    }
}
