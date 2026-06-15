using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 交互层:手势驱动的抓取状态机 + 吸附辅助。
///   悬停: 捏合点落在 Grabbable 抓取半径内 -> 高亮
///   抓取: 悬停 + 捏合强度 > grabThreshold -> 接管(放宽阈值/半径,容易抓)
///   搬运: 速度跟随;若附近有 SnapZone,目标点被磁吸向槽位中心(越近越强)
///   释放: 捏合强度 < releaseThreshold -> 放手;在 SnapZone 内则自动落位、摆正,否则按手速抛掷
/// </summary>
public class GraspController : MonoBehaviour
{
    [Header("引用")]
    public HandInput hand;
    public HandVisual handVisual;

    [Header("抓取(捏合强度 0~1)")]
    public float grabThreshold = 0.6f;
    public float releaseThreshold = 0.4f;
    public float releaseGraceSeconds = 0.08f;
    public float grabConfirmSeconds = 0.06f;
    public bool allowFistGrip = false;

    [Header("搬运 / 抛掷")]
    public float followStrength = 0.85f;
    public float maxFollowSpeed = 14f;
    public float throwScale = 1.0f;
    public float maxThrowSpeed = 12f;
    public bool useGravityOnRelease = true;
    public bool lockZToPlane = false;
    public float planeZ = 0f;

    [Header("吸附辅助")]
    [Range(0f, 1f)] public float carryMagnetism = 1.0f;  // 总开关:0 关闭磁吸, 1 全强度

    public List<Grabbable> grabbables = new List<Grabbable>();
    public List<SnapZone> snapZones = new List<SnapZone>();
    public Grabbable Held => _held;
    public Grabbable Hover => _hover;
    public Vector3 GripVelocity => _gripVel;
    public float GripSignal => _gripSignal;
    public bool IsGripDown => _gripSignal >= grabThreshold;

    Grabbable _held, _hover, _pendingGrab;
    Collider _heldCol;
    Vector3 _lastGrip, _gripVel, _grabOffset;
    bool _hasLastGrip;
    float _releaseTimer, _grabTimer, _gripSignal;

    void FixedUpdate()
    {
        if (hand == null) return;

        if (!hand.IsActive)
        {
            _gripSignal = 0f;
            if (_held != null) Release();
            SetHover(null);
            _hasLastGrip = false;
            _grabTimer = 0f;
            _pendingGrab = null;
            return;
        }

        Vector3 grip = hand.GripPoint;
        if (lockZToPlane) grip.z = planeZ;
        float dt = Mathf.Max(Time.fixedDeltaTime, 1e-4f);
        if (_hasLastGrip) _gripVel = Vector3.Lerp(_gripVel, (grip - _lastGrip) / dt, 0.5f);
        _lastGrip = grip; _hasLastGrip = true;

        _gripSignal = CurrentGripSignal();
        bool gripping = _gripSignal >= grabThreshold;

        if (_held != null)
        {
            // 搬运目标点:默认跟随捏合点;靠近吸附区时磁吸向槽位中心
            Vector3 dest = grip + _grabOffset;
            if (lockZToPlane) dest.z = planeZ;
            SnapZone z = NearestZone(_held.Body.position);
            if (z != null)
            {
                float d = Vector3.Distance(_held.Body.position, z.Center);
                if (d < z.radius)
                {
                    float pull = Mathf.Clamp01(1f - d / z.radius) * z.magnetism * carryMagnetism;
                    dest = Vector3.Lerp(grip, z.Center, pull);
                }
            }

            Vector3 v = (dest - _held.Body.position) / dt * followStrength;
            if (v.magnitude > maxFollowSpeed) v = v.normalized * maxFollowSpeed;
            _held.Body.velocity = v;
            _held.Body.angularVelocity = Vector3.Lerp(_held.Body.angularVelocity, Vector3.zero, 0.3f);

            if (_gripSignal < releaseThreshold) _releaseTimer += Time.fixedDeltaTime;
            else _releaseTimer = 0f;

            if (_releaseTimer >= releaseGraceSeconds) Release();
        }
        else
        {
            Grabbable cand = FindCandidate(grip);
            SetHover(cand);
            if (_pendingGrab != cand)
            {
                _pendingGrab = cand;
                _grabTimer = 0f;
            }

            if (cand != null && gripping)
            {
                _grabTimer += Time.fixedDeltaTime;
                if (_grabTimer >= grabConfirmSeconds) Grab(cand);
            }
            else
            {
                _grabTimer = 0f;
            }
        }
    }

    float CurrentGripSignal()
    {
        if (hand == null) return 0f;
        return allowFistGrip
            ? Mathf.Max(hand.PinchOnlyStrength, hand.FistStrength)
            : hand.PinchOnlyStrength;
    }

    Grabbable FindCandidate(Vector3 grip)
    {
        Grabbable best = null; float bestD = float.MaxValue;
        foreach (var g in grabbables)
        {
            if (g == null || !g.CanGrab) continue;
            float d = Vector3.Distance(grip, g.Body.position);
            if (d < g.GrabRadius && d < bestD) { bestD = d; best = g; }
        }
        return best;
    }

    SnapZone NearestZone(Vector3 p)
    {
        SnapZone best = null; float bestD = float.MaxValue;
        foreach (var z in snapZones)
        {
            if (z == null || !z.active) continue;
            float d = Vector3.Distance(p, z.Center);
            if (d < bestD) { bestD = d; best = z; }
        }
        return best;
    }

    void Grab(Grabbable g)
    {
        _held = g; _hover = null;
        _heldCol = g.GetComponent<Collider>();
        _grabOffset = g.Body.position - hand.GripPoint;
        if (lockZToPlane) _grabOffset.z = 0f;
        _releaseTimer = 0f;
        _grabTimer = 0f;
        _pendingGrab = null;
        g.OnGrabbed();
        if (_heldCol != null && handVisual != null)
            foreach (var c in handVisual.Colliders) if (c) Physics.IgnoreCollision(_heldCol, c, true);
        g.Body.useGravity = false;
        g.Body.velocity = Vector3.zero;
        g.Body.angularVelocity = Vector3.zero;
    }

    void Release()
    {
        if (_held == null) return;
        var g = _held; _held = null;
        _releaseTimer = 0f;
        _grabTimer = 0f;
        _pendingGrab = null;

        if (_heldCol != null && handVisual != null)
            foreach (var c in handVisual.Colliders) if (c) Physics.IgnoreCollision(_heldCol, c, false);
        _heldCol = null;

        g.Body.useGravity = useGravityOnRelease;
        if (lockZToPlane)
        {
            Vector3 p = g.Body.position;
            p.z = planeZ;
            g.Body.position = p;
        }

        // 在吸附区内 -> 自动落位、摆正;否则按手速抛掷
        SnapZone z = NearestZone(g.Body.position);
        if (z != null && z.active && Vector3.Distance(g.Body.position, z.Center) < z.radius)
        {
            g.Body.velocity = Vector3.zero;
            g.Body.angularVelocity = Vector3.zero;
            g.transform.position = z.SnapPosition;
            g.transform.rotation = z.SnapRotation;
        }
        else
        {
            Vector3 v = _gripVel * throwScale;
            if (v.magnitude > maxThrowSpeed) v = v.normalized * maxThrowSpeed;
            if (lockZToPlane) v.z = 0f;
            g.Body.velocity = v;
        }

        g.OnReleased();
    }

    void SetHover(Grabbable g)
    {
        if (_hover == g) return;
        if (_hover) _hover.SetHighlight(false);
        _hover = g;
        if (_hover) _hover.SetHighlight(true);
    }
}
