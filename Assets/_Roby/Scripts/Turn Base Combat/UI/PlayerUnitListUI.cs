using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitListUI : MonoBehaviour
{
    public PlayerUnitStatusUI unitA;
    public PlayerUnitStatusUI unitB;
    public PlayerUnitStatusUI unitC;

    readonly List<PlayerUnitStatusUI> unitSlots = new();

    void Awake()
    {
        unitSlots.Clear();
        if (unitA != null) unitSlots.Add(unitA);
        if (unitB != null) unitSlots.Add(unitB);
        if (unitC != null) unitSlots.Add(unitC);
    }

    public void Setup(TurnBaseCombatManager manager)
    {
        if (manager?.PlayerCombatants == null)
            return;

        for (int i = 0; i < unitSlots.Count; i++)
        {
            var slot = unitSlots[i];
            if (slot == null)
                continue;

            if (i < manager.PlayerCombatants.Count && manager.PlayerCombatants[i] != null)
            {
                slot.gameObject.SetActive(true);
                slot.Setup(manager.PlayerCombatants[i]);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    public void SetCurrentPlayerUnit(CombatantBase current)
    {
        foreach (var slot in unitSlots)
            slot?.SetCurrent(slot.Combatant == current);
    }

    public void RefreshAllStats()
    {
        foreach (var slot in unitSlots)
            slot?.RefreshStats();
    }

    public void RefreshStatsFor(CombatantBase combatant)
    {
        foreach (var slot in unitSlots)
        {
            if (slot?.Combatant == combatant)
                slot.RefreshStats();
        }
    }
}
