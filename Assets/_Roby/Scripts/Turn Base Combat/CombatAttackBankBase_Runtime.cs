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
