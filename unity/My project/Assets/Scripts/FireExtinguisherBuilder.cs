using System.Collections;
using UnityEngine;

public class FireExtinguisherBuilder : MonoBehaviour
{
    public const string StaticExtinguisherName = "Fire Extinguisher Static";
    public const string GeneratedModelName = "__Generated_Fire_Extinguisher_Model";

    [Header("Placement")]
    public Vector3 extinguisherWorldPosition = new Vector3(12f, 0f, -10f);

    [Tooltip("灭火器挂在南墙上，默认正面朝北，也就是 +Z 方向。")]
    public bool faceNorth = true;

    [Header("Size, meters")]
    public float bodyDiameter = 0.35f;
    public float totalHeight = 0.60f;

    [Header("Build Options")]
    public bool buildOnStart = false;
    public bool addColliders = true;

    private const string RootName = GeneratedModelName;

    private void Start()
    {
        if (buildOnStart)
        {
            BuildExtinguisher();
        }
    }

    [ContextMenu("Build / Rebuild Fire Extinguisher")]
    public void BuildExtinguisher()
    {
        ClearOldModel();

        transform.position = extinguisherWorldPosition;
        transform.rotation = faceNorth ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        transform.localScale = Vector3.one;

        Material matRed = CreateLitMaterial("MAT_Extinguisher_Red", new Color(0.85f, 0.02f, 0.02f), 0.05f, 0.42f);
        Material matDarkRed = CreateLitMaterial("MAT_Dark_Red_Cap", new Color(0.45f, 0.01f, 0.01f), 0.1f, 0.35f);
        Material matBlack = CreateLitMaterial("MAT_Black_Rubber", new Color(0.015f, 0.015f, 0.015f), 0.1f, 0.25f);
        Material matMetal = CreateLitMaterial("MAT_Metal", new Color(0.35f, 0.36f, 0.34f), 0.75f, 0.45f);
        Material matWhite = CreateLitMaterial("MAT_Label_White", new Color(0.92f, 0.90f, 0.82f), 0f, 0.28f);
        Material matGaugeWhite = CreateLitMaterial("MAT_Gauge_Face", new Color(0.96f, 0.96f, 0.92f), 0f, 0.4f);
        Material matGaugeGreen = CreateLitMaterial("MAT_Gauge_Green_Normal", new Color(0.0f, 1.0f, 0.12f), 0f, 0.55f);
        Material matGaugeNeedle = CreateLitMaterial("MAT_Gauge_Needle_Red", new Color(1.0f, 0.02f, 0.01f), 0f, 0.42f);
        Material matWarningYellow = CreateLitMaterial("MAT_Pin_Yellow", new Color(1.0f, 0.78f, 0.05f), 0.05f, 0.35f);

        GameObject root = new GameObject(RootName);
        root.transform.SetParent(transform, false);

        // 坐标约定：
        // X：左右
        // Y：高度
        // Z：前后。正面朝 +Z，南墙在后方。

        float radius = bodyDiameter * 0.5f;
        float bodyHeight = totalHeight * 0.78f;
        float bodyCenterY = 0.08f + bodyHeight * 0.5f;
        float bodyFrontZ = radius + 0.08f;

        // 壁挂支架：贴南墙
        CreateCube(
            "Wall_Back_Plate",
            root.transform,
            new Vector3(0f, 0.36f, -0.025f),
            new Vector3(0.46f, 0.72f, 0.035f),
            matMetal
        );

        CreateCube(
            "Upper_Wall_Bracket",
            root.transform,
            new Vector3(0f, 0.43f, 0.055f),
            new Vector3(0.42f, 0.055f, 0.08f),
            matMetal
        );

        CreateCube(
            "Lower_Wall_Bracket",
            root.transform,
            new Vector3(0f, 0.18f, 0.055f),
            new Vector3(0.42f, 0.055f, 0.08f),
            matMetal
        );

        CreateCube(
            "Bottom_Support_Shelf",
            root.transform,
            new Vector3(0f, 0.045f, 0.10f),
            new Vector3(0.42f, 0.05f, 0.18f),
            matMetal
        );

        // 灭火器罐体
        GameObject body = CreateCylinder(
            "Dry_Powder_Cylinder_Body",
            root.transform,
            new Vector3(0f, bodyCenterY, 0.16f),
            bodyDiameter,
            bodyHeight,
            matRed,
            CylinderAxis.Y
        );

        // 底部黑色底座
        CreateCylinder(
            "Bottom_Black_Base",
            root.transform,
            new Vector3(0f, 0.075f, 0.16f),
            bodyDiameter * 0.95f,
            0.06f,
            matBlack,
            CylinderAxis.Y
        );

        // 顶部瓶颈
        CreateCylinder(
            "Top_Neck",
            root.transform,
            new Vector3(0f, 0.56f, 0.16f),
            0.11f,
            0.12f,
            matDarkRed,
            CylinderAxis.Y
        );

        // 阀门金属块
        CreateCube(
            "Valve_Block",
            root.transform,
            new Vector3(0f, 0.635f, 0.16f),
            new Vector3(0.18f, 0.055f, 0.10f),
            matMetal
        );

        // 压把，上下两个手柄
        CreateCube(
            "Upper_Handle",
            root.transform,
            new Vector3(0f, 0.69f, 0.16f),
            new Vector3(0.34f, 0.035f, 0.075f),
            matBlack
        );

        CreateCube(
            "Lower_Handle",
            root.transform,
            new Vector3(0f, 0.625f, 0.16f),
            new Vector3(0.30f, 0.03f, 0.065f),
            matBlack
        );

        // 黄色安全销
        CreateCylinder(
            "Safety_Pin_Yellow_Ring",
            root.transform,
            new Vector3(-0.15f, 0.635f, 0.16f),
            0.055f,
            0.018f,
            matWarningYellow,
            CylinderAxis.X
        );

        // 喷嘴
        CreateCylinder(
            "Nozzle",
            root.transform,
            new Vector3(0.22f, 0.635f, 0.16f),
            0.045f,
            0.22f,
            matBlack,
            CylinderAxis.X
        );

        // 黑色软管，用多段 Cylinder 模拟
        CreateHose(root.transform, matBlack);

        // 压力表
        Vector3 gaugeCenter = new Vector3(0f, 0.57f, bodyFrontZ + 0.02f);

        GameObject gaugeRing = CreateCylinder(
            "Pressure_Gauge_Metal_Ring",
            root.transform,
            gaugeCenter,
            0.13f,
            0.022f,
            matMetal,
            CylinderAxis.Z
        );

        GameObject gaugeFace = CreateCylinder(
            "Pressure_Gauge_White_Face",
            root.transform,
            gaugeCenter + new Vector3(0f, 0f, 0.014f),
            0.105f,
            0.012f,
            matGaugeWhite,
            CylinderAxis.Z
        );

        // 压力表绿色正常区域：自定义 Mesh 扇形
        GameObject gaugeGreenZone = CreateGaugeGreenZone(
            "Pressure_Gauge_Green_Zone",
            root.transform,
            gaugeCenter + new Vector3(0f, 0f, 0.024f),
            0.016f,
            0.058f,
            205f,
            330f,
            matGaugeGreen
        );

        // 压力表指针，指向绿色区域
        Transform needlePivot = CreateGaugeNeedle(
            root.transform,
            gaugeCenter + new Vector3(0f, 0f, 0.034f),
            265f,
            matGaugeNeedle
        );

        // 给压力表加一个点击脚本，后续可以接入教学步骤系统
        FireExtinguisherGaugeInteraction gaugeInteraction =
            gaugeFace.AddComponent<FireExtinguisherGaugeInteraction>();

        gaugeInteraction.needlePivot = needlePivot;
        gaugeInteraction.normalAngle = 265f;
        gaugeInteraction.gaugeParts = new[]
        {
            gaugeRing.transform,
            gaugeFace.transform,
            gaugeGreenZone.transform,
            needlePivot
        };

        Debug.Log("Fire extinguisher generated at " + extinguisherWorldPosition);
    }

    private void CreateHose(Transform parent, Material material)
    {
        Vector3[] points =
        {
            new Vector3(0.13f, 0.63f, 0.20f),
            new Vector3(0.24f, 0.55f, 0.28f),
            new Vector3(0.22f, 0.38f, 0.31f),
            new Vector3(0.14f, 0.24f, 0.30f)
        };

        for (int i = 0; i < points.Length - 1; i++)
        {
            CreateCylinderBetween(
                "Black_Rubber_Hose_" + i,
                parent,
                points[i],
                points[i + 1],
                0.032f,
                material
            );
        }
    }

    private Transform CreateGaugeNeedle(Transform parent, Vector3 localPosition, float angleDeg, Material material)
    {
        GameObject pivot = new GameObject("Pressure_Gauge_Needle_Pivot");
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = localPosition;
        pivot.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);

        GameObject needle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        needle.name = "Pressure_Gauge_Needle";
        needle.transform.SetParent(pivot.transform, false);

        // 指针沿局部 X 轴伸出
        needle.transform.localPosition = new Vector3(0.036f, 0f, 0f);
        needle.transform.localRotation = Quaternion.identity;
        needle.transform.localScale = new Vector3(0.08f, 0.012f, 0.012f);

        Renderer renderer = needle.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        if (!addColliders)
        {
            Collider col = needle.GetComponent<Collider>();
            if (col != null) DestroySafe(col);
        }

        return pivot.transform;
    }

    private GameObject CreateGaugeGreenZone(
        string name,
        Transform parent,
        Vector3 localPosition,
        float innerRadius,
        float outerRadius,
        float startAngleDeg,
        float endAngleDeg,
        Material material
    )
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        int segments = 24;
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(startAngleDeg, endAngleDeg, t) * Mathf.Deg2Rad;

            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            int index = i * 2;

            vertices[index] = new Vector3(cos * innerRadius, sin * innerRadius, 0f);
            vertices[index + 1] = new Vector3(cos * outerRadius, sin * outerRadius, 0f);

            normals[index] = Vector3.forward;
            normals[index + 1] = Vector3.forward;

            uvs[index] = new Vector2(t, 0f);
            uvs[index + 1] = new Vector2(t, 1f);
        }

        for (int i = 0; i < segments; i++)
        {
            int v = i * 2;
            int tri = i * 6;

            triangles[tri] = v;
            triangles[tri + 1] = v + 1;
            triangles[tri + 2] = v + 2;

            triangles[tri + 3] = v + 2;
            triangles[tri + 4] = v + 1;
            triangles[tri + 5] = v + 3;
        }

        Mesh mesh = new Mesh();
        mesh.name = name + "_Mesh";
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        return obj;
    }

    private enum CylinderAxis
    {
        X,
        Y,
        Z
    }

    private GameObject CreateCube(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = size;

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        if (!addColliders)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null) DestroySafe(col);
        }

        return obj;
    }

    private GameObject CreateCylinder(
        string name,
        Transform parent,
        Vector3 localPosition,
        float diameter,
        float length,
        Material material,
        CylinderAxis axis
    )
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;

        // Unity 默认 Cylinder 高度沿 Y 轴，默认高度约为 2。
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);

        if (axis == CylinderAxis.X)
            obj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        else if (axis == CylinderAxis.Z)
            obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        else
            obj.transform.localRotation = Quaternion.identity;

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        if (!addColliders)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null) DestroySafe(col);
        }

        return obj;
    }

    private void CreateCylinderBetween(
        string name,
        Transform parent,
        Vector3 start,
        Vector3 end,
        float diameter,
        Material material
    )
    {
        Vector3 mid = (start + end) * 0.5f;
        Vector3 direction = end - start;
        float length = direction.magnitude;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = mid;
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        obj.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);

        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        if (!addColliders)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null) DestroySafe(col);
        }
    }

    private void CreateTextLabel(string name, Transform parent, string text, Vector3 localPosition, float size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;

        // TextMesh 默认在 XY 平面显示，正面大致朝 +Z。
        obj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = obj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = size;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
    }

    private Material CreateLitMaterial(string name, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader == null)
            shader = Shader.Find("Diffuse");

        Material mat = new Material(shader);
        mat.name = name;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);

        return mat;
    }

    private void ClearOldModel()
    {
        Transform old = transform.Find(RootName);

        if (old != null)
        {
            DestroySafe(old.gameObject);
        }
    }

    private void DestroySafe(Object obj)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(obj);
        else
            Destroy(obj);
#else
        Destroy(obj);
#endif
    }
}

#if false
public class FireExtinguisherGaugeInteraction : MonoBehaviour
{
    public Transform needlePivot;
    public float normalAngle = 265f;
    public KeyCode interactKey = KeyCode.H;
    public float enlargedScaleMultiplier = 5f;
    public float enlargedSeconds = 3f;
    public float transitionSeconds = 0.15f;
    public Transform gaugeRoot;
    public Transform[] gaugeParts;

    private Coroutine enlargeRoutine;
    private Transform[] runtimeGaugeParts;
    private Vector3[] originalGaugeScales;
    private Vector3[] originalGaugePositions;
    private bool hasOriginalGaugeState;

    private bool suppressGuidedInput;

    private void Awake()
    {
        EnsureGaugeParts();

        if (needlePivot != null)
        {
            needlePivot.gameObject.isStatic = false;
        }
    }

    private void Update()
    {
        if (suppressGuidedInput || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        InspectGauge();
    }

    private void OnMouseDown()
    {
        InspectGauge();
    }

    public void InspectGauge()
    {
        if (needlePivot != null)
        {
            needlePivot.localRotation = Quaternion.Euler(0f, 0f, normalAngle);
        }

        if (enlargeRoutine != null)
        {
            StopCoroutine(enlargeRoutine);
        }

        enlargeRoutine = StartCoroutine(EnlargeGaugeTemporarily());
        Debug.Log($"灭火器压力表检查完成：指针位于绿色区域，压力正常。表盘放大 {enlargedSeconds:0.##} 秒。");
    }

    private IEnumerator EnlargeGaugeTemporarily()
    {
        EnsureGaugeParts();

        if (runtimeGaugeParts == null || runtimeGaugeParts.Length == 0)
        {
            yield break;
        }

        CacheOriginalGaugeState();

        Vector3 center = transform.localPosition;

        yield return AnimateGaugeScale(enlargedScaleMultiplier, originalGaugeScales, originalGaugePositions, center);
        yield return new WaitForSeconds(enlargedSeconds);
        yield return AnimateGaugeScale(1f, originalGaugeScales, originalGaugePositions, center);

        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] == null)
            {
                continue;
            }

            runtimeGaugeParts[i].localScale = originalGaugeScales[i];
            runtimeGaugeParts[i].localPosition = originalGaugePositions[i];
        }

        enlargeRoutine = null;
    }

    private IEnumerator AnimateGaugeScale(
        float toMultiplier,
        Vector3[] originalScales,
        Vector3[] originalPositions,
        Vector3 center)
    {
        if (transitionSeconds <= 0f)
        {
            ApplyGaugeScale(toMultiplier, originalScales, originalPositions, center);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < transitionSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionSeconds);
            float smoothT = t * t * (3f - 2f * t);
            float scale = Mathf.Lerp(GetCurrentGaugeMultiplier(originalScales), toMultiplier, smoothT);
            ApplyGaugeScale(scale, originalScales, originalPositions, center);
            yield return null;
        }

        ApplyGaugeScale(toMultiplier, originalScales, originalPositions, center);
    }

    private void ApplyGaugeScale(
        float multiplier,
        Vector3[] originalScales,
        Vector3[] originalPositions,
        Vector3 center)
    {
        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] == null)
            {
                continue;
            }

            runtimeGaugeParts[i].localScale = originalScales[i] * multiplier;
            runtimeGaugeParts[i].localPosition = center + (originalPositions[i] - center) * multiplier;
        }
    }

    private float GetCurrentGaugeMultiplier(Vector3[] originalScales)
    {
        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] != null && originalScales[i].sqrMagnitude > 0f)
            {
                return runtimeGaugeParts[i].localScale.magnitude / originalScales[i].magnitude;
            }
        }

        return 1f;
    }

    private void CacheOriginalGaugeState()
    {
        if (hasOriginalGaugeState && originalGaugeScales != null && originalGaugeScales.Length == runtimeGaugeParts.Length)
        {
            return;
        }

        originalGaugeScales = new Vector3[runtimeGaugeParts.Length];
        originalGaugePositions = new Vector3[runtimeGaugeParts.Length];

        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] == null)
            {
                continue;
            }

            originalGaugeScales[i] = runtimeGaugeParts[i].localScale;
            originalGaugePositions[i] = runtimeGaugeParts[i].localPosition;
        }

        hasOriginalGaugeState = true;
    }

    private void EnsureGaugeParts()
    {
        if (gaugeRoot != null)
        {
            runtimeGaugeParts = new[] { gaugeRoot };
            gaugeRoot.gameObject.isStatic = false;
            return;
        }

        if (gaugeParts != null && gaugeParts.Length > 0)
        {
            runtimeGaugeParts = gaugeParts;
            MarkGaugePartsDynamic();
            return;
        }

        Transform parent = transform.parent;
        if (parent == null)
        {
            runtimeGaugeParts = new[] { transform };
            MarkGaugePartsDynamic();
            return;
        }

        Transform ring = parent.Find("Pressure_Gauge_Metal_Ring");
        Transform greenZone = parent.Find("Pressure_Gauge_Green_Zone");
        Transform pivot = needlePivot != null ? needlePivot : parent.Find("Pressure_Gauge_Needle_Pivot");

        runtimeGaugeParts = new[]
        {
            ring,
            transform,
            greenZone,
            pivot
        };

        MarkGaugePartsDynamic();
    }

    private void MarkGaugePartsDynamic()
    {
        if (runtimeGaugeParts == null)
        {
            return;
        }

        for (int i = 0; i < runtimeGaugeParts.Length; i++)
        {
            if (runtimeGaugeParts[i] != null)
            {
                runtimeGaugeParts[i].gameObject.isStatic = false;
            }
        }
    }

    public IEnumerator PlayGuidedSequence()
    {
        suppressGuidedInput = true;
        InspectGauge();
        yield return new WaitForSeconds(enlargedSeconds + transitionSeconds * 2f + 0.6f);
        suppressGuidedInput = false;
    }
}

#endif

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(FireExtinguisherBuilder))]
public class FireExtinguisherBuilderEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FireExtinguisherBuilder builder = (FireExtinguisherBuilder)target;

        GUILayout.Space(12);

        if (GUILayout.Button("Build / Rebuild Fire Extinguisher", GUILayout.Height(36)))
        {
            builder.BuildExtinguisher();
        }
    }
}
#endif
