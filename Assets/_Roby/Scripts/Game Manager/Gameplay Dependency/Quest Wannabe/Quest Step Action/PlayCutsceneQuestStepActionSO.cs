using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayCutsceneQuestStepActionSO", menuName = "RAXY/Quest/Action/Play Cutscene")]
public class PlayCutsceneQuestStepActionSO : QuestStepActionBaseSO
{
    public override Type ParameterType => typeof(PlayCutsceneQuestStepActionParameter);

    [HideLabel]
    [SerializeField]
    PlayCutsceneQuestStepActionParameter exampleParameter;
    public override QuestStepActionParameterBase ExampleParameter => exampleParameter;

    public override string ActionName => "Play Cutscene";

    public override async UniTask ExecuteAsync(QuestStepActionContext ctx, QuestStepActionParameterBase param)
    {
        var p = param as PlayCutsceneQuestStepActionParameter;
        if (p?.cutscenePrefab == null || ctx?.Manager == null)
            return;

        ctx.Manager.SetRuntimeAutoComplete(false);

        if (CutsceneManager.Instance == null)
        {
            Debug.LogWarning("[PlayCutsceneQuestStepActionSO] CutsceneManager.Instance is null.");
            return;
        }

        var tcs = new UniTaskCompletionSource();

        void OnCutsceneEndedHandler()
        {
            CutsceneManager.Instance.OnCutsceneEnded -= OnCutsceneEndedHandler;
            tcs.TrySetResult();
        }

        CutsceneManager.Instance.OnCutsceneEnded += OnCutsceneEndedHandler;
        CutsceneManager.Instance.PlayCutscene(p.cutscenePrefab);

        await tcs.Task;

        if (p.completeStepOnCutsceneEnd)
            ctx.Manager.CompleteCurrentStep();
    }
}

[Serializable]
public class PlayCutsceneQuestStepActionParameter : QuestStepActionParameterBase
{
    public Cutscene cutscenePrefab;
    public bool completeStepOnCutsceneEnd = true;
}
