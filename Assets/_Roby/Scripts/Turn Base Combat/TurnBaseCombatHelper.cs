using System;
using System.Collections.Generic;
using ToGaProTest.Shared;
using UnityEngine;

public static class TurnBaseCombatHelper
{
    public static bool TryGetAveragePosition(IReadOnlyList<CombatantBase> combatants, out Vector3 average)
    {
        average = Vector3.zero;
        int count = 0;

        for (int i = 0; i < combatants.Count; i++)
        {
            var combatant = combatants[i];
            if (combatant == null)
                continue;

            average += combatant.transform.position;
            count++;
        }

        if (count == 0)
            return false;

        average /= count;
        return true;
    }

    public static bool TrySnapToGround(Vector3 probe,
                                       float upDistance,
                                       float downDistance,
                                       LayerMask layerMask,
                                       float yOffset,
                                       out Vector3 snapped)
    {
        snapped = probe;
        bool found = false;
        float bestVerticalDist = float.MaxValue;
        Vector3 bestPoint = probe;

        if (Physics.Raycast(probe, Vector3.up, out RaycastHit hitUp, upDistance, layerMask))
        {
            float dist = Mathf.Abs(hitUp.point.y - probe.y);
            if (dist < bestVerticalDist)
            {
                bestVerticalDist = dist;
                bestPoint = hitUp.point;
                found = true;
            }
        }

        if (Physics.Raycast(probe, Vector3.down, out RaycastHit hitDown, downDistance, layerMask))
        {
            float dist = Mathf.Abs(hitDown.point.y - probe.y);
            if (dist < bestVerticalDist)
            {
                bestVerticalDist = dist;
                bestPoint = hitDown.point;
                found = true;
            }
        }

        if (found)
            snapped = bestPoint + Vector3.up * yOffset;

        return found;
    }

    public static void TeleportTo(Transform target, Vector3 worldPosition)
    {
        if (target == null)
            return;

        if (target.TryGetComponent(out CharacterController characterController))
        {
            characterController.enabled = false;
            target.position = worldPosition;
            characterController.enabled = true;
            return;
        }

        target.position = worldPosition;
    }

    public static List<CombatantBase> FilterNullCombatants(IEnumerable<CombatantBase> combatants)
    {
        var result = new List<CombatantBase>();
        if (combatants == null)
            return result;

        foreach (var combatant in combatants)
        {
            if (combatant != null)
                result.Add(combatant);
        }

        return result;
    }

    public static AttackRequest BuildAttackRequest(CombatantBase attacker, 
                                                    CombatantBase defender, 
                                                    DamageProfileWithAttribute damageProfile)
    {
        var attackReq = new AttackRequest();
        attackReq.Attacker = attacker;
        attackReq.Defender = defender;

        var attack = attacker.StatContainer.GetTotalValue(StatAttribute.Attack);
        attackReq.RawDamage = (attack * damageProfile.multiplierDamage) + damageProfile.flatDamage;

        return attackReq;
    }

    public static void SendAttack(AttackRequest attackReq, out AttackResult attackRes)
    {
        attackRes = new AttackResult
        {
            Attacker = attackReq.Attacker,
            Defender = attackReq.Defender,
            IncomingDamage = attackReq.RawDamage,
        };

        attackReq.Defender.TakeDamage(ref attackRes);
    }
}

[Serializable]
public struct AttackRequest
{
    public CombatantBase Attacker;
    public CombatantBase Defender;

    public float RawDamage;
}

[Serializable]
public struct AttackResult
{
    public CombatantBase Attacker;
    public CombatantBase Defender;

    public float IncomingDamage;
    public float ReceivedDamage;

    public bool IsDefenderDead;
}