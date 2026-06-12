using UnityEngine;

/// <summary>
/// HubRoom 场景搭建：程序化构建大房间几何体（地板、墙壁、天花板、灯光）。
/// 风格与现有 Bootstrap 一致（SceneBootstrap / DemoBootstrap）。
/// 挂在 HubRoom 场景的 GameObject 上。
/// </summary>
public class HubBootstrap : MonoBehaviour
{
    [Header("房间尺寸")]
    public float roomWidth  = 20f;   // X 方向
    public float roomDepth  = 14f;   // Z 方向
    public float roomHeight = 4.5f;
    public float wallThickness = 0.3f;

    [Header("灯光")]
    public float ambientIntensity = 0.38f;
    public float mainLightIntensity = 0.75f;

    void Start()
    {
        SetupLighting();
        BuildFloor();
        BuildWalls();
        BuildCeiling();

        Debug.Log("[HubBootstrap] 大房间已搭建完成: " +
                  roomWidth + "×" + roomDepth + "×" + roomHeight + "m");
    }

    void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(ambientIntensity, ambientIntensity, ambientIntensity + 0.04f);

        // 主方向光（模拟天花板顶灯）
        var keyGo = new GameObject("HubMainLight");
        var key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = mainLightIntensity;
        key.color = new Color(1f, 0.96f, 0.88f);
        keyGo.transform.rotation = Quaternion.Euler(55f, -40f, 0f);

        // 微弱补光（减少阴影太硬）
        var fillGo = new GameObject("HubFillLight");
        var fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.25f;
        fill.color = new Color(0.65f, 0.72f, 0.9f);
        fillGo.transform.rotation = Quaternion.Euler(-15f, 160f, 0f);
    }

    void BuildFloor()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0f, -0.15f, 0f);
        floor.transform.localScale = new Vector3(roomWidth, 0.3f, roomDepth);
        // 保留 Collider — CharacterController 需要站立和行走的物理地面
        SetColor(floor, new Color(0.22f, 0.24f, 0.28f));

        // 地板纹理线（装饰性线条）
        for (float x = -roomWidth / 2f + 2f; x <= roomWidth / 2f; x += 3f)
        {
            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "FloorLine_X";
            line.transform.position = new Vector3(x, 0.005f, 0f);
            line.transform.localScale = new Vector3(0.02f, 0.005f, roomDepth - 1f);
            Destroy(line.GetComponent<Collider>());
            SetColor(line, new Color(0.18f, 0.20f, 0.24f));
        }
    }

    void BuildWalls()
    {
        float hw = roomWidth / 2f;
        float hd = roomDepth / 2f;
        float hh = roomHeight / 2f;
        float wt = wallThickness;

        // 后墙 (Z+)
        CreateWall("BackWall",
            new Vector3(0f, hh, hd),
            new Vector3(roomWidth + wt * 2f, roomHeight, wt));

        // 前墙 (Z-)
        CreateWall("FrontWall",
            new Vector3(0f, hh, -hd),
            new Vector3(roomWidth + wt * 2f, roomHeight, wt));

        // 左墙 (X-)
        CreateWall("LeftWall",
            new Vector3(-hw, hh, 0f),
            new Vector3(wt, roomHeight, roomDepth));

        // 右墙 (X+)
        CreateWall("RightWall",
            new Vector3(hw, hh, 0f),
            new Vector3(wt, roomHeight, roomDepth));
    }

    void CreateWall(string name, Vector3 pos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        // 保留 Collider — CharacterController 不能穿墙
        SetColor(wall, new Color(0.32f, 0.34f, 0.38f));
    }

    void BuildCeiling()
    {
        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.position = new Vector3(0f, roomHeight, 0f);
        ceiling.transform.localScale = new Vector3(roomWidth, 0.15f, roomDepth);
        // 保留 Collider — 防止跳过头
        SetColor(ceiling, new Color(0.38f, 0.40f, 0.44f));

        // 天花板灯带（装饰条）
        for (float x = -roomWidth / 2f + 2.5f; x <= roomWidth / 2f; x += 3.5f)
        {
            var lightStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lightStrip.name = "CeilingLightStrip";
            lightStrip.transform.position = new Vector3(x, roomHeight - 0.08f, 0f);
            lightStrip.transform.localScale = new Vector3(0.18f, 0.02f, roomDepth - 1.5f);
            Destroy(lightStrip.GetComponent<Collider>());
            SetColor(lightStrip, new Color(0.9f, 0.88f, 0.78f) * 0.6f);
        }
    }

    static void SetColor(GameObject go, Color c)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        r.material.color = c;
        if (r.material.HasProperty("_BaseColor"))
            r.material.SetColor("_BaseColor", c);
    }
}
