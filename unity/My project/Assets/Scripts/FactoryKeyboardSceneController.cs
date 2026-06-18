using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FactoryKeyboardSceneController : MonoBehaviour
{
    private const string HubPicsResourcesFolder = "HubPics";
    private const float DefaultVisitorStartWorldX = 186.764f;
    private const float DefaultVisitorStartWorldZ = -58.323f;
    private const float DefaultVisitorFloorHeightRatio = 0.320f;

    private enum OverlaySurface
    {
        Front,
        Back,
        Right,
        Left,
        Top,
        Bottom,
        CustomLocal
    }

    [System.Serializable]
    private class RendererMaterialOverride
    {
        public string rendererName;
        public bool matchContains;
        public int materialSlot = -1;
        public Material material;
    }

    [System.Serializable]
    private class ImageOverlay
    {
        public string rendererName;
        public bool matchContains;
        public string imageAssetPath;
        public Texture2D image;
        public OverlaySurface surface = OverlaySurface.Front;
        public bool useViewerFacingSurface = true;
        public bool fitToSurface = true;
        public Vector2 size = new Vector2(20f, 10f);
        public Vector3 localOffset;
        public Vector3 customLocalEulerAngles;
        public float surfaceOffset = 0.8f;
    }

    [Header("Factory")]
    [SerializeField] private Transform sceneFactoryRoot;
    [SerializeField] private bool spawnFactoryAtRuntime;
    [SerializeField] private GameObject factoryPrefab;
    [SerializeField] private string factoryAssetPath = "Assets/Factory/LimeExp_2.FBX";
    [SerializeField] private float desiredFactorySize = 1000f;
    [SerializeField] private bool addMeshColliders = true;

    [Header("Material Overrides")]
    [SerializeField] private RendererMaterialOverride[] materialOverrides;

    [Header("Image Overlays")]
    [SerializeField] private ImageOverlay[] imageOverlays;

    [Header("First Person")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool spawnPointRepresentsEyePosition = true;
    [SerializeField] private float moveSpeed = 24f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float playerHeight = 1.8f;
    [SerializeField] private float playerRadius = 0.35f;
    [SerializeField] private bool useCollisionWalking = false;
    [SerializeField] private bool enableGravity = false;
    [SerializeField] private float gravity = -18f;
    [SerializeField] private Vector3 visitorStartOffset = new Vector3(0f, 0f, 0.35f);
    [SerializeField] private bool useDefaultVisitorStartPosition = true;
    [SerializeField] private Vector2 visitorStartWorldXZ = new Vector2(DefaultVisitorStartWorldX, DefaultVisitorStartWorldZ);
    [SerializeField] private bool forceSecondFloorStart = true;
    [SerializeField] private float secondFloorStartRatio = DefaultVisitorFloorHeightRatio;
    [SerializeField] private float fixedFloorHeightRatio = DefaultVisitorFloorHeightRatio;
    [SerializeField] private bool useRaycastFloorDetection = false;
    [SerializeField] private float debugVerticalAdjustSpeed = 20f;
    [SerializeField] private KeyCode logHeightKey = KeyCode.H;
    [SerializeField] private float visitorYaw = 0f;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float cameraFieldOfView = 70f;

    [Header("Scene Helpers")]
    [SerializeField] private bool createInteriorFloorCollider = true;
    [SerializeField] private bool createGround = true;
    [SerializeField] private float groundSize = 1200f;

    private Transform factoryInstance;
    private Transform invisibleInteriorFloor;
    private CharacterController controller;
    private Transform playerRig;
    private Bounds factoryBounds;
    private float verticalVelocity;
    private float cameraPitch;
    private float nextHeightLogTime;

    private void Awake()
    {
        ConfigureSceneLighting();
        InitializeFactory();
        CreateInteriorFloorCollider();
        SetupFirstPersonPlayer();
        CreateGround();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetVisitorToInterior();
        }

        AdjustVisitorHeight();
        if (Input.GetKeyDown(logHeightKey))
        {
            LogVisitorHeight();
        }

        MovePlayer();
        LookAround();
    }

    private void InitializeFactory()
    {
        if (sceneFactoryRoot != null)
        {
            factoryInstance = sceneFactoryRoot;
            PrepareFactoryInstance();
            return;
        }

        var staticFactory = GameObject.Find("Factory Static");
        if (staticFactory != null)
        {
            sceneFactoryRoot = staticFactory.transform;
            factoryInstance = sceneFactoryRoot;
            PrepareFactoryInstance();
            return;
        }

        if (!spawnFactoryAtRuntime)
        {
            Debug.LogWarning("Static factory not found. Assign Scene Factory Root or run Tools/Factory/Create Static Factory In Scene. Runtime spawning is disabled.", this);
            return;
        }

        SpawnFactoryAtRuntime();
    }

    private void SpawnFactoryAtRuntime()
    {
        var prefab = factoryPrefab;

#if UNITY_EDITOR
        if (prefab == null && !string.IsNullOrWhiteSpace(factoryAssetPath))
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(factoryAssetPath);
        }
#endif

        if (prefab == null)
        {
            Debug.LogWarning("Factory prefab is not assigned or could not be loaded.", this);
            return;
        }

        var instance = Instantiate(prefab, transform);
        instance.name = "Factory Interior";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        factoryInstance = instance.transform;

        PrepareFactoryInstance();
    }

    private void ConfigureSceneLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.72f, 0.76f, 0.82f);
        RenderSettings.ambientIntensity = 1.6f;

        foreach (var light in FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional)
            {
                light.intensity = Mathf.Max(light.intensity, 1.8f);
            }
        }
    }

    private void PrepareFactoryInstance()
    {
        NormalizeFactoryBounds();
        ApplyMaterialOverrides();
        ApplyImageOverlays();

        if (addMeshColliders)
        {
            AddFactoryColliders();
        }
    }

    private void NormalizeFactoryBounds()
    {
        if (factoryInstance == null)
        {
            return;
        }

        var renderers = factoryInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        factoryBounds = CalculateRendererBounds(renderers);

        var maxDimension = Mathf.Max(factoryBounds.size.x, factoryBounds.size.y, factoryBounds.size.z);
        if (maxDimension > 0.01f)
        {
            var scale = desiredFactorySize / maxDimension;
            factoryInstance.localScale *= scale;
        }

        factoryBounds = CalculateRendererBounds(renderers);
        factoryInstance.position -= factoryBounds.center - transform.position;
        factoryBounds = CalculateRendererBounds(renderers);
    }

    private Bounds CalculateRendererBounds(Renderer[] renderers)
    {
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private void AddFactoryColliders()
    {
        if (factoryInstance == null)
        {
            return;
        }

#if !UNITY_EDITOR
        Debug.Log(
            "[FactoryKeyboard] Skipped runtime MeshCollider generation in Player build. " +
            "FBX meshes are not readable after packaging and Unity can crash while creating collision data.",
            this);
        return;
#else
        var addedCount = 0;
        var skippedUnreadableCount = 0;
        foreach (var meshFilter in factoryInstance.GetComponentsInChildren<MeshFilter>())
        {
            var mesh = meshFilter.sharedMesh;
            if (mesh == null || meshFilter.GetComponent<Collider>() != null)
            {
                continue;
            }

            if (!mesh.isReadable)
            {
                skippedUnreadableCount++;
                continue;
            }

            var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            addedCount++;
        }

        Debug.Log(
            $"[FactoryKeyboard] MeshCollider generation finished. added={addedCount}, skippedUnreadable={skippedUnreadableCount}.",
            this);
#endif
    }

    private void ApplyMaterialOverrides()
    {
        if (factoryInstance == null || materialOverrides == null || materialOverrides.Length == 0)
        {
            return;
        }

        var renderers = factoryInstance.GetComponentsInChildren<Renderer>(true);
        foreach (var materialOverride in materialOverrides)
        {
            if (materialOverride == null ||
                string.IsNullOrWhiteSpace(materialOverride.rendererName) ||
                materialOverride.material == null)
            {
                continue;
            }

            var appliedCount = 0;
            foreach (var renderer in renderers)
            {
                if (!RendererNameMatches(renderer.name, materialOverride.rendererName, materialOverride.matchContains))
                {
                    continue;
                }

                var materials = renderer.sharedMaterials;
                if (materials.Length == 0)
                {
                    continue;
                }

                if (materialOverride.materialSlot >= 0 && materialOverride.materialSlot < materials.Length)
                {
                    materials[materialOverride.materialSlot] = materialOverride.material;
                }
                else
                {
                    for (var i = 0; i < materials.Length; i++)
                    {
                        materials[i] = materialOverride.material;
                    }
                }

                renderer.sharedMaterials = materials;
                appliedCount++;
            }

            if (appliedCount == 0)
            {
                Debug.LogWarning($"Material override target not found: {materialOverride.rendererName}", this);
            }
        }
    }

    private bool RendererNameMatches(string rendererName, string targetName, bool matchContains)
    {
        return matchContains
            ? rendererName.Contains(targetName)
            : rendererName == targetName;
    }

    private void ApplyImageOverlays()
    {
        if (factoryInstance == null)
        {
            return;
        }

        var overlays = GetImageOverlays();
        if (overlays.Length == 0)
        {
            return;
        }

        var renderers = factoryInstance.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"Applying {overlays.Length} image overlays to {renderers.Length} renderers.", this);

        foreach (var overlay in overlays)
        {
            var image = ResolveOverlayImage(overlay);
            if (overlay == null || string.IsNullOrWhiteSpace(overlay.rendererName) || image == null)
            {
                Debug.LogWarning($"Image overlay skipped. Target={overlay?.rendererName}, ImagePath={overlay?.imageAssetPath}", this);
                continue;
            }

            var appliedCount = 0;
            foreach (var renderer in renderers)
            {
                if (!RendererNameMatches(renderer.name, overlay.rendererName, overlay.matchContains))
                {
                    continue;
                }

                CreateImageOverlay(renderer, overlay, image);
                appliedCount++;
            }

            if (appliedCount == 0)
            {
                Debug.LogWarning($"Image overlay target not found: {overlay.rendererName}", this);
            }
            else
            {
                Debug.Log($"Image overlay applied: {overlay.rendererName} -> {image.name} ({appliedCount})", this);
            }
        }
    }

    private ImageOverlay[] GetImageOverlays()
    {
        if (imageOverlays != null && imageOverlays.Length > 0)
        {
            return imageOverlays;
        }

        return new[]
        {
            CreateDefaultOverlay("9g2pd_142", "HubPics/70e19bf7-8ef7-4064-89f9-dedc0045dab3"),
            CreateDefaultOverlay("9g2pd_141", "HubPics/808aafb0-5189-4670-9f70-286eec3d3607"),
            CreateDefaultOverlay("9g2pd_145", "HubPics/b97801c0-ce09-4d2d-8e2f-bfd248407f57"),
            CreateDefaultOverlay("9g2pd_146", "HubPics/f9e6b115-be42-4518-89cb-121dd70135ce"),
            CreateDefaultOverlay("9g2pd_941", "HubPics/info1"),
            CreateFixedSurfaceOverlay("9g2pd_943", "HubPics/info3", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_944", "HubPics/info4", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_945", "HubPics/info6", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_754", "HubPics/info5", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_755", "HubPics/info5", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_757", "HubPics/info4", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_758", "HubPics/info2", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_759", "HubPics/info1", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_760", "HubPics/info6", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_567", "HubPics/info4", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_568", "HubPics/info5", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_569", "HubPics/info2", OverlaySurface.Left),
            CreateFixedSurfaceOverlay("9g2pd_570", "HubPics/info4", OverlaySurface.Left),
            CreateDefaultOverlay("9g2pd_571", "HubPics/info4"),
            CreateDefaultOverlay("9g2pd_572", "HubPics/info1"),
            CreateDefaultOverlay("9g2pd_573", "HubPics/info3"),
            CreateDefaultOverlay("9g2pd_574", "HubPics/info5")
        };
    }

    private ImageOverlay CreateDefaultOverlay(string rendererName, string imageAssetPath)
    {
        return new ImageOverlay
        {
            rendererName = rendererName,
            matchContains = true,
            imageAssetPath = imageAssetPath,
            surface = OverlaySurface.Front,
            useViewerFacingSurface = true,
            fitToSurface = true,
            size = new Vector2(20f, 10f),
            localOffset = Vector3.zero,
            customLocalEulerAngles = Vector3.zero,
            surfaceOffset = 0.8f
        };
    }

    private ImageOverlay CreateFixedSurfaceOverlay(string rendererName, string imageAssetPath, OverlaySurface surface)
    {
        var overlay = CreateDefaultOverlay(rendererName, imageAssetPath);
        overlay.surface = surface;
        overlay.useViewerFacingSurface = false;
        return overlay;
    }

    private static string ToResourcesTexturePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var normalized = path.Replace('\\', '/').Trim();

        const string legacyPrefix = "Assets/HubPics/";
        if (normalized.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = HubPicsResourcesFolder + "/" + normalized.Substring(legacyPrefix.Length);
        }

        const string resourcesPrefix = "Assets/Resources/";
        if (normalized.StartsWith(resourcesPrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring(resourcesPrefix.Length);
        }

        if (!normalized.Contains("/"))
        {
            normalized = HubPicsResourcesFolder + "/" + normalized;
        }

        var lastSlash = normalized.LastIndexOf('/');
        if (lastSlash >= 0)
        {
            var folder = normalized.Substring(0, lastSlash);
            var fileName = Path.GetFileNameWithoutExtension(normalized.Substring(lastSlash + 1));
            return folder + "/" + fileName;
        }

        return Path.GetFileNameWithoutExtension(normalized);
    }

    private Texture2D ResolveOverlayImage(ImageOverlay overlay)
    {
        if (overlay == null)
        {
            return null;
        }

        if (overlay.image != null)
        {
            return overlay.image;
        }

        var resourcesPath = ToResourcesTexturePath(overlay.imageAssetPath);
        if (string.IsNullOrWhiteSpace(resourcesPath))
        {
            return null;
        }

        var texture = Resources.Load<Texture2D>(resourcesPath);
        if (texture == null)
        {
            Debug.LogError(
                $"海报贴图加载失败: Resources/{resourcesPath}（原始路径: {overlay.imageAssetPath}）",
                this);
        }

        return texture;
    }

    private void CreateImageOverlay(Renderer targetRenderer, ImageOverlay overlay, Texture2D image)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = $"Image Overlay - {targetRenderer.name}";

        var collider = quad.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        var quadRenderer = quad.GetComponent<Renderer>();
        quadRenderer.sharedMaterial = CreateOverlayMaterial(image);

        var bounds = targetRenderer.bounds;
        var normal = GetOverlayNormal(targetRenderer, overlay);
        var surfaceOffset = Mathf.Max(overlay.surfaceOffset, 0.8f);
        var center = GetOverlayCenter(bounds, normal, surfaceOffset);
        var overlaySize = overlay.fitToSurface ? GetSurfaceSize(bounds, normal) : overlay.size;

        quad.transform.SetParent(factoryInstance, true);
        quad.transform.position = center + targetRenderer.transform.TransformVector(overlay.localOffset);
        quad.transform.rotation = overlay.surface == OverlaySurface.CustomLocal
            ? targetRenderer.transform.rotation * Quaternion.Euler(overlay.customLocalEulerAngles)
            : Quaternion.LookRotation(-normal, GetOverlayUp(normal));
        quad.transform.localScale = new Vector3(overlaySize.x, overlaySize.y, 1f);
    }

    private Material CreateOverlayMaterial(Texture2D image)
    {
        var shader = Shader.Find("Unlit/Transparent");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Texture");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        var material = new Material(shader)
        {
            name = $"Overlay - {image.name}",
            mainTexture = image
        };
        material.SetInt("_Cull", 0);
        material.renderQueue = 3000;

        return material;
    }

    private Vector2 GetSurfaceSize(Bounds bounds, Vector3 normal)
    {
        const float inset = 0.045f;

        if (Mathf.Abs(normal.x) > 0.5f)
        {
            return new Vector2(bounds.size.z * inset, bounds.size.y * inset);
        }

        if (Mathf.Abs(normal.y) > 0.5f)
        {
            return new Vector2(bounds.size.x * inset, bounds.size.z * inset);
        }

        return new Vector2(bounds.size.x * inset, bounds.size.y * inset);
    }

    private Vector3 GetOverlayNormal(Renderer targetRenderer, ImageOverlay overlay)
    {
        if (overlay.useViewerFacingSurface && overlay.surface != OverlaySurface.CustomLocal)
        {
            return GetViewerFacingNormal(targetRenderer.bounds);
        }

        return GetOverlayNormal(overlay.surface);
    }

    private Vector3 GetViewerFacingNormal(Bounds bounds)
    {
        var viewerPosition = GetVisitorStartPosition();
        var toViewer = viewerPosition - bounds.center;
        toViewer.y = 0f;

        if (toViewer.sqrMagnitude < 0.001f)
        {
            return GetOverlayNormal(OverlaySurface.Front);
        }

        return Mathf.Abs(toViewer.x) > Mathf.Abs(toViewer.z)
            ? (toViewer.x >= 0f ? Vector3.right : Vector3.left)
            : (toViewer.z >= 0f ? Vector3.forward : Vector3.back);
    }

    private Vector3 GetOverlayNormal(OverlaySurface surface)
    {
        switch (surface)
        {
            case OverlaySurface.Back:
                return Vector3.back;
            case OverlaySurface.Right:
                return Vector3.right;
            case OverlaySurface.Left:
                return Vector3.left;
            case OverlaySurface.Top:
                return Vector3.up;
            case OverlaySurface.Bottom:
                return Vector3.down;
            case OverlaySurface.Front:
            case OverlaySurface.CustomLocal:
            default:
                return Vector3.forward;
        }
    }

    private Vector3 GetOverlayUp(Vector3 normal)
    {
        return Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.9f
            ? Vector3.forward
            : Vector3.up;
    }

    private Vector3 GetOverlayCenter(Bounds bounds, Vector3 normal, float surfaceOffset)
    {
        var center = bounds.center;
        center += new Vector3(
            normal.x * (bounds.extents.x + surfaceOffset),
            normal.y * (bounds.extents.y + surfaceOffset),
            normal.z * (bounds.extents.z + surfaceOffset));
        return center;
    }

    private void SetupFirstPersonPlayer()
    {
        var rig = new GameObject("First Person Visitor");
        playerRig = rig.transform;

        controller = rig.AddComponent<CharacterController>();
        controller.height = playerHeight;
        controller.radius = playerRadius;
        controller.center = Vector3.up * (playerHeight * 0.5f);
        controller.slopeLimit = 50f;
        controller.stepOffset = 0.35f;
        controller.detectCollisions = useCollisionWalking;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            var cameraObject = new GameObject("Main Camera");
            playerCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
        }

        playerCamera.fieldOfView = cameraFieldOfView;
        playerCamera.farClipPlane = Mathf.Max(playerCamera.farClipPlane, desiredFactorySize * 4f);
        playerCamera.transform.SetParent(playerRig);
        playerCamera.transform.localPosition = Vector3.up * (playerHeight - 0.15f);
        playerCamera.transform.localRotation = Quaternion.identity;
        ResetVisitorToInterior();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ResetVisitorToInterior()
    {
        if (playerRig == null)
        {
            return;
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        var explicitSpawnPoint = GetExplicitSpawnPoint();
        if (explicitSpawnPoint != null)
        {
            ApplySpawnPoint(explicitSpawnPoint);
        }
        else
        {
            playerRig.position = GetVisitorStartPosition();
            playerRig.rotation = Quaternion.Euler(0f, visitorYaw, 0f);
            cameraPitch = 0f;
        }

        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;
        }

        verticalVelocity = 0f;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private Transform GetExplicitSpawnPoint()
    {
        if (spawnPoint != null)
        {
            return spawnPoint;
        }

        var found = GameObject.Find("SpawnPoint");
        if (found == null)
        {
            return null;
        }

        spawnPoint = found.transform;
        return spawnPoint;
    }

    private void ApplySpawnPoint(Transform point)
    {
        var eyeOffset = spawnPointRepresentsEyePosition
            ? Vector3.up * (playerHeight - 0.15f)
            : Vector3.zero;

        playerRig.position = point.position - eyeOffset;
        playerRig.rotation = Quaternion.Euler(0f, point.eulerAngles.y, 0f);
        cameraPitch = NormalizePitch(point.eulerAngles.x);
    }

    private float NormalizePitch(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return Mathf.Clamp(angle, -80f, 80f);
    }

    private void AdjustVisitorHeight()
    {
        if (playerRig == null)
        {
            return;
        }

        var heightInput = 0f;
        if (Input.GetKey(KeyCode.PageUp))
        {
            heightInput += 1f;
        }

        if (Input.GetKey(KeyCode.PageDown))
        {
            heightInput -= 1f;
        }

        if (Mathf.Approximately(heightInput, 0f))
        {
            return;
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        var heightDelta = heightInput * debugVerticalAdjustSpeed * Time.deltaTime;
        playerRig.position += Vector3.up * heightDelta;
        if (invisibleInteriorFloor != null)
        {
            invisibleInteriorFloor.position += Vector3.up * heightDelta;
        }

        verticalVelocity = 0f;
        if (Time.time >= nextHeightLogTime)
        {
            LogVisitorHeight();
            nextHeightLogTime = Time.time + 0.5f;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private void LogVisitorHeight()
    {
        if (playerRig == null)
        {
            return;
        }

        var rootPosition = playerRig.position;
        var eyePosition = playerCamera == null
            ? rootPosition + Vector3.up * (playerHeight - 0.15f)
            : playerCamera.transform.position;
        var rootRatio = GetBoundsHeightRatio(rootPosition.y);
        var eyeRatio = GetBoundsHeightRatio(eyePosition.y);

        Debug.Log(
            $"Visitor height: rootY={rootPosition.y:F3}, eyeY={eyePosition.y:F3}, " +
            $"rootRatio={rootRatio:F3}, eyeRatio={eyeRatio:F3}, " +
            $"position=({rootPosition.x:F3}, {rootPosition.y:F3}, {rootPosition.z:F3}), " +
            $"eyePosition=({eyePosition.x:F3}, {eyePosition.y:F3}, {eyePosition.z:F3})",
            this);
    }

    private float GetBoundsHeightRatio(float worldY)
    {
        if (factoryBounds.size.y <= 0.001f)
        {
            return 0f;
        }

        return (worldY - factoryBounds.min.y) / factoryBounds.size.y;
    }

    private Vector3 GetVisitorStartPosition()
    {
        if (factoryInstance == null || factoryBounds.size == Vector3.zero)
        {
            return new Vector3(0f, playerHeight + 0.2f, -8f);
        }

        var clampedOffset = new Vector3(
            Mathf.Clamp(visitorStartOffset.x, -0.45f, 0.45f),
            Mathf.Clamp(visitorStartOffset.y, 0f, 0.9f),
            Mathf.Clamp(visitorStartOffset.z, -0.45f, 0.45f));

        var start = new Vector3(
            factoryBounds.center.x + factoryBounds.extents.x * clampedOffset.x,
            factoryBounds.min.y + 0.2f + factoryBounds.size.y * clampedOffset.y,
            factoryBounds.center.z + factoryBounds.extents.z * clampedOffset.z);

        if (useDefaultVisitorStartPosition)
        {
            start.x = DefaultVisitorStartWorldX;
            start.z = DefaultVisitorStartWorldZ;
        }
        else
        {
            start.x = visitorStartWorldXZ.x;
            start.z = visitorStartWorldXZ.y;
        }

        start.y = GetInteriorFloorY(start.x, start.z);
        return start;
    }

    private float GetInteriorFloorY(float x, float z)
    {
        if (useRaycastFloorDetection)
        {
            return FindInteriorFloorY(x, z);
        }

        var rawRatio = useDefaultVisitorStartPosition
            ? DefaultVisitorFloorHeightRatio
            : (forceSecondFloorStart ? secondFloorStartRatio : fixedFloorHeightRatio);
        var floorRatio = Mathf.Clamp(rawRatio, 0.02f, 1.2f);
        return factoryBounds.min.y + factoryBounds.size.y * floorRatio;
    }

    private float FindInteriorFloorY(float x, float z)
    {
        if (factoryBounds.size == Vector3.zero)
        {
            return playerHeight + 0.2f;
        }

        var rayStart = new Vector3(x, factoryBounds.max.y + 5f, z);
        var hits = Physics.RaycastAll(rayStart, Vector3.down, factoryBounds.size.y + 10f);
        if (hits.Length == 0)
        {
            return factoryBounds.min.y + 0.2f;
        }

        var minWalkableY = factoryBounds.min.y + factoryBounds.size.y * 0.06f;
        var maxWalkableY = factoryBounds.min.y + factoryBounds.size.y * 0.55f;
        var bestY = float.PositiveInfinity;

        foreach (var hit in hits)
        {
            var hitTransform = hit.collider.transform;
            if (factoryInstance != null && !hitTransform.IsChildOf(factoryInstance))
            {
                continue;
            }

            if (hit.normal.y < 0.45f)
            {
                continue;
            }

            if (hit.point.y < minWalkableY || hit.point.y > maxWalkableY)
            {
                continue;
            }

            if (hit.point.y < bestY)
            {
                bestY = hit.point.y;
            }
        }

        if (!float.IsPositiveInfinity(bestY))
        {
            return bestY + 0.08f;
        }

        return factoryBounds.min.y + factoryBounds.size.y * 0.18f;
    }

    private void MovePlayer()
    {
        if (playerRig == null)
        {
            return;
        }

        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        var speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
            ? moveSpeed * sprintMultiplier
            : moveSpeed;

        var movement = (playerRig.forward * input.z + playerRig.right * input.x) * speed;

        if (!useCollisionWalking)
        {
            playerRig.position += movement * Time.deltaTime;
            return;
        }

        if (controller == null)
        {
            return;
        }

        if (enableGravity)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = 0f;
        }

        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    private void LookAround()
    {
        if (playerRig == null || playerCamera == null)
        {
            return;
        }

        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        playerRig.Rotate(Vector3.up, mouseX);
        cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -80f, 80f);
        playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void CreateGround()
    {
        if (!createGround)
        {
            return;
        }

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Movement Ground";
        var groundY = factoryInstance == null ? 0f : factoryBounds.min.y - 0.02f;
        ground.transform.position = new Vector3(0f, groundY, 0f);
        ground.transform.localScale = Vector3.one * groundSize * 0.1f;

        var groundCollider = ground.GetComponent<Collider>();
        if (groundCollider != null)
        {
            Destroy(groundCollider);
        }
    }

    private void CreateInteriorFloorCollider()
    {
        if (!createInteriorFloorCollider || factoryBounds.size == Vector3.zero)
        {
            return;
        }

        var floorCenter = GetInvisibleFloorCenter();
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Invisible Interior Walk Floor";
        invisibleInteriorFloor = floor.transform;
        floor.transform.position = floorCenter;
        floor.transform.localScale = new Vector3(
            factoryBounds.size.x * 0.04f,
            1f,
            factoryBounds.size.z * 0.04f);

        var renderer = floor.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    private Vector3 GetInvisibleFloorCenter()
    {
        var explicitSpawnPoint = GetExplicitSpawnPoint();
        if (explicitSpawnPoint != null)
        {
            var rootPosition = spawnPointRepresentsEyePosition
                ? explicitSpawnPoint.position - Vector3.up * (playerHeight - 0.15f)
                : explicitSpawnPoint.position;

            return new Vector3(rootPosition.x, rootPosition.y - 0.02f, rootPosition.z);
        }

        var fallbackPosition = GetVisitorStartPosition();
        return new Vector3(fallbackPosition.x, fallbackPosition.y - 0.02f, fallbackPosition.z);
    }
}
