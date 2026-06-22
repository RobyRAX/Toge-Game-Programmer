using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using ToGaProTest.Shared;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StatContainer_Runtime
{
    static List<StatEntry> statEntries;

    [HideInInspector]
    public Action<StatAttribute> OnAttributeModifierChanged;

    [HideReferenceObjectPicker]
    [ListDrawerSettings(ListElementLabelName = "Label")]
    public List<Stat_Runtime> Stats;

    public StatContainer_Runtime() { }
    public StatContainer_Runtime(IStatEntryProvider statEntryProvider)
    {
        if (statEntries == null || statEntries != statEntryProvider.StatEntries)
            statEntries = statEntryProvider.StatEntries;

        Stats = new List<Stat_Runtime>();

        var entries = statEntries
                        .Where(entry => ToGaProTestShared.StatPercentAttributes.Contains(entry.attribute) == false)
                        .ToList();

        foreach (var statEntry in entries)
        {
            Stat_Runtime newStatRes = new Stat_Runtime(statEntry);
            Stats.Add(newStatRes);
        }
    }

    public Stat_Runtime GetStat(StatAttribute attribute)
    {
        if (Stats == null)
            return null;

        Stat_Runtime tempStat = Stats.Find(x => x.attribute == attribute);

        if (tempStat != null)
            return tempStat;
        else
            return null;
    }

    public float GetBaseValue(StatAttribute attribute)
    {
        if (Stats == null)
            return 0;

        Stat_Runtime tempStat = Stats.Find(x => x.attribute == attribute);

        if (tempStat != null)
            return tempStat.BaseValue;
        else
            return 0;
    }

    public float GetTotalValue(StatAttribute attribute)
    {
        if (Stats == null)
            return 0;

        Stat_Runtime tempStat = Stats.Find(x => x.attribute == attribute);

        if (tempStat != null)
            return tempStat.TotalValue;
        else
            return 0;
    }

    public void AddStatModifier(StatAttribute attribute, StatModifier modifier)
    {
        GetStat(attribute).AddModifier(modifier);
        OnAttributeModifierChanged?.Invoke(attribute);
    }

    public void RemoveStatModifier(StatAttribute attribute, StatModifier modifier)
    {
        GetStat(attribute).RemoveModifier(modifier);
        OnAttributeModifierChanged?.Invoke(attribute);
    }

    public void RemoveStatModifier(StatAttribute attribute, string modifierId)
    {
        GetStat(attribute).RemoveModifier(modifierId);
        OnAttributeModifierChanged?.Invoke(attribute);
    }

    public void UpdateStatModifier(StatAttribute attribute)
    {
        OnAttributeModifierChanged?.Invoke(attribute);
    }

    public bool HasStatModifier(StatAttribute attribute, string sourceId)
    {
        return GetStat(attribute).HasModifier(sourceId);
    }

    public StatModifier GetStatModifier(StatAttribute attribute, string sourceId)
    {
        return GetStat(attribute).GetModifier(sourceId);
    }

    public void SetAllToZero()
    {
        if (Stats == null)
            return;

        foreach (var stat in Stats)
        {
            stat.BaseValue = 0;
            stat.modifiers.Clear();
            OnAttributeModifierChanged?.Invoke(stat.attribute);
        }
    }
}