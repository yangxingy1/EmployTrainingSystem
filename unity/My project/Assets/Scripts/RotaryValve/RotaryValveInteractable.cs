using UnityEngine;

public class RotaryValveInteractable : MonoBehaviour
{
    public HandInput hand;
    public Transform wheelRoot;
    public Transform gripReference;
    public Renderer[] highlightRenderers;

    [Header("Rotation")]
    public float rotateGain = 1.15f;
    public float minAngle = -360f;
    public float maxAngle = 360f;
    public float targetAngle = 180f;
    public float targetTolerance = 12f;

    [Header("Grab")]
    public float grabRadius = 0.78f;
    public float grabThreshold = 0.45f;
    public float releaseThreshold = 0.24f;

    [Header("Feedback")]
    public Color idleColor = new Color(0.84f, 0.08f, 0.05f);
    public Color hoverColor = new Color(1f, 0.62f, 0.16f);
    public Color grabbedColor = new Color(0.20f, 0.90f, 0.40f);

    float _angle;
    float _previousHandAngle;
    bool _hasPreviousHandAngle;
    bool _isGrabbed;
    bool _isHovering;

    public bool IsGrabbed => _isGrabbed;
    public bool IsHovering => _isHovering;
    public float CurrentAngle => _angle;
    public float GripSignal => hand != null ? hand.PinchOnlyStrength : 0f;
    public Vector3 CenterPosition => wheelRoot != null ? wheelRoot.position : transform.position;
    public bool IsAtTarget => Mathf.Abs(_angle - targetAngle) <= targetTolerance;

    void Awake()
    {
        if (wheelRoot == null) wheelRoot = transform;
        if (gripReference == null) gripReference = wheelRoot;
        ApplyRotation();
        UpdateTint();
    }

    void Update()
    {
        if (hand == null || !hand.IsActive)
        {
            _isHovering = false;
            _isGrabbed = false;
            _hasPreviousHandAngle = false;
            UpdateTint();
            return;
        }

        Vector3 grip = hand.GripPoint;
        float gripSignal = hand.PinchOnlyStrength;
        _isHovering = Vector3.Distance(grip, CenterPosition) <= grabRadius;

        if (!_isGrabbed && _isHovering && gripSignal >= grabThreshold)
            BeginGrab(grip);

        if (_isGrabbed)
        {
            if (gripSignal <= releaseThreshold)
            {
                _isGrabbed = false;
                _hasPreviousHandAngle = false;
            }
            else
            {
                float handAngle = HandAngle(grip);
                if (_hasPreviousHandAngle)
                {
                    float delta = WrapDegrees(handAngle - _previousHandAngle);
                    _angle = Mathf.Clamp(_angle + delta * rotateGain, minAngle, maxAngle);
                    ApplyRotation();
                }
                _previousHandAngle = handAngle;
                _hasPreviousHandAngle = true;
            }
        }

        UpdateTint();
    }

    public void ResetValve()
    {
        _angle = 0f;
        _isGrabbed = false;
        _hasPreviousHandAngle = false;
        ApplyRotation();
        UpdateTint();
    }

    void BeginGrab(Vector3 grip)
    {
        _isGrabbed = true;
        _previousHandAngle = HandAngle(grip);
        _hasPreviousHandAngle = true;
    }

    float HandAngle(Vector3 grip)
    {
        Vector3 local = grip - CenterPosition;
        return Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
    }

    void ApplyRotation()
    {
        if (wheelRoot != null)
            wheelRoot.localRotation = Quaternion.Euler(0f, 0f, _angle);
    }

    void UpdateTint()
    {
        if (highlightRenderers == null) return;

        Color color = _isGrabbed ? grabbedColor : _isHovering ? hoverColor : idleColor;
        foreach (var renderer in highlightRenderers)
        {
            if (renderer == null) continue;
            var mat = renderer.material;
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        }
    }

    static float WrapDegrees(float delta)
    {
        while (delta > 180f) delta -= 360f;
        while (delta < -180f) delta += 360f;
        return delta;
    }
}
