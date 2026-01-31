using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Minimum speed for randomly generated cars")]
    public float minSpeed = 3f;
    
    [Tooltip("Maximum speed for randomly generated cars")]
    public float maxSpeed = 8f;
    
    [Tooltip("Fixed speed (used if not randomizing). Set to 0 to use random speed")]
    public float fixedSpeed = 0f;
    
    [Header("Loop Settings")]
    [Tooltip("Direction the car moves (forward/backward along its local axis)")]
    public Vector3 moveDirection = Vector3.forward;
    
    [Tooltip("Distance the car travels before looping back")]
    public float loopDistance = 50f;
    
    [Tooltip("If true, car teleports back instantly. If false, it moves back smoothly")]
    public bool instantLoop = true;
    
    [Header("Random Car Spawning")]
    [Tooltip("Array of car prefabs to randomly spawn")]
    public GameObject[] carPrefabs;
    
    [Tooltip("If true, spawn a random car model on start")]
    public bool spawnRandomCar = false;
    
    [Tooltip("Parent transform for spawned car model (leave null to use this transform)")]
    public Transform carModelParent;
    
    [Header("Destruction Settings")]
    [Tooltip("Destroy car after looping this many times (0 = never destroy)")]
    public int destroyAfterLoops = 0;
    
    private Vector3 startPosition;
    private float distanceTraveled = 0f;
    private float currentSpeed;
    private int loopCount = 0;
    private GameObject spawnedCarModel;

    void Start()
    {
        // Store the initial position of the car
        startPosition = transform.position;
        
        // Normalize the move direction to ensure consistent speed
        moveDirection = moveDirection.normalized;
        
        // Set speed (either fixed or random)
        if (fixedSpeed > 0f)
        {
            currentSpeed = fixedSpeed;
        }
        else
        {
            currentSpeed = Random.Range(minSpeed, maxSpeed);
        }
        
        // Spawn random car model if enabled
        if (spawnRandomCar && carPrefabs != null && carPrefabs.Length > 0)
        {
            SpawnRandomCar();
        }
        
        Debug.Log($"Car initialized with speed: {currentSpeed:F2}");
    }

    void Update()
    {
        // Move the car forward
        Vector3 movement = transform.TransformDirection(moveDirection) * currentSpeed * Time.deltaTime;
        transform.position += movement;
        
        // Track distance traveled
        distanceTraveled += currentSpeed * Time.deltaTime;
        
        // Check if we've reached the loop distance
        if (distanceTraveled >= loopDistance)
        {
            HandleLoop();
        }
    }
    
    void HandleLoop()
    {
        loopCount++;
        
        if (instantLoop)
        {
            // Instantly teleport back to start
            transform.position = startPosition;
            distanceTraveled = 0f;
        }
        else
        {
            // Smoothly move back (alternative behavior)
            transform.position = startPosition;
            distanceTraveled = 0f;
        }
        
        // Check if should destroy after certain loops
        if (destroyAfterLoops > 0 && loopCount >= destroyAfterLoops)
        {
            Debug.Log($"Car destroyed after {loopCount} loops");
            Destroy(gameObject);
        }
    }
    
    void SpawnRandomCar()
    {
        // Clean up existing car model if any
        if (spawnedCarModel != null)
        {
            Destroy(spawnedCarModel);
        }
        
        // Pick random prefab
        int randomIndex = Random.Range(0, carPrefabs.Length);
        GameObject selectedPrefab = carPrefabs[randomIndex];
        
        if (selectedPrefab == null)
        {
            Debug.LogWarning($"CarController: Car prefab at index {randomIndex} is null!");
            return;
        }
        
        // Determine parent
        Transform parent = carModelParent != null ? carModelParent : transform;
        
        // Instantiate the car model
        spawnedCarModel = Instantiate(selectedPrefab, parent);
        spawnedCarModel.transform.localPosition = Vector3.zero;
        spawnedCarModel.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"Spawned random car: {selectedPrefab.name}");
    }
    
    // Public method to change speed at runtime
    public void SetSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }
    
    // Public method to randomize speed again
    public void RandomizeSpeed()
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        Debug.Log($"Car speed randomized to: {currentSpeed:F2}");
    }
    
    // Public method to get current speed
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    // Public method to spawn a new random car
    public void RespawnRandomCar()
    {
        if (carPrefabs != null && carPrefabs.Length > 0)
        {
            SpawnRandomCar();
        }
    }
    
    // Public method to reset position
    public void ResetPosition()
    {
        transform.position = startPosition;
        distanceTraveled = 0f;
        loopCount = 0;
    }

    // Optional: Draw gizmos in the editor to visualize the path
    void OnDrawGizmos()
    {
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        Vector3 direction = transform.TransformDirection(moveDirection.normalized);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, start + direction * loopDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start, 0.5f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(start + direction * loopDistance, 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;
        
        // Display speed information
        #if UNITY_EDITOR
        string info = $"Speed: {currentSpeed:F1}\n";
        info += $"Loops: {loopCount}";
        if (destroyAfterLoops > 0)
        {
            info += $"/{destroyAfterLoops}";
        }
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, info);
        #endif
    }
}
