using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public sealed class FactoryOneSceneController : MonoBehaviour
{
    public const string StaticFactoryName = "Factory 1 Static";
    public const string DefaultFactoryAssetPath = "Assets/Factory/LimeExp_1.FBX";
    public const float KnownGoodStartCameraWorldHeight = -71.32077f;
    public const string OneShotStartCameraWorldHeightKey = "LeadTrain.OneShotStartCameraWorldHeight";
    public const string OneShotStartCameraPoseKey = "LeadTrain.OneShotStartCameraPose";

    [Header("Factory")]
    public GameObject sceneFactoryRoot;
    public bool spawnFactoryAtRuntime;
    public float desiredFactorySize = 1000f;
    public Vector3 factoryCenter = Vector3.zero;

    [Header("View")]
    public Camera playerCamera;
    public float eyeHeight = 1.7f;
    public float cameraHeightOffset;
    public bool useFixedStartCameraWorldHeight = true;
    public bool forceKnownGoodStartCameraWorldHeight;
    public float startCameraWorldHeight = KnownGoodStartCameraWorldHeight;
    public float startDistanceRatio = 0.22f;
    public float startGroundClearance = 0.15f;

    [Header("Movement")]
    [Tooltip("为 false 时 E 键不再用于上升，便于引导场景用 E 与设备交互。仍可用 PageUp/Q 调整高度。")]
    public bool allowInteractFly = true;
    public float moveSpeed = 12f;
    public float verticalMoveSpeed = 1f;
    public float fastMoveMultiplier = 4f;
    public float lookSensitivity = 2.2f;
    public float heightLogInterval = 0.25f;
    public KeyCode printHeightKey = KeyCode.H;
    public bool lockCursorOnPlay = true;

    private GameObject factoryInstance;
    private Transform cameraRig;
    private CharacterController characterController;
    private float pitch;
    private float nextHeightLogTime;
    private float ActualEyeHeight => Mathf.Max(0.1f, eyeHeight + cameraHeightOffset);

    private static bool _hasOneShotStartCameraPose;
    private static Vector3 _cachedRigPosition;
    private static Quaternion _cachedRigRotation;
    private static Vector3 _cachedCameraLocalPosition;
    private static Quaternion _cachedCameraLocalRotation;
    private static bool _hasCachedCameraWorldPose;
    private static Vector3 _cachedCameraWorldPosition;
    private static Quaternion _cachedCameraWorldRotation;

    private void Start()
    {
        ResolveFactory();
        EnsureCameraRig();
        PlaceCameraForFactory();
        LogCameraHeight("Start");

        if (lockCursorOnPlay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        UpdateCursorLock();
        UpdateLook();
        UpdateMove();
        UpdateHeightLogging();
    }

    private void ResolveFactory()
    {
        factoryInstance = sceneFactoryRoot != null ? sceneFactoryRoot : GameObject.Find(StaticFactoryName);

        if (factoryInstance != null)
        {
            sceneFactoryRoot = factoryInstance;
            PrepareFactoryInstance();
            return;
        }

        if (!spawnFactoryAtRuntime)
        {
            Debug.LogWarning("Factory 1 scene root was not found. Run Tools > Factory > Create Static Factory 1 In Scene first.");
            return;
        }

#if UNITY_EDITOR
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultFactoryAssetPath);
        if (prefab == null)
        {
            Debug.LogError($"Could not load factory FBX at {DefaultFactoryAssetPath}.");
            return;
        }

        factoryInstance = Instantiate(prefab);
        factoryInstance.name = StaticFactoryName;
        sceneFactoryRoot = factoryInstance;
        PrepareFactoryInstance();
#else
        Debug.LogWarning("Runtime factory spawning is only available in the Editor for this FBX asset path. Use the static scene builder before making a build.");
#endif
    }

    public void PrepareFactoryInstance()
    {
        if (factoryInstance == null)
        {
            return;
        }

        factoryInstance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        factoryInstance.transform.localScale = Vector3.one;

        if (TryGetRenderBounds(factoryInstance, out Bounds bounds))
        {
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxSize > 0.001f)
            {
                factoryInstance.transform.localScale = Vector3.one * (desiredFactorySize / maxSize);
            }

            if (TryGetRenderBounds(factoryInstance, out Bounds scaledBounds))
            {
                factoryInstance.transform.position += factoryCenter - scaledBounds.center;
            }
        }
    }

    private void EnsureCameraRig()
    {
        cameraRig = transform;
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }

        float actualEyeHeight = ActualEyeHeight;
        characterController.height = Mathf.Max(actualEyeHeight, 1.2f);
        characterController.radius = 0.35f;
        characterController.center = Vector3.up * (characterController.height * 0.5f);
        characterController.stepOffset = 0.4f;
        characterController.slopeLimit = 60f;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            GameObject cameraObject = new GameObject("Factory First Person Camera");
            playerCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
        }

        playerCamera.transform.SetParent(cameraRig, false);
        playerCamera.transform.localPosition = Vector3.up * actualEyeHeight;
        playerCamera.transform.localRotation = Quaternion.identity;
        playerCamera.nearClipPlane = 0.05f;
        playerCamera.farClipPlane = Mathf.Max(playerCamera.farClipPlane, desiredFactorySize * 2f);

        Vector3 euler = cameraRig.rotation.eulerAngles;
        pitch = playerCamera.transform.localEulerAngles.x;
        cameraRig.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }

    private void PlaceCameraForFactory()
    {
        if (factoryInstance == null || !TryGetRenderBounds(factoryInstance, out Bounds bounds))
        {
            cameraRig.position = new Vector3(0f, 2f, -10f);
            cameraRig.rotation = Quaternion.identity;
            TryApplyOneShotStartCameraPose(false);
            return;
        }

        float distance = Mathf.Clamp(bounds.extents.magnitude * startDistanceRatio, 12f, desiredFactorySize * 0.35f);
        Vector3 start = bounds.center - Vector3.forward * distance;
        start.y = bounds.min.y + startGroundClearance;

        cameraRig.position = start;
        float cameraWorldHeight = ResolveStartCameraWorldHeight(start.y, bounds);
        playerCamera.transform.localPosition = Vector3.up * (cameraWorldHeight - start.y);

        Vector3 lookTarget = bounds.center;
        lookTarget.y = cameraWorldHeight;
        cameraRig.LookAt(lookTarget);
        cameraRig.rotation = Quaternion.Euler(0f, cameraRig.eulerAngles.y, 0f);
        playerCamera.transform.localRotation = Quaternion.identity;
        pitch = 0f;

        TryApplyOneShotStartCameraPose(true, bounds);
    }

    private float ResolveStartCameraWorldHeight(float rigWorldY, Bounds factoryBounds)
    {
        float boundsBasedHeight = rigWorldY + ActualEyeHeight;
        if (TryConsumeOneShotStartCameraWorldHeight(out float savedCameraWorldHeight))
        {
            float savedEyeHeight = savedCameraWorldHeight - rigWorldY;
            if (savedEyeHeight >= 0.6f
                && savedEyeHeight <= 3.2f
                && IsCameraWorldHeightInsideFactory(factoryBounds, savedCameraWorldHeight))
            {
                return savedCameraWorldHeight;
            }

            Debug.LogWarning(
                $"Ignored stale gesture return camera height: {savedCameraWorldHeight:F6} " +
                $"for rigY {rigWorldY:F6}");
        }

        if (!useFixedStartCameraWorldHeight)
        {
            return boundsBasedHeight;
        }

        float configuredHeight = forceKnownGoodStartCameraWorldHeight ? KnownGoodStartCameraWorldHeight : startCameraWorldHeight;
        if (IsConfiguredStartHeightUsable(rigWorldY, factoryBounds, configuredHeight))
        {
            return configuredHeight;
        }

        Debug.LogWarning(
            $"Ignored configured factory camera height {configuredHeight:F6}; " +
            $"using bounds-based height {boundsBasedHeight:F6} for rigY {rigWorldY:F6}.");
        return boundsBasedHeight;
    }

    public static void RememberOneShotStartCameraWorldHeight(float cameraWorldHeight)
    {
        if (!IsValidHeight(cameraWorldHeight))
        {
            return;
        }

        PlayerPrefs.SetFloat(OneShotStartCameraWorldHeightKey, cameraWorldHeight);
        PlayerPrefs.Save();
    }

    public static void RememberOneShotStartCameraPose(Transform rig, Camera camera)
    {
        if (rig == null || camera == null)
        {
            return;
        }

        RememberOneShotStartCameraPose(
            rig.position,
            rig.rotation,
            camera.transform.localPosition,
            camera.transform.localRotation,
            camera.transform.position,
            camera.transform.rotation);
    }

    public static void RememberOneShotStartCameraPose(
        Vector3 rigPosition,
        Quaternion rigRotation,
        Vector3 cameraLocalPosition,
        Quaternion cameraLocalRotation,
        Vector3 cameraWorldPosition,
        Quaternion cameraWorldRotation)
    {
        if (!IsValidVector(rigPosition)
            || !IsValidQuaternion(rigRotation)
            || !IsValidVector(cameraLocalPosition)
            || !IsValidQuaternion(cameraLocalRotation)
            || !IsValidVector(cameraWorldPosition)
            || !IsValidQuaternion(cameraWorldRotation))
        {
            return;
        }

        _hasOneShotStartCameraPose = true;
        _cachedRigPosition = rigPosition;
        _cachedRigRotation = rigRotation;
        _cachedCameraLocalPosition = cameraLocalPosition;
        _cachedCameraLocalRotation = cameraLocalRotation;
        _hasCachedCameraWorldPose = true;
        _cachedCameraWorldPosition = cameraWorldPosition;
        _cachedCameraWorldRotation = cameraWorldRotation;

        PlayerPrefs.SetInt(OneShotStartCameraPoseKey + ".HasPose", 1);
        PlayerPrefs.SetInt(OneShotStartCameraPoseKey + ".HasWorldPose", 1);
        SetVector3(OneShotStartCameraPoseKey + ".RigPosition", rigPosition);
        SetQuaternion(OneShotStartCameraPoseKey + ".RigRotation", rigRotation);
        SetVector3(OneShotStartCameraPoseKey + ".CameraLocalPosition", cameraLocalPosition);
        SetQuaternion(OneShotStartCameraPoseKey + ".CameraLocalRotation", cameraLocalRotation);
        SetVector3(OneShotStartCameraPoseKey + ".CameraWorldPosition", cameraWorldPosition);
        SetQuaternion(OneShotStartCameraPoseKey + ".CameraWorldRotation", cameraWorldRotation);
        PlayerPrefs.DeleteKey(OneShotStartCameraWorldHeightKey);
        PlayerPrefs.Save();
    }

    public static void ClearOneShotStartCameraReturnOverride()
    {
        ClearOneShotStartCameraPose();
    }

    private static bool TryConsumeOneShotStartCameraWorldHeight(out float cameraWorldHeight)
    {
        cameraWorldHeight = 0f;
        if (!PlayerPrefs.HasKey(OneShotStartCameraWorldHeightKey))
        {
            return false;
        }

        cameraWorldHeight = PlayerPrefs.GetFloat(OneShotStartCameraWorldHeightKey);
        PlayerPrefs.DeleteKey(OneShotStartCameraWorldHeightKey);
        PlayerPrefs.Save();
        return IsValidHeight(cameraWorldHeight);
    }

    private bool TryApplyOneShotStartCameraPose(bool normalizeFirstPersonPose)
    {
        return TryApplyOneShotStartCameraPose(normalizeFirstPersonPose, default(Bounds));
    }

    private bool TryApplyOneShotStartCameraPose(bool normalizeFirstPersonPose, Bounds factoryBounds)
    {
        if (!TryConsumeOneShotStartCameraPose(
            out Vector3 rigPosition,
            out Quaternion rigRotation,
            out Vector3 cameraLocalPosition,
            out Quaternion cameraLocalRotation,
            out Vector3 cameraWorldPosition,
            out Quaternion cameraWorldRotation))
        {
            return false;
        }

        float savedPitch = NormalizePitch(cameraLocalRotation.eulerAngles.x);
        if (normalizeFirstPersonPose)
        {
            rigRotation = Quaternion.Euler(0f, rigRotation.eulerAngles.y, 0f);
            cameraLocalRotation = Quaternion.Euler(savedPitch, 0f, 0f);
        }

        bool hasCameraWorldPose = IsValidVector(cameraWorldPosition) && IsValidQuaternion(cameraWorldRotation);
        if (hasCameraWorldPose)
        {
            rigPosition = cameraWorldPosition - (rigRotation * cameraLocalPosition);
        }

        if (normalizeFirstPersonPose
            && factoryBounds.size != Vector3.zero
            && !IsReturnPoseInsideFactory(factoryBounds, rigPosition, cameraLocalPosition, cameraWorldPosition))
        {
            Debug.LogWarning(
                $"Ignored stale gesture return camera pose: rig=({rigPosition.x:F2}, {rigPosition.y:F2}, {rigPosition.z:F2}), " +
                $"cameraY={cameraWorldPosition.y:F6}");
            return false;
        }

        bool controllerWasEnabled = characterController != null && characterController.enabled;
        if (controllerWasEnabled)
        {
            characterController.enabled = false;
        }

        cameraRig.SetPositionAndRotation(rigPosition, rigRotation);
        playerCamera.transform.localPosition = cameraLocalPosition;
        playerCamera.transform.localRotation = cameraLocalRotation;

        if (controllerWasEnabled)
        {
            characterController.enabled = true;
        }

        pitch = savedPitch;
        Physics.SyncTransforms();
        Debug.Log(
            $"Factory Camera Pose [Gesture Return]: rig=({rigPosition.x:F2}, {rigPosition.y:F2}, {rigPosition.z:F2}), " +
            $"cameraY={playerCamera.transform.position.y:F6}");
        return true;
    }

    private static bool TryConsumeOneShotStartCameraPose(
        out Vector3 rigPosition,
        out Quaternion rigRotation,
        out Vector3 cameraLocalPosition,
        out Quaternion cameraLocalRotation,
        out Vector3 cameraWorldPosition,
        out Quaternion cameraWorldRotation)
    {
        rigPosition = Vector3.zero;
        rigRotation = Quaternion.identity;
        cameraLocalPosition = Vector3.zero;
        cameraLocalRotation = Quaternion.identity;
        cameraWorldPosition = Vector3.zero;
        cameraWorldRotation = Quaternion.identity;

        if (_hasOneShotStartCameraPose)
        {
            rigPosition = _cachedRigPosition;
            rigRotation = _cachedRigRotation;
            cameraLocalPosition = _cachedCameraLocalPosition;
            cameraLocalRotation = _cachedCameraLocalRotation;
            cameraWorldPosition = _cachedCameraWorldPosition;
            cameraWorldRotation = _cachedCameraWorldRotation;
            bool hasWorldPose = _hasCachedCameraWorldPose
                && IsValidVector(cameraWorldPosition)
                && IsValidQuaternion(cameraWorldRotation);
            ClearOneShotStartCameraPose();

            return IsValidVector(rigPosition)
                && IsValidQuaternion(rigRotation)
                && IsValidVector(cameraLocalPosition)
                && IsValidQuaternion(cameraLocalRotation)
                && hasWorldPose;
        }

        if (PlayerPrefs.GetInt(OneShotStartCameraPoseKey + ".HasPose", 0) != 1)
        {
            return false;
        }

        if (PlayerPrefs.GetInt(OneShotStartCameraPoseKey + ".HasWorldPose", 0) != 1)
        {
            ClearOneShotStartCameraPose();
            return false;
        }

        rigPosition = GetVector3(OneShotStartCameraPoseKey + ".RigPosition");
        rigRotation = GetQuaternion(OneShotStartCameraPoseKey + ".RigRotation");
        cameraLocalPosition = GetVector3(OneShotStartCameraPoseKey + ".CameraLocalPosition");
        cameraLocalRotation = GetQuaternion(OneShotStartCameraPoseKey + ".CameraLocalRotation");
        cameraWorldPosition = GetVector3(OneShotStartCameraPoseKey + ".CameraWorldPosition");
        cameraWorldRotation = GetQuaternion(OneShotStartCameraPoseKey + ".CameraWorldRotation");
        ClearOneShotStartCameraPose();

        return IsValidVector(rigPosition)
            && IsValidQuaternion(rigRotation)
            && IsValidVector(cameraLocalPosition)
            && IsValidQuaternion(cameraLocalRotation)
            && IsValidVector(cameraWorldPosition)
            && IsValidQuaternion(cameraWorldRotation);
    }

    private static void ClearOneShotStartCameraPose()
    {
        _hasOneShotStartCameraPose = false;
        _hasCachedCameraWorldPose = false;

        string[] suffixes =
        {
            ".HasPose",
            ".HasWorldPose",
            ".RigPosition.x",
            ".RigPosition.y",
            ".RigPosition.z",
            ".RigRotation.x",
            ".RigRotation.y",
            ".RigRotation.z",
            ".RigRotation.w",
            ".CameraLocalPosition.x",
            ".CameraLocalPosition.y",
            ".CameraLocalPosition.z",
            ".CameraLocalRotation.x",
            ".CameraLocalRotation.y",
            ".CameraLocalRotation.z",
            ".CameraLocalRotation.w",
            ".CameraWorldPosition.x",
            ".CameraWorldPosition.y",
            ".CameraWorldPosition.z",
            ".CameraWorldRotation.x",
            ".CameraWorldRotation.y",
            ".CameraWorldRotation.z",
            ".CameraWorldRotation.w"
        };

        for (int i = 0; i < suffixes.Length; i++)
        {
            PlayerPrefs.DeleteKey(OneShotStartCameraPoseKey + suffixes[i]);
        }

        PlayerPrefs.DeleteKey(OneShotStartCameraWorldHeightKey);
        PlayerPrefs.Save();
    }

    private static void SetVector3(string prefix, Vector3 value)
    {
        PlayerPrefs.SetFloat(prefix + ".x", value.x);
        PlayerPrefs.SetFloat(prefix + ".y", value.y);
        PlayerPrefs.SetFloat(prefix + ".z", value.z);
    }

    private static Vector3 GetVector3(string prefix)
    {
        return new Vector3(
            PlayerPrefs.GetFloat(prefix + ".x"),
            PlayerPrefs.GetFloat(prefix + ".y"),
            PlayerPrefs.GetFloat(prefix + ".z"));
    }

    private static void SetQuaternion(string prefix, Quaternion value)
    {
        PlayerPrefs.SetFloat(prefix + ".x", value.x);
        PlayerPrefs.SetFloat(prefix + ".y", value.y);
        PlayerPrefs.SetFloat(prefix + ".z", value.z);
        PlayerPrefs.SetFloat(prefix + ".w", value.w);
    }

    private static Quaternion GetQuaternion(string prefix)
    {
        return new Quaternion(
            PlayerPrefs.GetFloat(prefix + ".x"),
            PlayerPrefs.GetFloat(prefix + ".y"),
            PlayerPrefs.GetFloat(prefix + ".z"),
            PlayerPrefs.GetFloat(prefix + ".w", 1f));
    }

    private static bool IsValidHeight(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static bool IsValidVector(Vector3 value)
    {
        return IsValidHeight(value.x) && IsValidHeight(value.y) && IsValidHeight(value.z);
    }

    private bool IsConfiguredStartHeightUsable(float rigWorldY, Bounds factoryBounds, float cameraWorldHeight)
    {
        float eye = cameraWorldHeight - rigWorldY;
        return eye >= 0.6f
            && eye <= 3.2f
            && IsCameraWorldHeightInsideFactory(factoryBounds, cameraWorldHeight);
    }

    private static bool IsCameraWorldHeightInsideFactory(Bounds factoryBounds, float cameraWorldHeight)
    {
        if (factoryBounds.size == Vector3.zero)
        {
            return IsValidHeight(cameraWorldHeight);
        }

        float padding = Mathf.Max(2f, factoryBounds.size.y * 0.04f);
        return cameraWorldHeight >= factoryBounds.min.y - padding
            && cameraWorldHeight <= factoryBounds.max.y + padding;
    }

    private bool IsReturnPoseInsideFactory(Bounds factoryBounds, Vector3 rigPosition, Vector3 cameraLocalPosition, Vector3 cameraWorldPosition)
    {
        if (!IsValidVector(rigPosition) || !IsValidVector(cameraLocalPosition) || !IsValidVector(cameraWorldPosition))
        {
            return false;
        }

        float eye = cameraWorldPosition.y - rigPosition.y;
        if (eye < 0.6f || eye > 3.2f)
        {
            return false;
        }

        Bounds padded = factoryBounds;
        float horizontalPadding = Mathf.Max(8f, factoryBounds.extents.magnitude * 0.18f);
        float verticalPadding = Mathf.Max(2f, factoryBounds.size.y * 0.04f);
        padded.Expand(new Vector3(horizontalPadding, verticalPadding, horizontalPadding));
        return padded.Contains(cameraWorldPosition);
    }

    private static bool IsValidQuaternion(Quaternion value)
    {
        return IsValidHeight(value.x)
            && IsValidHeight(value.y)
            && IsValidHeight(value.z)
            && IsValidHeight(value.w)
            && value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w > 0.0001f;
    }

    private static float NormalizePitch(float eulerX)
    {
        float normalized = eulerX > 180f ? eulerX - 360f : eulerX;
        return Mathf.Clamp(normalized, -85f, 85f);
    }

    private void UpdateCursorLock()
    {
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

    private void UpdateLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        cameraRig.Rotate(Vector3.up, mouseX, Space.World);
        pitch = Mathf.Clamp(pitch - mouseY, -85f, 85f);
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void UpdateMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        float y = 0f;

        if (Input.GetKey(KeyCode.PageUp) || (allowInteractFly && Input.GetKey(KeyCode.E)))
        {
            y += 1f;
        }

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.PageDown))
        {
            y -= 1f;
        }

        Vector3 localMove = new Vector3(x, 0f, z);
        if (localMove.sqrMagnitude > 1f)
        {
            localMove.Normalize();
        }

        bool fastMove = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speedScale = fastMove ? fastMoveMultiplier : 1f;
        Vector3 worldMove = cameraRig.TransformDirection(localMove) * moveSpeed * speedScale * Time.deltaTime;
        worldMove += Vector3.up * y * verticalMoveSpeed * speedScale * Time.deltaTime;
        characterController.Move(worldMove);

        if (!Mathf.Approximately(y, 0f) && Time.unscaledTime >= nextHeightLogTime)
        {
            LogCameraHeight("Vertical Move");
            nextHeightLogTime = Time.unscaledTime + Mathf.Max(0.05f, heightLogInterval);
        }
    }

    private void UpdateHeightLogging()
    {
        if (Input.GetKeyDown(printHeightKey))
        {
            LogCameraHeight("Manual");
        }
    }

    private void LogCameraHeight(string reason)
    {
        if (playerCamera == null)
        {
            return;
        }

        Debug.Log(
            $"Factory Camera World Y [{reason}]: {playerCamera.transform.position.y:F6} " +
            $"(configured: {startCameraWorldHeight:F6}, forced: {forceKnownGoodStartCameraWorldHeight})");
    }

    private static bool TryGetRenderBounds(GameObject root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bounds = default;

        if (renderers.Length == 0)
        {
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return true;
    }
}
