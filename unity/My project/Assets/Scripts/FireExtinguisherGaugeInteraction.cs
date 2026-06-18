using System.Collections;
using UnityEngine;

public class FireExtinguisherGaugeInteraction : MonoBehaviour
{
    public Transform needlePivot;
    public float normalAngle = 265f;
    public KeyCode interactKey = KeyCode.H;
    public float enlargedScaleMultiplier = 5f;
    public float enlargedSeconds = 3f;
    public float transitionSeconds = 0.15f;
    public Transform gaugeRoot;
    public Transform[] gaugeParts;

    private Coroutine enlargeRoutine;
    private Transform[] runtimeGaugeParts;
    private Vector3[] originalGaugeScales;
    private Vector3[] originalGaugePositions;
    private bool hasOriginalGaugeState;

    private bool suppressGuidedInput;

    private void Awake()
    {
        EnsureGaugeParts();

        if (needlePivot != null)
        {
            needlePivot.gameObject.isStatic = false;
        }
    }

    private void Update()
    {
        if (suppressGuidedInput || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        InspectGauge();
    }

    private void OnMouseDown()
    {
        InspectGauge();
    }

    public void InspectGauge()
    {
        if (needlePivot != null)
        {
            needlePivot.localRotation = Quaternion.Euler(0f, 0f, normalAngle);
        }

        if (enlargeRoutine != null)
        {
            StopCoroutine(enlargeRoutine);
        }

        enlargeRoutine = StartCoroutine(EnlargeGaugeTemporarily());
        Debug.Log("Fire extinguisher gauge inspected.");
    }

    private IEnumerator EnlargeGaugeTemporarily()
    {
        EnsureGaugeParts();

        if (runtimeGaugeParts == null || runtimeGaugeParts.Length == 0)
        {
            yield break;
        }

        CacheOriginalGaugeState();

        Vector3 center = transform.localPosition;

        yield return AnimateGaugeScale(enlargedScaleMultiplier, originalGaugeScales, originalGaugePositions, center);
        yield return new WaitForSeconds(enlargedSeconds);
        yield return AnimateGaugeScale(1f, originalGaugeScales, originalGaugePositions, center);

        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] == null)
            {
                continue;
            }

            runtimeGaugeParts[i].localScale = originalGaugeScales[i];
            runtimeGaugeParts[i].localPosition = originalGaugePositions[i];
        }

        enlargeRoutine = null;
    }

    private IEnumerator AnimateGaugeScale(
        float toMultiplier,
        Vector3[] originalScales,
        Vector3[] originalPositions,
        Vector3 center)
    {
        if (transitionSeconds <= 0f)
        {
            ApplyGaugeScale(toMultiplier, originalScales, originalPositions, center);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < transitionSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionSeconds);
            float smoothT = t * t * (3f - 2f * t);
            float scale = Mathf.Lerp(GetCurrentGaugeMultiplier(originalScales), toMultiplier, smoothT);
            ApplyGaugeScale(scale, originalScales, originalPositions, center);
            yield return null;
        }

        ApplyGaugeScale(toMultiplier, originalScales, originalPositions, center);
    }

    private void ApplyGaugeScale(
        float multiplier,
        Vector3[] originalScales,
        Vector3[] originalPositions,
        Vector3 center)
    {
        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] == null)
            {
                continue;
            }

            runtimeGaugeParts[i].localScale = originalScales[i] * multiplier;
            runtimeGaugeParts[i].localPosition = center + (originalPositions[i] - center) * multiplier;
        }
    }

    private float GetCurrentGaugeMultiplier(Vector3[] originalScales)
    {
        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] != null && originalScales[i].sqrMagnitude > 0f)
            {
                return runtimeGaugeParts[i].localScale.magnitude / originalScales[i].magnitude;
            }
        }

        return 1f;
    }

    private void CacheOriginalGaugeState()
    {
        if (hasOriginalGaugeState && originalGaugeScales != null && originalGaugeScales.Length == runtimeGaugeParts.Length)
        {
            return;
        }

        originalGaugeScales = new Vector3[runtimeGaugeParts.Length];
        originalGaugePositions = new Vector3[runtimeGaugeParts.Length];

        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] == null)
            {
                continue;
            }

            originalGaugeScales[i] = runtimeGaugeParts[i].localScale;
            originalGaugePositions[i] = runtimeGaugeParts[i].localPosition;
        }

        hasOriginalGaugeState = true;
    }

    private void EnsureGaugeParts()
    {
        if (gaugeRoot != null)
        {
            runtimeGaugeParts = new[] { gaugeRoot };
            gaugeRoot.gameObject.isStatic = false;
            return;
        }

        if (gaugeParts != null && gaugeParts.Length > 0)
        {
            runtimeGaugeParts = gaugeParts;
            MarkGaugePartsDynamic();
            return;
        }

        Transform parent = transform.parent;
        if (parent == null)
        {
            runtimeGaugeParts = new[] { transform };
            MarkGaugePartsDynamic();
            return;
        }

        Transform ring = parent.Find("Pressure_Gauge_Metal_Ring");
        Transform greenZone = parent.Find("Pressure_Gauge_Green_Zone");
        Transform pivot = needlePivot != null ? needlePivot : parent.Find("Pressure_Gauge_Needle_Pivot");

        runtimeGaugeParts = new[]
        {
            ring,
            transform,
            greenZone,
            pivot
        };

        MarkGaugePartsDynamic();
    }

    private void MarkGaugePartsDynamic()
    {
        if (runtimeGaugeParts == null)
        {
            return;
        }

        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] != null)
            {
                runtimeGaugeParts[i].gameObject.isStatic = false;
            }
        }
    }

    public IEnumerator PlayGuidedSequence()
    {
        suppressGuidedInput = true;
        InspectGauge();
        yield return new WaitForSeconds(enlargedSeconds + transitionSeconds * 2f + 0.6f);
        suppressGuidedInput = false;
    }
}
