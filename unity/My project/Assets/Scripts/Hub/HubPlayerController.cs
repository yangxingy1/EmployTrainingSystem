using UnityEngine;

/// <summary>
/// 第一人称控制器：WASD 移动 + 鼠标环视。
/// 用于化工管道培训场景的自由走动。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class HubPlayerController : MonoBehaviour
{
    [Header("移动")]
    public float walkSpeed = 4f;

    [Header("视角")]
    public float mouseSensitivity = 2f;
    public float lookUpClamp = 70f;          // 垂直视角限制（度）

    CharacterController _cc;
    Camera _cam;
    float _pitch;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _cam = GetComponentInChildren<Camera>();

        if (_cam == null)
        {
            Debug.LogError("[HubPlayerController] 需要子物体上有 Camera 组件");
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (_cam == null) return;

        // ── 鼠标环视 ──
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _pitch = Mathf.Clamp(_pitch - my, -lookUpClamp, lookUpClamp);
        _cam.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mx);

        // ── WASD 移动 ──
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        _cc.SimpleMove(move * walkSpeed);

        // ── Escape 释放鼠标 ──
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // 点击重新锁定
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
