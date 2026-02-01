using UnityEngine;

public class DogController : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Distance at which the dog will start following the player")]
    public float detectionRange = 5f;

    [Tooltip("Layer mask for player detection")]
    public LayerMask playerLayer;

    [Tooltip("Tag to identify player objects")]
    public string playerTag = "Player";

    [Header("Follow Settings")]
    [Tooltip("Speed at which the dog follows the player")]
    public float followSpeed = 4f;

    [Tooltip("How close the dog gets to the player before stopping")]
    public float followStopDistance = 2f;
    
    [Tooltip("Distance at which dog catches player and triggers game over")]
    public float catchDistance = 1f;

    [Tooltip("Time to wait at player position before returning home")]
    public float waitDuration = 3f;

    [Header("Return Settings")]
    [Tooltip("Speed at which the dog returns to starting position")]
    public float returnSpeed = 3f;

    [Tooltip("How close to home position before considered 'returned'")]
    public float homeReachDistance = 0.5f;

    [Header("Sound Effects")]
    [Tooltip("Sound to play when dog starts following player")]
    public AudioClip barkSound;

    [Tooltip("Volume for bark sound")]
    [Range(0f, 1f)]
    public float barkVolume = 1f;

    [Header("Visual Debugging")]
    [Tooltip("Show detection range in Scene view")]
    public bool showDebugGizmos = true;

    [Tooltip("Color for detection range when idle")]
    public Color gizmoColorIdle = new Color(0f, 1f, 0f, 0.3f);

    [Tooltip("Color for detection range when following")]
    public Color gizmoColorFollowing = new Color(1f, 0.5f, 0f, 0.5f);

    // Dog states
    private enum DogState
    {
        Idle,           // At home position, waiting for player
        Following,      // Following the player
        Waiting,        // Waiting at player's location
        Returning       // Returning to home position
    }

    private DogState currentState = DogState.Idle;
    private Vector3 homePosition;
    private Transform targetPlayer;
    private float waitTimer = 0f;
    private float checkTimer = 0f;
    private float checkInterval = 0.3f; // Check for player every 0.3 seconds
    private bool hasCaughtPlayer = false;
    private AudioSource audioSource;

    void Start()
    {
        // Store the initial position as home
        homePosition = transform.position;

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && barkSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.volume = barkVolume;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case DogState.Idle:
                UpdateIdle();
                break;
            case DogState.Following:
                UpdateFollowing();
                break;
            case DogState.Waiting:
                UpdateWaiting();
                break;
            case DogState.Returning:
                UpdateReturning();
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

            // Check for player in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

            foreach (Collider col in colliders)
            {
                if (col.CompareTag(playerTag))
                {
                    // Player detected! Start following
                    targetPlayer = col.transform;
                    currentState = DogState.Following;
                    Debug.Log("Dog detected player and started following!");

                    // Start looping bark sound
                    if (audioSource != null && barkSound != null && !audioSource.isPlaying)
                    {
                        audioSource.clip = barkSound;
                        audioSource.loop = true;
                        audioSource.Play();
                    }

                    break;
                }
            }
        }
    }

    void UpdateFollowing()
    {
        if (targetPlayer == null)
        {
            // Player disappeared, return home
            StopBarkSound();
            currentState = DogState.Returning;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        // Check if dog caught the player
        if (distanceToPlayer <= catchDistance && !hasCaughtPlayer)
        {
            hasCaughtPlayer = true;
            CatchPlayer();
            return;
        }

        // Check if player moved out of detection range
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            // Player too far, start waiting then return
            StopBarkSound();
            targetPlayer = null;
            currentState = DogState.Waiting;
            waitTimer = 0f;
            return;
        }

        // Move towards player if not close enough
        if (distanceToPlayer > followStopDistance)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            transform.position += direction * followSpeed * Time.deltaTime;

            // Rotate to face the player
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
            }
        }
        else
        {
            // Close enough to player, start waiting
            StopBarkSound();
            currentState = DogState.Waiting;
            waitTimer = 0f;
        }
    }

    void UpdateWaiting()
    {
        // Wait at current position
        waitTimer += Time.deltaTime;

        // Check if player is still nearby
        if (targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            // If player moves away while waiting, follow again
            if (distanceToPlayer > followStopDistance * 1.5f)
            {
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = DogState.Following;
                    waitTimer = 0f;
                    return;
                }
            }
        }

        // After waiting, return home
        if (waitTimer >= waitDuration)
        {
            currentState = DogState.Returning;
            targetPlayer = null;
            waitTimer = 0f;
            Debug.Log("Dog finished waiting and is returning home!");
        }
    }

    void UpdateReturning()
    {
        float distanceToHome = Vector3.Distance(transform.position, homePosition);

        // Check if reached home
        if (distanceToHome <= homeReachDistance)
        {
            transform.position = homePosition;
            currentState = DogState.Idle;
            Debug.Log("Dog returned home!");
            return;
        }

        // Move towards home
        Vector3 direction = (homePosition - transform.position).normalized;
        transform.position += direction * returnSpeed * Time.deltaTime;

        // Rotate to face home direction
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 6f);
        }
    }

    void CatchPlayer()
    {
        Debug.Log("Dog caught the player! Game Over!");
        
        // Trigger game over
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogWarning("GameManager not found! Cannot trigger game over.");
        }
    }

    void StopBarkSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;

        Vector3 position = Application.isPlaying ? transform.position : transform.position;
        Vector3 home = Application.isPlaying ? homePosition : transform.position;

        // Draw detection range
        Color rangeColor = currentState == DogState.Following ? gizmoColorFollowing : gizmoColorIdle;
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(position, detectionRange);

        // Draw home position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(home, 0.5f);

        // Draw line to home if not idle
        if (Application.isPlaying && currentState != DogState.Idle)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(position, home);
        }

        // Draw follow stop distance when following
        if (currentState == DogState.Following && targetPlayer != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, followStopDistance);
            
            // Draw catch distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, catchDistance);
        }

        // Draw state indicator
        if (Application.isPlaying)
        {
            // Different colors for different states
            switch (currentState)
            {
                case DogState.Idle:
                    Gizmos.color = Color.green;
                    break;
                case DogState.Following:
                    Gizmos.color = Color.yellow;
                    break;
                case DogState.Waiting:
                    Gizmos.color = Color.red;
                    break;
                case DogState.Returning:
                    Gizmos.color = Color.cyan;
                    break;
            }
            Gizmos.DrawSphere(position + Vector3.up * 2f, 0.3f);
        }
    }

    // Optional: Display current state in inspector
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        // This appears in Scene view when selected
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, $"State: {currentState}");
        if (currentState == DogState.Waiting)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f,
                $"Wait: {waitTimer:F1}s / {waitDuration:F1}s");
        }
#endif
    }
}
