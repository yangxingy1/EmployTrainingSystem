using UnityEngine;

public enum TrainingMode
{
    Guide,
    Practice,
    Exam
}

public class TrainingTaskManager : MonoBehaviour
{
    public TrainingMode mode = TrainingMode.Practice;
    public GestureReceiver receiver;
    public ValveInteractable valve;
    public OperationLogger logger;
    public TextMesh statusText;
    public float targetValveAngle = 45f;
    public float angleTolerance = 10f;

    bool _clicked;

    void OnEnable()
    {
        if (receiver != null) receiver.OnGestureReceived += OnGesture;
    }

    void OnDisable()
    {
        if (receiver != null) receiver.OnGestureReceived -= OnGesture;
    }

    void Start()
    {
        UpdateStatus("Task: rotate valve, then click button.");
    }

    void Update()
    {
        if (valve == null) return;
        bool angleOk = Mathf.Abs(valve.CurrentAngle - targetValveAngle) <= angleTolerance;
        if (angleOk && _clicked)
        {
            UpdateStatus("Task complete. Score: 100");
        }
    }

    void OnGesture(GestureMessage msg)
    {
        if (logger != null)
        {
            float value = 0f;
            if (msg.@params != null) value = msg.@params.totalAngle;
            logger.Record(msg.gesture, msg.state, value);
        }

        if (msg.IsGesture("click", "trigger"))
        {
            _clicked = true;
            UpdateStatus("Button confirmed.");
        }
    }

    void UpdateStatus(string text)
    {
        if (statusText != null) statusText.text = text;
        Debug.Log("[TrainingTask] " + text);
    }
}

