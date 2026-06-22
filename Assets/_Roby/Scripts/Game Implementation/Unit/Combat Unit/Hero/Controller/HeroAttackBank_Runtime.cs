using System.Collections.Generic;
using UnityEngine;

public class HeroAttackBank_Runtime : CombatAttackBankBase_Runtime
{
    ItemInstance_Hero heroInstance;

    public Talent_Runtime NormalAttackTalent;
    public Talent_Runtime SkillTalent;
    public Talent_Runtime UltimateTalent;

    List<Attack_Runtime> _attacks;
    public override List<Attack_Runtime> Attacks => _attacks;

    public HeroAttackBank_Runtime(HeroCombatant heroCombatant) : base(heroCombatant)
    {
        heroInstance = heroCombatant.HeroInstance;
    }

    public Attack_Runtime CreateAttackRuntime()
    {
        return null;
    }
}

public class Talent_Runtime
{
    public int level;
    public List<Attack_Runtime> attacks;
}