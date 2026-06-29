using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class HeroAttackBank_Runtime : CombatAttackBankBase_Runtime
{
    ItemInstance_Hero HeroInstance => heroCombatant.HeroInstance;
    HeroCombatant heroCombatant;
    HeroCombatDataSO combatDataSO;

    [HideReferenceObjectPicker]
    public Talent_Runtime NormalAttackTalent;

    [HideReferenceObjectPicker]
    public Talent_Runtime SkillTalent;

    [HideReferenceObjectPicker]
    public Talent_Runtime UltimateTalent;

    public Attack_Runtime PrimaryUltimateAttack
    {
        get
        {
            var attacks = UltimateTalent?.attacks;
            return attacks != null && attacks.Count > 0 ? attacks[0] : null;
        }
    }

    List<Attack_Runtime> _attacks;
    public override List<Attack_Runtime> Attacks => _attacks;

    public HeroAttackBank_Runtime(HeroCombatant heroCombatant) : base(heroCombatant)
    {
        this.heroCombatant = heroCombatant;
        combatDataSO = heroCombatant.CombatDataSO as HeroCombatDataSO;

        BuildAttacks();
    }

    public void BuildAttacks()
    {
        if (heroCombatant == null || combatDataSO == null || HeroInstance == null)
            return;

        _attacks = new();

        NormalAttackTalent = BuildTalentAttacks(
            combatDataSO.NormalAttackTalent,
            HeroInstance.NormalAttackTalentLevel);

        SkillTalent = BuildTalentAttacks(
            combatDataSO.SkillTalent,
            HeroInstance.SkillTalentLevel);

        UltimateTalent = BuildTalentAttacks(
            combatDataSO.UltimateTalent,
            HeroInstance.UltimateTalentLevel);
    }

    Talent_Runtime BuildTalentAttacks(Talent talentData, int level)
    {
        var talentRuntime = new Talent_Runtime { level = level };

        if (talentData?.attacks == null)
            return talentRuntime;

        foreach (var attackSO in talentData.attacks)
        {
            if (attackSO == null)
                continue;

            var newAttack = CreateAttackRuntime(attackSO);
            if (newAttack == null)
                continue;

            if (newAttack.damageProfile == null)
                Debug.LogWarning($"{nameof(HeroAttackBank_Runtime)}: {attackSO.name} has no damage profile.");

            talentRuntime.attacks.Add(newAttack);
            _attacks.Add(newAttack);
        }

        return talentRuntime;
    }

    public Attack_Runtime CreateAttackRuntime(HeroAttackSO attackSO)
    {
        if (heroCombatant == null || combatDataSO == null)
            return null;

        Attack_Runtime newAttack = new(attackSO, heroCombatant);
        return newAttack;
    }
}

public class Talent_Runtime
{
    public int level;

    [HideReferenceObjectPicker]
    public List<Attack_Runtime> attacks = new();
}