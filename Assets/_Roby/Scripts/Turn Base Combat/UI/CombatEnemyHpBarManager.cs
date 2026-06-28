using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatEnemyHpBarManager : MonoBehaviour
{
    [TitleGroup("Setting")]
    [SerializeField]
    CombatWorldHpBar worldHpBarPrefab;

    [TitleGroup("Setting")]
    [SerializeField]
    float worldYOffset = 2f;

    readonly Dictionary<CombatantBase, CombatWorldHpBar> barByCombatant = new();
    TurnBaseCombatManager manager;

    public void Setup(TurnBaseCombatManager manager)
    {
        Teardown();

        this.manager = manager;
        if (manager == null || worldHpBarPrefab == null)
            return;

        manager.OnCombatantStatsChanged += HandleStatsChanged;

        if (manager.EnemyCombatants == null)
            return;

        foreach (var combatant in manager.EnemyCombatants)
        {
            if (combatant == null)
                continue;

            var bar = Instantiate(worldHpBarPrefab, combatant.transform);
            bar.transform.localPosition = Vector3.up * worldYOffset;
            bar.Setup(combatant);
            bar.SetVisible(combatant.IsAlive || combatant.DisplayedHp > 0f);
            barByCombatant[combatant] = bar;
        }
    }

    void HandleStatsChanged(CombatantBase combatant)
    {
        if (combatant == null || !barByCombatant.TryGetValue(combatant, out var bar))
            return;

        bar.Refresh();
        bar.SetVisible(combatant.IsAlive || combatant.DisplayedHp > 0f);
    }

    public void Teardown()
    {
        if (manager != null)
            manager.OnCombatantStatsChanged -= HandleStatsChanged;

        foreach (var pair in barByCombatant)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        barByCombatant.Clear();
        manager = null;
    }

    void OnDestroy()
    {
        Teardown();
    }
}
