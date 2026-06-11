using UnityEngine;

/// <summary>
/// 基础按钮点击训练: 根据提示点击正确的工位按钮。
/// </summary>
public class ButtonPressTrainingTask : MonoBehaviour
{
    public HandInput hand;
    public GraspController grasp;

    readonly string[] _labels = { "START", "RESET", "CONFIRM", "STOP" };
    readonly FingertipTapButton[] _buttons = new FingertipTapButton[4];
    TextMesh _status;
    int _targetIndex;
    int _success;
    int _mistake;

    void Start()
    {
        BuildTitle();
        BuildButtons();
        PickNextTarget();
        UpdateStatus("点击高亮提示的按钮");
    }

    void BuildTitle()
    {
        var titleGo = new GameObject("ButtonTaskTitle");
        titleGo.transform.parent = transform;
        titleGo.transform.position = new Vector3(0f, 1.72f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "按钮点击训练";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 54;
        title.characterSize = 0.052f;
        title.color = Color.white;

        var statusGo = new GameObject("ButtonTaskStatus");
        statusGo.transform.parent = transform;
        statusGo.transform.position = new Vector3(2.85f, 1.24f, -0.08f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void BuildButtons()
    {
        Vector3[] positions =
        {
            new Vector3(-1.25f, 0.55f, 0f),
            new Vector3(1.25f, 0.55f, 0f),
            new Vector3(-1.25f, -0.35f, 0f),
            new Vector3(1.25f, -0.35f, 0f),
        };

        Color[] colors =
        {
            new Color(0.12f, 0.48f, 0.92f),
            new Color(0.86f, 0.35f, 0.24f),
            new Color(0.18f, 0.70f, 0.38f),
            new Color(0.82f, 0.58f, 0.12f),
        };

        for (int i = 0; i < _buttons.Length; i++)
        {
            int index = i;
            var go = new GameObject("TrainingButton_" + _labels[i]);
            go.transform.parent = transform;
            var button = go.AddComponent<FingertipTapButton>();
            button.hand = hand;
            button.grasp = grasp;
            button.Build(positions[i], new Vector3(1.25f, 0.42f, 0.09f), _labels[i], colors[i]);
            button.Clicked += () => HandleButton(index);
            _buttons[i] = button;
        }
    }

    void PickNextTarget()
    {
        _targetIndex = Random.Range(0, _labels.Length);
        for (int i = 0; i < _buttons.Length; i++)
            if (_buttons[i] != null)
                _buttons[i].idleColor = i == _targetIndex
                    ? new Color(0.12f, 0.66f, 0.95f)
                    : new Color(0.28f, 0.32f, 0.40f);
    }

    void HandleButton(int index)
    {
        if (index == _targetIndex)
        {
            _success++;
            PickNextTarget();
            UpdateStatus("正确, 继续点击下一按钮");
        }
        else
        {
            _mistake++;
            UpdateStatus("误触: 请看清目标按钮");
        }
    }

    void UpdateStatus(string message)
    {
        if (_status == null) return;
        int total = _success + _mistake;
        int accuracy = total == 0 ? 100 : Mathf.RoundToInt(_success * 100f / total);
        _status.text =
            "任务: " + message +
            "\n目标: " + _labels[_targetIndex] +
            "\n正确: " + _success +
            "\n误触: " + _mistake +
            "\n正确率: " + accuracy;
    }
}
