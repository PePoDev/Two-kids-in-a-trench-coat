using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Handles player interaction input.
/// Detects nearby InteractableObjects and allows player to interact with them.
/// Shows tooltip when near an interactable object.
/// Uses Space bar for interaction.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Range to detect interactable objects")]
    public float detectionRange = 2f;
    
    [Tooltip("Layer mask for interactable objects")]
    public LayerMask interactableLayer = ~0;
    
    [Tooltip("How often to check for interactables (seconds)")]
    public float checkInterval = 0.1f;
    
    [Header("UI Settings")]
    [Tooltip("Show on-screen tooltip prompt")]
    public bool showTooltip = true;
    
    [Tooltip("TextMeshPro component for tooltip (optional, uses OnGUI if null)")]
    public TextMeshProUGUI tooltipText;
    
    [Tooltip("Canvas Group for tooltip (for fade effects)")]
    public CanvasGroup tooltipCanvasGroup;
    
    [Tooltip("Tooltip message")]
    public string tooltipMessage = "Press Space to interact";
    
    [Header("Events")]
    [Tooltip("Called when player interacts with any object")]
    public UnityEvent<GameObject> OnInteract;
    
    // Internal state
    private InteractableObject currentInteractable;
    private float checkTimer = 0f;
    private Key interactKey = Key.Space;
    
    void Start()
    {
        // Initialize events
        if (OnInteract == null)
            OnInteract = new UnityEvent<GameObject>();
        
        // Hide tooltip initially
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);
        
        if (tooltipCanvasGroup != null)
            tooltipCanvasGroup.alpha = 0f;
    }
    
    void Update()
    {
        // Periodically check for nearby interactables
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            DetectInteractables();
        }
        
        // Update tooltip display
        UpdateTooltip();
        
        // Handle interaction input
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[interactKey].wasPressedThisFrame)
        {
            TryInteract();
        }
    }
    
    void DetectInteractables()
    {
        // Find all colliders in range
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, interactableLayer);
        
        // Find the closest interactable
        InteractableObject closestInteractable = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in colliders)
        {
            InteractableObject interactable = col.GetComponent<InteractableObject>();
            if (interactable != null && interactable.CanInteract())
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
        
        currentInteractable = closestInteractable;
    }
    
    void UpdateTooltip()
    {
        if (!showTooltip)
            return;
        
        bool shouldShow = currentInteractable != null;
        
        // Update TextMeshPro tooltip
        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                tooltipText.text = tooltipMessage;
            }
        }
        
        // Update canvas group alpha
        if (tooltipCanvasGroup != null)
        {
            float targetAlpha = shouldShow ? 1f : 0f;
            tooltipCanvasGroup.alpha = Mathf.Lerp(tooltipCanvasGroup.alpha, targetAlpha, Time.deltaTime * 10f);
        }
    }
    
    void TryInteract()
    {
        if (currentInteractable != null)
        {
            // Interact with the object
            currentInteractable.Interact(gameObject);
            
            // Invoke event
            OnInteract?.Invoke(currentInteractable.gameObject);
        }
    }
    
    /// <summary>
    /// Check if player is currently near an interactable
    /// </summary>
    public bool IsNearInteractable()
    {
        return currentInteractable != null;
    }
    
    /// <summary>
    /// Get the current interactable object
    /// </summary>
    public InteractableObject GetCurrentInteractable()
    {
        return currentInteractable;
    }
    
    // Fallback OnGUI tooltip if no TextMeshPro is assigned
    void OnGUI()
    {
        if (showTooltip && tooltipText == null && currentInteractable != null)
        {
            // Simple GUI text at the bottom center of the screen
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(
                new Rect(0, Screen.height - 100, Screen.width, 50),
                tooltipMessage,
                style
            );
        }
    }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = currentInteractable != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw line to current interactable
        if (currentInteractable != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentInteractable.transform.position);
        }
    }
#endif
}
