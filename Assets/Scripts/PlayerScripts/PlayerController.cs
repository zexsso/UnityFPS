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
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Look Settings")]
    [SerializeField] public float lookSensitivity = 1f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private NetworkAnimator animator;
    [SerializeField] private List<Renderer> renderers = new();

    private CharacterController characterController;
    private Vector3 velocity;
    public static PlayerController LocalPlayer { get; private set; }

    // Movements variables
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

        if (playerCamera == null)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        bool isGrounded = IsGrounded();
        if (isGrounded) animator.SetBool("jump", false);
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        characterController.Move(moveSpeed * Time.deltaTime * moveDirection);

        // Movement animations
        animator.SetFloat("vertical", vertical);
        animator.SetFloat("horizontal", horizontal);

        // Jump part
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetBool("jump", true);
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);


        // Crouch part
        if (Input.GetKeyDown(KeyCode.LeftShift)) animator.SetBool("crouch", true);
        else if (Input.GetKeyUp(KeyCode.LeftShift)) animator.SetBool("crouch", false);
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

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