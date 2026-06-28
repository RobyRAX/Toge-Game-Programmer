using ToGaProTest.Shared;
using UnityEngine;

public class HeroCombatant : CombatantBase
{
    public HeroCombatDataSO heroCombatDataSO;
    public override CombatDataBaseSO CombatDataSO
    {
        get => heroCombatDataSO;
        set => heroCombatDataSO = value as HeroCombatDataSO;
    }

    HeroAttackBank_Runtime heroAttackBank;
    public override CombatAttackBankBase_Runtime AttackBank
    {
        get => heroAttackBank;
        set => heroAttackBank = value as HeroAttackBank_Runtime;
    }

    public ItemInstance_Hero HeroInstance { get; set; }

    float _currentStamina;
    public override float CurrentStamina
    {
        get => _currentStamina;
        set
        {
            _currentStamina = value;
            NotifyStatsChanged();
        }
    }

    public float CurrentUltimateGauge { get; private set; }
    public const float MaxUltimateGauge = 100f;
    public override float CurrentHp
    {
        get => HeroInstance?.currentHp ?? 0;
        set => HeroInstance.currentHp = value;
    }

    public override StatContainer_Runtime StatContainer
    {
        get
        {
            if (HeroInstance == null)
                return null;
                
            return HeroInstance.GetStatContainer();
        }
    }

    public override DamageProfileWithAttribute GetDamageProfile(CombatAttackBaseSO attackSO)
    {
        if (attackSO is not HeroAttackSO heroAttackSO)
            return base.GetDamageProfile(attackSO);

        var provider = heroAttackSO.damageProfileProvider;
        if (provider == null || HeroInstance == null)
            return null;

        int talentLevel = provider.talent switch
        {
            HeroTalentType.NormalAttack => HeroInstance.NormalAttackTalentLevel,
            HeroTalentType.Skill => HeroInstance.SkillTalentLevel,
            HeroTalentType.Ultimate => HeroInstance.UltimateTalentLevel,
            _ => 1
        };

        return provider.ResolveDamageProfile(heroCombatDataSO, talentLevel);
    }

    public void Init(ItemInstance_Hero heroInstance)
    {
        HeroInstance = heroInstance;
        heroCombatDataSO = HeroInstance.heroDataSO.heroCombatDataSO;

        AnimationClips = HeroInstance.heroDataSO.AnimationClipsSO;
        InitStateMachine();

        AttackBank = new HeroAttackBank_Runtime(this);

        CombatantInfo = new CombatantInfo
        {
            unitName = HeroInstance.heroDataSO.ItemName,
            unitIcon = HeroInstance.heroDataSO.ItemIcon,
        };

        SetAlive();
    }

    public void AddUltimateGauge(float amount)
    {
        CurrentUltimateGauge = Mathf.Min(MaxUltimateGauge, CurrentUltimateGauge + amount);
        NotifyStatsChanged();
    }

    public void SetAlive(bool fullHp = true, float hp = 1)
    {
        if (fullHp)
            CurrentHp = StatContainer.GetTotalValue(StatAttribute.MaxHp);
        else
            CurrentHp = hp;

        CurrentStamina = StatContainer.GetTotalValue(StatAttribute.MaxStamina);
        CurrentUltimateGauge = 0f;
        IsAlive = true;
    }
}
