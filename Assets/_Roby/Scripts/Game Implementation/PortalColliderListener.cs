using UnityEngine;

public class PortalColliderListener : MonoBehaviour
{
    public Portal Portal { get; set; }

    void OnTriggerEnter(Collider other)
    {
        Portal?.HandleHeroEntered(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        Portal?.HandleHeroEntered(collision.collider);
    }
}
