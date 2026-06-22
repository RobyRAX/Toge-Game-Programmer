using System;
using System.Collections.Generic;

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
}