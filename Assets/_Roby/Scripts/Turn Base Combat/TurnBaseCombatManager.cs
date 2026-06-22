using System.Collections.Generic;
using RAXY.Utility;
using UnityEngine;

public class TurnBaseCombatManager : Singleton<TurnBaseCombatManager>
{
    public List<Combatant> AllCombatants;

    public List<Combatant> HeroCombatants;
    public List<Combatant> EnemyCombatants;
}
