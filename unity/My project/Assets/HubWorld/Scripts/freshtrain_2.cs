using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class freshtrain_2 : MonoBehaviour
{
    const float FloorY = 0f;
    static readonly Vector3 DefaultPracticeSpawnPosition = new Vector3(0f, 1f, 6f);
    const float DefaultPracticeSpawnYaw = 0f;
    const float DefaultPracticeSpawnPitch = 18f;

    public string returnSceneName = "entry";
    public bool hideExistingFactoryRoot = true;

    void Start()
    {
        DisableExistingSceneRuntime();

        var session = EnsureSession();
        var handTracking = MockHandTrackingService.EnsureExists();

        SetupLighting();
        SetupHall();
        CreateFactoryScenery();
        CreateReadyStatusHud(handTracking);
        CreateTaskStations(session.assignedTasks);
        CreateReturnToHallExit();
        CreatePlayer(session);
    }

    void DisableExistingSceneRuntime()
    {
        foreach (var camera in FindObjectsOfType<Camera>())
            camera.gameObject.SetActive(false);

        foreach (var listener in FindObjectsOfType<AudioListener>())
            listener.enabled = false;

        if (!hideExistingFactoryRoot) return;

        var factory = GameObject.Find("Factory 1 Static");
        if (factory != null)
            factory.SetActive(false);
    }

    static SessionManager EnsureSession()
    {
        var session = SessionManager.EnsureExists();
        var taskSource = new MockAssignedTaskSource();
        session.Initialize(taskSource);
        session.assignedTasks = taskSource.GetAssignedTasks();
        return session;
    }

    static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.36f, 0.38f, 0.42f);

        var lightGo = new GameObject("FreshTrain2 Key Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.72f;
        light.color = new Color(1.0f, 0.96f, 0.88f);
        lightGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

        var fillGo = new GameObject("FreshTrain2 Cool Fill Light");
        var fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.28f;
        fill.color = new Color(0.58f, 0.70f, 1.0f);
        fillGo.transform.rotation = Quaternion.Euler(-18f, 42f, 0f);
    }

    static void SetupHall()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "FreshTrain2 Hub Floor";
        floor.transform.position = new Vector3(0f, -0.05f, 0f);
        floor.transform.localScale = new Vector3(56f, 0.1f, 38f);
        SetColor(floor, new Color(0.20f, 0.22f, 0.25f));

        CreateFloorTileGrid();
        CreateCeiling();

        CreateWall("FreshTrain2 North Wall", new Vector3(0f, 1.8f, 19f), new Vector3(56f, 3.6f, 0.25f));
        CreateWall("FreshTrain2 South Wall", new Vector3(0f, 1.8f, -19f), new Vector3(56f, 3.6f, 0.25f));
        CreateWall("FreshTrain2 East Wall", new Vector3(28f, 1.8f, 0f), new Vector3(0.25f, 3.6f, 38f));
        CreateWall("FreshTrain2 West Wall", new Vector3(-28f, 1.8f, 0f), new Vector3(0.25f, 3.6f, 38f));

        for (int i = 0; i < 10; i++)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "FreshTrain2 Hub Pillar " + (i + 1);
            pillar.transform.position = new Vector3(-22f + i * 4.9f, 1.8f, i % 2 == 0 ? -14.8f : 14.8f);
            pillar.transform.localScale = new Vector3(0.72f, 1.8f, 0.72f);
            SetColor(pillar, new Color(0.30f, 0.33f, 0.37f));
        }

        CreateVisualZones();
        CreateFloorGuide("Main Walkway Left", -3.8f, 0f, 34f);
        CreateFloorGuide("Main Walkway Right", 3.8f, 0f, 34f);
        CreateFloorGuideX("North Task Lane", 0f, 11.4f, 48f);
        CreateFloorGuideX("South Task Lane", 0f, -11.4f, 48f);

    }

    static void CreateFloorTileGrid()
    {
        for (int x = -24; x <= 24; x += 4)
            CreateFloorLine("FreshTrain2 Floor Grid X " + x, new Vector3(x, FloorY + 0.006f, 0f), new Vector3(0.035f, 0.012f, 36.0f));

        for (int z = -16; z <= 16; z += 4)
            CreateFloorLine("FreshTrain2 Floor Grid Z " + z, new Vector3(0f, FloorY + 0.007f, z), new Vector3(52.0f, 0.012f, 0.035f));
    }

    static void CreateFloorLine(string name, Vector3 position, Vector3 scale)
    {
        var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = name;
        line.transform.position = position;
        line.transform.localScale = scale;
        SetColor(line, new Color(0.27f, 0.30f, 0.34f));
        RemoveCollider(line);
    }

    static void CreateCeiling()
    {
        CreateRoofBeam("Roof Beam North", new Vector3(0f, 3.92f, 17.8f), new Vector3(54f, 0.12f, 0.22f));
        CreateRoofBeam("Roof Beam South", new Vector3(0f, 3.92f, -17.8f), new Vector3(54f, 0.12f, 0.22f));
        CreateRoofBeam("Roof Beam East", new Vector3(26.8f, 3.92f, 0f), new Vector3(0.22f, 0.12f, 36f));
        CreateRoofBeam("Roof Beam West", new Vector3(-26.8f, 3.92f, 0f), new Vector3(0.22f, 0.12f, 36f));

        for (int i = -2; i <= 2; i++)
            CreateRoofBeam("Roof Truss " + i, new Vector3(i * 9.5f, 3.86f, 0f), new Vector3(0.16f, 0.14f, 35.2f));
    }

    static void CreateRoofBeam(string name, Vector3 position, Vector3 scale)
    {
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "FreshTrain2 " + name;
        beam.transform.position = position;
        beam.transform.localScale = scale;
        SetColor(beam, new Color(0.15f, 0.17f, 0.20f));
        RemoveCollider(beam);
    }

    static void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        SetColor(wall, new Color(0.12f, 0.14f, 0.17f));
    }

    static void CreateVisualZones()
    {
        CreateZoneBand("FreshTrain2 Entry Staging Zone", new Vector3(0f, 0.012f, -7.2f), new Vector3(22f, 0.018f, 5.8f), new Color(0.07f, 0.18f, 0.23f), "操作入口");
        CreateZoneBand("FreshTrain2 Production Spine Zone", new Vector3(0f, 0.014f, 3.2f), new Vector3(40f, 0.016f, 8.4f), new Color(0.16f, 0.18f, 0.20f), "生产线");
        CreateZoneBand("FreshTrain2 Left Training Lane Zone", new Vector3(-22.5f, 0.016f, 0.2f), new Vector3(5.4f, 0.018f, 28.8f), new Color(0.09f, 0.17f, 0.24f), "基础工位");
        CreateZoneBand("FreshTrain2 Right Training Lane Zone", new Vector3(22.5f, 0.016f, 0.2f), new Vector3(5.4f, 0.018f, 28.8f), new Color(0.13f, 0.18f, 0.15f), "进阶工位");

        CreateOverheadSign("新手教学区", "选择工位  /  进入操作  /  完成训练", new Vector3(0f, 3.05f, -13.9f), new Color(0.08f, 0.28f, 0.36f), new Color(0.12f, 0.72f, 0.94f));
        CreateOverheadSign("自由练习工位", "靠近蓝色工位后按 E 进入训练", new Vector3(0f, 3.12f, 14.1f), new Color(0.13f, 0.18f, 0.22f), new Color(1f, 0.76f, 0.16f));
        CreateLaneHeader("基础操作", new Vector3(-22.5f, 2.75f, 15.25f), new Color(0.10f, 0.34f, 0.48f));
        CreateLaneHeader("综合训练", new Vector3(22.5f, 2.75f, 15.25f), new Color(0.18f, 0.36f, 0.23f));
        CreateWallAccentPanels();
    }

    static void CreateZoneBand(string name, Vector3 position, Vector3 scale, Color color, string label)
    {
        var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
        band.name = name;
        band.transform.position = position;
        band.transform.localScale = scale;
        SetColor(band, color);
        RemoveCollider(band);

        CreateZoneEdge(name + " North Edge", position + new Vector3(0f, 0.012f, scale.z * 0.5f), new Vector3(scale.x, 0.022f, 0.055f));
        CreateZoneEdge(name + " South Edge", position + new Vector3(0f, 0.012f, -scale.z * 0.5f), new Vector3(scale.x, 0.022f, 0.055f));
        CreateZoneEdge(name + " West Edge", position + new Vector3(-scale.x * 0.5f, 0.012f, 0f), new Vector3(0.055f, 0.022f, scale.z));
        CreateZoneEdge(name + " East Edge", position + new Vector3(scale.x * 0.5f, 0.012f, 0f), new Vector3(0.055f, 0.022f, scale.z));

        var labelGo = new GameObject(name + " Floor Label");
        labelGo.transform.position = position + new Vector3(0f, 0.035f, -scale.z * 0.28f);
        labelGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var text = labelGo.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.22f;
        text.fontSize = 54;
        text.color = new Color(0.84f, 0.91f, 0.94f);
    }

    static void CreateZoneEdge(string name, Vector3 position, Vector3 scale)
    {
        var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edge.name = name;
        edge.transform.position = position;
        edge.transform.localScale = scale;
        SetColor(edge, new Color(1f, 0.74f, 0.12f));
        RemoveCollider(edge);
    }

    static void CreateOverheadSign(string title, string subtitle, Vector3 position, Color plateColor, Color accentColor)
    {
        var root = new GameObject("FreshTrain2 Sign " + title);
        root.transform.position = position;
        root.AddComponent<FreshTrain2FaceCameraLabel>();

        var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = title + " Sign Plate";
        plate.transform.SetParent(root.transform, false);
        plate.transform.localPosition = Vector3.zero;
        plate.transform.localScale = new Vector3(6.4f, 0.72f, 0.08f);
        SetColor(plate, plateColor);
        RemoveCollider(plate);

        var accent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        accent.name = title + " Sign Accent";
        accent.transform.SetParent(root.transform, false);
        accent.transform.localPosition = new Vector3(0f, -0.41f, 0.055f);
        accent.transform.localScale = new Vector3(6.2f, 0.07f, 0.04f);
        SetColor(accent, accentColor);
        RemoveCollider(accent);

        CreateSignText(root.transform, title, new Vector3(0f, 0.13f, 0.08f), 0.16f, 64, Color.white);
        CreateSignText(root.transform, subtitle, new Vector3(0f, -0.18f, 0.08f), 0.062f, 30, new Color(0.78f, 0.88f, 0.92f));
    }

    static void CreateLaneHeader(string title, Vector3 position, Color color)
    {
        var root = new GameObject("FreshTrain2 Lane Header " + title);
        root.transform.position = position;
        root.AddComponent<FreshTrain2FaceCameraLabel>();

        var leftPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPost.name = title + " Header Left Post";
        leftPost.transform.SetParent(root.transform, false);
        leftPost.transform.localPosition = new Vector3(-1.5f, -0.48f, 0f);
        leftPost.transform.localScale = new Vector3(0.12f, 0.95f, 0.12f);
        SetColor(leftPost, Color.Lerp(color, Color.white, 0.10f));
        RemoveCollider(leftPost);

        var rightPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPost.name = title + " Header Right Post";
        rightPost.transform.SetParent(root.transform, false);
        rightPost.transform.localPosition = new Vector3(1.5f, -0.48f, 0f);
        rightPost.transform.localScale = new Vector3(0.12f, 0.95f, 0.12f);
        SetColor(rightPost, Color.Lerp(color, Color.white, 0.10f));
        RemoveCollider(rightPost);

        var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = title + " Header Beam";
        beam.transform.SetParent(root.transform, false);
        beam.transform.localPosition = new Vector3(0f, 0f, 0f);
        beam.transform.localScale = new Vector3(3.35f, 0.36f, 0.08f);
        SetColor(beam, color);
        RemoveCollider(beam);

        CreateSignText(root.transform, title, new Vector3(0f, 0.01f, 0.07f), 0.105f, 44, Color.white);
    }

    static void CreateSignText(Transform parent, string textValue, Vector3 localPosition, float characterSize, int fontSize, Color color)
    {
        var textGo = new GameObject(textValue + " Text");
        textGo.transform.SetParent(parent, false);
        textGo.transform.localPosition = localPosition;
        var text = textGo.AddComponent<TextMesh>();
        text.text = textValue;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = characterSize;
        text.fontSize = fontSize;
        text.color = color;
    }

    static void CreateWallAccentPanels()
    {
        for (int i = 0; i < 6; i++)
        {
            float x = -20f + i * 8f;
            CreateWallPanel("FreshTrain2 North Wall Panel " + i, new Vector3(x, 2.25f, 18.83f), new Vector3(4.2f, 0.92f, 0.08f), i % 2 == 0 ? new Color(0.08f, 0.26f, 0.34f) : new Color(0.18f, 0.22f, 0.26f));
            CreateWallPanel("FreshTrain2 South Wall Panel " + i, new Vector3(x, 2.25f, -18.83f), new Vector3(4.2f, 0.92f, 0.08f), i % 2 == 0 ? new Color(0.20f, 0.18f, 0.12f) : new Color(0.18f, 0.22f, 0.26f));
        }
    }

    static void CreateWallPanel(string name, Vector3 position, Vector3 scale, Color color)
    {
        var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = name;
        panel.transform.position = position;
        panel.transform.localScale = scale;
        SetColor(panel, color);
        RemoveCollider(panel);
    }

    static void CreateFloorGuide(string name, float x, float centerZ, float length)
    {
        for (int i = 0; i < 26; i++)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "FreshTrain2 " + name + " " + i;
            stripe.transform.position = new Vector3(x, 0.015f, centerZ - length * 0.5f + i * length / 25f);
            stripe.transform.localScale = new Vector3(0.12f, 0.035f, 0.62f);
            SetColor(stripe, new Color(1f, 0.76f, 0.12f));
            RemoveCollider(stripe);
        }
    }

    static void CreateFloorGuideX(string name, float centerX, float z, float length)
    {
        for (int i = 0; i < 30; i++)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "FreshTrain2 " + name + " " + i;
            stripe.transform.position = new Vector3(centerX - length * 0.5f + i * length / 29f, 0.02f, z);
            stripe.transform.localScale = new Vector3(0.68f, 0.035f, 0.12f);
            SetColor(stripe, new Color(1f, 0.76f, 0.12f));
            RemoveCollider(stripe);
        }
    }

    static void CreateEquipmentCluster(Vector3 origin, string label, Color color)
    {
        var root = new GameObject("FreshTrain2 " + label + " Interactive Cluster");
        root.transform.position = origin;

        var renderers = new Renderer[3];
        for (int i = 0; i < 3; i++)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = label + " Box " + i;
            box.transform.SetParent(root.transform, false);
            box.transform.localPosition = new Vector3((i % 3) * 0.62f, 0.21f, 0f);
            box.transform.localScale = new Vector3(0.46f, 0.42f, 0.46f);
            SetColor(box, Color.Lerp(color, Color.black, 0.12f));
            renderers[i] = box.GetComponent<Renderer>();
        }

        var sign = new GameObject(label + " Label");
        sign.transform.SetParent(root.transform, false);
        sign.transform.localPosition = new Vector3(0.85f, 1.75f, -0.30f);
        sign.AddComponent<FreshTrain2FaceCameraLabel>();
        var text = sign.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.08f;
        text.fontSize = 34;
        text.color = Color.white;

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            label.Contains("成品") ? FreshTrain2InteractiveElementType.StorageCheckIn : FreshTrain2InteractiveElementType.MaintenanceBench,
            label,
            label.Contains("成品") ? "核对成品数量并完成入库确认" : "检查待处理物料和工具准备状态",
            label.Contains("成品") ? "入库" : "检查",
            "等待确认",
            "状态已确认",
            "靠近后按 F 记录当前区域状态",
            root.transform,
            renderers,
            null,
            label.Contains("成品") ? "成品缓存与入库确认" : "物料暂存与作业准备",
            "确认次数、误操作、处理用时");
        element.ConfigureTrainingLink(label.Contains("成品") ? "storage_checkin" : "material_transfer", label.Contains("成品") ? "成品入库确认训练" : "物料搬运训练", "SampleScene", label.Contains("成品") ? 16 : 7);
        element.interactionRadius = 4.6f;

        var trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0.62f, 0.8f, 0f);
        trigger.size = new Vector3(2.8f, 1.9f, 2.2f);
    }

    static void CreateControlPanel(Vector3 origin)
    {
        var root = new GameObject("FreshTrain2 Central Control Interactive Panel");
        root.transform.position = origin;

        var cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabinet.name = "Central Control Cabinet";
        cabinet.transform.SetParent(root.transform, false);
        cabinet.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        cabinet.transform.localScale = new Vector3(1.8f, 1.3f, 0.28f);
        SetColor(cabinet, new Color(0.30f, 0.34f, 0.38f));

        var screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "Central Control Screen";
        screen.transform.SetParent(root.transform, false);
        screen.transform.localPosition = new Vector3(0f, 1.22f, -0.18f);
        screen.transform.localScale = new Vector3(1.15f, 0.36f, 0.05f);
        SetColor(screen, new Color(0.03f, 0.09f, 0.12f));

        var label = new GameObject("Central Control Label");
        label.transform.SetParent(root.transform, false);
        label.transform.localPosition = new Vector3(0f, 2.35f, -0.34f);
        label.AddComponent<FreshTrain2FaceCameraLabel>();
        var text = label.AddComponent<TextMesh>();
        text.text = "中央控制台";
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.08f;
        text.fontSize = 34;
        text.color = Color.white;

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.ControlPanel,
            "中央控制台",
            "查看训练区域状态，推进设备点检流程",
            "点检",
            "等待点检",
            "控制台已确认",
            "靠近控制台按 F，可依次切换电源、急停、联锁检查状态",
            screen.transform,
            new[] { screen.GetComponent<Renderer>() },
            null,
            "训练前设备状态确认",
            "点检步骤、遗漏项、确认用时");
        element.ConfigureTrainingLink("central_control", "中央控制台点检", "SampleScene", 18);
        element.interactionRadius = 4.8f;

        var trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.1f, 0f);
        trigger.size = new Vector3(2.8f, 2.4f, 2.2f);
    }

    void CreatePlayer(SessionManager session)
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "FreshTrain2 Hub Player";
        player.transform.position = session.hasHubReturnPosition ? session.hubReturnPosition : DefaultPracticeSpawnPosition;
        player.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        SetColor(player, new Color(0.18f, 0.56f, 0.82f));

        var capsuleCollider = player.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null) Destroy(capsuleCollider);

        var controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.38f;
        controller.center = Vector3.zero;

        var cameraGo = new GameObject("FreshTrain2 Follow Camera");
        cameraGo.tag = "MainCamera";
        var camera = cameraGo.AddComponent<Camera>();
        camera.fieldOfView = 62f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.055f, 0.070f, 0.090f);
        cameraGo.AddComponent<AudioListener>();

        var playerController = player.AddComponent<FreshTrain2PlayerController>();
        playerController.followCamera = cameraGo.transform;
        playerController.moveSpeed = 6.2f;
        playerController.cameraDistance = 7.0f;
        playerController.cameraHeight = 3.0f;
        playerController.initialYaw = DefaultPracticeSpawnYaw;
        playerController.initialPitch = DefaultPracticeSpawnPitch;
        playerController.worldBoundsX = new Vector2(-26.5f, 26.5f);
        playerController.worldBoundsZ = new Vector2(-17.5f, 17.5f);

        var interactor = player.AddComponent<FreshTrain2HubInteractor>();
        interactor.interactionRadius = 3.0f;

    }

    static void CreateTaskStations(IReadOnlyList<AssignedTask> assignedTasks)
    {
        var spawnerGo = new GameObject("FreshTrain2 TaskStationSpawner");
        var spawner = spawnerGo.AddComponent<FreshTrain2TaskStationSpawner>();
        spawner.SpawnStations(assignedTasks);
    }

    static void CreateFactoryScenery()
    {
        var sceneryGo = new GameObject("FreshTrain2 FactoryScenery");
        sceneryGo.AddComponent<FreshTrain2FactoryScenery>();
    }

    static void CreateHallInteractiveElements()
    {
        var builderGo = new GameObject("FreshTrain2 HallInteractiveElements");
        var builder = builderGo.AddComponent<FreshTrain2InteractiveSceneBuilder>();
        builder.useLocalLayout = false;
        builder.layoutScale = 1f;
        builder.equipmentScale = 1.0f;
        builder.buildRouteMarkers = false;
        builder.compactTrainingEntranceLayout = true;
    }

    void CreateReturnToHallExit()
    {
        var exitGo = new GameObject("FreshTrain2 Return To Hall Exit");
        exitGo.transform.position = new Vector3(0f, 0f, 16.3f);
        var exit = exitGo.AddComponent<FreshTrain2ReturnExit>();
        exit.hallSceneName = returnSceneName;
        exit.triggerRadius = 3.8f;
        exit.holdSeconds = 1.0f;
        exit.confirmKey = KeyCode.F;
        exit.alternateConfirmKey = KeyCode.Space;
    }

    static void CreateReadyStatusHud(IHandTrackingService handTrackingService)
    {
        var hudGo = new GameObject("FreshTrain2 ReadyStatusHud");
        var hud = hudGo.AddComponent<ReadyStatusHud>();
        hud.Initialize(handTrackingService);
    }

    static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static void RemoveCollider(GameObject go)
    {
        var collider = go.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
    }
}

public class FreshTrain2TaskStationSpawner : MonoBehaviour
{
    static readonly Vector3[] AnchorPositions =
    {
        new Vector3(-22.5f, 0f, 12.6f),
        new Vector3(-22.5f, 0f, 6.4f),
        new Vector3(-22.5f, 0f, 0.2f),
        new Vector3(-22.5f, 0f, -6.0f),
        new Vector3(-22.5f, 0f, -12.2f),
        new Vector3(22.5f, 0f, 12.6f),
        new Vector3(22.5f, 0f, 6.4f),
        new Vector3(22.5f, 0f, 0.2f),
        new Vector3(22.5f, 0f, -6.0f),
        new Vector3(22.5f, 0f, -12.2f),
    };

    static readonly string[] PlannedStationNames =
    {
        "旋转阀门",
        "拉杆电闸",
        "传送分拣",
        "急停复位",
        "按钮点击",
        "滑块校准",
        "螺栓拧紧",
        "物料搬运",
        "档位选择",
        "综合考核",
    };

    public void SpawnStations(IReadOnlyList<AssignedTask> assignedTasks)
    {
        var tasksByAnchor = new Dictionary<int, AssignedTask>();
        if (assignedTasks != null)
        {
            foreach (var task in assignedTasks)
            {
                if (task == null) continue;
                if (task.anchorId < 0 || task.anchorId >= AnchorPositions.Length) continue;
                tasksByAnchor[task.anchorId] = task;
            }
        }

        for (int anchorId = 0; anchorId < AnchorPositions.Length; anchorId++)
        {
            var stationGo = new GameObject("FreshTrain2 TaskStation Anchor " + anchorId);
            stationGo.transform.SetParent(transform, false);
            stationGo.transform.localPosition = AnchorPositions[anchorId];

            var station = stationGo.AddComponent<FreshTrain2TaskStation>();
            if (tasksByAnchor.TryGetValue(anchorId, out var task))
                station.Configure(task, anchorId);
            else
                station.ConfigureLocked(anchorId, PlannedName(anchorId));
        }
    }

    static string PlannedName(int anchorId)
    {
        return anchorId >= 0 && anchorId < PlannedStationNames.Length
            ? PlannedStationNames[anchorId]
            : "预留工位";
    }
}

public enum FreshTrain2TaskStationState
{
    Locked,
    Active
}

public class FreshTrain2TaskStation : MonoBehaviour
{
    public string taskId = "";
    public string displayName = "";
    public string sceneName = "";
    public string plannedName = "";
    public int anchorId;
    public FreshTrain2TaskStationState state = FreshTrain2TaskStationState.Locked;

    static readonly Color ActiveBaseColor = new Color(0.12f, 0.42f, 0.68f);
    static readonly Color ActiveMarkerColor = new Color(0.18f, 0.82f, 1.0f);
    static readonly Color ActivePanelColor = new Color(0.05f, 0.12f, 0.17f);
    static readonly Color LockedBaseColor = new Color(0.30f, 0.31f, 0.33f);
    static readonly Color LockedMarkerColor = new Color(0.48f, 0.49f, 0.52f);
    static readonly Color LockedPanelColor = new Color(0.16f, 0.17f, 0.18f);

    Transform _labelTransform;

    void LateUpdate()
    {
        if (_labelTransform == null || Camera.main == null) return;
        FaceCameraYawOnly(_labelTransform);
    }

    public void Configure(AssignedTask task, int anchorId)
    {
        this.anchorId = anchorId;
        taskId = task.taskId;
        displayName = task.displayName;
        sceneName = task.sceneName;
        state = FreshTrain2TaskStationState.Active;
        BuildVisual();
    }

    public void ConfigureLocked(int anchorId, string plannedName)
    {
        this.anchorId = anchorId;
        taskId = "";
        this.plannedName = plannedName;
        displayName = plannedName;
        sceneName = "";
        state = FreshTrain2TaskStationState.Locked;
        BuildVisual();
    }

    void BuildVisual()
    {
        var active = state == FreshTrain2TaskStationState.Active;
        var baseColor = active ? ActiveBaseColor : LockedBaseColor;
        var markerColor = active ? ActiveMarkerColor : LockedMarkerColor;
        var panelColor = active ? ActivePanelColor : LockedPanelColor;

        var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad.name = "Station Floor Pad";
        pad.transform.SetParent(transform, false);
        pad.transform.localPosition = new Vector3(0f, 0.015f, 0f);
        pad.transform.localScale = new Vector3(1.85f, 0.015f, 1.85f);
        SetColor(pad, Color.Lerp(baseColor, Color.black, 0.32f));

        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseGo.name = "Station Base";
        baseGo.transform.SetParent(transform, false);
        baseGo.transform.localPosition = new Vector3(0f, 0.07f, 0f);
        baseGo.transform.localScale = active ? new Vector3(1.58f, 0.08f, 1.58f) : new Vector3(1.35f, 0.07f, 1.35f);
        SetColor(baseGo, baseColor);

        var plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plate.name = "Station Top Plate";
        plate.transform.SetParent(transform, false);
        plate.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        plate.transform.localScale = active ? new Vector3(1.28f, 0.04f, 1.28f) : new Vector3(1.05f, 0.035f, 1.05f);
        SetColor(plate, panelColor);

        var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = active ? "Active Beacon" : "Inactive Beacon";
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = active ? new Vector3(0f, 0.86f, 0f) : new Vector3(0f, 0.72f, 0f);
        marker.transform.localScale = active ? new Vector3(0.54f, 1.28f, 0.54f) : new Vector3(0.34f, 0.82f, 0.34f);
        SetColor(marker, markerColor);

        if (active)
            CreateEntryBeacon(markerColor);

        CreateStatusLight(new Vector3(-0.72f, 0.32f, -0.72f), markerColor);
        CreateStatusLight(new Vector3(0.72f, 0.32f, -0.72f), markerColor);
        CreateStatusLight(new Vector3(-0.72f, 0.32f, 0.72f), markerColor);
        CreateStatusLight(new Vector3(0.72f, 0.32f, 0.72f), markerColor);
        CreateStationNumber(active, markerColor);

        var trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 0.86f, 0f);
        trigger.size = active ? new Vector3(2.9f, 2.1f, 2.9f) : new Vector3(2.4f, 1.8f, 2.4f);

        CreateLabel(active ? displayName : displayName + "\n待开放", active ? Color.white : new Color(0.82f, 0.84f, 0.86f), panelColor, active);
    }

    void CreateEntryBeacon(Color markerColor)
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Entry Glow Ring";
        ring.transform.SetParent(transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.245f, 0f);
        ring.transform.localScale = new Vector3(1.72f, 0.018f, 1.72f);
        SetColor(ring, Color.Lerp(markerColor, Color.white, 0.18f));

        var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "Entry Beacon Core";
        core.transform.SetParent(transform, false);
        core.transform.localPosition = new Vector3(0f, 1.58f, 0f);
        core.transform.localScale = new Vector3(0.54f, 0.54f, 0.54f);
        SetColor(core, Color.Lerp(markerColor, Color.white, 0.25f));
    }

    void CreateStatusLight(Vector3 localPosition, Color color)
    {
        var light = GameObject.CreatePrimitive(PrimitiveType.Cube);
        light.name = "Station Status Light";
        light.transform.SetParent(transform, false);
        light.transform.localPosition = localPosition;
        light.transform.localScale = new Vector3(0.16f, 0.12f, 0.16f);
        SetColor(light, color);
    }

    void CreateStationNumber(bool active, Color color)
    {
        var numberGo = new GameObject("Station Number");
        numberGo.transform.SetParent(transform, false);
        numberGo.transform.localPosition = new Vector3(0f, 0.28f, -1.02f);
        numberGo.AddComponent<FreshTrain2FaceCameraLabel>();

        var number = numberGo.AddComponent<TextMesh>();
        number.text = (anchorId + 1).ToString("00");
        number.anchor = TextAnchor.MiddleCenter;
        number.alignment = TextAlignment.Center;
        number.characterSize = 0.09f;
        number.fontSize = 40;
        number.color = active ? color : new Color(0.62f, 0.64f, 0.66f);
    }

    void CreateLabel(string text, Color color, Color panelColor, bool active)
    {
        var labelGo = new GameObject("Station Label");
        labelGo.transform.SetParent(transform, false);
        labelGo.transform.localPosition = active ? new Vector3(0f, 2.08f, 0f) : new Vector3(0f, 1.42f, 0f);
        _labelTransform = labelGo.transform;

        var backPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backPlate.name = "Label Back Plate";
        backPlate.transform.SetParent(labelGo.transform, false);
        backPlate.transform.localPosition = new Vector3(0f, 0f, -0.045f);
        backPlate.transform.localScale = active ? new Vector3(1.48f, 0.40f, 0.06f) : new Vector3(1.15f, 0.34f, 0.05f);
        SetColor(backPlate, active ? Color.Lerp(panelColor, ActiveMarkerColor, 0.24f) : Color.Lerp(panelColor, Color.black, 0.12f));

        var textGo = new GameObject("Label Text");
        textGo.transform.SetParent(labelGo.transform, false);
        textGo.transform.localPosition = new Vector3(0f, 0f, 0.045f);

        var label = textGo.AddComponent<TextMesh>();
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = active ? (displayName.Length > 4 ? 0.105f : 0.125f) : 0.09f;
        label.fontSize = active ? (displayName.Length > 4 ? 42 : 50) : 38;
        label.color = color;
    }

    static void SetColor(GameObject go, Color color)
    {
        var collider = go.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static void FaceCameraYawOnly(Transform target)
    {
        var direction = Camera.main.transform.position - target.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            target.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}

public class FreshTrain2PlayerController : MonoBehaviour
{
    public Transform followCamera;
    public float moveSpeed = 4.5f;
    public float lookSensitivity = 0.12f;
    public float cameraDistance = 5.2f;
    public float cameraHeight = 2.4f;
    public float cameraSmoothTime = 0.08f;
    public float initialYaw = 0f;
    public float initialPitch = 18f;
    public bool lockCursorOnClick = true;
    public Vector2 worldBoundsX = new Vector2(-26.5f, 26.5f);
    public Vector2 worldBoundsZ = new Vector2(-17.5f, 17.5f);

    public event System.Action InteractPressed;

    CharacterController _controller;
    Vector2 _lookAngles = new Vector2(0f, 18f);
    Vector3 _cameraVelocity;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        SetLookAngles(initialYaw, initialPitch);
        SnapCameraToTarget();
    }

    public void SetLookAngles(float yaw, float pitch)
    {
        _lookAngles = new Vector2(yaw, Mathf.Clamp(pitch, -18f, 55f));
        transform.rotation = Quaternion.Euler(0f, _lookAngles.x, 0f);
    }

    void Update()
    {
        UpdateCursorLock();
        UpdateLook();
        UpdateMovement();
        UpdateFallbackInteract();
    }

    void LateUpdate()
    {
        UpdateCamera();
    }

    void UpdateCursorLock()
    {
        if (!lockCursorOnClick) return;

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void UpdateLook()
    {
        var delta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * lookSensitivity * 8f;
        _lookAngles.x += delta.x;
        _lookAngles.y = Mathf.Clamp(_lookAngles.y - delta.y, -18f, 55f);
        transform.rotation = Quaternion.Euler(0f, _lookAngles.x, 0f);
    }

    void UpdateMovement()
    {
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude <= 0.001f)
        {
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            input = new Vector2(x, y);
        }

        var forward = Quaternion.Euler(0f, _lookAngles.x, 0f) * Vector3.forward;
        var right = Quaternion.Euler(0f, _lookAngles.x, 0f) * Vector3.right;
        var move = forward * input.y + right * input.x;
        if (move.sqrMagnitude > 1f) move.Normalize();

        var velocity = move * moveSpeed;
        velocity.y = Physics.gravity.y;
        _controller.Move(velocity * Time.deltaTime);

        var position = transform.position;
        position.x = Mathf.Clamp(position.x, worldBoundsX.x, worldBoundsX.y);
        position.z = Mathf.Clamp(position.z, worldBoundsZ.x, worldBoundsZ.y);
        transform.position = position;
    }

    void UpdateFallbackInteract()
    {
        bool pressed = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space);
        if (!pressed) return;

        Debug.Log("[FreshTrain2PlayerController] Fallback interact pressed");
        InteractPressed?.Invoke();
    }

    void UpdateCamera()
    {
        if (followCamera == null) return;

        var pivot = transform.position + Vector3.up * cameraHeight;
        var rotation = Quaternion.Euler(_lookAngles.y, _lookAngles.x, 0f);
        var desiredPosition = pivot - rotation * Vector3.forward * cameraDistance;

        followCamera.position = Vector3.SmoothDamp(followCamera.position, desiredPosition, ref _cameraVelocity, cameraSmoothTime);
        followCamera.rotation = Quaternion.LookRotation(pivot - followCamera.position, Vector3.up);
    }

    void SnapCameraToTarget()
    {
        if (followCamera == null) return;

        var pivot = transform.position + Vector3.up * cameraHeight;
        var rotation = Quaternion.Euler(_lookAngles.y, _lookAngles.x, 0f);
        followCamera.position = pivot - rotation * Vector3.forward * cameraDistance;
        followCamera.rotation = Quaternion.LookRotation(pivot - followCamera.position, Vector3.up);
        _cameraVelocity = Vector3.zero;
    }
}

[RequireComponent(typeof(FreshTrain2PlayerController))]
public class FreshTrain2HubInteractor : MonoBehaviour
{
    public float interactionRadius = 2.2f;

    readonly List<FreshTrain2TaskStation> _nearbyStations = new List<FreshTrain2TaskStation>();
    FreshTrain2PlayerController _playerController;
    FreshTrain2TaskStation _currentStation;
    string _statusMessage = "";
    float _statusMessageUntil;
    bool _enteringTraining;

    void Awake()
    {
        _playerController = GetComponent<FreshTrain2PlayerController>();
    }

    void OnEnable()
    {
        if (_playerController != null)
            _playerController.InteractPressed += HandleInteractPressed;
    }

    void OnDisable()
    {
        if (_playerController != null)
            _playerController.InteractPressed -= HandleInteractPressed;
    }

    void Update()
    {
        PruneInvalidStations();
        _currentStation = FindNearestStation();
        if (_currentStation == null)
            _currentStation = FindNearestStationInScene();
    }

    void OnTriggerEnter(Collider other)
    {
        var station = other.GetComponent<FreshTrain2TaskStation>();
        if (station == null) station = other.GetComponentInParent<FreshTrain2TaskStation>();
        if (station != null && !_nearbyStations.Contains(station))
            _nearbyStations.Add(station);
    }

    void OnTriggerExit(Collider other)
    {
        var station = other.GetComponent<FreshTrain2TaskStation>();
        if (station == null) station = other.GetComponentInParent<FreshTrain2TaskStation>();
        if (station != null)
            _nearbyStations.Remove(station);
    }

    void OnGUI()
    {
        FreshTrain2GuiStyles.Ensure();

        if (Time.time < _statusMessageUntil && !string.IsNullOrEmpty(_statusMessage))
        {
            DrawToast(_statusMessage);
            return;
        }

        if (_currentStation == null) return;

        var width = Mathf.Min(560f, Screen.width - 40f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 118f, width, 82f);
        FreshTrain2GuiStyles.DrawCard(rect, true);
        FreshTrain2GuiStyles.DrawAccentBar(new Rect(rect.x, rect.y, 5f, rect.height));

        GUI.Label(new Rect(rect.x + 22f, rect.y + 13f, rect.width - 140f, 24f), _currentStation.displayName, FreshTrain2GuiStyles.SectionTitle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 40f, rect.width - 150f, 20f), PromptSubtitle(), FreshTrain2GuiStyles.Muted);
        GUI.Label(new Rect(rect.x + rect.width - 126f, rect.y + 22f, 96f, 34f), "训练入口", FreshTrain2GuiStyles.ActionKey);
    }

    void HandleInteractPressed()
    {
        if (_currentStation == null)
        {
            ShowStatus("请先移动到任务工位附近。");
            return;
        }

        if (_currentStation.state == FreshTrain2TaskStationState.Locked)
        {
            ShowStatus("该工位暂未分配训练任务");
            Debug.Log("[FreshTrain2HubInteractor] Locked station ignored: anchor=" + _currentStation.anchorId);
            return;
        }

        if (_enteringTraining) return;

        var session = SessionManager.EnsureExists();
        var task = new AssignedTask(
            _currentStation.taskId,
            _currentStation.displayName,
            _currentStation.sceneName,
            _currentStation.anchorId);

        var returnPosition = GetReturnPositionNearStation(_currentStation);
        session.selectedTaskId = task.taskId;
        session.selectedInstruction = "";
        session.selectedSuccessMessage = "成功完成训练";

        Debug.Log("[FreshTrain2HubInteractor] Enter scene: " + task.sceneName + " (taskId=" + task.taskId + ", anchor=" + task.anchorId + ")");
        StartCoroutine(EnterTrainingRoutine(task, returnPosition));
    }

    string GetPromptText()
    {
        if (Time.time < _statusMessageUntil && !string.IsNullOrEmpty(_statusMessage))
            return _statusMessage;

        if (_currentStation == null) return "";
        if (_currentStation.state == FreshTrain2TaskStationState.Locked)
            return "该工位暂未分配";

        return "进入 " + _currentStation.displayName;
    }

    void ShowStatus(string message)
    {
        _statusMessage = message;
        _statusMessageUntil = Time.time + 1.8f;
    }

    IEnumerator EnterTrainingRoutine(AssignedTask task, Vector3 returnPosition)
    {
        _enteringTraining = true;

        var handService = MockHandTrackingService.Instance;
        if (handService != null && handService.Status != HandTrackingStatus.Ready)
        {
            ShowStatus("手势识别未完全就绪，将先进入练习模式");
            Debug.LogWarning("[FreshTrain2HubInteractor] Hand tracking not ready: " + handService.Status + ". Entering practice mode anyway.");
            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            ShowStatus("正在进入 " + task.displayName);
            yield return new WaitForSeconds(0.15f);
        }

        SceneFlow.EnsureExists().EnterTraining(task, returnPosition);
    }

    void DrawToast(string message)
    {
        var width = Mathf.Min(460f, Screen.width - 40f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 106f, width, 58f);
        FreshTrain2GuiStyles.DrawCard(rect, true);
        GUI.Label(new Rect(rect.x + 18f, rect.y + 14f, rect.width - 36f, 28f), message, FreshTrain2GuiStyles.CenterTitle);
    }

    string PromptSubtitle()
    {
        if (_currentStation == null) return "";
        if (_currentStation.state == FreshTrain2TaskStationState.Locked)
            return "当前工位未开放，请选择右侧任务清单中的训练项";

        return "靠近工位后选择当前训练场景";
    }

    Vector3 GetReturnPositionNearStation(FreshTrain2TaskStation station)
    {
        var offset = transform.position - station.transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude < 0.01f)
            offset = Vector3.back;

        return station.transform.position + offset.normalized * 1.8f + Vector3.up;
    }

    FreshTrain2TaskStation FindNearestStation()
    {
        FreshTrain2TaskStation nearest = null;
        var nearestDistance = float.MaxValue;

        foreach (var station in _nearbyStations)
        {
            if (station == null) continue;

            var distance = Vector3.Distance(transform.position, station.transform.position);
            if (distance > interactionRadius) continue;
            if (distance >= nearestDistance) continue;

            nearest = station;
            nearestDistance = distance;
        }

        return nearest;
    }

    FreshTrain2TaskStation FindNearestStationInScene()
    {
        FreshTrain2TaskStation nearest = null;
        var nearestDistance = float.MaxValue;
        var stations = FindObjectsOfType<FreshTrain2TaskStation>();

        foreach (var station in stations)
        {
            if (station == null) continue;

            var distance = Vector3.Distance(transform.position, station.transform.position);
            if (distance > interactionRadius) continue;
            if (distance >= nearestDistance) continue;

            nearest = station;
            nearestDistance = distance;
        }

        return nearest;
    }

    void PruneInvalidStations()
    {
        for (int i = _nearbyStations.Count - 1; i >= 0; i--)
        {
            var station = _nearbyStations[i];
            if (station == null || Vector3.Distance(transform.position, station.transform.position) > interactionRadius + 0.8f)
                _nearbyStations.RemoveAt(i);
        }
    }
}

public enum FreshTrain2InteractiveElementType
{
    Checklist,
    ControlPanel,
    SafetyGate,
    AlarmReset,
    TaskDispatch,
    QualityScanner,
    MaintenanceBench,
    StorageCheckIn
}

public class FreshTrain2InteractiveElement : MonoBehaviour
{
    public FreshTrain2InteractiveElementType elementType = FreshTrain2InteractiveElementType.Checklist;
    public string displayName = "互动设备";
    public string subtitle = "靠近后执行操作";
    public string actionText = "操作";
    public string idleStatus = "待检查";
    public string activeStatus = "已完成";
    public string detailText = "";
    public string trainingPurpose = "训练设备认知与操作流程";
    public string scoreHint = "可记录完成状态和用时";
    public string targetTaskId = "";
    public string targetTaskName = "";
    public string targetSceneName = "SampleScene";
    public int targetAnchorId = -1;
    public Transform movingPart;
    public Renderer[] indicatorRenderers;
    public Light signalLight;
    public float interactionRadius = 3.2f;

    bool _active;
    bool _focused;
    int _step;
    float _pulse;
    float _lastInteractAt = -99f;
    Vector3 _movingPartStartLocalPosition;
    Quaternion _movingPartStartLocalRotation;

    public bool IsActive => _active;
    public string StatusText => _active ? activeStatus : idleStatus;
    public string DetailText => detailText;
    public float LastInteractAt => _lastInteractAt;

    void Awake()
    {
        CacheMovingPartPose();
        ApplyVisuals();
    }

    void Update()
    {
        _pulse += Time.deltaTime;
        AnimateMovingPart();
        AnimateIndicators();
    }

    public void Configure(
        FreshTrain2InteractiveElementType type,
        string name,
        string subtitleText,
        string action,
        string idle,
        string active,
        string detail,
        Transform moving = null,
        Renderer[] indicators = null,
        Light light = null,
        string purpose = "",
        string score = "")
    {
        elementType = type;
        displayName = name;
        subtitle = subtitleText;
        actionText = action;
        idleStatus = idle;
        activeStatus = active;
        detailText = detail;
        if (!string.IsNullOrEmpty(purpose)) trainingPurpose = purpose;
        if (!string.IsNullOrEmpty(score)) scoreHint = score;
        movingPart = moving;
        indicatorRenderers = indicators;
        signalLight = light;
        CacheMovingPartPose();
        ApplyVisuals();
    }

    public void ConfigureTrainingLink(string taskId, string taskName, string sceneName = "SampleScene", int anchorId = -1)
    {
        targetTaskId = taskId;
        targetTaskName = string.IsNullOrEmpty(taskName) ? displayName : taskName;
        targetSceneName = string.IsNullOrEmpty(sceneName) ? "SampleScene" : sceneName;
        targetAnchorId = anchorId;
        actionText = "进入";
        activeStatus = "进入训练";
    }

    public float DistanceTo(Vector3 worldPosition)
    {
        if (!TryGetRendererBounds(out var bounds))
            return HorizontalDistance(worldPosition, transform.position);

        var sample = worldPosition;
        sample.y = bounds.center.y;
        var closest = bounds.ClosestPoint(sample);
        closest.y = worldPosition.y;
        return HorizontalDistance(worldPosition, closest);
    }

    public void Interact()
    {
        _lastInteractAt = Time.time;

        if (!string.IsNullOrEmpty(targetTaskId))
        {
            EnterLinkedTraining();
            return;
        }

        switch (elementType)
        {
            case FreshTrain2InteractiveElementType.Checklist:
                _active = true;
                _step = Mathf.Min(_step + 1, 3);
                detailText = "PPE 确认 " + _step + "/3: 安全帽、护目镜、绝缘手套";
                if (_step >= 3) activeStatus = "PPE 检查完成";
                break;
            case FreshTrain2InteractiveElementType.ControlPanel:
                _active = true;
                _step = (_step + 1) % 4;
                detailText = ControlPanelStepText(_step);
                activeStatus = _step == 0 ? "点检循环完成" : "正在点检";
                break;
            case FreshTrain2InteractiveElementType.SafetyGate:
                _active = !_active;
                detailText = _active ? "安全闸门已打开，允许进入设备观察区" : "安全闸门已关闭，设备观察区已隔离";
                break;
            case FreshTrain2InteractiveElementType.AlarmReset:
                _active = true;
                _step++;
                detailText = "告警已复位 " + _step + " 次，现场状态恢复为绿色";
                activeStatus = "告警已复位";
                break;
            case FreshTrain2InteractiveElementType.TaskDispatch:
                _active = true;
                _step = (_step + 1) % 4;
                detailText = TaskDispatchStepText(_step);
                activeStatus = _step == 0 ? "任务路线已确认" : "派工中";
                break;
            case FreshTrain2InteractiveElementType.QualityScanner:
                _active = true;
                _step = (_step + 1) % 4;
                detailText = QualityScannerStepText(_step);
                activeStatus = _step == 0 ? "质检样本已归档" : "正在扫描样本";
                break;
            case FreshTrain2InteractiveElementType.MaintenanceBench:
                _active = true;
                _step = (_step + 1) % 4;
                detailText = MaintenanceToolStepText(_step);
                activeStatus = _step == 0 ? "维修工具已归位" : "工具选择中";
                break;
            case FreshTrain2InteractiveElementType.StorageCheckIn:
                _active = true;
                _step++;
                detailText = "成品入库确认 " + _step + " 件，生成批次追踪记录";
                activeStatus = "库存已更新";
                break;
        }

        ApplyVisuals();
        Debug.Log("[FreshTrain2InteractiveElement] " + displayName + " -> " + StatusText + " | " + detailText);
    }

    void EnterLinkedTraining()
    {
        var taskName = string.IsNullOrEmpty(targetTaskName) ? displayName : targetTaskName;
        var sceneName = string.IsNullOrEmpty(targetSceneName) ? "SampleScene" : targetSceneName;
        var session = SessionManager.EnsureExists();
        session.selectedTaskId = targetTaskId;

        var returnPosition = transform.position + Vector3.back * 2.0f + Vector3.up;
        var task = new AssignedTask(targetTaskId, taskName, sceneName, targetAnchorId);
        Debug.Log("[FreshTrain2InteractiveElement] Enter linked training: " + taskName + " -> " + targetTaskId);
        SceneFlow.EnsureExists().EnterTraining(task, returnPosition);
    }

    public void SetFocus(bool focused)
    {
        if (_focused == focused) return;
        _focused = focused;
        ApplyVisuals();
    }

    void CacheMovingPartPose()
    {
        if (movingPart == null) return;
        _movingPartStartLocalPosition = movingPart.localPosition;
        _movingPartStartLocalRotation = movingPart.localRotation;
    }

    void AnimateMovingPart()
    {
        if (movingPart == null) return;

        if (elementType == FreshTrain2InteractiveElementType.SafetyGate)
        {
            var target = _active ? Quaternion.Euler(0f, 74f, 0f) : _movingPartStartLocalRotation;
            movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, target, Time.deltaTime * 6f);
            return;
        }

        if (elementType == FreshTrain2InteractiveElementType.Checklist)
        {
            var target = _active ? Quaternion.Euler(0f, -18f, 0f) : _movingPartStartLocalRotation;
            movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, target, Time.deltaTime * 5f);
            return;
        }

        if (elementType == FreshTrain2InteractiveElementType.ControlPanel)
        {
            var y = _active ? 0.04f + Mathf.Sin(Time.time * 3.4f) * 0.015f : 0f;
            movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, _movingPartStartLocalPosition + Vector3.up * y, Time.deltaTime * 5f);
            return;
        }

        if (elementType == FreshTrain2InteractiveElementType.TaskDispatch || elementType == FreshTrain2InteractiveElementType.QualityScanner)
        {
            var y = _active ? Mathf.Sin(Time.time * 2.8f) * 0.025f : 0f;
            movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, _movingPartStartLocalPosition + Vector3.up * y, Time.deltaTime * 4f);
            return;
        }

        if (elementType == FreshTrain2InteractiveElementType.MaintenanceBench)
        {
            var target = _active ? Quaternion.Euler(0f, Mathf.Sin(Time.time * 3.0f) * 8f, 0f) : _movingPartStartLocalRotation;
            movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, target, Time.deltaTime * 5f);
            return;
        }

        if (elementType == FreshTrain2InteractiveElementType.AlarmReset)
        {
            var y = Time.time - _lastInteractAt < 0.24f ? -0.06f : 0f;
            movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, _movingPartStartLocalPosition + Vector3.up * y, Time.deltaTime * 10f);
        }
    }

    void AnimateIndicators()
    {
        var color = CurrentColor();
        var highlight = Color.Lerp(color, Color.white, (Mathf.Sin(_pulse * 4.0f) + 1f) * 0.10f);

        if (indicatorRenderers != null)
        {
            foreach (var item in indicatorRenderers)
                SetRendererColor(item, highlight);
        }

        if (signalLight != null)
        {
            signalLight.color = color;
            signalLight.intensity = _active ? 1.5f + Mathf.Sin(_pulse * 5.0f) * 0.25f : 0.65f;
        }
    }

    void ApplyVisuals()
    {
        var color = CurrentColor();

        if (indicatorRenderers != null)
        {
            foreach (var item in indicatorRenderers)
                SetRendererColor(item, color);
        }

        if (signalLight != null)
        {
            signalLight.color = color;
            signalLight.intensity = _active ? 1.35f : 0.65f;
        }
    }

    Color CurrentColor()
    {
        if (_active) return new Color(0.24f, 0.88f, 0.48f);
        if (_focused) return new Color(1.00f, 0.86f, 0.22f);
        if (elementType == FreshTrain2InteractiveElementType.AlarmReset) return new Color(1.00f, 0.30f, 0.18f);
        if (elementType == FreshTrain2InteractiveElementType.QualityScanner) return new Color(0.20f, 0.66f, 1.00f);
        if (elementType == FreshTrain2InteractiveElementType.StorageCheckIn) return new Color(0.38f, 0.78f, 0.58f);
        return new Color(1.00f, 0.70f, 0.16f);
    }

    static string ControlPanelStepText(int step)
    {
        switch (step)
        {
            case 1: return "步骤 1/3: 电源指示正常";
            case 2: return "步骤 2/3: 急停回路正常";
            case 3: return "步骤 3/3: 安全门联锁正常";
            default: return "点检记录已归档，可以进入训练";
        }
    }

    static string TaskDispatchStepText(int step)
    {
        switch (step)
        {
            case 1: return "路线 1/3: 先完成 PPE 和设备点检";
            case 2: return "路线 2/3: 前往分拣线完成主任务";
            case 3: return "路线 3/3: 完成质检与成品入库确认";
            default: return "派工完成: 本次训练路线已锁定";
        }
    }

    static string QualityScannerStepText(int step)
    {
        switch (step)
        {
            case 1: return "样本 A: 合格件，流向成品缓存区";
            case 2: return "样本 B: 表面缺陷，流向返修区";
            case 3: return "样本 C: 标签缺失，流向人工复核";
            default: return "质检记录已归档，等待下一批样本";
        }
    }

    static string MaintenanceToolStepText(int step)
    {
        switch (step)
        {
            case 1: return "已选择绝缘手套: 适用于电气柜操作";
            case 2: return "已选择扭矩扳手: 适用于螺栓拧紧训练";
            case 3: return "已选择点检表: 适用于班前巡检";
            default: return "工具归位完成，维修区恢复整洁";
        }
    }

    static void SetRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    bool TryGetRendererBounds(out Bounds bounds)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        bounds = default;
        bool hasBounds = false;

        foreach (var renderer in renderers)
        {
            if (renderer == null || !(renderer is MeshRenderer)) continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}

public class FreshTrain2EnvironmentInteractor : MonoBehaviour
{
    public float interactionRadius = 2.7f;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode alternateInteractKey = KeyCode.Space;
    public string keyLabel = "E";
    public bool listenToPlayerInteract;
    public bool allowDirectKeyInputWithPlayerController = true;

    FreshTrain2PlayerController _playerController;
    FreshTrain2InteractiveElement _current;
    string _lastFeedback = "";
    float _feedbackUntil;

    void Awake()
    {
        _playerController = GetComponent<FreshTrain2PlayerController>();
    }

    void OnEnable()
    {
        if (listenToPlayerInteract && _playerController != null)
            _playerController.InteractPressed += HandleInteractPressed;
    }

    void OnDisable()
    {
        if (listenToPlayerInteract && _playerController != null)
            _playerController.InteractPressed -= HandleInteractPressed;
    }

    void Update()
    {
        _current = FindNearestElement();
        if ((_playerController == null || allowDirectKeyInputWithPlayerController) && IsFallbackInteractPressed())
            HandleInteractPressed();

        HighlightCurrentElement();
    }

    void OnGUI()
    {
        FreshTrain2GuiStyles.Ensure();

        if (Time.time < _feedbackUntil && !string.IsNullOrEmpty(_lastFeedback))
            DrawFeedback();

        if (_current != null)
            DrawPrompt(_current);
    }

    void HandleInteractPressed()
    {
        if (_current == null) return;

        _current.Interact();
        _lastFeedback = _current.displayName + ": " + _current.StatusText;
        _feedbackUntil = Time.time + 2.0f;
    }

    bool IsFallbackInteractPressed()
    {
        if (interactKey != KeyCode.None && Input.GetKeyDown(interactKey)) return true;
        if (alternateInteractKey != KeyCode.None && Input.GetKeyDown(alternateInteractKey)) return true;
        return false;
    }

    FreshTrain2InteractiveElement FindNearestElement()
    {
        FreshTrain2InteractiveElement nearest = null;
        var nearestDistance = float.MaxValue;
        var elements = FindObjectsOfType<FreshTrain2InteractiveElement>();

        foreach (var element in elements)
        {
            if (element == null) continue;

            float effectiveRadius = Mathf.Max(interactionRadius, element.interactionRadius);
            var distance = element.DistanceTo(transform.position);
            if (distance > effectiveRadius || distance >= nearestDistance) continue;

            nearest = element;
            nearestDistance = distance;
        }

        return nearest;
    }

    void HighlightCurrentElement()
    {
        var elements = FindObjectsOfType<FreshTrain2InteractiveElement>();
        foreach (var element in elements)
        {
            if (element == null) continue;
            element.SetFocus(element == _current);
        }
    }

    void DrawPrompt(FreshTrain2InteractiveElement element)
    {
        var width = Mathf.Min(Mathf.Clamp(Screen.width * 0.62f, 460f, 720f), Mathf.Max(340f, Screen.width - 48f));
        var height = Screen.height < 560f ? 116f : 138f;
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - height - 26f, width, height);
        var keyBoxWidth = 84f;
        var textWidth = rect.width - keyBoxWidth - 58f;

        FreshTrain2GuiStyles.DrawCard(rect, true);
        FreshTrain2GuiStyles.DrawAccentBar(new Rect(rect.x, rect.y, 5f, rect.height));

        GUI.Label(new Rect(rect.x + 22f, rect.y + 12f, textWidth, 28f), element.displayName, FreshTrain2GuiStyles.SectionTitle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 42f, textWidth, 24f), element.subtitle, FreshTrain2GuiStyles.Muted);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 68f, textWidth, 24f), "状态: " + element.StatusText, FreshTrain2GuiStyles.Body);
        string detail = string.IsNullOrEmpty(element.DetailText) ? element.trainingPurpose : element.DetailText;
        GUI.Label(new Rect(rect.x + 22f, rect.y + 94f, textWidth, 34f), detail, FreshTrain2GuiStyles.Muted);

        var actionRect = new Rect(rect.x + rect.width - keyBoxWidth - 24f, rect.y + 34f, keyBoxWidth, 44f);
        GUI.Label(actionRect, keyLabel, FreshTrain2GuiStyles.ActionKey);
        GUI.Label(new Rect(actionRect.x - 18f, actionRect.y + 48f, actionRect.width + 36f, 24f), element.actionText, FreshTrain2GuiStyles.Muted);
    }

    void DrawFeedback()
    {
        var width = Mathf.Min(440f, Screen.width - 40f);
        var rect = new Rect(Screen.width - width - 18f, 18f, width, 58f);

        FreshTrain2GuiStyles.DrawCard(rect, true);
        FreshTrain2GuiStyles.DrawDot(new Rect(rect.x + 18f, rect.y + 20f, 14f, 14f), FreshTrain2GuiStyles.SuccessTexture);
        GUI.Label(new Rect(rect.x + 42f, rect.y + 13f, rect.width - 60f, 26f), _lastFeedback, FreshTrain2GuiStyles.SectionTitle);
    }
}

public class FreshTrain2ReturnExit : MonoBehaviour
{
    public string hallSceneName = "entry";
    public float triggerRadius = 3.4f;
    public float holdSeconds = 1.0f;
    public KeyCode confirmKey = KeyCode.F;
    public KeyCode alternateConfirmKey = KeyCode.Space;

    Transform _player;
    Transform _label;
    float _holdTime;
    bool _loading;

    void Start()
    {
        BuildExitVisual();
    }

    void Update()
    {
        if (_loading) return;

        if (_player == null)
        {
            var playerGo = GameObject.Find("FreshTrain2 Hub Player");
            if (playerGo != null) _player = playerGo.transform;
        }

        if (_label != null && Camera.main != null)
            FaceCameraYawOnly(_label);

        if (_player == null)
        {
            _holdTime = 0f;
            return;
        }

        float distance = HorizontalDistance(_player.position, transform.position);
        if (distance > triggerRadius)
        {
            _holdTime = 0f;
            return;
        }

        _holdTime += Time.deltaTime;
        bool confirm = Input.GetKeyDown(confirmKey) || Input.GetKeyDown(alternateConfirmKey);
        if (_holdTime >= holdSeconds || confirm)
            ReturnToHall();
    }

    void OnGUI()
    {
        if (_loading || _player == null) return;
        if (HorizontalDistance(_player.position, transform.position) > triggerRadius) return;

        FreshTrain2GuiStyles.Ensure();
        float width = Mathf.Min(520f, Screen.width - 40f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 112f, width, 76f);
        FreshTrain2GuiStyles.DrawCard(rect, true);
        FreshTrain2GuiStyles.DrawAccentBar(new Rect(rect.x, rect.y, 5f, rect.height));

        GUI.Label(new Rect(rect.x + 22f, rect.y + 10f, rect.width - 44f, 26f), "返回大工厂大厅", FreshTrain2GuiStyles.SectionTitle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 40f, rect.width - 170f, 22f), "在出口处短暂停留，或按 F / 空格确认返回", FreshTrain2GuiStyles.Muted);

        float progress = Mathf.Clamp01(_holdTime / Mathf.Max(0.1f, holdSeconds));
        var barRect = new Rect(rect.x + rect.width - 145f, rect.y + 48f, 110f * progress, 5f);
        GUI.DrawTexture(barRect, FreshTrain2GuiStyles.AccentTexture);
    }

    void ReturnToHall()
    {
        _loading = true;
        var session = SessionManager.EnsureExists();
        session.selectedTaskId = "";
        session.hasHubReturnPosition = false;
        session.hubReturnPosition = Vector3.zero;
        SceneManager.LoadScene(hallSceneName);
    }

    void BuildExitVisual()
    {
        CreateCylinder("Return Exit Outer Halo", new Vector3(0f, 0.04f, 0f), new Vector3(3.25f, 0.035f, 3.25f), new Color(0.05f, 0.55f, 0.82f));
        CreateCylinder("Return Exit Inner Pad", new Vector3(0f, 0.08f, 0f), new Vector3(2.05f, 0.032f, 2.05f), new Color(0.04f, 0.20f, 0.28f));
        CreateCube("Return Exit Left Pillar", new Vector3(-1.25f, 1.25f, 0f), new Vector3(0.22f, 2.5f, 0.22f), new Color(0.10f, 0.72f, 0.92f));
        CreateCube("Return Exit Right Pillar", new Vector3(1.25f, 1.25f, 0f), new Vector3(0.22f, 2.5f, 0.22f), new Color(0.10f, 0.72f, 0.92f));
        CreateCube("Return Exit Beam", new Vector3(0f, 2.55f, 0f), new Vector3(2.9f, 0.24f, 0.34f), new Color(0.06f, 0.40f, 0.58f));

        var labelRoot = new GameObject("Return Exit Label");
        labelRoot.transform.SetParent(transform, false);
        labelRoot.transform.localPosition = new Vector3(0f, 3.15f, 0f);
        _label = labelRoot.transform;

        var plate = CreateCube("Return Exit Label Plate", new Vector3(0f, 0f, -0.05f), new Vector3(3.25f, 0.52f, 0.07f), new Color(0.035f, 0.13f, 0.18f));
        plate.transform.SetParent(labelRoot.transform, false);

        var textGo = new GameObject("Return Exit Label Text");
        textGo.transform.SetParent(labelRoot.transform, false);
        textGo.transform.localPosition = new Vector3(0f, 0.02f, 0.04f);
        var text = textGo.AddComponent<TextMesh>();
        text.text = "返回大厅";
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.26f;
        text.fontSize = 64;
        text.color = Color.white;
    }

    GameObject CreateCube(string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        SetColor(go, color);
        return go;
    }

    GameObject CreateCylinder(string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        SetColor(go, color);
        return go;
    }

    static void SetColor(GameObject go, Color color)
    {
        var collider = go.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    static void FaceCameraYawOnly(Transform target)
    {
        Vector3 direction = Camera.main.transform.position - target.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            target.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}

public class FreshTrain2FactoryScenery : MonoBehaviour
{
    const string KitPath = "ThirdParty/Kenney/FactoryKit/Models/";
    const float FloorY = 0f;
    const float ConveyorCargoY = 0.54f;

    Transform _root;

    void Start()
    {
        _root = new GameObject("FreshTrain2 Factory Scenery").transform;
        BuildProductionLines();
        BuildMachineCells();
        BuildWarehouseZones();
        BuildOverheadDetails();
        BuildControlArea();
        BuildSafetyDetails();
    }

    void BuildProductionLines()
    {
        CreateConveyorRun("FreshTrain2 Main Assembly Conveyor", new Vector3(-14.8f, FloorY, 4.2f), 13, Vector3.right);
        CreateConveyorRun("FreshTrain2 Packing Conveyor", new Vector3(10.0f, FloorY, -10.8f), 7, Vector3.left);

        Kit("conveyor-corner", new Vector3(12.4f, FloorY, 4.2f), new Vector3(0f, 90f, 0f), Vector3.one);
        Kit("conveyor-long-sides", new Vector3(12.4f, FloorY, 1.85f), new Vector3(0f, 0f, 0f), Vector3.one);
        Kit("conveyor-long-sides", new Vector3(12.4f, FloorY, -0.30f), new Vector3(0f, 0f, 0f), Vector3.one);
        Kit("conveyor-junction-t", new Vector3(12.4f, FloorY, -2.55f), new Vector3(0f, 90f, 0f), Vector3.one);

        Kit("scanner-high", new Vector3(-3.2f, FloorY, 4.2f), new Vector3(0f, 90f, 0f), Vector3.one);
        Kit("scanner-low", new Vector3(5.0f, FloorY, 4.2f), new Vector3(0f, 90f, 0f), Vector3.one);
        Kit("robot-arm-a", new Vector3(-7.6f, FloorY, 1.2f), new Vector3(0f, 32f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
        Kit("robot-arm-b", new Vector3(7.6f, FloorY, 7.1f), new Vector3(0f, -145f, 0f), new Vector3(1.2f, 1.2f, 1.2f));

        for (int i = 0; i < 12; i++)
        {
            var x = -14.2f + i * 2.05f;
            Kit(i % 2 == 0 ? "box-small" : "box-wide", new Vector3(x, ConveyorCargoY, 4.2f), new Vector3(0f, i * 17f, 0f), Vector3.one);
        }
    }

    void BuildMachineCells()
    {
        CreateMachineCluster(new Vector3(-18.0f, FloorY, 7.5f), 90f);
        CreateMachineCluster(new Vector3(18.0f, FloorY, 7.5f), -90f);
        CreateMachineCluster(new Vector3(-18.0f, FloorY, -3.6f), 90f);
        CreateMachineCluster(new Vector3(18.0f, FloorY, -3.6f), -90f);

        Kit("hopper-high-square", new Vector3(-14.5f, FloorY, -13.8f), new Vector3(0f, 25f, 0f), Vector3.one);
        Kit("hopper-high-round", new Vector3(14.5f, FloorY, -13.8f), new Vector3(0f, -25f, 0f), Vector3.one);
        Kit("machine-bed", new Vector3(-8.5f, FloorY, -14.4f), new Vector3(0f, 0f, 0f), Vector3.one);
        Kit("machine-fortified", new Vector3(8.5f, FloorY, -14.4f), new Vector3(0f, 180f, 0f), Vector3.one);
    }

    void BuildWarehouseZones()
    {
        CreateBoxStack(new Vector3(-25.0f, FloorY, -16.0f), 4, 3, new Vector3(0.85f, 0f, 0.82f));
        CreateBoxStack(new Vector3(21.0f, FloorY, -16.0f), 5, 3, new Vector3(0.82f, 0f, 0.82f));
        CreateBoxStack(new Vector3(-25.0f, FloorY, 16.1f), 4, 2, new Vector3(0.85f, 0f, 0.82f));
        CreateBoxStack(new Vector3(21.5f, FloorY, 16.1f), 4, 2, new Vector3(0.85f, 0f, 0.82f));

        Kit("warning-traffic", new Vector3(-18.5f, FloorY, 16.2f), Vector3.zero, Vector3.one);
        Kit("warning-orange", new Vector3(18.5f, FloorY, 16.2f), Vector3.zero, Vector3.one);
        Kit("door-wide-closed", new Vector3(0f, FloorY, 18.72f), new Vector3(0f, 180f, 0f), new Vector3(1.6f, 1.35f, 1.35f));
    }

    void BuildOverheadDetails()
    {
        CreateCeilingBeam(new Vector3(-19.0f, 3.85f, 0f), new Vector3(0.18f, 0.16f, 36.0f));
        CreateCeilingBeam(new Vector3(-9.5f, 3.85f, 0f), new Vector3(0.18f, 0.16f, 36.0f));
        CreateCeilingBeam(new Vector3(0f, 3.85f, 0f), new Vector3(0.18f, 0.16f, 36.0f));
        CreateCeilingBeam(new Vector3(9.5f, 3.85f, 0f), new Vector3(0.18f, 0.16f, 36.0f));
        CreateCeilingBeam(new Vector3(19.0f, 3.85f, 0f), new Vector3(0.18f, 0.16f, 36.0f));

        for (int i = 0; i < 10; i++)
        {
            var z = -16.0f + i * 3.55f;
            CreateLightPanel(new Vector3(-11.0f, 3.55f, z));
            CreateLightPanel(new Vector3(0f, 3.55f, z));
            CreateLightPanel(new Vector3(11.0f, 3.55f, z));
        }

        Kit("crane", new Vector3(0f, 3.15f, 1.2f), new Vector3(0f, 90f, 0f), new Vector3(2.0f, 2.0f, 2.0f), false);
        Kit("crane-lift", new Vector3(0f, 1.95f, 1.2f), new Vector3(0f, 90f, 0f), new Vector3(1.45f, 1.45f, 1.45f), false);
        Kit("crane-magnet", new Vector3(0f, 1.25f, 1.2f), new Vector3(0f, 90f, 0f), new Vector3(1.45f, 1.45f, 1.45f), false);

        CreateCatwalk(new Vector3(-25.2f, 2.75f, 0.0f), 10);
        CreateCatwalk(new Vector3(25.2f, 2.75f, 0.0f), 10);
        CreatePipeRun(new Vector3(-26.0f, 2.85f, -15.5f), 10, Vector3.forward);
        CreatePipeRun(new Vector3(26.0f, 2.85f, -15.5f), 10, Vector3.forward);
    }

    void BuildControlArea()
    {
        Kit("screen-panel-wide", new Vector3(-2.0f, FloorY, -16.75f), new Vector3(0f, 180f, 0f), Vector3.one);
        Kit("screen-panel-small", new Vector3(1.8f, FloorY, -16.75f), new Vector3(0f, 180f, 0f), Vector3.one);
        Kit("lever-double", new Vector3(-4.25f, FloorY, -16.35f), new Vector3(0f, 180f, 0f), Vector3.one);
        Kit("button-floor-round", new Vector3(4.45f, FloorY, -16.15f), new Vector3(0f, 180f, 0f), Vector3.one);
        Kit("screen-hanging-wide", new Vector3(0f, 2.55f, -16.05f), new Vector3(0f, 180f, 0f), new Vector3(1.35f, 1.35f, 1.35f), false);
    }

    void BuildSafetyDetails()
    {
        for (int i = 0; i < 18; i++)
        {
            float x = -20.4f + i * 2.4f;
            Kit("cone", new Vector3(x, FloorY, 9.85f), Vector3.zero, Vector3.one);
            Kit("cone", new Vector3(x, FloorY, -8.85f), Vector3.zero, Vector3.one);
        }

        CreateSafetyStripe("FreshTrain2 North Safety Edge", new Vector3(0f, 0.025f, 10.25f), new Vector3(42f, 0.035f, 0.10f));
        CreateSafetyStripe("FreshTrain2 South Safety Edge", new Vector3(0f, 0.025f, -9.25f), new Vector3(42f, 0.035f, 0.10f));
    }

    void CreateConveyorRun(string name, Vector3 start, int segments, Vector3 direction)
    {
        var run = new GameObject(name).transform;
        run.SetParent(_root, false);

        for (int i = 0; i < segments; i++)
        {
            var pos = start + direction.normalized * i * 2.05f;
            var yaw = Mathf.Abs(direction.x) > 0.5f ? 90f : 0f;
            if (direction.x < -0.5f) yaw = -90f;
            Kit("conveyor-long-stripe-sides", pos, new Vector3(0f, yaw, 0f), Vector3.one, run);
        }
    }

    void CreateMachineCluster(Vector3 origin, float yaw)
    {
        Kit("machine", origin, new Vector3(0f, yaw, 0f), Vector3.one);
        Kit("machine-window", origin + new Vector3(0f, 0f, 1.35f), new Vector3(0f, yaw, 0f), Vector3.one);
        Kit("screen-wide", origin + new Vector3(0f, 0f, -1.20f), new Vector3(0f, yaw, 0f), Vector3.one);
        Kit("pipe-large-valve", origin + new Vector3(0f, 1.35f, 0.15f), new Vector3(0f, yaw + 90f, 0f), Vector3.one);
    }

    void CreateBoxStack(Vector3 origin, int columns, int rows, Vector3 spacing)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var model = (x + y) % 3 == 0 ? "box-large" : (x + y) % 3 == 1 ? "box-wide" : "box-small";
                var jitter = new Vector3(0f, 0f, (x % 2) * 0.14f);
                Kit(model, origin + new Vector3(x * spacing.x, y * 0.58f, y * spacing.z) + jitter,
                    new Vector3(0f, x * 11f + y * 7f, 0f), Vector3.one);
            }
        }
    }

    void CreateCatwalk(Vector3 start, int segments)
    {
        for (int i = 0; i < segments; i++)
            Kit("catwalk-straight", start + new Vector3(0f, 0f, -7.0f + i * 2.6f), Vector3.zero, Vector3.one, false);

        Kit("catwalk-stairs", start + new Vector3(0f, -1.15f, 14.0f), new Vector3(0f, 180f, 0f), Vector3.one, false);
    }

    void CreatePipeRun(Vector3 start, int segments, Vector3 direction)
    {
        for (int i = 0; i < segments; i++)
        {
            var pos = start + direction.normalized * i * 2.15f;
            Kit("pipe-large-long", pos, new Vector3(0f, 0f, 90f), Vector3.one, false);
        }
        Kit("pipe-large-valve", start + direction.normalized * 2.15f * (segments - 1) + new Vector3(0f, 0f, 1.45f), new Vector3(0f, 0f, 90f), Vector3.one, false);
    }

    void CreateCeilingBeam(Vector3 position, Vector3 scale)
    {
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "FreshTrain2 Ceiling Beam";
        beam.transform.SetParent(_root, false);
        beam.transform.position = position;
        beam.transform.localScale = scale;
        SetColor(beam, new Color(0.18f, 0.20f, 0.22f));
    }

    void CreateLightPanel(Vector3 position)
    {
        var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "FreshTrain2 Ceiling Light Panel";
        panel.transform.SetParent(_root, false);
        panel.transform.position = position;
        panel.transform.localScale = new Vector3(1.95f, 0.035f, 0.42f);
        SetColor(panel, new Color(0.92f, 0.96f, 1f));

        var lightGo = new GameObject("FreshTrain2 Soft Factory Light");
        lightGo.transform.SetParent(_root, false);
        lightGo.transform.position = position + new Vector3(0f, -0.18f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 5.4f;
        light.intensity = 0.35f;
        light.color = new Color(0.78f, 0.88f, 1f);
    }

    void CreateSafetyStripe(string name, Vector3 position, Vector3 scale)
    {
        var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stripe.name = name;
        stripe.transform.SetParent(_root, false);
        stripe.transform.position = position;
        stripe.transform.localScale = scale;
        SetColor(stripe, new Color(1f, 0.72f, 0.08f));
    }

    GameObject Kit(string modelName, Vector3 position, Vector3 euler, Vector3 scale, bool alignBottomToY = true)
    {
        return Kit(modelName, position, euler, scale, _root, alignBottomToY);
    }

    GameObject Kit(string modelName, Vector3 position, Vector3 euler, Vector3 scale, Transform parent, bool alignBottomToY = true)
    {
        var prefab = Resources.Load<GameObject>(KitPath + modelName);
        GameObject go;
        if (prefab != null)
        {
            go = Instantiate(prefab, position, Quaternion.Euler(euler), parent);
            go.name = "FreshTrain2_Kenney_" + modelName;
            go.transform.localScale = Vector3.Scale(go.transform.localScale, scale);
            if (alignBottomToY) AlignBottomToY(go, position.y);
            DisableColliders(go);
            return go;
        }

        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "FreshTrain2_Fallback_" + modelName;
        go.transform.SetParent(parent, false);
        go.transform.position = alignBottomToY ? position + Vector3.up * (scale.y * 0.5f) : position;
        go.transform.rotation = Quaternion.Euler(euler);
        go.transform.localScale = scale;
        SetColor(go, new Color(0.36f, 0.40f, 0.44f));
        return go;
    }

    static void AlignBottomToY(GameObject go, float targetY)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        var deltaY = targetY - bounds.min.y;
        go.transform.position += Vector3.up * deltaY;
    }

    static void DisableColliders(GameObject go)
    {
        var colliders = go.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
            Destroy(collider);
    }

    static void SetColor(GameObject go, Color color)
    {
        var collider = go.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }
}

public class FreshTrain2InteractiveSceneBuilder : MonoBehaviour
{
    const float FloorY = 0f;
    public bool useLocalLayout;
    public float layoutScale = 1f;
    public float equipmentScale = 1f;
    public bool buildRouteMarkers = true;
    public bool compactTrainingEntranceLayout;

    Transform _root;

    void Start()
    {
        _root = new GameObject("FreshTrain2 Hub Interactive Elements").transform;
        if (useLocalLayout)
        {
            _root.SetParent(transform, false);
            _root.localPosition = Vector3.zero;
            _root.localRotation = Quaternion.identity;
            _root.localScale = Vector3.one * Mathf.Max(0.1f, layoutScale);
        }

        if (buildRouteMarkers)
            BuildRouteMarkers();

        if (compactTrainingEntranceLayout)
        {
            BuildPpeLocker(new Vector3(-9.0f, FloorY, -2.0f));
            BuildTaskDispatchConsole(new Vector3(-3.0f, FloorY, -2.0f));
            BuildInspectionPanel(new Vector3(3.0f, FloorY, -2.0f));
            BuildSafetyGate(new Vector3(9.0f, FloorY, -2.0f));
            BuildQualityScanner(new Vector3(-9.0f, FloorY, -7.2f));
            BuildMaintenanceBench(new Vector3(-3.0f, FloorY, -7.2f));
            BuildStorageCheckIn(new Vector3(3.0f, FloorY, -7.2f));
            BuildAlarmResetPost(new Vector3(9.0f, FloorY, -7.2f));
            return;
        }

        BuildPpeLocker(new Vector3(-7.3f, FloorY, -16.15f));
        BuildTaskDispatchConsole(new Vector3(-1.6f, FloorY, -14.95f));
        BuildInspectionPanel(new Vector3(4.3f, FloorY, -16.18f));
        BuildSafetyGate(new Vector3(-4.8f, FloorY, 10.3f));
        BuildQualityScanner(new Vector3(-3.2f, FloorY, 6.25f));
        BuildMaintenanceBench(new Vector3(-18.7f, FloorY, -11.2f));
        BuildStorageCheckIn(new Vector3(20.6f, FloorY, -13.0f));
        BuildAlarmResetPost(new Vector3(14.4f, FloorY, 6.6f));
    }

    void BuildPpeLocker(Vector3 position)
    {
        var root = CreateRoot("FreshTrain2 Interactive PPE Locker", position);

        Cube(root.transform, "Locker Cabinet", new Vector3(0f, 1.0f, 0f), new Vector3(1.15f, 2.0f, 0.42f), new Color(0.14f, 0.24f, 0.30f));
        var door = Cube(root.transform, "Locker Door", new Vector3(-0.30f, 1.0f, -0.24f), new Vector3(0.48f, 1.72f, 0.08f), new Color(0.18f, 0.43f, 0.56f));
        var indicator = Cube(root.transform, "PPE Status Light", new Vector3(0.40f, 1.65f, -0.31f), new Vector3(0.18f, 0.18f, 0.08f), new Color(1f, 0.7f, 0.16f));

        Cylinder(root.transform, "Helmet Marker", new Vector3(-0.22f, 2.16f, -0.24f), new Vector3(0.34f, 0.10f, 0.34f), new Color(1f, 0.78f, 0.16f));
        Label(root.transform, "PPE 工具柜", new Vector3(0f, 2.55f, -0.35f), 0.075f);

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.Checklist,
            "PPE 工具柜",
            "检查安全帽、护目镜和绝缘手套",
            "检查",
            "PPE 待确认",
            "PPE 检查完成",
            "靠近工具柜，逐项确认个人防护装备",
            door.transform,
            new[] { indicator.GetComponent<Renderer>() },
            null,
            "入厂前安全准备",
            "遗漏项、确认次数、准备用时");
        element.ConfigureTrainingLink("ppe_check", "PPE 安全装备检查", "SampleScene", 10);

        AddTrigger(root, new Vector3(1.8f, 2.2f, 1.55f), new Vector3(0f, 1.1f, 0f));
    }

    void BuildTaskDispatchConsole(Vector3 position)
    {
        var root = CreateRoot("FreshTrain2 Interactive Task Dispatch Console", position);

        Cube(root.transform, "Dispatch Desk", new Vector3(0f, 0.48f, 0f), new Vector3(1.65f, 0.42f, 0.78f), new Color(0.18f, 0.22f, 0.25f));
        var screen = Cube(root.transform, "Dispatch Screen", new Vector3(0f, 1.18f, -0.22f), new Vector3(1.38f, 0.72f, 0.10f), new Color(0.03f, 0.14f, 0.19f));
        var lineA = Cube(root.transform, "Dispatch Route Line A", new Vector3(-0.42f, 1.26f, -0.29f), new Vector3(0.42f, 0.045f, 0.035f), new Color(0.24f, 0.88f, 0.48f));
        var lineB = Cube(root.transform, "Dispatch Route Line B", new Vector3(0.18f, 1.12f, -0.29f), new Vector3(0.58f, 0.045f, 0.035f), new Color(1f, 0.70f, 0.16f));
        var lineC = Cube(root.transform, "Dispatch Route Line C", new Vector3(0.0f, 0.98f, -0.29f), new Vector3(0.92f, 0.045f, 0.035f), new Color(0.20f, 0.66f, 1f));

        Label(root.transform, "中央派工台", new Vector3(0f, 1.92f, -0.36f), 0.072f);

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.TaskDispatch,
            "中央派工台",
            "领取本次训练路线，串联安全、点检、作业与入库",
            "派工",
            "等待派工",
            "任务路线已确认",
            "派工台会给出完整上岗路线",
            screen.transform,
            new[] { lineA.GetComponent<Renderer>(), lineB.GetComponent<Renderer>(), lineC.GetComponent<Renderer>() },
            null,
            "任务领取与流程认知",
            "是否按路线完成、开始时间、路线节点");
        element.ConfigureTrainingLink("dispatch_console", "派工路线确认", "SampleScene", 11);

        AddTrigger(root, new Vector3(2.2f, 2.0f, 1.7f), new Vector3(0f, 1.0f, 0f));
    }

    void BuildInspectionPanel(Vector3 position)
    {
        var root = CreateRoot("FreshTrain2 Interactive Inspection Panel", position);

        Cube(root.transform, "Panel Stand", new Vector3(0f, 0.58f, 0.03f), new Vector3(0.22f, 1.16f, 0.22f), new Color(0.24f, 0.27f, 0.30f));
        var panel = Cube(root.transform, "Inspection Panel", new Vector3(0f, 1.36f, -0.02f), new Vector3(1.5f, 0.82f, 0.16f), new Color(0.07f, 0.14f, 0.18f));
        var lightA = Cube(root.transform, "Inspection Light A", new Vector3(-0.48f, 1.48f, -0.14f), new Vector3(0.18f, 0.18f, 0.06f), new Color(1f, 0.7f, 0.16f));
        var lightB = Cube(root.transform, "Inspection Light B", new Vector3(0f, 1.48f, -0.14f), new Vector3(0.18f, 0.18f, 0.06f), new Color(1f, 0.7f, 0.16f));
        var lightC = Cube(root.transform, "Inspection Light C", new Vector3(0.48f, 1.48f, -0.14f), new Vector3(0.18f, 0.18f, 0.06f), new Color(1f, 0.7f, 0.16f));

        Label(root.transform, "设备点检台", new Vector3(0f, 2.0f, -0.22f), 0.075f);

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.ControlPanel,
            "设备点检台",
            "按顺序完成电源、急停和联锁点检",
            "点检",
            "等待点检",
            "正在点检",
            "每次操作推进一个点检步骤",
            panel.transform,
            new[] { lightA.GetComponent<Renderer>(), lightB.GetComponent<Renderer>(), lightC.GetComponent<Renderer>() },
            null,
            "班前设备点检",
            "步骤顺序、遗漏项、点检用时");
        element.ConfigureTrainingLink("inspection_check", "设备点检训练", "SampleScene", 12);

        AddTrigger(root, new Vector3(2.2f, 2.0f, 1.7f), new Vector3(0f, 1.0f, 0f));
    }

    void BuildSafetyGate(Vector3 position)
    {
        var root = CreateRoot("FreshTrain2 Interactive Safety Gate", position);

        Cube(root.transform, "Gate Post Left", new Vector3(-0.78f, 0.68f, 0f), new Vector3(0.16f, 1.36f, 0.16f), new Color(0.15f, 0.17f, 0.19f));
        Cube(root.transform, "Gate Post Right", new Vector3(0.78f, 0.68f, 0f), new Vector3(0.16f, 1.36f, 0.16f), new Color(0.15f, 0.17f, 0.19f));
        var arm = Cube(root.transform, "Gate Arm", new Vector3(0f, 0.95f, -0.02f), new Vector3(1.55f, 0.14f, 0.12f), new Color(1f, 0.74f, 0.12f));
        var indicator = Cube(root.transform, "Gate Status Light", new Vector3(0.0f, 1.45f, -0.02f), new Vector3(0.22f, 0.22f, 0.12f), new Color(1f, 0.7f, 0.16f));

        Label(root.transform, "安全闸门", new Vector3(0f, 1.86f, -0.20f), 0.07f);

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.SafetyGate,
            "分拣线安全闸门",
            "开合护栏，确认进入设备观察区的隔离状态",
            "开合",
            "闸门关闭",
            "闸门打开",
            "使用闸门前先确认传送线停稳",
            arm.transform,
            new[] { indicator.GetComponent<Renderer>() },
            null,
            "危险区准入控制",
            "是否违规进入、开关顺序");
        element.ConfigureTrainingLink("safety_gate", "安全闸门开合训练", "SampleScene", 13);

        AddTrigger(root, new Vector3(2.6f, 1.7f, 1.7f), new Vector3(0f, 0.85f, 0f));
    }

    void BuildQualityScanner(Vector3 position)
    {
        var root = CreateRoot("FreshTrain2 Interactive Quality Scanner", position);

        Cube(root.transform, "Scanner Left Post", new Vector3(-0.66f, 0.82f, 0f), new Vector3(0.18f, 1.64f, 0.28f), new Color(0.18f, 0.23f, 0.27f));
        Cube(root.transform, "Scanner Right Post", new Vector3(0.66f, 0.82f, 0f), new Vector3(0.18f, 1.64f, 0.28f), new Color(0.18f, 0.23f, 0.27f));
        var scannerHead = Cube(root.transform, "Scanner Head", new Vector3(0f, 1.68f, 0f), new Vector3(1.52f, 0.20f, 0.34f), new Color(0.08f, 0.22f, 0.30f));
        var beam = Cube(root.transform, "Scanner Beam", new Vector3(0f, 0.94f, -0.03f), new Vector3(1.12f, 0.035f, 0.035f), new Color(0.20f, 0.66f, 1f));
        var rejectLight = Cube(root.transform, "Scanner Reject Light", new Vector3(0.42f, 1.92f, -0.12f), new Vector3(0.18f, 0.18f, 0.08f), new Color(1f, 0.30f, 0.18f));
        var passLight = Cube(root.transform, "Scanner Pass Light", new Vector3(-0.42f, 1.92f, -0.12f), new Vector3(0.18f, 0.18f, 0.08f), new Color(0.24f, 0.88f, 0.48f));

        Label(root.transform, "质检扫码门", new Vector3(0f, 2.28f, -0.28f), 0.070f);

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.QualityScanner,
            "质检扫码门",
            "识别物料状态，判断合格、返修或人工复核",
            "扫码",
            "等待样本",
            "质检样本已归档",
            "靠近后模拟扫描不同物料样本",
            scannerHead.transform,
            new[] { beam.GetComponent<Renderer>(), rejectLight.GetComponent<Renderer>(), passLight.GetComponent<Renderer>() },
            null,
            "质量识别与分流判断",
            "判定正确率、误判次数、处理速度");
        element.ConfigureTrainingLink("quality_scan", "质检扫码训练", "SampleScene", 14);

        AddTrigger(root, new Vector3(2.4f, 2.4f, 1.9f), new Vector3(0f, 1.2f, 0f));
    }

    void BuildMaintenanceBench(Vector3 position)
    {
        var root = CreateRoot("FreshTrain2 Interactive Maintenance Bench", position);

        Cube(root.transform, "Maintenance Table", new Vector3(0f, 0.55f, 0f), new Vector3(2.25f, 0.30f, 0.82f), new Color(0.22f, 0.18f, 0.15f));
        Cube(root.transform, "Maintenance Backboard", new Vector3(0f, 1.35f, 0.34f), new Vector3(2.25f, 1.22f, 0.12f), new Color(0.13f, 0.17f, 0.19f));
        var toolA = Cube(root.transform, "Insulated Glove Tool", new Vector3(-0.66f, 0.86f, -0.06f), new Vector3(0.36f, 0.12f, 0.18f), new Color(1f, 0.78f, 0.16f));
        var toolB = Cube(root.transform, "Torque Wrench Tool", new Vector3(0f, 0.88f, -0.06f), new Vector3(0.68f, 0.08f, 0.12f), new Color(0.72f, 0.78f, 0.84f));
        var toolC = Cube(root.transform, "Checklist Tool", new Vector3(0.66f, 0.88f, -0.06f), new Vector3(0.34f, 0.24f, 0.08f), new Color(0.20f, 0.66f, 1f));

        Label(root.transform, "维修工具台", new Vector3(0f, 2.14f, -0.18f), 0.070f);

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.MaintenanceBench,
            "维修工具台",
            "根据作业类型选择绝缘手套、扭矩扳手或点检表",
            "选择",
            "工具待选择",
            "工具选择中",
            "选择工具前要先判断作业风险",
            toolB.transform,
            new[] { toolA.GetComponent<Renderer>(), toolB.GetComponent<Renderer>(), toolC.GetComponent<Renderer>() },
            null,
            "维修前工具识别",
            "工具选择正确率、误选次数");
        element.ConfigureTrainingLink("tool_select", "维修工具选择训练", "SampleScene", 15);

        AddTrigger(root, new Vector3(2.8f, 2.0f, 1.8f), new Vector3(0f, 1.0f, 0f));
    }

    void BuildStorageCheckIn(Vector3 position)
    {
        var root = CreateRoot("FreshTrain2 Interactive Storage Check In", position);

        Cube(root.transform, "Storage Console", new Vector3(0f, 0.62f, 0f), new Vector3(1.5f, 0.52f, 0.62f), new Color(0.18f, 0.25f, 0.26f));
        var screen = Cube(root.transform, "Storage Screen", new Vector3(0f, 1.12f, -0.22f), new Vector3(1.08f, 0.38f, 0.06f), new Color(0.04f, 0.16f, 0.12f));
        var barcode = Cube(root.transform, "Barcode Reader", new Vector3(0.48f, 0.94f, -0.30f), new Vector3(0.24f, 0.16f, 0.10f), new Color(0.24f, 0.88f, 0.48f));

        for (int i = 0; i < 4; i++)
            Cube(root.transform, "Checked Storage Box " + i, new Vector3(-0.66f + i * 0.44f, 0.32f, 0.58f), new Vector3(0.34f, 0.34f, 0.34f), new Color(0.25f, 0.37f, 0.48f));

        Label(root.transform, "成品入库台", new Vector3(0f, 1.72f, -0.25f), 0.070f);

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.StorageCheckIn,
            "成品入库台",
            "完成训练后确认成品批次，形成闭环记录",
            "入库",
            "等待入库",
            "库存已更新",
            "用于记录完成件数和批次追踪",
            screen.transform,
            new[] { screen.GetComponent<Renderer>(), barcode.GetComponent<Renderer>() },
            null,
            "作业完成确认",
            "完成数量、入库确认、批次记录");
        element.ConfigureTrainingLink("storage_checkin", "成品入库确认训练", "SampleScene", 16);

        AddTrigger(root, new Vector3(2.3f, 1.8f, 1.9f), new Vector3(0f, 0.9f, 0f));
    }

    void BuildAlarmResetPost(Vector3 position)
    {
        var root = CreateRoot("FreshTrain2 Interactive Alarm Reset", position);

        Cylinder(root.transform, "Reset Post", new Vector3(0f, 0.55f, 0f), new Vector3(0.28f, 0.55f, 0.28f), new Color(0.24f, 0.27f, 0.30f));
        var button = Cylinder(root.transform, "Reset Button", new Vector3(0f, 1.18f, -0.02f), new Vector3(0.34f, 0.10f, 0.34f), new Color(1f, 0.3f, 0.18f));
        var beacon = Cylinder(root.transform, "Alarm Beacon", new Vector3(0f, 1.72f, 0f), new Vector3(0.28f, 0.28f, 0.28f), new Color(1f, 0.3f, 0.18f));

        var lightGo = new GameObject("FreshTrain2 Alarm Reset Light");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.78f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 3.6f;
        light.intensity = 0.8f;

        Label(root.transform, "告警复位", new Vector3(0f, 2.16f, -0.18f), 0.07f);

        var element = root.AddComponent<FreshTrain2InteractiveElement>();
        element.Configure(
            FreshTrain2InteractiveElementType.AlarmReset,
            "告警复位柱",
            "模拟现场告警后的确认与复位",
            "复位",
            "红灯告警",
            "告警已复位",
            "靠近后执行复位，观察状态灯变化",
            button.transform,
            new[] { button.GetComponent<Renderer>(), beacon.GetComponent<Renderer>() },
            light,
            "异常处理与恢复确认",
            "反应时间、复位次数、是否按流程确认");
        element.ConfigureTrainingLink("alarm_reset", "告警复位训练", "SampleScene", 17);

        AddTrigger(root, new Vector3(1.9f, 2.2f, 1.9f), new Vector3(0f, 1.1f, 0f));
    }

    void BuildRouteMarkers()
    {
        RouteMarker(new Vector3(-7.3f, 0.035f, -14.2f), "01 安全准备", new Color(1f, 0.78f, 0.16f));
        RouteMarker(new Vector3(-1.6f, 0.035f, -13.1f), "02 派工", new Color(0.20f, 0.66f, 1f));
        RouteMarker(new Vector3(4.3f, 0.035f, -14.2f), "03 点检", new Color(0.24f, 0.88f, 0.48f));
        RouteMarker(new Vector3(-4.8f, 0.035f, 8.8f), "04 准入", new Color(1f, 0.70f, 0.16f));
        RouteMarker(new Vector3(-3.2f, 0.035f, 7.8f), "05 质检", new Color(0.20f, 0.66f, 1f));
        RouteMarker(new Vector3(20.6f, 0.035f, -11.2f), "06 入库", new Color(0.38f, 0.78f, 0.58f));
    }

    void RouteMarker(Vector3 position, string text, Color color)
    {
        var marker = Cube(_root, "FreshTrain2 Route Marker " + text, position, new Vector3(2.1f, 0.035f, 0.72f), color);
        marker.transform.localRotation = Quaternion.identity;

        var labelGo = new GameObject("FreshTrain2 " + text + " Floor Label");
        labelGo.transform.SetParent(_root, false);
        labelGo.transform.localPosition = position + new Vector3(0f, 0.04f, -0.02f);
        labelGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.12f;
        label.fontSize = 42;
        label.color = Color.black;
    }

    GameObject CreateRoot(string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_root, false);
        if (useLocalLayout)
            go.transform.localPosition = position;
        else
            go.transform.position = position;
        go.transform.localScale = Vector3.one * Mathf.Max(0.1f, equipmentScale);
        return go;
    }

    static GameObject Cube(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = scale;
        SetColor(go, color);
        RemoveCollider(go);
        return go;
    }

    static GameObject Cylinder(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = scale;
        SetColor(go, color);
        RemoveCollider(go);
        return go;
    }

    static void Label(Transform parent, string text, Vector3 localPosition, float characterSize)
    {
        var labelGo = new GameObject(text + " Label");
        labelGo.transform.SetParent(parent, false);
        labelGo.transform.localPosition = localPosition;
        labelGo.AddComponent<FreshTrain2FaceCameraLabel>();

        var label = labelGo.AddComponent<TextMesh>();
        label.text = CompactLabel(text);
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = Mathf.Min(characterSize, text.Length > 5 ? 0.052f : 0.060f);
        label.fontSize = text.Length > 5 ? 28 : 32;
        label.color = Color.white;
    }

    static string CompactLabel(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= 5) return text;
        int split = Mathf.CeilToInt(text.Length * 0.5f);
        return text.Substring(0, split) + "\n" + text.Substring(split);
    }

    static void AddTrigger(GameObject root, Vector3 size, Vector3 center)
    {
        var trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = size;
        trigger.center = center;

        var element = root.GetComponent<FreshTrain2InteractiveElement>();
        if (element != null)
            element.interactionRadius = Mathf.Max(3.4f, Mathf.Max(size.x, size.z) * 0.75f + 1.4f);
    }

    static void RemoveCollider(GameObject go)
    {
        var collider = go.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
    }

    static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }
}

public class FreshTrain2FaceCameraLabel : MonoBehaviour
{
    public bool rotateOnlyAroundY = true;

    void LateUpdate()
    {
        if (Camera.main == null) return;

        var direction = Camera.main.transform.position - transform.position;
        if (rotateOnlyAroundY)
            direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}

public static class FreshTrain2GuiStyles
{
    static bool _ready;
    static Texture2D _cardTexture;
    static Texture2D _strongCardTexture;
    static Texture2D _accentTexture;
    static Texture2D _successTexture;
    static Texture2D _buttonTexture;

    public static GUIStyle SectionTitle { get; private set; }
    public static GUIStyle Body { get; private set; }
    public static GUIStyle Muted { get; private set; }
    public static GUIStyle ActionKey { get; private set; }
    public static GUIStyle CenterTitle { get; private set; }

    public static Texture2D AccentTexture { get { Ensure(); return _accentTexture; } }
    public static Texture2D SuccessTexture { get { Ensure(); return _successTexture; } }

    public static void Ensure()
    {
        if (_ready) return;

        _cardTexture = MakeTexture(new Color(0.055f, 0.070f, 0.090f, 0.82f));
        _strongCardTexture = MakeTexture(new Color(0.070f, 0.095f, 0.120f, 0.92f));
        _accentTexture = MakeTexture(new Color(0.16f, 0.72f, 0.94f, 1f));
        _successTexture = MakeTexture(new Color(0.22f, 0.86f, 0.52f, 1f));
        _buttonTexture = MakeTexture(new Color(0.13f, 0.42f, 0.60f, 0.95f));

        SectionTitle = new GUIStyle(GUI.skin.label);
        SectionTitle.fontSize = 15;
        SectionTitle.fontStyle = FontStyle.Bold;
        SectionTitle.normal.textColor = new Color(0.88f, 0.96f, 1f, 1f);
        SectionTitle.wordWrap = true;

        CenterTitle = new GUIStyle(SectionTitle);
        CenterTitle.alignment = TextAnchor.MiddleCenter;

        Body = new GUIStyle(GUI.skin.label);
        Body.fontSize = 14;
        Body.normal.textColor = new Color(0.92f, 0.95f, 0.97f, 1f);
        Body.wordWrap = true;

        Muted = new GUIStyle(Body);
        Muted.fontSize = 12;
        Muted.normal.textColor = new Color(0.65f, 0.71f, 0.76f, 1f);

        ActionKey = new GUIStyle(Body);
        ActionKey.fontSize = 18;
        ActionKey.fontStyle = FontStyle.Bold;
        ActionKey.alignment = TextAnchor.MiddleCenter;
        ActionKey.normal.background = _buttonTexture;
        ActionKey.padding = new RectOffset(12, 12, 5, 5);

        _ready = true;
    }

    public static void DrawCard(Rect rect, bool strong = false)
    {
        Ensure();
        GUI.DrawTexture(rect, strong ? _strongCardTexture : _cardTexture);
    }

    public static void DrawAccentBar(Rect rect)
    {
        Ensure();
        GUI.DrawTexture(rect, _accentTexture);
    }

    public static void DrawDot(Rect rect, Texture2D texture)
    {
        Ensure();
        GUI.DrawTexture(rect, texture);
    }

    static Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        return texture;
    }
}
