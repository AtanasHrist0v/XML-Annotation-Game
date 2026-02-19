using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField, Range(0f, 20f), Tooltip("Horizontal movement speed (units/second)")]
    private float moveSpeed = 5f;

    [SerializeField, Range(0f, 20f), Tooltip("Small downward force to stick to flat floor")]
    private float downForce = 5f;

    [Header("Look Settings")]
    [SerializeField, Range(0f, 500f), Tooltip("Adjust lower if choppy (try 50-200)")]
    private float mouseSensitivity = 10f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller; // Auto-assigned in Awake

    private float xRotation = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Awake() {
        controller = GetComponent<CharacterController>();
        if (controller == null) {
            Debug.LogError("CharacterController missing on " + gameObject.name + "!");
            enabled = false; // Disable script
            return;
        }

        if (cameraTransform == null) {
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
            if (cameraTransform == null) {
                Debug.LogError("No Camera found as child of " + gameObject.name + "!");
                enabled = false;
                return;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnValidate() {
        if (moveSpeed < 0f) moveSpeed = 0f;
        if (downForce < 0f) downForce = 0f;
        if (mouseSensitivity < 0f) mouseSensitivity = 0f;
    }

    void Update() {
        HandleLook();
    }

    void FixedUpdate() {
        HandleMovement();
    }

    private void HandleMovement() {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= moveSpeed * Time.fixedDeltaTime;
        if (!controller.isGrounded) move.y -= downForce * Time.fixedDeltaTime;

        controller.Move(move);
    }

    private void HandleLook() {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
