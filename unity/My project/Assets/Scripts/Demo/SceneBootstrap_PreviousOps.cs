using UnityEngine;

public class SceneBootstrap_PreviousOps : MonoBehaviour
{
    public TrainingSceneKind sceneKind = TrainingSceneKind.RotaryValve;

    void Start()
    {
        SetupCamera();
        SetupLighting();
        SetupWorkbench();

        var handInput = CreateVirtualHand(out var handVisual);
        string taskId = CurrentTaskId();
        string flowTaskId = !string.IsNullOrEmpty(taskId)
            ? taskId
            : (sceneKind == TrainingSceneKind.RotaryValve ? "rotary_valve" : "electric_switch");

        TrainingFlowController.EnsureExists(flowTaskId);

        if (!string.IsNullOrEmpty(taskId))
            CreateAssignedTask(taskId, handInput, handVisual);
        else if (sceneKind == TrainingSceneKind.RotaryValve)
            CreateRotaryValvePractice(handInput);
        else
            CreateElectricSwitchPractice(handInput);

        if (FindObjectOfType<ReturnToHubInput>() == null)
            gameObject.AddComponent<ReturnToHubInput>();
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

    static void CreateAssignedTask(string taskId, HandInput handInput, HandVisual handVisual)
    {
        switch (taskId)
        {
            case "rotary_valve":
                CreateRotaryValvePractice(handInput);
                break;
            case "electric_switch":
                CreateElectricSwitchPractice(handInput);
                break;
            case "sort_line":
                CreateConveyorSortingPractice(handInput, handVisual);
                break;
            case "integrated_exam":
                CreateFreeMovePractice(handInput, handVisual);
                break;
            case "button_press":
                CreateButtonPressPractice(handInput, handVisual);
                break;
            case "slider_calibration":
                CreateLinearSliderPractice(handInput);
                break;
            case "emergency_stop":
                CreateEmergencyStopPractice(handInput);
                break;
            case "mode_selector":
                CreateModeSelectorPractice(handInput);
                break;
            case "bolt_tightening":
                CreateBoltTighteningPractice(handInput);
                break;
            case "material_transfer":
                CreatePickPlacePractice(handInput, handVisual);
                break;
            case "ppe_check":
            case "dispatch_console":
            case "inspection_check":
            case "safety_gate":
            case "quality_scan":
            case "tool_select":
            case "storage_checkin":
            case "alarm_reset":
            case "central_control":
                CreateProcessStepPractice(taskId, handInput, handVisual);
                break;
            default:
                CreateRotaryValvePractice(handInput);
                break;
        }
    }

    static string CurrentTaskId()
    {
        var session = SessionManager.EnsureExists();
        return session != null ? session.selectedTaskId : "";
    }

    static GraspController CreateGraspController(HandInput handInput, HandVisual handVisual)
    {
        var interactionGo = new GameObject("InteractionRig");

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

        return grasp;
    }

    static void CreateFreeMovePractice(HandInput handInput, HandVisual handVisual)
    {
        var taskGo = new GameObject("FreeMovePractice");
        var task = taskGo.AddComponent<FreeMoveTask>();
        task.grasp = CreateGraspController(handInput, handVisual);
    }

    static void CreateConveyorSortingPractice(HandInput handInput, HandVisual handVisual)
    {
        var taskGo = new GameObject("ConveyorSortingPractice");
        var task = taskGo.AddComponent<ConveyorSortingTrainingTask>();
        task.grasp = CreateGraspController(handInput, handVisual);
    }

    static void CreatePickPlacePractice(HandInput handInput, HandVisual handVisual)
    {
        var taskGo = new GameObject("PickPlacePractice");
        var task = taskGo.AddComponent<PickPlaceTask>();
        task.grasp = CreateGraspController(handInput, handVisual);
    }

    static void CreateButtonPressPractice(HandInput handInput, HandVisual handVisual)
    {
        var taskGo = new GameObject("ButtonPressPractice");
        var task = taskGo.AddComponent<ButtonPressTrainingTask>();
        task.hand = handInput;
        task.grasp = CreateGraspController(handInput, handVisual);
    }

    static void CreateLinearSliderPractice(HandInput handInput)
    {
        var taskGo = new GameObject("LinearSliderPractice");
        var task = taskGo.AddComponent<LinearSliderTrainingTask>();
        task.hand = handInput;
    }

    static void CreateEmergencyStopPractice(HandInput handInput)
    {
        var taskGo = new GameObject("EmergencyStopPractice");
        var task = taskGo.AddComponent<EmergencyStopTrainingTask>();
        task.hand = handInput;
    }

    static void CreateModeSelectorPractice(HandInput handInput)
    {
        var taskGo = new GameObject("ModeSelectorPractice");
        var task = taskGo.AddComponent<ModeSelectorTrainingTask>();
        task.hand = handInput;
    }

    static void CreateBoltTighteningPractice(HandInput handInput)
    {
        var taskGo = new GameObject("BoltTighteningPractice");
        var task = taskGo.AddComponent<BoltTighteningTrainingTask>();
        task.hand = handInput;
    }

    static void CreateProcessStepPractice(string scenarioId, HandInput handInput, HandVisual handVisual)
    {
        var taskGo = new GameObject("ProcessStepPractice_" + scenarioId);
        var task = taskGo.AddComponent<ProcessStepTrainingTask>();
        task.hand = handInput;
        task.grasp = CreateGraspController(handInput, handVisual);
        task.scenarioId = scenarioId;
    }

    static void CreateElectricSwitchPractice(HandInput handInput)
    {
        var taskGo = new GameObject("ElectricSwitchPractice");
        var task = taskGo.AddComponent<ElectricSwitchTask>();
        task.hand = handInput;
    }

    static void CreateRotaryValvePractice(HandInput handInput)
    {
        var taskGo = new GameObject("RotaryValvePractice");
        var task = taskGo.AddComponent<RotaryValveTask>();
        task.hand = handInput;
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
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }
}
