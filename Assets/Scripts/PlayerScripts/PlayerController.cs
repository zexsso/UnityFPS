using System.Collections.Generic;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 1f;
    [SerializeField] private float maxLookAngle = 80f;

    public float LookSensitivity => lookSensitivity;

    [Header("References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private NetworkAnimator animator;
    [SerializeField] private List<Renderer> renderers = new();

    private CharacterController characterController;
    private Vector3 velocity;
    private GameInput _gameInput;
    private bool _isCrouching;

    public static PlayerController LocalPlayer { get; private set; }

    // Movement variables
    private float verticalRotation = 0f;
    private readonly float gravity = -9.81f;
    private readonly float jumpForce = 1f;


    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
        playerCamera.gameObject.SetActive(isOwner);

        if (isOwner)
        {
            LocalPlayer = this;

            // Load saved sensitivity from PlayerPrefs
            lookSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);

            foreach (var rend in renderers)
            {
                rend.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }
    }

    private void OnDisable()
    {
        if (!isOwner) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        characterController = GetComponent<CharacterController>();
        _gameInput = GameInput.Instance;

        if (playerCamera == null)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (_gameInput == null)
        {
            _gameInput = GameInput.Instance;
            if (_gameInput == null) return;
        }

        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        bool isGrounded = IsGrounded();
        if (isGrounded) animator.SetBool("jump", false);
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Get input from new Input System
        Vector2 moveInput = _gameInput.MoveInput;
        float horizontal = moveInput.x;
        float vertical = moveInput.y;

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        // Apply sprint speed multiplier
        float currentSpeed = _gameInput.SprintHeld && !_isCrouching ? moveSpeed * sprintSpeedMultiplier : moveSpeed;
        characterController.Move(currentSpeed * Time.deltaTime * moveDirection);

        // Movement animations
        animator.SetFloat("vertical", vertical);
        animator.SetFloat("horizontal", horizontal);

        // Jump
        if (_gameInput.JumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetBool("jump", true);
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Crouch - toggle on press
        if (_gameInput.CrouchPressed)
        {
            _isCrouching = !_isCrouching;
            animator.SetBool("crouch", _isCrouching);
        }
    }

    private void HandleRotation()
    {
        Vector2 lookInput = _gameInput.LookInput;
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.03f, Vector3.down, groundCheckDistance);
    }

    public void SetLookSensitivity(float newSensitivity)
    {
        lookSensitivity = newSensitivity;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.03f, Vector3.down * groundCheckDistance);
    }
#endif
}