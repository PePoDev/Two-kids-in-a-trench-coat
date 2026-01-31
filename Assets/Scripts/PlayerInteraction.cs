using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// Handles player interaction input.
/// Other scripts can subscribe to OnInteract event to implement interaction logic.
/// 
/// Player 1: E key
/// Player 2: Numpad0 key
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    public enum PlayerNumber
    {
        Player1,  // E key
        Player2   // Numpad0
    }

    [Header("Player Settings")]
    [SerializeField] private PlayerNumber playerNumber = PlayerNumber.Player1;
    
    [Header("Input Keys")]
    [SerializeField] private Key player1InteractKey = Key.E;
    [SerializeField] private Key player2InteractKey = Key.Numpad0;
    
    [Header("Events")]
    [Tooltip("Called when player presses interact key (single press, not holding for merge)")]
    public UnityEvent OnInteract;
    
    [Tooltip("Called when player starts holding interact key")]
    public UnityEvent OnInteractHoldStart;
    
    [Tooltip("Called when player releases interact key")]
    public UnityEvent OnInteractHoldEnd;
    
    // State
    private Key myInteractKey;
    private bool isHolding = false;
    private float holdStartTime = 0f;
    private const float HOLD_THRESHOLD = 0.3f; // seconds before considered "holding"
    
    void Start()
    {
        SetupKey();
    }

    void OnValidate()
    {
        SetupKey();
    }

    private void SetupKey()
    {
        myInteractKey = playerNumber == PlayerNumber.Player1 ? player1InteractKey : player2InteractKey;
    }

    void Update()
    {
        // Skip if in first-person mode (merge system handles this)
        if (ViewSwitchManager.Instance != null && ViewSwitchManager.Instance.IsFirstPersonMode)
        {
            return;
        }
        
        HandleInput();
    }

    private void HandleInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        if (keyboard[myInteractKey].wasPressedThisFrame)
        {
            holdStartTime = Time.time;
            isHolding = true;
            OnInteractHoldStart?.Invoke();
        }
        
        if (keyboard[myInteractKey].wasReleasedThisFrame)
        {
            float holdDuration = Time.time - holdStartTime;
            
            // Only trigger interact if it was a quick press (not holding for merge)
            if (holdDuration < HOLD_THRESHOLD)
            {
                OnInteract?.Invoke();
            }
            
            isHolding = false;
            OnInteractHoldEnd?.Invoke();
        }
    }

    // ===== PUBLIC API =====
    
    public bool IsHolding => isHolding;
    public float HoldDuration => isHolding ? Time.time - holdStartTime : 0f;
    public PlayerNumber GetPlayerNumber() => playerNumber;
    
    /// <summary>
    /// Manually trigger interact (for testing or other scripts)
    /// </summary>
    public void TriggerInteract()
    {
        OnInteract?.Invoke();
    }
}
