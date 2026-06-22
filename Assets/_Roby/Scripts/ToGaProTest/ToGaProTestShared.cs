using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ToGaProTest.Shared
{
    public class ToGaProTestShared
    {
        public static StatValueType GetStatValueType(StatAttribute attribute)
        {
            return StatPercentAttributes.Contains(attribute)
                ? StatValueType.Percent
                : StatValueType.Flat;
        }

        public static bool ShouldRoundStat_Static(StatAttribute attribute)
        {
            return GetStatValueType(attribute) == StatValueType.Flat;
        }

        public static List<StatAttribute> MainStatAttributes => new List<StatAttribute>()
        {
            StatAttribute.MaxHp,
            StatAttribute.Attack,
            StatAttribute.MaxStamina,
            StatAttribute.Defense
        };

        public static List<StatAttribute> StatPercentAttributes => new List<StatAttribute>()
        {
            StatAttribute.MaxHpPercent,
            StatAttribute.AttackPercent,
            StatAttribute.MaxStaminaPercent,
            StatAttribute.DefensePercent,
        };

        public static Dictionary<StatAttribute, StatAttribute> MainToPercentStatMap => new Dictionary<StatAttribute, StatAttribute>()
        {
            { StatAttribute.MaxHp, StatAttribute.MaxHpPercent },
            { StatAttribute.Attack, StatAttribute.AttackPercent },
            { StatAttribute.MaxStamina, StatAttribute.MaxStaminaPercent },
            { StatAttribute.Defense, StatAttribute.DefensePercent }
        };
    }

    public enum HeroTalentType
    {
        NormalAttack = 0,
        Skill = 1,
        Ultimate = 2,
    }

    public enum BrainExplorationType
    {
        ActiveUnit, PartyAI, EnemyAI
    }

    public enum StatAttribute
    {
        None = -1,

        MaxHp = 0,
        Attack = 1,
        MaxStamina = 2,
        Defense = 3,

        MaxHpPercent = 6,
        AttackPercent = 7,
        MaxStaminaPercent = 8,
        DefensePercent = 9,

        AttackSpeed = 11,
    }

    public enum StatValueType
    {
        Flat,
        Percent
    }

    [Serializable]
    public class StatAttributeWithValue
    {
        public StatAttribute attribute = StatAttribute.None;

        [SuffixLabel("@SuffixLabel", true)]
        public float value = 1;

#if UNITY_EDITOR
        protected virtual string SuffixLabel => "";
#endif
    }
}
