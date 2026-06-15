using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class RotaryValveTask : MonoBehaviour
{
    public HandInput hand;
    public RotaryValveInteractable valve;

    TextMesh _status;
    GameObject _cursor;
    Renderer _cursorRenderer;
    LineRenderer _line;
    Renderer _targetLamp;
    Renderer _flowLamp;
    float _completedAt = -999f;
    bool _wasAtTarget;
    bool _returnScheduled;
    string _instruction = "握拳后拧动阀门，将红色手轮旋转到目标角度。";
    string _successMessage = "成功完成训练";

    readonly Color _pipeYellow = new Color(0.94f, 0.62f, 0.05f);
    readonly Color _wheelRed = new Color(0.84f, 0.08f, 0.05f);
    readonly Color _metalDark = new Color(0.25f, 0.27f, 0.30f);
    readonly Color _metalLight = new Color(0.58f, 0.60f, 0.62f);
    readonly Color _lampDim = new Color(0.08f, 0.09f, 0.10f);
    readonly Color _lampGreen = new Color(0.20f, 0.85f, 0.32f);

    void Start()
    {
        LoadSessionPrompt();
        BuildValveScene();
        BuildCursor();
        BuildText();
        _wasAtTarget = valve != null && valve.IsAtTarget;
        UpdateLamps();
        UpdateStatus();
    }

    void Update()
    {
        UpdateCursor();
        UpdateLamps();
        UpdateStatus();

        if (valve != null && valve.IsAtTarget && !_wasAtTarget)
        {
            _completedAt = Time.time;
            ScheduleReturnAfterSuccess();
        }
        if (valve != null) _wasAtTarget = valve.IsAtTarget;
    }

    void BuildValveScene()
    {
        var root = new GameObject("RotaryValveScene");
        root.transform.parent = transform;
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        CreatePipe(root.transform, "MainYellowPipe", new Vector3(0f, -0.12f, 0.24f), 5.4f, 0.38f);
        CreatePipe(root.transform, "VerticalYellowPipe", new Vector3(-1.85f, 0.55f, 0.34f), 2.5f, 0.30f, true);
        CreateFlange(root.transform, "LeftFlange", new Vector3(-1.05f, -0.12f, -0.05f), 0.60f);
        CreateFlange(root.transform, "RightFlange", new Vector3(1.05f, -0.12f, -0.05f), 0.60f);
        CreateBox(root.transform, "ValveBody", new Vector3(0f, -0.12f, -0.02f), new Vector3(1.25f, 0.92f, 0.34f), _metalDark);
        CreateBox(root.transform, "ValveNeck", new Vector3(0f, 0.66f, -0.06f), new Vector3(0.38f, 0.62f, 0.28f), _metalDark);
        CreateFlange(root.transform, "TopBonnet", new Vector3(0f, 1.05f, -0.08f), 0.46f, 8);

        var wheelRoot = new GameObject("RedValveWheel");
        wheelRoot.transform.parent = root.transform;
        wheelRoot.transform.localPosition = new Vector3(0f, 0.16f, -0.42f);
        wheelRoot.transform.localRotation = Quaternion.identity;

        var renderers = BuildHandWheel(wheelRoot.transform);
        valve = root.AddComponent<RotaryValveInteractable>();
        valve.hand = hand;
        valve.wheelRoot = wheelRoot.transform;
        valve.highlightRenderers = renderers;
        valve.targetAngle = 180f;
        valve.targetTolerance = 15f;
        valve.grabRadius = 1.22f;

        _targetLamp = CreateLamp(root.transform, "TargetLamp", new Vector3(-1.42f, 1.30f, -0.20f));
        _flowLamp = CreateLamp(root.transform, "FlowLamp", new Vector3(1.42f, 1.30f, -0.20f));
    }

    Renderer[] BuildHandWheel(Transform parent)
    {
        var renderers = new List<Renderer>();

        int ringSegments = 18;
        float ringRadius = 0.86f;
        float segmentLength = 2f * Mathf.PI * ringRadius / ringSegments * 0.94f;
        for (int i = 0; i < ringSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / ringSegments;
            var segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segment.name = "OuterRedWheelRing_" + i;
            segment.transform.parent = parent;
            segment.transform.localPosition = new Vector3(Mathf.Cos(angle) * ringRadius, Mathf.Sin(angle) * ringRadius, 0f);
            segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            segment.transform.localScale = new Vector3(0.07f, segmentLength * 0.5f, 0.07f);
            SetColor(segment, _wheelRed);
            renderers.Add(segment.GetComponent<Renderer>());
        }

        var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hub.name = "WheelHub";
        hub.transform.parent = parent;
        hub.transform.localPosition = new Vector3(0f, 0f, -0.03f);
        hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        hub.transform.localScale = new Vector3(0.25f, 0.12f, 0.25f);
        SetColor(hub, _wheelRed);
        renderers.Add(hub.GetComponent<Renderer>());

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f + 22.5f;
            var spoke = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spoke.name = "WheelSpoke_" + i;
            spoke.transform.parent = parent;
            spoke.transform.localPosition = Vector3.zero;
            spoke.transform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
            spoke.transform.localScale = new Vector3(0.07f, 0.45f, 0.07f);
            SetColor(spoke, _wheelRed);
            renderers.Add(spoke.GetComponent<Renderer>());
        }

        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "ValveShaft";
        shaft.transform.parent = parent;
        shaft.transform.localPosition = new Vector3(0f, 0f, 0.22f);
        shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        shaft.transform.localScale = new Vector3(0.15f, 0.28f, 0.15f);
        SetColor(shaft, _metalLight);
        renderers.Add(shaft.GetComponent<Renderer>());

        var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knob.name = "GripKnob";
        knob.transform.parent = parent;
        knob.transform.localPosition = new Vector3(0.78f, 0.34f, -0.05f);
        knob.transform.localRotation = Quaternion.identity;
        knob.transform.localScale = Vector3.one * 0.18f;
        SetColor(knob, _wheelRed);
        renderers.Add(knob.GetComponent<Renderer>());

        return renderers.ToArray();
    }

    void CreatePipe(Transform parent, string name, Vector3 position, float length, float radius, bool vertical = false)
    {
        var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pipe.name = name;
        pipe.transform.parent = parent;
        pipe.transform.position = position;
        pipe.transform.localRotation = vertical ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f);
        pipe.transform.localScale = new Vector3(radius, length * 0.5f, radius);
        SetColor(pipe, _pipeYellow);
    }

    void CreateFlange(Transform parent, string name, Vector3 position, float radius, int boltCount = 10)
    {
        var flange = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flange.name = name;
        flange.transform.parent = parent;
        flange.transform.position = position;
        flange.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        flange.transform.localScale = new Vector3(radius, 0.12f, radius);
        SetColor(flange, _metalLight);

        for (int i = 0; i < boltCount; i++)
        {
            float a = i * Mathf.PI * 2f / boltCount;
            Vector3 offset = new Vector3(Mathf.Cos(a) * radius * 0.78f, Mathf.Sin(a) * radius * 0.78f, -0.10f);
            var bolt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bolt.name = name + "_Bolt_" + i;
            bolt.transform.parent = parent;
            bolt.transform.position = position + offset;
            bolt.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            bolt.transform.localScale = new Vector3(0.065f, 0.06f, 0.065f);
            SetColor(bolt, new Color(0.12f, 0.13f, 0.14f));
        }
    }

    Renderer CreateLamp(Transform parent, string name, Vector3 position)
    {
        var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = name;
        lamp.transform.parent = parent;
        lamp.transform.position = position;
        lamp.transform.localRotation = Quaternion.identity;
        lamp.transform.localScale = Vector3.one * 0.22f;
        Destroy(lamp.GetComponent<Collider>());
        SetColor(lamp, _lampDim);
        return lamp.GetComponent<Renderer>();
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "ValveGripCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("ValveGripLine");
        _line = lineGo.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.025f;
        _line.endWidth = 0.012f;
        _line.material = MakeMaterial(new Color(0.82f, 0.94f, 1f));
        _line.enabled = false;
    }

    void BuildText()
    {
        var titleGo = new GameObject("RotaryValveTitle");
        titleGo.transform.position = new Vector3(0f, 2.05f, -0.24f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "旋转阀门训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 58;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var statusGo = new GameObject("RotaryValveStatus");
        statusGo.transform.position = new Vector3(-2.90f, 1.50f, -0.24f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperLeft;
        _status.alignment = TextAlignment.Left;
        _status.fontSize = 40;
        _status.characterSize = 0.041f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void UpdateCursor()
    {
        if (_cursor == null || hand == null || valve == null) return;

        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            _line.enabled = false;
            return;
        }

        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.26f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.22f, valve.GripSignal);

        Color color = valve.IsGrabbed
            ? new Color(0.20f, 0.90f, 0.38f)
            : valve.IsHovering ? new Color(1f, 0.78f, 0.18f) : new Color(0.18f, 0.66f, 1f);
        SetRendererColor(_cursorRenderer, color);

        _line.enabled = valve.IsHovering || valve.IsGrabbed;
        if (_line.enabled)
        {
            _line.SetPosition(0, grip);
            _line.SetPosition(1, valve.CenterPosition + new Vector3(0f, 0f, -0.26f));
        }
    }

    void UpdateLamps()
    {
        if (valve == null) return;
        SetRendererColor(_targetLamp, valve.IsAtTarget ? _lampGreen : _lampDim);
        SetRendererColor(_flowLamp, Mathf.Abs(valve.CurrentAngle) > 5f ? _lampGreen : _lampDim);
    }

    void UpdateStatus()
    {
        if (_status == null || hand == null || valve == null) return;

        string phase;
        if (!hand.IsActive) phase = "等待手势识别服务连接";
        else if (valve.IsGrabbed) phase = "已握住手轮：持续旋转手腕";
        else if (valve.IsHovering) phase = "握拳抓住红色手轮";
        else phase = "移动手部光标到红色手轮";

        string done = Time.time - _completedAt < 1.8f ? "\n" + _successMessage : "";
        _status.text =
            "目标：" + _instruction +
            "\n动作：" + phase +
            "\n握拳强度：" + valve.GripSignal.ToString("0.00") + " / " + valve.grabThreshold.ToString("0.00") +
            "\n阀门角度：" + valve.CurrentAngle.ToString("0") + "°" +
            "\n目标角度：" + valve.targetAngle.ToString("0") + "° ± " + valve.targetTolerance.ToString("0") + "°" +
            done;
    }

    void LoadSessionPrompt()
    {
        var session = SessionManager.Instance;
        if (session == null) return;

        if (!string.IsNullOrEmpty(session.selectedInstruction))
            _instruction = session.selectedInstruction;
        if (!string.IsNullOrEmpty(session.selectedSuccessMessage))
            _successMessage = session.selectedSuccessMessage;
    }

    void ScheduleReturnAfterSuccess()
    {
        if (_returnScheduled) return;

        var session = SessionManager.Instance;
        if (session == null || !session.hasHubReturnPosition) return;

        _returnScheduled = true;
        StartCoroutine(ReturnAfterSuccessRoutine());
    }

    IEnumerator ReturnAfterSuccessRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        SceneFlow.EnsureExists().ReturnToHub();
    }

    GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.parent = parent;
        box.transform.position = position;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = scale;
        SetColor(box, color);
        return box;
    }

    static void SetColor(GameObject go, Color color)
    {
        SetRendererColor(go.GetComponent<Renderer>(), color);
    }

    static void SetRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        return material;
    }
}
