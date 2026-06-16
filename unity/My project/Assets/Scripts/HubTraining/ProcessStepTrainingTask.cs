using UnityEngine;

public class ProcessStepTrainingTask : MonoBehaviour
{
    class Scenario
    {
        public string title;
        public string objective;
        public string[] buttons;
        public int[] sequence;
        public Color accent;
    }

    public HandInput hand;
    public GraspController grasp;
    public string scenarioId = "ppe_check";

    FingertipTapButton[] _buttons;
    TextMesh _status;
    TextMesh _targetText;
    Scenario _scenario;
    int _step;
    int _mistakes;
    bool _completed;

    void Start()
    {
        _scenario = BuildScenario(scenarioId);
        BuildScene();
        UpdateStatus();
    }

    void Update()
    {
        if (_completed) return;

        float progress = _scenario.sequence.Length == 0 ? 0f : _step / (float)_scenario.sequence.Length;
        TrainingFlowController.Active?.ReportProgress(progress, "按顺序完成流程点击", CurrentTargetText());
    }

    void BuildScene()
    {
        BuildPanel();
        BuildButtons();
        BuildText();
    }

    void BuildPanel()
    {
        CreateBox("ProcessPanel", new Vector3(0f, 0f, 0.16f), new Vector3(4.75f, 2.65f, 0.16f), new Color(0.18f, 0.20f, 0.24f));
        CreateBox("ProcessHeader", new Vector3(0f, 1.12f, 0.05f), new Vector3(4.75f, 0.28f, 0.10f), Color.Lerp(_scenario.accent, Color.black, 0.25f));
        CreateBox("ProcessFooter", new Vector3(0f, -1.13f, 0.05f), new Vector3(4.75f, 0.16f, 0.10f), new Color(0.10f, 0.12f, 0.15f));

        for (int i = 0; i < _scenario.sequence.Length; i++)
        {
            float x = -1.5f + i * (3f / Mathf.Max(1, _scenario.sequence.Length - 1));
            CreateBox("StepIndicator_" + i, new Vector3(x, -1.12f, -0.05f), new Vector3(0.34f, 0.10f, 0.06f), new Color(0.35f, 0.40f, 0.46f));
        }
    }

    void BuildButtons()
    {
        int count = _scenario.buttons.Length;
        _buttons = new FingertipTapButton[count];
        int columns = Mathf.Min(3, count);
        float startX = -(columns - 1) * 0.88f;
        float startY = count > 3 ? 0.42f : 0.08f;

        for (int i = 0; i < count; i++)
        {
            int index = i;
            int row = i / columns;
            int col = i % columns;
            float x = startX + col * 1.76f;
            float y = startY - row * 0.72f;

            var go = new GameObject("ProcessButton_" + _scenario.buttons[i]);
            go.transform.parent = transform;
            var button = go.AddComponent<FingertipTapButton>();
            button.hand = hand;
            button.grasp = grasp;
            button.Build(new Vector3(x, y, -0.05f), new Vector3(1.42f, 0.40f, 0.09f), _scenario.buttons[i], ButtonColor(i));
            button.Clicked += () => HandleButton(index);
            _buttons[i] = button;
        }
    }

    void BuildText()
    {
        var titleGo = new GameObject("ProcessTitle");
        titleGo.transform.parent = transform;
        titleGo.transform.position = new Vector3(0f, 1.76f, -0.08f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = _scenario.title;
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 52;
        title.characterSize = 0.048f;
        title.color = Color.white;

        var targetGo = new GameObject("ProcessTarget");
        targetGo.transform.parent = transform;
        targetGo.transform.position = new Vector3(0f, 1.28f, -0.08f);
        _targetText = targetGo.AddComponent<TextMesh>();
        _targetText.anchor = TextAnchor.MiddleCenter;
        _targetText.alignment = TextAlignment.Center;
        _targetText.fontSize = 36;
        _targetText.characterSize = 0.036f;
        _targetText.color = new Color(0.82f, 1f, 0.88f);

        var statusGo = new GameObject("ProcessStatus");
        statusGo.transform.parent = transform;
        statusGo.transform.position = new Vector3(3.05f, 0.90f, -0.08f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperRight;
        _status.alignment = TextAlignment.Right;
        _status.fontSize = 34;
        _status.characterSize = 0.034f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void HandleButton(int index)
    {
        if (_completed) return;

        int expected = _scenario.sequence[Mathf.Clamp(_step, 0, _scenario.sequence.Length - 1)];
        if (index == expected)
        {
            _step++;
            if (_step >= _scenario.sequence.Length)
            {
                _completed = true;
                TrainingFlowController.Active?.RecordSuccess(_scenario.title + "完成");
                TrainingFlowController.Active?.CompleteTraining(_scenario.objective);
            }
        }
        else
        {
            _mistakes++;
            TrainingFlowController.Active?.RecordMistake("误点: " + _scenario.buttons[index]);
        }

        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (_targetText != null)
            _targetText.text = _completed ? "流程完成" : CurrentTargetText();

        if (_status == null) return;
        _status.text =
            "目标: " + _scenario.objective +
            "\n进度: " + Mathf.Min(_step, _scenario.sequence.Length) + "/" + _scenario.sequence.Length +
            "\n误操作: " + _mistakes +
            "\n状态: " + (_completed ? "已完成" : "等待点击");
    }

    string CurrentTargetText()
    {
        if (_completed || _scenario.sequence.Length == 0) return "流程完成";
        int target = _scenario.sequence[Mathf.Clamp(_step, 0, _scenario.sequence.Length - 1)];
        return "当前步骤: 点击 " + _scenario.buttons[target];
    }

    Color ButtonColor(int index)
    {
        Color[] colors =
        {
            new Color(0.18f, 0.58f, 1f),
            new Color(0.20f, 0.72f, 0.42f),
            new Color(0.92f, 0.52f, 0.18f),
            new Color(0.72f, 0.42f, 0.92f),
        };
        return Color.Lerp(colors[index % colors.Length], _scenario.accent, 0.20f);
    }

    Scenario BuildScenario(string id)
    {
        switch (id)
        {
            case "dispatch_console":
                return ScenarioOf("派工路线确认", "按路线领取本次任务", new[] { "安全准备", "设备点检", "主任务", "入库确认" }, new[] { 0, 1, 2, 3 }, new Color(0.20f, 0.66f, 1f));
            case "inspection_check":
                return ScenarioOf("设备点检训练", "依次确认班前点检项", new[] { "电源", "急停", "联锁" }, new[] { 0, 1, 2 }, new Color(0.24f, 0.88f, 0.48f));
            case "safety_gate":
                return ScenarioOf("安全闸门训练", "停线后开闸，离开后关闸", new[] { "停线", "开闸", "关闸" }, new[] { 0, 1, 2 }, new Color(1f, 0.72f, 0.16f));
            case "quality_scan":
                return ScenarioOf("质检扫码训练", "识别样本并选择处理流向", new[] { "合格", "返修", "复核" }, new[] { 0, 1, 2 }, new Color(0.20f, 0.66f, 1f));
            case "tool_select":
                return ScenarioOf("维修工具选择", "按作业类型选择工具", new[] { "绝缘手套", "扭矩扳手", "点检表" }, new[] { 0, 1, 2 }, new Color(0.92f, 0.60f, 0.18f));
            case "storage_checkin":
                return ScenarioOf("成品入库确认", "完成扫码、核数和入库", new[] { "扫码", "核数", "入库" }, new[] { 0, 1, 2 }, new Color(0.38f, 0.78f, 0.58f));
            case "alarm_reset":
                return ScenarioOf("告警复位训练", "确认告警后复位并记录", new[] { "确认告警", "复位", "记录" }, new[] { 0, 1, 2 }, new Color(1f, 0.30f, 0.18f));
            case "central_control":
                return ScenarioOf("中央控制台点检", "确认控制台关键状态", new[] { "电源", "急停", "联锁" }, new[] { 0, 1, 2 }, new Color(0.20f, 0.66f, 1f));
            default:
                return ScenarioOf("PPE 安全装备检查", "依次确认个人防护装备", new[] { "安全帽", "护目镜", "绝缘手套" }, new[] { 0, 1, 2 }, new Color(1f, 0.78f, 0.16f));
        }
    }

    Scenario ScenarioOf(string title, string objective, string[] buttons, int[] sequence, Color accent)
    {
        return new Scenario
        {
            title = title,
            objective = objective,
            buttons = buttons,
            sequence = sequence,
            accent = accent,
        };
    }

    GameObject CreateBox(string name, Vector3 position, Vector3 scale, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.parent = transform;
        box.transform.localPosition = position;
        box.transform.localScale = scale;
        Destroy(box.GetComponent<Collider>());
        SetColor(box.GetComponent<Renderer>(), color);
        return box;
    }

    static void SetColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }
}
