using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using RAXY.Dialogue;
using RAXY.InputSystem;
using RAXY.InteractionSystem;
using RAXY.Movement;
using RAXY.Utility;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    const float PortalTeleportCooldown = 0.5f;

    string pendingPortalTargetId;
    bool isPortalTransitionInProgress;
    float lastPortalTeleportTime;

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

        TurnBaseCombatManager.Instance.OnCombatStarted += CombatStartedHandler;
        TurnBaseCombatManager.Instance.OnCombatEnded += CombatEndedHandler;

        CutsceneManager.Instance.OnCutsceneStarted += StoryStartedHandler;
        CutsceneManager.Instance.OnCutsceneEnded += StoryEndedHandler;

        DialogueManager.Instance.OnDialogueStarted += StoryStartedHandler;
        DialogueManager.Instance.OnDialogueEnded += StoryEndedHandler;

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

        QuestWannabe.Instance.StartQuest(0);

        SceneManager.sceneLoaded -= OnSceneLoadedForPortal;
        SceneManager.sceneLoaded += OnSceneLoadedForPortal;

        GameplayDependencyManager.Instance.OnInitDone -= GameplayDependencyInitDoneHandler;
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForPortal;
        base.OnDestroy();
    }

    void InteractHandler(InputContext ctx)
    {
        if (MainHeroInteractor == null)
            return;
        
        MainHeroInteractor.Interact();
    }

    void StoryStartedHandler()
    {
        foreach (var hero in SpawnedHeroDict.Values)
        {
            hero.SetSuspend(true);
        }

        exploreUI.Hide();
    }

    void StoryEndedHandler()
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

        await TeleportHeroTo(hero, spawnPoint.spawnPoint);
    }

    public void RequestTeleportViaPortal(Portal source)
    {
        if (source == null || isPortalTransitionInProgress)
            return;

        if (CurrentState != GameplayState.Explore || MainHero == null)
            return;

        if (Time.time - lastPortalTeleportTime < PortalTeleportCooldown)
            return;

        if (string.IsNullOrEmpty(source.portalTargetId))
            return;

        TeleportViaPortalAsync(source).Forget();
    }

    async UniTask TeleportViaPortalAsync(Portal source)
    {
        isPortalTransitionInProgress = true;
        MainHero.SetSuspend(true);

        try
        {
            bool isSameScene = string.IsNullOrEmpty(source.sceneTargetName) ||
                               source.sceneTargetName == SceneManager.GetActiveScene().name;

            if (isSameScene)
            {
                await TeleportToTargetPortal(source.portalTargetId);
            }
            else
            {
                pendingPortalTargetId = source.portalTargetId;
                await SceneManager.LoadSceneAsync(source.sceneTargetName);
                await CompletePendingPortalArrival();
                await UniTask.Yield();
                await GameplayDependencyManager.Instance.InitializeAsync_Teleport();
            }
        }
        finally
        {
            MainHero?.SetSuspend(false);
            isPortalTransitionInProgress = false;
            lastPortalTeleportTime = Time.time;
        }
    }

    void OnSceneLoadedForPortal(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(pendingPortalTargetId) || !isPortalTransitionInProgress)
            return;

        CompletePendingPortalArrival().Forget();
    }

    async UniTask CompletePendingPortalArrival()
    {
        if (string.IsNullOrEmpty(pendingPortalTargetId))
            return;

        var targetId = pendingPortalTargetId;
        pendingPortalTargetId = null;

        await TeleportToTargetPortal(targetId);
    }

    async UniTask TeleportToTargetPortal(string portalId)
    {
        var target = FindPortalById(portalId);
        if (target == null)
        {
            Debug.LogWarning($"[GameplayManager] Portal target '{portalId}' not found in scene '{SceneManager.GetActiveScene().name}'.");
            return;
        }

        await TeleportHeroTo(MainHero, ResolveArrivalPoint(target));
    }

    public async UniTask TeleportHeroTo(HeroController hero, Transform destination)
    {
        if (hero == null || destination == null)
            return;

        hero.GetComponent<CharacterController>().enabled = false;
        hero.GetComponent<UnitMovement>().enabled = false;

        hero.transform.SetPositionAndRotation(destination.position, destination.rotation);

        await UniTask.Yield();

        hero.GetComponent<CharacterController>().enabled = true;
        hero.GetComponent<UnitMovement>().enabled = true;
    }

    Portal FindPortalById(string portalId)
    {
        if (string.IsNullOrEmpty(portalId))
            return null;

        var portals = FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var portal in portals)
        {
            if (portal.portalId == portalId)
                return portal;
        }

        return null;
    }

    static Transform ResolveArrivalPoint(Portal target)
    {
        return target.spawnPoint != null ? target.spawnPoint : target.transform;
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