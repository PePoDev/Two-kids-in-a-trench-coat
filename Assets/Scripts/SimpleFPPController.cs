using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple First-Person Controller with split-screen support.
/// No Cinemachine, no StarterAssets dependencies.
/// 
/// Controls:
/// - A/D: Rotate top camera (independent view)
/// - Left/Right Arrow: Rotate bottom camera + player (movement direction)
/// - Up/Down Arrow: Move forward/backward (follows bottom camera direction)
/// - E or Numpad0: Switch between top-down and first-person modes
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleFPPController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float gravity = -15f;
    
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 120f;
    
    [Header("Cameras")]
    [SerializeField] private Camera topCamera;    // Top split - independent rotation (A/D)
    [SerializeField] private Camera bottomCamera; // Bottom split - controls movement direction (Arrow)
    [SerializeField] private Transform cameraMount; // Where cameras are positioned (head level)
    
    [Header("Camera Offset")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);
    
    // Components
    private CharacterController controller;
    
    // State
    private float topCameraYRotation = 0f;
    private float bottomCameraYRotation = 0f; // This also controls player rotation
    private float verticalVelocity = 0f;
    private bool isGrounded = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        SetupSplitScreen();
        InitializeRotations();
    }

    void Update()
    {
        GroundCheck();
        HandleRotation();
        HandleMovement();
        UpdateCameras();
    }

    private void SetupSplitScreen()
    {
        if (topCamera != null)
        {
            topCamera.rect = new Rect(0f, 0.5f, 1f, 0.5f); // Top half
        }
        
        if (bottomCamera != null)
        {
            bottomCamera.rect = new Rect(0f, 0f, 1f, 0.5f); // Bottom half
        }
    }

    private void InitializeRotations()
    {
        float initialY = transform.eulerAngles.y;
        topCameraYRotation = initialY;
        bottomCameraYRotation = initialY;
    }

    private void GroundCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
    }

    private void HandleRotation()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Top camera rotation (A/D) - independent
        float topRotate = 0f;
        if (keyboard[Key.A].isPressed) topRotate = -1f;
        if (keyboard[Key.D].isPressed) topRotate = 1f;
        
        if (topRotate != 0f)
        {
            topCameraYRotation += topRotate * rotationSpeed * Time.deltaTime;
        }
        
        // Bottom camera + player rotation (Arrow keys)
        float bottomRotate = 0f;
        if (keyboard[Key.LeftArrow].isPressed) bottomRotate = -1f;
        if (keyboard[Key.RightArrow].isPressed) bottomRotate = 1f;
        
        if (bottomRotate != 0f)
        {
            bottomCameraYRotation += bottomRotate * rotationSpeed * Time.deltaTime;
            
            // Player rotates with bottom camera
            transform.rotation = Quaternion.Euler(0f, bottomCameraYRotation, 0f);
        }
    }

    private void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Movement input (Arrow Up/Down)
        float moveInput = 0f;
        if (keyboard[Key.UpArrow].isPressed) moveInput = 1f;
        if (keyboard[Key.DownArrow].isPressed) moveInput = -1f;
        
        // Calculate movement direction based on bottom camera (player) rotation
        Vector3 moveDirection = transform.forward * moveInput;
        
        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;
        moveDirection.y = verticalVelocity;
        
        // Move
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void UpdateCameras()
    {
        Vector3 cameraPosition = transform.position + cameraOffset;
        
        // Use cameraMount if assigned, otherwise use offset
        if (cameraMount != null)
        {
            cameraPosition = cameraMount.position;
        }
        
        // Update top camera
        if (topCamera != null)
        {
            topCamera.transform.position = cameraPosition;
            topCamera.transform.rotation = Quaternion.Euler(0f, topCameraYRotation, 0f);
        }
        
        // Update bottom camera
        if (bottomCamera != null)
        {
            bottomCamera.transform.position = cameraPosition;
            bottomCamera.transform.rotation = Quaternion.Euler(0f, bottomCameraYRotation, 0f);
        }
    }

    // ===== PUBLIC API =====
    
    /// <summary>
    /// Set initial rotation for both cameras
    /// </summary>
    public void SetRotation(float yRotation)
    {
        topCameraYRotation = yRotation;
        bottomCameraYRotation = yRotation;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
    
    /// <summary>
    /// Enable/disable this controller
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (controller != null) controller.enabled = enabled;
    }

    public float TopCameraRotation => topCameraYRotation;
    public float BottomCameraRotation => bottomCameraYRotation;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw camera position
        Vector3 camPos = transform.position + cameraOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(camPos, 0.2f);
        
        // Draw forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(camPos, transform.forward * 2f);
    }
#endif
}
