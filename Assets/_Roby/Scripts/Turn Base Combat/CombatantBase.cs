
using System;
using Cysharp.Threading.Tasks;
using RAXY.Movement;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

public abstract class CombatantBase : MonoBehaviour
{
    public virtual int Level { get; set; }

    [TitleGroup("Current Value")]
    [ShowInInspector]
    public virtual float CurrentHp { get; set; }

    [TitleGroup("Current Value")]
    [ShowInInspector]
    [ReadOnly]
    public float DisplayedHp { get; private set; }

    [TitleGroup("Current Value")]
    [ShowInInspector]
    public virtual float CurrentStamina { get; set; }

    [TitleGroup("Current Value")]
    [ShowInInspector]
    public virtual bool IsAlive { get; set; }

    [TitleGroup("Stat")]
    [ShowInInspector]
    [HideReferenceObjectPicker]
    [HideLabel]
    public virtual StatContainer_Runtime StatContainer { get; set; }

    [TitleGroup("Attack Bank")]
    [ShowInInspector]
    [HideReferenceObjectPicker]
    [HideLabel]
    public virtual CombatAttackBankBase_Runtime AttackBank { get; set; }

    public virtual CombatDataBaseSO CombatDataSO { get; set; }

    [TitleGroup("Combat")]
    [ShowInInspector]
    [ReadOnly]
    public Transform FormationSlot { get; private set; }

    [TitleGroup("Animation Clips")]
    [ShowInInspector]
    public ICombatAnimationClipsProvider AnimationClips;

    [TitleGroup("Combat")]
    [ShowInInspector]
    public CombatantState CurrentState => StateMachine != null ? StateMachine.CurrentState : CombatantState.Idle;
    public CombatantStateMachine StateMachine { get; private set; }

    public CombatantInfo CombatantInfo { get; set; }
    public bool HasFormationSlot => FormationSlot != null;

    public event Action<CombatantBase> OnStatsChanged;

    protected void NotifyStatsChanged() => OnStatsChanged?.Invoke(this);

    public void InitDisplayedHp()
    {
        DisplayedHp = CurrentHp;
        NotifyStatsChanged();
    }

    public void ApplyVisualHit(float hitDamage)
    {
        DisplayedHp = Mathf.Max(0f, DisplayedHp - hitDamage);
        NotifyStatsChanged();
    }

    public void SyncDisplayedHp()
    {
        DisplayedHp = CurrentHp;
        NotifyStatsChanged();
    }

    protected void InitStateMachine()
    {
        StateMachine = new CombatantStateMachine(this);
        StateMachine.ChangeState(CombatantState.Idle);
    }

    protected virtual void Update()
    {
        AttackBank?.Tick(Time.deltaTime);
    }

    public void SetFormationSlot(Transform slot)
    {
        FormationSlot = slot;
    }

    public void SetExplorationMovementEnabled(bool isEnabled)
    {
        if (TryGetComponent(out UnitMovement movement))
            movement.enabled = isEnabled;
        if (TryGetComponent(out GroundChecker groundChecker))
            groundChecker.enabled = isEnabled;

        // CharacterController sengaja dibiarkan enabled agar tetap jadi collider
        // yang bisa di-raycast untuk hover/klik target saat combat.
        if (TryGetComponent(out CharacterController controller))
            controller.enabled = true;
    }

    public virtual void TakeDamage(ref AttackResult attackRes)
    {
        if (!IsAlive)
            return;

        float incoming = Mathf.Max(0, attackRes.IncomingDamage);

        // Defense
        float defense = StatContainer.GetTotalValue(StatAttribute.Defense);
        const float DEFENSE_K = 100f; // tuning knob

        float reduction = defense / (defense + DEFENSE_K);
        float afterDefense = incoming * (1f - reduction);

        CurrentHp -= afterDefense;

        attackRes.ReceivedDamage = afterDefense;
        attackRes.IsDefenderDead = CurrentHp <= 0;

        if (attackRes.IsDefenderDead)
            IsAlive = false;

        NotifyStatsChanged();
    }

    public virtual DamageProfileWithAttribute GetDamageProfile(CombatAttackBaseSO attackSO)
    {
        return attackSO.DamageProfile;
    }

    public event Action<CombatHitInfo> OnDoHit;

    public void RaiseDoHit(CombatHitInfo hitInfo) => OnDoHit?.Invoke(hitInfo);

    public async UniTask ExecuteAttack(Attack_Runtime attack,
                                        CombatantBase targetOpponent,
                                        CombatantBase targetTeam,
                                        AttackResult attackResult = default)
    {
        if (AttackBank.Attacks.Contains(attack) == false)
            return;

        await attack.ExecuteAttackActionSequenceAsync(targetOpponent, targetTeam, attackResult);
    }
}

public enum CombatantState
{
    Idle,
    Ready,
    Attack,
    Hit,
    Stun,
    Dead,
    Win
}

public class CombatantInfo
{
    public string unitName;
    public Sprite unitIcon;
}