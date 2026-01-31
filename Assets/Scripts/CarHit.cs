using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects when a car hits the player and triggers an event.
/// Attach this script to the car GameObject.
/// </summary>
public class CarHit : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Tag of the player object to detect")]
    public string playerTag = "Player";
    
    [Tooltip("Use trigger collider instead of collision")]
    public bool useTrigger = false;
    
    [Header("Events")]
    [Tooltip("Called when the car hits the player")]
    public UnityEvent OnHitPlayer;
    
    [Tooltip("Called when the car hits the player, passes the player GameObject")]
    public UnityEvent<GameObject> OnHitPlayerWithObject;
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebug = true;
    
    void Awake()
    {
        // Initialize events
        if (OnHitPlayer == null)
            OnHitPlayer = new UnityEvent();
        
        if (OnHitPlayerWithObject == null)
            OnHitPlayerWithObject = new UnityEvent<GameObject>();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger)
        {
            HandleHit(collision.gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (useTrigger)
        {
            HandleHit(other.gameObject);
        }
    }
    
    void HandleHit(GameObject hitObject)
    {
        // Check if the hit object is the player
        if (hitObject.CompareTag(playerTag))
        {
            if (showDebug)
            {
                Debug.Log($"Car hit player: {hitObject.name}");
            }
            
            // Trigger events
            OnHitPlayer?.Invoke();
            OnHitPlayerWithObject?.Invoke(hitObject);
        }
    }
}
