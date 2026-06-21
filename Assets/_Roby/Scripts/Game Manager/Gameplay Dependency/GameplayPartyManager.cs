using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using RAXY.Utility;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

public class GameplayPartyManager : Singleton<GameplayPartyManager>, ISepObject
{
    #region  ISepObject
    public GameObject GetGameObject => gameObject;

    public bool FirstInitDone { get; set; }
    public int Order { get; set; }
    public string SepGroup { get; set; }
    public bool UsePreInit { get; set; }

    public async UniTask Init()
    {
        var allInstanceHeroes = InventoryManager.Instance.GetAllItemInstanceHeroes();

        foreach (var heroInstance in allInstanceHeroes)
        {
            await SpawnHero(heroInstance.ItemId);
        }
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    [TitleGroup("Brain Config")]
    public ActiveUnitBrainExplorationConfigSO defaultActiveUnitBrainExplorationConfigSO;

    void Start()
    {
    }

    public async UniTask SpawnHero(string heroId)
    {
        var heroDataSO = GlobalManager.Instance.HeroDatabase.GetHeroData(heroId);

        if (heroDataSO == null)
            return;
        
        var heroPrefab = heroDataSO.heroPrefab;

        var heroClone = Instantiate(heroPrefab);
        heroClone.transform.SetParent(GameplayDependencyManager.Instance.heroSpawnRoot);
        
        var heroCont = heroClone.GetComponent<HeroController>();

        await heroCont.Init();
        heroCont.Setup_BrainExploration(BrainExplorationType.ActiveUnit, defaultActiveUnitBrainExplorationConfigSO);
    }
}
