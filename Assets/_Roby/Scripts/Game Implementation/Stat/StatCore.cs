using System.Collections.Generic;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

public class Stat_Runtime
{
    string Label
    {
        get
        {
            var suffix = valueType == StatValueType.Percent ? "%" : "";
            return $"{attribute} -> {TotalValue}{suffix}";
        }
    }

    [TitleGroup("Typing")]
    public StatAttribute attribute;
    [TitleGroup("Typing")]
    public StatValueType valueType;
    [TitleGroup("Typing")]
    public bool round;

    float _baseValue;
    [ShowInInspector]
    [TitleGroup("Base")]
    [SuffixLabel("@_suffix")]
    public float BaseValue
    {
        get
        {
            if (round)
                return Mathf.Round(_baseValue);
            else
                return _baseValue;
        }
        set => _baseValue = value;
    }

    [TitleGroup("Modifiers")]
    [ShowInInspector]
    [PropertyOrder(1)]
    [HideReferenceObjectPicker]
    public List<StatModifier> modifiers;

    [TitleGroup("Modifiers")]
    [ShowInInspector]
    [PropertyOrder(1)]
    public float TotalFlatModsValue
    {
        get
        {
            float total = 0;

            if (modifiers != null && modifiers.Count > 0)
            {
                foreach (StatModifier modifier in modifiers)
                {
                    if (modifier.valueType == StatValueType.Flat)
                        total += modifier.Value;
                }
            }

            return total;
        }
    }

    [TitleGroup("Modifiers")]
    [ShowInInspector]
    [PropertyOrder(1)]
    public float BasePlusFlatMods
    {
        get
        {
            return BaseValue + TotalFlatModsValue;
        }
    }

    [TitleGroup("Modifiers")]
    [ShowInInspector]
    [PropertyOrder(1)]
    [SuffixLabel("%")]
    public float TotalPercentageModsValue
    {
        get
        {
            float total = 0;

            if (modifiers != null && modifiers.Count > 0)
            {
                foreach (StatModifier modifier in modifiers)
                {
                    if (modifier.valueType == StatValueType.Percent)
                        total += modifier.Value;
                }
            }

            return total;
        }
    }

    [TitleGroup("Modifiers")]
    [ShowInInspector]
    [PropertyOrder(1)]
    [SuffixLabel("@_suffix")]
    public float TotalModsValue
    {
        get
        {
            float total = 0;

            if (modifiers != null && modifiers.Count > 0)
            {
                foreach (StatModifier modifier in modifiers)
                {
                    if (modifier.valueType == StatValueType.Flat)
                    {
                        total += modifier.Value;
                    }
                    else if (modifier.valueType == StatValueType.Percent)
                    {
                        if (valueType == StatValueType.Flat)
                        {
                            float convertedPercentage = BaseValue / 100f * modifier.Value;
                            total += convertedPercentage;
                        }
                        else if (valueType == StatValueType.Percent)
                        {
                            total += modifier.Value;
                        }
                    }

                }
            }

            return total;
        }
    }

    [TitleGroup("Total")]
    [ShowInInspector]
    [PropertyOrder(1)]
    [SuffixLabel("@_suffix")]
    public float TotalValue
    {
        get
        {
            if (round)
                return Mathf.Round(_baseValue + TotalModsValue);
            else
                return BaseValue + TotalModsValue;
        }
    }

    char _suffix
    {
        get
        {
            return valueType == StatValueType.Percent ? '%' : ' ';
        }
    }

    public Stat_Runtime(Stat_Runtime statToClone)
    {
        this.attribute = statToClone.attribute;
        this.valueType = statToClone.valueType;
        this.round = statToClone.round;
        this._baseValue = statToClone._baseValue;
        this.modifiers = new List<StatModifier>();
    }

    public Stat_Runtime(StatEntry statEntry)
    {
        this.attribute = statEntry.attribute;
        this.valueType = ToGaProTestShared.GetStatValueType(statEntry.attribute);
        this.round = ToGaProTestShared.ShouldRoundStat_Static(statEntry.attribute);
        this._baseValue = statEntry.defaultValue;
        this.modifiers = new List<StatModifier>();
    }

    public Stat_Runtime() { }

    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        if (HasModifier(modifier.modifierId) == false)
            return;

        modifiers.Remove(modifier);
    }

    public void RemoveModifier(string modifierId)
    {
        if (HasModifier(modifierId) == false)
            return;

        modifiers.Remove(GetModifier(modifierId));
    }

    public bool HasModifier(string modifierId)
    {
        return modifiers.Contains(modifiers.Find((item) => item.modifierId == modifierId));
    }

    public StatModifier GetModifier(string modifierId)
    {
        return modifiers.Find((item) => item.modifierId == modifierId);
    }
}

public class StatModifier
{
    [ShowInInspector]
    [ReadOnly]
    public string modifierId;

    public StatValueType valueType;

    [SuffixLabel("@_suffix", overlay: true)]
    public float Value;

    char _suffix
    {
        get
        {
            return valueType == StatValueType.Percent ? '%' : ' ';
        }
    }

    public StatModifier(StatValueType valueType)
    {
        this.valueType = valueType;
    }

    public StatModifier(string modifierId, StatValueType valueType, float value)
    {
        this.valueType = valueType;
        this.modifierId = modifierId;
        Value = value;
    }

    public StatModifier() { }
}