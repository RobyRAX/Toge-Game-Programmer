using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RAXY.Core;
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

    public QuestMarker questMarker;

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

    public event Action<QuestStepEntry> OnQuestStepChanged;

    protected override void Awake()
    {
        base.Awake();

        actionContext = new QuestStepActionContext { Manager = this };
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedHandler;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedHandler;
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
    }

    public void StartQuest(int index = 0)
    {
        if (questStepEntries == null || questStepEntries.Count == 0)
            return;

        CurrentStepIndex = Mathf.Clamp(index, 0, questStepEntries.Count - 1);
        IsQuestActive = true;
        IsAllQuestCompleted = false;
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

            CurrentStepIndex++;

            if (CurrentStepIndex >= questStepEntries.Count)
            {
                IsAllQuestCompleted = true;
                IsQuestActive = false;
                CurrentQuest = null;
                questMarker?.Hide();
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
            return;
        }

        questMarker.SetMarker(GetResolvedMarkerPosition(), CurrentQuest.markerRadius);
    }

    async UniTask EnterStepAsync()
    {
        CurrentQuest = questStepEntries[CurrentStepIndex];
        RuntimeAutoComplete = CurrentQuest.autoCompleteOnReachMarker;

        await RunActions(CurrentQuest.onEnterActions, CurrentQuest);
        RefreshMarker();
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
