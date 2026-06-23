using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackSO", menuName = "RAXY/Unit/Enemy/Attack SO")]
public class EnemyAttackSO : CombatAttackBaseSO
{
    [TitleGroup("Damage Profile")]
    [HideLabel]
    public EnemyDamageProfileProvider damageProfileProvider;
}
