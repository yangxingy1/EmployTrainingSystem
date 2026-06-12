using UnityEngine;

/// <summary>
/// 阀门+按钮培训场景一键启动。
/// 程序化搭建: 摄像机 → 光源 → 工作台 → 阀门 → 按钮 → 虚拟手 → HandGestureDriver
/// </summary>
public class ValveButtonBootstrap : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        SetupCamera();
        SetupLighting();
        SetupWorkbench();

        var handInput = CreateVirtualHand(out var handVisual);
        var (valve, button) = CreateValveAndButton();
        CreateGestureDriver(handInput, valve, button);

        gameObject.AddComponent<TaskReturnHandler>();
        Debug.Log("[ValveButtonBootstrap] 阀门按钮场景已就绪。将手移到阀门旋转,移到按钮点击。");
    }

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        cam.transform.position = new Vector3(0f, -0.08f, -6.2f);
        cam.transform.rotation = Quaternion.Euler(2f, 0f, 0f);
        cam.fieldOfView = 50f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
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
        table.transform.position = new Vector3(0f, -1.63f, 0.2f);
        table.transform.localScale = new Vector3(6.2f, 0.18f, 1.1f);
        SetColor(table, new Color(0.21f, 0.23f, 0.27f));

        var backPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backPanel.name = "BackPanel";
        backPanel.transform.position = new Vector3(0f, 1.05f, 0.52f);
        backPanel.transform.localScale = new Vector3(6.5f, 0.10f, 0.35f);
        SetColor(backPanel, new Color(0.16f, 0.18f, 0.22f));
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

    static (ValveInteractable, ButtonInteractable) CreateValveAndButton()
    {
        // 阀门 — 在工作台左侧
        var valveGo = new GameObject("Valve");
        valveGo.transform.position = new Vector3(-1.6f, -0.45f, 0f);
        // 阀门视觉: 一个扁圆柱作为手轮
        var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "ValveWheel";
        wheel.transform.SetParent(valveGo.transform, false);
        wheel.transform.localScale = new Vector3(0.55f, 0.04f, 0.55f);
        wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        SetColor(wheel, new Color(0.85f, 0.22f, 0.18f));
        var valve = valveGo.AddComponent<ValveInteractable>();
        valve.valveWheel = wheel.transform;

        // 阀门标签
        var valveLabel = new GameObject("ValveLabel");
        valveLabel.transform.position = new Vector3(-1.6f, -0.45f, -0.35f);
        var vText = valveLabel.AddComponent<TextMesh>();
        vText.text = "VALVE";
        vText.anchor = TextAnchor.MiddleCenter;
        vText.alignment = TextAlignment.Center;
        vText.fontSize = 42;
        vText.characterSize = 0.04f;
        vText.color = new Color(0.82f, 0.86f, 1f);

        // 按钮 — 在工作台右侧
        var buttonGo = new GameObject("Button");
        buttonGo.transform.position = new Vector3(1.6f, -0.6f, 0f);
        var btnVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        btnVisual.name = "ButtonVisual";
        btnVisual.transform.SetParent(buttonGo.transform, false);
        btnVisual.transform.localScale = new Vector3(0.2f, 0.03f, 0.2f);
        SetColor(btnVisual, new Color(0.18f, 0.45f, 0.85f));
        var button = buttonGo.AddComponent<ButtonInteractable>();
        button.targetRenderer = btnVisual.GetComponent<Renderer>();

        // 按钮标签
        var btnLabel = new GameObject("ButtonLabel");
        btnLabel.transform.position = new Vector3(1.6f, -0.6f, -0.35f);
        var bText = btnLabel.AddComponent<TextMesh>();
        bText.text = "BUTTON";
        bText.anchor = TextAnchor.MiddleCenter;
        bText.alignment = TextAlignment.Center;
        bText.fontSize = 42;
        bText.characterSize = 0.04f;
        bText.color = new Color(0.82f, 0.86f, 1f);

        return (valve, button);
    }

    static void CreateGestureDriver(HandInput hand, ValveInteractable valve, ButtonInteractable button)
    {
        var go = new GameObject("GestureDriver");

        var statusGo = new GameObject("StatusText");
        statusGo.transform.position = new Vector3(-2.8f, 1.5f, -0.05f);
        var statusText = statusGo.AddComponent<TextMesh>();
        statusText.anchor = TextAnchor.UpperLeft;
        statusText.alignment = TextAlignment.Left;
        statusText.fontSize = 40;
        statusText.characterSize = 0.04f;
        statusText.color = new Color(0.76f, 0.88f, 1f);

        var taskGo = new GameObject("TaskText");
        taskGo.transform.position = new Vector3(0f, 2.0f, -0.05f);
        var taskText = taskGo.AddComponent<TextMesh>();
        taskText.anchor = TextAnchor.MiddleCenter;
        taskText.alignment = TextAlignment.Center;
        taskText.fontSize = 52;
        taskText.characterSize = 0.05f;
        taskText.color = Color.white;

        var driver = go.AddComponent<HandGestureDriver>();
        driver.handInput = hand;
        driver.valve = valve;
        driver.button = button;
        driver.statusText = statusText;
        driver.taskText = taskText;
    }

    static void SetColor(GameObject go, Color c)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var m = r.material;
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
    }
}
