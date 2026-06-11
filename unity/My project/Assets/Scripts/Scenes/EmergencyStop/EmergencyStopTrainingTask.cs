using UnityEngine;

/// <summary>
/// 急停按钮训练: 向下按下红色急停按钮后进入锁定状态，再捏合旋转完成复位。
/// </summary>
public class EmergencyStopTrainingTask : MonoBehaviour
{
    public HandInput hand;

    GameObject _buttonCap;
    GameObject _cursor;
    Renderer _buttonRenderer;
    Renderer _cursorRenderer;
    Renderer _alarmLamp;
    Renderer _readyLamp;
    Renderer _resetProgressRenderer;
    LineRenderer _guideLine;
    TextMesh _status;

    Vector3 _lastFingerPoint;
    float _hoverStartTime = -99f;
    float _lastPressTime = -99f;
    float _lastRotateAngle;
    float _resetProgress;
    bool _hasFingerPoint;
    bool _hovering;
    bool _pressReady;
    bool _pressedThisFrame;
    bool _locked;
    bool _rotatingReset;
    int _success;

    readonly Vector3 _buttonCenter = new Vector3(0f, 0.02f, -0.18f);
    readonly Color _red = new Color(0.95f, 0.12f, 0.08f);
    readonly Color _redDark = new Color(0.48f, 0.05f, 0.04f);
    readonly Color _yellow = new Color(1f, 0.78f, 0.16f);
    readonly Color _green = new Color(0.20f, 0.86f, 0.34f);
    readonly Color _lampDim = new Color(0.08f, 0.09f, 0.10f);

    const float PressRadius = 0.62f;
    const float PressReadySeconds = 0.10f;
    const float PressDownDelta = 0.035f;
    const float PressCooldown = 0.65f;
    const float ResetGrabRadius = 0.88f;
    const float ResetGripThreshold = 0.44f;
    const float ResetReleaseThreshold = 0.24f;
    const float ResetDegreesRequired = 95f;

    void Start()
    {
        BuildPanel();
        BuildCursorAndGuide();
        BuildText();
        UpdateVisuals();
        UpdateStatus();
    }

    void Update()
    {
        UpdatePress();
        UpdateResetRotation();
        UpdateCursorAndGuide();
        UpdateVisuals();
        UpdateStatus();
    }

    void BuildPanel()
    {
        var root = new GameObject("EmergencyStopStation");
        root.transform.parent = transform;
        root.transform.position = Vector3.zero;

        CreateBox(root.transform, "EmergencyPanel", new Vector3(0f, 0f, 0.16f), new Vector3(2.75f, 2.55f, 0.16f), new Color(0.18f, 0.20f, 0.23f));
        CreateBox(root.transform, "WarningPlate", new Vector3(0f, -0.98f, -0.04f), new Vector3(1.78f, 0.28f, 0.08f), _yellow);

        var plateTextGo = new GameObject("WarningPlateText");
        plateTextGo.transform.parent = root.transform;
        plateTextGo.transform.localPosition = new Vector3(0f, -0.98f, -0.10f);
        var plateText = plateTextGo.AddComponent<TextMesh>();
        plateText.text = "EMERGENCY STOP";
        plateText.anchor = TextAnchor.MiddleCenter;
        plateText.alignment = TextAlignment.Center;
        plateText.fontSize = 34;
        plateText.characterSize = 0.030f;
        plateText.color = new Color(0.12f, 0.10f, 0.03f);

        var baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseRing.name = "EmergencyStopYellowBase";
        baseRing.transform.parent = root.transform;
        baseRing.transform.localPosition = new Vector3(0f, 0.02f, -0.08f);
        baseRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        baseRing.transform.localScale = new Vector3(0.92f, 0.05f, 0.92f);
        Destroy(baseRing.GetComponent<Collider>());
        SetColor(baseRing, _yellow);

        _buttonCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _buttonCap.name = "EmergencyStopCap";
        _buttonCap.transform.parent = root.transform;
        _buttonCap.transform.localPosition = _buttonCenter;
        _buttonCap.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        _buttonCap.transform.localScale = new Vector3(0.66f, 0.12f, 0.66f);
        Destroy(_buttonCap.GetComponent<Collider>());
        _buttonRenderer = _buttonCap.GetComponent<Renderer>();
        SetRendererColor(_buttonRenderer, _red);

        CreateBox(root.transform, "ResetArrow", new Vector3(0.64f, 0.62f, -0.08f), new Vector3(0.48f, 0.08f, 0.06f), new Color(0.86f, 0.92f, 1f)).transform.rotation = Quaternion.Euler(0f, 0f, 36f);

        _alarmLamp = CreateLamp(root.transform, "AlarmLamp", new Vector3(-0.88f, 0.88f, -0.08f));
        _readyLamp = CreateLamp(root.transform, "ReadyLamp", new Vector3(0.88f, 0.88f, -0.08f));

        CreateBox(root.transform, "ResetRail", new Vector3(0f, -0.66f, -0.08f), new Vector3(1.55f, 0.055f, 0.06f), new Color(0.26f, 0.29f, 0.33f));
        var progress = CreateBox(root.transform, "ResetProgressFill", new Vector3(-0.76f, -0.66f, -0.11f), new Vector3(0.04f, 0.10f, 0.06f), _green);
        _resetProgressRenderer = progress.GetComponent<Renderer>();
    }

    Renderer CreateLamp(Transform parent, string name, Vector3 position)
    {
        var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = name;
        lamp.transform.parent = parent;
        lamp.transform.localPosition = position;
        lamp.transform.localScale = Vector3.one * 0.26f;
        Destroy(lamp.GetComponent<Collider>());
        SetColor(lamp, _lampDim);
        return lamp.GetComponent<Renderer>();
    }

    void BuildCursorAndGuide()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "EmergencyStopCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("EmergencyStopGuideLine");
        _guideLine = lineGo.AddComponent<LineRenderer>();
        _guideLine.positionCount = 2;
        _guideLine.startWidth = 0.026f;
        _guideLine.endWidth = 0.012f;
        _guideLine.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _guideLine.enabled = false;
    }

    void BuildText()
    {
        var titleGo = new GameObject("EmergencyStopTitle");
        titleGo.transform.position = new Vector3(0f, 2.05f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "急停按钮训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 58;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var statusGo = new GameObject("EmergencyStopStatus");
        statusGo.transform.position = new Vector3(3.02f, 1.28f, -0.08f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void UpdatePress()
    {
        _pressedThisFrame = false;
        if (_locked || hand == null || !hand.IsActive)
        {
            _hovering = false;
            _pressReady = false;
            _hasFingerPoint = false;
            return;
        }

        Vector3 finger = FingerPoint();
        float distance = Vector2.Distance(new Vector2(finger.x, finger.y), new Vector2(_buttonCenter.x, _buttonCenter.y));
        _hovering = distance <= PressRadius;

        if (!_hovering)
        {
            _pressReady = false;
            _hasFingerPoint = false;
            _hoverStartTime = -99f;
            return;
        }

        if (!_hasFingerPoint)
        {
            _hasFingerPoint = true;
            _lastFingerPoint = finger;
            _hoverStartTime = Time.time;
            return;
        }

        if (!_pressReady)
        {
            if (Time.time - _hoverStartTime >= PressReadySeconds)
            {
                _pressReady = true;
                _lastFingerPoint = finger;
            }
            return;
        }

        if (finger.y > _lastFingerPoint.y)
            _lastFingerPoint = finger;

        float down = _lastFingerPoint.y - finger.y;
        bool canPress = down >= PressDownDelta && Time.time - _lastPressTime >= PressCooldown;
        if (!canPress) return;

        _locked = true;
        _pressedThisFrame = true;
        _lastPressTime = Time.time;
        _pressReady = false;
        _resetProgress = 0f;
        _rotatingReset = false;
    }

    void UpdateResetRotation()
    {
        if (!_locked || hand == null || !hand.IsActive)
        {
            _rotatingReset = false;
            return;
        }

        Vector3 grip = hand.GripPoint;
        Vector3 offset = grip - _buttonCenter;
        float distance = new Vector2(offset.x, offset.y).magnitude;
        bool near = distance <= ResetGrabRadius;
        bool gripping = hand.PinchOnlyStrength >= ResetGripThreshold;

        if (near && gripping)
        {
            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            if (!_rotatingReset)
            {
                _rotatingReset = true;
                _lastRotateAngle = angle;
            }
            else
            {
                float step = Mathf.Abs(Mathf.DeltaAngle(_lastRotateAngle, angle));
                if (step < 35f)
                    _resetProgress += step;
                _lastRotateAngle = angle;
            }
        }
        else
        {
            _rotatingReset = false;
            if (hand.PinchOnlyStrength <= ResetReleaseThreshold)
                _lastRotateAngle = 0f;
        }

        if (_resetProgress < ResetDegreesRequired) return;

        _locked = false;
        _rotatingReset = false;
        _resetProgress = 0f;
        _success++;
    }

    void UpdateCursorAndGuide()
    {
        if (_cursor == null || _guideLine == null || hand == null) return;

        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            _guideLine.enabled = false;
            return;
        }

        Vector3 point = _locked ? hand.GripPoint : FingerPoint();
        Vector3 cursor = point + new Vector3(0f, 0f, -0.18f);
        _cursor.transform.position = cursor;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.23f, hand.PinchOnlyStrength);

        Color cursorColor = _rotatingReset
            ? _green
            : _hovering || _locked ? _yellow : new Color(0.18f, 0.66f, 1f);
        SetRendererColor(_cursorRenderer, cursorColor);

        bool showLine = _hovering || _locked || _rotatingReset;
        _guideLine.enabled = showLine;
        if (!showLine) return;

        _guideLine.SetPosition(0, cursor);
        _guideLine.SetPosition(1, _buttonCenter + new Vector3(0f, 0f, -0.18f));
    }

    void UpdateVisuals()
    {
        if (_buttonCap == null) return;

        float press = _locked ? 1f : _pressedThisFrame ? 0.75f : _hovering ? 0.18f : 0f;
        _buttonCap.transform.localPosition = Vector3.Lerp(_buttonCenter, _buttonCenter + new Vector3(0f, -0.04f, 0.10f), press);
        _buttonCap.transform.localScale = Vector3.Lerp(new Vector3(0.66f, 0.12f, 0.66f), new Vector3(0.62f, 0.10f, 0.62f), press);

        Color buttonColor = _locked ? _redDark : _hovering ? Color.Lerp(_red, _yellow, 0.30f) : _red;
        SetRendererColor(_buttonRenderer, buttonColor);
        SetRendererColor(_alarmLamp, _locked ? _red : _lampDim);
        SetRendererColor(_readyLamp, _locked ? _lampDim : _green);

        float pct = Mathf.Clamp01(_resetProgress / ResetDegreesRequired);
        if (_resetProgressRenderer != null)
        {
            float width = Mathf.Max(0.035f, 1.52f * pct);
            _resetProgressRenderer.transform.localScale = new Vector3(width, 0.10f, 0.06f);
            _resetProgressRenderer.transform.localPosition = new Vector3(-0.76f + width * 0.5f, -0.66f, -0.11f);
            SetRendererColor(_resetProgressRenderer, _locked ? _green : _lampDim);
        }
    }

    void UpdateStatus()
    {
        if (_status == null || hand == null) return;

        string phase;
        if (!hand.IsActive) phase = "等待手势识别";
        else if (_rotatingReset) phase = "捏合旋转复位";
        else if (_locked) phase = "急停已锁定, 捏合旋转复位";
        else if (_hovering) phase = "向下轻按急停按钮";
        else phase = "移动食指到红色按钮";

        _status.text =
            "状态: " + (_locked ? "急停锁定" : "设备就绪") +
            "\n操作: " + phase +
            "\n复位进度: " + Mathf.RoundToInt(Mathf.Clamp01(_resetProgress / ResetDegreesRequired) * 100f) + "%" +
            "\n完成次数: " + _success +
            "\n捏合: " + hand.PinchOnlyStrength.ToString("0.00");
    }

    Vector3 FingerPoint()
    {
        if (hand != null && hand.Points != null && hand.Points.Length > 8)
            return hand.Points[8];
        return hand != null ? hand.GripPoint : Vector3.zero;
    }

    GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.parent = parent;
        box.transform.localPosition = position;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = scale;
        Destroy(box.GetComponent<Collider>());
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
