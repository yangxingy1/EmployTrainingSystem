using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class OperationLogEntry
{
    public float time;
    public string eventName;
    public string state;
    public float value;
}

public class OperationLogger : MonoBehaviour
{
    public List<OperationLogEntry> entries = new List<OperationLogEntry>();

    public void Record(string eventName, string state, float value = 0f)
    {
        entries.Add(new OperationLogEntry
        {
            time = Time.time,
            eventName = eventName,
            state = state,
            value = value
        });
    }

    public string SaveJsonl(string fileName = "operation-log.jsonl")
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        using (var writer = new StreamWriter(path, false))
        {
            foreach (var entry in entries)
                writer.WriteLine(JsonUtility.ToJson(entry));
        }
        Debug.Log("[OperationLogger] Saved " + path);
        return path;
    }
}

