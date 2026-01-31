using UnityEngine;

public class PeopleRunning : MonoBehaviour
{
    [Header("Patrol Path Settings")]
    [Tooltip("Array of waypoints defining the running path")]
    public Transform[] runningPath;
    
    [Tooltip("How close to waypoint before moving to next one")]
    public float waypointReachDistance = 0.5f;
    
    [Tooltip("If true, loops back to start. If false, ping-pong")]
    public bool loopPath = true;
    
    [Header("Speed Settings")]
    [Tooltip("Speed when running")]
    public float runningSpeed = 5f;
    
    [Tooltip("Speed when walking/resting")]
    public float walkingSpeed = 1.5f;
    
    [Tooltip("Minimum time to run before resting")]
    public float minRunDuration = 5f;
    
    [Tooltip("Maximum time to run before resting")]
    public float maxRunDuration = 15f;
    
    [Tooltip("Minimum rest/walk duration")]
    public float minRestDuration = 3f;
    
    [Tooltip("Maximum rest/walk duration")]
    public float maxRestDuration = 8f;
    
    [Tooltip("Chance (0-1) to rest at each waypoint")]
    [Range(0f, 1f)]
    public float restChanceAtWaypoint = 0.3f;
    
    [Header("Animation/Visual Settings")]
    [Tooltip("Optional animator component (will set 'Speed' parameter)")]
    public Animator animator;
    
    [Tooltip("Smooth rotation speed")]
    public float rotationSpeed = 8f;
    
    [Header("Visual Debugging")]
    [Tooltip("Show running path in Scene view")]
    public bool showDebugPath = true;
    
    [Tooltip("Color for the path")]
    public Color pathColor = new Color(0f, 1f, 1f, 0.7f);
    
    [Tooltip("Color when running")]
    public Color runningColor = Color.yellow;
    
    [Tooltip("Color when resting")]
    public Color restingColor = Color.blue;
    
    // Movement states
    private enum MovementState
    {
        Running,
        Resting
    }
    
    private MovementState currentState = MovementState.Running;
    private int currentWaypointIndex = 0;
    private bool isMovingForward = true;
    private float currentSpeed;
    private float stateTimer = 0f;
    private float nextStateChangeTime;
    
    void Start()
    {
        if (runningPath == null || runningPath.Length == 0)
        {
            Debug.LogWarning("PeopleRunning: No running path assigned!");
            enabled = false;
            return;
        }
        
        // Auto-find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Start running
        SetRunningState();
    }
    
    void Update()
    {
        if (runningPath == null || runningPath.Length == 0)
            return;
        
        // Update state timer
        stateTimer += Time.deltaTime;
        
        // Check if time to change state
        if (stateTimer >= nextStateChangeTime)
        {
            ToggleState();
        }
        
        // Move along path
        MoveAlongPath();
        
        // Update animator if available
        UpdateAnimator();
    }
    
    void MoveAlongPath()
    {
        Transform targetWaypoint = runningPath[currentWaypointIndex];
        
        if (targetWaypoint == null)
        {
            Debug.LogWarning($"PeopleRunning: Waypoint {currentWaypointIndex} is null!");
            return;
        }
        
        // Calculate direction to waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        
        // Move towards waypoint
        transform.position += direction * currentSpeed * Time.deltaTime;
        
        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        
        // Check if reached waypoint
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distance <= waypointReachDistance)
        {
            OnReachedWaypoint();
        }
    }
    
    void OnReachedWaypoint()
    {
        // Randomly decide to rest at waypoint
        if (currentState == MovementState.Running && Random.value < restChanceAtWaypoint)
        {
            SetRestingState();
        }
        
        // Move to next waypoint
        MoveToNextWaypoint();
    }
    
    void MoveToNextWaypoint()
    {
        if (loopPath)
        {
            // Loop back to start
            currentWaypointIndex = (currentWaypointIndex + 1) % runningPath.Length;
        }
        else
        {
            // Ping-pong between waypoints
            if (isMovingForward)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= runningPath.Length)
                {
                    currentWaypointIndex = runningPath.Length - 2;
                    isMovingForward = false;
                }
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    isMovingForward = true;
                }
            }
            
            // Clamp to valid range
            currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, runningPath.Length - 1);
        }
    }
    
    void ToggleState()
    {
        if (currentState == MovementState.Running)
        {
            SetRestingState();
        }
        else
        {
            SetRunningState();
        }
    }
    
    void SetRunningState()
    {
        currentState = MovementState.Running;
        currentSpeed = runningSpeed;
        stateTimer = 0f;
        nextStateChangeTime = Random.Range(minRunDuration, maxRunDuration);
        
        Debug.Log($"NPC started running (will run for {nextStateChangeTime:F1}s)");
    }
    
    void SetRestingState()
    {
        currentState = MovementState.Resting;
        currentSpeed = walkingSpeed;
        stateTimer = 0f;
        nextStateChangeTime = Random.Range(minRestDuration, maxRestDuration);
        
        Debug.Log($"NPC is resting/walking (will rest for {nextStateChangeTime:F1}s)");
    }
    
    void UpdateAnimator()
    {
        if (animator == null)
            return;
        
        // Set Speed parameter for animation blending
        // You can adjust this based on your animation setup
        float normalizedSpeed = currentSpeed / runningSpeed;
        animator.SetFloat("Speed", normalizedSpeed);
        
        // Alternative: Set boolean states if your animator uses them
        // animator.SetBool("IsRunning", currentState == MovementState.Running);
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugPath || runningPath == null || runningPath.Length < 2)
            return;
        
        // Draw the running path
        Gizmos.color = pathColor;
        for (int i = 0; i < runningPath.Length; i++)
        {
            if (runningPath[i] != null)
            {
                // Draw waypoint sphere
                Gizmos.DrawWireSphere(runningPath[i].position, 0.5f);
                
                // Draw line to next waypoint
                int nextIndex = loopPath ? (i + 1) % runningPath.Length : i + 1;
                if (nextIndex < runningPath.Length && runningPath[nextIndex] != null)
                {
                    Gizmos.DrawLine(runningPath[i].position, runningPath[nextIndex].position);
                }
            }
        }
        
        // Highlight current target waypoint
        if (Application.isPlaying && currentWaypointIndex < runningPath.Length 
            && runningPath[currentWaypointIndex] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(runningPath[currentWaypointIndex].position, 0.7f);
        }
        
        // Show state indicator above NPC
        if (Application.isPlaying)
        {
            Color stateColor = currentState == MovementState.Running ? runningColor : restingColor;
            Gizmos.color = stateColor;
            Gizmos.DrawSphere(transform.position + Vector3.up * 2.5f, 0.3f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;
        
        // Display detailed state information
        #if UNITY_EDITOR
        string stateInfo = $"State: {currentState}\n";
        stateInfo += $"Speed: {currentSpeed:F1}\n";
        stateInfo += $"Time: {stateTimer:F1}s / {nextStateChangeTime:F1}s";
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, stateInfo);
        
        // Draw line to current target
        if (currentWaypointIndex < runningPath.Length && runningPath[currentWaypointIndex] != null)
        {
            Color lineColor = currentState == MovementState.Running ? runningColor : restingColor;
            UnityEditor.Handles.color = lineColor;
            UnityEditor.Handles.DrawDottedLine(transform.position, 
                runningPath[currentWaypointIndex].position, 2f);
        }
        #endif
    }
    
    // Public methods for external control
    public void SetRunning()
    {
        SetRunningState();
    }
    
    public void SetResting()
    {
        SetRestingState();
    }
    
    public bool IsRunning()
    {
        return currentState == MovementState.Running;
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}
