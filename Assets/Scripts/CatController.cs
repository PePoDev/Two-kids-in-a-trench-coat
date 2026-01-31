using UnityEngine;

public class CatController : MonoBehaviour
{
    [Header("Flee Settings")]
    [Tooltip("Distance at which the cat detects and flees from player")]
    public float fleeDetectionRange = 5f;
    
    [Tooltip("Speed at which the cat runs away")]
    public float fleeSpeed = 6f;
    
    [Tooltip("Layer mask for player detection")]
    public LayerMask playerLayer;
    
    [Tooltip("Tag to identify player objects")]
    public string playerTag = "Player";
    
    [Header("Movement Settings")]
    [Tooltip("How far the cat moves when fleeing")]
    public float fleeDistance = 3f;
    
    [Tooltip("Time cat pauses after fleeing before it can flee again")]
    public float fleeCooldown = 1.5f;
    
    [Tooltip("If true, cat avoids obstacles while fleeing")]
    public bool avoidObstacles = true;
    
    [Tooltip("Layer mask for obstacles to avoid")]
    public LayerMask obstacleLayer;
    
    [Header("Capture Settings")]
    [Tooltip("Tag for the capture box/area")]
    public string boxTag = "CaptureBox";
    
    [Tooltip("Event called when cat is caught")]
    public UnityEngine.Events.UnityEvent onCatCaught;
    
    [Header("Visual Debugging")]
    [Tooltip("Show detection and flee range in Scene view")]
    public bool showDebugGizmos = true;
    
    [Tooltip("Color for detection range")]
    public Color gizmoColorDetection = new Color(1f, 0.5f, 0f, 0.3f);
    
    [Tooltip("Color when fleeing")]
    public Color gizmoColorFleeing = new Color(1f, 0f, 0f, 0.5f);
    
    [Tooltip("Color when caught")]
    public Color gizmoColorCaught = new Color(0f, 1f, 0f, 0.7f);
    
    // Cat states
    private enum CatState
    {
        Idle,       // Waiting, looking for players
        Fleeing,    // Running away from player
        Cooldown,   // Pausing after fleeing
        Caught      // Inside the box, caught!
    }
    
    private CatState currentState = CatState.Idle;
    private Transform nearestPlayer;
    private float cooldownTimer = 0f;
    private float checkTimer = 0f;
    private float checkInterval = 0.2f;
    private Vector3 fleeTarget;
    private bool isCaught = false;
    
    void Start()
    {
        // Initialize
    }
    
    void Update()
    {
        if (isCaught)
        {
            // Cat is caught, no more updates needed
            return;
        }
        
        switch (currentState)
        {
            case CatState.Idle:
                UpdateIdle();
                break;
            case CatState.Fleeing:
                UpdateFleeing();
                break;
            case CatState.Cooldown:
                UpdateCooldown();
                break;
        }
    }
    
    void UpdateIdle()
    {
        // Periodically check for nearby players
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckForPlayers();
        }
    }
    
    void CheckForPlayers()
    {
        // Find players in detection range
        Collider[] colliders = Physics.OverlapSphere(transform.position, fleeDetectionRange, playerLayer);
        
        float closestDistance = float.MaxValue;
        Transform closestPlayer = null;
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag(playerTag))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = col.transform;
                }
            }
        }
        
        if (closestPlayer != null)
        {
            // Player detected! Start fleeing
            nearestPlayer = closestPlayer;
            StartFleeing();
        }
    }
    
    void StartFleeing()
    {
        if (nearestPlayer == null)
            return;
        
        // Calculate flee direction (away from player)
        Vector3 directionAwayFromPlayer = (transform.position - nearestPlayer.position).normalized;
        
        // Add some randomness to make it more natural
        float randomAngle = Random.Range(-30f, 30f);
        directionAwayFromPlayer = Quaternion.Euler(0, randomAngle, 0) * directionAwayFromPlayer;
        
        // Check for obstacles if enabled
        if (avoidObstacles)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionAwayFromPlayer, out hit, fleeDistance, obstacleLayer))
            {
                // Obstacle detected, try to flee to the side
                Vector3 alternateDirection = Quaternion.Euler(0, 90f, 0) * directionAwayFromPlayer;
                
                if (!Physics.Raycast(transform.position, alternateDirection, fleeDistance, obstacleLayer))
                {
                    directionAwayFromPlayer = alternateDirection;
                }
                else
                {
                    // Try other side
                    alternateDirection = Quaternion.Euler(0, -90f, 0) * directionAwayFromPlayer;
                    if (!Physics.Raycast(transform.position, alternateDirection, fleeDistance, obstacleLayer))
                    {
                        directionAwayFromPlayer = alternateDirection;
                    }
                }
            }
        }
        
        // Store current Y and set flee target on same Y level
        float currentY = transform.position.y;
        fleeTarget = transform.position + directionAwayFromPlayer * fleeDistance;
        fleeTarget = new Vector3(fleeTarget.x, currentY, fleeTarget.z);
        currentState = CatState.Fleeing;
        
        Debug.Log("Cat is fleeing from player!");
    }
    
    void UpdateFleeing()
    {
        // Move towards flee target (only calculate XZ distance)
        Vector3 currentPosFlat = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 fleeTargetFlat = new Vector3(fleeTarget.x, 0f, fleeTarget.z);
        float distance = Vector3.Distance(currentPosFlat, fleeTargetFlat);
        
        if (distance > 0.1f)
        {
            Vector3 direction = (fleeTarget - transform.position);
            direction.y = 0f; // Flatten direction for horizontal movement only
            direction.Normalize();
            
            // Store current Y position
            float currentY = transform.position.y;
            
            // Move the cat
            transform.position += direction * fleeSpeed * Time.deltaTime;
            
            // Lock Y position to prevent sinking/floating
            transform.position = new Vector3(transform.position.x, currentY, transform.position.z);
            
            // Rotate to face movement direction (Y-axis only)
            if (direction != Vector3.zero)
            {
                // Flatten direction to XZ plane for Y-axis only rotation
                Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;
                if (flatDirection != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(flatDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
                }
            }
        }
        else
        {
            // Reached flee target, start cooldown
            currentState = CatState.Cooldown;
            cooldownTimer = 0f;
        }
    }
    
    void UpdateCooldown()
    {
        cooldownTimer += Time.deltaTime;
        
        if (cooldownTimer >= fleeCooldown)
        {
            // Cooldown finished, back to idle
            currentState = CatState.Idle;
            nearestPlayer = null;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if cat entered the capture box
        if (other.CompareTag(boxTag) && !isCaught)
        {
            CatchCat();
        }
    }
    
    void CatchCat()
    {
        isCaught = true;
        currentState = CatState.Caught;
        
        Debug.Log("Cat has been caught in the box!");
        
        // Invoke the caught event
        if (onCatCaught != null)
        {
            onCatCaught.Invoke();
        }
    }
    
    // Public method to reset the cat (if needed for replay)
    public void ResetCat(Vector3 newPosition)
    {
        transform.position = newPosition;
        isCaught = false;
        currentState = CatState.Idle;
        nearestPlayer = null;
        cooldownTimer = 0f;
        checkTimer = 0f;
        Debug.Log("Cat has been reset!");
    }
    
    // Public method to check if caught
    public bool IsCaught()
    {
        return isCaught;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;
        
        Vector3 position = transform.position;
        
        // Choose color based on state
        Color gizmoColor = gizmoColorDetection;
        if (isCaught)
        {
            gizmoColor = gizmoColorCaught;
        }
        else if (currentState == CatState.Fleeing)
        {
            gizmoColor = gizmoColorFleeing;
        }
        
        // Draw detection range
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(position, fleeDetectionRange);
        
        // Draw flee target when fleeing
        if (Application.isPlaying && currentState == CatState.Fleeing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, fleeTarget);
            Gizmos.DrawWireSphere(fleeTarget, 0.3f);
        }
        
        // Draw state indicator above cat
        if (Application.isPlaying)
        {
            switch (currentState)
            {
                case CatState.Idle:
                    Gizmos.color = Color.yellow;
                    break;
                case CatState.Fleeing:
                    Gizmos.color = Color.red;
                    break;
                case CatState.Cooldown:
                    Gizmos.color = Color.blue;
                    break;
                case CatState.Caught:
                    Gizmos.color = Color.green;
                    break;
            }
            Gizmos.DrawSphere(position + Vector3.up * 2f, 0.25f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;
        
        // Display state information in Scene view
        #if UNITY_EDITOR
        string stateInfo = $"State: {currentState}";
        if (isCaught)
        {
            stateInfo += " (CAUGHT!)";
        }
        else if (currentState == CatState.Cooldown)
        {
            stateInfo += $" ({cooldownTimer:F1}s / {fleeCooldown:F1}s)";
        }
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, stateInfo);
        #endif
    }
}
