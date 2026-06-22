using System;
using ToGaProTest.Shared;

public static class TurnBaseCombatHelper
{
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