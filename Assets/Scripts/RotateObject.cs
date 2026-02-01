using UnityEngine;

/// <summary>
/// Rotates an object continuously in a specific direction and optionally adds floating (bobbing) motion.
/// Perfect for indicating interactable items to the player.
/// </summary>
public class RotateObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 50f;
    
    [Tooltip("Rotation axis (e.g., Vector3.up for Y-axis, Vector3.right for X-axis)")]
    public Vector3 rotationAxis = Vector3.up;
    
    [Tooltip("Use local space for rotation instead of world space")]
    public bool useLocalSpace = true;
    
    [Header("Float/Bob Settings")]
    [Tooltip("Enable floating/bobbing effect")]
    public bool enableFloat = true;
    
    [Tooltip("Height of the floating motion")]
    public float floatAmplitude = 0.3f;
    
    [Tooltip("Speed of the floating motion")]
    public float floatSpeed = 2f;
    
    private Vector3 startPosition;
    private float floatTimer;
    
    void Start()
    {
        // Store starting position for float effect
        startPosition = transform.localPosition;
        
        // Randomize start time for variety when multiple objects use this
        floatTimer = Random.Range(0f, Mathf.PI * 2f);
    }
    
    void Update()
    {
        // Apply rotation
        if (useLocalSpace)
        {
            transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime, Space.World);
        }
        
        // Apply floating effect
        if (enableFloat)
        {
            floatTimer += Time.deltaTime * floatSpeed;
            float yOffset = Mathf.Sin(floatTimer) * floatAmplitude;
            transform.localPosition = startPosition + new Vector3(0, yOffset, 0);
        }
    }
}
