using UnityEngine;

/// <summary>
/// 训练站视觉样式
/// </summary>
public enum StationStyle
{
    Pedestal,          // 默认：立柱展示台
    TableWithBlocks    // 小桌子 + 彩色物块（自由抓取站）
}

/// <summary>
/// HubRoom 中的训练站：程序化搭建视觉 + 接近检测 + 按E加载对应训练场景。
/// 挂在大房间中的空 GameObject 上，在 Inspector 中配置 taskId/taskName/sceneName。
/// </summary>
public class TaskStation : MonoBehaviour
{
    [Header("配置")]
    public string taskId   = "FreeMove";       // 唯一标识，如 "Sort", "Weld"
    public string taskName = "Free Grab Practice";
    public string sceneName = "FreeMoveScene";  // Build Settings 中注册的场景名

    [Header("触发")]
    public float triggerRadius = 2.2f;          // 玩家在此距离内触发提示

    [Header("外观")]
    public StationStyle style = StationStyle.Pedestal;
    public Color idleColor   = new Color(0.25f, 0.35f, 0.50f);   // 底座颜色
    public Color promptColor = Color.white;

    // ── 内部引用 ──
    TextMesh _label;         // 任务名
    TextMesh _prompt;        // "按 E 进入…"
    Renderer _markerRenderer; // 完成标记球
    GameObject _pedestalRoot; // 底座根节点
    bool _playerNear;

    readonly Color _doneColor  = new Color(0.18f, 0.88f, 0.36f);  // 已完成绿色
    readonly Color _undoneColor = new Color(0.25f, 0.28f, 0.32f);  // 未完成灰色

    void Start()
    {
        BuildVisual();
        if (_prompt != null) _prompt.gameObject.SetActive(false);
    }

    void Update()
    {
        // 距离检测：用 Tag="Player" 查找玩家
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.transform.position.x, 0, player.transform.position.z));

        bool near = dist < triggerRadius;
        if (near != _playerNear)
        {
            _playerNear = near;
            if (_prompt != null) _prompt.gameObject.SetActive(_playerNear);
        }

        if (_playerNear && Input.GetKeyDown(KeyCode.E))
            GameManager.Instance?.LoadTaskScene(sceneName);
    }

    /// <summary>
    /// 由 GameManager 在 HubRoom 加载时调用，刷新完成标记颜色。
    /// </summary>
    public void RefreshVisual()
    {
        bool done = GameManager.Instance != null && GameManager.Instance.IsTaskCompleted(taskId);
        SetMarkerColor(done ? _doneColor : _undoneColor);
    }

    // ═══════════════════ 程序化搭建视觉 ═══════════════════

    void BuildVisual()
    {
        _pedestalRoot = new GameObject("StationVisual");
        _pedestalRoot.transform.SetParent(transform, false);
        _pedestalRoot.transform.localPosition = Vector3.zero;

        switch (style)
        {
            case StationStyle.TableWithBlocks:
                BuildTableWithBlocks();
                break;
            default:
                BuildPedestal();
                break;
        }

        // 完成标记球（公共）
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "Checkmark";
        marker.transform.SetParent(_pedestalRoot.transform, false);
        marker.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        marker.transform.localScale = Vector3.one * 0.18f;
        Destroy(marker.GetComponent<Collider>());
        _markerRenderer = marker.GetComponent<Renderer>();
        SetOne(_markerRenderer, _undoneColor);

        // 任务名 TextMesh（公共）
        var labelGo = new GameObject("TaskLabel");
        labelGo.transform.SetParent(_pedestalRoot.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.12f, 0f);
        _label = labelGo.AddComponent<TextMesh>();
        _label.text = taskName;
        _label.anchor = TextAnchor.MiddleCenter;
        _label.alignment = TextAlignment.Center;
        _label.fontSize = 44;
        _label.characterSize = 0.045f;
        _label.color = new Color(0.82f, 0.88f, 1f);

        // 提示 TextMesh（公共，默认隐藏）
        var promptGo = new GameObject("EnterPrompt");
        promptGo.transform.SetParent(_pedestalRoot.transform, false);
        promptGo.transform.localPosition = new Vector3(0f, 1.60f, 0f);
        _prompt = promptGo.AddComponent<TextMesh>();
        _prompt.text = "按 E 进入\n" + taskName;
        _prompt.anchor = TextAnchor.MiddleCenter;
        _prompt.alignment = TextAlignment.Center;
        _prompt.fontSize = 38;
        _prompt.characterSize = 0.042f;
        _prompt.color = promptColor;
    }

    void BuildPedestal()
    {
        // 底座圆柱
        var baseCyl = CreatePart("BaseCylinder", PrimitiveType.Cylinder,
            new Vector3(0f, -0.25f, 0f), new Vector3(0.8f, 0.15f, 0.8f), idleColor);

        // 立杆
        CreatePart("Pole", PrimitiveType.Cylinder,
            new Vector3(0f, 0.25f, 0f), new Vector3(0.12f, 0.7f, 0.12f), idleColor * 1.2f);

        // 顶部平板
        CreatePart("TopPanel", PrimitiveType.Cube,
            new Vector3(0f, 0.65f, 0f), new Vector3(0.7f, 0.06f, 0.45f), new Color(0.18f, 0.22f, 0.28f));
    }

    void BuildTableWithBlocks()
    {
        float tw = 1.0f;  // 桌面半宽
        float td = 0.55f; // 桌面半深
        float th = 0.04f; // 桌面厚度

        // ── 桌面 ──
        var tabletop = CreatePart("Tabletop", PrimitiveType.Cube,
            new Vector3(0f, 0.40f, 0f), new Vector3(tw * 2f, th * 2f, td * 2f),
            new Color(0.28f, 0.22f, 0.16f));

        // ── 4条桌腿 ──
        float legH = 0.38f;
        float legR = 0.04f;
        Vector3[] legPositions = {
            new Vector3( tw - 0.08f, legH / 2f,  td - 0.08f),
            new Vector3(-tw + 0.08f, legH / 2f,  td - 0.08f),
            new Vector3( tw - 0.08f, legH / 2f, -td + 0.08f),
            new Vector3(-tw + 0.08f, legH / 2f, -td + 0.08f),
        };
        foreach (var pos in legPositions)
        {
            var leg = CreatePart("Leg", PrimitiveType.Cylinder, pos,
                new Vector3(legR * 2f, legH * 0.5f, legR * 2f),
                new Color(0.20f, 0.16f, 0.10f));
            leg.transform.localRotation = Quaternion.identity;
        }

        // ── 桌上的物块 ──
        float blockS = 0.12f;
        float blockY = 0.40f + th + blockS / 2f;

        CreateBlock(new Vector3(-0.18f, blockY, 0.12f),  blockS, new Color(1f, 0.45f, 0.12f)); // 橙
        CreateBlock(new Vector3( 0.05f, blockY, -0.10f), blockS, new Color(0.25f, 0.62f, 1f));  // 蓝
        CreateBlock(new Vector3( 0.22f, blockY, 0.18f),  blockS, new Color(0.35f, 0.85f, 0.38f)); // 绿
    }

    GameObject CreatePart(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(_pedestalRoot.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        Destroy(go.GetComponent<Collider>());
        SetOne(go.GetComponent<Renderer>(), color);
        return go;
    }

    void CreateBlock(Vector3 pos, float size, Color color)
    {
        var block = CreatePart("Block", PrimitiveType.Cube, pos,
            Vector3.one * size, color);
        // 微旋让物块看起来随意摆放
        block.transform.localRotation = Quaternion.Euler(0f, Random.Range(-15f, 15f), 0f);
    }

    void SetMarkerColor(Color c)
    {
        if (_markerRenderer == null) return;
        _markerRenderer.material.color = c;
        if (_markerRenderer.material.HasProperty("_BaseColor"))
            _markerRenderer.material.SetColor("_BaseColor", c);
    }

    static void SetOne(Renderer r, Color c)
    {
        if (r == null) return;
        r.material.color = c;
        if (r.material.HasProperty("_BaseColor"))
            r.material.SetColor("_BaseColor", c);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
