using UnityEngine;

/// <summary>
/// 单任务训练:用虚拟手抓取工件,移动到目标槽位,松手后自动吸附落位。
/// 场景内提供源区、目标区、吸附范围、光标、连线和状态文字,方便课堂演示。
/// </summary>
public class PickPlaceTask : MonoBehaviour
{
    [Header("引用")]
    public GraspController grasp;

    [Header("布局 (XY 平面, z=0)")]
    public Vector3 sourcePos = new Vector3(-2f, -1.2f, 0f);
    public Vector3 targetPos = new Vector3(2f, -1.2f, 0f);
    public float blockSize = 0.5f;
    public float targetRadius = 0.6f;
    public float snapRadius = 1.2f;
    public float settleSpeed = 0.75f;
    public float holdToConfirm = 0.25f;

    PhysicMaterial _physMat;
    Grabbable _block;
    Renderer _targetRenderer;
    Renderer _ghostRenderer;
    GameObject _cursor;
    Renderer _cursorRenderer;
    LineRenderer _line;
    TextMesh _label;
    TextMesh _status;
    int _placed;
    float _confirmTimer;
    bool _success;

    readonly Color _sourceColor = new Color(0.36f, 0.40f, 0.47f);
    readonly Color _targetIdle = new Color(0.18f, 0.54f, 0.88f);
    readonly Color _targetHover = new Color(1f, 0.78f, 0.25f);
    readonly Color _targetOk = new Color(0.18f, 0.82f, 0.36f);

    void Start()
    {
        _physMat = new PhysicMaterial("TrainingPartSurface")
        {
            dynamicFriction = 0.65f,
            staticFriction = 0.75f,
            bounciness = 0.02f,
        };

        BuildPad(sourcePos, _sourceColor, "SOURCE");
        var targetPad = BuildPad(targetPos, _targetIdle, "TARGET");
        _targetRenderer = targetPad.GetComponent<Renderer>();
        BuildGhost();
        BuildSnapZone();
        BuildGuideLine();
        BuildCursor();
        BuildLabels();
        SpawnBlock();
    }

    void Update()
    {
        UpdateCursorAndLine();
        if (_block == null) return;

        if (_success)
        {
            _confirmTimer -= Time.deltaTime;
            if (_confirmTimer <= 0f) NextRound();
            return;
        }

        bool held = grasp != null && grasp.Held == _block;
        float distanceToTarget = Vector2.Distance(
            new Vector2(_block.transform.position.x, _block.transform.position.y),
            new Vector2(targetPos.x, targetPos.y));
        bool inZone = distanceToTarget < targetRadius;
        bool inMagnet = distanceToTarget < snapRadius;
        bool resting = _block.Body.velocity.magnitude < settleSpeed;

        if (_targetRenderer != null)
        {
            Color targetColor = inZone ? _targetOk : inMagnet || held ? _targetHover : _targetIdle;
            SetColor(_targetRenderer.gameObject, targetColor);
        }

        if (!held && inZone && resting)
        {
            _confirmTimer += Time.deltaTime;
            if (_confirmTimer >= holdToConfirm) Succeed();
        }
        else
        {
            _confirmTimer = 0f;
        }

        UpdateStatus(held, inMagnet, inZone);
    }

    GameObject BuildPad(Vector3 pos, Color color, string label)
    {
        var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad.name = label + "Pad";
        pad.transform.position = pos + new Vector3(0f, -blockSize * 0.55f, 0.03f);
        pad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        pad.transform.localScale = new Vector3(targetRadius * 1.6f, 0.035f, targetRadius * 1.6f);
        Destroy(pad.GetComponent<Collider>());
        SetColor(pad, color);

        var textGo = new GameObject(label + "Label");
        textGo.transform.position = pos + new Vector3(0f, -0.58f, -0.05f);
        var text = textGo.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 48;
        text.characterSize = 0.045f;
        text.color = new Color(0.78f, 0.84f, 0.92f);
        return pad;
    }

    void BuildGhost()
    {
        var ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ghost.name = "TargetGhostPart";
        ghost.transform.position = targetPos + new Vector3(0f, 0.05f, 0f);
        ghost.transform.localScale = Vector3.one * blockSize;
        Destroy(ghost.GetComponent<Collider>());
        _ghostRenderer = ghost.GetComponent<Renderer>();
        var mat = _ghostRenderer.material;
        mat.color = new Color(0.25f, 0.95f, 0.45f, 0.22f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", mat.color);
    }

    void BuildSnapZone()
    {
        var zoneGo = new GameObject("TargetSnapZone");
        zoneGo.transform.position = targetPos;
        var zone = zoneGo.AddComponent<SnapZone>();
        zone.radius = snapRadius;
        zone.magnetism = 0.9f;
        if (grasp != null) grasp.snapZones.Add(zone);

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "MagnetAssistRange";
        ring.transform.position = targetPos + new Vector3(0f, 0f, 0.06f);
        ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ring.transform.localScale = new Vector3(snapRadius * 1.0f, 0.01f, snapRadius * 1.0f);
        Destroy(ring.GetComponent<Collider>());
        SetColor(ring, new Color(0.13f, 0.34f, 0.46f));
    }

    void BuildGuideLine()
    {
        var guideGo = new GameObject("TransferPath");
        var line = guideGo.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, sourcePos + new Vector3(0f, -0.38f, -0.08f));
        line.SetPosition(1, targetPos + new Vector3(0f, -0.38f, -0.08f));
        line.startWidth = 0.035f;
        line.endWidth = 0.035f;
        line.material = MakeMaterial(new Color(0.55f, 0.62f, 0.70f));
    }

    void BuildCursor()
    {
        _cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursor.name = "GripCursor";
        Destroy(_cursor.GetComponent<Collider>());
        _cursorRenderer = _cursor.GetComponent<Renderer>();

        var lineGo = new GameObject("GripTargetLine");
        _line = lineGo.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.025f;
        _line.endWidth = 0.012f;
        _line.material = MakeMaterial(new Color(0.80f, 0.95f, 1f));
        _line.enabled = false;
    }

    void BuildLabels()
    {
        var labelGo = new GameObject("TaskTitle");
        labelGo.transform.position = new Vector3(0f, 2.08f, -0.05f);
        _label = labelGo.AddComponent<TextMesh>();
        _label.anchor = TextAnchor.MiddleCenter;
        _label.alignment = TextAlignment.Center;
        _label.fontSize = 62;
        _label.characterSize = 0.055f;
        _label.color = Color.white;

        var statusGo = new GameObject("TaskStatus");
        statusGo.transform.position = new Vector3(-2.85f, 1.45f, -0.05f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperLeft;
        _status.alignment = TextAlignment.Left;
        _status.fontSize = 42;
        _status.characterSize = 0.043f;
        _status.color = new Color(0.76f, 0.88f, 1f);
        UpdateLabel("抓取方块并移动到右侧目标区");
    }

    void SpawnBlock()
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = "TrainingPart";
        block.transform.position = sourcePos + new Vector3(0f, 0.55f, 0f);
        block.transform.localScale = Vector3.one * blockSize;
        block.GetComponent<Collider>().material = _physMat;

        var rb = block.AddComponent<Rigidbody>();
        rb.mass = 0.35f;
        rb.drag = 1.1f;
        rb.angularDrag = 1.8f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        _block = block.AddComponent<Grabbable>();
        _block.margin = 0.45f;
        _block.RefreshGrabRadius();
        _block.SetBaseColor(new Color(1f, 0.48f, 0.12f));

        if (grasp != null) grasp.grabbables.Add(_block);
        _success = false;
        _confirmTimer = 0f;
    }

    void UpdateCursorAndLine()
    {
        if (grasp == null || grasp.hand == null || _cursor == null) return;
        var hand = grasp.hand;
        bool active = hand.IsActive;
        _cursor.SetActive(active);
        if (!active)
        {
            if (_line != null) _line.enabled = false;
            return;
        }

        Vector3 grip = hand.GripPoint + new Vector3(0f, 0f, -0.12f);
        _cursor.transform.position = grip;
        float size = Mathf.Lerp(0.12f, 0.22f, hand.PinchStrength);
        _cursor.transform.localScale = Vector3.one * size;

        Color cursorColor = grasp.Held != null
            ? new Color(0.18f, 0.9f, 0.38f)
            : grasp.Hover != null ? new Color(1f, 0.82f, 0.18f) : new Color(0.22f, 0.70f, 1f);
        SetMaterialColor(_cursorRenderer, cursorColor);

        Vector3 target = grasp.Held != null
            ? targetPos + new Vector3(0f, 0f, -0.12f)
            : grasp.Hover != null ? grasp.Hover.transform.position + new Vector3(0f, 0f, -0.12f) : Vector3.zero;
        bool hasTarget = grasp.Held != null || grasp.Hover != null;
        _line.enabled = hasTarget;
        if (hasTarget)
        {
            _line.SetPosition(0, grip);
            _line.SetPosition(1, target);
        }
    }

    void UpdateStatus(bool held, bool inMagnet, bool inZone)
    {
        if (_status == null || grasp == null || grasp.hand == null) return;
        string phase;
        if (held) phase = inMagnet ? "正在磁吸辅助对准" : "正在搬运";
        else if (grasp.Hover != null) phase = "靠近方块, 捏合抓取";
        else if (inZone) phase = "已进入目标区, 松手确认";
        else phase = "移动手部光标到方块";

        _status.text =
            "状态: " + phase +
            "\n抓取强度: " + grasp.hand.PinchStrength.ToString("0.00") +
            "\n完成次数: " + _placed;
    }

    void Succeed()
    {
        _success = true;
        _placed++;
        _block.CanGrab = false;
        if (_ghostRenderer != null) SetMaterialColor(_ghostRenderer, new Color(0.24f, 1f, 0.42f, 0.35f));
        UpdateLabel("完成: 方块已放入目标区");
        _confirmTimer = 0.65f;
    }

    void NextRound()
    {
        if (_block != null)
        {
            if (grasp != null) grasp.grabbables.Remove(_block);
            Destroy(_block.gameObject);
            _block = null;
        }

        if (_targetRenderer != null) SetColor(_targetRenderer.gameObject, _targetIdle);
        if (_ghostRenderer != null) SetMaterialColor(_ghostRenderer, new Color(0.25f, 0.95f, 0.45f, 0.22f));
        UpdateLabel("继续训练: 抓取新方块并移动到目标区");
        SpawnBlock();
    }

    void UpdateLabel(string message)
    {
        if (_label == null) return;
        _label.text = message;
    }

    static void SetColor(GameObject go, Color color)
    {
        SetMaterialColor(go.GetComponent<Renderer>(), color);
    }

    static void SetMaterialColor(Renderer renderer, Color color)
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
