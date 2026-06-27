using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "EnemyCombatDataSO", menuName = "RAXY/Unit/Enemy/Combat Data")]
public class EnemyCombatDataSO : CombatDataBaseSO
{
    [SerializeField]
    [ListDrawerSettings(ShowIndexLabels = true)]
    List<EnemyAttackSO> attacks;
    public override List<CombatAttackBaseSO> Attacks
    {
        get
        {
            var temp = new List<CombatAttackBaseSO>();
            foreach (var attack in attacks)
            {
                temp.Add(attack);
            }

            return temp;
        }
    }
}
