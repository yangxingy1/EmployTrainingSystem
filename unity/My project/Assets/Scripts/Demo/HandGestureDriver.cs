using UnityEngine;

public class HandGestureDriver : MonoBehaviour
{
    public HandInput handInput;
    public ValveInteractable valve;
    public ButtonInteractable button;
    public TextMesh statusText;
    public TextMesh taskText;

    public float rotateGain = 1.6f;
    public float grabThreshold = 0.28f;
    public float clickThreshold = 0.48f;
    public float clickCooldown = 0.45f;
    public float valveHoverRadius = 1.25f;
    public float buttonHoverRadius = 1.05f;
    public float targetValveAngle = 45f;
    public float angleTolerance = 12f;
    public float cursorMinRadius = 0.09f;
    public float cursorMaxRadius = 0.18f;

    float _previousPalmAngle;
    bool _hasPreviousAngle;
    bool _wasClickDown;
    float _lastClickTime = -999f;
    string _gesture = "waiting";
    string _state = "";
    bool _clicked;
    GameObject _cursor;
    Renderer _cursorRenderer;
    LineRenderer _line;

    void Start()
    {
        BuildCursor();
    }

    void Update()
    {
        if (handInput == null || !handInput.IsActive)
        {
            _hasPreviousAngle = false;
            _wasClickDown = false;
            _gesture = "waiting";
            _state = "";
            if (valve != null) valve.SetHighlighted(false);
            SetCursor(false, Vector3.zero, 0f, Color.gray);
            SetLine(false, Vector3.zero, Vector3.zero);
            UpdateDebug(false, 0f, 0f, false, false);
            return;
        }

        var points = handInput.Points;
        float grip = handInput.PinchStrength;
        float palmAngle = handInput.PalmAngle;
        float delta = 0f;
        if (_hasPreviousAngle)
            delta = WrapDegrees(palmAngle - _previousPalmAngle);
        _previousPalmAngle = palmAngle;
        _hasPreviousAngle = true;

        Vector3 gripPoint = handInput.GripPoint;
        bool nearValve = valve != null && Vector3.Distance(gripPoint, valve.transform.position) <= valveHoverRadius;
        bool nearButton = button != null && Vector3.Distance(gripPoint, button.transform.position) <= buttonHoverRadius;
        if (valve != null) valve.SetHighlighted(nearValve);

        Vector3 target = Vector3.zero;
        bool hasTarget = false;
        if (nearValve && valve != null)
        {
            target = valve.transform.position;
            hasTarget = true;
        }
        else if (nearButton && button != null)
        {
            target = button.transform.position;
            hasTarget = true;
        }

        Color cursorColor = handInput.IsGripping
            ? new Color(0.15f, 0.85f, 0.35f)
            : hasTarget ? new Color(1f, 0.8f, 0.2f) : new Color(0.2f, 0.7f, 1f);
        SetCursor(true, gripPoint, grip, cursorColor);
        SetLine(hasTarget, gripPoint, target);

        bool rotated = Mathf.Abs(delta) >= 0.4f;
        if (rotated && valve != null && nearValve)
        {
            valve.ApplyRotation(delta * rotateGain);
            _gesture = "rotate";
            _state = "update";
        }
        else if (handInput.IsGripping || grip >= grabThreshold)
        {
            _gesture = nearButton ? "press-ready" : "grab";
            _state = "hold";
        }
        else
        {
            _gesture = nearValve || nearButton ? "hover" : "tracking";
            _state = "";
        }

        bool clickDown = handInput.IsGripping || grip >= clickThreshold;
        if (clickDown && !_wasClickDown && nearButton && Time.time - _lastClickTime >= clickCooldown)
        {
            _lastClickTime = Time.time;
            _clicked = true;
            _gesture = "click";
            _state = "trigger";
            button.TriggerClick();
        }
        _wasClickDown = clickDown;

        if (taskText != null && valve != null)
        {
            bool angleOk = Mathf.Abs(valve.CurrentAngle - targetValveAngle) <= angleTolerance;
            if (angleOk && _clicked)
                taskText.text = "Task complete. Score: 100";
            else if (_clicked)
                taskText.text = "Button confirmed. Rotate valve to target.";
            else
                taskText.text = "Task: rotate valve, then click button.";
        }

        UpdateDebug(true, grip, valve != null ? valve.CurrentAngle : 0f, nearValve, nearButton);
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "GestureCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("GestureTargetLine");
        _line = lineGo.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.025f;
        _line.endWidth = 0.01f;
        _line.material = MakeMaterial(new Color(0.8f, 0.95f, 1f));
        _line.enabled = false;
    }

    void SetCursor(bool visible, Vector3 position, float grip, Color color)
    {
        if (_cursor == null) return;
        _cursor.SetActive(visible);
        if (!visible) return;
        _cursor.transform.position = position + new Vector3(0f, 0f, -0.08f);
        float radius = Mathf.Lerp(cursorMinRadius, cursorMaxRadius, Mathf.Clamp01(grip));
        _cursor.transform.localScale = Vector3.one * radius;
        if (_cursorRenderer != null)
        {
            var mat = _cursorRenderer.material;
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        }
    }

    void SetLine(bool visible, Vector3 from, Vector3 to)
    {
        if (_line == null) return;
        _line.enabled = visible;
        if (!visible) return;
        _line.SetPosition(0, from + new Vector3(0f, 0f, -0.12f));
        _line.SetPosition(1, to + new Vector3(0f, 0f, -0.12f));
    }

    void UpdateDebug(bool tracked, float grip, float valveAngle, bool nearValve, bool nearButton)
    {
        if (statusText == null) return;
        statusText.text =
            "Hand: " + (tracked ? "tracked" : "not tracked") +
            "\nGesture: " + _gesture + " " + _state +
            "\nGrip: " + grip.ToString("0.00") +
            "\nValve: " + valveAngle.ToString("0.0") +
            "\nNear: " + (nearValve ? "valve" : nearButton ? "button" : "-") +
            "\nTip: move cursor to target";
    }

    static float WrapDegrees(float delta)
    {
        while (delta > 180f) delta -= 360f;
        while (delta < -180f) delta += 360f;
        return delta;
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
