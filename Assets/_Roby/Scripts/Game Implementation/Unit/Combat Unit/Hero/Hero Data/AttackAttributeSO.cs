using RAXY.Utility.Localization;
using RAXY.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using ToGaProTest.Shared;

[CreateAssetMenu(fileName = "Attack Attribute", menuName = "RAXY/Unit/Hero/Attack Attribute")]
public class AttackAttributeSO : ScriptableObject
{
    [HideLabel]
    public AttackAttribute attackAttribute;
}

[Serializable]
public class AttackAttribute
{
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "attributeId", Expanded = true)]
    [HideReferenceObjectPicker]
    public List<AttributeEntry> attributes;

    public AttributeEntry GetEntry(string attId)
    {
        return attributes?.Find(x => x.attributeId == attId);
    }

    public DamageProfile GetDamageProfile(string attId, int level = 1)
    {
        var entry = GetEntry(attId);
        if (entry == null)
            return default;

        var profiles = entry.damageProfiles;
        if (profiles == null || profiles.Count == 0)
            return default;

        int index = level - 1;
        if (index < 0 || index >= profiles.Count)
            return default;

        return profiles[index];
    }

#if UNITY_EDITOR
    public List<string> AttributeIds
    {
        get
        {
            if (attributes == null)
                return new List<string>();

            return attributes.Select(x => x.attributeId).ToList();
        }
    }
#endif
}

[Serializable]
public class AttributeEntry
{
    public string attributeId;
    public string attributeName;

    [TitleGroup("Value Per Level")]
    [TableList(ShowIndexLabels = true)]
    public List<DamageProfile> damageProfiles;

    [FoldoutGroup("Setup Helper")]
    [SerializeField]
    int maxLevel;

    [FoldoutGroup("Setup Helper")]
    [SerializeField]
    DamageProfile initial;

    [FoldoutGroup("Setup Helper")]
    [SerializeField]
    DamageProfile max;

    [FoldoutGroup("Setup Helper")]
    [Button]
    public void Setup()
    {
        if (maxLevel <= 0)
            return;

        damageProfiles = new List<DamageProfile>(maxLevel);

        for (int level = 1; level <= maxLevel; level++)
        {
            float t = (level - 1) / (float)(maxLevel - 1);
            damageProfiles.Add(new DamageProfile
            {
                flatDamage = Mathf.Round(Mathf.Lerp(initial.flatDamage, max.flatDamage, t)),
                multiplierDamage = Mathf.Round(Mathf.Lerp(initial.multiplierDamage, max.multiplierDamage, t))
            });
        }
    }
}