using UnityEngine;

public class HubWorldBootstrap : MonoBehaviour
{
    void Start()
    {
        var session = EnsureSession();
        var handTracking = MockHandTrackingService.EnsureExists();

        SetupLighting();
        SetupHall();
        CreateReadyStatusHud(handTracking);
        CreateTaskStations(session);
        CreatePlayer(session);
    }

    static SessionManager EnsureSession()
    {
        var session = SessionManager.EnsureExists();
        session.Initialize(new MockAssignedTaskSource());
        return session;
    }

    static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.48f, 0.50f, 0.54f);

        var lightGo = new GameObject("HubWorld Key Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.95f;
        light.color = new Color(1.0f, 0.96f, 0.88f);
        lightGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
    }

    static void SetupHall()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Hub Floor";
        floor.transform.position = new Vector3(0f, -0.05f, 0f);
        floor.transform.localScale = new Vector3(22f, 0.1f, 16f);
        SetColor(floor, new Color(0.28f, 0.30f, 0.33f));

        CreateWall("North Wall", new Vector3(0f, 1.5f, 8f), new Vector3(22f, 3f, 0.25f));
        CreateWall("South Wall", new Vector3(0f, 1.5f, -8f), new Vector3(22f, 3f, 0.25f));
        CreateWall("East Wall", new Vector3(11f, 1.5f, 0f), new Vector3(0.25f, 3f, 16f));
        CreateWall("West Wall", new Vector3(-11f, 1.5f, 0f), new Vector3(0.25f, 3f, 16f));

        for (int i = 0; i < 4; i++)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Hub Pillar " + (i + 1);
            pillar.transform.position = new Vector3(i < 2 ? -7f : 7f, 1.5f, i % 2 == 0 ? -4.5f : 4.5f);
            pillar.transform.localScale = new Vector3(0.65f, 1.5f, 0.65f);
            SetColor(pillar, new Color(0.43f, 0.46f, 0.50f));
        }
    }

    static void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        SetColor(wall, new Color(0.18f, 0.20f, 0.23f));
    }

    static void CreatePlayer(SessionManager session)
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Hub Player";
        player.transform.position = session.hasHubReturnPosition ? session.hubReturnPosition : new Vector3(0f, 1f, -4f);
        player.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        SetColor(player, new Color(0.18f, 0.56f, 0.82f));

        var capsuleCollider = player.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null) Destroy(capsuleCollider);

        var controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.38f;
        controller.center = Vector3.zero;

        var cameraGo = new GameObject("Hub Follow Camera");
        cameraGo.tag = "MainCamera";
        var camera = cameraGo.AddComponent<Camera>();
        camera.fieldOfView = 62f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.10f, 0.12f, 0.15f);
        cameraGo.AddComponent<AudioListener>();

        var playerController = player.AddComponent<PlayerController>();
        playerController.followCamera = cameraGo.transform;

        player.AddComponent<HubInteractor>();
    }

    static void CreateTaskStations(SessionManager session)
    {
        var spawnerGo = new GameObject("TaskStationSpawner");
        var spawner = spawnerGo.AddComponent<TaskStationSpawner>();
        spawner.SpawnStations(session.assignedTasks);
    }

    static void CreateReadyStatusHud(IHandTrackingService handTrackingService)
    {
        var hudGo = new GameObject("ReadyStatusHud");
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
}
