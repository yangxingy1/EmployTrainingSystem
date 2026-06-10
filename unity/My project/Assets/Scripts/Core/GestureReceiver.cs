using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GestureReceiver : MonoBehaviour
{
    [Header("Connection")]
    public string url = "ws://127.0.0.1:8766";

    public bool IsConnected { get; private set; }
    public event Action<GestureMessage> OnGestureReceived;

    readonly ConcurrentQueue<GestureMessage> _queue = new ConcurrentQueue<GestureMessage>();
    ClientWebSocket _ws;
    CancellationTokenSource _cts;

    void Start()
    {
        _cts = new CancellationTokenSource();
        _ = ReceiveLoop(_cts.Token);
    }

    void Update()
    {
        while (_queue.TryDequeue(out var msg))
            OnGestureReceived?.Invoke(msg);
    }

    async Task ReceiveLoop(CancellationToken token)
    {
        var buffer = new byte[32 * 1024];
        while (!token.IsCancellationRequested)
        {
            try
            {
                _ws = new ClientWebSocket();
                await _ws.ConnectAsync(new Uri(url), token);
                IsConnected = true;
                Debug.Log("[GestureReceiver] Connected " + url);

                var sb = new StringBuilder();
                while (_ws.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    sb.Clear();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token);
                            break;
                        }
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    if (sb.Length > 0)
                    {
                        var msg = JsonUtility.FromJson<GestureMessage>(sb.ToString());
                        if (msg != null && msg.type == "gesture")
                            _queue.Enqueue(msg);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[GestureReceiver] " + e.GetType().Name + ": " + e.Message + " - retrying in 1s\n" + e);
                IsConnected = false;
                try { await Task.Delay(1000, token); } catch { break; }
            }
        }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        try { _ws?.Dispose(); } catch { }
    }
}
