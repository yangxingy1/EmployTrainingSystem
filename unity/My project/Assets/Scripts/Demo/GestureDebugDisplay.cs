using UnityEngine;

public class GestureDebugDisplay : MonoBehaviour
{
    public GestureReceiver receiver;
    public HandInput handInput;
    public TextMesh statusText;

    string _lastGesture = "waiting";
    string _lastState = "";
    float _lastConfidence;
    float _lastAngle;

    void OnEnable()
    {
        if (receiver != null) receiver.OnGestureReceived += OnGesture;
    }

    void OnDisable()
    {
        if (receiver != null) receiver.OnGestureReceived -= OnGesture;
    }

    void Update()
    {
        if (statusText == null) return;

        string handState = handInput != null && handInput.IsActive ? "tracked" : "not tracked";
        float pinch = handInput != null ? handInput.PinchStrength : 0f;
        statusText.text =
            "Hand: " + handState +
            "\nGesture: " + _lastGesture + " " + _lastState +
            "\nGrip: " + pinch.ToString("0.00") +
            "\nAngle: " + _lastAngle.ToString("0.0") +
            "\nConfidence: " + _lastConfidence.ToString("0.00");
    }

    void OnGesture(GestureMessage message)
    {
        _lastGesture = message.gesture;
        _lastState = message.state;
        _lastConfidence = message.confidence;
        if (message.@params != null) _lastAngle = message.@params.totalAngle;
    }
}
