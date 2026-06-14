using System;
using UnityEngine;

public class MockHandTrackingService : MonoBehaviour, IHandTrackingService
{
    public static MockHandTrackingService Instance { get; private set; }

    public HandTrackingStatus status = HandTrackingStatus.Ready;

    public HandTrackingStatus Status => status;
    public event Action<HandTrackingStatus> OnStatusChanged;

    HandTrackingStatus _lastStatus;

    public static MockHandTrackingService EnsureExists()
    {
        if (Instance != null) return Instance;

        var go = new GameObject("MockHandTrackingService");
        return go.AddComponent<MockHandTrackingService>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _lastStatus = status;
    }

    void Update()
    {
        if (_lastStatus == status) return;

        _lastStatus = status;
        OnStatusChanged?.Invoke(status);
        Debug.Log($"[MockHandTrackingService] Status changed: {status}");
    }
}
