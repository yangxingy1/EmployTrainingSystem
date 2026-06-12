using UnityEngine;

/// <summary>
/// 可抓取标记:挂在物块上。缓存刚体、抓取半径(由碰撞体包围盒推出),并提供悬停/抓取的高亮反馈。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    public float margin = 0.45f;

    public Rigidbody Body { get; private set; }
    public float GrabRadius { get; private set; }
    public bool CanGrab { get; set; } = true;

    Renderer _r;
    Color _base;
    bool _highlight, _grabbed;

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
        _r = GetComponentInChildren<Renderer>();
        RefreshGrabRadius();
        if (_r) _base = _r.material.color;
    }

    public void RefreshGrabRadius()
    {
        var col = GetComponent<Collider>();
        float ext = col ? col.bounds.extents.magnitude : 0.5f;
        GrabRadius = ext + margin;
    }

    public void SetBaseColor(Color c) { _base = c; Tint(); }
    public void SetHighlight(bool on) { if (_highlight == on) return; _highlight = on; Tint(); }
    public void OnGrabbed() { _grabbed = true; Tint(); }
    public void OnReleased() { _grabbed = false; Tint(); }

    void Tint()
    {
        if (!_r) return;
        Color c = _base;
        if (_grabbed) c = Color.Lerp(_base, Color.white, 0.45f);
        else if (_highlight) c = Color.Lerp(_base, Color.yellow, 0.45f);
        var m = _r.material;
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
    }
}
