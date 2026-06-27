using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackBank_Runtime : CombatAttackBankBase_Runtime
{
    EnemyCombatant enemyCombatant;
    EnemyCombatDataSO combatDataSO;

    public EnemyAttackBank_Runtime(EnemyCombatant enemyCombatant) : base(enemyCombatant)
    {
        this.enemyCombatant = enemyCombatant;
        combatDataSO = enemyCombatant.CombatDataSO as EnemyCombatDataSO;
        BuildAttacks();
    }

    public void BuildAttacks()
    {
        if (enemyCombatant == null || combatDataSO == null)
            return;

        Attacks = new();
        foreach (var attackSO in combatDataSO.Attacks)
        {
            if (attackSO == null)
                continue;

            var newAttack = new Attack_Runtime(attackSO, enemyCombatant);
            if (newAttack.damageProfile == null)
                Debug.LogWarning($"{nameof(EnemyAttackBank_Runtime)}: {attackSO.name} has no damage profile.");

            Attacks.Add(newAttack);
        }
    }
}
