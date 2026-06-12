using UnityEngine;

public class ButtonInteractable : MonoBehaviour, IGestureInteractable
{
    public GestureReceiver receiver;
    public Renderer targetRenderer;
    public Color idleColor = new Color(0.2f, 0.45f, 0.9f);
    public Color pressedColor = new Color(0.1f, 0.8f, 0.3f);
    public float pressedSeconds = 0.2f;

    float _pressedUntil;

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
        SetColor(idleColor);
    }

    void OnEnable()
    {
        if (receiver != null) receiver.OnGestureReceived += HandleGesture;
    }

    void OnDisable()
    {
        if (receiver != null) receiver.OnGestureReceived -= HandleGesture;
    }

    void Update()
    {
        if (Time.time > _pressedUntil) SetColor(idleColor);
    }

    public void HandleGesture(GestureMessage message)
    {
        if (!message.IsGesture("click", "trigger")) return;
        TriggerClick();
    }

    public void TriggerClick()
    {
        _pressedUntil = Time.time + pressedSeconds;
        SetColor(pressedColor);
        Debug.Log("[ButtonInteractable] Click triggered");
    }

    void SetColor(Color color)
    {
        if (targetRenderer == null) return;
        var mat = targetRenderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }
}
