using Cysharp.Threading.Tasks;
using UnityEngine;

public class HeroController : CombatUnitController
{
    public HeroDataSO heroDataSO;
    public HeroCombatant heroCombatant;

    public override CombatantBase CombatantCont { get => heroCombatant; }

    public override async UniTask Init()
    {
        await base.Init();

        FirstInitDone = false;

        heroCombatant = GetComponent<HeroCombatant>();
        heroCombatant.heroCombatDataSO = heroDataSO.CombatDataSO;
        heroCombatant.Init(InventoryManager.Instance.GetInstanceHero(heroDataSO.ItemId));

        FirstInitDone = true;
    }
}
