using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

public class EnemyCombatant : CombatantBase
{
    public EnemyCombatDataSO enemyCombatDataSO;
    public override CombatDataBaseSO CombatDataSO { get => enemyCombatDataSO; }

    EnemyAttackBank_Runtime enemyAttackBank;
    public override CombatAttackBankBase_Runtime AttackBank
    {
        get => enemyAttackBank;
        set => enemyAttackBank = value as EnemyAttackBank_Runtime;
    }

    EnemyController _enemyCont;

    public void Init(EnemyController enemyCont)
    {
        _enemyCont = enemyCont;
        enemyCombatDataSO = enemyCont.enemyDataSO.enemyCombatDataSO;
        Level = enemyCont.EnemyGroup.Level;

        AnimationClips = enemyCont.AnimationClips;

        UpdateStatContainer();
        AttackBank = new EnemyAttackBank_Runtime(this);
        SetAlive();
    }

    public void SetAlive(bool fullHp = true, float hp = 1)
    {
        if (fullHp)
            CurrentHp = StatContainer.GetTotalValue(StatAttribute.MaxHp);
        else
            CurrentHp = hp;

        IsAlive = true;
    }

    public void UpdateStatContainer()
    {
        if (StatContainer == null)
            StatContainer = new StatContainer_Runtime(GameplayConfig.Instance);
        
        _enemyCont.enemyDataSO.StatGrowth.ApplyMainStatsTo(StatContainer, Level);
    }
}
