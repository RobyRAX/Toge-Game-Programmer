
using System;
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

    [TitleGroup("Formation")]
    [ShowInInspector]
    [ReadOnly]
    public Transform FormationSlot { get; private set; }

    [TitleGroup("Animation Clips")]
    [ShowInInspector]
    public ICombatAnimationClipsProvider AnimationClips;

    public bool HasFormationSlot => FormationSlot != null;

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
        if (TryGetComponent(out CharacterController controller))
            controller.enabled = isEnabled;
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
    }

    public virtual DamageProfileWithAttribute GetDamageProfile(CombatAttackBaseSO attackSO)
    {
        return attackSO.DamageProfile;
    }
}