using Cysharp.Threading.Tasks;
using UnityEngine;

public class HeroController : CombatUnitController
{
    public HeroDataSO heroDataSO;
    public HeroCombatant HeroCombatant { get; set; }

    public override async UniTask Init()
    {
        await base.Init();

        FirstInitDone = false;

        HeroCombatant = GetComponent<HeroCombatant>();
        HeroCombatant.CombatData = heroDataSO.CombatDataSO;
        HeroCombatant.Init(InventoryManager.Instance.GetInstanceHero(heroDataSO.ItemId));

        FirstInitDone = true;
    }
}
