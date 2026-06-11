using UnityEngine;

/// <summary>
/// 装配层:搭场景 + 创建手/抓取/任务并接线。本版默认开启较强辅助(增益 + 吸附),便于顺畅操作。
/// 挂到一个空 GameObject 上,点 Play(确保 Python 服务在跑)。
/// </summary>
public class DemoBootstrap : MonoBehaviour
{
    void Start()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, -0.2f, -6f);
            cam.transform.rotation = Quaternion.Euler(4f, 0f, 0f);
            cam.fieldOfView = 50f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.12f, 0.16f);
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.44f, 0.48f);
        var fillGo = new GameObject("FillLight");
        var fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.6f;
        fill.color = new Color(0.8f, 0.85f, 1f);
        fillGo.transform.rotation = Quaternion.Euler(50f, -130f, 0f);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0f, -1.6f, 0f);
        floor.transform.localScale = Vector3.one * 3f;
        floor.GetComponent<Collider>().material =
            new PhysicMaterial("Floor") { dynamicFriction = 0.7f, staticFriction = 0.8f, bounciness = 0.02f };
        SetColor(floor, new Color(0.22f, 0.24f, 0.28f));

        // 手:数据层(开启位移增益)+ 表现层
        var handGo = new GameObject("Hand");
        var hand = handGo.AddComponent<HandInput>();
        hand.planeWidth = 5.4f;
        hand.planeHeight = 3.6f;
        hand.gain = 1.4f;              // 小动作覆盖全场
        var visual = handGo.AddComponent<HandVisual>();

        // 交互层:放宽抓取 + 开启吸附
        var grasp = handGo.AddComponent<GraspController>();
        grasp.hand = hand;
        grasp.handVisual = visual;
        grasp.grabThreshold = 0.6f;
        grasp.releaseThreshold = 0.4f;
        grasp.carryMagnetism = 1.0f;

        // 任务层
        var task = handGo.AddComponent<PickPlaceTask>();
        task.grasp = grasp;

        Debug.Log("[DemoBootstrap] 已开启辅助(增益+吸附)。捏合抓左侧方块,移到右侧绿区松手会自动落位。");
    }

    static void SetColor(GameObject go, Color c)
    {
        var m = go.GetComponent<Renderer>().material;
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
    }
}
