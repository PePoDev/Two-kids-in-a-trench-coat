using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down 3D player controller that supports two players with different input schemes.
/// Player 1: WASD controls
/// Player 2: Arrow keys controls
/// Movement is on the XZ plane (horizontal ground plane).
/// Uses New Input System.
/// </summary>
public class TopDownPlayer3D : MonoBehaviour
{
    public enum PlayerNumber
    {
        Player1,  // WASD
        Player2   // Arrow Keys
    }

    [Header("Player Settings")]
    [SerializeField] private PlayerNumber playerNumber = PlayerNumber.Player1;
    
    [Header("Walking Events")]
    [Tooltip("Called when player starts walking")]
    [SerializeField] private UnityEvent onWalkStart = new UnityEvent();
    
    [Tooltip("Called when player stops walking")]
    [SerializeField] private UnityEvent onWalkStop = new UnityEvent();
    
    /// <summary>Public accessor for OnWalkStart event</summary>
    public UnityEvent OnWalkStart => onWalkStart;
    
    /// <summary>Public accessor for OnWalkStop event</summary>
    public UnityEvent OnWalkStop => onWalkStop;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private bool enableSprint = true;

    [Header("Rotation Settings")]
    [SerializeField] private bool rotateTowardsMovement = true;
    [SerializeField] private float rotationSpeed = 720f; // degrees per second
    [SerializeField] private bool instantRotation = false;

    [Header("Physics Settings")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Header("Animation")]
    [SerializeField] private bool useAnimator = true;
    [SerializeField] private string walkBoolName = "Walk";
    [SerializeField] private string useItemTriggerName = "UseItem";

    // Internal state
    private Vector3 moveInput;
    private Vector3 velocity;
    private bool isSprinting;
    private bool isGrounded;
    private bool wasMoving = false; // Track walking state for events

    // Components
    private Rigidbody rb;
    private CharacterController characterController;
    private Animator animator;

    // Input keys based on player number (New Input System)
    private Key upKey;
    private Key downKey;
    private Key leftKey;
    private Key rightKey;
    private Key sprintKey;

    void Awake()
    {
        SetupInputKeys();
    }

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();

        // Get animator from first child if using animations
        if (useAnimator && transform.childCount > 0)
        {
            animator = transform.GetChild(0).GetComponent<Animator>();
            if (animator != null)
            {
                // Debug.Log($"Animator found on first child: {transform.GetChild(0).name}");
            }
        }

        // Configure Rigidbody for top-down movement
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void SetupInputKeys()
    {
        if (playerNumber == PlayerNumber.Player1)
        {
            // WASD controls
            upKey = Key.W;
            downKey = Key.S;
            leftKey = Key.A;
            rightKey = Key.D;
            sprintKey = Key.LeftShift;
        }
        else
        {
            // Arrow key controls
            upKey = Key.UpArrow;
            downKey = Key.DownArrow;
            leftKey = Key.LeftArrow;
            rightKey = Key.RightArrow;
            sprintKey = Key.RightShift;
        }
    }

    void Update()
    {
        HandleInput();

        if (rotateTowardsMovement)
        {
            HandleRotation();
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
    }

    private void HandleInput()
    {
        // Get keyboard reference
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Reset input
        moveInput = Vector3.zero;

        // Forward/Backward (Z-axis in world space for top-down)
        if (keyboard[upKey].isPressed)
        {
            moveInput.z += 1f;
        }
        if (keyboard[downKey].isPressed)
        {
            moveInput.z -= 1f;
        }

        // Left/Right (X-axis)
        if (keyboard[leftKey].isPressed)
        {
            moveInput.x -= 1f;
        }
        if (keyboard[rightKey].isPressed)
        {
            moveInput.x += 1f;
        }

        // Normalize for consistent diagonal movement
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
        
        // Check walking state and invoke events
        bool isMovingNow = moveInput.sqrMagnitude > 0.01f;
        
        if (isMovingNow && !wasMoving)
        {
            onWalkStart.Invoke();
        }
        else if (!isMovingNow && wasMoving)
        {
            onWalkStop.Invoke();
        }
        
        wasMoving = isMovingNow;

        // Sprint
        isSprinting = enableSprint && keyboard[sprintKey].isPressed;
    }

    private void CheckGrounded()
    {
        if (characterController != null)
        {
            isGrounded = characterController.isGrounded;
        }
        else
        {
            // Raycast ground check
            isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f,
                Vector3.down, groundCheckDistance + 0.1f, groundLayer);
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 movement = moveInput * currentSpeed;

        if (characterController != null)
        {
            // CharacterController movement
            if (useGravity)
            {
                if (isGrounded && velocity.y < 0)
                {
                    velocity.y = -2f; // Small downward force to keep grounded
                }
                velocity.y += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                velocity.y = 0f;
            }

            Vector3 finalMovement = movement + new Vector3(0, velocity.y, 0);
            characterController.Move(finalMovement * Time.fixedDeltaTime);
        }
        else if (rb != null)
        {
            // Rigidbody movement - preserve Y velocity for gravity
            Vector3 targetVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // Transform-based movement (no physics)
            transform.Translate(movement * Time.fixedDeltaTime, Space.World);
        }
    }

    private void HandleRotation()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Calculate target rotation based on movement direction
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            if (instantRotation)
            {
                transform.rotation = targetRotation;
            }
            else
            {
                // Smooth rotation
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    private void UpdateAnimator()
    {
        if (!useAnimator || animator == null) return;

        // Set Walk bool based on movement
        bool isWalking = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool(walkBoolName, isWalking);
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Trigger the UseItem animation
    /// </summary>
    public void UseItem()
    {
        if (useAnimator && animator != null)
        {
            animator.SetTrigger(useItemTriggerName);
        }
    }

    /// <summary>
    /// Get the current player number
    /// </summary>
    public PlayerNumber GetPlayerNumber()
    {
        return playerNumber;
    }

    /// <summary>
    /// Set the player number and reconfigure inputs
    /// </summary>
    public void SetPlayerNumber(PlayerNumber number)
    {
        playerNumber = number;
        SetupInputKeys();
    }

    /// <summary>
    /// Get current movement input (normalized)
    /// </summary>
    public Vector3 GetMoveInput()
    {
        return moveInput;
    }

    /// <summary>
    /// Check if player is currently moving
    /// </summary>
    public bool IsMoving()
    {
        return moveInput.sqrMagnitude > 0.01f;
    }

    /// <summary>
    /// Check if player is sprinting
    /// </summary>
    public bool IsSprinting()
    {
        return isSprinting && IsMoving();
    }

    /// <summary>
    /// Check if player is on the ground
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// Set move speed at runtime
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// Get current move speed
    /// </summary>
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    /// <summary>
    /// Get current actual speed (including sprint)
    /// </summary>
    public float GetCurrentSpeed()
    {
        return moveSpeed * (isSprinting ? sprintMultiplier : 1f);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update input keys when player number changes in editor
        SetupInputKeys();
    }

    void OnDrawGizmosSelected()
    {
        // Draw ground check ray
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.1f,
            transform.position + Vector3.down * groundCheckDistance);
    }
#endif
}
