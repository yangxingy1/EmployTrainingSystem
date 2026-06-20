using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 化工厂管道培训流程管理器。
/// 管理 10 个培训步骤的推进、验证和状态展示。
/// 支持 Guide（引导）和 Practice（自由练习）两种模式。
/// </summary>
public class PipelineTrainingManager : MonoBehaviour
{
    // ── 培训模式 ───────────────────────────────────────────────
    public TrainingMode mode = TrainingMode.Guide;
    public OperationLogger logger;

    [Header("UI")]
    public TextMesh titleText;
    public TextMesh stepText;
    public TextMesh instructionText;
    public TextMesh statusText;

    // ── 10 个培训步骤定义 ──────────────────────────────────────

    public enum PipelineStep
    {
        NotStarted = 0,
        PPECheck = 1,              // 1. 安全装备确认
        SystemInspection = 2,       // 2. 系统状态巡检
        ReadInitialPressure = 3,    // 3. 读取初始压力表
        OpenInletValve = 4,         // 4. 开启进口主阀门
        MonitorFlowMeter = 5,       // 5. 监测流量计
        AdjustControlValve = 6,     // 6. 调节中间控制阀
        CheckMidPressure = 7,       // 7. 检查中段压力表
        OpenOutletValve = 8,        // 8. 开启出口阀门
        EmergencyStopTest = 9,      // 9. 急停功能测试
        SystemShutdown = 10,        // 10. 系统关停与记录
        AllComplete = 11
    }

    [Serializable]
    public struct StepDefinition
    {
        public PipelineStep step;
        public string name;
        public string instruction;
        public string completionHint;     // 完成后显示的提示
        public float targetValveAngle;    // 阀门目标角度（阀门类步骤用）
        public float angleTolerance;      // 角度容差
        public string targetButtonId;     // 按钮 ID（按钮类步骤用）
        public string targetZoneId;       // 区域 ID（巡检类步骤用）
        public float targetGaugeValue;    // 压力表目标值（读数类步骤用）
        public float gaugeTolerance;      // 读数容差
    }

    public List<StepDefinition> steps = new List<StepDefinition>();

    // ── 运行时状态 ─────────────────────────────────────────────
    private PipelineStep _currentStep = PipelineStep.NotStarted;
    private Dictionary<PipelineStep, bool> _stepCompleted = new Dictionary<PipelineStep, bool>();
    private Dictionary<string, float> _valveAngles = new Dictionary<string, float>();
    private Dictionary<string, float> _gaugeValues = new Dictionary<string, float>();
    private Dictionary<string, bool> _buttonsPressed = new Dictionary<string, bool>();
    private Dictionary<string, bool> _zonesVisited = new Dictionary<string, bool>();

    // 巡检点独立计数（每区一个 key）
    private HashSet<string> _inspectionZonesVisited = new HashSet<string>();
    [HideInInspector] public int totalInspectionZones = 3; // 由 Runtime 动态更新

    // 仪表读数交互标记
    private HashSet<string> _gaugesRead = new HashSet<string>();

    private bool _flowMeterObserved = false;
    private float _flowMeterObservedTime = 0f;
    private float _startedAtTime;
    private bool _resultUploaded;

    // ── 事件 ───────────────────────────────────────────────────
    public event Action<PipelineStep> OnStepCompleted;
    public event Action<PipelineStep> OnStepChanged;

    // ═══════════════════════════════════════════════════════════
    //  初始化
    // ═══════════════════════════════════════════════════════════

    void Start()
    {
        InitializeSteps();
        ResetTraining();

        if (mode == TrainingMode.Guide)
            AdvanceToStep(PipelineStep.PPECheck);
        else
            _currentStep = PipelineStep.NotStarted; // 自由模式：同时开放所有步骤
    }

    void Update()
    {
        if (mode == TrainingMode.Guide && _currentStep != PipelineStep.AllComplete)
        {
            CheckStepCompletion(_currentStep);
        }
        else if (mode == TrainingMode.Practice)
        {
            // 自由模式：检查所有未完成步骤
            for (PipelineStep s = PipelineStep.PPECheck; s <= PipelineStep.SystemShutdown; s++)
            {
                if (!IsStepCompleted(s))
                    CheckStepCompletion(s);
            }
        }

        UpdateUI();
    }

    void InitializeSteps()
    {
        if (steps.Count > 0) return; // 已在 Inspector 中配置

        steps = new List<StepDefinition>
        {
            new StepDefinition
            {
                step = PipelineStep.PPECheck,
                name = "1. 安全装备确认",
                instruction = "进入操作区域前，请确认已正确穿戴安全帽、防护手套和护目镜。\n确认后按下【确认】按钮。",
                completionHint = "PPE 确认完成，可以进入操作区域。",
                targetButtonId = "PPE_Confirm"
            },
            new StepDefinition
            {
                step = PipelineStep.SystemInspection,
                name = "2. 系统状态巡检",
                instruction = "沿管道走向检查管线外观：\n· 确认管道无泄漏痕迹\n· 检查法兰螺栓是否紧固\n· 确认无异常报警信号\n走到每个巡检点按 F 键确认。",
                completionHint = "系统巡检完成，管道状态正常。",
                targetZoneId = "InspectionZone"
            },
            new StepDefinition
            {
                step = PipelineStep.ReadInitialPressure,
                name = "3. 读取初始压力表",
                instruction = "走到管道起始端压力表（P1）前，\n读取并记录当前压力值。\n正常范围：0.0 - 0.1 MPa（静止状态）。",
                completionHint = "初始压力读数已记录。",
                targetGaugeValue = 0.05f,
                gaugeTolerance = 0.10f
            },
            new StepDefinition
            {
                step = PipelineStep.OpenInletValve,
                name = "4. 开启进口主阀门",
                instruction = "握住进口阀门（V1）手轮，\n逆时针缓慢旋转至全开位置。\n目标：旋转 ≥ 1440°（约 4 圈），\n注意：缓慢开启，观察压力变化。",
                completionHint = "进口阀门已全开，流体开始进入管道。",
                targetValveAngle = 1440f,
                angleTolerance = 90f
            },
            new StepDefinition
            {
                step = PipelineStep.MonitorFlowMeter,
                name = "5. 监测流量计",
                instruction = "观察管道中段流量计（F1）的读数。\n正常流量范围：20 - 50 L/min。\n如果流量不在正常范围，请进入下一步调节。",
                completionHint = "流量计读数已确认。",
                targetGaugeValue = 35f,
                gaugeTolerance = 15f
            },
            new StepDefinition
            {
                step = PipelineStep.AdjustControlValve,
                name = "6. 调节中间控制阀",
                instruction = "旋转控制阀门（V2）手轮，\n将流量调节至目标值：35 ± 5 L/min。\n顺时针 = 减小流量，逆时针 = 增大流量。",
                completionHint = "流量已调节至目标范围。",
                targetValveAngle = 720f,
                angleTolerance = 180f
            },
            new StepDefinition
            {
                step = PipelineStep.CheckMidPressure,
                name = "7. 检查中段压力表",
                instruction = "走到管道中段压力表（P2）前，\n确认压力稳定在安全范围：0.4 - 0.8 MPa。\n如果压力异常，回到步骤 6 重新调节。",
                completionHint = "中段压力正常，管道运行稳定。",
                targetGaugeValue = 0.6f,
                gaugeTolerance = 0.25f
            },
            new StepDefinition
            {
                step = PipelineStep.OpenOutletValve,
                name = "8. 开启出口阀门",
                instruction = "走到管道出口端，缓慢打开出口阀门（V3）。\n目标：旋转 ≥ 1080°（约 3 圈）。\n注意：打开后观察下游流量变化。",
                completionHint = "出口阀门已打开，完整流体通路建立。",
                targetValveAngle = 1080f,
                angleTolerance = 90f
            },
            new StepDefinition
            {
                step = PipelineStep.EmergencyStopTest,
                name = "9. 急停功能测试",
                instruction = "按下急停按钮（EMG-STOP），\n验证系统快速切断功能：\n· 阀门应自动关断\n· 报警灯应亮起\n按下【复位】按钮恢复正常状态。",
                completionHint = "急停功能测试通过，系统已复位。",
                targetButtonId = "EStop_Button"
            },
            new StepDefinition
            {
                step = PipelineStep.SystemShutdown,
                name = "10. 系统关停与记录",
                instruction = "按正确顺序关闭系统：\n① 先关闭进口阀门（V1）\n② 等待管道降压（观察 P2 < 0.1 MPa）\n③ 再关闭出口阀门（V3）\n④ 记录最终仪表读数\n完成后按下【提交记录】按钮。",
                completionHint = "培训完成！系统已安全关停，操作记录已保存。",
                targetButtonId = "Shutdown_Confirm"
            }
        };
    }

    // ═══════════════════════════════════════════════════════════
    //  公开 API —— 供交互组件调用
    // ═══════════════════════════════════════════════════════════

    /// <summary>阀门角度变化时调用</summary>
    public void ReportValveAngle(string valveId, float currentAngle)
    {
        _valveAngles[valveId] = currentAngle;

        if (logger != null)
            logger.Record("valve_" + valveId, "rotate", currentAngle);
    }

    /// <summary>按钮被按下时调用</summary>
    public void ReportButtonPress(string buttonId)
    {
        _buttonsPressed[buttonId] = true;

        if (logger != null)
            logger.Record("button_" + buttonId, "press", 1f);
    }

    /// <summary>玩家进入区域时调用</summary>
    public void ReportZoneVisit(string zoneId)
    {
        _zonesVisited[zoneId] = true;

        // 巡检点独立追踪（key 格式: "InspectionZone_0", "InspectionZone_1" ...）
        if (zoneId.StartsWith("InspectionZone_"))
            _inspectionZonesVisited.Add(zoneId);

        if (logger != null)
            logger.Record("zone_" + zoneId, "enter", 1f);
    }

    /// <summary>仪表读数交互（玩家按 F 靠近仪表时调用）</summary>
    public void ReportGaugeRead(string gaugeId)
    {
        _gaugesRead.Add(gaugeId);
        Debug.Log("[PipelineTraining] 仪表读数: " + gaugeId);
    }

    /// <summary>压力表读数变化时调用</summary>
    public void ReportGaugeValue(string gaugeId, float value)
    {
        _gaugeValues[gaugeId] = value;
    }

    /// <summary>流量计被观察（玩家看向流量计一定时间）</summary>
    public void ReportFlowMeterObserved()
    {
        _flowMeterObserved = true;
        _flowMeterObservedTime = Time.time;
    }

    /// <summary>获取当前步骤索引（供 UI 使用）</summary>
    public PipelineStep CurrentStep => _currentStep;

    /// <summary>检查某步骤是否已完成</summary>
    public bool IsStepCompleted(PipelineStep step)
    {
        return _stepCompleted.TryGetValue(step, out bool v) && v;
    }

    /// <summary>获取步骤定义</summary>
    public StepDefinition GetStepDef(PipelineStep step)
    {
        foreach (var def in steps)
            if (def.step == step) return def;
        return default;
    }

    /// <summary>获取当前阀门角度</summary>
    public float GetValveAngle(string valveId)
    {
        _valveAngles.TryGetValue(valveId, out float v);
        return v;
    }

    /// <summary>强制完成某个步骤（调试用）</summary>
    public void ForceCompleteStep(PipelineStep step)
    {
        CompleteStep(step);
    }

    /// <summary>重置全部培训状态</summary>
    public void ResetTraining()
    {
        _currentStep = PipelineStep.NotStarted;
        _startedAtTime = Time.time;
        _resultUploaded = false;
        _stepCompleted.Clear();
        _valveAngles.Clear();
        _gaugeValues.Clear();
        _buttonsPressed.Clear();
        _zonesVisited.Clear();
        _inspectionZonesVisited.Clear();
        _gaugesRead.Clear();
        _flowMeterObserved = false;
        _flowMeterObservedTime = 0f;

        foreach (var def in steps)
            _stepCompleted[def.step] = false;
    }

    // ═══════════════════════════════════════════════════════════
    //  内部逻辑
    // ═══════════════════════════════════════════════════════════

    void CheckStepCompletion(PipelineStep step)
    {
        if (IsStepCompleted(step)) return;

        StepDefinition def = GetStepDef(step);
        if (def.step == PipelineStep.NotStarted) return;

        bool completed = false;

        switch (step)
        {
            case PipelineStep.PPECheck:
                completed = IsButtonPressed(def.targetButtonId);
                break;

            case PipelineStep.SystemInspection:
                completed = _inspectionZonesVisited.Count >= totalInspectionZones;
                break;

            case PipelineStep.ReadInitialPressure:
                // 需要玩家按 F 靠近 P1 仪表 + 读数在正常范围
                completed = _gaugesRead.Contains("Gauge_P1")
                         && IsGaugeNearTarget("gauge_P1", def.targetGaugeValue, def.gaugeTolerance);
                break;

            case PipelineStep.OpenInletValve:
                completed = IsValveNearTarget("valve_V1", def.targetValveAngle, def.angleTolerance);
                break;

            case PipelineStep.MonitorFlowMeter:
                completed = _flowMeterObserved && (Time.time - _flowMeterObservedTime > 2f);
                break;

            case PipelineStep.AdjustControlValve:
            {
                bool valveOk = IsValveNearTarget("valve_V2", def.targetValveAngle, def.angleTolerance);
                bool flowOk = IsGaugeNearTarget("flow_F1", 35f, 5f);
                completed = valveOk || flowOk; // 任一条件满足即完成（阀位或流量）
                break;
            }

            case PipelineStep.CheckMidPressure:
                completed = IsGaugeNearTarget("gauge_P2", def.targetGaugeValue, def.gaugeTolerance);
                break;

            case PipelineStep.OpenOutletValve:
                completed = IsValveNearTarget("valve_V3", def.targetValveAngle, def.angleTolerance);
                break;

            case PipelineStep.EmergencyStopTest:
                completed = IsButtonPressed(def.targetButtonId);
                break;

            case PipelineStep.SystemShutdown:
                completed = CheckShutdownComplete();
                break;
        }

        if (completed)
        {
            CompleteStep(step);
        }
    }

    void CompleteStep(PipelineStep step)
    {
        _stepCompleted[step] = true;
        OnStepCompleted?.Invoke(step);

        if (logger != null)
            logger.Record("step_" + (int)step, "complete", 1f);

        Debug.Log("[PipelineTraining] Step completed: " + GetStepDef(step).name);

        // 引导模式：自动推进到下一步
        if (mode == TrainingMode.Guide)
        {
            PipelineStep next = GetNextStep(step);
            if (next != PipelineStep.AllComplete)
                AdvanceToStep(next);
            else
                AdvanceToStep(PipelineStep.AllComplete);
        }
    }

    void AdvanceToStep(PipelineStep step)
    {
        _currentStep = step;
        OnStepChanged?.Invoke(step);
        Debug.Log("[PipelineTraining] Advanced to: " + (step == PipelineStep.AllComplete ? "ALL COMPLETE" : GetStepDef(step).name));
        if (step == PipelineStep.AllComplete)
            UploadPipelineReportIfNeeded();
    }

    void UploadPipelineReportIfNeeded()
    {
        if (_resultUploaded) return;
        _resultUploaded = true;

        int totalSteps = 0;
        int completedSteps = 0;
        var report = new TrainingReportPayload
        {
            taskId = "train2_pipeline",
            sceneName = SceneNameAliases.ToPublicSceneName(SceneManager.GetActiveScene().name),
            trainTime = Mathf.RoundToInt(Mathf.Max(0f, Time.time - _startedAtTime)),
            startedAt = DateTime.Now.AddSeconds(-Mathf.Max(0f, Time.time - _startedAtTime)).ToString("o"),
            finishedAt = DateTime.Now.ToString("o")
        };

        for (int i = 0; i < steps.Count; i++)
        {
            var def = steps[i];
            if (def.step == PipelineStep.NotStarted || def.step == PipelineStep.AllComplete)
                continue;

            bool completed = IsStepCompleted(def.step);
            totalSteps++;
            if (completed) completedSteps++;
            report.steps.Add(new TrainingStepReport
            {
                index = totalSteps - 1,
                name = def.name,
                expectedAction = def.instruction,
                completed = completed,
                mistakeCount = 0
            });
        }

        report.score = totalSteps > 0 ? Mathf.RoundToInt(completedSteps * 100f / totalSteps) : 100;
        Debug.Log("[PipelineTraining] Uploading training report: scene=" + report.sceneName + ", score=" + report.score + ", steps=" + completedSteps + "/" + totalSteps);
        TrainingBackendClient.EnsureExists().UploadReport(report);
    }

    PipelineStep GetNextStep(PipelineStep current)
    {
        if (current == PipelineStep.SystemShutdown)
            return PipelineStep.AllComplete;
        return current + 1;
    }

    // ── 条件检查辅助 ───────────────────────────────────────────

    bool IsButtonPressed(string buttonId)
    {
        if (string.IsNullOrEmpty(buttonId)) return false;
        return _buttonsPressed.TryGetValue(buttonId, out bool v) && v;
    }

    bool IsZoneVisited(string zoneId)
    {
        if (string.IsNullOrEmpty(zoneId)) return false;
        return _zonesVisited.TryGetValue(zoneId, out bool v) && v;
    }

    bool IsValveNearTarget(string valveId, float target, float tolerance)
    {
        if (!_valveAngles.TryGetValue(valveId, out float angle)) return false;
        return Mathf.Abs(angle) >= target - tolerance;
    }

    bool IsGaugeNearTarget(string gaugeId, float target, float tolerance)
    {
        if (!_gaugeValues.TryGetValue(gaugeId, out float val)) return false;
        return Mathf.Abs(val - target) <= tolerance;
    }

    bool CheckShutdownComplete()
    {
        // 条件：V1 已关闭（角度回 0）、V3 已关闭、P2 压力 < 0.1、提交按钮已按
        bool v1Closed = _valveAngles.TryGetValue("valve_V1", out float a1) && Mathf.Abs(a1) < 90f;
        bool v3Closed = _valveAngles.TryGetValue("valve_V3", out float a3) && Mathf.Abs(a3) < 90f;
        bool p2Low = _gaugeValues.TryGetValue("gauge_P2", out float p2) && p2 < 0.1f;
        bool submitted = IsButtonPressed("Shutdown_Confirm");

        return v1Closed && v3Closed && p2Low && submitted;
    }

    // ═══════════════════════════════════════════════════════════
    //  UI 更新
    // ═══════════════════════════════════════════════════════════

    void UpdateUI()
    {
        if (mode == TrainingMode.Guide)
        {
            if (_currentStep == PipelineStep.AllComplete)
            {
                SetText(titleText, "化工厂管道操作培训");
                SetText(stepText, "全部完成！");
                SetText(instructionText, "恭喜！您已完成所有 10 个培训步骤。\n系统已安全关停，操作记录已保存。");
                SetText(statusText, "成绩：合格");
            }
            else
            {
                StepDefinition def = GetStepDef(_currentStep);
                SetText(titleText, "化工厂管道操作培训");
                SetText(stepText, def.name);
                SetText(instructionText, def.instruction);
                SetText(statusText, GetStepProgressText());
            }
        }
        else // Practice
        {
            SetText(titleText, "化工厂管道操作 —— 自由练习");
            SetText(stepText, "自由练习模式");
            SetText(instructionText, "请按照标准操作流程依次完成各项操作。\n已完成步骤将显示绿色标记。");
            SetText(statusText, GetPracticeProgressText());
        }
    }

    string GetStepProgressText()
    {
        int completed = 0;
        for (PipelineStep s = PipelineStep.PPECheck; s <= PipelineStep.SystemShutdown; s++)
            if (IsStepCompleted(s)) completed++;
        return $"进度：{completed} / 10   |   当前步骤：{_currentStep}";
    }

    string GetPracticeProgressText()
    {
        int completed = 0;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("步骤完成状态：");
        for (PipelineStep s = PipelineStep.PPECheck; s <= PipelineStep.SystemShutdown; s++)
        {
            bool ok = IsStepCompleted(s);
            if (ok) completed++;
            StepDefinition def = GetStepDef(s);
            sb.AppendLine($"  {(ok ? "[√]" : "[ ]")} {def.name}");
        }
        sb.AppendLine($"总计：{completed} / 10");
        return sb.ToString();
    }

    void SetText(TextMesh tm, string text)
    {
        if (tm != null) tm.text = text;
    }
}
