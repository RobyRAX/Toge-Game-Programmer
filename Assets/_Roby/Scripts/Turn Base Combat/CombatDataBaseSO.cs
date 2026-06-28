using System.Collections.Generic;
using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class CombatDataBaseSO : ScriptableObject
{
    public abstract List<CombatAttackBaseSO> Attacks { get; }
}
