using System;
using System.Collections.Generic;
using RAXY.Core.Addressable;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroDataSO", menuName = "RAXY/Unit/Hero/Hero Data")]
public class HeroDataSO : ScriptableObject, IItemEntry
{
    [SerializeField] 
    string heroId;

    [SerializeField]  
    string heroName;

    [SerializeField]  
    Sprite heroIcon;

    public GameObject heroPrefab;

    public string ItemId => heroId;
    public bool IsStackable => false;
    public string ItemName => heroName;
    public string ItemDescription => "";
    public string ItemAdditionalDescription => "";
    public Sprite ItemIcon => heroIcon;

    [TitleGroup("Combat Data")]
    [HideLabel]
    public HeroCombatDataSO CombatDataSO;

    [TitleGroup("Stat Growth")]
    [HideLabel]
    public HeroStatGrowth StatGrowth;
}

[Serializable]
public class HeroStatGrowth
{
    [TableList(ShowIndexLabels = true)]
    public List<MainAttackValueSet> values;

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
        else
            return -1;
    }

    [FoldoutGroup("Setup Helper")]
    [SerializeField]
    int maxLevel;

    [FoldoutGroup("Setup Helper")]
    [SerializeField]
    MainAttackValueSet initial;

    [FoldoutGroup("Setup Helper")]
    [SerializeField]
    MainAttackValueSet max;

    [FoldoutGroup("Setup Helper")]
    [Button]
    public void Setup()
    {
        if (maxLevel <= 0)
            return;

        values = new List<MainAttackValueSet>(maxLevel);

        if (maxLevel == 1)
        {
            values.Add(CopyValueSet(initial));
            return;
        }

        for (int level = 1; level <= maxLevel; level++)
        {
            float t = (level - 1) / (float)(maxLevel - 1);
            values.Add(new MainAttackValueSet
            {
                maxHp = Mathf.Round(Mathf.Lerp(initial.maxHp, max.maxHp, t)),
                attack = Mathf.Round(Mathf.Lerp(initial.attack, max.attack, t)),
                maxStamina = Mathf.Round(Mathf.Lerp(initial.maxStamina, max.maxStamina, t)),
                defense = Mathf.Round(Mathf.Lerp(initial.defense, max.defense, t)),
            });
        }
    }

    static MainAttackValueSet CopyValueSet(MainAttackValueSet source)
    {
        return new MainAttackValueSet
        {
            maxHp = source.maxHp,
            attack = source.attack,
            maxStamina = source.maxStamina,
            defense = source.defense,
        };
    }
}

[Serializable]
public class MainAttackValueSet
{
    public float maxHp;
    public float attack;
    public float maxStamina;
    public float defense;
}