using UnityEngine;

/// <summary>
/// Controls the player animator based on movement.
/// Gets the Animator component from the first child object.
/// Attach this to your player character that has an Animator on its first child.
/// </summary>
public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The Animator component (auto-assigned from first child if not set)")]
    public Animator animator;
    
    [Header("Animation Names")]
    [Tooltip("Name of the Idle animation state")]
    public string idleStateName = "Idle";
    
    [Tooltip("Name of the Walk animation state")]
    public string walkStateName = "Walk";
    
    [Tooltip("Name of the Interact animation trigger")]
    public string interactTriggerName = "Interact";
    
    [Header("Speed Calculation")]
    [Tooltip("Use Rigidbody velocity for speed calculation")]
    public bool useRigidbody = false;
    
    [Tooltip("Rigidbody component (required if useRigidbody is true)")]
    public Rigidbody rb;
    
    [Tooltip("Use Rigidbody2D velocity for speed calculation")]
    public bool useRigidbody2D = false;
    
    [Tooltip("Rigidbody2D component (required if useRigidbody2D is true)")]
    public Rigidbody2D rb2D;
    
    [Tooltip("Smoothing factor for speed transitions (higher = smoother)")]
    [Range(0f, 1f)]
    public float speedSmoothing = 0.1f;
    
    [Tooltip("Minimum speed threshold to trigger walk animation")]
    public float walkSpeedThreshold = 0.1f;
    
    [Header("Debug")]
    [Tooltip("Show debug UI with speed information")]
    public bool showDebugUI = false;

    private Vector3 lastPosition;
    private float currentSpeed;
    private float smoothedSpeed;

    void Start()
    {
        // Get animator from first child if not assigned
        if (animator == null)
        {
            if (transform.childCount > 0)
            {
                animator = transform.GetChild(0).GetComponent<Animator>();
                if (animator != null)
                {
                    Debug.Log($"Animator found on first child: {transform.GetChild(0).name}");
                }
                else
                {
                    Debug.LogWarning($"No Animator component found on first child: {transform.GetChild(0).name}");
                }
            }
            else
            {
                Debug.LogWarning("No child objects found. Please add an Animator to the first child or assign manually.");
            }
        }

        // Auto-assign rigidbody if useRigidbody is true
        if (useRigidbody && rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Auto-assign rigidbody2D if useRigidbody2D is true
        if (useRigidbody2D && rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
        }

        lastPosition = transform.position;
        
        // Set to idle state by default
        if (animator != null)
        {
            animator.Play(idleStateName);
        }
    }

    void Update()
    {
        CalculateSpeed();
        UpdateAnimator();
    }

    void CalculateSpeed()
    {
        if (useRigidbody && rb != null)
        {
            // Use Rigidbody velocity
            currentSpeed = rb.linearVelocity.magnitude;
        }
        else if (useRigidbody2D && rb2D != null)
        {
            // Use Rigidbody2D velocity
            currentSpeed = rb2D.linearVelocity.magnitude;
        }
        else
        {
            // Calculate speed from position change
            float distance = Vector3.Distance(transform.position, lastPosition);
            currentSpeed = distance / Time.deltaTime;
            lastPosition = transform.position;
        }

        // Smooth the speed value
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, currentSpeed, 1f - speedSmoothing);
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Set the Speed parameter in the animator (if using parameters)
        if (animator.parameters.Length > 0)
        {
            animator.SetFloat("Speed", smoothedSpeed);
        }
        else
        {
            // Direct state switching if not using parameters
            if (smoothedSpeed > walkSpeedThreshold)
            {
                if (!IsInState(walkStateName))
                {
                    animator.Play(walkStateName);
                }
            }
            else
            {
                if (!IsInState(idleStateName))
                {
                    animator.Play(idleStateName);
                }
            }
        }
    }

    /// <summary>
    /// Manually set the speed value (useful for input-based systems)
    /// </summary>
    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    /// <summary>
    /// Get the current smoothed speed
    /// </summary>
    public float GetSpeed()
    {
        return smoothedSpeed;
    }

    /// <summary>
    /// Check if the character is currently walking
    /// </summary>
    public bool IsWalking()
    {
        return smoothedSpeed > walkSpeedThreshold;
    }

    /// <summary>
    /// Play the interact animation
    /// </summary>
    public void PlayInteract()
    {
        if (animator == null)
        {
            Debug.LogWarning("Cannot play interact animation - Animator is null");
            return;
        }

        // Try to trigger the interact animation
        if (HasParameter(interactTriggerName))
        {
            animator.SetTrigger(interactTriggerName);
            Debug.Log("Playing interact animation trigger");
        }
        else
        {
            Debug.LogWarning($"Interact trigger '{interactTriggerName}' not found in animator. Add it in the animator controller.");
        }
    }

    /// <summary>
    /// Check if the animator has a specific parameter
    /// </summary>
    private bool HasParameter(string paramName)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check if the animator is currently in a specific state
    /// </summary>
    private bool IsInState(string stateName)
    {
        if (animator == null) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName);
    }

    // Optional: Visualize speed in the inspector
    void OnGUI()
    {
        if (showDebugUI && Application.isPlaying && animator != null)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"Speed: {smoothedSpeed:F2}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Is Walking: {IsWalking()}");
            
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                GUI.Label(new Rect(10, 50, 300, 20), $"Current State: {stateInfo.shortNameHash}");
            }
        }
    }
}
