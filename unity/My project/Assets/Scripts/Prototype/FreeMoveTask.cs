using UnityEngine;

/// <summary>
/// 平面自由操作调试场景:不做目标区、不做吸附,只验证手部映射、抓取、拖动和释放。
/// </summary>
public class FreeMoveTask : MonoBehaviour
{
    public GraspController grasp;
    public Vector2 areaMin = new Vector2(-2.7f, -1.35f);
    public Vector2 areaMax = new Vector2(2.7f, 1.25f);
    public float blockSize = 0.42f;
    public bool clampBlocksToArea = true;

    TextMesh _status;
    GameObject _cursor;
    Renderer _cursorRenderer;
    LineRenderer _line;
    PhysicMaterial _blockPhysic;

    void Start()
    {
        _blockPhysic = new PhysicMaterial("FreeMoveBlock")
        {
            dynamicFriction = 0.7f,
            staticFriction = 0.85f,
            bounciness = 0.02f,
        };

        BuildPracticeArea();
        BuildCursor();
        BuildStatus();
        SpawnBlocks();
    }

    void Update()
    {
        UpdateCursor();
        UpdateStatus();
        ClampBlocksToArea();
    }

    void BuildPracticeArea()
    {
        var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "FreeMovePlane";
        board.transform.position = new Vector3(0f, -1.55f, 0.08f);
        board.transform.localScale = new Vector3(5.9f, 0.08f, 0.75f);
        SetColor(board, new Color(0.22f, 0.25f, 0.29f));

        CreateBorder("LeftLimit", new Vector3(areaMin.x, -0.05f, 0.02f), new Vector3(0.035f, 2.7f, 0.035f));
        CreateBorder("RightLimit", new Vector3(areaMax.x, -0.05f, 0.02f), new Vector3(0.035f, 2.7f, 0.035f));
        CreateBorder("TopLimit", new Vector3(0f, areaMax.y, 0.02f), new Vector3(5.4f, 0.035f, 0.035f));
        CreateBorder("BottomLimit", new Vector3(0f, areaMin.y, 0.02f), new Vector3(5.4f, 0.035f, 0.035f));

        var titleGo = new GameObject("FreeMoveTitle");
        titleGo.transform.position = new Vector3(0f, 1.95f, -0.05f);
        var title = titleGo.AddComponent<TextMesh>();
        title.text = "Free grab practice: release to drop";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.fontSize = 58;
        title.characterSize = 0.052f;
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

    void SpawnBlocks()
    {
        CreateBlock("Block_A", new Vector3(-1.55f, -0.85f, 0f), new Color(1f, 0.45f, 0.12f));
        CreateBlock("Block_B", new Vector3(0f, -0.45f, 0f), new Color(0.25f, 0.62f, 1f));
        CreateBlock("Block_C", new Vector3(1.35f, -0.95f, 0f), new Color(0.35f, 0.85f, 0.38f));
    }

    void CreateBlock(string name, Vector3 position, Color color)
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
        var statusGo = new GameObject("FreeMoveStatus");
        statusGo.transform.position = new Vector3(-2.8f, 1.45f, -0.05f);
        _status = statusGo.AddComponent<TextMesh>();
        _status.anchor = TextAnchor.UpperLeft;
        _status.alignment = TextAlignment.Left;
        _status.fontSize = 42;
        _status.characterSize = 0.042f;
        _status.color = new Color(0.76f, 0.88f, 1f);
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
        string phase;
        if (!grasp.hand.IsActive) phase = "等待摄像头识别手";
        else if (grasp.Held != null) phase = "已抓住: 移动方块, 松开后自由落体";
        else if (grasp.Hover != null) phase = "靠近方块: 拇指和食指捏合抓取";
        else phase = "移动手部光标靠近任意方块";

        _status.text =
            "状态: " + phase +
            "\n抓取判定: " + grasp.GripSignal.ToString("0.00") + " / " + grasp.grabThreshold.ToString("0.00") +
            "\n捏合强度: " + grasp.hand.PinchOnlyStrength.ToString("0.00") +
            "\n握拳强度: " + grasp.hand.FistStrength.ToString("0.00") + (grasp.allowFistGrip ? " (参与)" : " (不参与)") +
            "\n自由落体: 松手后开启" +
            "\n手部速度: " + grasp.GripVelocity.magnitude.ToString("0.00") +
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
