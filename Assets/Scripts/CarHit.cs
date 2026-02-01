using UnityEngine;

/// <summary>
/// Detects when a car hits the player and triggers game over.
/// This script is automatically added to cars when spawned.
///
/// Note: This script works with colliders on child objects.
/// Place this script on the parent car object, and it will detect
/// collisions from any colliders on its children.
/// </summary>
public class CarHit : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Tag of the player object to detect")]
    public string playerTag = "Player";
    
    [Tooltip("Use trigger collider instead of collision")]
    public bool useTrigger = false;
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebug = true;
    
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
            
            // Call game over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Debug.LogWarning("GameManager not found! Cannot trigger game over.");
            }
        }
    }
    
    /// <summary>
    /// Called by CarHitForwarder when a child collider detects a collision
    /// </summary>
    public void OnChildCollision(GameObject hitObject)
    {
        if (!useTrigger)
        {
            HandleHit(hitObject);
        }
    }
    
    /// <summary>
    /// Called by CarHitForwarder when a child collider detects a trigger
    /// </summary>
    public void OnChildTrigger(GameObject hitObject)
    {
        if (useTrigger)
        {
            HandleHit(hitObject);
        }
    }
}
