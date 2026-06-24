using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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

    [HideReferenceObjectPicker]
    public DamageProfileWithAttribute damageProfile; 

    [HideReferenceObjectPicker]
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
        AttackSO = attackSO;
        CombatantOwner = combatantOwner;
        BuildAttack();
    }

    public void BuildAttack()
    {
        if (AttackSO == null || CombatantOwner == null)
            return;

        damageProfile = AttackSO.DamageProfile;
        if (damageProfile == null)
            damageProfile = CombatantOwner.GetDamageProfile(AttackSO);

        AttackActions = new();
        if (AttackSO.attackActionEntries == null)
            return;

        foreach (var entry in AttackSO.attackActionEntries)
        {
            if (entry?.AttackActionSO == null)
                continue;

            var actionRuntime = entry.AttackActionSO.CreateRuntime(entry, CombatantOwner);
            if (actionRuntime == null)
                continue;

            AttackActions.Add(actionRuntime);
        }
    }

    public void RefreshDamageProfile()
    {
        damageProfile = CombatantOwner.GetDamageProfile(AttackSO);
    }
}