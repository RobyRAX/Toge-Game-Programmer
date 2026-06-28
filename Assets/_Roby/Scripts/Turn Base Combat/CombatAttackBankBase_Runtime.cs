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

    [TitleGroup("Status")]
    [ShowInInspector]
    float ElapsedTime { get; set; }

    List<bool> hitFired;
    AttackResult currentAttackResult;
    CombatantBase currentTargetOpponent;
    int lastHitIndex = -1;

    public async UniTask ExecuteAttackActionSequenceAsync(
        CombatantBase targetOpponent,
        CombatantBase targetTeam,
        AttackResult attackResult = default)
    {
        if (IsActionRunning || AttackActions == null || AttackActions.Count == 0)
            return;

        IsActionRunning = true;
        ElapsedTime = 0f;
        PrepareHits(targetOpponent, attackResult);
        OnActionStarted?.Invoke();
        CombatantOwner.StateMachine.ChangeState(CombatantState.Attack);

        foreach (var action in AttackActions)
        {
            if (action == null)
                continue;

            await action.Start(targetOpponent, targetTeam);
        }

        FlushRemainingHits();
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
        if (!IsActionRunning)
            return;

        ElapsedTime += deltaTime;
        EvaluateHits();
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

            AttackActions.Add(actionRuntime);
        }
    }

    public void RefreshDamageProfile()
    {
        damageProfile = CombatantOwner.GetDamageProfile(AttackSO);
    }

    void PrepareHits(CombatantBase targetOpponent, AttackResult attackResult)
    {
        currentAttackResult = attackResult;
        currentTargetOpponent = targetOpponent;
        hitFired = null;
        lastHitIndex = -1;

        if (AttackSO?.hitEntries == null || AttackSO.hitEntries.Count == 0)
            return;

        hitFired = new List<bool>(AttackSO.hitEntries.Count);
        for (int i = 0; i < AttackSO.hitEntries.Count; i++)
            hitFired.Add(false);

        for (int i = AttackSO.hitEntries.Count - 1; i >= 0; i--)
        {
            if (AttackSO.hitEntries[i] == null)
                continue;

            lastHitIndex = i;
            break;
        }
    }

    void EvaluateHits()
    {
        if (hitFired == null || AttackSO?.hitEntries == null)
            return;

        for (int i = 0; i < hitFired.Count; i++)
        {
            if (hitFired[i])
                continue;

            var hit = AttackSO.hitEntries[i];
            if (hit == null)
            {
                hitFired[i] = true;
                continue;
            }

            if (ElapsedTime >= hit.timeToCall)
            {
                FireHit(i, hit, i == lastHitIndex);
                hitFired[i] = true;
            }
        }
    }

    void FlushRemainingHits()
    {
        if (hitFired == null || AttackSO?.hitEntries == null)
            return;

        for (int i = 0; i < hitFired.Count; i++)
        {
            if (hitFired[i])
                continue;

            var hit = AttackSO.hitEntries[i];
            if (hit == null)
            {
                hitFired[i] = true;
                continue;
            }

            FireHit(i, hit, i == lastHitIndex);
            hitFired[i] = true;
        }
    }

    void FireHit(int hitIndex, HitEntry hit, bool isLastHit)
    {
        var defender = currentAttackResult.Defender != null ? currentAttackResult.Defender : currentTargetOpponent;
        if (defender == null)
            return;

        float hitDamage = currentAttackResult.ReceivedDamage * hit.damageProportion / 100f;
        float maxHp = defender.StatContainer != null ? defender.StatContainer.GetTotalValue(StatAttribute.MaxHp) : 0f;
        var severity = CombatantStateMachine.ResolveHitSeverity(hitDamage, maxHp);

        bool playDeath = isLastHit
            && currentAttackResult.IsDefenderDead
            && !defender.IsAlive;

        if (playDeath)
            defender.StateMachine?.StartDeath();
        else
            defender.StateMachine?.ChangeHitState(severity);

        CombatantOwner?.RaiseDoHit(new CombatHitInfo
        {
            Attacker = CombatantOwner,
            Defender = defender,
            HitIndex = hitIndex,
            IsLastHit = isLastHit,
            DamageProportion = hit.damageProportion,
            HitDamage = hitDamage,
            Severity = severity,
        });
    }
}

public struct CombatHitInfo
{
    public CombatantBase Attacker;
    public CombatantBase Defender;
    public int HitIndex;
    public bool IsLastHit;
    public float DamageProportion;
    public float HitDamage;
    public HitSeverity Severity;
}
