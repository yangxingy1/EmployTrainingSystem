using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform followCamera;
    public float moveSpeed = 4.5f;
    public float lookSensitivity = 0.12f;
    public float cameraDistance = 5.2f;
    public float cameraHeight = 2.4f;
    public float cameraSmoothTime = 0.08f;

    public event Action InteractPressed;

    CharacterController _controller;
    InputAction _moveAction;
    InputAction _lookAction;
    InputAction _interactAction;
    Vector2 _lookAngles = new Vector2(0f, 18f);
    Vector3 _cameraVelocity;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();

        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        _lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
        _interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
        _interactAction.performed += _ =>
        {
            Debug.Log("[PlayerController] Interact pressed");
            InteractPressed?.Invoke();
        };
    }

    void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _interactAction.Enable();
    }

    void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _interactAction.Disable();
    }

    void Update()
    {
        UpdateLook();
        UpdateMovement();
    }

    void LateUpdate()
    {
        UpdateCamera();
    }

    void UpdateLook()
    {
        var delta = _lookAction.ReadValue<Vector2>() * lookSensitivity;
        _lookAngles.x += delta.x;
        _lookAngles.y = Mathf.Clamp(_lookAngles.y - delta.y, -18f, 55f);
        transform.rotation = Quaternion.Euler(0f, _lookAngles.x, 0f);
    }

    void UpdateMovement()
    {
        var input = _moveAction.ReadValue<Vector2>();
        var forward = Quaternion.Euler(0f, _lookAngles.x, 0f) * Vector3.forward;
        var right = Quaternion.Euler(0f, _lookAngles.x, 0f) * Vector3.right;
        var move = (forward * input.y + right * input.x);
        if (move.sqrMagnitude > 1f) move.Normalize();

        var velocity = move * moveSpeed;
        velocity.y = Physics.gravity.y;
        _controller.Move(velocity * Time.deltaTime);
    }

    void UpdateCamera()
    {
        if (followCamera == null) return;

        var pivot = transform.position + Vector3.up * cameraHeight;
        var rotation = Quaternion.Euler(_lookAngles.y, _lookAngles.x, 0f);
        var desiredPosition = pivot - rotation * Vector3.forward * cameraDistance;

        followCamera.position = Vector3.SmoothDamp(followCamera.position, desiredPosition, ref _cameraVelocity, cameraSmoothTime);
        followCamera.rotation = Quaternion.LookRotation(pivot - followCamera.position, Vector3.up);
    }
}
