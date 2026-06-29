using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ToGaProTest.Shared
{
    [Serializable]
    public class DamageProfile
    {
        public float flatDamage;
        [SuffixLabel("%")]
        public float multiplierDamage;
    }

    [Serializable]
    public class DamageProfileWithAttribute
    {
        public float flatDamage;
        public float multiplierDamage;
        public StatAttribute attribute;
    }

    [Serializable]
    public class StatGrowth
    {
        [TableList(ShowIndexLabels = true)]
        public List<MainStatValueSet> values;

        public float GetValueAtLevel(StatAttribute attr, int level)
        {
            if (values == null || values.Count == 0)
                return -1;

            int index = level - 1;
            if (index < 0 || index >= values.Count)
                return -1;

            var valueSet = values[index];

            if (attr == StatAttribute.MaxHp)
                return valueSet.maxHp;
            else if (attr == StatAttribute.Attack)
                return valueSet.attack;
            else if (attr == StatAttribute.MaxStamina)
                return valueSet.maxStamina;
            else if (attr == StatAttribute.Defense)
                return valueSet.defense;
            else if (attr == StatAttribute.StaminaRegen)
                return valueSet.staminaRegen;
            else if (attr == StatAttribute.AttackSpeed)
                return valueSet.attackSpeed;
            else
                return -1;
        }

        public void ApplyMainStatsTo(StatContainer_Runtime container, int level)
        {
            if (container == null)
                return;

            container.GetStat(StatAttribute.MaxHp).BaseValue = GetValueAtLevel(StatAttribute.MaxHp, level);
            container.GetStat(StatAttribute.Attack).BaseValue = GetValueAtLevel(StatAttribute.Attack, level);
            container.GetStat(StatAttribute.MaxStamina).BaseValue = GetValueAtLevel(StatAttribute.MaxStamina, level);
            container.GetStat(StatAttribute.Defense).BaseValue = GetValueAtLevel(StatAttribute.Defense, level);
            container.GetStat(StatAttribute.StaminaRegen).BaseValue = GetValueAtLevel(StatAttribute.StaminaRegen, level);
            container.GetStat(StatAttribute.AttackSpeed).BaseValue = GetValueAtLevel(StatAttribute.AttackSpeed, level);
        }

        [FoldoutGroup("Setup Helper")]
        [SerializeField]
        int maxLevel;

        [FoldoutGroup("Setup Helper")]
        [SerializeField]
        MainStatValueSet initial;

        [FoldoutGroup("Setup Helper")]
        [SerializeField]
        MainStatValueSet max;

        [FoldoutGroup("Setup Helper")]
        [Button]
        public void Setup()
        {
            if (maxLevel <= 0)
                return;

            values = new List<MainStatValueSet>(maxLevel);

            if (maxLevel == 1)
            {
                values.Add(CopyValueSet(initial));
                return;
            }

            for (int level = 1; level <= maxLevel; level++)
            {
                float t = (level - 1) / (float)(maxLevel - 1);
                values.Add(new MainStatValueSet
                {
                    maxHp = Mathf.Round(Mathf.Lerp(initial.maxHp, max.maxHp, t)),
                    attack = Mathf.Round(Mathf.Lerp(initial.attack, max.attack, t)),
                    maxStamina = Mathf.Round(Mathf.Lerp(initial.maxStamina, max.maxStamina, t)),
                    defense = Mathf.Round(Mathf.Lerp(initial.defense, max.defense, t)),
                    staminaRegen = Mathf.Lerp(initial.staminaRegen, max.staminaRegen, t),
                    attackSpeed = Mathf.Lerp(initial.attackSpeed, max.attackSpeed, t),
                });
            }
        }

        static MainStatValueSet CopyValueSet(MainStatValueSet source)
        {
            return new MainStatValueSet
            {
                maxHp = source.maxHp,
                attack = source.attack,
                defense = source.defense,
                maxStamina = source.maxStamina,
                staminaRegen = source.staminaRegen,
                attackSpeed = source.attackSpeed,
            };
        }
    }

    [Serializable]
    public class MainStatValueSet
    {
        public float maxHp;
        public float attack;
        public float defense;
        public float maxStamina;
        public float staminaRegen;
        public float attackSpeed;
    }

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
            if (attribute == StatAttribute.AttackSpeed || attribute == StatAttribute.StaminaRegen)
                return false;

            return GetStatValueType(attribute) == StatValueType.Flat;
        }

        public static List<StatAttribute> MainStatAttributes => new List<StatAttribute>()
        {
            StatAttribute.MaxHp,
            StatAttribute.Attack,
            StatAttribute.Defense,
            StatAttribute.MaxStamina,
            StatAttribute.StaminaRegen,
            StatAttribute.AttackSpeed,
        };

        public static List<StatAttribute> StatPercentAttributes => new List<StatAttribute>()
        {
            StatAttribute.MaxHpPercent,
            StatAttribute.AttackPercent,
            StatAttribute.MaxStaminaPercent,
            StatAttribute.DefensePercent,
            StatAttribute.StaminaRegenPercent,
        };

        public static Dictionary<StatAttribute, StatAttribute> MainToPercentStatMap => new Dictionary<StatAttribute, StatAttribute>()
        {
            { StatAttribute.MaxHp, StatAttribute.MaxHpPercent },
            { StatAttribute.Attack, StatAttribute.AttackPercent },
            { StatAttribute.MaxStamina, StatAttribute.MaxStaminaPercent },
            { StatAttribute.Defense, StatAttribute.DefensePercent },
            { StatAttribute.StaminaRegen, StatAttribute.StaminaRegenPercent }
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
        Defense = 3,
        MaxStamina = 2,
        StaminaRegen = 4,

        MaxHpPercent = 6,
        AttackPercent = 7,
        DefensePercent = 9,
        MaxStaminaPercent = 8,
        StaminaRegenPercent = 10,

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
