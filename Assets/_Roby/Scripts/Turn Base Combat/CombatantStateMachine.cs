using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatantStateMachine
{
    public const float DefaultFadeDuration = 0.2f;

    readonly CombatantBase owner;
    readonly AnimancerController animancerCont;

    CancellationTokenSource hitCts;

    bool deathStarted;
    float deathStartedAt;
    float deathDuration;

    [ShowInInspector]
    public CombatantState CurrentState { get; private set; }

    public CombatantStateMachine(CombatantBase owner)
    {
        this.owner = owner;
        animancerCont = owner?.GetComponent<CombatUnitController>()?.AnimancerCont;
    }

    public void ResetForCombat()
    {
        CancelHitReturn();
        deathStarted = false;
        deathStartedAt = 0f;
        deathDuration = 0f;
        CurrentState = CombatantState.Idle;
    }

    public void ChangeState(CombatantState state)
    {
        if (CurrentState == CombatantState.Dead)
            return;

        if (state != CombatantState.Idle)
            CancelHitReturn();

        CurrentState = state;

        var clipSet = GetClip(state);
        if (clipSet == null || animancerCont == null)
            return;

        animancerCont.PlayAnimation(clipSet, DefaultFadeDuration);
    }

    public void ChangeHitState(HitSeverity severity)
    {
        CurrentState = CombatantState.Hit;

        var clipSet = GetHitClip(severity);
        if (clipSet == null || animancerCont == null)
            return;

        CancelHitReturn();
        hitCts = new CancellationTokenSource();

        animancerCont.StopAnimation(clipSet);
        animancerCont.PlayAnimation(clipSet, DefaultFadeDuration);

        ReturnToIdleAfterHit(clipSet, hitCts.Token).Forget();
    }

    void CancelHitReturn()
    {
        if (hitCts == null)
            return;

        hitCts.Cancel();
        hitCts.Dispose();
        hitCts = null;
    }

    async UniTask ReturnToIdleAfterHit(AnimationClipSet clipSet, CancellationToken token)
    {
        float duration = GetClipDuration(clipSet);
        bool canceled = await UniTask
            .WaitForSeconds(duration, cancellationToken: token)
            .SuppressCancellationThrow();

        if (canceled || token.IsCancellationRequested)
            return;

        CancelHitReturn();

        if (!owner.IsAlive)
            return;

        ChangeState(CombatantState.Idle);
    }

    public void StartDeath()
    {
        if (deathStarted)
            return;

        deathStarted = true;
        deathStartedAt = Time.time;
        CancelHitReturn();

        var clipSet = GetClip(CombatantState.Dead);
        deathDuration = GetClipDuration(clipSet);
        CurrentState = CombatantState.Dead;

        if (clipSet == null || animancerCont == null)
        {
            Debug.LogWarning(
                $"{nameof(CombatantStateMachine)}: Death clip is missing on '{owner?.name}'. Assign `Die` in Animation Clips SO.");
            return;
        }

        animancerCont.StopAnimation(clipSet);
        animancerCont.PlayAnimation(clipSet, DefaultFadeDuration);
    }

    public async UniTask WaitForDeathAsync()
    {
        if (!deathStarted)
            StartDeath();

        if (deathDuration <= 0f)
            return;

        float elapsed = Time.time - deathStartedAt;
        float remaining = deathDuration - elapsed;
        if (remaining > 0f)
            await UniTask.WaitForSeconds(remaining);
    }

    public UniTask PlayDeathAsync() => WaitForDeathAsync();

    public void PlayWin()
    {
        ChangeState(CombatantState.Win);
    }

    static float GetClipDuration(AnimationClipSet clipSet)
    {
        var clip = clipSet?.AnimationClip;
        if (clip == null)
            return 0f;

        float speed = Mathf.Abs(clipSet.speed);
        if (speed < 0.0001f)
            speed = 1f;

        return clip.length / speed;
    }

    public static HitSeverity ResolveHitSeverity(float damage, float maxHp)
    {
        if (maxHp <= 0f)
            return HitSeverity.Light;

        float ratio = damage / maxHp;

        if (ratio < 0.15f)
            return HitSeverity.Light;
        if (ratio < 0.40f)
            return HitSeverity.Medium;

        return HitSeverity.Heavy;
    }

    AnimationClipSet GetClip(CombatantState state)
    {
        var clips = owner?.AnimationClips;
        if (clips == null)
            return null;

        return state switch
        {
            CombatantState.Idle => clips.CombatIdle,
            CombatantState.Ready => clips.Ready,
            CombatantState.Attack => null,
            CombatantState.Hit => clips.LightHit,
            CombatantState.Stun => clips.Stun,
            CombatantState.Dead => clips.Die,
            CombatantState.Win => clips.Win,
            _ => null
        };
    }

    AnimationClipSet GetHitClip(HitSeverity severity)
    {
        var clips = owner?.AnimationClips;
        if (clips == null)
            return null;
        
        switch (severity)
        {
            case HitSeverity.Light:
                return clips.LightHit;
            case HitSeverity.Medium:
                return clips.MediumHit;
            case HitSeverity.Heavy:
                return clips.HeavyHit;
        }

        return clips.LightHit;
    }
}

public enum HitSeverity
{
    Light,
    Medium,
    Heavy
}
