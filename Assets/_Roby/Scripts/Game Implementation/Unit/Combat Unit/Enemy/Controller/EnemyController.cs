using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyController : CombatUnitController
{
    public EnemyDataSO enemyDataSO;
    public EnemyCombatant enemyCombatant;

    public override CombatantBase CombatantCont { get => enemyCombatant; }

    [Button]
    void TestAttacked()
    {
        Invoke_OnAttacked();
    }
}
