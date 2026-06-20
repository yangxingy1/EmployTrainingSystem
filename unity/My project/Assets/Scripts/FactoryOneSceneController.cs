using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public sealed class FactoryOneSceneController : MonoBehaviour
{
    public const string StaticFactoryName = "Factory 1 Static";
    public const string DefaultFactoryAssetPath = "Assets/Factory/LimeExp_1.FBX";
    // Legacy editor-tuned value; device-ground alignment takes precedence at runtime.
    public const float KnownGoodStartCameraWorldHeight = -85.59f;
    public const string OneShotStartCameraWorldHeightKey = "LeadTrain.OneShotStartCameraWorldHeight";
    public const string OneShotStartCameraPoseKey = "LeadTrain.OneShotStartCameraPose";

    [Header("Factory")]
    public GameObject sceneFactoryRoot;
    public bool spawnFactoryAtRuntime;
    public float desiredFactorySize = 1000f;
    public Vector3 factoryCenter = Vector3.zero;

    [Header("View")]
    public Camera playerCamera;
    [Tooltip("优先使用该 Transform 定位出生点；留空则自动查找场景中的训练设备。")]
    public Transform trainingSpawnDevice;
    public float eyeHeight = 1.7f;
    public float cameraHeightOffset;
    public bool useFixedStartCameraWorldHeight;
    public bool forceKnownGoodStartCameraWorldHeight;
    public float startCameraWorldHeight = KnownGoodStartCameraWorldHeight;
    public float startDistanceRatio = 0.22f;
    public float startGroundClearance = 0.15f;
    [Tooltip("进入 leadTrain1 时，与首台设备（配电柜）的水平距离，需大于引导交互距离才能显示引导线。")]
    public float leadTrainFirstMachineSpawnDistance = 52f;

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

    private bool _pendingGestureReturnPose;
    private bool _initialPlacementDone;

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        _pendingGestureReturnPose = _hasOneShotStartCameraPose;

        ResolveFactory();
        EnsureCameraRig();

        const int maxPlacementAttempts = 12;
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            if (TryResolveLocalTrainingDeviceRoot(out _))
            {
                break;
            }

            yield return null;
        }

        PlaceCameraForFactory();
        yield return null;
        yield return new WaitForFixedUpdate();
        FinalizeInitialCameraPlacement("PostPhysics");

        _initialPlacementDone = true;

        LogCameraHeight("Start");

        if (lockCursorOnPlay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RepositionCameraToTrainingDevice()
    {
        if (_pendingGestureReturnPose)
        {
            return;
        }

        PlaceCameraForFactory();
        FinalizeInitialCameraPlacement("Manual Reposition");
        LogCameraHeight("Reposition");
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
            if (spawnFactoryAtRuntime)
            {
                PrepareFactoryInstance();
            }
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
            GameObject sceneCameraObject = GameObject.Find("Main Camera");
            if (sceneCameraObject != null)
            {
                playerCamera = sceneCameraObject.GetComponent<Camera>();
            }
        }

        if (playerCamera == null)
        {
            GameObject cameraObject = new GameObject("Factory First Person Camera");
            playerCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
        }

        DisableExtraCameras();

        playerCamera.transform.SetParent(cameraRig, false);
        playerCamera.transform.localPosition = Vector3.up * actualEyeHeight;
        playerCamera.transform.localRotation = Quaternion.identity;
        playerCamera.nearClipPlane = 0.05f;
        playerCamera.farClipPlane = Mathf.Max(playerCamera.farClipPlane, desiredFactorySize * 2f);

        Vector3 euler = cameraRig.rotation.eulerAngles;
        pitch = playerCamera.transform.localEulerAngles.x;
        cameraRig.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }

    private void DisableExtraCameras()
    {
        Camera[] cameras = FindObjectsOfType<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera cam = cameras[i];
            if (cam == null || cam == playerCamera)
            {
                continue;
            }

            cam.enabled = false;
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            AudioListener activeListener = playerCamera.GetComponent<AudioListener>();
            if (activeListener == null)
            {
                activeListener = playerCamera.gameObject.AddComponent<AudioListener>();
            }

            activeListener.enabled = true;
        }
    }

    private void PlaceCameraForFactory()
    {
        if (_pendingGestureReturnPose && TryApplyOneShotStartCameraPose(true))
        {
            _pendingGestureReturnPose = false;
            return;
        }

        _pendingGestureReturnPose = false;

        if (TryComputeLeadTrainEntrySpawn(out Vector3 rigPosition, out Vector3 lookTarget))
        {
            PlaceCameraAtLeadTrainEntry(rigPosition, lookTarget);
            return;
        }

        if (TryResolveLeadTrainMachinesCentroid(out Vector3 centroid))
        {
            PlaceCameraNearAnchor(centroid, centroid);
            return;
        }

        if (TryResolveLocalTrainingDeviceRoot(out GameObject deviceRoot))
        {
            PlaceCameraNearTrainingDevice(deviceRoot);
            return;
        }

        Debug.LogWarning("[FactoryOne] Training device not found. Falling back to factory bounds or default spawn.");

        if (factoryInstance == null || !TryGetRenderBounds(factoryInstance, out Bounds bounds))
        {
            ApplyRigWorldPosition(ResolveFallbackStartPosition(), Quaternion.identity, ActualEyeHeight, null);
            return;
        }

        float distance = Mathf.Clamp(bounds.extents.magnitude * startDistanceRatio, 12f, desiredFactorySize * 0.35f);
        Vector3 start = bounds.center - Vector3.forward * distance;
        start.y = ResolveStartRigWorldY(bounds);

        float cameraWorldHeight = ResolveStartCameraWorldHeight(start.y, bounds);
        ApplyRigWorldPosition(start, Quaternion.identity, cameraWorldHeight - start.y, bounds.center, cameraWorldHeight);
    }

    private void FinalizeInitialCameraPlacement(string reason)
    {
        if (_pendingGestureReturnPose)
        {
            return;
        }

        Vector3 anchor;
        Vector3 lookTarget;
        if (TryComputeLeadTrainEntrySpawn(out Vector3 rigPosition, out lookTarget))
        {
            anchor = rigPosition;
        }
        else if (TryResolveLeadTrainMachinesCentroid(out Vector3 centroid))
        {
            anchor = centroid;
            lookTarget = centroid;
        }
        else if (TryResolveLocalTrainingDeviceRoot(out GameObject deviceRoot))
        {
            anchor = deviceRoot.transform.position;
            lookTarget = anchor;
        }
        else
        {
            return;
        }

        float expectedCameraY = ResolveDeviceAlignedCameraWorldHeight(anchor);
        float currentCameraY = playerCamera != null ? playerCamera.transform.position.y : float.NaN;
        if (!IsValidHeight(currentCameraY) || Mathf.Abs(currentCameraY - expectedCameraY) > 1.5f)
        {
            Debug.LogWarning(
                $"[FactoryOne] Camera height mismatch after {reason}: current={currentCameraY:F3}, expected={expectedCameraY:F3}. Re-snapping.");
            if (TryComputeLeadTrainEntrySpawn(out Vector3 retryRig, out Vector3 retryLook))
            {
                PlaceCameraAtLeadTrainEntry(retryRig, retryLook);
            }
            else if (TryResolveLeadTrainMachinesCentroid(out Vector3 recentroid))
            {
                PlaceCameraNearAnchor(recentroid, recentroid);
            }
            else if (TryResolveLocalTrainingDeviceRoot(out GameObject retryDevice))
            {
                PlaceCameraNearTrainingDevice(retryDevice);
            }
        }
    }

    private void PlaceCameraNearTrainingDevice(GameObject deviceRoot)
    {
        PlaceCameraNearAnchor(deviceRoot.transform.position, deviceRoot.transform.position);
    }

    private void PlaceCameraNearAnchor(Vector3 anchorPosition, Vector3 lookTarget)
    {
        float groundY = SampleGroundY(anchorPosition);
        float rigY = groundY + startGroundClearance;
        float cameraWorldHeight = rigY + ActualEyeHeight;

        Vector3 start = new Vector3(anchorPosition.x, rigY, anchorPosition.z - 12f);
        ApplyRigWorldPosition(start, null, ActualEyeHeight, lookTarget, cameraWorldHeight);

        Debug.Log(
            $"Factory Camera [Anchor Aligned]: anchor=({anchorPosition.x:F2}, {anchorPosition.z:F2}), groundY={groundY:F3}, rigY={rigY:F3}, cameraY={cameraWorldHeight:F3}");
    }

    private void PlaceCameraAtLeadTrainEntry(Vector3 rigPosition, Vector3 lookTarget)
    {
        float groundY = SampleGroundY(rigPosition);
        float rigY = groundY + startGroundClearance;
        float cameraWorldHeight = rigY + ActualEyeHeight;
        Vector3 start = new Vector3(rigPosition.x, rigY, rigPosition.z);
        ApplyRigWorldPosition(start, null, ActualEyeHeight, lookTarget, cameraWorldHeight);

        Debug.Log(
            $"Factory Camera [LeadTrain Entry]: rig=({start.x:F2}, {start.z:F2}), look=({lookTarget.x:F2}, {lookTarget.z:F2}), cameraY={cameraWorldHeight:F3}");
    }

    private bool TryComputeLeadTrainEntrySpawn(out Vector3 rigPosition, out Vector3 lookTarget)
    {
        rigPosition = Vector3.zero;
        lookTarget = Vector3.zero;

        if (!TryResolveLeadTrainMachinesCentroid(out Vector3 centroid))
        {
            return false;
        }

        GameObject firstMachine = GameObject.Find(ElectricalControlCabinetBuilder.StaticCabinetName);
        if (!IsValidDeviceRoot(firstMachine))
        {
            return false;
        }

        float groundY = SampleGroundY(centroid);
        float rigY = groundY + startGroundClearance;
        Vector3 flatCentroid = new Vector3(centroid.x, rigY, centroid.z);
        Vector3 flatFirst = new Vector3(firstMachine.transform.position.x, rigY, firstMachine.transform.position.z);

        Vector3 awayFromFirst = flatCentroid - flatFirst;
        if (awayFromFirst.sqrMagnitude < 4f)
        {
            awayFromFirst = new Vector3(0f, 0f, -1f);
        }

        awayFromFirst.Normalize();
        float spawnDistance = Mathf.Max(leadTrainFirstMachineSpawnDistance, 12f);
        Vector3 flatSpawn = flatFirst + awayFromFirst * spawnDistance;
        rigPosition = new Vector3(flatSpawn.x, rigY, flatSpawn.z);
        lookTarget = flatFirst;
        return true;
    }

    private static bool TryResolveLeadTrainMachinesCentroid(out Vector3 centroid)
    {
        centroid = Vector3.zero;

        string[] machineNames =
        {
            ElectricalControlCabinetBuilder.StaticCabinetName,
            BreakerShutdownStationBuilder.StaticStationName,
            CNCTrainingMachineBuilder.StaticMachineName
        };

        Vector3 sum = Vector3.zero;
        int count = 0;
        for (int i = 0; i < machineNames.Length; i++)
        {
            GameObject machine = GameObject.Find(machineNames[i]);
            if (!IsValidDeviceRoot(machine))
            {
                continue;
            }

            sum += machine.transform.position;
            count++;
        }

        if (count < machineNames.Length)
        {
            return false;
        }

        centroid = sum / count;
        return IsValidVector(centroid);
    }

    private void ApplyRigWorldPosition(
        Vector3 rigPosition,
        Quaternion? rigRotation,
        float cameraLocalY,
        Vector3? lookTargetWorld,
        float? lookTargetEyeHeight = null)
    {
        bool controllerWasEnabled = characterController != null && characterController.enabled;
        if (controllerWasEnabled)
        {
            characterController.enabled = false;
        }

        cameraRig.position = rigPosition;
        if (rigRotation.HasValue)
        {
            cameraRig.rotation = rigRotation.Value;
        }

        playerCamera.transform.localPosition = Vector3.up * cameraLocalY;
        playerCamera.transform.localRotation = Quaternion.identity;
        pitch = 0f;

        if (lookTargetWorld.HasValue)
        {
            Vector3 lookTarget = lookTargetWorld.Value;
            lookTarget.y = lookTargetEyeHeight ?? (rigPosition.y + cameraLocalY);
            cameraRig.LookAt(lookTarget);
            cameraRig.rotation = Quaternion.Euler(0f, cameraRig.eulerAngles.y, 0f);
            playerCamera.transform.localRotation = Quaternion.identity;
            pitch = 0f;
        }

        Physics.SyncTransforms();

        if (controllerWasEnabled)
        {
            characterController.enabled = true;
        }
    }

    private float ResolveDeviceAlignedCameraWorldHeight(GameObject deviceRoot)
    {
        return ResolveDeviceAlignedCameraWorldHeight(deviceRoot.transform.position);
    }

    private float ResolveDeviceAlignedCameraWorldHeight(Vector3 anchorPosition)
    {
        float groundY = SampleGroundY(anchorPosition);
        return groundY + startGroundClearance + ActualEyeHeight;
    }

    private float SampleGroundY(Vector3 nearPosition)
    {
        Vector3 origin = nearPosition + Vector3.up * 80f;
        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            240f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        if (hits.Length > 0)
        {
            float bestGroundY = nearPosition.y;
            float bestScore = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                float hitY = hits[i].point.y;
                if (hitY > nearPosition.y + 2f)
                {
                    continue;
                }

                float score = Mathf.Abs(hitY - nearPosition.y);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestGroundY = hitY;
                }
            }

            if (bestScore < 25f)
            {
                return bestGroundY;
            }
        }

        return nearPosition.y;
    }

    private float ResolveStartCameraWorldHeight(float rigWorldY, Bounds factoryBounds)
    {
        float boundsBasedHeight = rigWorldY + ActualEyeHeight;

        if (TryResolveLocalTrainingDeviceRoot(out GameObject deviceRoot))
        {
            float deviceAlignedHeight = ResolveDeviceAlignedCameraWorldHeight(deviceRoot);
            Debug.Log(
                $"Factory Camera World Y [Device Ground]: {deviceAlignedHeight:F6} from {deviceRoot.name}");
            return deviceAlignedHeight;
        }

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

    private float ResolveStartRigWorldY(Bounds factoryBounds)
    {
        if (TryResolveLocalTrainingDeviceRoot(out GameObject deviceRoot))
        {
            float groundY = SampleGroundY(deviceRoot.transform.position);
            float deviceGroundY = groundY + startGroundClearance;
            Debug.Log(
                $"Factory Camera Rig Y [Device Ground Aligned]: {deviceGroundY:F6} " +
                $"from {deviceRoot.name} (cameraY={(deviceGroundY + ActualEyeHeight):F6})");
            return deviceGroundY;
        }

        return factoryBounds.min.y + startGroundClearance;
    }

    private Vector3 ResolveFallbackStartPosition()
    {
        if (TryResolveLocalTrainingDeviceRoot(out GameObject deviceRoot))
        {
            Vector3 devicePosition = deviceRoot.transform.position;
            float groundY = SampleGroundY(devicePosition);
            return new Vector3(devicePosition.x, groundY + startGroundClearance, devicePosition.z - 12f);
        }

        return new Vector3(0f, startGroundClearance, -10f);
    }

    private bool TryResolveLocalTrainingDeviceRoot(out GameObject deviceRoot)
    {
        return TryResolveTrainingDeviceRoot(trainingSpawnDevice, out deviceRoot);
    }

    private static bool TryResolveTrainingDeviceRoot(out GameObject deviceRoot)
    {
        return TryResolveTrainingDeviceRoot(null, out deviceRoot);
    }

    private static bool TryResolveTrainingDeviceRoot(Transform preferredDevice, out GameObject deviceRoot)
    {
        deviceRoot = null;

        if (preferredDevice != null && IsValidHeight(preferredDevice.position.y))
        {
            deviceRoot = preferredDevice.gameObject;
            return true;
        }

        ElectricalControlCabinetBuilder cabinet = Object.FindObjectOfType<ElectricalControlCabinetBuilder>();
        if (IsValidDeviceRoot(cabinet != null ? cabinet.gameObject : null))
        {
            deviceRoot = cabinet.gameObject;
            return true;
        }

        BreakerShutdownStationBuilder breaker = Object.FindObjectOfType<BreakerShutdownStationBuilder>();
        if (IsValidDeviceRoot(breaker != null ? breaker.gameObject : null))
        {
            deviceRoot = breaker.gameObject;
            return true;
        }

        CNCTrainingMachineBuilder cnc = Object.FindObjectOfType<CNCTrainingMachineBuilder>();
        if (IsValidDeviceRoot(cnc != null ? cnc.gameObject : null))
        {
            deviceRoot = cnc.gameObject;
            return true;
        }

        deviceRoot = GameObject.Find(ElectricalControlCabinetBuilder.StaticCabinetName);
        if (IsValidDeviceRoot(deviceRoot))
        {
            return true;
        }

        deviceRoot = GameObject.Find(BreakerShutdownStationBuilder.StaticStationName);
        if (IsValidDeviceRoot(deviceRoot))
        {
            return true;
        }

        deviceRoot = GameObject.Find(CNCTrainingMachineBuilder.StaticMachineName);
        return IsValidDeviceRoot(deviceRoot);
    }

    private static bool IsValidDeviceRoot(GameObject candidate)
    {
        return candidate != null && IsValidHeight(candidate.transform.position.y);
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
            && hasCameraWorldPose
            && !IsReturnPoseNearTrainingDevice(cameraWorldPosition))
        {
            return false;
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

        if (!_hasOneShotStartCameraPose)
        {
            return false;
        }

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

    private static bool IsReturnPoseNearTrainingDevice(Vector3 cameraWorldPosition)
    {
        if (!TryResolveTrainingDeviceRoot(out GameObject deviceRoot))
        {
            Debug.LogWarning(
                "[FactoryOne] Ignored return pose because training device was not found for validation.");
            return false;
        }

        float deviceEye = cameraWorldPosition.y - deviceRoot.transform.position.y;
        if (deviceEye >= 0.6f && deviceEye <= 3.2f)
        {
            return true;
        }

        Debug.LogWarning(
            $"Ignored return pose outside training-device eye height: cameraY={cameraWorldPosition.y:F6}, " +
            $"deviceY={deviceRoot.transform.position.y:F6}, deviceEye={deviceEye:F6}");
        return false;
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
