using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

public abstract class DamageProfileProviderBase
{
#if UNITY_EDITOR
    public virtual void Set_EditorData(CombatAttackBaseSO attackSO)
    {
    }
#endif
}

[Serializable]
public class EnemyDamageProfileProvider : DamageProfileProviderBase
{
    [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
    public DamageProfile damageProfileEntries;
}

[Serializable]
public class HeroDamageProfileProvider : DamageProfileProviderBase
{
    [OnValueChanged("OnTalentChangedHandler")]
    public HeroTalentType talent;

    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true, ListElementLabelName = "Label")]
    [OnValueChanged("Refresh_EntryPreview", IncludeChildren = true)]
    public HeroAttackDamageProfileEntry damageProfileEntry;

#if UNITY_EDITOR
    [FoldoutGroup("Simulation")]
    [SerializeField]
    GameplayConfigSO configSO;

    [FoldoutGroup("Simulation")]
    [ReadOnly]
    [ShowInInspector]
    [HideReferenceObjectPicker]
    AttackAttribute attackAttribute; 

    [FoldoutGroup("Simulation")]
    [SerializeField]
    [PropertyRange(0, "@MaxTalentLevel")]
    [OnValueChanged("Refresh_EntryPreview")]
    int talentLevel;

    int MaxTalentLevel => configSO?.maxTalentLevel ?? 10;

    public void OnTalentChangedHandler()
    {
        if (damageProfileEntry == null)
            return;

        damageProfileEntry.attributeId = null;
        damageProfileEntry.damageProfile_Preview = new DamageProfile();
    }

    public void Refresh_EntryPreview()
    {
        if (damageProfileEntry == null)
            return;

        var dmgProfile = attackAttribute.GetDamageProfile(damageProfileEntry.attributeId, talentLevel);
        damageProfileEntry.Set_DamageProfilePreview(dmgProfile);
    }

    public override void Set_EditorData(CombatAttackBaseSO attackSO)
    {
        base.Set_EditorData(attackSO);

        if (attackSO is HeroAttackSO heroAttackSO)
        {
            var heroCombatData = heroAttackSO.heroCombatDataSO_editorData;
            
            switch (talent)
            {
                case HeroTalentType.NormalAttack:
                    attackAttribute = heroCombatData.NormalAttackTalent.AttackAttribute;
                    break;
                case HeroTalentType.Skill:
                    attackAttribute = heroCombatData.SkillTalent.AttackAttribute;
                    break;
                case HeroTalentType.Ultimate:
                    attackAttribute = heroCombatData.UltimateTalent.AttackAttribute;
                    break;
            }

            HeroAttackDamageProfileEntry.Set_EditorData(attackAttribute.AttributeIds);
        }
    }
#endif
}

[Serializable]
public class HeroAttackDamageProfileEntry
{
    [ValueDropdown("attributeIds")]
    public string attributeId;
    public StatAttribute attribute = StatAttribute.Attack;

#if UNITY_EDITOR
    public string Label => $"{attributeId} -> {damageProfile_Preview.flatDamage} + {damageProfile_Preview.multiplierDamage}% {attribute}";

    [TitleGroup("Preview")]
    [HideLabel]
    [ReadOnly]
    public DamageProfile damageProfile_Preview;

    public void Set_DamageProfilePreview(DamageProfile damageProfile)
    {
        damageProfile_Preview.flatDamage = damageProfile.flatDamage;
        damageProfile_Preview.multiplierDamage = damageProfile.multiplierDamage;
    }

    static List<string> attributeIds;

    public static void Set_EditorData(List<string> attIds)
    {
        attributeIds = attIds;
    }
#endif
}

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

