using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Car Prefabs")]
    [Tooltip("Array of car prefabs to randomly spawn")]
    public GameObject[] carPrefabs;
    
    [Header("Spawn Settings")]
    [Tooltip("Position where cars spawn")]
    public Transform spawnPoint;
    
    [Tooltip("Auto-spawn first car on start")]
    public bool spawnOnStart = true;
    
    [Header("Speed Settings")]
    [Tooltip("Minimum speed for spawned cars")]
    public float minSpeed = 3f;
    
    [Tooltip("Maximum speed for spawned cars")]
    public float maxSpeed = 8f;
    
    [Header("Movement Settings")]
    [Tooltip("Direction cars move (forward/backward along local axis)")]
    public Vector3 moveDirection = Vector3.forward;
    
    [Tooltip("Distance cars travel before being destroyed")]
    public float travelDistance = 50f;
    
    [Header("Auto-Respawn Settings")]
    [Tooltip("Automatically spawn new car when previous is destroyed")]
    public bool autoRespawn = true;
    
    [Tooltip("Use random delay between spawns")]
    public bool useRandomDelay = true;
    
    [Tooltip("Minimum delay before spawning next car (seconds)")]
    public float minRespawnDelay = 1f;
    
    [Tooltip("Maximum delay before spawning next car (seconds)")]
    public float maxRespawnDelay = 5f;
    
    [Tooltip("Fixed delay if not using random (seconds)")]
    public float fixedRespawnDelay = 2f;
    
    private GameObject currentCar;
    private float respawnTimer = 0f;
    private bool waitingToSpawn = false;
    private float currentRespawnDelay = 0f;
    
    void Start()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("CarSpawner: No car prefabs assigned!");
            enabled = false;
            return;
        }
        
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        if (spawnOnStart)
        {
            SpawnCar();
        }
    }
    
    void Update()
    {
        // Handle respawn timer
        if (waitingToSpawn)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= currentRespawnDelay)
            {
                SpawnCar();
                waitingToSpawn = false;
                respawnTimer = 0f;
            }
        }
    }
    
    public void SpawnCar()
    {
        // Pick random car prefab
        int randomIndex = Random.Range(0, carPrefabs.Length);
        GameObject selectedPrefab = carPrefabs[randomIndex];
        
        if (selectedPrefab == null)
        {
            Debug.LogWarning($"CarSpawner: Car prefab at index {randomIndex} is null!");
            return;
        }
        
        // Spawn the car
        currentCar = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Configure CarController component
        CarController carController = currentCar.GetComponent<CarController>();
        if (carController == null)
        {
            carController = currentCar.AddComponent<CarController>();
        }
        
        // Set random speed
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        carController.fixedSpeed = randomSpeed;
        carController.moveDirection = moveDirection;
        carController.loopDistance = travelDistance;
        carController.destroyAfterLoops = 1; // Destroy after reaching distance once
        carController.instantLoop = true; // Not used since we destroy, but set anyway
        
        // Subscribe to destruction to spawn new car
        StartCoroutine(MonitorCarDestruction(currentCar));
        
        Debug.Log($"Spawned car: {selectedPrefab.name} with speed: {randomSpeed:F2}");
    }
    
    System.Collections.IEnumerator MonitorCarDestruction(GameObject car)
    {
        // Wait until car is destroyed
        while (car != null)
        {
            yield return null;
        }
        
        // Car was destroyed
        Debug.Log("Car destroyed, preparing to spawn new one");
        
        if (autoRespawn)
        {
            // Calculate random or fixed delay
            if (useRandomDelay)
            {
                currentRespawnDelay = Random.Range(minRespawnDelay, maxRespawnDelay);
            }
            else
            {
                currentRespawnDelay = fixedRespawnDelay;
            }
            
            if (currentRespawnDelay > 0f)
            {
                waitingToSpawn = true;
                respawnTimer = 0f;
                Debug.Log($"Waiting {currentRespawnDelay:F2}s before spawning next car");
            }
            else
            {
                SpawnCar();
            }
        }
    }
    
    // Public method to manually spawn a new car
    public void ForceSpawnCar()
    {
        // Destroy current car if exists
        if (currentCar != null)
        {
            Destroy(currentCar);
        }
        
        SpawnCar();
    }
    
    void OnDrawGizmos()
    {
        if (spawnPoint == null)
            return;
        
        Vector3 start = spawnPoint.position;
        Vector3 direction = spawnPoint.TransformDirection(moveDirection.normalized);
        
        // Draw spawn point
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(start, 1f);
        
        // Draw travel path
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, start + direction * travelDistance);
        
        // Draw end point
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start + direction * travelDistance, 1f);
    }
}
