using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages view switching between top-down and first-person split-screen modes.
/// 
/// Top-Down Mode: Single camera, both players active
/// First-Person Mode: Split screen (2 cameras), players disabled, SimpleFPPController active
/// </summary>
public class ViewSwitchManager : MonoBehaviour
{
    // ===== SINGLETON =====
    public static ViewSwitchManager Instance { get; private set; }
    
    [Header("Players")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    
    [Header("First-Person")]
    [SerializeField] private SimpleFPPController fppController;
    
    [Header("Cameras")]
    [SerializeField] private Camera topDownCamera;
    [SerializeField] private Camera topFPPCamera;    // Top split (A/D rotation)
    [SerializeField] private Camera bottomFPPCamera; // Bottom split (Arrow rotation)
    
    [Header("Switch Settings")]
    [SerializeField] private float proximityThreshold = 3f;
    [SerializeField] private Key player1SwitchKey = Key.E;
    [SerializeField] private Key player2SwitchKey = Key.Numpad0;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // State
    private bool isFirstPersonMode = false;
    private Vector3 player1LastPosition;
    private Vector3 player2LastPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("ViewSwitchManager: Duplicate instance found, destroying this one.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetTopDownMode();
    }

    void Update()
    {
        HandleSwitchInput();
    }

    // ===== SIMULTANEOUS HOLD DETECTION =====
    
    private bool hasSwitched = false; // Prevent repeated switching while holding
    
    private void HandleSwitchInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Check if BOTH players are holding their keys
        bool player1Holding = keyboard[player1SwitchKey].isPressed;
        bool player2Holding = keyboard[player2SwitchKey].isPressed;
        
        if (player1Holding && player2Holding)
        {
            // Both holding - switch if not already switched
            if (!hasSwitched)
            {
                hasSwitched = true;
                
                if (isFirstPersonMode)
                {
                    SwitchToTopDown();
                }
                else if (CanSwitch())
                {
                    SwitchToFirstPerson();
                }
            }
        }
        else
        {
            // At least one released - allow switching again
            hasSwitched = false;
        }
    }

    // ===== MODE SWITCHING =====

    public bool CanSwitch()
    {
        if (player1 == null || player2 == null) return false;
        float distance = Vector3.Distance(player1.transform.position, player2.transform.position);
        return distance <= proximityThreshold;
    }

    public float GetPlayerDistance()
    {
        if (player1 == null || player2 == null) return float.MaxValue;
        return Vector3.Distance(player1.transform.position, player2.transform.position);
    }

    public void SwitchToFirstPerson()
    {
        if (fppController == null)
        {
            Debug.LogWarning("ViewSwitchManager: FPP Controller not assigned!");
            return;
        }
        
        // Calculate midpoint
        Vector3 midpoint = (player1.transform.position + player2.transform.position) / 2f;
        float rotation = player1.transform.eulerAngles.y;
        
        // Save positions
        player1LastPosition = player1.transform.position;
        player2LastPosition = player2.transform.position;
        
        // Disable players
        player1.SetActive(false);
        player2.SetActive(false);
        
        // Position and activate FPP controller
        fppController.transform.position = midpoint;
        fppController.SetRotation(rotation);
        fppController.gameObject.SetActive(true);
        fppController.SetEnabled(true);
        
        // Setup cameras
        if (topDownCamera != null) topDownCamera.enabled = false;
        
        if (topFPPCamera != null)
        {
            topFPPCamera.enabled = true;
            topFPPCamera.rect = new Rect(0f, 0.5f, 1f, 0.5f);
        }
        
        if (bottomFPPCamera != null)
        {
            bottomFPPCamera.enabled = true;
            bottomFPPCamera.rect = new Rect(0f, 0f, 1f, 0.5f);
        }
        
        isFirstPersonMode = true;
        Debug.Log($"ViewSwitchManager: Switched to Split-Screen FPP at {midpoint}");
    }

    public void SwitchToTopDown()
    {
        Vector3 returnPosition = fppController != null ? 
            fppController.transform.position : 
            (player1LastPosition + player2LastPosition) / 2f;
        
        Vector3 offset = new Vector3(1f, 0f, 0f);
        
        // Disable FPP controller
        if (fppController != null)
        {
            fppController.SetEnabled(false);
            fppController.gameObject.SetActive(false);
        }
        
        // Reposition and activate players
        player1.transform.position = returnPosition - offset;
        player2.transform.position = returnPosition + offset;
        player1.SetActive(true);
        player2.SetActive(true);
        
        // Reset cameras
        if (topFPPCamera != null) topFPPCamera.enabled = false;
        if (bottomFPPCamera != null) bottomFPPCamera.enabled = false;
        
        if (topDownCamera != null)
        {
            topDownCamera.enabled = true;
            topDownCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
        
        isFirstPersonMode = false;
        Debug.Log($"ViewSwitchManager: Switched to Top-Down at {returnPosition}");
    }

    private void SetTopDownMode()
    {
        isFirstPersonMode = false;
        
        if (player1 != null) player1.SetActive(true);
        if (player2 != null) player2.SetActive(true);
        
        if (fppController != null)
        {
            fppController.SetEnabled(false);
            fppController.gameObject.SetActive(false);
        }
        
        if (topDownCamera != null)
        {
            topDownCamera.enabled = true;
            topDownCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
        if (topFPPCamera != null) topFPPCamera.enabled = false;
        if (bottomFPPCamera != null) bottomFPPCamera.enabled = false;
    }

    // ===== PUBLIC API =====
    
    public bool IsFirstPersonMode => isFirstPersonMode;
    public bool IsTopDownMode => !isFirstPersonMode;
    public float ProximityThreshold => proximityThreshold;
    
    /// <summary>
    /// Change the top-down camera reference
    /// </summary>
    public void SetTopDownCamera(Camera newCamera)
    {
        // Disable old camera if switching while in top-down mode
        if (!isFirstPersonMode && topDownCamera != null)
        {
            topDownCamera.enabled = false;
        }
        
        topDownCamera = newCamera;
        
        // Enable new camera if in top-down mode
        if (!isFirstPersonMode && topDownCamera != null)
        {
            topDownCamera.enabled = true;
            topDownCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }
    
    /// <summary>
    /// Get current top-down camera
    /// </summary>
    public Camera GetTopDownCamera() => topDownCamera;
    
    /// <summary>
    /// Set FPP cameras
    /// </summary>
    public void SetFPPCameras(Camera top, Camera bottom)
    {
        topFPPCamera = top;
        bottomFPPCamera = bottom;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (player1 == null || player2 == null) return;
        
        Gizmos.color = CanSwitch() ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(player1.transform.position, proximityThreshold / 2f);
        Gizmos.DrawWireSphere(player2.transform.position, proximityThreshold / 2f);
        
        Gizmos.color = CanSwitch() ? Color.green : Color.red;
        Gizmos.DrawLine(player1.transform.position, player2.transform.position);
        
        if (CanSwitch())
        {
            Vector3 midpoint = (player1.transform.position + player2.transform.position) / 2f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(midpoint, 0.5f);
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label($"Mode: {(isFirstPersonMode ? "Split-Screen FPP" : "Top-Down")}");
        GUILayout.Label($"Distance: {GetPlayerDistance():F2} / {proximityThreshold:F2}");
        GUILayout.Label($"Can Switch: {CanSwitch()}");
        if (CanSwitch() && !isFirstPersonMode)
        {
            GUILayout.Label("Press E or Numpad0 to switch!");
        }
        GUILayout.EndArea();
    }
#endif
}
