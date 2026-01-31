using UnityEngine;

/// <summary>
/// Top-down camera controller that can follow and frame one or two player characters.
/// Dynamically adjusts position and zoom to keep both players in view.
/// </summary>
public class CameraTopViewMovement : MonoBehaviour
{
    [Header("Player Targets")]
    [Tooltip("First player to follow")]
    public Transform player1;
    
    [Tooltip("Second player to follow (optional)")]
    public Transform player2;
    
    [Header("Camera Settings")]
    [Tooltip("Height of the camera above the ground")]
    public float cameraHeight = 15f;
    
    [Tooltip("Camera angle (0 = straight down, 45 = angled)")]
    [Range(0f, 89f)]
    public float cameraAngle = 60f;
    
    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows (higher = smoother but slower)")]
    [Range(0.01f, 1f)]
    public float followSmoothing = 0.1f;
    
    [Tooltip("Offset from the center point")]
    public Vector3 offset = Vector3.zero;
    
    [Header("Dynamic Zoom")]
    [Tooltip("Enable dynamic zoom to fit both players")]
    public bool useDynamicZoom = true;
    
    [Tooltip("Minimum camera height (perspective) or orthographic size")]
    public float minZoom = 5f;
    
    [Tooltip("Maximum camera height (perspective) or orthographic size")]
    public float maxZoom = 25f;
    
    [Tooltip("Padding around players (larger = more space)")]
    public float zoomPadding = 3f;
    
    [Tooltip("How smoothly the camera zooms")]
    [Range(0.01f, 1f)]
    public float zoomSmoothing = 0.1f;
    
    [Tooltip("Extra multiplier for perspective zoom (adjust if needed)")]
    [Range(1f, 3f)]
    public float perspectiveZoomMultiplier = 1.5f;
    
    [Header("Boundaries (Optional)")]
    [Tooltip("Enable camera boundaries")]
    public bool useBoundaries = false;
    
    [Tooltip("Minimum X position")]
    public float minX = -50f;
    
    [Tooltip("Maximum X position")]
    public float maxX = 50f;
    
    [Tooltip("Minimum Z position")]
    public float minZ = -50f;
    
    [Tooltip("Maximum Z position")]
    public float maxZ = 50f;
    
    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private float zoomVelocity = 0f;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraTopViewMovement requires a Camera component!");
        }
    }
    
    void LateUpdate()
    {
        if (!HasValidTarget())
        {
            return;
        }
        
        // Calculate target position
        Vector3 targetPosition = GetCenterPoint();
        
        // Handle dynamic zoom (affects height for both camera types)
        float targetHeight = cameraHeight;
        if (useDynamicZoom && cam != null && player1 != null && player2 != null)
        {
            targetHeight = CalculateDynamicHeight();
        }
        
        // Apply camera angle offset with dynamic height
        Vector3 angleOffset = GetAngleOffsetWithHeight(targetHeight);
        targetPosition += angleOffset + offset;
        
        // Apply boundaries if enabled
        if (useBoundaries)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        }
        
        // Smooth follow
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, followSmoothing);
        
        // Set camera rotation
        transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
        
        // Handle dynamic zoom for orthographic camera (size adjustment)
        if (useDynamicZoom && cam != null && cam.orthographic && player1 != null && player2 != null)
        {
            float targetSize = CalculateOrthographicSize();
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref zoomVelocity, zoomSmoothing);
        }
    }
    
    bool HasValidTarget()
    {
        return player1 != null || player2 != null;
    }
    
    Vector3 GetCenterPoint()
    {
        // If only one player exists, follow that player
        if (player1 != null && player2 == null)
        {
            return player1.position;
        }
        else if (player1 == null && player2 != null)
        {
            return player2.position;
        }
        // If both players exist, follow the center point
        else if (player1 != null && player2 != null)
        {
            return (player1.position + player2.position) / 2f;
        }
        
        return transform.position;
    }
    
    Vector3 GetAngleOffset()
    {
        return GetAngleOffsetWithHeight(cameraHeight);
    }
    
    Vector3 GetAngleOffsetWithHeight(float height)
    {
        // Calculate the backward offset based on camera angle and height
        float angleInRadians = cameraAngle * Mathf.Deg2Rad;
        float horizontalDistance = height / Mathf.Tan(angleInRadians);
        
        // Offset backward (negative Z) and up
        return new Vector3(0f, height, -horizontalDistance);
    }
    
    float CalculateDynamicHeight()
    {
        if (player1 == null || player2 == null)
        {
            return cameraHeight;
        }
        
        // Calculate distance between players
        float distance = Vector3.Distance(player1.position, player2.position);
        
        // Calculate required height based on distance
        // For perspective camera, height affects how much is visible
        float requiredHeight = (distance / 2f) + zoomPadding;
        
        // Apply multiplier for perspective cameras
        if (cam != null && !cam.orthographic)
        {
            requiredHeight *= perspectiveZoomMultiplier;
        }
        
        // Clamp between min and max
        return Mathf.Clamp(requiredHeight, minZoom, maxZoom);
    }
    
    float CalculateOrthographicSize()
    {
        if (player1 == null || player2 == null)
        {
            return cam.orthographicSize;
        }
        
        // Calculate distance between players
        float distance = Vector3.Distance(player1.position, player2.position);
        
        // Calculate required size based on distance
        // Consider both horizontal and vertical distance
        Vector3 player1Pos = player1.position;
        Vector3 player2Pos = player2.position;
        
        float verticalDistance = Mathf.Abs(player1Pos.x - player2Pos.x);
        float horizontalDistance = Mathf.Abs(player1Pos.z - player2Pos.z);
        
        // Use the larger distance to ensure both players are visible
        float maxDistance = Mathf.Max(verticalDistance, horizontalDistance);
        
        // Calculate required size based on distance
        // Add padding to ensure players aren't at screen edge
        float requiredSize = (maxDistance / 2f) + zoomPadding;
        
        // Clamp between min and max
        return Mathf.Clamp(requiredSize, minZoom, maxZoom);
    }
    
    /// <summary>
    /// Set the first player target
    /// </summary>
    public void SetPlayer1(Transform target)
    {
        player1 = target;
    }
    
    /// <summary>
    /// Set the second player target
    /// </summary>
    public void SetPlayer2(Transform target)
    {
        player2 = target;
    }
    
    /// <summary>
    /// Set both player targets
    /// </summary>
    public void SetPlayers(Transform target1, Transform target2)
    {
        player1 = target1;
        player2 = target2;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw camera boundaries if enabled
        if (useBoundaries)
        {
            Gizmos.color = Color.yellow;
            
            Vector3 bottomLeft = new Vector3(minX, 0, minZ);
            Vector3 bottomRight = new Vector3(maxX, 0, minZ);
            Vector3 topLeft = new Vector3(minX, 0, maxZ);
            Vector3 topRight = new Vector3(maxX, 0, maxZ);
            
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
        
        // Draw lines to players
        if (player1 != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, player1.position);
        }
        
        if (player2 != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player2.position);
        }
        
        // Draw center point
        if (player1 != null && player2 != null)
        {
            Gizmos.color = Color.green;
            Vector3 center = GetCenterPoint();
            Gizmos.DrawWireSphere(center, 0.5f);
        }
    }
#endif
}
