using System.Collections.Generic;
using UnityEngine;

public class TaskStationSpawner : MonoBehaviour
{
    static readonly Vector3[] AnchorPositions =
    {
        new Vector3(-6.8f, 0f, 4.9f),
        new Vector3(6.8f, 0f, 4.9f),
        new Vector3(-6.8f, 0f, -1.9f),
        new Vector3(6.8f, 0f, -1.9f),
    };

    public void SpawnStations(IReadOnlyList<AssignedTask> assignedTasks)
    {
        ClearChildren();

        var tasksByAnchor = new Dictionary<int, AssignedTask>();
        if (assignedTasks != null)
        {
            foreach (var task in assignedTasks)
            {
                if (task == null) continue;
                if (task.anchorId < 0 || task.anchorId >= AnchorPositions.Length)
                {
                    Debug.LogWarning($"[TaskStationSpawner] Ignoring task with invalid anchorId: {task.taskId} -> {task.anchorId}");
                    continue;
                }

                tasksByAnchor[task.anchorId] = task;
            }
        }

        for (int anchorId = 0; anchorId < AnchorPositions.Length; anchorId++)
        {
            var stationGo = new GameObject("TaskStation Anchor " + anchorId);
            stationGo.transform.SetParent(transform, false);
            stationGo.transform.localPosition = AnchorPositions[anchorId];

            var station = stationGo.AddComponent<TaskStation>();
            if (tasksByAnchor.TryGetValue(anchorId, out var task))
                station.Configure(task, anchorId);
            else
                station.ConfigureLocked(anchorId);
        }
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}
