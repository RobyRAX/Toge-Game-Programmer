using Cysharp.Threading.Tasks;
using UnityEngine;

public class HeroController : CombatUnitController
{
    public HeroDataSO heroDataSO;
    public HeroCombatant heroCombatant;

    public override CombatantBase CombatantCont { get => heroCombatant; }
    public override UnitDataSO UnitData => heroDataSO;

    public override async UniTask Init()
    {
        await base.Init();

        FirstInitDone = false;

        SetHitboxSetting(GameplayConfig.Instance.ConfigSO.heroHitboxSetting);

        heroCombatant = GetComponent<HeroCombatant>();
        heroCombatant.Init(InventoryManager.Instance.GetInstanceHero(heroDataSO.ItemId));

        FirstInitDone = true;
    }
}
