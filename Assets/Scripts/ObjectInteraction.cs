using UnityEngine;
using UnityEngine.Events;

public class ObjectInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;
    
    [Header("Detection Settings")]
    [SerializeField] private bool use2D = true;
    [SerializeField] private Transform raycastOrigin;
    
    [Header("UI Feedback")]
    [SerializeField] private bool showDebugRays = true;
    [SerializeField] private string interactionPrompt = "Press E to interact";
    
    [Header("Events")]
    public UnityEvent OnInteractionAttempt;
    public UnityEvent<GameObject> OnObjectInteracted;
    
    // Internal state
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject;
    
    void Start()
    {
        // Set raycast origin to self if not assigned
        if (raycastOrigin == null)
        {
            raycastOrigin = transform;
        }
        
        // Initialize events if null
        if (OnInteractionAttempt == null)
            OnInteractionAttempt = new UnityEvent();
        
        if (OnObjectInteracted == null)
            OnObjectInteracted = new UnityEvent<GameObject>();
    }

    void Update()
    {
        // Detect interactable objects
        DetectInteractables();
        
        // Handle interaction input
        if (Input.GetKeyDown(interactionKey))
        {
            TryInteract();
        }
    }
    
    private void DetectInteractables()
    {
        currentInteractable = null;
        currentInteractableObject = null;
        
        if (use2D)
        {
            DetectInteractables2D();
        }
        else
        {
            DetectInteractables3D();
        }
    }
    
    private void DetectInteractables2D()
    {
        Vector2 origin = raycastOrigin.position;
        Vector2 direction = raycastOrigin.right; // Right direction in 2D
        
        // Raycast to detect interactable objects
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, interactionRange, interactableLayer);
        
        // Draw debug ray
        if (showDebugRays)
        {
            Color rayColor = hit.collider != null ? Color.green : Color.red;
            Debug.DrawRay(origin, direction * interactionRange, rayColor);
        }
        
        if (hit.collider != null)
        {
            // Try to get IInteractable component
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                currentInteractableObject = hit.collider.gameObject;
            }
        }
    }
    
    private void DetectInteractables3D()
    {
        Vector3 origin = raycastOrigin.position;
        Vector3 direction = raycastOrigin.forward;
        
        // Raycast to detect interactable objects
        RaycastHit hit;
        bool didHit = Physics.Raycast(origin, direction, out hit, interactionRange, interactableLayer);
        
        // Draw debug ray
        if (showDebugRays)
        {
            Color rayColor = didHit ? Color.green : Color.red;
            Debug.DrawRay(origin, direction * interactionRange, rayColor);
        }
        
        if (didHit)
        {
            // Try to get IInteractable component
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                currentInteractableObject = hit.collider.gameObject;
            }
        }
    }
    
    private void TryInteract()
    {
        // Invoke attempt event regardless of success
        OnInteractionAttempt?.Invoke();
        
        if (currentInteractable != null && currentInteractableObject != null)
        {
            // Call the interact method on the interactable object
            currentInteractable.Interact(gameObject);
            
            // Invoke the object interacted event
            OnObjectInteracted?.Invoke(currentInteractableObject);
            
            Debug.Log($"Interacted with: {currentInteractableObject.name}");
        }
        else
        {
            Debug.Log("No interactable object in range.");
        }
    }
    
    // Optional: Public method to check if player can interact
    public bool CanInteract()
    {
        return currentInteractable != null;
    }
    
    // Optional: Get the current interactable object
    public GameObject GetCurrentInteractable()
    {
        return currentInteractableObject;
    }
    
    // Optional: Display interaction prompt
    void OnGUI()
    {
        if (currentInteractable != null && !string.IsNullOrEmpty(interactionPrompt))
        {
            // Simple GUI text at the bottom center of the screen
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(
                new Rect(0, Screen.height - 100, Screen.width, 50),
                interactionPrompt,
                style
            );
        }
    }
}

// Interface for interactable objects
public interface IInteractable
{
    void Interact(GameObject interactor);
}
