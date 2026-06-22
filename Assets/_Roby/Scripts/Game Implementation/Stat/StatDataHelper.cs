using System.Collections.Generic;
using ToGaProTest.Shared;
using UnityEngine;

public static class StatDataHelper
{
    public const string STAT_MULTIPLIER_MODIFIER_ID = "stat-multiplier";
    const string MAPPED_PERCENT_STAT_MODIFIER_ID_PREFIX = "mapped-percent-stat:";

    public static void MergeStatData(StatContainer_Runtime target, params IStatDataModifierProvider[] modifiers)
    {
        if (target == null || modifiers == null || modifiers.Length == 0)
            return;

        if (target.Stats == null)
            target.Stats = new List<Stat_Runtime>();

        foreach (var modifier in modifiers)
        {
            if (modifier == null || modifier.StatAttributeModifiers == null)
                continue;

            foreach (var statModifier in modifier.StatAttributeModifiers)
            {
                if (statModifier == null)
                    continue;

                var existingStat = target.Stats.Find(s => s != null && s.attribute == statModifier.attribute);
                if (existingStat != null)
                {
                    existingStat.BaseValue += statModifier.value;
                }
                else
                {
                    var newStat = new Stat_Runtime()
                    {
                        attribute = statModifier.attribute,
                        valueType = ToGaProTestShared.GetStatValueType(statModifier.attribute),
                        round = ToGaProTestShared.ShouldRoundStat_Static(statModifier.attribute),
                        BaseValue = statModifier.value
                    };

                    target.Stats.Add(newStat);
                }
            }
        }
    }
    
    public static void MergeStatData(StatContainer_Runtime target, params StatContainer_Runtime[] sources)
    {
        if (target == null || sources == null || sources.Length == 0)
            return;

        if (target.Stats == null)
            target.Stats = new List<Stat_Runtime>();

        foreach (var source in sources)
        {
            if (source == null || source.Stats == null)
                continue;

            foreach (var stat in source.Stats)
            {
                if (stat == null)
                    continue;

                var existingStat = target.Stats.Find(s => s != null && s.attribute == stat.attribute);
                if (existingStat != null)
                {
                    existingStat.BaseValue += stat.BaseValue;
                }
                else
                {
                    var newStat = new Stat_Runtime(stat)
                    {
                        BaseValue = stat.BaseValue
                    };

                    target.Stats.Add(newStat);
                }
            }
        }
    }

    public static StatContainer_Runtime BuildFinalStatData(StatContainer_Runtime baseStatData,
                                                        params IStatDataModifierProvider[] modifiers)
    {
        var finalStatData = new StatContainer_Runtime();
        finalStatData.SetAllToZero();

        MergeStatData(finalStatData, baseStatData);
        MergeStatData(finalStatData, modifiers);
        ApplyMappedPercentStatsAsModifiers(finalStatData);

        return finalStatData;
    }

    public static void ApplyMappedPercentStatsAsModifiers(StatContainer_Runtime target)
    {
        if (target == null || target.Stats == null || target.Stats.Count == 0)
            return;

        foreach (var pair in ToGaProTestShared.MainToPercentStatMap)
        {
            var flatStat = target.Stats.Find(x => x != null && x.attribute == pair.Key);
            var percentStat = target.Stats.Find(x => x != null && x.attribute == pair.Value);

            if (percentStat == null)
                continue;

            if (flatStat != null)
            {
                EnsureModifierList(flatStat);

                string modifierId = GetMappedPercentStatModifierId(pair.Value);
                flatStat.RemoveModifier(modifierId);
                flatStat.AddModifier(new StatModifier(modifierId,
                                                       StatValueType.Percent,
                                                       percentStat.TotalValue));
            }

            target.Stats.RemoveAll(x => x != null && x.attribute == pair.Value);
        }
    }

    public static void MultiplyStatData(StatContainer_Runtime target, IStatDataModifierProvider multiplierProvider)
    {
        if (target == null || multiplierProvider == null)
            return;

        MultiplyStatData(target, multiplierProvider.StatAttributeModifiers);
    }

    public static void MultiplyStatData(StatContainer_Runtime target, List<StatAttributeWithValue> multipliers)
    {
        if (target == null || target.Stats == null || multipliers == null || multipliers.Count == 0)
            return;

        var mappedPercentAttributes = new HashSet<StatAttribute>();

        foreach (var multiplierData in multipliers)
        {
            if (multiplierData == null)
                continue;

            StatAttribute targetAttribute = ResolveMappedTargetAttribute(multiplierData.attribute);
            if (targetAttribute != multiplierData.attribute)
                mappedPercentAttributes.Add(multiplierData.attribute);

            var targetStat = target.Stats.Find(x => x != null && x.attribute == targetAttribute);
            if (targetStat == null)
                continue;

            targetStat.RemoveModifier(STAT_MULTIPLIER_MODIFIER_ID);

            var modifier = new StatModifier(STAT_MULTIPLIER_MODIFIER_ID,
                                            StatValueType.Percent,
                                            Mathf.Max(0f, multiplierData.value));
            targetStat.AddModifier(modifier);
        }

        foreach (var mappedPercentAttr in mappedPercentAttributes)
        {
            target.Stats.RemoveAll(x => x != null && x.attribute == mappedPercentAttr);
        }
    }

    static StatAttribute ResolveMappedTargetAttribute(StatAttribute sourceAttribute)
    {
        foreach (var pair in ToGaProTestShared.MainToPercentStatMap)
        {
            if (pair.Value == sourceAttribute)
                return pair.Key;
        }

        return sourceAttribute;
    }

    static void MergeFinalStatModifierGroup(StatContainer_Runtime target,
                                            string mappedPercentModifierId,
                                            params IStatDataModifierProvider[] providers)
    {
        if (target == null || providers == null || providers.Length == 0)
            return;

        if (target.Stats == null)
            target.Stats = new List<Stat_Runtime>();

        foreach (var provider in providers)
        {
            if (provider == null || provider.StatAttributeModifiers == null)
                continue;

            foreach (var statModifier in provider.StatAttributeModifiers)
            {
                if (statModifier == null)
                    continue;

                if (TryGetMappedFlatAttribute(statModifier.attribute, out var flatAttribute))
                {
                    AddMappedPercentModifier(target,
                                             flatAttribute,
                                             mappedPercentModifierId,
                                             statModifier.value);
                    continue;
                }

                MergeStatValue(target, statModifier.attribute, statModifier.value);
            }
        }
    }

    static void MergeStatValue(StatContainer_Runtime target, StatAttribute attribute, float value)
    {
        var existingStat = target.Stats.Find(s => s != null && s.attribute == attribute);
        if (existingStat != null)
        {
            existingStat.BaseValue += value;
            return;
        }

        target.Stats.Add(new Stat_Runtime
        {
            attribute = attribute,
            valueType = ToGaProTestShared.GetStatValueType(attribute),
            round = ToGaProTestShared.ShouldRoundStat_Static(attribute),
            BaseValue = value,
            modifiers = new List<StatModifier>()
        });
    }

    static void AddMappedPercentModifier(StatContainer_Runtime target,
                                         StatAttribute flatAttribute,
                                         string modifierId,
                                         float value)
    {
        var flatStat = target.Stats.Find(x => x != null && x.attribute == flatAttribute);
        if (flatStat == null)
        {
            flatStat = new Stat_Runtime
            {
                attribute = flatAttribute,
                valueType = ToGaProTestShared.GetStatValueType(flatAttribute),
                round = ToGaProTestShared.ShouldRoundStat_Static(flatAttribute),
                BaseValue = 0f,
                modifiers = new List<StatModifier>()
            };
            target.Stats.Add(flatStat);
        }

        EnsureModifierList(flatStat);

        var existingModifier = flatStat.GetModifier(modifierId);
        if (existingModifier != null)
        {
            existingModifier.Value += value;
            return;
        }

        flatStat.AddModifier(new StatModifier(modifierId,
                                              StatValueType.Percent,
                                              value));
    }

    static bool TryGetMappedFlatAttribute(StatAttribute sourceAttribute, out StatAttribute flatAttribute)
    {
        foreach (var pair in ToGaProTestShared.MainToPercentStatMap)
        {
            if (pair.Value == sourceAttribute)
            {
                flatAttribute = pair.Key;
                return true;
            }
        }

        flatAttribute = StatAttribute.None;
        return false;
    }

    static void EnsureModifierList(Stat_Runtime stat)
    {
        if (stat.modifiers == null)
            stat.modifiers = new List<StatModifier>();
    }

    static string GetMappedPercentStatModifierId(StatAttribute percentAttribute)
    {
        return $"{MAPPED_PERCENT_STAT_MODIFIER_ID_PREFIX}{percentAttribute}";
    }
}
