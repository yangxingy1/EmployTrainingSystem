using System.Collections;
using UnityEngine;

public class BreakerSwitchRuntime : MonoBehaviour
{
    public BreakerShutdownStationRuntime station;
    public int breakerNumber;

    public Transform leverPivot;

    public Quaternion onRotation;
    public Quaternion offRotation;
    public float pullDuration = 0.42f;

    public bool isOff = false;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (leverPivot == null)
        {
            leverPivot = transform;
        }

        gameObject.isStatic = false;
        leverPivot.gameObject.isStatic = false;
    }

    public void SetOff()
    {
        if (isOff) return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(SetOffRoutine());
    }

    public IEnumerator SetOffRoutine()
    {
        if (isOff) yield break;

        isOff = true;
        yield return AnimateTo(offRotation, pullDuration);
        animationCoroutine = null;
    }

    public void ResetToOn()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        isOff = false;

        if (leverPivot == null)
        {
            leverPivot = transform;
        }

        leverPivot.localRotation = onRotation;
    }

    public void BounceBack()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        Quaternion start = leverPivot.localRotation;
        Quaternion wrongPull = Quaternion.Slerp(onRotation, offRotation, 0.45f);

        yield return AnimateToInternal(start, wrongPull, 0.08f);
        yield return AnimateToInternal(wrongPull, onRotation, 0.14f);

        leverPivot.localRotation = onRotation;
    }

    private IEnumerator AnimateTo(Quaternion target, float duration)
    {
        Quaternion start = leverPivot.localRotation;
        yield return AnimateToInternal(start, target, duration);
        leverPivot.localRotation = target;
    }

    private IEnumerator AnimateToInternal(Quaternion start, Quaternion target, float duration)
    {
        if (leverPivot == null)
        {
            leverPivot = transform;
        }

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float smooth = k * k * (3f - 2f * k);

            leverPivot.localRotation = Quaternion.Slerp(start, target, smooth);
            yield return null;
        }

        leverPivot.localRotation = target;
    }
}
