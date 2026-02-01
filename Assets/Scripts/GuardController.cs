using UnityEngine;
using System.Collections.Generic;

public class GuardController : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("Array of waypoints that define the guard's patrol path")]
    public Transform[] patrolWaypoints;

    [Tooltip("Speed at which the guard moves along the path")]
    public float moveSpeed = 3f;

    [Tooltip("How close the guard needs to be to a waypoint before moving to the next")]
    public float waypointReachDistance = 0.5f;

    [Tooltip("If true, guard loops back to first waypoint. If false, ping-pong between waypoints")]
    public bool loopPath = true;

    [Tooltip("Time to wait at each waypoint before moving to the next")]
    public float waitTimeAtWaypoint = 1f;

    [Header("Vision Settings")]
    [Tooltip("Distance the guard can see")]
    public float visionRange = 10f;

    [Tooltip("Angle of the vision cone (in degrees)")]
    public float visionAngle = 90f;

    [Tooltip("Layer mask for detection (set to player layer)")]
    public LayerMask detectionLayer;

    [Tooltip("How often to check for player (in seconds)")]
    public float detectionInterval = 0.2f;

    [Header("Animation")]
    [Tooltip("Animator component for controlling guard animations")]
    public Animator animator;

    [Tooltip("Name of the boolean parameter for controlling walk/idle states")]
    public string isWalkingParameter = "Walk";

    [Header("Danger Zone Visualization")]
    [Tooltip("Show danger zone in-game (visible to players)")]
    public bool showDangerZone = true;

    [Tooltip("Material for the danger zone visualization")]
    public Material dangerZoneMaterial;

    [Tooltip("Color of danger zone when no detection")]
    public Color dangerZoneColorNormal = new Color(1f, 0f, 0f, 0.2f);

    [Tooltip("Color of danger zone when kid detected")]
    public Color dangerZoneColorDetected = new Color(1f, 0f, 0f, 0.5f);

    [Tooltip("Number of segments for vision cone mesh (higher = smoother)")]
    [Range(8, 64)]
    public int visionConeSegments = 24;

    [Header("Visual Debugging")]
    [Tooltip("Show vision cone in Scene view")]
    public bool showVisionGizmos = true;

    [Tooltip("Color of vision cone gizmos when no detection")]
    public Color visionGizmoNormal = new Color(1f, 1f, 0f, 0.3f);

    [Tooltip("Color of vision cone gizmos when kid detected")]
    public Color visionGizmoDetected = new Color(1f, 0f, 0f, 0.5f);

    // Private variables
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isMovingForward = true;
    private float detectionTimer = 0f;
    private bool kidDetected = false;
    
    // Danger zone visualization
    private GameObject dangerZoneObject;
    private MeshFilter dangerZoneMeshFilter;
    private MeshRenderer dangerZoneMeshRenderer;
    private Mesh dangerZoneMesh;

    void Start()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            Debug.LogWarning("GuardController: No patrol waypoints assigned!");
        }

        // Get Animator component if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("GuardController: No Animator component found!");
            }
        }

        // Start in idle state
        SetAnimationState(true);

        // Create danger zone visualization
        if (showDangerZone)
        {
            CreateDangerZoneVisualization();
        }
    }

    void Update()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            return;

        // Handle patrol movement
        PatrolPath();

        // Handle vision detection
        DetectPlayers();

        // Update danger zone visualization
        if (showDangerZone && dangerZoneMeshFilter != null)
        {
            UpdateDangerZone();
        }
    }

    void PatrolPath()
    {
        // Handle waiting at waypoint
        if (isWaiting)
        {
            // Set idle animation
            SetAnimationState(true);

            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextWaypoint();

                // Set walk animation
                SetAnimationState(false);
            }
            return;
        }

        // Set walk animation when moving
        SetAnimationState(false);

        // Get current target waypoint
        Transform targetWaypoint = patrolWaypoints[currentWaypointIndex];

        if (targetWaypoint == null)
        {
            Debug.LogWarning($"GuardController: Waypoint {currentWaypointIndex} is null!");
            return;
        }

        // Move towards waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // Check if reached waypoint
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distance <= waypointReachDistance)
        {
            isWaiting = true;
        }
    }

    void MoveToNextWaypoint()
    {
        if (loopPath)
        {
            // Loop back to start
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
        }
        else
        {
            // Ping-pong between waypoints
            if (isMovingForward)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= patrolWaypoints.Length)
                {
                    currentWaypointIndex = patrolWaypoints.Length - 2;
                    isMovingForward = false;
                }
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    isMovingForward = true;
                }
            }

            // Clamp to valid range
            currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, patrolWaypoints.Length - 1);
        }
    }

    void DetectPlayers()
    {
        // Update detection timer
        detectionTimer += Time.deltaTime;
        if (detectionTimer < detectionInterval)
            return;

        detectionTimer = 0f;
        kidDetected = false;

        // Find all colliders in vision range
        Collider[] colliders = Physics.OverlapSphere(transform.position, visionRange, detectionLayer);

        foreach (Collider collider in colliders)
        {
            // Check if it's a kid player (has TopDownPlayer3D component)
            TopDownPlayer3D kid = collider.GetComponent<TopDownPlayer3D>();
            if (kid != null)
            {
                // Check if within vision cone
                Vector3 directionToTarget = (collider.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                if (angleToTarget <= visionAngle / 2f)
                {
                    // Check if there's a clear line of sight (no obstacles)
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, directionToTarget, out hit, visionRange))
                    {
                        if (hit.collider == collider)
                        {
                            // Kid detected!
                            kidDetected = true;
                            OnKidDetected(kid);
                            break;
                        }
                    }
                }
            }
        }
    }

    void OnKidDetected(TopDownPlayer3D kid)
    {
        Debug.Log($"Guard detected kid player: {kid.gameObject.name}");

        // Trigger game over
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogWarning("GameManager not found! Cannot trigger game over.");
        }
    }

    void SetAnimationState(bool isIdle)
    {
        if (animator == null)
            return;

        // Set the isWalking parameter (true when walking, false when idle)
        animator.SetBool(isWalkingParameter, !isIdle);
    }

    void CreateDangerZoneVisualization()
    {
        // Create a child object for the danger zone
        dangerZoneObject = new GameObject("DangerZone");
        dangerZoneObject.transform.SetParent(transform);
        dangerZoneObject.transform.localPosition = Vector3.zero;
        dangerZoneObject.transform.localRotation = Quaternion.identity;

        // Add mesh components
        dangerZoneMeshFilter = dangerZoneObject.AddComponent<MeshFilter>();
        dangerZoneMeshRenderer = dangerZoneObject.AddComponent<MeshRenderer>();

        // Create and assign material
        if (dangerZoneMaterial != null)
        {
            dangerZoneMeshRenderer.material = dangerZoneMaterial;
        }
        else
        {
            // Create a basic transparent material if none assigned
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha blend
            mat.renderQueue = 3000;
            dangerZoneMeshRenderer.material = mat;
        }

        // Create the vision cone mesh
        dangerZoneMesh = CreateVisionConeMesh();
        dangerZoneMeshFilter.mesh = dangerZoneMesh;

        // Set initial color
        UpdateDangerZoneColor();
    }

    Mesh CreateVisionConeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "VisionConeMesh";

        int segments = visionConeSegments;
        float angleStep = visionAngle / segments;
        float startAngle = -visionAngle / 2f;

        // Calculate vertices with raycasting to detect obstacles
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        // Origin point at guard position
        vertices[0] = Vector3.zero + Vector3.up * 0.1f; // Slightly above ground

        // Create arc vertices with raycasting
        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + (angleStep * i);
            float rad = angle * Mathf.Deg2Rad;
            
            Vector3 direction = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
            float distance = visionRange;

            // Raycast from guard position in this direction
            RaycastHit hit;
            Vector3 worldOrigin = transform.position + Vector3.up * 0.5f;
            Vector3 worldDirection = transform.TransformDirection(direction);

            if (Physics.Raycast(worldOrigin, worldDirection, out hit, visionRange))
            {
                // Hit an obstacle, shorten the distance
                distance = hit.distance;
            }

            float x = Mathf.Sin(rad) * distance;
            float z = Mathf.Cos(rad) * distance;
            
            vertices[i + 1] = new Vector3(x, 0.1f, z);
        }

        // Create triangles
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    void UpdateDangerZone()
    {
        // Recreate the mesh with current raycast data
        if (dangerZoneMesh != null)
        {
            dangerZoneMesh.Clear();
            Mesh newMesh = CreateVisionConeMesh();
            dangerZoneMesh.vertices = newMesh.vertices;
            dangerZoneMesh.triangles = newMesh.triangles;
            dangerZoneMesh.RecalculateNormals();
            dangerZoneMesh.RecalculateBounds();
        }

        // Update color
        UpdateDangerZoneColor();
    }

    void UpdateDangerZoneColor()
    {
        if (dangerZoneMeshRenderer == null)
            return;

        Color targetColor = kidDetected ? dangerZoneColorDetected : dangerZoneColorNormal;
        dangerZoneMeshRenderer.material.color = targetColor;
    }

    void OnDestroy()
    {
        // Clean up danger zone object
        if (dangerZoneObject != null)
        {
            Destroy(dangerZoneObject);
        }
    }

    // Draw debug gizmos in the editor
    void OnDrawGizmos()
    {
        // Draw patrol path
        if (patrolWaypoints != null && patrolWaypoints.Length > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolWaypoints.Length; i++)
            {
                if (patrolWaypoints[i] != null)
                {
                    // Draw waypoint sphere
                    Gizmos.DrawWireSphere(patrolWaypoints[i].position, 0.5f);

                    // Draw line to next waypoint
                    int nextIndex = loopPath ? (i + 1) % patrolWaypoints.Length : i + 1;
                    if (nextIndex < patrolWaypoints.Length && patrolWaypoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[nextIndex].position);
                    }
                }
            }

            // Highlight current waypoint
            if (Application.isPlaying && currentWaypointIndex < patrolWaypoints.Length
                && patrolWaypoints[currentWaypointIndex] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(patrolWaypoints[currentWaypointIndex].position, 0.7f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showVisionGizmos)
            return;

        // Draw vision cone
        Color visionColor = kidDetected ? visionGizmoDetected : visionGizmoNormal;
        Gizmos.color = visionColor;

        Vector3 forward = Application.isPlaying ? transform.forward : transform.forward;
        Vector3 position = transform.position;

        // Draw vision range sphere
        Gizmos.DrawWireSphere(position, visionRange);

        // Draw vision cone
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * forward * visionRange;

        Gizmos.DrawLine(position, position + leftBoundary);
        Gizmos.DrawLine(position, position + rightBoundary);
        Gizmos.DrawLine(position, position + forward * visionRange);

        // Draw arc for vision cone
        int segments = 20;
        float angleStep = visionAngle / segments;
        Vector3 prevPoint = position + leftBoundary;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -visionAngle / 2f + angleStep * i;
            Vector3 nextPoint = position + Quaternion.Euler(0, angle, 0) * forward * visionRange;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}
