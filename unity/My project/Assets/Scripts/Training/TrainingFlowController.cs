using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingFlowController : MonoBehaviour
{
    public static TrainingFlowController Active { get; private set; }

    public string taskId;
    public string taskName;
    public string objective;
    public string gestureHint;
    public int targetSuccessCount = 3;
    public float targetSeconds = 90f;

    int _successCount;
    int _mistakeCount;
    float _startedAt;
    float _progress01;
    bool _completed;
    string _phase = "准备开始";
    string _detail = "";
    string _lastEvent = "暂无";
    float _lastEventAt = -99f;

    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _labelStyle;
    GUIStyle _smallStyle;
    GUIStyle _resultTitleStyle;
    GUIStyle _buttonStyle;

    public float Elapsed => Time.time - _startedAt;
    public int SuccessCount => _successCount;
    public int MistakeCount => _mistakeCount;
    public bool IsCompleted => _completed;

    public static TrainingFlowController EnsureExists(string selectedTaskId)
    {
        if (Active != null)
        {
            Active.ConfigureForTask(selectedTaskId);
            return Active;
        }

        var go = new GameObject("TrainingFlowController");
        var controller = go.AddComponent<TrainingFlowController>();
        controller.ConfigureForTask(selectedTaskId);
        return controller;
    }

    void Awake()
    {
        if (Active != null && Active != this)
        {
            Destroy(gameObject);
            return;
        }

        Active = this;
    }

    void OnDestroy()
    {
        if (Active == this) Active = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            RestartCurrentTraining();
    }

    public void ConfigureForTask(string selectedTaskId)
    {
        taskId = string.IsNullOrEmpty(selectedTaskId) ? "rotary_valve" : selectedTaskId;
        ApplyTaskPreset(taskId);
        ResetFlow();
    }

    public void ResetFlow()
    {
        _successCount = 0;
        _mistakeCount = 0;
        _progress01 = 0f;
        _completed = false;
        _startedAt = Time.time;
        _phase = "等待操作";
        _detail = objective;
        _lastEvent = "训练已开始";
        _lastEventAt = Time.time;
    }

    public void ReportProgress(float progress01, string phase, string detail = "")
    {
        if (_completed) return;

        _progress01 = Mathf.Clamp01(progress01);
        if (!string.IsNullOrEmpty(phase)) _phase = phase;
        if (!string.IsNullOrEmpty(detail)) _detail = detail;
    }

    public void RecordSuccess(string detail = "")
    {
        if (_completed) return;

        _successCount++;
        _lastEvent = string.IsNullOrEmpty(detail) ? "完成一次有效操作" : detail;
        _lastEventAt = Time.time;
        _progress01 = Mathf.Max(_progress01, Mathf.Clamp01(_successCount / Mathf.Max(1f, targetSuccessCount)));

        if (_successCount >= targetSuccessCount)
            CompleteTraining();
    }

    public void RecordMistake(string detail = "")
    {
        if (_completed) return;

        _mistakeCount++;
        _lastEvent = string.IsNullOrEmpty(detail) ? "记录一次误操作" : detail;
        _lastEventAt = Time.time;
    }

    public void CompleteTraining(string detail = "")
    {
        if (_completed) return;

        _completed = true;
        _progress01 = 1f;
        _phase = "训练完成";
        _detail = string.IsNullOrEmpty(detail) ? "已达到本工位训练目标" : detail;
        _lastEvent = _detail;
        _lastEventAt = Time.time;
    }

    public void RestartCurrentTraining()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ApplyTaskPreset(string id)
    {
        switch (id)
        {
            case "electric_switch":
                taskName = "拉杆电闸训练";
                objective = "完成2次电闸状态切换";
                gestureHint = "移动到拉杆把手，捏合后沿竖直方向拉动";
                targetSuccessCount = 2;
                targetSeconds = 60f;
                break;
            case "sort_line":
                taskName = "传送带分拣训练";
                objective = "正确分拣10件物料，并点击确认按钮";
                gestureHint = "抓取物块，放入同色料箱，达到目标后点击确认";
                targetSuccessCount = 11;
                targetSeconds = 150f;
                break;
            case "button_press":
                taskName = "按钮点击训练";
                objective = "连续正确点击5个指定按钮";
                gestureHint = "食指移到目标按钮上方，稳定后做一次下点";
                targetSuccessCount = 5;
                targetSeconds = 60f;
                break;
            case "slider_calibration":
                taskName = "滑块校准训练";
                objective = "将滑块调到3个目标刻度";
                gestureHint = "捏合滑块，沿水平轨道移动到目标刻度";
                targetSuccessCount = 3;
                targetSeconds = 75f;
                break;
            case "emergency_stop":
                taskName = "急停复位训练";
                objective = "完成一次急停按下与旋转复位";
                gestureHint = "食指下点急停按钮，锁定后捏合旋转复位";
                targetSuccessCount = 1;
                targetSeconds = 60f;
                break;
            case "mode_selector":
                taskName = "档位选择训练";
                objective = "切换到3个指定档位";
                gestureHint = "捏合旋钮，旋转到高亮目标档位并保持";
                targetSuccessCount = 3;
                targetSeconds = 75f;
                break;
            case "bolt_tightening":
                taskName = "螺栓拧紧训练";
                objective = "按顺序完成一组3颗螺栓拧紧";
                gestureHint = "捏合高亮螺栓，顺时针小幅连续旋转";
                targetSuccessCount = 1;
                targetSeconds = 120f;
                break;
            case "pick_place":
            case "material_transfer":
                taskName = "物料搬运训练";
                objective = "将3个方块搬运到目标区域";
                gestureHint = "靠近方块捏合抓取，移动到目标区后张开释放";
                targetSuccessCount = 3;
                targetSeconds = 90f;
                break;
            case "integrated_exam":
                taskName = "综合考核";
                objective = "完成当前综合任务流程";
                gestureHint = "按提示完成抓取、点击、旋转和确认操作";
                targetSuccessCount = 1;
                targetSeconds = 180f;
                break;
            case "ppe_check":
                taskName = "PPE 安全装备检查";
                objective = "依次确认安全帽、护目镜和绝缘手套";
                gestureHint = "食指点击当前提示的装备按钮";
                targetSuccessCount = 1;
                targetSeconds = 60f;
                break;
            case "dispatch_console":
                taskName = "派工路线确认";
                objective = "按路线领取本次训练任务";
                gestureHint = "依次点击安全准备、设备点检、主任务和入库确认";
                targetSuccessCount = 1;
                targetSeconds = 75f;
                break;
            case "inspection_check":
                taskName = "设备点检训练";
                objective = "依次确认电源、急停和安全联锁";
                gestureHint = "按点检顺序点击对应项目";
                targetSuccessCount = 1;
                targetSeconds = 75f;
                break;
            case "safety_gate":
                taskName = "安全闸门训练";
                objective = "按停线、开闸、关闸顺序完成准入流程";
                gestureHint = "按提示点击对应流程按钮";
                targetSuccessCount = 1;
                targetSeconds = 75f;
                break;
            case "quality_scan":
                taskName = "质检扫码训练";
                objective = "识别样本并选择合格、返修和复核流向";
                gestureHint = "按提示依次点击质检流向按钮";
                targetSuccessCount = 1;
                targetSeconds = 75f;
                break;
            case "tool_select":
                taskName = "维修工具选择训练";
                objective = "按作业要求选择绝缘手套、扭矩扳手和点检表";
                gestureHint = "按提示点击正确工具";
                targetSuccessCount = 1;
                targetSeconds = 75f;
                break;
            case "storage_checkin":
                taskName = "成品入库确认";
                objective = "完成扫码、核数和入库确认";
                gestureHint = "按提示点击入库流程按钮";
                targetSuccessCount = 1;
                targetSeconds = 75f;
                break;
            case "alarm_reset":
                taskName = "告警复位训练";
                objective = "确认告警、执行复位并记录";
                gestureHint = "按提示点击告警处理流程";
                targetSuccessCount = 1;
                targetSeconds = 75f;
                break;
            case "central_control":
                taskName = "中央控制台点检";
                objective = "完成控制台关键状态确认";
                gestureHint = "按提示点击电源、急停和联锁按钮";
                targetSuccessCount = 1;
                targetSeconds = 75f;
                break;
            default:
                taskName = "旋转阀门训练";
                objective = "将阀门旋转到3个指定角度";
                gestureHint = "捏合阀门边缘，围绕中心平稳旋转";
                targetSuccessCount = 3;
                targetSeconds = 90f;
                break;
        }
    }

    int CalculateScore()
    {
        int total = Mathf.Max(1, _successCount + _mistakeCount);
        float accuracyScore = _successCount * 100f / total;
        float timeScore = Elapsed <= targetSeconds
            ? 100f
            : Mathf.Clamp(100f - (Elapsed - targetSeconds) * 1.4f, 60f, 100f);
        return Mathf.RoundToInt(accuracyScore * 0.70f + timeScore * 0.30f);
    }

    void EnsureStyles()
    {
        if (_panelStyle != null) return;

        _panelStyle = new GUIStyle(GUI.skin.box);
        _panelStyle.normal.textColor = Color.white;
        _panelStyle.padding = new RectOffset(16, 16, 14, 14);
        _panelStyle.alignment = TextAnchor.UpperLeft;

        _titleStyle = new GUIStyle(GUI.skin.label);
        _titleStyle.fontSize = 22;
        _titleStyle.fontStyle = FontStyle.Bold;
        _titleStyle.normal.textColor = new Color(0.92f, 0.97f, 1f);
        _titleStyle.wordWrap = true;

        _labelStyle = new GUIStyle(GUI.skin.label);
        _labelStyle.fontSize = 17;
        _labelStyle.normal.textColor = new Color(0.86f, 0.92f, 0.98f);
        _labelStyle.wordWrap = true;

        _smallStyle = new GUIStyle(GUI.skin.label);
        _smallStyle.fontSize = 14;
        _smallStyle.normal.textColor = new Color(0.70f, 0.78f, 0.86f);
        _smallStyle.wordWrap = true;

        _resultTitleStyle = new GUIStyle(_titleStyle);
        _resultTitleStyle.fontSize = 28;
        _resultTitleStyle.alignment = TextAnchor.MiddleCenter;
        _resultTitleStyle.normal.textColor = new Color(0.35f, 1f, 0.55f);

        _buttonStyle = new GUIStyle(GUI.skin.button);
        _buttonStyle.fontSize = 16;
        _buttonStyle.fontStyle = FontStyle.Bold;
    }

    void OnGUI()
    {
        EnsureStyles();
        DrawTrainingPanel();
        if (_completed) DrawResultPanel();
    }

    void DrawTrainingPanel()
    {
        float width = 380f;
        var rect = new Rect(Screen.width - width - 22f, 22f, width, 270f);
        GUI.Box(rect, "", _panelStyle);

        GUILayout.BeginArea(new Rect(rect.x + 16f, rect.y + 12f, rect.width - 32f, rect.height - 24f));
        GUILayout.Label(taskName, _titleStyle);
        GUILayout.Space(4f);
        GUILayout.Label("目标: " + objective, _labelStyle);
        GUILayout.Label("提示: " + gestureHint, _smallStyle);
        GUILayout.Space(8f);
        GUILayout.Label("状态: " + _phase, _labelStyle);
        if (!string.IsNullOrEmpty(_detail))
            GUILayout.Label(_detail, _smallStyle);
        GUILayout.Space(6f);
        GUILayout.Label("进度: " + _successCount + "/" + targetSuccessCount + "    误操作: " + _mistakeCount, _labelStyle);
        DrawProgressBar(GUILayoutUtility.GetRect(1f, 16f), _progress01);
        GUILayout.Space(6f);
        GUILayout.Label("用时: " + Elapsed.ToString("0.0") + "s / " + targetSeconds.ToString("0") + "s", _smallStyle);
        string recent = Time.time - _lastEventAt < 1.8f ? _lastEvent : "持续训练中";
        GUILayout.Label("最近反馈: " + recent, _smallStyle);
        GUILayout.Space(6f);
        GUILayout.Label("T: 重新开始    R/Esc: 返回大厅", _smallStyle);
        GUILayout.EndArea();
    }

    void DrawResultPanel()
    {
        float width = 440f;
        float height = 250f;
        var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        GUI.Box(rect, "", _panelStyle);

        GUILayout.BeginArea(new Rect(rect.x + 22f, rect.y + 18f, rect.width - 44f, rect.height - 36f));
        GUILayout.Label("训练完成", _resultTitleStyle);
        GUILayout.Space(10f);
        GUILayout.Label("工位: " + taskName, _labelStyle);
        GUILayout.Label("用时: " + Elapsed.ToString("0.0") + " 秒", _labelStyle);
        GUILayout.Label("有效操作: " + _successCount + "    误操作: " + _mistakeCount, _labelStyle);
        GUILayout.Label("综合评分: " + CalculateScore() + " 分", _labelStyle);
        GUILayout.Space(12f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新开始 (T)", _buttonStyle, GUILayout.Height(38f)))
            RestartCurrentTraining();
        if (GUILayout.Button("返回大厅", _buttonStyle, GUILayout.Height(38f)))
            SceneFlow.EnsureExists().ReturnToHub();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void DrawProgressBar(Rect rect, float value)
    {
        GUI.Box(rect, "");
        var fill = new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(value), rect.height - 4f);
        var previous = GUI.color;
        GUI.color = _completed ? new Color(0.25f, 0.95f, 0.42f) : new Color(0.20f, 0.58f, 1f);
        GUI.DrawTexture(fill, Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
