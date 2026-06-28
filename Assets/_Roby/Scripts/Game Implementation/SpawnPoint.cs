using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnPoint : MonoBehaviour
{
    public string spawnPointId;
    public Transform spawnPoint;

    void OnTriggerEnter(Collider other)
    {
        var hero = other.GetComponent<HeroController>();
        if (hero == null)
            return;

        GameplayManager.Instance.NotifySpawnPointReached(this, hero);
    }

    void OnCollisionEnter(Collision collision)
    {
        var hero = collision.gameObject.GetComponent<HeroController>();
        if (hero == null)
            return;

        GameplayManager.Instance.NotifySpawnPointReached(this, hero);
    }
}
