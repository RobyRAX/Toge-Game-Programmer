using ToGaProTest.Shared;
using UnityEngine;

public class HeroCombatant : CombatantBase
{
    public HeroCombatDataSO CombatData;

    public ItemInstance_Hero HeroInstance { get; set; }
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

    public void Init(ItemInstance_Hero heroInstance)
    {
        HeroInstance = heroInstance;
        SetAlive();
    }

    public void SetAlive(bool fullHp = true, float hp = 1)
    {
        if (fullHp)
            CurrentHp = StatContainer.GetTotalValue(StatAttribute.MaxHp);
        else
            CurrentHp = hp;
        
        IsAlive = true;
    }
}
