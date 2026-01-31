using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interactable object that can be triggered by the player.
/// Attach this to any object you want the player to interact with.
/// Configure the OnInteract event in the Inspector to define what happens.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Name of this interactable object")]
    public string objectName = "Interactable";
    
    [Tooltip("Can this object be interacted with multiple times?")]
    public bool canInteractMultipleTimes = true;
    
    [Tooltip("Cooldown between interactions (seconds)")]
    public float interactionCooldown = 0f;
    
    [Header("Events")]
    [Tooltip("Called when player interacts with this object")]
    public UnityEvent OnInteract;
    
    [Tooltip("Called when player interacts, passes the player GameObject")]
    public UnityEvent<GameObject> OnInteractWithPlayer;
    
    // Internal state
    private bool hasInteracted = false;
    private float lastInteractionTime = -Mathf.Infinity;
    
    void Awake()
    {
        // Initialize events
        if (OnInteract == null)
            OnInteract = new UnityEvent();
        
        if (OnInteractWithPlayer == null)
            OnInteractWithPlayer = new UnityEvent<GameObject>();
    }
    
    /// <summary>
    /// Called by PlayerInteraction when player presses interact key
    /// </summary>
    public void Interact(GameObject player)
    {
        // Check if we can interact
        if (!CanInteract())
        {
            Debug.Log($"{objectName} cannot be interacted with right now.");
            return;
        }
        
        // Mark as interacted
        hasInteracted = true;
        lastInteractionTime = Time.time;
        
        // Invoke events
        Debug.Log($"{player.name} interacted with {objectName}");
        OnInteract?.Invoke();
        OnInteractWithPlayer?.Invoke(player);
    }
    
    /// <summary>
    /// Check if this object can be interacted with
    /// </summary>
    public bool CanInteract()
    {
        // Check if already interacted and multiple interactions not allowed
        if (hasInteracted && !canInteractMultipleTimes)
        {
            return false;
        }
        
        // Check cooldown
        if (Time.time - lastInteractionTime < interactionCooldown)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Reset the interaction state
    /// </summary>
    public void ResetInteraction()
    {
        hasInteracted = false;
        lastInteractionTime = -Mathf.Infinity;
    }
    
    /// <summary>
    /// Get the name of this interactable
    /// </summary>
    public string GetName()
    {
        return objectName;
    }
}
