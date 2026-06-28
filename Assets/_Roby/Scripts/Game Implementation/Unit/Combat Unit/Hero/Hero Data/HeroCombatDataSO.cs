using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroCombatDataSO", menuName = "RAXY/Unit/Hero/Combat Data")]
public class HeroCombatDataSO : CombatDataBaseSO
{
    [TitleGroup("Talent")]
    public Talent NormalAttackTalent;

    [TitleGroup("Talent")]
    public Talent SkillTalent;

    [TitleGroup("Talent")]
    public Talent UltimateTalent;

    public override List<CombatAttackBaseSO> Attacks
    {
        get
        {
            var attacksTemp = new List<CombatAttackBaseSO>();
            attacksTemp.AddRange(NormalAttackTalent.attacks);
            attacksTemp.AddRange(SkillTalent.attacks);
            attacksTemp.AddRange(UltimateTalent.attacks);

            return attacksTemp;
        }
    }
}

[Serializable]
public class Talent
{
    public Sprite talentIcon;

    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]
    public List<HeroAttackSO> attacks;

    [BoxGroup("Attack Attribute")]
    [SerializeField]
    bool useAttributeSO;

    [BoxGroup("Attack Attribute")]
    [SerializeField]
    [ShowIf("@useAttributeSO")]
    AttackAttributeSO attackAttributeSO;

    [BoxGroup("Attack Attribute")]
    [SerializeField]
    [HideIf("@useAttributeSO")]
    [HideLabel]
    AttackAttribute attackAttribute;

    public AttackAttribute AttackAttribute
    {
        get
        {
            return useAttributeSO ? attackAttributeSO.attackAttribute : attackAttribute;
        }
    }
}