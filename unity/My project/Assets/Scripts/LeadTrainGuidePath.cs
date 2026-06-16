using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class LeadTrainGuidePath : MonoBehaviour
{
    static readonly Color LightBlue = new Color(0.55f, 0.82f, 1f, 1f);
    static readonly Color LightBlueSoft = new Color(0.62f, 0.86f, 1f, 0.85f);

    [Header("Ground Line")]
    public Color lineColorA = new Color(0.45f, 0.78f, 1f, 1f);
    public Color lineColorB = new Color(0.72f, 0.92f, 1f, 1f);
    public float colorAlternateSpeed = 8f;
    public float stripWidth = 1.25f;
    public float stripLength = 8f;
    public float pathGroundOffset = 0.03f;
    public float groundRaycastHeight = 30f;
    public float groundRaycastDistance = 120f;

    [Header("Ground Chevrons")]
    public Color chevronColorA = new Color(0.45f, 0.78f, 1f, 1f);
    public Color chevronColorB = new Color(0.72f, 0.92f, 1f, 1f);
    public float chevronSpacing = 18f;
    public int maxChevrons = 14;
    public float chevronLength = 4f;
    public float chevronWidth = 2.5f;
    public float chevronHeight = 0.04f;

    [Header("Target Ring")]
    public Color ringColor = LightBlueSoft;
    public float ringRadius = 5f;

    Material _lineMaterialA;
    Material _lineMaterialB;
    Material _chevronMaterialA;
    Material _chevronMaterialB;
    Material _ringMaterial;
    readonly List<Transform> _stripSegments = new List<Transform>();
    readonly List<Transform> _chevrons = new List<Transform>();
    Transform _targetRing;
    float _resolvedGroundY;
    bool _visible;
    bool _hasCachedPath;
    Vector3 _cachedStart;
    Vector3 _cachedEnd;
    int _activeStripCount;
    int _activeChevronCount;

    public void SetVisible(bool visible)
    {
        _visible = visible;

        if (_targetRing != null)
        {
            _targetRing.gameObject.SetActive(visible);
        }

        if (!visible)
        {
            SetStripSegmentCount(0);
            SetChevronCount(0);
        }
    }

    public void UpdatePath(Vector3 playerPosition, Vector3 targetPosition)
    {
        EnsureVisuals();

        float groundY = ResolveHorizontalGroundY(playerPosition, targetPosition);
        Vector3 start = new Vector3(playerPosition.x, groundY, playerPosition.z);
        Vector3 end = new Vector3(targetPosition.x, groundY, targetPosition.z);
        _resolvedGroundY = groundY;

        _cachedStart = start;
        _cachedEnd = end;
        _hasCachedPath = true;

        UpdatePathStrip(start, end);
        UpdateChevrons(start, end);
        UpdateTargetRing(end);
    }

    void Update()
    {
        if (!_visible || !_hasCachedPath)
        {
            return;
        }

        RefreshAlternatingColors();
    }

    void UpdateTargetRing(Vector3 end)
    {
        if (_targetRing == null)
        {
            return;
        }

        _targetRing.position = end + Vector3.up * 0.01f;
        _targetRing.rotation = Quaternion.Euler(90f, 0f, 0f);
        float pulse = 1f + Mathf.Sin(Time.time * 3.5f) * 0.08f;
        _targetRing.localScale = new Vector3(ringRadius * pulse, ringRadius * pulse, 1f);
        _targetRing.gameObject.SetActive(_visible);
        ApplyRendererColor(_targetRing.GetComponent<Renderer>(), GetAlternatingColor(0, 0));
    }

    int GetColorPhase()
    {
        return Mathf.FloorToInt(Time.time * colorAlternateSpeed);
    }

    Color GetAlternatingColor(int index, int phase)
    {
        return ((index + phase) & 1) == 0 ? lineColorA : lineColorB;
    }

    Color GetAlternatingChevronColor(int index, int phase)
    {
        return ((index + phase) & 1) == 0 ? chevronColorA : chevronColorB;
    }

    void RefreshAlternatingColors()
    {
        int phase = GetColorPhase();

        for (int i = 0; i < _activeStripCount && i < _stripSegments.Count; i++)
        {
            Transform segment = _stripSegments[i];
            if (segment == null || !segment.gameObject.activeSelf)
            {
                continue;
            }

            ApplyRendererColor(segment.GetComponent<Renderer>(), GetAlternatingColor(i, phase));
        }

        for (int i = 0; i < _activeChevronCount && i < _chevrons.Count; i++)
        {
            Transform chevron = _chevrons[i];
            if (chevron == null || !chevron.gameObject.activeSelf)
            {
                continue;
            }

            Color chevronColor = GetAlternatingChevronColor(i, phase);
            foreach (Renderer renderer in chevron.GetComponentsInChildren<Renderer>())
            {
                ApplyRendererColor(renderer, chevronColor);
            }
        }

        if (_targetRing != null && _targetRing.gameObject.activeSelf)
        {
            ApplyRendererColor(_targetRing.GetComponent<Renderer>(), GetAlternatingColor(0, phase));
        }
    }

    static void ApplyRendererColor(Renderer renderer, Color color)
    {
        if (renderer == null)
        {
            return;
        }

        Material material = renderer.material;
        ConfigureMaterialColor(material, color);
    }

    float ResolveHorizontalGroundY(Vector3 playerPosition, Vector3 targetPosition)
    {
        // 以当前目标设备根节点 Y 作为工厂地面基准，避免射线打到机器本体或玩家层导致线条悬空。
        float floorY = targetPosition.y;

        if (TrySampleLowestGroundY(targetPosition, out float targetGroundY))
        {
            floorY = Mathf.Min(floorY, targetGroundY);
        }

        if (TrySampleLowestGroundY(playerPosition, out float playerGroundY))
        {
            floorY = Mathf.Min(floorY, playerGroundY);
        }

        return floorY + pathGroundOffset;
    }

    bool TrySampleLowestGroundY(Vector3 worldPosition, out float groundY)
    {
        Vector3 origin = worldPosition + Vector3.up * groundRaycastHeight;
        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            groundRaycastDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            groundY = worldPosition.y;
            return false;
        }

        groundY = hits[0].point.y;
        for (int i = 1; i < hits.Length; i++)
        {
            groundY = Mathf.Min(groundY, hits[i].point.y);
        }

        return true;
    }

    bool TrySampleGroundY(Vector3 worldPosition, out float groundY)
    {
        if (TrySampleLowestGroundY(worldPosition, out groundY))
        {
            return true;
        }

        groundY = worldPosition.y;
        return false;
    }

    void EnsureVisuals()
    {
        if (_lineMaterialA != null)
        {
            return;
        }

        _lineMaterialA = CreateGuideMaterial(lineColorA);
        _lineMaterialB = CreateGuideMaterial(lineColorB);
        _chevronMaterialA = CreateGuideMaterial(chevronColorA);
        _chevronMaterialB = CreateGuideMaterial(chevronColorB);
        _ringMaterial = CreateGuideMaterial(lineColorA);
        _targetRing = CreateFlatQuad("LeadTrainGuideTargetRing", _ringMaterial);
        EnsureStripSegments();
        EnsureChevrons();
    }

    void UpdatePathStrip(Vector3 start, Vector3 end)
    {
        EnsureStripSegments();

        Vector3 delta = end - start;
        float planarDistance = new Vector3(delta.x, 0f, delta.z).magnitude;
        if (planarDistance < 0.01f)
        {
            _activeStripCount = 0;
            SetStripSegmentCount(0);
            return;
        }

        Vector3 planarDirection = new Vector3(delta.x, 0f, delta.z).normalized;
        int segmentCount = Mathf.Clamp(Mathf.CeilToInt(planarDistance / stripLength), 2, 64);
        _activeStripCount = segmentCount;
        SetStripSegmentCount(segmentCount);

        int phase = GetColorPhase();
        for (int i = 0; i < segmentCount; i++)
        {
            float centerT = (i + 0.5f) / segmentCount;
            Transform segment = _stripSegments[i];
            segment.gameObject.SetActive(_visible);
            segment.position = Vector3.Lerp(start, end, centerT);
            segment.rotation = CreateHorizontalStripRotation(planarDirection);

            float overlap = 1.08f;
            float lengthScale = (planarDistance / segmentCount) * overlap;
            segment.localScale = new Vector3(stripWidth, lengthScale, 1f);
            ApplyRendererColor(segment.GetComponent<Renderer>(), GetAlternatingColor(i, phase));
        }
    }

    static Quaternion CreateHorizontalStripRotation(Vector3 planarDirection)
    {
        float yAngle = Mathf.Atan2(planarDirection.x, planarDirection.z) * Mathf.Rad2Deg;
        return Quaternion.Euler(90f, yAngle, 0f);
    }

    void EnsureStripSegments()
    {
        while (_stripSegments.Count < 64)
        {
            int index = _stripSegments.Count;
            Material material = (index & 1) == 0 ? _lineMaterialA : _lineMaterialB;
            _stripSegments.Add(CreateFlatQuad("LeadTrainGuideStrip_" + index, material));
        }
    }

    void SetStripSegmentCount(int count)
    {
        for (int i = 0; i < _stripSegments.Count; i++)
        {
            if (_stripSegments[i] != null)
            {
                _stripSegments[i].gameObject.SetActive(_visible && i < count);
            }
        }
    }

    Transform CreateFlatQuad(string objectName, Material material)
    {
        GameObject quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadObject.name = objectName;
        quadObject.transform.SetParent(transform, false);

        Collider collider = quadObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = quadObject.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return quadObject.transform;
    }

    void EnsureChevrons()
    {
        while (_chevrons.Count < maxChevrons)
        {
            GameObject chevronObject = new GameObject("LeadTrainGuideChevron_" + _chevrons.Count);
            chevronObject.transform.SetParent(transform, false);

            Transform body = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            body.name = "Body";
            body.SetParent(chevronObject.transform, false);
            body.localScale = new Vector3(chevronWidth * 0.16f, chevronHeight, chevronLength * 0.5f);
            body.localPosition = new Vector3(0f, 0f, chevronLength * 0.16f);
            DestroyCollider(body);

            Transform leftWing = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            leftWing.name = "Left";
            leftWing.SetParent(chevronObject.transform, false);
            leftWing.localScale = new Vector3(chevronWidth * 0.38f, chevronHeight, chevronLength * 0.2f);
            leftWing.localPosition = new Vector3(-chevronWidth * 0.2f, 0f, -chevronLength * 0.1f);
            leftWing.localRotation = Quaternion.Euler(0f, -28f, 0f);
            DestroyCollider(leftWing);

            Transform rightWing = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            rightWing.name = "Right";
            rightWing.SetParent(chevronObject.transform, false);
            rightWing.localScale = new Vector3(chevronWidth * 0.38f, chevronHeight, chevronLength * 0.2f);
            rightWing.localPosition = new Vector3(chevronWidth * 0.2f, 0f, -chevronLength * 0.1f);
            rightWing.localRotation = Quaternion.Euler(0f, 28f, 0f);
            DestroyCollider(rightWing);

            ApplyMaterial(body, _chevronMaterialA);
            ApplyMaterial(leftWing, _chevronMaterialA);
            ApplyMaterial(rightWing, _chevronMaterialA);
            DisableShadows(body);
            DisableShadows(leftWing);
            DisableShadows(rightWing);

            _chevrons.Add(chevronObject.transform);
        }
    }

    void UpdateChevrons(Vector3 start, Vector3 end)
    {
        EnsureChevrons();

        Vector3 delta = end - start;
        float planarDistance = new Vector3(delta.x, 0f, delta.z).magnitude;
        if (planarDistance < 0.01f)
        {
            _activeChevronCount = 0;
            SetChevronCount(0);
            return;
        }

        Vector3 direction = new Vector3(delta.x, 0f, delta.z).normalized;
        int chevronCount = Mathf.Clamp(Mathf.FloorToInt(planarDistance / chevronSpacing), 2, maxChevrons);
        _activeChevronCount = chevronCount;
        SetChevronCount(chevronCount);

        int phase = GetColorPhase();
        for (int i = 0; i < chevronCount; i++)
        {
            float t = (i + 1f) / (chevronCount + 1f);
            Transform chevron = _chevrons[i];
            Vector3 position = Vector3.Lerp(start, end, t);
            position.y = _resolvedGroundY + chevronHeight * 0.25f;

            chevron.position = position;
            chevron.rotation = Quaternion.LookRotation(direction, Vector3.up);

            float pulse = 1f + Mathf.Sin(Time.time * 6f - i * 0.5f) * 0.06f;
            chevron.localScale = Vector3.one * pulse;

            Color chevronColor = GetAlternatingChevronColor(i, phase);
            foreach (Renderer renderer in chevron.GetComponentsInChildren<Renderer>())
            {
                ApplyRendererColor(renderer, chevronColor);
            }
        }
    }

    void SetChevronCount(int count)
    {
        for (int i = 0; i < _chevrons.Count; i++)
        {
            if (_chevrons[i] != null)
            {
                _chevrons[i].gameObject.SetActive(_visible && i < count);
            }
        }
    }

    static Material CreateGuideMaterial(Color color)
    {
        Shader shader = ResolveUnlitShader();
        Material material = new Material(shader);
        ConfigureMaterialColor(material, color);
        return material;
    }

    static Shader ResolveUnlitShader()
    {
        string[] shaderNames =
        {
            "Unlit/Color",
            "Sprites/Default",
            "Universal Render Pipeline/Unlit",
            "Legacy Shaders/Transparent/Diffuse",
            "Mobile/Unlit (Supports Lightmap)",
            "Standard"
        };

        foreach (string shaderName in shaderNames)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
            {
                return shader;
            }
        }

        return Shader.Find("Hidden/InternalErrorShader");
    }

    static void ConfigureMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        material.color = color;

        if (color.a < 0.999f)
        {
            SetTransparentMode(material);
        }
    }

    static void SetTransparentMode(Material material)
    {
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = (int)RenderQueue.Transparent;
    }

    static void ApplyMaterial(Transform part, Material material)
    {
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    static void DisableShadows(Transform part)
    {
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    static void DestroyCollider(Transform part)
    {
        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }
}
