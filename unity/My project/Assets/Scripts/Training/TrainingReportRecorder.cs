using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class TrainingStepReport
{
    public int index;
    public string name;
    public string expectedAction;
    public bool completed;
    public int mistakeCount;
}

[Serializable]
public class TrainingErrorReport
{
    public float time;
    public int stepIndex;
    public string stepName;
    public string reason;
    public string consequence;
    public string severity;
}

[Serializable]
public class TrainingReportPayload
{
    public string taskId;
    public string sceneName;
    public int score;
    public int trainTime;
    public string startedAt;
    public string finishedAt;
    public List<TrainingStepReport> steps = new List<TrainingStepReport>();
    public List<TrainingErrorReport> errors = new List<TrainingErrorReport>();
}

public class TrainingReportRecorder
{
    public TrainingReportPayload Report { get; private set; }
    public string LastSavedPath { get; private set; }
    public string ErrorSummary { get; private set; } = "无关键错误";

    float _startedAtTime;
    int _mistakePenalty;

    public void Begin(string taskId, string sceneName, string[] stepNames, string[] expectedActions)
    {
        _startedAtTime = Time.time;
        _mistakePenalty = 0;
        LastSavedPath = "";
        ErrorSummary = "无关键错误";

        Report = new TrainingReportPayload
        {
            taskId = taskId,
            sceneName = sceneName,
            score = 100,
            trainTime = 0,
            startedAt = DateTime.Now.ToString("o"),
            finishedAt = ""
        };

        int count = Mathf.Max(stepNames != null ? stepNames.Length : 0, expectedActions != null ? expectedActions.Length : 0);
        for (int i = 0; i < count; i++)
        {
            Report.steps.Add(new TrainingStepReport
            {
                index = i,
                name = stepNames != null && i < stepNames.Length ? stepNames[i] : "步骤 " + (i + 1),
                expectedAction = expectedActions != null && i < expectedActions.Length ? expectedActions[i] : "",
                completed = false,
                mistakeCount = 0
            });
        }
    }

    public void MarkStepCompleted(int stepIndex)
    {
        if (Report == null || stepIndex < 0 || stepIndex >= Report.steps.Count) return;
        Report.steps[stepIndex].completed = true;
    }

    public void RecordError(int stepIndex, string reason, string consequence, string severity)
    {
        if (Report == null) return;

        string stepName = "未知步骤";
        int mistakeCountForStep = 1;
        if (stepIndex >= 0 && stepIndex < Report.steps.Count)
        {
            var step = Report.steps[stepIndex];
            stepName = step.name;
            step.mistakeCount++;
            mistakeCountForStep = step.mistakeCount;
        }

        Report.errors.Add(new TrainingErrorReport
        {
            time = Mathf.Max(0f, Time.time - _startedAtTime),
            stepIndex = stepIndex,
            stepName = stepName,
            reason = reason,
            consequence = consequence,
            severity = string.IsNullOrEmpty(severity) ? "normal" : severity
        });

        _mistakePenalty += severity == "safety" ? 12 : 5;
        if (mistakeCountForStep >= 3)
            _mistakePenalty += 2;

        ErrorSummary = BuildErrorSummary();
    }

    public int Complete(float targetSeconds)
    {
        if (Report == null) return 0;

        Report.finishedAt = DateTime.Now.ToString("o");
        Report.trainTime = Mathf.RoundToInt(Mathf.Max(0f, Time.time - _startedAtTime));

        int timePenalty = 0;
        if (targetSeconds > 0f && Report.trainTime > targetSeconds)
        {
            float overRatio = (Report.trainTime - targetSeconds) / targetSeconds;
            timePenalty = Mathf.Clamp(Mathf.CeilToInt(overRatio * 10f), 1, 10);
        }

        Report.score = Mathf.Clamp(100 - _mistakePenalty - timePenalty, 0, 100);
        ErrorSummary = BuildErrorSummary();
        LastSavedPath = SaveLatestReport();
        return Report.score;
    }

    string SaveLatestReport()
    {
        string dir = Path.Combine(Application.persistentDataPath, "training-results");
        Directory.CreateDirectory(dir);

        string path = Path.Combine(dir, "latest-training-report.json");
        File.WriteAllText(path, JsonUtility.ToJson(Report, true));
        Debug.Log("[TrainingReport] Saved " + path);
        return path;
    }

    string BuildErrorSummary()
    {
        if (Report == null || Report.errors == null || Report.errors.Count == 0)
            return "无关键错误";

        int count = Mathf.Min(Report.errors.Count, 3);
        var parts = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var error = Report.errors[i];
            parts.Add((i + 1) + ". " + error.stepName + "：" + error.reason + "；后果：" + error.consequence);
        }

        if (Report.errors.Count > count)
            parts.Add("另有 " + (Report.errors.Count - count) + " 条错误，详见 JSON 报告。");

        return string.Join("\n", parts.ToArray());
    }
}
