using System;
using UnityEngine;

/// <summary>
/// 推拉开关交互件: 捏合横向把手后沿本地 Y 轴上下拖动，松手后吸附到 ON/OFF 档位。
/// </summary>
public class ElectricSwitchInteractable : MonoBehaviour
{
    public HandInput hand;
    public Transform sliderRoot;
    public Transform handle;
    public Renderer[] highlightRenderers;

    [Header("Travel")]
    public float downY = -0.82f;
    public float upY = 0.82f;
    public float snapSpeed = 4.5f;
    public float dragSpeed = 8f;

    [Header("Grab")]
    public float grabRadius = 0.58f;
    public float grabThreshold = 0.48f;
    public float releaseThreshold = 0.25f;

    [Header("Feedback")]
    public Color idleColor = new Color(0.90f, 0.18f, 0.12f);
    public Color hoverColor = new Color(1f, 0.72f, 0.16f);
    public Color grabbedColor = new Color(0.20f, 0.90f, 0.40f);

    public event Action<bool> OnSwitchChanged;

    float _currentY;
    float _snapTargetY;
    float _grabOffsetY;
    bool _isGrabbed;
    bool _isHovering;
    bool _isUp;

    public bool IsGrabbed => _isGrabbed;
    public bool IsHovering => _isHovering;
    public bool IsUp => _isUp;
    public float CurrentY => _currentY;
    public float CurrentTravel => Mathf.InverseLerp(downY, upY, _currentY);
    public Vector3 HandlePosition => handle != null ? handle.position : transform.position;
    public float GripSignal => hand != null ? hand.PinchOnlyStrength : 0f;

    void Awake()
    {
        if (sliderRoot == null)
        {
            _currentY = downY;
            _snapTargetY = downY;
            return;
        }

        if (handle == null) handle = sliderRoot;

        _currentY = downY;
        _snapTargetY = downY;
        ApplyPosition(_currentY);
        UpdateState(true);
        UpdateTint();
    }

    void Update()
    {
        if (hand == null || !hand.IsActive)
        {
            ReleaseToNearest();
            _isHovering = false;
            SnapIfNeeded();
            UpdateTint();
            return;
        }

        Vector3 grip = hand.GripPoint;
        float gripSignal = hand.PinchOnlyStrength;
        _isHovering = Vector3.Distance(grip, HandlePosition) <= grabRadius;

        if (!_isGrabbed && _isHovering && gripSignal >= grabThreshold)
            BeginGrab(grip);

        if (_isGrabbed)
        {
            if (gripSignal <= releaseThreshold)
            {
                ReleaseToNearest();
            }
            else
            {
                float targetY = Mathf.Clamp(LocalGripY(grip) + _grabOffsetY, downY, upY);
                _currentY = Mathf.MoveTowards(_currentY, targetY, dragSpeed * Time.deltaTime);
                ApplyPosition(_currentY);
                UpdateState(false);
            }
        }
        else
        {
            SnapIfNeeded();
        }

        UpdateTint();
    }

    public void SetUp(bool up)
    {
        _isGrabbed = false;
        _snapTargetY = up ? upY : downY;
        _currentY = _snapTargetY;
        ApplyPosition(_currentY);
        UpdateState(true);
        UpdateTint();
    }

    void BeginGrab(Vector3 grip)
    {
        _isGrabbed = true;
        _grabOffsetY = _currentY - LocalGripY(grip);
    }

    void ReleaseToNearest()
    {
        if (!_isGrabbed) return;
        _isGrabbed = false;
        _snapTargetY = _currentY >= MidY ? upY : downY;
    }

    void SnapIfNeeded()
    {
        if (Mathf.Approximately(_currentY, _snapTargetY)) return;

        _currentY = Mathf.MoveTowards(_currentY, _snapTargetY, snapSpeed * Time.deltaTime);
        ApplyPosition(_currentY);
        UpdateState(false);
    }

    float LocalGripY(Vector3 grip)
    {
        Transform parent = sliderRoot != null ? sliderRoot.parent : transform.parent;
        return parent != null ? parent.InverseTransformPoint(grip).y : grip.y;
    }

    void ApplyPosition(float y)
    {
        if (sliderRoot == null) return;
        Vector3 local = sliderRoot.localPosition;
        local.y = y;
        sliderRoot.localPosition = local;
        sliderRoot.localRotation = Quaternion.identity;
    }

    void UpdateState(bool silent)
    {
        bool nowUp = _currentY >= MidY;
        if (_isUp == nowUp && !silent) return;

        _isUp = nowUp;
        if (!silent) OnSwitchChanged?.Invoke(_isUp);
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

    float MidY => (upY + downY) * 0.5f;
}
