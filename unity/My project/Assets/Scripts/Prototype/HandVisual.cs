using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表现层:从 HandInput 读取关节点,构建并驱动手的视觉+物理。
///   - 渐变手指(腕粗->指尖细)的关节球与骨头胶囊(带碰撞体,可推动物体)
///   - 填充的手掌网格(看起来是手掌而非方块) + 一个隐形手掌碰撞体
///   - 跟随 HandInput.IsActive 显隐(带丢帧宽限,短暂丢追踪不闪)
/// 对外暴露 Colliders 供抓取时屏蔽碰撞。
/// </summary>
[RequireComponent(typeof(HandInput))]
public class HandVisual : MonoBehaviour
{
    public float jointRadius = 0.1f;
    public Color skinColor = new Color(0.95f, 0.78f, 0.66f);
    public Color gripColor = new Color(0.25f, 0.9f, 0.45f);
    public float boneRadiusScale = 1.15f;
    public bool enablePhysicalColliders = true;

    public List<Collider> Colliders { get; private set; } = new List<Collider>();

    static readonly int[,] Conn = new int[,]
    {
        {0,1},{1,2},{2,3},{3,4},
        {0,5},{5,6},{6,7},{7,8},
        {5,9},{9,10},{10,11},{11,12},
        {9,13},{13,14},{14,15},{15,16},
        {13,17},{17,18},{18,19},{19,20},
        {0,17}
    };
    // 手掌多边形顶点(腕 + 五指根),做扇形三角化
    static readonly int[] Palm = { 0, 5, 9, 13, 17 };

    HandInput _in;
    GameObject _root;
    Rigidbody[] _jr = new Rigidbody[21];
    Rigidbody[] _br; Transform[] _bt; float[] _brad;
    Rigidbody _palmRb; Transform _palmTr;
    Mesh _palmMesh; Vector3[] _palmVerts;
    Material _skin;
    bool _collidersActive = true;

    float JointR(int i)
    {
        switch (i)
        {
            case 0: return jointRadius * 1.6f;
            case 1: case 5: case 9: case 13: case 17: return jointRadius * 1.25f;
            case 4: case 8: case 12: case 16: case 20: return jointRadius * 0.75f;
            default: return jointRadius * 1.0f;
        }
    }

    void Awake() { _in = GetComponent<HandInput>(); }

    void Start() { _skin = MakeMaterial(skinColor); Build(); }

    void Build()
    {
        _root = new GameObject("HandRig");
        _root.transform.parent = transform;

        for (int i = 0; i < 21; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Joint_" + i;
            go.transform.parent = _root.transform;
            go.transform.localScale = Vector3.one * (JointR(i) * 2f);
            go.GetComponent<Renderer>().material = _skin;
            Colliders.Add(go.GetComponent<Collider>());
            _jr[i] = SetupKinematic(go.AddComponent<Rigidbody>());
        }

        int n = Conn.GetLength(0);
        _br = new Rigidbody[n]; _bt = new Transform[n]; _brad = new float[n];
        for (int i = 0; i < n; i++)
        {
            _brad[i] = (JointR(Conn[i, 0]) + JointR(Conn[i, 1])) * 0.5f * boneRadiusScale;
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Bone_" + i;
            go.transform.parent = _root.transform;
            go.GetComponent<Renderer>().material = _skin;
            Colliders.Add(go.GetComponent<Collider>());
            _br[i] = SetupKinematic(go.AddComponent<Rigidbody>());
            _bt[i] = go.transform;
        }

        // 手掌物理碰撞体(隐形 box)
        var palmCol = GameObject.CreatePrimitive(PrimitiveType.Cube);
        palmCol.name = "PalmCollider";
        palmCol.transform.parent = _root.transform;
        palmCol.GetComponent<Renderer>().enabled = false;
        Colliders.Add(palmCol.GetComponent<Collider>());
        _palmRb = SetupKinematic(palmCol.AddComponent<Rigidbody>());
        _palmTr = palmCol.transform;

        // 手掌视觉网格(填充蹼面)
        var palmVis = new GameObject("PalmMesh");
        palmVis.transform.parent = _root.transform;
        var mf = palmVis.AddComponent<MeshFilter>();
        var mr = palmVis.AddComponent<MeshRenderer>();
        mr.material = _skin;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _palmMesh = new Mesh { name = "PalmMesh" };
        _palmVerts = new Vector3[Palm.Length];
        // 扇形三角(从腕部 v0 出发) + 反面,做成双面
        var tris = new List<int>();
        for (int i = 1; i < Palm.Length - 1; i++)
        { tris.Add(0); tris.Add(i); tris.Add(i + 1); }
        int frontCount = tris.Count;
        for (int i = 0; i < frontCount; i += 3)
        { tris.Add(tris[i]); tris.Add(tris[i + 2]); tris.Add(tris[i + 1]); }
        _palmMesh.vertices = _palmVerts;
        _palmMesh.triangles = tris.ToArray();
        mf.mesh = _palmMesh;
        _root.SetActive(false);
        SetHandColliders(false);
    }

    Rigidbody SetupKinematic(Rigidbody rb)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        return rb;
    }

    void FixedUpdate()
    {
        bool active = _in.IsActive;
        if (_root.activeSelf != active) _root.SetActive(active);
        SetHandColliders(active && enablePhysicalColliders);
        if (!active) return;

        var P = _in.Points;
        ApplyGripTint();

        for (int i = 0; i < 21; i++)
            _jr[i].MovePosition(P[i]);

        int n = Conn.GetLength(0);
        for (int i = 0; i < n; i++)
        {
            Vector3 a = P[Conn[i, 0]], b = P[Conn[i, 1]];
            Vector3 dir = b - a; float len = dir.magnitude;
            if (len < 1e-4f) continue;
            _br[i].MovePosition((a + b) * 0.5f);
            _br[i].MoveRotation(Quaternion.FromToRotation(Vector3.up, dir / len));
            _bt[i].localScale = new Vector3(_brad[i] * 2f, len * 0.5f, _brad[i] * 2f);
        }

        UpdatePalm(P);
        UpdatePalmMesh(P);
    }

    void ApplyGripTint()
    {
        Color color = Color.Lerp(skinColor, gripColor, Mathf.Clamp01(_in.PinchOnlyStrength) * 0.45f);
        _skin.color = color;
        if (_skin.HasProperty("_BaseColor")) _skin.SetColor("_BaseColor", color);
    }

    void SetHandColliders(bool active)
    {
        if (_collidersActive == active) return;
        _collidersActive = active;
        foreach (var c in Colliders)
            if (c != null) c.enabled = active;
    }

    void UpdatePalm(Vector3[] P)
    {
        Vector3 wrist = P[0];
        Vector3 knuckles = (P[5] + P[9] + P[13] + P[17]) * 0.25f;
        Vector3 up = knuckles - wrist; float upLen = up.magnitude;
        if (upLen < 1e-4f) return; up /= upLen;
        Vector3 right = P[5] - P[17]; float rLen = right.magnitude;
        if (rLen < 1e-4f) return; right /= rLen;
        Vector3 fwd = Vector3.Cross(up, right).normalized;
        if (fwd.sqrMagnitude < 1e-6f) return;
        _palmRb.MovePosition((wrist + knuckles) * 0.5f);
        _palmRb.MoveRotation(Quaternion.LookRotation(fwd, up));
        _palmTr.localScale = new Vector3(rLen * 1.05f, upLen * 0.9f, jointRadius * 1.6f);
    }

    void UpdatePalmMesh(Vector3[] P)
    {
        for (int i = 0; i < Palm.Length; i++)
            _palmVerts[i] = P[Palm[i]];
        _palmMesh.vertices = _palmVerts;
        _palmMesh.RecalculateNormals();
        _palmMesh.RecalculateBounds();
    }

    void OnDestroy() { if (_palmMesh) Destroy(_palmMesh); }

    static Material MakeMaterial(Color c)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Sprites/Default");
        var m = new Material(s);
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.35f);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.35f);
        return m;
    }
}
