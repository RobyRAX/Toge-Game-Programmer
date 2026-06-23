using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyController : CombatUnitController
{
    public EnemyGroup EnemyGroup { get; set; }
    public EnemyDataSO enemyDataSO;
    public EnemyCombatant enemyCombatant;

    public override CombatantBase CombatantCont { get => enemyCombatant; }

    public override async UniTask Init()
    {
        await base.Init();

        enemyCombatant = GetComponent<EnemyCombatant>();
        enemyCombatant.Init(this);
        FirstInitDone = true;
    }

    [Button]
    void TestAttacked()
    {
        Invoke_OnAttacked();
    }
}
