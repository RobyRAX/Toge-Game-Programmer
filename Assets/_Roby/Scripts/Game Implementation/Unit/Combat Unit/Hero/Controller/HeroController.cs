using Animancer;
using Cysharp.Threading.Tasks;
using RAXY.Movement;
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

    public override void SetSuspend(bool suspend)
    {
        base.SetSuspend(suspend);

        if (suspend)
        {
            Brain_Exploration.Unsubscribe();
            enabled = false;
            MovementCont.enabled = false;
            GetComponent<GroundChecker>().enabled = false;
            heroCombatant.enabled = false;
            GetComponent<AnimancerComponent>().enabled = false;
        }
        else
        {
            Brain_Exploration.Subscribe();
            enabled = true;
            MovementCont.enabled = true;
            GetComponent<GroundChecker>().enabled = true;
            heroCombatant.enabled = true;
            GetComponent<AnimancerComponent>().enabled = true;
        }
    }
}
