using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using RAXY.Utility;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using Unity.Cinemachine;
using UnityEngine;

public class GameplayManager : Singleton<GameplayManager>, ISepObject
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

        SpawnPointDict = new();
        CurrentSpawnPointId = GameplayConfig.Instance.ConfigSO.initialSpawnPoint;

        var allSpawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var spawnPoint in allSpawnPoints)
        {
            SpawnPointDict.Add(spawnPoint.spawnPointId, spawnPoint);
        }

        SpawnedHeroDict = new();

        int index = 0;
        foreach (var heroId in partyIds)
        {
            await SpawnHero(heroId, index == 0);
            index++;
        }

        FirstInitDone = true;
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    [TitleGroup("UI")]
    public DefeatScreenUI defeatScreen;

    [TitleGroup("Camera")]
    public Camera mainCamera;

    [TitleGroup("Camera")]
    public CinemachineCamera topDownCamera;

    [TitleGroup("Camera")]
    public CinemachineCamera combatCamera;

    [TitleGroup("Brain Config")]
    public ActiveUnitBrainExplorationConfigSO defaultActiveUnitBrainExplorationConfigSO;

    [TitleGroup("Party")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<string> partyIds;

    [TitleGroup("Spawned Hero")]
    [ShowInInspector]
    public Dictionary<string, HeroController> SpawnedHeroDict { get; set; } = new();

    [TitleGroup("Spawned Hero")]
    [ShowInInspector]
    public HeroController MainHero { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public GameplayState CurrentState { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public string CurrentSpawnPointId { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public Dictionary<string, SpawnPoint> SpawnPointDict { get; set; }

    public event Action OnRespawn;

    void Start()
    {
    }

    public async UniTask SpawnHero(string heroId, bool isMainHero)
    {
        var heroDataSO = GlobalManager.Instance.HeroDatabase.GetHeroData(heroId);

        if (heroDataSO == null)
            return;
        
        var heroPrefab = heroDataSO.unitPrefab;

        var heroClone = Instantiate(heroPrefab);
        heroClone.transform.SetParent(GameplayDependencyManager.Instance.heroSpawnRoot);
        
        var heroCont = heroClone.GetComponent<HeroController>();
        heroCont.heroDataSO = heroDataSO;
        heroCont.AnimationClips = heroDataSO.AnimationClipsSO;

        if (isMainHero)
            MainHero = heroCont;

        GameplayDependencyManager.Instance.RegisterSepObject(heroCont, GameplayDependencyManager.HERO_SEP_GROUP);

        SpawnedHeroDict.Add(heroId, heroCont);
    }

    void OnGameplayDependencyInitDoneHandler(bool firstInit)
    {
        foreach (var hero in SpawnedHeroDict.Values)
        {
            var heroCont = hero.GetComponent<HeroController>();

            if (hero == MainHero)
            {
                heroCont.Setup_BrainExploration(BrainExplorationType.ActiveUnit, defaultActiveUnitBrainExplorationConfigSO);
                topDownCamera.Follow = hero.transform;
                topDownCamera.LookAt = hero.transform;

                MoveHeroToSpawnPoint(hero).Forget();
            }
        }

        GameplayDependencyManager.Instance.OnInitDone -= OnGameplayDependencyInitDoneHandler;

        TurnBaseCombatManager.Instance.OnCombatStarted += CombatStartedHandler;
        TurnBaseCombatManager.Instance.OnCombatEnded += CombatEndedHandler;

        if (defeatScreen != null)
        {
            defeatScreen.OnRespawnClicked -= RespawnClickedHandler;
            defeatScreen.OnRespawnClicked += RespawnClickedHandler;
            defeatScreen.Hide();
        }

        ChangeState(GameplayState.Explore);
    }

    public async UniTask MoveHeroToSpawnPoint(HeroController hero)
    {
        if (string.IsNullOrEmpty(CurrentSpawnPointId) ||
            !SpawnPointDict.TryGetValue(CurrentSpawnPointId, out var spawnPoint) ||
            spawnPoint == null)
            return;
        
        hero.GetComponent<CharacterController>().enabled = false;
        hero.GetComponent<UnitMovement>().enabled = false;
        
        var spawnPointReal = spawnPoint.spawnPoint.transform;
        hero.transform.SetPositionAndRotation(spawnPointReal.position,
                                                spawnPointReal.rotation);

        await UniTask.Yield();

        hero.GetComponent<CharacterController>().enabled = true;
        hero.GetComponent<UnitMovement>().enabled = true;
    }

    private void CombatEndedHandler(TurnSide side)
    {
        if (side == TurnSide.Player)
        {
            ChangeState(GameplayState.Explore);
        }
        else if (side == TurnSide.Enemy)
        {
            defeatScreen?.Show();
        }
    }

    void RespawnClickedHandler()
    {
        defeatScreen?.Hide();
        HandleLoseCondition().Forget();
    }

    async UniTask HandleLoseCondition()
    {
        ReviveParty();

        foreach (var hero in SpawnedHeroDict.Values)
        {
            if (hero == null)
                continue;

            await MoveHeroToSpawnPoint(hero);
        }

        OnRespawn?.Invoke();
        ChangeState(GameplayState.Explore);
    }

    void ReviveParty()
    {
        foreach (var hero in SpawnedHeroDict.Values)
        {
            if (hero == null || hero.heroCombatant == null)
                continue;

            hero.heroCombatant.SetAlive();
            hero.StateMachine_Exploration.ChangeState_Idle();
        }
    }

    public void NotifySpawnPointReached(SpawnPoint spawnPoint, HeroController hero)
    {
        if (spawnPoint == null || hero == null)
            return;

        // Hanya main hero yang menentukan checkpoint terakhir.
        if (hero != MainHero)
            return;

        if (string.IsNullOrEmpty(spawnPoint.spawnPointId))
            return;

        CurrentSpawnPointId = spawnPoint.spawnPointId;
    }

    void CombatStartedHandler()
    {
        ChangeState(GameplayState.Combat);
    }

    public void ChangeState(GameplayState newState)
    {
        CurrentState = newState;

        if (CurrentState == GameplayState.Explore)
        {
            topDownCamera.Prioritize();
        }
        else if (CurrentState == GameplayState.Combat)
        {
            combatCamera.Prioritize();
        }
    }
}

public enum GameplayState
{
    None, Explore, Combat
}