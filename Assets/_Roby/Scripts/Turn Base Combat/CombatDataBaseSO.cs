using System.Collections.Generic;
using UnityEngine;

public abstract class CombatDataBaseSO : ScriptableObject
{
    public abstract List<CombatAttackBaseSO> Attacks { get; }
}
