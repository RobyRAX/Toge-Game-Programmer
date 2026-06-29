using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

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
    public float UltimateRegen => AttackSO is HeroAttackSO heroAttackSO ? heroAttackSO.ultimateRegen : 0f;

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
    AttackCameraPhase attackCameraPhase;
    bool attackCameraDetached;

    public async UniTask ExecuteAttackActionSequenceAsync(
        CombatantBase targetOpponent,
        CombatantBase targetTeam,
        AttackResult attackResult = default)
    {
        if (IsActionRunning || AttackActions == null || AttackActions.Count == 0)
            return;

        IsActionRunning = true;
        ElapsedTime = 0f;
        attackCameraPhase = AttackCameraPhase.Idle;
        attackCameraDetached = false;
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
        RestoreCombatCamera();
        CombatantOwner.StateMachine.ChangeState(CombatantState.Idle);
        IsActionRunning = false;
    }

    public void Tick(float deltaTime)
    {
        if (!IsActionRunning)
            return;

        ElapsedTime += deltaTime;
        EvaluateHits();
        EvaluateAttackCamera();
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

    void EvaluateAttackCamera()
    {
        var owner = CombatantOwner;
        if (AttackSO == null || !AttackSO.useAttackCamera || owner?.attackCamera == null)
            return;

        var param = AttackSO.attackCameraParam;
        if (param == null)
            return;

        // Start hanya boleh sekali per attack (Idle -> Active). Tanpa guard ini,
        // setelah endCamera men-set fase non-active, blok ini akan re-trigger tiap
        // frame (ElapsedTime tetap >= startCamera) sehingga attack camera "balik lagi".
        if (attackCameraPhase == AttackCameraPhase.Idle && ElapsedTime >= param.startCamera)
        {
            // Inject blend "to attack camera" sebelum Prioritize, supaya transisi pakai blend dari attack SO.
            TurnBaseCombatHelper.ChangeAttackCameraBlending(owner.attackCamera, param.blend);

            owner.attackCamera.transform.SetParent(owner.attackCameraParent);
            owner.attackCamera.transform.localPosition = Vector3.zero;
            owner.attackCamera.transform.localEulerAngles = Vector3.zero;

            owner.attackCamera.Prioritize();
            attackCameraPhase = AttackCameraPhase.Active;
        }

        if (attackCameraPhase == AttackCameraPhase.Active && !attackCameraDetached && ElapsedTime >= param.detachCamera)
            DetachAttackCamera();

        if (attackCameraPhase == AttackCameraPhase.Active && ElapsedTime >= param.endCamera)
            RestoreCombatCamera();
    }

    void DetachAttackCamera()
    {
        if (attackCameraDetached)
            return;

        CombatantOwner?.attackCamera?.transform.SetParent(null);
        attackCameraDetached = true;
    }

    void RestoreCombatCamera()
    {
        if (attackCameraPhase != AttackCameraPhase.Active)
            return;

        // Sengaja TIDAK menyentuh parent attack camera di sini.
        // Attack camera dibiarkan di posisi/parent terakhirnya dan baru di-home ulang
        // (SetParent + reset local transform) saat attack berikutnya yang pakai camera mulai.
        attackCameraPhase = AttackCameraPhase.Ended;
        TurnBaseCombatManager.Instance?.CameraDirector?.RestoreCombatCamera();
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

public enum AttackCameraPhase
{
    Idle,
    Active,
    Ended
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
