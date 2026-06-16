using System.Collections;
using UnityEngine;

public class ElectricalCabinetEInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public Transform playerOrCamera;
    public bool requireInteractionDistance = false;
    public float interactionDistance = 2.0f;
    public KeyCode interactKey = KeyCode.G;

    [Header("Cabinet Parts")]
    public Transform switchHandle;
    public Renderer ledRenderer;
    public Light ledLight;

    [Header("Switch State")]
    public bool isOn = false;

    [Tooltip("旋钮每次切换旋转多少度。这里按你的需求设置为 180。")]
    public float rotationDegrees = 180f;

    [Tooltip("旋钮绕哪个本地轴旋转。电柜前面板朝 -X 时，通常绕 X 轴旋转。")]
    public Vector3 localRotationAxis = Vector3.right;

    public bool useAbsoluteAngles = true;
    public float onAngle = -55f;
    public float offAngle = 55f;

    [Header("Animation")]
    public float rotateDuration = 0.6f;

    [Header("LED Colors")]
    public Color offColor = new Color(0.25f, 0.02f, 0.02f, 1f);
    public Color onColor = new Color(0.05f, 1.0f, 0.2f, 1f);
    public float ledEmissionIntensity = 2.0f;

    private Quaternion offRotation;
    private Quaternion onRotation;
    private float currentHandleAngle;
    private Coroutine rotateCoroutine;
    private MaterialPropertyBlock propertyBlock;

    private bool suppressGuidedInput;

    private void Awake()
    {
        AutoFindCabinetParts();

        if (playerOrCamera == null && Camera.main != null)
        {
            playerOrCamera = Camera.main.transform;
        }

        if (switchHandle != null)
        {
            switchHandle.gameObject.isStatic = false;

            if (useAbsoluteAngles)
            {
                offRotation = Quaternion.AngleAxis(offAngle, localRotationAxis.normalized);
                onRotation = Quaternion.AngleAxis(onAngle, localRotationAxis.normalized);
                currentHandleAngle = isOn ? onAngle : offAngle;
            }
            else
            {
                offRotation = switchHandle.localRotation;
                onRotation = offRotation * Quaternion.AngleAxis(rotationDegrees, localRotationAxis.normalized);
            }
        }

        propertyBlock = new MaterialPropertyBlock();

        ApplySwitchStateImmediate();
    }

    private void Update()
    {
        if (suppressGuidedInput || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        if (requireInteractionDistance)
        {
            if (playerOrCamera == null)
            {
                return;
            }

            float distance = Vector3.Distance(playerOrCamera.position, transform.position);
            if (distance > interactionDistance)
            {
                return;
            }
        }

        ToggleSwitch();
    }

    public void ToggleSwitch()
    {
        SetSwitchState(!isOn);
    }

    public void SetSwitchState(bool targetState)
    {
        isOn = targetState;

        if (switchHandle != null)
        {
            if (rotateCoroutine != null)
            {
                StopCoroutine(rotateCoroutine);
            }

            rotateCoroutine = StartCoroutine(RotateHandle(isOn ? onAngle : offAngle, isOn ? onRotation : offRotation));
        }

        ApplyLEDColor();
        Debug.Log(isOn ? "Electrical cabinet switched ON." : "Electrical cabinet switched OFF.");
    }

    private IEnumerator RotateHandle(float targetAngle, Quaternion fallbackTargetRotation)
    {
        float startAngle = currentHandleAngle;
        Quaternion startRotation = switchHandle.localRotation;
        float timer = 0f;

        while (timer < rotateDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / rotateDuration);

            // SmoothStep 让旋钮动作更自然，不是机械地匀速跳过去
            float smoothT = t * t * (3f - 2f * t);

            if (useAbsoluteAngles)
            {
                currentHandleAngle = Mathf.LerpAngle(startAngle, targetAngle, smoothT);
                switchHandle.localRotation = Quaternion.AngleAxis(currentHandleAngle, localRotationAxis.normalized);
            }
            else
            {
                switchHandle.localRotation = Quaternion.Slerp(startRotation, fallbackTargetRotation, smoothT);
            }

            yield return null;
        }

        if (useAbsoluteAngles)
        {
            currentHandleAngle = targetAngle;
            switchHandle.localRotation = Quaternion.AngleAxis(currentHandleAngle, localRotationAxis.normalized);
        }
        else
        {
            switchHandle.localRotation = fallbackTargetRotation;
        }
    }

    private void ApplySwitchStateImmediate()
    {
        if (switchHandle != null)
        {
            if (useAbsoluteAngles)
            {
                currentHandleAngle = isOn ? onAngle : offAngle;
                switchHandle.localRotation = Quaternion.AngleAxis(currentHandleAngle, localRotationAxis.normalized);
            }
            else
            {
                switchHandle.localRotation = isOn ? onRotation : offRotation;
            }
        }

        ApplyLEDColor();
    }

    private void ApplyLEDColor()
    {
        Color targetColor = isOn ? onColor : offColor;

        if (ledRenderer != null)
        {
            ledRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor("_BaseColor", targetColor);
            propertyBlock.SetColor("_Color", targetColor);
            propertyBlock.SetColor("_EmissionColor", targetColor * ledEmissionIntensity);

            ledRenderer.SetPropertyBlock(propertyBlock);
        }

        if (ledLight != null)
        {
            ledLight.color = targetColor;
            ledLight.enabled = isOn;
        }
    }

    private void AutoFindCabinetParts()
    {
        if (switchHandle == null)
        {
            Transform foundHandle = FindChildRecursive(transform, "Main_Breaker_Handle_Clickable");
            if (foundHandle != null)
            {
                switchHandle = foundHandle;
            }
        }

        if (ledRenderer == null)
        {
            Transform foundLed = FindChildRecursive(transform, "Power_LED_Green");
            if (foundLed != null)
            {
                ledRenderer = foundLed.GetComponent<Renderer>();
            }
        }
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
            {
                return child;
            }

            Transform result = FindChildRecursive(child, targetName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!requireInteractionDistance)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }

    public IEnumerator PlayGuidedSequence()
    {
        suppressGuidedInput = true;

        if (isOn)
        {
            SetSwitchState(false);
            yield return new WaitForSeconds(rotateDuration + 0.4f);
        }

        Debug.Log("引导：配电柜合闸送电");
        SetSwitchState(true);
        yield return new WaitForSeconds(rotateDuration + 0.8f);

        Debug.Log("引导：配电柜分闸断电");
        SetSwitchState(false);
        yield return new WaitForSeconds(rotateDuration + 0.4f);

        suppressGuidedInput = false;
    }
}
