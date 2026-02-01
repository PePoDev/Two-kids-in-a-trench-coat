using UnityEngine;

/// <summary>
/// Trap that triggers game over when the player touches it.
/// Works with both collision and trigger detection.
/// </summary>
public class Trap : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Tag to identify player objects")]
    public string playerTag = "Player";
    
    [Tooltip("Use trigger collider instead of collision")]
    public bool useTrigger = true;
    
    [Header("Sound Effect")]
    [Tooltip("Sound to play when trap is triggered")]
    public AudioClip trapSound;
    
    [Tooltip("Volume for trap sound")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;
    
    [Header("Visual Feedback")]
    [Tooltip("Particle effect to play when triggered (optional)")]
    public ParticleSystem trapEffect;
    
    [Tooltip("Delay before triggering game over (allows sound/effects to play)")]
    public float gameOverDelay = 0.5f;
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebug = true;
    
    private bool hasBeenTriggered = false;
    private AudioSource audioSource;
    
    void Start()
    {
        // Setup audio source if trap sound is assigned
        if (trapSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }
        
        // Validate collider setup
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"Trap '{gameObject.name}' has no Collider component! Add a Collider for detection to work.");
        }
        else if (useTrigger && !col.isTrigger)
        {
            Debug.LogWarning($"Trap '{gameObject.name}' is set to use trigger but collider is not marked as trigger. Setting it now.");
            col.isTrigger = true;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger)
        {
            HandlePlayerHit(collision.gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (useTrigger)
        {
            HandlePlayerHit(other.gameObject);
        }
    }
    
    void HandlePlayerHit(GameObject hitObject)
    {
        // Check if already triggered
        if (hasBeenTriggered)
            return;
        
        // Check if the hit object is the player
        if (hitObject.CompareTag(playerTag))
        {
            hasBeenTriggered = true;
            
            if (showDebug)
            {
                Debug.Log($"Player hit trap: {gameObject.name}");
            }
            
            // Play sound effect
            if (audioSource != null && trapSound != null)
            {
                audioSource.PlayOneShot(trapSound);
            }
            
            // Play particle effect
            if (trapEffect != null)
            {
                trapEffect.Play();
            }
            
            // Trigger game over after delay
            Invoke(nameof(TriggerGameOver), gameOverDelay);
        }
    }
    
    void TriggerGameOver()
    {
        if (showDebug)
        {
            Debug.Log("Trap triggered game over!");
        }
        
        // Call GameManager to trigger game over
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogWarning("GameManager not found! Cannot trigger game over.");
        }
    }
    
    /// <summary>
    /// Reset the trap so it can be triggered again (useful for testing or respawning)
    /// </summary>
    public void ResetTrap()
    {
        hasBeenTriggered = false;
        CancelInvoke(nameof(TriggerGameOver));
    }
}
