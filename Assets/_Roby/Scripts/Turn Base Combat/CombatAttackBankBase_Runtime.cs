using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;

public abstract class CombatAttackBankBase_Runtime
{
    public virtual CombatantBase CombatantOwner { get; set; }

    [ShowInInspector]
    [HideReferenceObjectPicker]
    public virtual List<Attack_Runtime> Attacks { get; set; }

    public CombatAttackBankBase_Runtime(CombatantBase combatantOwner)
    {
        CombatantOwner = combatantOwner;
    }

    public void Tick(float deltaTime)
    {
        if (Attacks == null)
            return;

        foreach (var attack in Attacks)
            attack?.Tick(deltaTime);
    }

    [Button("Rebuild Attack Bank")]
    public void RebuildBank()
    {
        if (Attacks == null)
            return;

        foreach (var attack in Attacks)
            attack?.BuildAttack();
    }
}

public class Attack_Runtime
{
    public event Action OnActionStarted;
    public event Action OnActionEnded;

    public CombatantBase CombatantOwner { get; set; }

    [PropertyOrder(-1)]
    [ShowInInspector]
    public CombatAttackBaseSO AttackSO { get; set; }
    public int StaminaCost => AttackSO.staminaCost;

    [HideReferenceObjectPicker]
    public DamageProfileWithAttribute damageProfile;

    [HideReferenceObjectPicker]
    public List<AttackActionBase_Runtime> AttackActions;
    public bool IsActionRunning { get; set; }

    public async UniTask ExecuteAttackActionSequenceAsync(
        CombatantBase targetOpponent,
        CombatantBase targetTeam,
        AttackResult attackResult = default)
    {
        if (IsActionRunning || AttackActions == null || AttackActions.Count == 0)
            return;

        IsActionRunning = true;
        OnActionStarted?.Invoke();
        CombatantOwner.StateMachine.ChangeState(CombatantState.Attack);

        foreach (var action in AttackActions)
        {
            if (action == null)
                continue;

            await action.Start(targetOpponent, targetTeam, attackResult);
        }

        EndAttackActionSequence();
        OnActionEnded?.Invoke();
    }

    public void EndAttackActionSequence()
    {
        CombatantOwner.StateMachine.ChangeState(CombatantState.Idle);
        IsActionRunning = false;
    }

    public void Tick(float deltaTime)
    {
        if (!IsActionRunning || AttackActions == null)
            return;

        foreach (var action in AttackActions)
            action?.Tick(deltaTime);
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

        for (int i = 0; i < AttackSO.attackActionEntries.Count; i++)
        {
            var entry = AttackSO.attackActionEntries[i];
            if (entry?.AttackActionSO == null)
                continue;

            var actionRuntime = entry.AttackActionSO.CreateRuntime(entry, CombatantOwner);
            if (actionRuntime == null)
                continue;

            actionRuntime.SetHits(CollectHitsForActionIndex(i));
            AttackActions.Add(actionRuntime);
        }
    }

    public void RefreshDamageProfile()
    {
        damageProfile = CombatantOwner.GetDamageProfile(AttackSO);
    }

    List<HitEntry> CollectHitsForActionIndex(int actionIndex)
    {
        var result = new List<HitEntry>();
        if (AttackSO?.hitEntries == null)
            return result;

        foreach (var hit in AttackSO.hitEntries)
        {
            if (hit != null && hit.attackActionIndex == actionIndex)
                result.Add(hit);
        }

        return result;
    }
}

public struct CombatHitInfo
{
    public CombatantBase Attacker;
    public CombatantBase Defender;
    public int HitIndex;
    public float DamageProportion;
    public float HitDamage;
    public HitSeverity Severity;
}
