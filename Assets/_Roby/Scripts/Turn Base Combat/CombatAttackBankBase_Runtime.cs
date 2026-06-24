using System;
using System.Collections.Generic;
using ToGaProTest.Shared;

public abstract class CombatAttackBankBase_Runtime
{
    public virtual CombatantBase CombatantOwner { get; set; }
    public abstract List<Attack_Runtime> Attacks { get; }

    public CombatAttackBankBase_Runtime(CombatantBase combatantOwner)
    {
        CombatantOwner = combatantOwner;
    }
}

public class Attack_Runtime
{
    public event Action OnActionStarted;
    public event Action OnActionEnded;

    public CombatantBase CombatantOwner { get; set; }

    public CombatAttackBaseSO AttackSO { get; set; }
    public int StaminaCost => AttackSO.staminaCost;

    public DamageProfileWithAttribute damageProfile; 

    public List<AttackActionBase_Runtime> AttackActions;
    public bool IsActionRunning { get; set; }

    public void ExecuteAttackActionSequence()
    {
        IsActionRunning = true;
    }

    public void EndAttackActionSequence()
    {
        IsActionRunning = false;
    }

    public Attack_Runtime() { }
    public Attack_Runtime(CombatAttackBaseSO attackSO, CombatantBase combatantOwner)
    {
        BuildAttack();
    }

    public void BuildAttack()
    {
        if (AttackSO == null || CombatantOwner == null)
            return;

        damageProfile = AttackSO.DamageProfile;
        if (damageProfile == null)
        {
            damageProfile = CombatantOwner.GetDamageProfile(AttackSO);
        }
        
        AttackActions = new();
    }

    public void RefreshDamageProfile()
    {
        damageProfile = CombatantOwner.GetDamageProfile(AttackSO);
    }
}