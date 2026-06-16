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
    public bool forceKnownGoodStartCameraWorldHeight = true;
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
            return;
        }

        float distance = Mathf.Clamp(bounds.extents.magnitude * startDistanceRatio, 12f, desiredFactorySize * 0.35f);
        Vector3 start = bounds.center - Vector3.forward * distance;
        start.y = bounds.min.y + startGroundClearance;

        cameraRig.position = start;
        float cameraWorldHeight = ResolveStartCameraWorldHeight(start.y);
        playerCamera.transform.localPosition = Vector3.up * (cameraWorldHeight - start.y);

        Vector3 lookTarget = bounds.center;
        lookTarget.y = cameraWorldHeight;
        cameraRig.LookAt(lookTarget);
        cameraRig.rotation = Quaternion.Euler(0f, cameraRig.eulerAngles.y, 0f);
        playerCamera.transform.localRotation = Quaternion.identity;
        pitch = 0f;
    }

    private float ResolveStartCameraWorldHeight(float rigWorldY)
    {
        if (!useFixedStartCameraWorldHeight)
        {
            return rigWorldY + ActualEyeHeight;
        }

        return forceKnownGoodStartCameraWorldHeight ? KnownGoodStartCameraWorldHeight : startCameraWorldHeight;
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
