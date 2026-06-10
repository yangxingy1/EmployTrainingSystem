using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 数据层:WebSocket 接收 21 关节,映射到相机正对的 XY 交互平面(z 恒为 0)。
/// 位移增益(gain):只放大手的整体位移、不放大手本身尺寸,小动作即可跑满全场。
/// 对外:IsActive / Points[21] / GripPoint / PinchOnlyStrength / FistStrength / PinchStrength。
/// </summary>
public class HandInput : MonoBehaviour
{
    [Header("连接")]
    public string url = "ws://127.0.0.1:8765";

    [Header("交互平面 (相机正对的 XY 平面, z=0)")]
    public float planeWidth = 5.4f;
    public float planeHeight = 3.6f;
    public Vector3 planeOrigin = Vector3.zero;

    [Header("位移增益 (小动作覆盖大范围, 减少挥臂)")]
    public float gain = 1.4f;

    [Header("平滑 / 捏合(尺度无关)")]
    [Range(0.05f, 1f)] public float smoothing = 0.5f;
    public float openRatio = 1.0f;
    public float closeRatio = 0.35f;
    public float graceTime = 0.3f;

    public bool IsActive { get; private set; }
    public bool IsTrackedNow { get; private set; }
    public Vector3[] Points { get; private set; } = new Vector3[21];
    public Vector3 GripPoint { get; private set; }
    public Vector3 PalmCenter { get; private set; }
    public float PalmAngle { get; private set; }
    public float PinchOnlyStrength { get; private set; }
    public float FistStrength { get; private set; }
    public float PinchRatio { get; private set; }
    public float PinchStrength { get; private set; }
    public bool IsGripping { get; private set; }

    static readonly int[] PalmIdx = { 0, 5, 9, 13, 17 };

    readonly object _lock = new object();
    float[] _latest;
    bool _present;
    Vector3[] _smooth = new Vector3[21];
    bool _hasSmooth;
    float _lastSeen = -999f;
    const float GripStartThreshold = 0.42f;
    const float GripEndThreshold = 0.24f;

    ClientWebSocket _ws;
    CancellationTokenSource _cts;

    [Serializable] class HandMsg { public bool present; public float[] x; public float[] y; public float[] z; }

    void Start() { _cts = new CancellationTokenSource(); _ = Loop(_cts.Token); }

    void FixedUpdate()
    {
        bool present; float[] data = null;
        lock (_lock) { present = _present; if (present && _latest != null) data = (float[])_latest.Clone(); }

        IsTrackedNow = present;
        if (present && data != null)
        {
            // 掌心(归一化)作为增益参考点
            float cx = 0f, cy = 0f;
            foreach (int k in PalmIdx) { cx += data[k * 3 + 0]; cy += data[k * 3 + 1]; }
            cx /= PalmIdx.Length; cy /= PalmIdx.Length;
            float acx = (cx - 0.5f) * gain + 0.5f;   // 只放大整体位移
            float acy = (cy - 0.5f) * gain + 0.5f;

            for (int i = 0; i < 21; i++)
            {
                float fx = acx + (data[i * 3 + 0] - cx);  // 保留手内部相对形状
                float fy = acy + (data[i * 3 + 1] - cy);
                Vector3 p = planeOrigin + new Vector3(
                    (fx - 0.5f) * planeWidth,
                    (0.5f - fy) * planeHeight,
                    0f);
                _smooth[i] = _hasSmooth ? Vector3.Lerp(_smooth[i], p, smoothing) : p;
                Points[i] = _smooth[i];
            }
            _hasSmooth = true;

            GripPoint = (Points[4] + Points[8]) * 0.5f;
            PalmCenter = Vector3.zero;
            foreach (int k in PalmIdx) PalmCenter += Points[k];
            PalmCenter /= PalmIdx.Length;
            Vector3 palmAxis = Points[17] - Points[5];
            PalmAngle = Mathf.Atan2(palmAxis.y, palmAxis.x) * Mathf.Rad2Deg;

            float rawPalmW = RawDistance(data, 5, 17);
            if (rawPalmW < 1e-4f) rawPalmW = 1f;
            PinchRatio = RawDistance(data, 4, 8) / rawPalmW;
            float pinch = Mathf.Clamp01((openRatio - PinchRatio) / (openRatio - closeRatio));

            float scenePalmW = Vector3.Distance(Points[5], Points[17]);
            if (scenePalmW < 1e-4f) scenePalmW = 1f;
            float fist = (
                FingerCurl(5, 8, scenePalmW) +
                FingerCurl(9, 12, scenePalmW) +
                FingerCurl(13, 16, scenePalmW) +
                FingerCurl(17, 20, scenePalmW)
            ) * 0.25f;

            PinchOnlyStrength = pinch;
            FistStrength = fist;
            PinchStrength = Mathf.Max(PinchOnlyStrength, FistStrength);
            if (!IsGripping && PinchStrength >= GripStartThreshold) IsGripping = true;
            if (IsGripping && PinchStrength <= GripEndThreshold) IsGripping = false;

            _lastSeen = Time.time;
        }
        IsActive = (Time.time - _lastSeen) <= graceTime;
    }

    float FingerCurl(int mcp, int tip, float palmW)
    {
        float mcpDist = Vector3.Distance(Points[mcp], Points[0]);
        float tipDist = Vector3.Distance(Points[tip], Points[0]);
        float folded = (mcpDist * 1.25f - tipDist) / Mathf.Max(palmW * 0.65f, 1e-4f);
        return Mathf.Clamp01(folded);
    }

    float RawDistance(float[] data, int a, int b)
    {
        float dx = data[a * 3 + 0] - data[b * 3 + 0];
        float dy = data[a * 3 + 1] - data[b * 3 + 1];
        float dz = data[a * 3 + 2] - data[b * 3 + 2];
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    async Task Loop(CancellationToken token)
    {
        var buffer = new byte[64 * 1024];
        while (!token.IsCancellationRequested)
        {
            try
            {
                _ws = new ClientWebSocket();
                await _ws.ConnectAsync(new Uri(url), token);
                Debug.Log("[HandInput] 已连接 " + url);

                var sb = new StringBuilder();
                while (_ws.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    sb.Clear();
                    WebSocketReceiveResult r;
                    do
                    {
                        r = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (r.MessageType == WebSocketMessageType.Close)
                        { await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token); break; }
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, r.Count));
                    } while (!r.EndOfMessage);
                    if (sb.Length > 0) Parse(sb.ToString());
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception e)
            {
                Debug.LogWarning("[HandInput] 连接异常: " + e.Message + " — 1 秒后重试");
                try { await Task.Delay(1000, token); } catch { break; }
            }
        }
    }

    void Parse(string json)
    {
        HandMsg msg;
        try { msg = JsonUtility.FromJson<HandMsg>(json); } catch { return; }
        if (msg == null) return;
        lock (_lock)
        {
            bool ok = msg.present && msg.x != null && msg.x.Length == 21;
            _present = ok;
            if (ok)
            {
                if (_latest == null) _latest = new float[63];
                for (int i = 0; i < 21; i++)
                {
                    _latest[i * 3 + 0] = msg.x[i];
                    _latest[i * 3 + 1] = msg.y[i];
                    _latest[i * 3 + 2] = msg.z[i];
                }
            }
        }
    }

    void OnDestroy() { _cts?.Cancel(); try { _ws?.Dispose(); } catch { } }
}
