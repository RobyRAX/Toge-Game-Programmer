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
        if (heroCombatant == null || combatDataSO == null)
            return;

        _attacks = new();

        NormalAttackTalent = new();
        NormalAttackTalent.level = HeroInstance.NormalAttackTalentLevel;
        foreach (var attackSO in combatDataSO.NormalAttackTalent.attacks)
        {
            var newAttack = CreateAttackRuntime(attackSO);
            NormalAttackTalent.attacks.Add(newAttack);
            _attacks.Add(newAttack);
        }

        SkillTalent = new();
        SkillTalent.level = HeroInstance.SkillTalentLevel;
        foreach (var attackSO in combatDataSO.SkillTalent.attacks)
        {
            var newAttack = CreateAttackRuntime(attackSO);
            SkillTalent.attacks.Add(newAttack);
            _attacks.Add(newAttack);
        }

        UltimateTalent = new();
        UltimateTalent.level = HeroInstance.UltimateTalentLevel;
        foreach (var attackSO in combatDataSO.UltimateTalent.attacks)
        {
            var newAttack = CreateAttackRuntime(attackSO);
            UltimateTalent.attacks.Add(newAttack);
            _attacks.Add(newAttack);
        }
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
    public List<Attack_Runtime> attacks = new();
}