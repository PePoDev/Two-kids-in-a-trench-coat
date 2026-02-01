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

    [Header("Detection Response")]
    [Tooltip("What happens when kid is detected")]
    public UnityEngine.Events.UnityEvent onKidDetected;

    [Header("Visual Debugging")]
    [Tooltip("Show vision cone in Scene view")]
    public bool showVisionGizmos = true;

    [Tooltip("Color of vision cone when no detection")]
    public Color visionColorNormal = new Color(1f, 1f, 0f, 0.3f);

    [Tooltip("Color of vision cone when kid detected")]
    public Color visionColorDetected = new Color(1f, 0f, 0f, 0.5f);

    // Private variables
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isMovingForward = true;
    private float detectionTimer = 0f;
    private bool kidDetected = false;

    void Start()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            Debug.LogWarning("GuardController: No patrol waypoints assigned!");
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
    }

    void PatrolPath()
    {
        // Handle waiting at waypoint
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextWaypoint();
            }
            return;
        }

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

        // Invoke the event
        if (onKidDetected != null)
        {
            onKidDetected.Invoke();
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
        Color visionColor = kidDetected ? visionColorDetected : visionColorNormal;
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
