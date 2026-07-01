using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using RAXY.InputSystem;
using RAXY.InteractionSystem;
using RAXY.Movement;
using RAXY.Utility;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using Unity.Cinemachine;
using UnityEngine;

public class GameplayManager : Singleton<GameplayManager>, ISepObject
{
    #region  ISepObject
    public GameObject GetGameObject => gameObject;

    public bool InitDone { get; set; }
    public int Order { get; set; }
    public string SepGroup { get; set; }
    public bool UsePreInit { get; set; }

    public async UniTask Init()
    {
        GameplayDependencyManager.Instance.OnInitDone -= GameplayDependencyInitDoneHandler;
        GameplayDependencyManager.Instance.OnInitDone += GameplayDependencyInitDoneHandler;

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

        InitDone = true;
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    [TitleGroup("UI")]
    public ExploreUI exploreUI;

    [TitleGroup("UI")]
    public DefeatScreenUI defeatScreen;

    [TitleGroup("Camera")]
    public Camera mainCamera;

    [TitleGroup("Camera")]
    public CinemachineCamera topDownCamera;

    [TitleGroup("Camera")]
    public CinemachineCamera combatCamera;

    [TitleGroup("Party")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<string> partyIds;

    [TitleGroup("Spawned Hero")]
    [ShowInInspector]
    public Dictionary<string, HeroController> SpawnedHeroDict { get; set; } = new();

    [TitleGroup("Spawned Hero")]
    [ShowInInspector]
    public HeroController MainHero { get; set; }

    [TitleGroup("Spawned Hero")]
    [ShowInInspector]
    public Interactor MainHeroInteractor { get; set; }

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
        {
            MainHero = heroCont;
            MainHeroInteractor = heroCont.GetComponent<Interactor>();
        }

        GameplayDependencyManager.Instance.RegisterSepObject(heroCont, GameplayDependencyManager.HERO_SEP_GROUP);

        SpawnedHeroDict.Add(heroId, heroCont);
    }

    void GameplayDependencyInitDoneHandler(bool firstInit)
    {
        foreach (var hero in SpawnedHeroDict.Values)
        {
            var heroCont = hero.GetComponent<HeroController>();

            if (hero == MainHero)
            {
                var config = GameplayConfig.Instance.ConfigSO.defaultActiveUnitBrainExplorationConfigSO;
                heroCont.Setup_BrainExploration(BrainExplorationType.ActiveUnit, config);
                topDownCamera.Follow = hero.transform;
                topDownCamera.LookAt = hero.transform;

                MoveHeroToSpawnPoint(hero).Forget();
            }
        }

        GameplayDependencyManager.Instance.OnInitDone -= GameplayDependencyInitDoneHandler;

        TurnBaseCombatManager.Instance.OnCombatStarted += CombatStartedHandler;
        TurnBaseCombatManager.Instance.OnCombatEnded += CombatEndedHandler;

        CutsceneManager.Instance.OnCutsceneStarted += CutsceneStartedHandler;
        CutsceneManager.Instance.OnCutsceneEnded += CutsceneEndedHandler;

        GameplayConfig.Instance.ConfigSO.InteractEventSO.Unsubscribe(InteractHandler);
        GameplayConfig.Instance.ConfigSO.InteractEventSO.Subscribe(InteractHandler);

        if (defeatScreen != null)
        {
            defeatScreen.OnRespawnClicked -= RespawnClickedHandler;
            defeatScreen.OnRespawnClicked += RespawnClickedHandler;
            defeatScreen.Hide();
        }

        exploreUI?.Setup(this);

        ChangeState(GameplayState.Explore);
    }

    void InteractHandler(InputContext ctx)
    {
        if (MainHeroInteractor == null)
            return;
        
        MainHeroInteractor.Interact();
    }

    void CutsceneStartedHandler()
    {
        foreach (var hero in SpawnedHeroDict.Values)
        {
            hero.SetSuspend(true);
        }

        exploreUI.Hide();
    }

    void CutsceneEndedHandler()
    {
        foreach (var hero in SpawnedHeroDict.Values)
        {
            hero.SetSuspend(false);
        }

        exploreUI.Show();
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
            exploreUI?.Show();
        }
        else if (CurrentState == GameplayState.Combat)
        {
            combatCamera.Prioritize();
            exploreUI?.Hide();
        }
    }
}

public enum GameplayState
{
    None, Explore, Combat
}