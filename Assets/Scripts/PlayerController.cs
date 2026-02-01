using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    
    [Header("Top-Down Settings")]
    [SerializeField] private float topDownCameraHeight = 10f;
    [SerializeField] private float topDownCameraAngle = 90f;
    
    [Header("First Person Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalLookLimit = 80f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float firstPersonCameraHeight = 1.6f;
    
    [Header("View Toggle")]
    [SerializeField] private KeyCode viewToggleKey = KeyCode.V;
    
    // Internal state
    private bool isFirstPersonView = false;
    private Vector2 moveInput;
    private bool isSprinting = false;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    
    // Components
    private Rigidbody2D rb2D;
    private Rigidbody rb3D;
    private CharacterController characterController;
    
    void Start()
    {
        // Try to get components
        rb2D = GetComponent<Rigidbody2D>();
        rb3D = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        
        // Setup camera if not assigned
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }
        
        // Initialize in top-down view
        SetTopDownView();
    }

    void Update()
    {
        // Handle input
        HandleInput();
        
        // Handle view toggle
        if (Input.GetKeyDown(viewToggleKey))
        {
            ToggleView();
        }
        
        // Handle camera rotation in first person
        if (isFirstPersonView)
        {
            HandleFirstPersonCamera();
        }
    }
    
    void FixedUpdate()
    {
        // Handle movement based on current view
        if (isFirstPersonView)
        {
            HandleFirstPersonMovement();
        }
        else
        {
            HandleTopDownMovement();
        }
    }
    
    private void HandleInput()
    {
        // Movement input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;
        
        // Sprint input
        isSprinting = Input.GetKey(KeyCode.LeftShift);
    }
    
    private void HandleTopDownMovement()
    {
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0f) * currentSpeed;
        
        if (rb2D != null)
        {
            // Use Rigidbody2D for physics-based movement
            rb2D.linearVelocity = new Vector2(movement.x, movement.y);
        }
        else
        {
            // Use transform for simple movement
            transform.Translate(movement * Time.fixedDeltaTime, Space.World);
        }
    }
    
    private void HandleFirstPersonMovement()
    {
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        
        // Calculate movement direction relative to camera
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        // Remove vertical component for ground-based movement
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        Vector3 movement = (forward * moveInput.y + right * moveInput.x) * currentSpeed;
        
        if (characterController != null)
        {
            // Use CharacterController with gravity
            movement.y = -9.81f; // Apply gravity
            characterController.Move(movement * Time.fixedDeltaTime);
        }
        else if (rb3D != null)
        {
            // Use Rigidbody3D
            Vector3 velocity = movement;
            velocity.y = rb3D.linearVelocity.y; // Preserve vertical velocity
            rb3D.linearVelocity = velocity;
        }
        else
        {
            // Simple transform movement
            transform.Translate(movement * Time.fixedDeltaTime, Space.World);
        }
    }
    
    private void HandleFirstPersonCamera()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Horizontal rotation (Y-axis)
        horizontalRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        
        // Vertical rotation (X-axis) with clamping
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);
        
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
    
    private void ToggleView()
    {
        isFirstPersonView = !isFirstPersonView;
        
        if (isFirstPersonView)
        {
            SetFirstPersonView();
        }
        else
        {
            SetTopDownView();
        }
    }
    
    private void SetTopDownView()
    {
        if (cameraTransform == null) return;
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Position camera above player
        cameraTransform.position = transform.position + new Vector3(0f, topDownCameraHeight, 0f);
        cameraTransform.rotation = Quaternion.Euler(topDownCameraAngle, 0f, 0f);
        
        // Reset player rotation
        transform.rotation = Quaternion.identity;
        verticalRotation = 0f;
        horizontalRotation = 0f;
        
        // Set camera as child for following
        if (cameraTransform.parent != transform)
        {
            cameraTransform.SetParent(transform);
            cameraTransform.localPosition = new Vector3(0f, topDownCameraHeight, 0f);
        }
        
        Debug.Log("Switched to Top-Down View");
    }
    
    private void SetFirstPersonView()
    {
        if (cameraTransform == null) return;
        
        // Lock cursor for first person
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Position camera at eye level
        cameraTransform.SetParent(transform);
        cameraTransform.localPosition = new Vector3(0f, firstPersonCameraHeight, 0f);
        cameraTransform.localRotation = Quaternion.identity;
        
        // Reset rotation values
        verticalRotation = 0f;
        horizontalRotation = transform.eulerAngles.y;
        
        Debug.Log("Switched to First-Person View");
    }
    
    // Public methods for external control
    public void SetViewMode(bool firstPerson)
    {
        if (firstPerson != isFirstPersonView)
        {
            ToggleView();
        }
    }
    
    public bool IsFirstPersonView()
    {
        return isFirstPersonView;
    }
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
    
    void OnDestroy()
    {
        // Restore cursor state
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
