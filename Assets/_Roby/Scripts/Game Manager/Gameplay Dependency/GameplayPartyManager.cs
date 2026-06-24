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
        GameplayDependencyManager.Instance.OnInitDone -= OnGameplayDependencyInitDoneHandler;
        GameplayDependencyManager.Instance.OnInitDone += OnGameplayDependencyInitDoneHandler;

        var allInstanceHeroes = InventoryManager.Instance.GetAllItemInstanceHeroes();

        SpawnedHeroObjDict = new();
        foreach (var heroInstance in allInstanceHeroes)
        {
            await SpawnHero(heroInstance.ItemId);
        }

        FirstInitDone = true;
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    [TitleGroup("Brain Config")]
    public ActiveUnitBrainExplorationConfigSO defaultActiveUnitBrainExplorationConfigSO;

    [TitleGroup("Spawned Hero")]
    public Dictionary<string, GameObject> SpawnedHeroObjDict = new();

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
        heroCont.heroDataSO = heroDataSO;
        heroCont.AnimationClips = heroDataSO.AnimationClipsSO;
        GameplayDependencyManager.Instance.RegisterSepObject(heroCont, GameplayDependencyManager.HERO_SEP_GROUP);

        SpawnedHeroObjDict.Add(heroId, heroClone);
    }

    void OnGameplayDependencyInitDoneHandler(bool firstInit)
    {
        foreach (var hero in SpawnedHeroObjDict.Values)
        {
            var heroCont = hero.GetComponent<HeroController>();
            heroCont.Setup_BrainExploration(BrainExplorationType.ActiveUnit, defaultActiveUnitBrainExplorationConfigSO);
        }

        GameplayDependencyManager.Instance.OnInitDone -= OnGameplayDependencyInitDoneHandler;
    }
}
