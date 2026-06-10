using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 工位训练调试场景:保留自由抓取,增加颜色投放、按钮确认、阀门旋转三个操作。
/// </summary>
public class FreeMoveTask : MonoBehaviour
{
    class BlockGoal
    {
        public string label;
        public Grabbable block;
        public Vector3 zoneCenter;
        public Renderer zoneRenderer;
        public Color color;
        public bool placed;
    }

    public GraspController grasp;
    public Vector2 areaMin = new Vector2(-2.7f, -1.35f);
    public Vector2 areaMax = new Vector2(2.7f, 1.25f);
    public float blockSize = 0.42f;
    public bool clampBlocksToArea = true;

    readonly List<BlockGoal> _goals = new List<BlockGoal>();
    TextMesh _status;
    GameObject _cursor, _button, _valveRoot;
    Renderer _cursorRenderer, _buttonRenderer, _valveRenderer;
    LineRenderer _line;
    PhysicMaterial _blockPhysic;
    Grabbable _lastHeld;

    float _startTime;
    float _valveAngle;
    float _lastPalmAngle;
    float _lastButtonPressTime = -99f;
    int _dropCount;
    bool _buttonPressed;
    bool _rotatingValve;
    bool _valveComplete;
    bool _trainingComplete;

    const float ZoneRadius = 0.42f;
    const float ButtonRadius = 0.42f;
    const float ButtonCooldown = 0.45f;
    const float ValveRadius = 0.55f;
    const float TargetValveAngle = 90f;
    const float ValveTolerance = 12f;

    void Start()
    {
        _startTime = Time.time;
        _blockPhysic = new PhysicMaterial("TrainingBlock")
        {
            dynamicFriction = 0.7f,
            staticFriction = 0.85f,
            bounciness = 0.02f,
        };

        BuildPracticeArea();
        BuildOperationDevices();
        BuildCursor();
        BuildStatus();
        SpawnBlocksAndZones();
    }

    void Update()
    {
        UpdateReleaseAccounting();
        UpdatePlacement();
        UpdateButton();
        UpdateValve();
        UpdateCursor();
        UpdateStatus();
        ClampBlocksToArea();
    }

    void BuildPracticeArea()
    {
        var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "TrainingOperationPlane";
        board.transform.position = new Vector3(0f, -1.55f, 0.08f);
        board.transform.localScale = new Vector3(5.9f, 0.08f, 0.75f);
        SetColor(board, new Color(0.22f, 0.25f, 0.29f));

        CreateBorder("LeftLimit", new Vector3(areaMin.x, -0.05f, 0.02f), new Vector3(0.035f, 2.7f, 0.035f));
        CreateBorder("RightLimit", new Vector3(areaMax.x, -0.05f, 0.02f), new Vector3(0.035f, 2.7f, 0.035f));
        CreateBorder("TopLimit", new Vector3(0f, areaMax.y, 0.02f), new Vector3(5.4f, 0.035f, 0.035f));
        CreateBorder("BottomLimit", new Vector3(0f, areaMin.y, 0.02f), new Vector3(5.4f, 0.035f, 0.035f));

        var sourceShelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sourceShelf.name = "SourceShelf";
        sourceShelf.transform.position = new Vector3(0f, 0.42f, 0.08f);
        sourceShelf.transform.localScale = new Vector3(4.7f, 0.05f, 0.08f);
        SetColor(sourceShelf, new Color(0.33f, 0.36f, 0.41f));

        var titleGo = new GameObject("TrainingTitle");
        titleGo.transform.position = new Vector3(0f, 1.95f, -0.05f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "Training: sort blocks, press button, rotate valve";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 56;
        title.characterSize = 0.05f;
        title.color = Color.white;
    }

    void CreateBorder(string name, Vector3 position, Vector3 scale)
    {
        var border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = name;
        border.transform.position = position;
        border.transform.localScale = scale;
        SetColor(border, new Color(0.45f, 0.52f, 0.60f));
    }

    void BuildOperationDevices()
    {
        BuildButton(new Vector3(2.25f, 0.83f, 0f));
        BuildValve(new Vector3(-2.25f, 0.83f, 0f));
    }

    void BuildButton(Vector3 center)
    {
        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.name = "ConfirmButtonBase";
        baseGo.transform.position = center + new Vector3(0f, -0.18f, 0.06f);
        baseGo.transform.localScale = new Vector3(0.78f, 0.13f, 0.10f);
        SetColor(baseGo, new Color(0.18f, 0.22f, 0.28f));

        _button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _button.name = "ConfirmButton";
        _button.transform.position = center + new Vector3(0f, 0f, -0.02f);
        _button.transform.localScale = new Vector3(0.52f, 0.24f, 0.10f);
        _buttonRenderer = _button.GetComponent<Renderer>();
        SetRendererColor(_buttonRenderer, new Color(0.12f, 0.44f, 0.92f));

        var labelGo = new GameObject("ButtonLabel");
        labelGo.transform.position = center + new Vector3(0f, 0.28f, -0.05f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = "CONFIRM";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 34;
        label.characterSize = 0.035f;
        label.color = new Color(0.82f, 0.90f, 1f);
    }

    void BuildValve(Vector3 center)
    {
        _valveRoot = new GameObject("SafetyValve");
        _valveRoot.transform.position = center;

        var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "ValveWheel";
        wheel.transform.parent = _valveRoot.transform;
        wheel.transform.localPosition = Vector3.zero;
        wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        wheel.transform.localScale = new Vector3(0.46f, 0.035f, 0.46f);
        _valveRenderer = wheel.GetComponent<Renderer>();
        SetRendererColor(_valveRenderer, new Color(0.96f, 0.52f, 0.16f));

        var valveNeedle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        valveNeedle.name = "ValvePointer";
        valveNeedle.transform.parent = _valveRoot.transform;
        valveNeedle.transform.localPosition = new Vector3(0.18f, 0f, -0.08f);
        valveNeedle.transform.localScale = new Vector3(0.42f, 0.055f, 0.08f);
        SetColor(valveNeedle, new Color(1f, 0.9f, 0.2f));

        var targetGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        targetGo.name = "ValveTargetMark";
        targetGo.transform.position = center + new Vector3(0f, 0.46f, -0.08f);
        targetGo.transform.localScale = new Vector3(0.10f, 0.22f, 0.06f);
        SetColor(targetGo, new Color(0.25f, 1f, 0.45f));

        var ringGo = new GameObject("ValveRangeRing");
        var valveRing = ringGo.AddComponent<LineRenderer>();
        valveRing.positionCount = 65;
        valveRing.startWidth = 0.018f;
        valveRing.endWidth = 0.018f;
        valveRing.material = MakeMaterial(new Color(1f, 0.82f, 0.35f));
        for (int i = 0; i < valveRing.positionCount; i++)
        {
            float a = i / 64f * Mathf.PI * 2f;
            valveRing.SetPosition(i, center + new Vector3(Mathf.Cos(a) * ValveRadius, Mathf.Sin(a) * ValveRadius, -0.09f));
        }

        var labelGo = new GameObject("ValveLabel");
        labelGo.transform.position = center + new Vector3(0f, 0.72f, -0.05f);
        var label = labelGo.AddComponent<TextMesh>();
        label.text = "VALVE 90";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 34;
        label.characterSize = 0.035f;
        label.color = new Color(1f, 0.86f, 0.54f);
    }

    void SpawnBlocksAndZones()
    {
        AddBlockGoal("A", new Vector3(-1.55f, 0.72f, 0f), new Vector3(-1.55f, -1.12f, 0f), new Color(1f, 0.45f, 0.12f));
        AddBlockGoal("B", new Vector3(0f, 0.72f, 0f), new Vector3(0f, -1.12f, 0f), new Color(0.25f, 0.62f, 1f));
        AddBlockGoal("C", new Vector3(1.55f, 0.72f, 0f), new Vector3(1.55f, -1.12f, 0f), new Color(0.35f, 0.85f, 0.38f));
    }

    void AddBlockGoal(string label, Vector3 blockPosition, Vector3 zoneCenter, Color color)
    {
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = "DropZone_" + label;
        zone.transform.position = zoneCenter + new Vector3(0f, -0.02f, 0.12f);
        zone.transform.localScale = new Vector3(0.92f, 0.10f, 0.05f);
        var zoneCol = zone.GetComponent<Collider>();
        if (zoneCol != null) Destroy(zoneCol);
        var zoneRenderer = zone.GetComponent<Renderer>();
        SetRendererColor(zoneRenderer, Color.Lerp(color, Color.black, 0.20f));

        var labelGo = new GameObject("DropZoneLabel_" + label);
        labelGo.transform.position = zoneCenter + new Vector3(0f, 0.15f, -0.05f);
        var text = labelGo.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 44;
        text.characterSize = 0.042f;
        text.color = Color.white;

        var block = CreateBlock("Block_" + label, blockPosition, color);
        _goals.Add(new BlockGoal
        {
            label = label,
            block = block,
            zoneCenter = zoneCenter,
            zoneRenderer = zoneRenderer,
            color = color,
        });
    }

    Grabbable CreateBlock(string name, Vector3 position, Color color)
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.position = position;
        block.transform.localScale = Vector3.one * blockSize;
        var col = block.GetComponent<Collider>();
        if (col != null) col.material = _blockPhysic;

        var rb = block.AddComponent<Rigidbody>();
        rb.mass = 0.32f;
        rb.drag = 1.4f;
        rb.angularDrag = 2.2f;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var grabbable = block.AddComponent<Grabbable>();
        grabbable.margin = 0.62f;
        grabbable.RefreshGrabRadius();
        grabbable.SetBaseColor(color);

        if (grasp != null) grasp.grabbables.Add(grabbable);
        return grabbable;
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "GripCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("GripHoverLine");
        _line = lineGo.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.025f;
        _line.endWidth = 0.012f;
        _line.material = MakeMaterial(new Color(0.82f, 0.95f, 1f));
        _line.enabled = false;
    }

    void BuildStatus()
    {
        var statusGo = new GameObject("TrainingStatus");
        statusGo.transform.position = new Vector3(-2.8f, 1.45f, -0.05f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperLeft;
        _status.alignment = TextAlignment.Left;
        _status.fontSize = 38;
        _status.characterSize = 0.038f;
        _status.color = new Color(0.76f, 0.88f, 1f);
    }

    void UpdateReleaseAccounting()
    {
        if (grasp == null) return;
        if (_lastHeld != null && grasp.Held == null)
        {
            var goal = FindGoal(_lastHeld);
            if (goal != null && !goal.placed && !IsInsideGoal(goal))
            {
                _dropCount++;
            }
        }

        _lastHeld = grasp.Held;
    }

    void UpdatePlacement()
    {
        foreach (var goal in _goals)
        {
            if (goal.block == null || goal.placed) continue;
            if (grasp != null && grasp.Held == goal.block) continue;
            if (!IsInsideGoal(goal)) continue;
            if (goal.block.Body.velocity.magnitude > 0.35f) continue;

            goal.placed = true;
            goal.block.Body.useGravity = false;
            goal.block.Body.velocity = Vector3.zero;
            goal.block.Body.angularVelocity = Vector3.zero;
            goal.block.Body.position = goal.zoneCenter + new Vector3(0f, blockSize * 0.55f, 0f);
            goal.block.transform.rotation = Quaternion.identity;
            goal.block.SetHighlight(false);
            SetRendererColor(goal.zoneRenderer, Color.Lerp(goal.color, Color.white, 0.25f));
        }
    }

    void UpdateButton()
    {
        if (grasp == null || grasp.hand == null || _button == null) return;

        bool ready = PlacedCount() >= _goals.Count;
        bool near = grasp.hand.IsActive && Vector3.Distance(grasp.hand.GripPoint, _button.transform.position) <= ButtonRadius;
        bool click = near && grasp.Held == null && grasp.GripSignal >= grasp.grabThreshold && Time.time - _lastButtonPressTime >= ButtonCooldown;

        if (click)
        {
            _lastButtonPressTime = Time.time;
            if (ready) _buttonPressed = true;
        }

        Color color;
        if (_buttonPressed) color = new Color(0.12f, 0.82f, 0.34f);
        else if (!ready) color = new Color(0.24f, 0.30f, 0.38f);
        else if (near) color = new Color(0.98f, 0.78f, 0.16f);
        else color = new Color(0.12f, 0.44f, 0.92f);
        SetRendererColor(_buttonRenderer, color);

        float pressScale = _buttonPressed ? 0.08f : near ? 0.14f : 0.24f;
        _button.transform.localScale = new Vector3(0.52f, pressScale, 0.10f);
    }

    void UpdateValve()
    {
        if (grasp == null || grasp.hand == null || _valveRoot == null) return;

        bool ready = _buttonPressed;
        bool near = grasp.hand.IsActive && Vector3.Distance(grasp.hand.GripPoint, _valveRoot.transform.position) <= ValveRadius;
        bool active = ready && near && grasp.Held == null && grasp.GripSignal >= grasp.grabThreshold;

        if (active)
        {
            if (!_rotatingValve)
            {
                _rotatingValve = true;
                _lastPalmAngle = grasp.hand.PalmAngle;
            }
            else
            {
                float delta = Mathf.DeltaAngle(_lastPalmAngle, grasp.hand.PalmAngle);
                _valveAngle = Mathf.Clamp(_valveAngle + delta, 0f, 120f);
                _lastPalmAngle = grasp.hand.PalmAngle;
            }
        }
        else
        {
            _rotatingValve = false;
        }

        if (Mathf.Abs(_valveAngle - TargetValveAngle) <= ValveTolerance && ready)
            _valveComplete = true;

        _valveRoot.transform.rotation = Quaternion.Euler(0f, 0f, _valveAngle);

        Color color;
        if (_valveComplete) color = new Color(0.20f, 0.88f, 0.38f);
        else if (!ready) color = new Color(0.45f, 0.38f, 0.32f);
        else if (near) color = new Color(1f, 0.76f, 0.18f);
        else color = new Color(0.96f, 0.52f, 0.16f);
        SetRendererColor(_valveRenderer, color);

        if (!_trainingComplete && _valveComplete) _trainingComplete = true;
    }

    void UpdateCursor()
    {
        if (grasp == null || grasp.hand == null || _cursor == null) return;
        var hand = grasp.hand;
        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            _line.enabled = false;
            return;
        }

        float gripSignal = grasp.GripSignal;
        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.16f);
        _cursor.transform.position = grip;
        _cursor.transform.localScale = Vector3.one * Mathf.Lerp(0.13f, 0.23f, gripSignal);

        Color color = grasp.Held != null
            ? new Color(0.16f, 0.88f, 0.35f)
            : grasp.Hover != null ? new Color(1f, 0.82f, 0.18f) : new Color(0.18f, 0.68f, 1f);
        SetRendererColor(_cursorRenderer, color);

        bool showLine = grasp.Held != null || grasp.Hover != null;
        _line.enabled = showLine;
        if (!showLine) return;

        Vector3 target = (grasp.Held != null ? grasp.Held.transform.position : grasp.Hover.transform.position)
            + new Vector3(0f, 0f, -0.16f);
        _line.SetPosition(0, grip);
        _line.SetPosition(1, target);
    }

    void UpdateStatus()
    {
        if (_status == null || grasp == null || grasp.hand == null) return;

        int placed = PlacedCount();
        string phase;
        if (!grasp.hand.IsActive) phase = "等待摄像头识别手";
        else if (placed < _goals.Count) phase = "步骤1: 抓取方块并投放到同色目标区";
        else if (!_buttonPressed) phase = "步骤2: 在 CONFIRM 按钮上捏合确认";
        else if (!_valveComplete) phase = "步骤3: 捏合阀门并旋转到绿色 90 度标记";
        else phase = "训练完成";

        float elapsed = Time.time - _startTime;
        int score = Mathf.Clamp(100 - _dropCount * 5 - Mathf.FloorToInt(elapsed / 20f), 0, 100);

        _status.text =
            "状态: " + phase +
            "\n投放进度: " + placed + " / " + _goals.Count +
            "\n按钮确认: " + (_buttonPressed ? "已完成" : "未完成") +
            "\n阀门角度: " + _valveAngle.ToString("0") + " / " + TargetValveAngle.ToString("0") +
            "\n掉落/误放次数: " + _dropCount +
            "\n预计得分: " + score +
            "\n抓取判定: " + grasp.GripSignal.ToString("0.00") + " / " + grasp.grabThreshold.ToString("0.00") +
            "\n当前对象: " + (grasp.Held != null ? grasp.Held.name : grasp.Hover != null ? grasp.Hover.name : "-");
    }

    void ClampBlocksToArea()
    {
        if (!clampBlocksToArea || grasp == null) return;
        float half = blockSize * 0.5f;
        foreach (var g in grasp.grabbables)
        {
            if (g == null || g.Body == null) continue;
            Vector3 p = g.Body.position;
            Vector3 clamped = new Vector3(
                Mathf.Clamp(p.x, areaMin.x + half, areaMax.x - half),
                Mathf.Clamp(p.y, areaMin.y + half, areaMax.y - half),
                0f);

            if ((clamped - p).sqrMagnitude < 0.0001f) continue;
            g.Body.position = clamped;
            g.Body.velocity = Vector3.zero;
            g.Body.angularVelocity = Vector3.zero;
        }
    }

    bool IsInsideGoal(BlockGoal goal)
    {
        Vector2 block = new Vector2(goal.block.Body.position.x, goal.block.Body.position.y);
        Vector2 zone = new Vector2(goal.zoneCenter.x, goal.zoneCenter.y);
        return Vector2.Distance(block, zone) <= ZoneRadius;
    }

    int PlacedCount()
    {
        int count = 0;
        foreach (var goal in _goals)
            if (goal.placed) count++;
        return count;
    }

    BlockGoal FindGoal(Grabbable block)
    {
        foreach (var goal in _goals)
            if (goal.block == block) return goal;
        return null;
    }

    static void SetColor(GameObject go, Color color)
    {
        SetRendererColor(go.GetComponent<Renderer>(), color);
    }

    static void SetRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        var mat = renderer.material;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        return material;
    }
}
