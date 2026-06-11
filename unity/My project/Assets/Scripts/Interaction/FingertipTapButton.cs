using System;
using UnityEngine;

/// <summary>
/// 摄像头平面按钮: 食指进入按钮只算悬停,稳定后向下点按才触发一次。
/// </summary>
public class FingertipTapButton : MonoBehaviour
{
    public HandInput hand;
    public GraspController grasp;
    public bool requireFreeHand = true;
    public bool interactable = true;
    public Action Clicked;

    public Color idleColor = new Color(0.16f, 0.36f, 0.76f);
    public Color hoverColor = new Color(0.98f, 0.72f, 0.18f);
    public Color pressedColor = new Color(0.20f, 0.82f, 0.38f);
    public Color disabledColor = new Color(0.24f, 0.28f, 0.34f);

    GameObject _button;
    Renderer _renderer;
    LineRenderer _guideLine;
    Vector3 _size = new Vector3(1.18f, 0.34f, 0.08f);
    Vector3 _lastPoint;
    bool _armed;
    bool _ready;
    bool _pressed;
    bool _hasLastPoint;
    float _hoverStartTime = -99f;
    float _lastPressTime = -99f;

    const float HoverWidthFactor = 1.12f;
    const float HoverHeightFactor = 1.18f;
    const float ClickWidthFactor = 0.90f;
    const float ClickHeightFactor = 0.88f;
    const float TapReadySeconds = 0.11f;
    const float TapMinDownDelta = 0.028f;
    const float TapMinDownSpeed = 0.18f;
    const float TapMaxSideOffsetFactor = 0.62f;
    const float TapStabilizeMaxDrift = 0.060f;
    const float CooldownSeconds = 0.45f;

    public void Build(Vector3 center, Vector3 size, string label, Color color)
    {
        transform.position = center;
        _size = size;
        idleColor = color;

        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.name = label + "_Base";
        baseGo.transform.parent = transform;
        baseGo.transform.localPosition = new Vector3(0f, -size.y * 0.32f, 0.04f);
        baseGo.transform.localScale = new Vector3(size.x * 1.08f, size.y * 0.32f, size.z);
        Destroy(baseGo.GetComponent<Collider>());
        SetColor(baseGo.GetComponent<Renderer>(), Color.Lerp(color, Color.black, 0.35f));

        _button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _button.name = label + "_Button";
        _button.transform.parent = transform;
        _button.transform.localPosition = Vector3.zero;
        _button.transform.localScale = size;
        Destroy(_button.GetComponent<Collider>());
        _renderer = _button.GetComponent<Renderer>();
        SetColor(_renderer, idleColor);

        var labelGo = new GameObject(label + "_Label");
        labelGo.transform.parent = transform;
        labelGo.transform.localPosition = new Vector3(0f, 0f, -0.08f);
        var text = labelGo.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 38;
        text.characterSize = Mathf.Min(0.040f, size.x / Mathf.Max(label.Length, 1) * 0.42f);
        text.color = Color.white;

        var lineGo = new GameObject(label + "_GuideLine");
        lineGo.transform.parent = transform;
        _guideLine = lineGo.AddComponent<LineRenderer>();
        _guideLine.positionCount = 2;
        _guideLine.startWidth = 0.020f;
        _guideLine.endWidth = 0.012f;
        _guideLine.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _guideLine.enabled = false;
    }

    void Update()
    {
        if (_button == null || _renderer == null) return;

        bool click = UpdateTap(out bool near, out float pressAmount);
        if (!interactable)
        {
            SetColor(_renderer, disabledColor);
            _button.transform.localScale = _size;
            UpdateGuide(false);
            return;
        }

        if (click) Clicked?.Invoke();

        Color color = click || pressAmount > 0.01f
            ? pressedColor
            : near ? hoverColor : idleColor;
        SetColor(_renderer, color);

        float pressY = Mathf.Lerp(near ? _size.y * 0.72f : _size.y, _size.y * 0.48f, pressAmount);
        _button.transform.localScale = new Vector3(_size.x, pressY, _size.z);
        UpdateGuide(near);
    }

    bool UpdateTap(out bool near, out float pressAmount)
    {
        near = false;
        pressAmount = 0f;
        if (!interactable || hand == null || !hand.IsActive)
        {
            ResetTap();
            return false;
        }

        Vector3 point = ButtonPoint();
        Vector3 center = transform.position;
        float halfW = _size.x * 0.5f;
        float halfH = _size.y * 0.5f;
        float dx = Mathf.Abs(point.x - center.x);
        float dy = Mathf.Abs(point.y - center.y);
        bool inHover = dx <= halfW * HoverWidthFactor && dy <= halfH * HoverHeightFactor;
        bool inClick = dx <= halfW * ClickWidthFactor && dy <= halfH * ClickHeightFactor;
        near = inHover;

        bool eligible = !requireFreeHand || grasp == null || grasp.Held == null;
        if (!eligible || !inHover)
        {
            ResetTap();
            return false;
        }

        if (!_armed)
        {
            _armed = true;
            _ready = false;
            _pressed = false;
            _hasLastPoint = true;
            _lastPoint = point;
            _hoverStartTime = Time.time;
            pressAmount = 0.16f;
            return false;
        }

        if (!inClick)
        {
            _ready = false;
            _pressed = false;
            _hasLastPoint = true;
            _lastPoint = point;
            _hoverStartTime = Time.time;
            pressAmount = 0.22f;
            return false;
        }

        if (!_hasLastPoint)
        {
            _hasLastPoint = true;
            _lastPoint = point;
            _hoverStartTime = Time.time;
            pressAmount = 0.16f;
            return false;
        }

        if (!_ready)
        {
            if (Time.time - _hoverStartTime < TapReadySeconds)
            {
                Vector3 settleDelta = point - _lastPoint;
                if (Mathf.Abs(settleDelta.x) > TapStabilizeMaxDrift || Mathf.Abs(settleDelta.y) > TapStabilizeMaxDrift)
                {
                    _hoverStartTime = Time.time;
                    _lastPoint = point;
                }
                pressAmount = 0.16f;
                return false;
            }

            _ready = true;
            _lastPoint = point;
            pressAmount = 0.16f;
            return false;
        }

        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        Vector3 frameDelta = point - _lastPoint;
        if (!_pressed && point.y > _lastPoint.y)
            _lastPoint = point;

        float downDistance = _lastPoint.y - point.y;
        float downSpeed = Mathf.Max(0f, -frameDelta.y / dt);
        float sideOffset = Mathf.Abs(point.x - _lastPoint.x);
        pressAmount = Mathf.Clamp01(downDistance / Mathf.Max(TapMinDownDelta, 1e-4f));

        if (_pressed)
        {
            if (downDistance <= TapMinDownDelta * 0.25f)
                _pressed = false;
            else
                return false;
        }

        bool tapDown = (downDistance >= TapMinDownDelta && downSpeed >= TapMinDownSpeed)
            || downDistance >= TapMinDownDelta * 1.6f;
        bool sideStable = sideOffset <= halfW * TapMaxSideOffsetFactor;
        bool canClick = tapDown
            && !_pressed
            && sideStable
            && Time.time - _lastPressTime >= CooldownSeconds;
        _hasLastPoint = true;
        if (!canClick) return false;

        _lastPressTime = Time.time;
        _pressed = true;
        return true;
    }

    Vector3 ButtonPoint()
    {
        if (hand.Points != null && hand.Points.Length > 8)
            return hand.Points[8];
        return hand.GripPoint;
    }

    void UpdateGuide(bool near)
    {
        if (_guideLine == null) return;
        bool show = near && interactable && hand != null && hand.IsActive;
        _guideLine.enabled = show;
        if (!show) return;

        Vector3 point = ButtonPoint() + new Vector3(0f, 0f, -0.14f);
        Vector3 target = transform.position + new Vector3(0f, 0f, -0.14f);
        _guideLine.SetPosition(0, point);
        _guideLine.SetPosition(1, target);
    }

    void ResetTap()
    {
        _armed = false;
        _ready = false;
        _pressed = false;
        _hasLastPoint = false;
        _hoverStartTime = -99f;
    }

    static void SetColor(Renderer renderer, Color color)
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
