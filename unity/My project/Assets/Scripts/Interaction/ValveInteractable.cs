using UnityEngine;

public class ValveInteractable : MonoBehaviour, IGestureInteractable
{
    public GestureReceiver receiver;
    public Transform valveWheel;
    public float rotateGain = 1.0f;
    public float minAngle = -180f;
    public float maxAngle = 180f;
    public bool requireGrabToRotate = false;
    public Renderer targetRenderer;
    public Color idleColor = new Color(0.9f, 0.55f, 0.15f);
    public Color activeColor = new Color(1f, 0.85f, 0.25f);

    float _angle;
    bool _grabbed;

    void Awake()
    {
        if (valveWheel == null) valveWheel = transform;
        if (targetRenderer == null) targetRenderer = valveWheel.GetComponentInChildren<Renderer>();
        SetHighlighted(false);
    }

    void OnEnable()
    {
        if (receiver != null) receiver.OnGestureReceived += HandleGesture;
    }

    void OnDisable()
    {
        if (receiver != null) receiver.OnGestureReceived -= HandleGesture;
    }

    public void HandleGesture(GestureMessage message)
    {
        if (message.IsGesture("grab", "start")) _grabbed = true;
        if (message.IsGesture("grab", "end")) _grabbed = false;

        if ((!requireGrabToRotate || _grabbed) && message.IsGesture("rotate", "update") && message.@params != null)
            ApplyRotation(message.@params.angleDelta);
    }

    public void ApplyRotation(float angleDelta)
    {
        _angle = Mathf.Clamp(_angle + angleDelta * rotateGain, minAngle, maxAngle);
        valveWheel.localRotation = Quaternion.Euler(0f, 0f, _angle);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (targetRenderer == null) return;
        var mat = targetRenderer.material;
        var color = highlighted ? activeColor : idleColor;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    public float CurrentAngle => _angle;
}
