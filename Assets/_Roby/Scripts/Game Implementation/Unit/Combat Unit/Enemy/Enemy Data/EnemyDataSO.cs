using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDataSO", menuName = "RAXY/Unit/Enemy/Enemy Data")]
public class EnemyDataSO : UnitDataSO
{
    [TitleGroup("Combat Data")]
    [HideLabel]
    public EnemyCombatDataSO enemyCombatDataSO;

    public override CombatDataBaseSO CombatDataSO => enemyCombatDataSO;
}
