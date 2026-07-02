using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroAttackSO", menuName = "RAXY/Unit/Hero/Attack SO")]
public class HeroAttackSO : CombatAttackBaseSO
{
    [TitleGroup("Setting")]
    public float ultimateRegen;

    [TitleGroup("Damage Profile")]
    [HideLabel]
    public HeroDamageProfileProvider damageProfileProvider;

    public override DamageProfileWithAttribute DamageProfile => null;

#if UNITY_EDITOR
    [TitleGroup("Editor")]
    [OnValueChanged(nameof(RefreshDamageProfileEditor))]
    public HeroCombatDataSO heroCombatDataSO_editorData;

    [OnInspectorInit]
    void OnInspectorInit_DamageProfile()
    {
        RefreshDamageProfileEditor();
    }

    void RefreshDamageProfileEditor()
    {
        damageProfileProvider?.Set_EditorData(this);
        damageProfileProvider?.Refresh_EntryPreview();
    }
#endif
}

[Serializable]
public class HeroDamageProfileProvider
{
    [OnValueChanged("OnTalentChangedHandler")]
    public HeroTalentType talent;

    [InlineProperty]
    [OnValueChanged("Refresh_EntryPreview", IncludeChildren = true)]
    [HideLabel]
    public HeroAttackDamageProfileEntry damageProfileEntry = new();

    public DamageProfileWithAttribute ResolveDamageProfile(HeroCombatDataSO combatData, int talentLevel)
    {
        if (combatData == null || damageProfileEntry == null || string.IsNullOrEmpty(damageProfileEntry.attributeId))
            return null;

        var attackAttribute = GetAttackAttribute(combatData);
        if (attackAttribute == null)
            return null;

        var profile = attackAttribute.GetDamageProfile(damageProfileEntry.attributeId, talentLevel);
        if (profile == null)
            return null;

        var entry = attackAttribute.GetEntry(damageProfileEntry.attributeId);
        var multiplierAttribute = entry?.multiplierAttribute ?? StatAttribute.Attack;

        return new DamageProfileWithAttribute
        {
            flatDamage = profile.flatDamage,
            multiplierDamage = profile.multiplierDamage,
            attribute = multiplierAttribute
        };
    }

    public AttackAttribute GetAttackAttribute(HeroCombatDataSO combatData)
    {
        if (combatData == null)
            return null;

        return talent switch
        {
            HeroTalentType.NormalAttack => combatData.NormalAttackTalent?.AttackAttribute,
            HeroTalentType.Skill => combatData.SkillTalent?.AttackAttribute,
            HeroTalentType.Ultimate => combatData.UltimateTalent?.AttackAttribute,
            _ => null
        };
    }

#if UNITY_EDITOR
    HeroAttackSO _editorHeroAttackSO;

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
    [PropertyRange(1, "@MaxTalentLevel")]
    [OnValueChanged("Refresh_EntryPreview")]
    int talentLevel = 1;

    int MaxTalentLevel => configSO?.maxTalentLevel ?? 10;

    public void OnTalentChangedHandler()
    {
        Refresh_AttackAttribute();

        if (damageProfileEntry == null)
            return;

        damageProfileEntry.attributeId = null;
        damageProfileEntry.damageProfile_Preview = new DamageProfile();
        Refresh_EntryPreview();
    }

    public void Refresh_EntryPreview()
    {
        if (damageProfileEntry == null || attackAttribute == null)
            return;

        var dmgProfile = attackAttribute.GetDamageProfile(damageProfileEntry.attributeId, talentLevel);
        var entry = attackAttribute.GetEntry(damageProfileEntry.attributeId);
        var multiplierAttribute = entry?.multiplierAttribute ?? StatAttribute.Attack;
        damageProfileEntry.Set_DamageProfilePreview(dmgProfile, multiplierAttribute);
    }

    public void Set_EditorData(CombatAttackBaseSO attackSO)
    {
        _editorHeroAttackSO = attackSO as HeroAttackSO;
        Refresh_AttackAttribute();
    }

    void Refresh_AttackAttribute()
    {
        var heroCombatData = _editorHeroAttackSO?.heroCombatDataSO_editorData;
        attackAttribute = GetAttackAttribute(heroCombatData);
        HeroAttackDamageProfileEntry.Set_EditorData(attackAttribute?.AttributeIds);
    }
#endif
}

[Serializable]
public class HeroAttackDamageProfileEntry
{
    [ValueDropdown("attributeIds")]
    public string attributeId;

#if UNITY_EDITOR
    public string Label => damageProfile_Preview == null
        ? attributeId
        : $"{attributeId} -> {damageProfile_Preview.flatDamage} + {damageProfile_Preview.multiplierDamage}% {multiplierAttribute_Preview}";

    [TitleGroup("Preview")]
    [HideLabel]
    [ReadOnly]
    public DamageProfile damageProfile_Preview;

    [TitleGroup("Preview")]
    [ReadOnly]
    public StatAttribute multiplierAttribute_Preview = StatAttribute.Attack;

    public void Set_DamageProfilePreview(DamageProfile damageProfile, StatAttribute multiplierAttribute)
    {
        if (damageProfile == null)
            return;

        damageProfile_Preview ??= new DamageProfile();
        damageProfile_Preview.flatDamage = damageProfile.flatDamage;
        damageProfile_Preview.multiplierDamage = damageProfile.multiplierDamage;
        multiplierAttribute_Preview = multiplierAttribute;
    }

    static List<string> attributeIds;

    public static void Set_EditorData(List<string> attIds)
    {
        attributeIds = attIds;
    }
#endif
}
