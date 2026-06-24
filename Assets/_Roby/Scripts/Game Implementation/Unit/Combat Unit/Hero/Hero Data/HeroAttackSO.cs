using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroAttackSO", menuName = "RAXY/Unit/Hero/Attack SO")]
public class HeroAttackSO : CombatAttackBaseSO
{
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
        damageProfileEntry.Set_DamageProfilePreview(dmgProfile);
    }

    public void Set_EditorData(CombatAttackBaseSO attackSO)
    {
        _editorHeroAttackSO = attackSO as HeroAttackSO;
        Refresh_AttackAttribute();
    }

    void Refresh_AttackAttribute()
    {
        attackAttribute = null;

        var heroCombatData = _editorHeroAttackSO?.heroCombatDataSO_editorData;
        if (heroCombatData == null)
        {
            HeroAttackDamageProfileEntry.Set_EditorData(null);
            return;
        }

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

        HeroAttackDamageProfileEntry.Set_EditorData(attackAttribute?.AttributeIds);
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
    public string Label => damageProfile_Preview == null
        ? attributeId
        : $"{attributeId} -> {damageProfile_Preview.flatDamage} + {damageProfile_Preview.multiplierDamage}% {attribute}";

    [TitleGroup("Preview")]
    [HideLabel]
    [ReadOnly]
    public DamageProfile damageProfile_Preview;

    public void Set_DamageProfilePreview(DamageProfile damageProfile)
    {
        if (damageProfile == null)
            return;

        damageProfile_Preview ??= new DamageProfile();
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
