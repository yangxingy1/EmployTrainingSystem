using UnityEngine;

public class SceneBootstrap : MonoBehaviour
{
    void Start()
    {
        SetupCamera();
        SetupLighting();
        SetupWorkbench();

        var handInput = CreateVirtualHand(out var handVisual);
        CreateFreeMovePractice(handInput, handVisual);
    }

    static HandInput CreateVirtualHand(out HandVisual visual)
    {
        var handGo = new GameObject("VirtualHand");
        var input = handGo.AddComponent<HandInput>();
        input.url = "ws://127.0.0.1:8765";
        input.planeWidth = 5.4f;
        input.planeHeight = 3.6f;
        input.gain = 1.35f;
        input.smoothing = 0.68f;
        input.graceTime = 0.45f;

        visual = handGo.AddComponent<HandVisual>();
        visual.jointRadius = 0.045f;
        visual.skinColor = new Color(0.96f, 0.78f, 0.62f);
        visual.gripColor = new Color(0.28f, 0.9f, 0.5f);
        visual.enablePhysicalColliders = false;

        return input;
    }

    static void CreateFreeMovePractice(HandInput handInput, HandVisual handVisual)
    {
        var interactionGo = new GameObject("FreeMovePractice");

        var grasp = interactionGo.AddComponent<GraspController>();
        grasp.hand = handInput;
        grasp.handVisual = handVisual;
        grasp.grabThreshold = 0.58f;
        grasp.releaseThreshold = 0.30f;
        grasp.releaseGraceSeconds = 0.05f;
        grasp.grabConfirmSeconds = 0.08f;
        grasp.allowFistGrip = false;
        grasp.followStrength = 0.78f;
        grasp.maxFollowSpeed = 10f;
        grasp.throwScale = 0.0f;
        grasp.maxThrowSpeed = 0f;
        grasp.useGravityOnRelease = true;
        grasp.lockZToPlane = true;
        grasp.planeZ = 0f;
        grasp.carryMagnetism = 0.0f;

        var task = interactionGo.AddComponent<FreeMoveTask>();
        task.grasp = grasp;
    }

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        cam.transform.position = new Vector3(0f, -0.05f, -6.2f);
        cam.transform.rotation = Quaternion.Euler(2f, 0f, 0f);
        cam.fieldOfView = 48f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.10f, 0.13f);
    }

    static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.40f, 0.42f, 0.46f);

        var keyGo = new GameObject("KeyLight");
        var key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 0.85f;
        key.color = new Color(1f, 0.96f, 0.88f);
        keyGo.transform.rotation = Quaternion.Euler(42f, -125f, 0f);

        var fillGo = new GameObject("FillLight");
        var fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.35f;
        fill.color = new Color(0.65f, 0.78f, 1f);
        fillGo.transform.rotation = Quaternion.Euler(-20f, 35f, 0f);
    }

    static void SetupWorkbench()
    {
        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "TrainingWorkbench";
        table.transform.position = new Vector3(0f, -1.63f, 0.24f);
        table.transform.localScale = new Vector3(6.1f, 0.18f, 1.1f);
        SetColor(table, new Color(0.21f, 0.23f, 0.27f));

        var backPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backPanel.name = "BackPanel";
        backPanel.transform.position = new Vector3(0f, 1.05f, 0.55f);
        backPanel.transform.localScale = new Vector3(6.4f, 0.10f, 0.35f);
        SetColor(backPanel, new Color(0.16f, 0.18f, 0.22f));

        var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = "TransferGuideRail";
        rail.transform.position = new Vector3(0f, -1.35f, -0.03f);
        rail.transform.localScale = new Vector3(4.4f, 0.035f, 0.035f);
        SetColor(rail, new Color(0.48f, 0.54f, 0.62f));
    }

    static void SetColor(GameObject go, Color color)
    {
        var mat = go.GetComponent<Renderer>().material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }
}
