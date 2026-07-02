using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using RAXY.InputSystem;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestWannabe : Singleton<QuestWannabe>//, ISepObject
{
// #if UNITY_EDITOR
//     public GameObject GetGameObject => gameObject;

//     public bool InitDone { get; set; }
//     public int Order { get; set; }
//     public string SepGroup { get; set; }
//     public bool UsePreInit { get; set; }

//     public async UniTask PreInit()
//     {
//     }

//     public async UniTask Init()
//     {
//         if (questStepEntries != null && questStepEntries.Count > 0)
//             StartQuest(0);

//         InitDone = true;
//     }
// #endif

    [TitleGroup("Navigator")]
    public GameObject navigatorObject;

    [TitleGroup("Navigator")]
    public InputActionEventSO showNavigatorEventSO;

    [TitleGroup("Navigator")]
    [Min(0f)]
    public float showNavigatorDuration = 3f;

    [TitleGroup("Marker")]
    public QuestMarker questMarker;

    [TitleGroup("Quest Step")]
    public string initialScene = "Guild";

    [TitleGroup("Quest Step")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "questId")]
    public List<QuestStepEntry> questStepEntries;

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public QuestStepEntry CurrentQuest { get; private set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public int CurrentStepIndex { get; private set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public bool IsQuestActive { get; private set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public bool IsAllQuestCompleted { get; private set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public bool RuntimeAutoComplete { get; private set; }

    QuestStepActionContext actionContext;
    bool isAdvancingStep;
    GameObject navigatorInstance;
    QuestNavigator navigatorComponent;
    bool navigatorWarnedMissingPrefab;
    Transform navigatorOriginalParent;
    Quaternion navigatorOriginalLocalRotation;
    Vector3 navigatorOriginalLocalScale;
    bool navigatorOriginalTransformCached;
    CancellationTokenSource showNavigatorCts;

    static readonly Vector3 NavigatorHiddenLocalPosition = new(0f, -1000f, 0f);

    public event Action<QuestStepEntry> OnQuestStepChanged;

    public bool IsNavigatorVisible => navigatorInstance != null && navigatorInstance.activeSelf;

    public void ShowNavigator()
    {
        EnsureNavigator();
        SetNavigatorActive(true);
    }

    public void HideNavigator()
    {
        SetNavigatorActive(false);
    }

    protected override void Awake()
    {
        base.Awake();

        actionContext = new QuestStepActionContext { Manager = this };
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedHandler;

        showNavigatorEventSO?.Unsubscribe(ShowNavigatorInputHandler);
        showNavigatorEventSO?.Subscribe(ShowNavigatorInputHandler);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedHandler;

        showNavigatorEventSO?.Unsubscribe(ShowNavigatorInputHandler);

        showNavigatorCts?.Cancel();
        showNavigatorCts?.Dispose();
        showNavigatorCts = null;
    }

    void Update()
    {
        if (!IsQuestActive || IsAllQuestCompleted || !RuntimeAutoComplete)
            return;

        if (CurrentQuest == null || !CurrentQuest.autoCompleteOnReachMarker)
            return;

        if (GameplayManager.Instance == null ||
            GameplayManager.Instance.CurrentState != GameplayState.Explore ||
            GameplayManager.Instance.MainHero == null)
            return;

        if (!IsPlayerAtMarker())
            return;

        CompleteCurrentStep();
    }

    void OnSceneLoadedHandler(Scene scene, LoadSceneMode mode)
    {
        RefreshMarker();
        EnsureNavigator();
    }

    public void StartQuest(int index = 0)
    {
        if (questStepEntries == null || questStepEntries.Count == 0)
            return;
        
        if (index == 0)
        {
            if (SceneManager.GetActiveScene().name != initialScene)
                return;
        }

        CurrentStepIndex = Mathf.Clamp(index, 0, questStepEntries.Count - 1);
        IsQuestActive = true;
        IsAllQuestCompleted = false;
        EnsureNavigator();
        SetNavigatorActive(false);
        EnterStepAsync().Forget();
    }

    public void CompleteCurrentStep()
    {
        CompleteCurrentStepAsync().Forget();
    }

    async UniTask CompleteCurrentStepAsync()
    {
        if (!IsQuestActive || IsAllQuestCompleted || CurrentQuest == null || isAdvancingStep)
            return;

        isAdvancingStep = true;

        try
        {
            var completedStep = CurrentQuest;
            await RunActions(completedStep.onExitActions, completedStep);

            if (completedStep.expReward > 0)
                GameplayManager.Instance?.AddExpToSpawnedHeroes(completedStep.expReward);

            CurrentStepIndex++;

            if (CurrentStepIndex >= questStepEntries.Count)
            {
                IsAllQuestCompleted = true;
                IsQuestActive = false;
                CurrentQuest = null;
                questMarker?.Hide();
                SetNavigatorActive(false);
                OnQuestStepChanged?.Invoke(null);
                return;
            }

            await EnterStepAsync();
        }
        finally
        {
            isAdvancingStep = false;
        }
    }

    public void SetRuntimeAutoComplete(bool value)
    {
        RuntimeAutoComplete = value;
    }

    public Vector3 GetResolvedMarkerPosition()
    {
        if (CurrentQuest == null)
            return Vector3.zero;

        if (SceneManager.GetActiveScene().name == CurrentQuest.sceneLocation)
            return CurrentQuest.markerPosition;

        var portal = FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.sceneTargetName == CurrentQuest.sceneLocation);

        if (portal == null)
        {
            Debug.LogWarning(
                $"[QuestWannabe] No portal found for scene '{CurrentQuest.sceneLocation}'. Using marker position fallback.");
            return CurrentQuest.markerPosition;
        }

        return portal.transform.position;
    }

    public void RefreshMarker()
    {
        if (questMarker == null)
            return;

        if (!IsQuestActive || IsAllQuestCompleted || CurrentQuest == null)
        {
            questMarker.Hide();
            SetNavigatorActive(false);
            return;
        }

        questMarker.SetMarker(GetResolvedMarkerPosition(), CurrentQuest.markerRadius);
        EnsureNavigator();
    }

    async UniTask EnterStepAsync()
    {
        CurrentQuest = questStepEntries[CurrentStepIndex];
        RuntimeAutoComplete = CurrentQuest.autoCompleteOnReachMarker;

        await RunActions(CurrentQuest.onEnterActions, CurrentQuest);
        RefreshMarker();
        EnsureNavigator();
        SetNavigatorActive(false);
        OnQuestStepChanged?.Invoke(CurrentQuest);
    }

    async UniTask RunActions(List<QuestStepActionEntry> actions, QuestStepEntry step)
    {
        if (actions == null || actions.Count == 0 || step == null)
            return;

        actionContext.Step = step;

        foreach (var entry in actions)
        {
            if (entry?.ActionSO == null)
                continue;

            await entry.ActionSO.ExecuteAsync(actionContext, entry.Parameter);
        }
    }

    bool IsPlayerAtMarker()
    {
        var heroPos = GameplayManager.Instance.MainHero.transform.position;
        var targetPos = GetResolvedMarkerPosition();

        var heroFlat = new Vector3(heroPos.x, 0f, heroPos.z);
        var targetFlat = new Vector3(targetPos.x, 0f, targetPos.z);

        return Vector3.Distance(heroFlat, targetFlat) <= CurrentQuest.markerRadius;
    }

    void EnsureNavigator()
    {
        if (!IsQuestActive || IsAllQuestCompleted || CurrentQuest == null)
        {
            SetNavigatorActive(false);
            return;
        }

        if (GameplayManager.Instance == null || GameplayManager.Instance.MainHero == null)
        {
            SetNavigatorActive(false);
            return;
        }

        if (navigatorObject == null)
        {
            SetNavigatorActive(false);

            if (!navigatorWarnedMissingPrefab)
            {
                navigatorWarnedMissingPrefab = true;
                Debug.LogWarning("[QuestWannabe] navigatorObject is not assigned. Quest navigator will not appear.");
            }

            return;
        }

        navigatorWarnedMissingPrefab = false;

        if (navigatorInstance == null)
            navigatorInstance = navigatorObject;

        if (navigatorInstance == null)
            return;

        if (!navigatorOriginalTransformCached)
        {
            navigatorOriginalParent = navigatorInstance.transform.parent;
            navigatorOriginalLocalRotation = navigatorInstance.transform.localRotation;
            navigatorOriginalLocalScale = navigatorInstance.transform.localScale;
            navigatorOriginalTransformCached = true;
        }

        if (navigatorInstance.transform.parent != GameplayManager.Instance.MainHero.transform)
        {
            navigatorInstance.transform.SetParent(GameplayManager.Instance.MainHero.transform, worldPositionStays: false);
            navigatorInstance.transform.localPosition = Vector3.zero;
            navigatorInstance.transform.localRotation = navigatorOriginalLocalRotation;
            navigatorInstance.transform.localScale = navigatorOriginalLocalScale;
        }

        navigatorComponent = navigatorInstance.GetComponent<QuestNavigator>();
        if (navigatorComponent == null)
            navigatorComponent = navigatorInstance.AddComponent<QuestNavigator>();

        if (navigatorComponent != null)
            navigatorComponent.Setup(this, GameplayManager.Instance.MainHero.transform);
    }

    void SetNavigatorActive(bool active)
    {
        if (navigatorInstance == null)
            return;

        if (!active && navigatorOriginalTransformCached)
        {
            if (navigatorInstance.transform.parent != navigatorOriginalParent)
            {
                navigatorInstance.transform.SetParent(navigatorOriginalParent, worldPositionStays: false);
                navigatorInstance.transform.localPosition = NavigatorHiddenLocalPosition;
                navigatorInstance.transform.localRotation = navigatorOriginalLocalRotation;
                navigatorInstance.transform.localScale = navigatorOriginalLocalScale;
            }
        }

        if (navigatorInstance.activeSelf == active)
            return;

        navigatorInstance.SetActive(active);
    }

    void ShowNavigatorInputHandler(InputContext ctx)
    {
        if (ctx.BoolValue == false)
            return;

        if (!IsQuestActive || IsAllQuestCompleted || CurrentQuest == null)
            return;

        ShowNavigatorForSeconds(showNavigatorDuration).Forget();
    }

    async UniTask ShowNavigatorForSeconds(float seconds)
    {
        showNavigatorCts?.Cancel();
        showNavigatorCts?.Dispose();
        showNavigatorCts = new CancellationTokenSource();

        ShowNavigator();

        if (seconds <= 0f)
            return;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: showNavigatorCts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        HideNavigator();
    }

    
}

[Serializable]
public class QuestStepEntry
{
    public string questId;
    public string questDesc;
    public string sceneLocation;
    public Vector3 markerPosition;
    public float markerRadius = 2;

    public bool autoCompleteOnReachMarker = true;

    public int expReward;

    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label")]
    public List<QuestStepActionEntry> onEnterActions = new();

    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label")]
    public List<QuestStepActionEntry> onExitActions = new();

#if UNITY_EDITOR
    [TitleGroup("Helper")]
    [ShowInInspector]
    [OnValueChanged("OnHelperChanged")]
    Transform helperTransform;

    void OnHelperChanged()
    {
        sceneLocation = SceneManager.GetActiveScene().name;
        markerPosition = helperTransform.position;

        helperTransform = null;
    }
#endif
}
