using UnityEngine;

/// <summary>
/// 化工管道培训场景 — 大型工厂仓库环境。
/// Play 直接进入，无需 Hub。WASD 移动，鼠标环视。
///
/// 环境特征：
///   - 40m×30m×14m 大跨度钢结构厂房
///   - 精简钢柱（仅两排靠墙）+ 屋顶桁架
///   - 高侧窗 + 天窗自然采光（模拟日光）
///   - 混凝土地面 + 波纹金属墙板
///   - 目前仅搭建环境框架，设备模型后续添加
/// </summary>
public class PipelineSceneBootstrap : MonoBehaviour
{
    // ═══════════════════ 厂房尺寸 ═══════════════════
    const float BayW = 40f;      // X 方向总宽
    const float BayD = 30f;      // Z 方向纵深
    const float BayH = 14f;      // 檐口高度（墙顶）
    const float PeakH = 16f;     // 屋脊高度
    const float WallT = 0.25f;   // 墙厚

    // 柱网坐标（精简：仅靠南北墙两排，每排3根）
    static readonly float[] ColX = { -15f, 0f, 15f };
    static readonly float[] ColZ = { -12f, 12f };

    // ═══════════════════ 材质色板 ═══════════════════
    // 混凝土 / 钢结构 / 墙面
    static readonly Color C_Concrete     = new Color(0.38f, 0.37f, 0.35f);
    static readonly Color C_ConcreteLine = new Color(0.30f, 0.29f, 0.28f);
    static readonly Color C_SteelCol     = new Color(0.28f, 0.30f, 0.33f);
    static readonly Color C_SteelBeam    = new Color(0.32f, 0.34f, 0.37f);
    static readonly Color C_WallPanel    = new Color(0.42f, 0.44f, 0.47f);
    static readonly Color C_WallSeam     = new Color(0.35f, 0.37f, 0.40f);
    static readonly Color C_RoofPanel    = new Color(0.36f, 0.38f, 0.41f);
    static readonly Color C_Skylight     = new Color(0.72f, 0.80f, 0.92f);
    static readonly Color C_WindowGlass  = new Color(0.55f, 0.68f, 0.85f);
    static readonly Color C_WindowFrame  = new Color(0.22f, 0.24f, 0.27f);
    static readonly Color C_BasePlate    = new Color(0.18f, 0.19f, 0.21f);

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupDaylight();
        BuildFloor();
        BuildWalls();
        BuildSteelFrame();
        BuildRoof();
        BuildSkylights();
        BuildHighWindows();
        BuildDetails();
        BuildPipelineEquipment();
        CreatePlayer();

        Debug.Log("[PipelineScene] 工厂厂房环境已就绪。"
            + BayW + "x" + BayD + "x" + BayH + "m · 钢结构 · 自然采光。");
    }

    // ═══════════════════ 自然日光 ═══════════════════
    void SetupDaylight()
    {
        // 环境光 — 模拟天空散射 + 地面反弹
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.46f, 0.52f); // 偏冷天光

        // ── 主日光（透过天窗/高窗斜射入厂房）──
        var sunGo = new GameObject("SunLight");
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.1f;
        sun.color = new Color(1f, 0.92f, 0.78f);   // 暖日光 ~4800K
        sun.shadowStrength = 0.55f;
        sun.shadows = LightShadows.Soft;
        sun.shadowNormalBias = 0.4f;
        // 太阳从东南方向斜射（模拟上午 10 点左右）
        sunGo.transform.rotation = Quaternion.Euler(48f, -140f, 0f);

        // ── 天光补光（模拟窗户/天窗散射进来的冷光）──
        var skyGo = new GameObject("SkyBounce");
        var sky = skyGo.AddComponent<Light>();
        sky.type = LightType.Directional;
        sky.intensity = 0.35f;
        sky.color = new Color(0.55f, 0.65f, 0.88f); // 蓝天散射冷光
        skyGo.transform.rotation = Quaternion.Euler(60f, -30f, 0f);

        // ── 地面反弹暖光 ──
        var groundGo = new GameObject("GroundBounce");
        var ground = groundGo.AddComponent<Light>();
        ground.type = LightType.Directional;
        ground.intensity = 0.20f;
        ground.color = new Color(0.55f, 0.45f, 0.32f); // 混凝土地面反射
        groundGo.transform.rotation = Quaternion.Euler(-35f, 60f, 0f);

        // ── 天窗下方点光源（模拟光束感）──
        for (float x = -14f; x <= 14f; x += 7f)
        {
            for (float z = -8f; z <= 8f; z += 8f)
            {
                var pt = new GameObject("SkylightGlow_" + x + "_" + z);
                var l = pt.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 12f;
                l.intensity = 0.12f;
                l.color = new Color(0.88f, 0.92f, 1f);
                l.shadows = LightShadows.None;
                pt.transform.position = new Vector3(x, PeakH - 0.5f, z);
            }
        }

        RenderSettings.reflectionIntensity = 0.40f;
        RenderSettings.reflectionBounces = 2;
    }

    // ═══════════════════ 混凝土地面 ═══════════════════
    void BuildFloor()
    {
        // 整体地板
        var floor = MakeBox("ConcreteFloor", null,
            new Vector3(0f, -0.2f, 0f),
            new Vector3(BayW + WallT * 2f, 0.4f, BayD + WallT * 2f));
        SetMat(floor, C_Concrete, 0.02f, 0.12f);
        // 保留 Collider 供 CharacterController 站立

        // 伸缩缝（避开正中间）
        foreach (float x in new[] { -15f, -7.5f, 7.5f, 15f })
        {
            var joint = MakeBox("ExpJoint_X", null,
                new Vector3(x, 0.01f, 0f),
                new Vector3(0.015f, 0.003f, BayD));
            Destroy(joint.GetComponent<Collider>());
            SetMat(joint, C_ConcreteLine, 0.01f, 0.08f);
        }
        for (float z = -10f; z <= 10f; z += 10f)
        {
            var joint = MakeBox("ExpJoint_Z", null,
                new Vector3(0f, 0.01f, z),
                new Vector3(BayW, 0.003f, 0.015f));
            Destroy(joint.GetComponent<Collider>());
            SetMat(joint, C_ConcreteLine, 0.01f, 0.08f);
        }
    }

    // ═══════════════════ 四面墙体 ═══════════════════
    void BuildWalls()
    {
        float hw = BayW / 2f, hd = BayD / 2f;
        float wh = BayH / 2f; // 墙半高

        // 北墙 Z+
        BuildWall("NorthWall", new Vector3(0f, wh, hd),
            new Vector3(BayW, BayH, WallT));
        // 南墙 Z-
        BuildWall("SouthWall", new Vector3(0f, wh, -hd),
            new Vector3(BayW, BayH, WallT));
        // 东墙 X+
        BuildWall("EastWall", new Vector3(hw, wh, 0f),
            new Vector3(WallT, BayH, BayD));
        // 西墙 X-
        BuildWall("WestWall", new Vector3(-hw, wh, 0f),
            new Vector3(WallT, BayH, BayD));
    }

    void BuildWall(string name, Vector3 pos, Vector3 scale)
    {
        var wall = MakeBox(name, null, pos, scale);
        SetMat(wall, C_WallPanel, 0.15f, 0.18f);

        // 波纹金属板竖缝（每 1m 一道，模拟波纹板拼缝）
        bool isXWall = scale.z < scale.x; // Z 方向薄 = 南北墙
        float length = isXWall ? scale.x : scale.z;
        float z = pos.z;
        float x = pos.x;
        float halfLen = length / 2f;
        float seamZ = z + (isXWall ? (z > 0 ? -WallT * 0.45f : WallT * 0.45f) : 0f);
        float seamX = !isXWall ? x + (x > 0 ? -WallT * 0.45f : WallT * 0.45f) : x;

        for (float s = -halfLen + 1f; s < halfLen; s += 1.05f)
        {
            var seam = MakeBox("Seam", null, Vector3.zero, Vector3.zero);
            Destroy(seam.GetComponent<Collider>());
            if (isXWall)
            {
                seam.transform.position = new Vector3(pos.x + s, pos.y, seamZ);
                seam.transform.localScale = new Vector3(0.012f, BayH - 0.3f, 0.004f);
            }
            else
            {
                seam.transform.position = new Vector3(seamX, pos.y, pos.z + s);
                seam.transform.localScale = new Vector3(0.004f, BayH - 0.3f, 0.012f);
            }
            SetMat(seam, C_WallSeam, 0.08f, 0.12f);
        }
    }

    // ═══════════════════ 钢结构框架（精简） ═══════════════════
    void BuildSteelFrame()
    {
        var frame = new GameObject("SteelFrame");

        float colW = 0.30f;

        // ── 立柱（3×2=6根，仅靠南北墙两排）──
        foreach (float cx in ColX)
        {
            foreach (float cz in ColZ)
            {
                var col = MakeBox("Column", frame,
                    new Vector3(cx, BayH / 2f, cz),
                    new Vector3(colW, BayH, colW));
                SetMat(col, C_SteelCol, 0.60f, 0.30f);

                // 柱脚底板
                var basePlate = MakeBox("BasePlate", frame,
                    new Vector3(cx, 0.04f, cz),
                    new Vector3(colW * 2f, 0.06f, colW * 2f));
                SetMat(basePlate, C_BasePlate, 0.70f, 0.35f);
            }
        }

        // ── 纵向主梁（沿X方向，连接同排柱顶）──
        foreach (float cz in ColZ)
        {
            float minX = ColX[0], maxX = ColX[ColX.Length - 1];
            var beam = MakeBox("BeamX", frame,
                new Vector3((minX + maxX) / 2f, BayH, cz),
                new Vector3(maxX - minX, 0.35f, 0.22f));
            SetMat(beam, C_SteelBeam, 0.62f, 0.32f);
        }

        // ── 横向主梁（沿Z方向，跨接南北排柱顶）──
        foreach (float cx in ColX)
        {
            float minZ = ColZ[0], maxZ = ColZ[ColZ.Length - 1];
            var beam = MakeBox("BeamZ", frame,
                new Vector3(cx, BayH, (minZ + maxZ) / 2f),
                new Vector3(0.22f, 0.35f, maxZ - minZ));
            SetMat(beam, C_SteelBeam, 0.62f, 0.32f);
        }
    }

    // ═══════════════════ 屋顶 ═══════════════════
    void BuildRoof()
    {
        // 主屋面（略高于檐口，微坡向两侧）
        var roofMain = MakeBox("RoofMain", null,
            new Vector3(0f, BayH + 0.6f, 0f),
            new Vector3(BayW + WallT * 2f, 0.15f, BayD + WallT * 2f));
        SetMat(roofMain, C_RoofPanel, 0.20f, 0.18f);

        // 屋脊隆起（沿X方向，屋顶中线）
        var ridge = MakeBox("Ridge", null,
            new Vector3(0f, PeakH - 0.1f, 0f),
            new Vector3(BayW + 1f, 0.25f, 3f));
        SetMat(ridge, C_RoofPanel, 0.25f, 0.20f);

        // 屋面檩条（每3m一道）
        for (float z = -12f; z <= 12f; z += 3f)
        {
            var purlin = MakeBox("Purlin", null,
                new Vector3(0f, BayH + 0.35f, z),
                new Vector3(BayW + 0.5f, 0.10f, 0.08f));
            SetMat(purlin, C_SteelBeam, 0.55f, 0.30f);
        }
    }

    // ═══════════════════ 天窗（屋脊两侧） ═══════════════════
    void BuildSkylights()
    {
        // 天窗透光面板 — 沿屋脊两侧各一排
        for (int side = 0; side < 2; side++)
        {
            float zOff = side == 0 ? -2f : 2f;
            for (float x = -14f; x <= 14f; x += 7f)
            {
                // 透光面板
                var panel = MakeBox("SkylightPanel", null,
                    new Vector3(x, PeakH - 0.05f, zOff),
                    new Vector3(1.8f, 0.04f, 1.2f));
                SetMatEmissive(panel, C_Skylight, 0.50f);

                // 窗框
                var frame = MakeBox("SkylightFrame", null,
                    new Vector3(x, PeakH - 0.08f, zOff),
                    new Vector3(2.0f, 0.015f, 1.4f));
                SetMat(frame, C_WindowFrame, 0.70f, 0.40f);

                // 天窗竖框
                var mullion = MakeBox("Mullion", null,
                    new Vector3(x, PeakH - 0.08f, zOff),
                    new Vector3(0.04f, 0.015f, 1.4f));
                SetMat(mullion, C_WindowFrame, 0.70f, 0.40f);
            }
        }
    }

    // ═══════════════════ 高侧窗 ═══════════════════
    void BuildHighWindows()
    {
        float winBotY = 8f;
        float winTopY = BayH - 0.8f;
        float winMidY = (winBotY + winTopY) / 2f;
        float winH = winTopY - winBotY;

        // 南北墙高窗（长条带窗，柱间分布）
        foreach (float z in new[] { BayD / 2f, -BayD / 2f })
        {
            float zOff = z > 0 ? z - WallT * 0.55f : z + WallT * 0.55f;

            for (int i = 0; i < ColX.Length - 1; i++)
            {
                float cx = (ColX[i] + ColX[i + 1]) / 2f;
                float spacing = Mathf.Abs(ColX[i + 1] - ColX[i]);
                float w = spacing * 0.75f;

                var glass = MakeBox("Glass", null,
                    new Vector3(cx, winMidY, zOff),
                    new Vector3(w, winH, 0.008f));
                SetMatEmissive(glass, C_WindowGlass, 0.15f);

                var frame = MakeBox("WinFrame", null,
                    new Vector3(cx, winMidY, zOff + (z > 0 ? -0.02f : 0.02f)),
                    new Vector3(w + 0.15f, winH + 0.15f, 0.01f));
                SetMat(frame, C_WindowFrame, 0.70f, 0.40f);
            }
        }

        // 东西墙高窗
        foreach (float x in new[] { BayW / 2f, -BayW / 2f })
        {
            float xOff = x > 0 ? x - WallT * 0.55f : x + WallT * 0.55f;
            float spacing = Mathf.Abs(ColZ[1] - ColZ[0]);
            float w = spacing * 0.75f;
            float cz = 0f;

            var glass = MakeBox("Glass", null,
                new Vector3(xOff, winMidY, cz),
                new Vector3(0.008f, winH, w));
            SetMatEmissive(glass, C_WindowGlass, 0.15f);

            var frame = MakeBox("WinFrame", null,
                new Vector3(xOff + (x > 0 ? -0.02f : 0.02f), winMidY, cz),
                new Vector3(0.01f, winH + 0.15f, w + 0.15f));
            SetMat(frame, C_WindowFrame, 0.70f, 0.40f);
        }
    }

    // ═══════════════════ 工业细节 ═══════════════════
    void BuildDetails()
    {
        // ── 屋顶通风脊（沿屋脊线的凸起）──
        var ventRidge = MakeBox("VentRidge", null,
            new Vector3(0f, PeakH + 0.15f, 0f),
            new Vector3(BayW - 4f, 0.20f, 1.5f));
        SetMat(ventRidge, C_RoofPanel, 0.30f, 0.22f);

        // ── 地面柱脚护墩（黄色警示）──
        foreach (float cx in ColX)
        {
            foreach (float cz in ColZ)
            {
                var bollard = MakeBox("Bollard", null,
                    new Vector3(cx, 0.15f, cz),
                    new Vector3(0.50f, 0.35f, 0.50f));
                SetMat(bollard, new Color(0.82f, 0.72f, 0.12f), 0.05f, 0.25f);
            }
        }
    }

    // ═══════════════════ 化工管道设备 ═══════════════════
    void BuildPipelineEquipment()
    {
        // ═══════════════════ 加载 Resources 中所有管线 prefab ═══════════════════
        var pf01 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_01"); //短直管
        var pf02 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_02");
        var pf03 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_03");
        var pf04 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_04"); //长直管
        var pf05 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_05");
        var pf06 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_06"); //细十字
        var pf07 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_07");
        var pf08 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_08");
        var pf09 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_09");
        var pf10 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_10"); //长U管
        var pf11 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_11"); //短U管
        var pf12 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_12"); //粗十字
        var pf13 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_13"); //细T
        var pf14 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_14"); //粗T
        // 法兰 / 垫片（Pipe_15~16）
        var pf15 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_15"); // 法兰圈
        var pf16 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_16"); // 垫片
        // 阀门
        var pf18 = Resources.Load<GameObject>("PipelinePrefabs/Pipe_18");
        // 仪表
        var pfGauge = Resources.Load<GameObject>("PipelinePrefabs/Manometr"); // 压力表

        var root = new GameObject("PipelineEquipment");

        // ═══════════════════ 布局参数 ═══════════════════
        float pipeY = 0.8f;   // 管道中心高度（便于培训操作）
        float halfW = 2f;     // 环道半宽 X
        float halfD = 3f;     // 环道半深 Z

        // 旋转预设（prefab 本地 Z 轴 = 管道走向）
        Quaternion R_NS = Quaternion.identity;                    // 南北走向（不变）
        Quaternion R_EW = Quaternion.Euler(0f, 90f, 0f);         // 东西走向
        Quaternion R_FL = Quaternion.Euler(0f, 0f, 0f);          // 法兰（跟管走向）

        PlacePrefab(pf09, new Vector3(-13f, 0.65f,  5f), Quaternion.Euler(0f, -90f, -90f), root);

    }

    /// 实例化 prefab + 可选 pivot 补偿偏移
    /// autoCalibrate=true 时自动读取子 mesh bounds 中心补偿偏移
    static GameObject PlacePrefab(GameObject prefab, Vector3 pos, Quaternion rot, GameObject parent,
        bool autoCalibrate = false)
    {
        if (prefab == null) return null;
        var go = Instantiate(prefab, pos, rot);
        go.name = prefab.name;

        // ── 诊断: 打印 mesh 几何中心 vs transform 原点的偏移 ──
        Vector3 meshCenter = GetMeshCenter(go);
        if (meshCenter.magnitude > 0.005f)
        {
            Debug.Log($"[Pipeline] {prefab.name} mesh偏移="
                + $"({meshCenter.x:F3}, {meshCenter.y:F3}, {meshCenter.z:F3}) "
                + $"长度={meshCenter.magnitude:F3}m — "
                + $"若视觉位置与Editor不符, 在代码中将 pos 减去此偏移量");
        }

        // ── 可选: 自动补偿 pivot 偏移 ──
        if (autoCalibrate && meshCenter.magnitude > 0.01f)
            go.transform.position = pos - meshCenter;

        if (parent != null) go.transform.SetParent(parent.transform, true);
        return go;
    }

    /// 计算 GameObject 下所有 MeshFilter 的组合包围盒中心（本地坐标）
    static Vector3 GetMeshCenter(GameObject go)
    {
        var mfs = go.GetComponentsInChildren<MeshFilter>();
        if (mfs.Length == 0) return Vector3.zero;

        Bounds combined = mfs[0].sharedMesh.bounds;
        // 转换第一个 mesh 的 bounds 到世界空间来计算
        Matrix4x4 m0 = mfs[0].transform.localToWorldMatrix;
        combined = TransformBounds(mfs[0].sharedMesh.bounds, m0);

        for (int i = 1; i < mfs.Length; i++)
        {
            Matrix4x4 m = mfs[i].transform.localToWorldMatrix;
            Bounds wb = TransformBounds(mfs[i].sharedMesh.bounds, m);
            combined.Encapsulate(wb);
        }

        // 返回世界空间 bounds 中心相对于 go.transform.position 的偏移
        return combined.center - go.transform.position;
    }

    static Bounds TransformBounds(Bounds b, Matrix4x4 m)
    {
        Vector3 center = m.MultiplyPoint3x4(b.center);
        // 用旋转后的 extents 保守近似
        Vector3 ext = b.extents;
        Vector3 ax = m.MultiplyVector(new Vector3(ext.x, 0, 0));
        Vector3 ay = m.MultiplyVector(new Vector3(0, ext.y, 0));
        Vector3 az = m.MultiplyVector(new Vector3(0, 0, ext.z));
        ext = new Vector3(
            Mathf.Abs(ax.x) + Mathf.Abs(ay.x) + Mathf.Abs(az.x),
            Mathf.Abs(ax.y) + Mathf.Abs(ay.y) + Mathf.Abs(az.y),
            Mathf.Abs(ax.z) + Mathf.Abs(ay.z) + Mathf.Abs(az.z));
        return new Bounds(center, ext * 2f);
    }

    // ═══════════════════ 玩家 ═══════════════════
    void CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        // 站在厂房中央偏南，面朝北
        player.transform.position = new Vector3(0f, 0f, -8f);

        var cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 1f, 0f);
        cc.radius = 0.35f;
        cc.height = 2f;
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;

        var hpc = player.AddComponent<HubPlayerController>();
        hpc.walkSpeed = 6f;
        hpc.mouseSensitivity = 2.5f;

        // 摄像机
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.SetParent(player.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 1.55f, 0f);
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 78f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 80f;
        cam.clearFlags = CameraClearFlags.Skybox;
    }

    // ═══════════════════ 工具方法 ═══════════════════
    static GameObject MakeBox(string name, GameObject parent, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent.transform, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        return go;
    }

    static void SetMat(GameObject go, Color color, float metallic, float smoothness)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var mat = new Material(Shader.Find("Standard"));
        mat.SetColor("_Color", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", smoothness);
        r.material = mat;
    }

    static void SetMatEmissive(GameObject go, Color color, float intensity)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var mat = new Material(Shader.Find("Standard"));
        mat.SetColor("_Color", color);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        r.material = mat;
    }
}
