using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Example implementation of an interactable object.
/// Attach this to any GameObject you want the player to interact with.
/// </summary>
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string objectName = "Object";
    [SerializeField] private bool canInteractMultipleTimes = true;
    [SerializeField] private float cooldownTime = 0f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private bool enableHighlight = true;
    
    [Header("Custom Events")]
    public UnityEvent OnInteract;
    public UnityEvent<GameObject> OnInteractWithPlayer;
    
    // Internal state
    private bool hasInteracted = false;
    private float lastInteractionTime = -Mathf.Infinity;
    private Renderer objectRenderer;
    private Material originalMaterial;
    private Color originalColor;
    
    void Start()
    {
        // Initialize events
        if (OnInteract == null)
            OnInteract = new UnityEvent();
        
        if (OnInteractWithPlayer == null)
            OnInteractWithPlayer = new UnityEvent<GameObject>();
        
        // Get renderer for visual feedback
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
            originalColor = objectRenderer.material.color;
        }
    }
    
    public void Interact(GameObject interactor)
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
        
        // Perform interaction
        PerformInteraction(interactor);
        
        // Invoke Unity events
        OnInteract?.Invoke();
        OnInteractWithPlayer?.Invoke(interactor);
    }
    
    private bool CanInteract()
    {
        // Check if we've already interacted and if multiple interactions are allowed
        if (hasInteracted && !canInteractMultipleTimes)
        {
            return false;
        }
        
        // Check cooldown
        if (Time.time - lastInteractionTime < cooldownTime)
        {
            return false;
        }
        
        return true;
    }
    
    private void PerformInteraction(GameObject interactor)
    {
        // This is where you put your custom interaction logic
        Debug.Log($"{interactor.name} interacted with {objectName}!");
        
        // Example: Change color on interaction
        if (enableHighlight && objectRenderer != null)
        {
            StartCoroutine(HighlightEffect());
        }
    }
    
    private System.Collections.IEnumerator HighlightEffect()
    {
        // Apply highlight
        if (highlightMaterial != null)
        {
            objectRenderer.material = highlightMaterial;
        }
        else
        {
            objectRenderer.material.color = highlightColor;
        }
        
        // Wait
        yield return new WaitForSeconds(0.3f);
        
        // Restore original
        if (highlightMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }
        else
        {
            objectRenderer.material.color = originalColor;
        }
    }
    
    // Optional: Reset the interaction state
    public void ResetInteraction()
    {
        hasInteracted = false;
        lastInteractionTime = -Mathf.Infinity;
    }
    
    // Optional: Enable/disable interaction
    public void SetInteractionEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
}
