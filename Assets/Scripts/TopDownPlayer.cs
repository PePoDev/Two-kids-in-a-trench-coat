using UnityEngine;

/// <summary>
/// Top-down player controller that supports two players with different input schemes.
/// Player 1: WASD controls
/// Player 2: Arrow keys controls
/// </summary>
public class TopDownPlayer : MonoBehaviour
{
    public enum PlayerNumber
    {
        Player1,  // WASD
        Player2   // Arrow Keys
    }

    [Header("Player Settings")]
    [SerializeField] private PlayerNumber playerNumber = PlayerNumber.Player1;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private bool enableSprint = true;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool rotateTowardsMovement = true;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color player1Color = new Color(0.2f, 0.6f, 1f); // Blue
    [SerializeField] private Color player2Color = new Color(1f, 0.4f, 0.4f); // Red
    
    // Internal state
    private Vector2 moveInput;
    private bool isSprinting;
    
    // Components
    private Rigidbody2D rb2D;
    private Rigidbody rb3D;
    
    // Input keys based on player number
    private KeyCode upKey;
    private KeyCode downKey;
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode sprintKey;

    void Awake()
    {
        SetupInputKeys();
    }

    void Start()
    {
        // Get components
        rb2D = GetComponent<Rigidbody2D>();
        rb3D = GetComponent<Rigidbody>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Apply player color
        ApplyPlayerColor();
    }

    private void SetupInputKeys()
    {
        if (playerNumber == PlayerNumber.Player1)
        {
            // WASD controls
            upKey = KeyCode.W;
            downKey = KeyCode.S;
            leftKey = KeyCode.A;
            rightKey = KeyCode.D;
            sprintKey = KeyCode.LeftShift;
        }
        else
        {
            // Arrow key controls
            upKey = KeyCode.UpArrow;
            downKey = KeyCode.DownArrow;
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            sprintKey = KeyCode.RightShift;
        }
    }

    private void ApplyPlayerColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = playerNumber == PlayerNumber.Player1 ? player1Color : player2Color;
        }
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
        
        if (rotateTowardsMovement)
        {
            HandleRotation();
        }
    }

    private void HandleInput()
    {
        // Reset input
        moveInput = Vector2.zero;
        
        // Vertical movement
        if (Input.GetKey(upKey))
        {
            moveInput.y += 1f;
        }
        if (Input.GetKey(downKey))
        {
            moveInput.y -= 1f;
        }
        
        // Horizontal movement
        if (Input.GetKey(leftKey))
        {
            moveInput.x -= 1f;
        }
        if (Input.GetKey(rightKey))
        {
            moveInput.x += 1f;
        }
        
        // Normalize for consistent diagonal movement
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
        
        // Sprint
        isSprinting = enableSprint && Input.GetKey(sprintKey);
    }

    private void HandleMovement()
    {
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0f) * currentSpeed;
        
        if (rb2D != null)
        {
            // Physics-based 2D movement
            rb2D.linearVelocity = new Vector2(movement.x, movement.y);
        }
        else if (rb3D != null)
        {
            // Physics-based 3D movement (XZ plane for top-down 3D)
            rb3D.linearVelocity = new Vector3(movement.x, rb3D.linearVelocity.y, movement.y);
        }
        else
        {
            // Transform-based movement
            transform.Translate(movement * Time.fixedDeltaTime, Space.World);
        }
    }

    private void HandleRotation()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Calculate target rotation based on movement direction
            float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            
            if (rb2D != null)
            {
                // 2D rotation (Z-axis)
                float currentAngle = transform.eulerAngles.z;
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                rb2D.MoveRotation(newAngle);
            }
            else
            {
                // 3D rotation (Y-axis for top-down)
                float targetYAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetYAngle, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    // ===== PUBLIC API =====
    
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
        ApplyPlayerColor();
    }
    
    /// <summary>
    /// Get current movement input
    /// </summary>
    public Vector2 GetMoveInput()
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

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update input keys when player number changes in editor
        SetupInputKeys();
    }
#endif
}
