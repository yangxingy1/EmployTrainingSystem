using UnityEngine;

public class MainBreakerToggle : MonoBehaviour
{
    public bool enableMouseClick = false;

    public Transform handle;
    public Renderer ledRenderer;

    public Material onMaterial;
    public Material offMaterial;

    public bool isOn = true;

    public float onAngle = -35f;
    public float offAngle = 35f;

    private void OnMouseDown()
    {
        if (!enableMouseClick)
        {
            return;
        }

        Toggle();
    }

    public void Toggle()
    {
        isOn = !isOn;

        if (handle != null)
        {
            float angle = isOn ? onAngle : offAngle;
            handle.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        if (ledRenderer != null)
        {
            ledRenderer.sharedMaterial = isOn ? onMaterial : offMaterial;
        }

        Debug.Log(isOn ? "Main breaker switched ON." : "Main breaker switched OFF.");
    }
}
