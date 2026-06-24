using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackSO", menuName = "RAXY/Unit/Enemy/Attack SO")]
public class EnemyAttackSO : CombatAttackBaseSO
{
    [TitleGroup("Damage Profile")]
    [HideLabel]
    [SerializeField]
    DamageProfile damageProfileEntry;

    public override DamageProfileWithAttribute DamageProfile
    {
        get
        {
            var newDamageProfile = new DamageProfileWithAttribute();
            newDamageProfile.flatDamage = damageProfileEntry.flatDamage;
            newDamageProfile.multiplierDamage = damageProfileEntry.multiplierDamage;
            newDamageProfile.attribute = ToGaProTest.Shared.StatAttribute.Attack;

            return newDamageProfile;
        }
    }
}
