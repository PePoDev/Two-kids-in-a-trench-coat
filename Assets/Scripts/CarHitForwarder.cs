using UnityEngine;

/// <summary>
/// Attach this script to child objects with colliders.
/// It forwards collision events to the CarHit script on the parent.
/// </summary>
public class CarHitForwarder : MonoBehaviour
{
    private CarHit parentCarHit;
    
    void Start()
    {
        // Find CarHit script in parent hierarchy
        parentCarHit = GetComponentInParent<CarHit>();
        
        if (parentCarHit == null)
        {
            Debug.LogWarning($"CarHitForwarder on {name}: No CarHit script found in parent hierarchy!");
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (parentCarHit != null)
        {
            parentCarHit.OnChildCollision(collision.gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (parentCarHit != null)
        {
            parentCarHit.OnChildTrigger(other.gameObject);
        }
    }
}
