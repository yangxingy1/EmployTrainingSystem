using UnityEngine;

public class ReadyStatusHud : MonoBehaviour
{
    IHandTrackingService _handTrackingService;
    HandTrackingStatus _handStatus = HandTrackingStatus.NotConnected;

    public void Initialize(IHandTrackingService handTrackingService)
    {
        if (_handTrackingService != null)
            _handTrackingService.OnStatusChanged -= HandleHandStatusChanged;

        _handTrackingService = handTrackingService;
        _handStatus = handTrackingService != null ? handTrackingService.Status : HandTrackingStatus.NotConnected;

        if (_handTrackingService != null)
            _handTrackingService.OnStatusChanged += HandleHandStatusChanged;
    }

    void OnDestroy()
    {
        if (_handTrackingService != null)
            _handTrackingService.OnStatusChanged -= HandleHandStatusChanged;
    }

    void OnGUI()
    {
        var rect = new Rect(18f, 18f, 260f, 112f);
        GUI.Box(rect, "Hub Ready Status");

        DrawLamp(new Rect(34f, 48f, 220f, 20f), "Backend", true, "Mock");
        DrawLamp(new Rect(34f, 72f, 220f, 20f), "Camera", true, "Mock");
        DrawLamp(new Rect(34f, 96f, 220f, 20f), "Hand", _handStatus == HandTrackingStatus.Ready, _handStatus.ToString());
    }

    void DrawLamp(Rect rect, string label, bool ready, string value)
    {
        var oldColor = GUI.color;
        GUI.color = ready ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.72f, 0.24f);
        GUI.Label(new Rect(rect.x, rect.y, 18f, rect.height), ready ? "OK" : "!");
        GUI.color = oldColor;
        GUI.Label(new Rect(rect.x + 30f, rect.y, rect.width - 30f, rect.height), $"{label}: {value}");
    }

    void HandleHandStatusChanged(HandTrackingStatus status)
    {
        _handStatus = status;
    }
}
