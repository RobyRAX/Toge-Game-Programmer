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

    public static float GetCenterOutOffset(int index, float spacing)
    {
        if (index <= 0)
            return 0f;

        int ring = (index + 1) / 2;
        float side = index % 2 == 1 ? 1f : -1f;
        return side * ring * spacing;
    }

    public static void TeleportTo(Transform target, Vector3 worldPosition, Quaternion? worldRotation = null)
    {
        if (target == null)
            return;

        if (target.TryGetComponent(out CharacterController characterController))
        {
            bool wasEnabled = characterController.enabled;
            characterController.enabled = false;
            target.position = worldPosition;
            if (worldRotation.HasValue)
                target.rotation = worldRotation.Value;
            characterController.enabled = wasEnabled;
            return;
        }

        target.position = worldPosition;
        if (worldRotation.HasValue)
            target.rotation = worldRotation.Value;
    }

    public static bool TryGetFlatFacingRotation(Vector3 from, Vector3 to, out Quaternion rotation)
    {
        Vector3 flatDir = to - from;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude < 0.0001f)
        {
            rotation = Quaternion.identity;
            return false;
        }

        rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
        return true;
    }

    public static bool TryGetCombatFacingRotations(Vector3 heroAvg,
                                                   Vector3 enemyAvg,
                                                   out Quaternion heroFacing,
                                                   out Quaternion enemyFacing)
    {
        Vector3 flatDir = enemyAvg - heroAvg;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude < 0.0001f)
        {
            heroFacing = Quaternion.identity;
            enemyFacing = Quaternion.identity;
            return false;
        }

        flatDir.Normalize();
        heroFacing = Quaternion.LookRotation(flatDir, Vector3.up);
        enemyFacing = Quaternion.LookRotation(-flatDir, Vector3.up);
        return true;
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
        attackReq.RawDamage = (attack * damageProfile.multiplierDamage / 100) + damageProfile.flatDamage;

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

    #region Timeline

    public const float TimelineBaseStepInterval = 12f;
    public const float TimelineMinAttackSpeed = 0.01f;

    public static float GetCombatantAttackSpeed(CombatantBase combatant)
    {
        if (combatant?.StatContainer == null)
            return TimelineMinAttackSpeed;

        float rawSpeed = combatant.StatContainer.GetTotalValue(StatAttribute.AttackSpeed);
        return Mathf.Max(rawSpeed, TimelineMinAttackSpeed);
    }

    public static int CalculateStepInterval(float attackSpeed, float baseStepInterval = TimelineBaseStepInterval)
    {
        float clampedSpeed = Mathf.Max(attackSpeed, TimelineMinAttackSpeed);
        return Mathf.Max(1, Mathf.RoundToInt(baseStepInterval / clampedSpeed));
    }

    public static int CalculateNextTurnStep(int currentStep, int stepInterval)
    {
        return currentStep + Mathf.Max(1, stepInterval);
    }

    public static int CompareTimelineTurnPriority(float rawSpeedA,
                                                  int registrationOrderA,
                                                  float rawSpeedB,
                                                  int registrationOrderB)
    {
        int speedCompare = rawSpeedB.CompareTo(rawSpeedA);
        if (speedCompare != 0)
            return speedCompare;

        return registrationOrderA.CompareTo(registrationOrderB);
    }

    #endregion
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