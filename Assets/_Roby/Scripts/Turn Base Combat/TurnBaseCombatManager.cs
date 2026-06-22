using System.Collections.Generic;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

public class TurnBaseCombatManager : Singleton<TurnBaseCombatManager>
{
    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> AllCombatants { get; set; }

    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> TeamCombatants { get; set; }

    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> EnemyCombatants { get; set; }

    [TitleGroup("Runtime")]
    [ReadOnly]
    [ShowInInspector]
    public Queue<CombatantBase> TurnQueues { get; set; }

    [TitleGroup("Debug Functions")]
    [Button]
    public void StartCombat(List<CombatantBase> teamCombatants, List<CombatantBase> enemyCombatants)
    {
        
    }
}
