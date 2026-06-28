using DamageNumbersPro;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatDamageNumberSpawner : MonoBehaviour
{
    [TitleGroup("Setting")]
    [SerializeField]
    DamageNumber damageNumberPrefab;

    [TitleGroup("Setting")]
    [SerializeField]
    float worldYOffset = 2f;

    TurnBaseCombatManager manager;

    public void Setup(TurnBaseCombatManager manager)
    {
        Teardown();

        this.manager = manager;
        if (manager == null || damageNumberPrefab == null)
            return;

        manager.OnCombatantDoHit += HandleDoHit;
    }

    void HandleDoHit(CombatHitInfo hitInfo)
    {
        var defender = hitInfo.Defender;
        if (defender == null)
            return;

        Vector3 spawnPos = defender.transform.position + Vector3.up * worldYOffset;
        damageNumberPrefab.Spawn(spawnPos, hitInfo.HitDamage, defender.transform);
    }

    public void Teardown()
    {
        if (manager != null)
            manager.OnCombatantDoHit -= HandleDoHit;

        manager = null;
    }

    void OnDestroy()
    {
        Teardown();
    }
}
