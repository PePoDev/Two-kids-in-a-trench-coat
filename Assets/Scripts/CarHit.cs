using UnityEngine;

/// <summary>
/// Detects when a car hits the player and triggers game over.
/// This script is automatically added to cars when spawned.
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
}
