using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;

public abstract class AttackActionBase_Runtime
{
    public CombatantBase CombatantOwner { get; set; }

    [HideReferenceObjectPicker]
    public AttackActionEntry entry;

    [TitleGroup("Status")]
    [ShowInInspector]
    public bool IsRunning { get; set; }

    [TitleGroup("Status")]
    [ShowInInspector]
    public bool IsCompleted { get; set; }

    [TitleGroup("Status")]
    [ShowInInspector]
    public float ElapsedTime { get; protected set; }

    [HorizontalGroup("Status/Hit")]
    [HideReferenceObjectPicker]
    [ShowInInspector]
    public List<HitEntry> Hits { get; private set; }

    [HorizontalGroup("Status/Hit")]
    [ShowInInspector]
    List<bool> hitFired;
    
    AttackResult currentAttackResult;
    CombatantBase currentTargetOpponent;

    public AttackActionBase_Runtime(AttackActionEntry entry, CombatantBase combatantOwner)
    {
        this.entry = entry;
        CombatantOwner = combatantOwner;
    }

    public void SetHits(List<HitEntry> hits) => Hits = hits;

    public async UniTask Start(CombatantBase targetOpponent, CombatantBase targetTeam, AttackResult attackResult = default)
    {
        if (IsRunning)
            return;

        IsRunning = true;
        IsCompleted = false;
        ElapsedTime = 0f;
        PrepareHits(targetOpponent, attackResult);

        await ExecuteAsync(targetOpponent, targetTeam);

        FlushRemainingHits();
        IsRunning = false;
        IsCompleted = true;
    }

    public virtual void Tick(float deltaTime)
    {
        if (!IsRunning)
            return;

        ElapsedTime += deltaTime;
        EvaluateHits();
    }

    protected abstract UniTask ExecuteAsync(CombatantBase targetOpponent, CombatantBase targetTeam);

    protected T GetParameter<T>() where T : AttackActionParameterBase
        => entry?.AttackActionParameter as T;

    void PrepareHits(CombatantBase targetOpponent, AttackResult attackResult)
    {
        currentAttackResult = attackResult;
        currentTargetOpponent = targetOpponent;
        hitFired = null;

        if (Hits == null || Hits.Count == 0)
            return;

        hitFired = new List<bool>(Hits.Count);
        for (int i = 0; i < Hits.Count; i++)
            hitFired.Add(false);
    }

    void EvaluateHits()
    {
        if (hitFired == null || Hits == null)
            return;

        for (int i = 0; i < hitFired.Count; i++)
        {
            if (hitFired[i])
                continue;

            var hit = Hits[i];
            if (hit == null)
            {
                hitFired[i] = true;
                continue;
            }

            if (ElapsedTime >= hit.timeToCall)
            {
                FireHit(i, hit);
                hitFired[i] = true;
            }
        }
    }

    void FlushRemainingHits()
    {
        if (hitFired == null || Hits == null)
            return;

        for (int i = 0; i < hitFired.Count; i++)
        {
            if (hitFired[i])
                continue;

            var hit = Hits[i];
            if (hit == null)
            {
                hitFired[i] = true;
                continue;
            }

            FireHit(i, hit);
            hitFired[i] = true;
        }
    }

    void FireHit(int hitIndex, HitEntry hit)
    {
        var defender = currentAttackResult.Defender != null ? currentAttackResult.Defender : currentTargetOpponent;
        if (defender == null)
            return;

        float hitDamage = currentAttackResult.ReceivedDamage * hit.damageProportion / 100f;
        float maxHp = defender.StatContainer != null ? defender.StatContainer.GetTotalValue(StatAttribute.MaxHp) : 0f;
        var severity = CombatantStateMachine.ResolveHitSeverity(hitDamage, maxHp);

        defender.StateMachine?.ChangeHitState(severity);
        CombatantOwner?.RaiseDoHit(new CombatHitInfo
        {
            Attacker = CombatantOwner,
            Defender = defender,
            HitIndex = hitIndex,
            DamageProportion = hit.damageProportion,
            HitDamage = hitDamage,
            Severity = severity,
        });
    }
}
