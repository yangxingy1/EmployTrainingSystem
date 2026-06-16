using UnityEngine;

public class HubTrainingSceneEntry : MonoBehaviour
{
    HandInput _hand;
    HandVisual _handVisual;
    GraspController _grasp;

    void Start()
    {
        SetupCamera();
        SetupLighting();
        SetupWorkbench();
        CreateVirtualHand();
        TrainingFlowController.EnsureExists(CurrentTaskId());
        CreateTask();
        gameObject.AddComponent<ReturnToHubInput>();
    }

    void CreateVirtualHand()
    {
        var handGo = new GameObject("VirtualHand");
        _hand = handGo.AddComponent<HandInput>();
        _hand.url = "ws://127.0.0.1:8765";
        _hand.planeWidth = 5.4f;
        _hand.planeHeight = 3.6f;
        _hand.gain = 1.35f;
        _hand.smoothing = 0.68f;
        _hand.graceTime = 0.45f;

        _handVisual = handGo.AddComponent<HandVisual>();
        _handVisual.jointRadius = 0.045f;
        _handVisual.skinColor = new Color(0.96f, 0.78f, 0.62f);
        _handVisual.gripColor = new Color(0.28f, 0.9f, 0.5f);
        _handVisual.enablePhysicalColliders = false;
    }

    void CreateTask()
    {
        string taskId = CurrentTaskId();
        if (string.IsNullOrEmpty(taskId)) taskId = "rotary_valve";

        switch (taskId)
        {
            case "rotary_valve":
                CreateValveRotateTask();
                break;
            case "electric_switch":
                CreateElectricSwitchTask();
                break;
            case "sort_line":
                CreateConveyorSortingTask();
                break;
            case "button_press":
                CreateButtonPressTask();
                break;
            case "slider_calibration":
                CreateLinearSliderTask();
                break;
            case "emergency_stop":
                CreateEmergencyStopTask();
                break;
            case "mode_selector":
                CreateModeSelectorTask();
                break;
            case "bolt_tightening":
                CreateBoltTighteningTask();
                break;
            case "pick_place":
            case "material_transfer":
                CreatePickPlaceTask();
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
                CreateProcessStepTask(taskId);
                break;
            default:
                CreateValveRotateTask();
                break;
        }
    }

    string CurrentTaskId()
    {
        var session = SessionManager.EnsureExists();
        return session != null ? session.selectedTaskId : "";
    }

    GraspController EnsureGrasp()
    {
        if (_grasp != null) return _grasp;

        var interactionGo = new GameObject("InteractionRig");
        _grasp = interactionGo.AddComponent<GraspController>();
        _grasp.hand = _hand;
        _grasp.handVisual = _handVisual;
        _grasp.grabThreshold = 0.58f;
        _grasp.releaseThreshold = 0.30f;
        _grasp.releaseGraceSeconds = 0.05f;
        _grasp.grabConfirmSeconds = 0.08f;
        _grasp.allowFistGrip = false;
        _grasp.followStrength = 0.78f;
        _grasp.maxFollowSpeed = 10f;
        _grasp.throwScale = 0f;
        _grasp.maxThrowSpeed = 0f;
        _grasp.useGravityOnRelease = true;
        _grasp.lockZToPlane = true;
        _grasp.planeZ = 0f;
        _grasp.carryMagnetism = 0f;
        return _grasp;
    }

    void CreateConveyorSortingTask()
    {
        var taskGo = new GameObject("Task_ConveyorSorting");
        var task = taskGo.AddComponent<ConveyorSortingTrainingTask>();
        task.grasp = EnsureGrasp();
    }

    void CreatePickPlaceTask()
    {
        var taskGo = new GameObject("Task_PickPlace");
        var task = taskGo.AddComponent<PickPlaceTask>();
        task.grasp = EnsureGrasp();
    }

    void CreateButtonPressTask()
    {
        var taskGo = new GameObject("Task_ButtonPress");
        var task = taskGo.AddComponent<ButtonPressTrainingTask>();
        task.hand = _hand;
        task.grasp = EnsureGrasp();
    }

    void CreateValveRotateTask()
    {
        var taskGo = new GameObject("Task_ValveRotate");
        var task = taskGo.AddComponent<ValveRotateTrainingTask>();
        task.grasp = EnsureGrasp();
    }

    void CreateElectricSwitchTask()
    {
        var taskGo = new GameObject("Task_ElectricSwitch");
        var task = taskGo.AddComponent<ElectricSwitchTask>();
        task.hand = _hand;
    }

    void CreateLinearSliderTask()
    {
        var taskGo = new GameObject("Task_LinearSlider");
        var task = taskGo.AddComponent<LinearSliderTrainingTask>();
        task.hand = _hand;
    }

    void CreateEmergencyStopTask()
    {
        var taskGo = new GameObject("Task_EmergencyStop");
        var task = taskGo.AddComponent<EmergencyStopTrainingTask>();
        task.hand = _hand;
    }

    void CreateModeSelectorTask()
    {
        var taskGo = new GameObject("Task_ModeSelector");
        var task = taskGo.AddComponent<ModeSelectorTrainingTask>();
        task.hand = _hand;
    }

    void CreateBoltTighteningTask()
    {
        var taskGo = new GameObject("Task_BoltTightening");
        var task = taskGo.AddComponent<BoltTighteningTrainingTask>();
        task.hand = _hand;
    }

    void CreateProcessStepTask(string scenarioId)
    {
        var taskGo = new GameObject("Task_ProcessStep_" + scenarioId);
        var task = taskGo.AddComponent<ProcessStepTrainingTask>();
        task.hand = _hand;
        task.grasp = EnsureGrasp();
        task.scenarioId = scenarioId;
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
