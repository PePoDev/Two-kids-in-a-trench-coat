using UnityEngine;

/// <summary>
/// Controls a box that closes when a chicken/player enters it.
/// Switches between open and closed box models by toggling their active state.
/// Requires a trigger collider on this GameObject.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BoxController : MonoBehaviour
{
    [Header("Box Models")]
    [Tooltip("The open box model (active by default)")]
    public GameObject openBoxModel;
    
    [Tooltip("The closed box model (inactive by default)")]
    public GameObject closedBoxModel;
    
    [Header("Trigger Settings")]
    [Tooltip("Tag of the object that triggers the box to close (e.g., 'Player' or 'Chicken')")]
    public string triggerTag = "Player";
    
    [Tooltip("Allow the box to be opened again when object exits")]
    public bool canReopen = false;
    
    [Header("State")]
    [Tooltip("Is the box currently closed?")]
    public bool isClosed = false;
    
    [Header("Audio (Optional)")]
    [Tooltip("Sound to play when box closes")]
    public AudioClip closeSound;
    
    [Tooltip("Sound to play when box opens")]
    public AudioClip openSound;
    
    private AudioSource audioSource;
    private Collider boxCollider;
    private GameObject capturedObject;
    
    void Start()
    {
        // Validate models
        if (openBoxModel == null || closedBoxModel == null)
        {
            Debug.LogError("BoxController: Both open and closed box models must be assigned!");
            enabled = false;
            return;
        }
        
        // Setup trigger collider
        boxCollider = GetComponent<Collider>();
        if (!boxCollider.isTrigger)
        {
            Debug.LogWarning("BoxController: Collider should be set as trigger! Enabling trigger...");
            boxCollider.isTrigger = true;
        }
        
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (closeSound != null || openSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set initial state
        SetBoxState(isClosed);
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object has the correct tag
        if (other.CompareTag(triggerTag))
        {
            if (!isClosed)
            {
                // Store the captured object
                capturedObject = other.gameObject;
                CloseBox();
                Debug.Log($"Box closed by {other.name}");
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Only allow reopening if enabled
        if (canReopen && other.CompareTag(triggerTag))
        {
            if (isClosed)
            {
                OpenBox();
                Debug.Log($"Box opened as {other.name} left");
            }
        }
    }
    
    /// <summary>
    /// Close the box (show closed model, hide open model)
    /// </summary>
    public void CloseBox()
    {
        SetBoxState(true);
        
        // Play close sound
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
        
        // Destroy the captured object
        if (capturedObject != null)
        {
            Destroy(capturedObject);
            Debug.Log($"Destroyed {capturedObject.name}");
            capturedObject = null;
        }
    }
    
    /// <summary>
    /// Open the box (show open model, hide closed model)
    /// </summary>
    public void OpenBox()
    {
        SetBoxState(false);
        
        // Play open sound
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
    }
    
    /// <summary>
    /// Toggle between open and closed state
    /// </summary>
    public void ToggleBox()
    {
        if (isClosed)
        {
            OpenBox();
        }
        else
        {
            CloseBox();
        }
    }
    
    /// <summary>
    /// Set the box state directly
    /// </summary>
    void SetBoxState(bool closed)
    {
        isClosed = closed;
        
        // Switch model active states
        if (openBoxModel != null)
        {
            openBoxModel.SetActive(!closed);
        }
        
        if (closedBoxModel != null)
        {
            closedBoxModel.SetActive(closed);
        }
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Draw the trigger area
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = isClosed ? Color.red : Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider boxCol)
            {
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawWireSphere(sphereCol.center, sphereCol.radius);
            }
        }
    }
#endif
}
