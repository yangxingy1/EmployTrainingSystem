using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class HubInteractor : MonoBehaviour
{
    public float interactionRadius = 2.2f;

    readonly List<TaskStation> _nearbyStations = new List<TaskStation>();
    PlayerController _playerController;
    TaskStation _currentStation;
    string _statusMessage = "";
    float _statusMessageUntil;
    bool _enteringTraining;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        if (_playerController != null)
            _playerController.InteractPressed += HandleInteractPressed;
    }

    void OnDisable()
    {
        if (_playerController != null)
            _playerController.InteractPressed -= HandleInteractPressed;
    }

    void Update()
    {
        PruneInvalidStations();
        _currentStation = FindNearestStation();
        if (_currentStation == null)
            _currentStation = FindNearestStationInScene();
    }

    void OnTriggerEnter(Collider other)
    {
        var station = other.GetComponent<TaskStation>();
        if (station == null) station = other.GetComponentInParent<TaskStation>();
        if (station != null && !_nearbyStations.Contains(station))
            _nearbyStations.Add(station);
    }

    void OnTriggerExit(Collider other)
    {
        var station = other.GetComponent<TaskStation>();
        if (station == null) station = other.GetComponentInParent<TaskStation>();
        if (station != null)
            _nearbyStations.Remove(station);
    }

    void OnGUI()
    {
        var prompt = GetPromptText();
        if (string.IsNullOrEmpty(prompt)) return;

        var width = Mathf.Min(620f, Screen.width - 40f);
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 92f, width, 42f);
        GUI.Box(rect, prompt);
    }

    void HandleInteractPressed()
    {
        if (_currentStation == null)
        {
            ShowStatus("Move near a task station first.");
            return;
        }

        if (_currentStation.state == TaskStationState.Locked)
        {
            ShowStatus("Locked / Not assigned");
            Debug.Log($"[HubInteractor] Locked station ignored: anchor={_currentStation.anchorId}");
            return;
        }

        if (_enteringTraining) return;

        var session = SessionManager.EnsureExists();
        var task = new AssignedTask(
            _currentStation.taskId,
            _currentStation.displayName,
            _currentStation.sceneName,
            _currentStation.anchorId);

        var returnPosition = GetReturnPositionNearStation(_currentStation);
        session.selectedTaskId = task.taskId;
        session.selectedInstruction = "";
        session.selectedSuccessMessage = "成功完成训练";

        Debug.Log($"[HubInteractor] Enter scene: {task.sceneName} (taskId={task.taskId}, anchor={task.anchorId})");
        StartCoroutine(EnterTrainingRoutine(task, returnPosition));
    }

    string GetPromptText()
    {
        if (Time.time < _statusMessageUntil && !string.IsNullOrEmpty(_statusMessage))
            return _statusMessage;

        if (_currentStation == null) return "";
        if (_currentStation.state == TaskStationState.Locked)
            return "Locked / Not assigned";

        return $"Press E to enter {_currentStation.displayName}";
    }

    void ShowStatus(string message)
    {
        _statusMessage = message;
        _statusMessageUntil = Time.time + 1.8f;
    }

    IEnumerator EnterTrainingRoutine(AssignedTask task, Vector3 returnPosition)
    {
        _enteringTraining = true;

        var handService = MockHandTrackingService.Instance;
        if (handService != null && handService.Status != HandTrackingStatus.Ready)
        {
            ShowStatus($"Hand tracking is {handService.Status}. Calibrate first; entering practice mode.");
            Debug.LogWarning($"[HubInteractor] Hand tracking not ready: {handService.Status}. Entering practice mode anyway.");
            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            ShowStatus($"Entering {task.displayName}: {task.sceneName}");
            yield return new WaitForSeconds(0.15f);
        }

        SceneFlow.EnsureExists().EnterTraining(task, returnPosition);
    }

    Vector3 GetReturnPositionNearStation(TaskStation station)
    {
        var offset = transform.position - station.transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude < 0.01f)
            offset = Vector3.back;

        return station.transform.position + offset.normalized * 1.8f + Vector3.up;
    }

    TaskStation FindNearestStation()
    {
        TaskStation nearest = null;
        var nearestDistance = float.MaxValue;

        foreach (var station in _nearbyStations)
        {
            if (station == null) continue;

            var distance = Vector3.Distance(transform.position, station.transform.position);
            if (distance > interactionRadius) continue;
            if (distance >= nearestDistance) continue;

            nearest = station;
            nearestDistance = distance;
        }

        return nearest;
    }

    TaskStation FindNearestStationInScene()
    {
        TaskStation nearest = null;
        var nearestDistance = float.MaxValue;
        var stations = FindObjectsOfType<TaskStation>();

        foreach (var station in stations)
        {
            if (station == null) continue;

            var distance = Vector3.Distance(transform.position, station.transform.position);
            if (distance > interactionRadius) continue;
            if (distance >= nearestDistance) continue;

            nearest = station;
            nearestDistance = distance;
        }

        return nearest;
    }

    void PruneInvalidStations()
    {
        for (int i = _nearbyStations.Count - 1; i >= 0; i--)
        {
            var station = _nearbyStations[i];
            if (station == null || Vector3.Distance(transform.position, station.transform.position) > interactionRadius + 0.8f)
                _nearbyStations.RemoveAt(i);
        }
    }
}
