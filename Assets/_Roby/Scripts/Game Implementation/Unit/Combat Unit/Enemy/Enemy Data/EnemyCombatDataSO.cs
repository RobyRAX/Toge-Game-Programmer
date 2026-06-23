using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCombatDataSO", menuName = "RAXY/Unit/Enemy/Combat Data")]
public class EnemyCombatDataSO : CombatDataBaseSO
{
    public override List<CombatAttackBaseSO> Attacks => throw new System.NotImplementedException();
}
