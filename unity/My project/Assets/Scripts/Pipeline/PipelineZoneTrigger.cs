using UnityEngine;

/// <summary>
/// 管道培训场景区域触发器。
/// 挂载到巡检点 GameObject 上，自动检测玩家进入并报告给 TrainingManager。
/// </summary>
public class PipelineZoneTrigger : MonoBehaviour
{
    [Tooltip("区域标识 ID，对应 TrainingManager 中的 targetZoneId")]
    public string zoneId = "InspectionZone";

    [Tooltip("触发后是否自动标记为已访问")]
    public bool autoComplete = true;

    [Tooltip("触发后的冷却时间（秒），防止重复触发")]
    public float cooldown = 3f;

    private float _lastTriggerTime = -999f;
    private PipelineTrainingManager _trainingManager;

    void Start()
    {
        _trainingManager = FindObjectOfType<PipelineTrainingManager>();
        if (_trainingManager == null)
        {
            // 尝试从生成的场景根查找
            Transform root = transform;
            while (root.parent != null) root = root.parent;
            _trainingManager = root.GetComponentInChildren<PipelineTrainingManager>();
        }

        // 确保有触发器碰撞体
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.8f, 1.6f, 0.8f);
            col.center = new Vector3(0f, 0.8f, 0f);
        }
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!autoComplete) return;
        if (Time.time - _lastTriggerTime < cooldown) return;

        // 检查是否是玩家
        if (IsPlayer(other))
        {
            _lastTriggerTime = Time.time;
            _trainingManager?.ReportZoneVisit(zoneId);
            Debug.Log("[PipelineZoneTrigger] 玩家进入区域: " + zoneId);
            OnPlayerEntered();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!autoComplete) return;

        // 持续检测（适用于玩家已在区域内且按下交互键的场景）
        if (IsPlayer(other) && Input.GetKeyDown(KeyCode.F))
        {
            if (Time.time - _lastTriggerTime < cooldown) return;
            _lastTriggerTime = Time.time;
            _trainingManager?.ReportZoneVisit(zoneId);
            OnPlayerEntered();
        }
    }

    bool IsPlayer(Collider other)
    {
        // 通过标签、名称或 CharacterController 判断
        if (other.CompareTag("Player")) return true;
        if (other.name.Contains("Player")) return true;
        if (other.GetComponent<CharacterController>() != null) return true;
        // 也检查父级
        if (other.transform.parent != null && other.transform.parent.name.Contains("Player")) return true;
        return false;
    }

    void OnPlayerEntered()
    {
        // 视觉反馈：改变灯泡颜色
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            // 找到灯泡（通常是球体，带有 emission 材质）
            if (r.gameObject.name.Contains("Bulb") || r.gameObject.name.Contains("Lamp"))
            {
                Material mat = r.sharedMaterial;
                if (mat != null && mat.HasProperty("_BaseColor"))
                {
                    // 变为绿色表示已访问
                    mat.SetColor("_BaseColor", new Color(0.2f, 0.9f, 0.3f));
                }
            }
        }
    }
}
