using System;
using Cysharp.Threading.Tasks;
using RAXY.Dialogue;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ListenToDialogueEndQuestStepActionSO", menuName = "RAXY/Quest/Action/Listen To Dialogue End")]
public class ListenToDialogueEndQuestStepActionSO : QuestStepActionBaseSO
{
    public override Type ParameterType => typeof(ListenToDialogueEndQuestStepActionParameter);

    [HideLabel]
    [SerializeField]
    ListenToDialogueEndQuestStepActionParameter exampleParameter;
    public override QuestStepActionParameterBase ExampleParameter => exampleParameter;

    public override string ActionName => "Listen To Dialogue End";

    public override UniTask ExecuteAsync(QuestStepActionContext ctx, QuestStepActionParameterBase param)
    {
        var p = param as ListenToDialogueEndQuestStepActionParameter;
        if (p == null || ctx?.Manager == null || ctx.Step == null)
            return UniTask.CompletedTask;

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[ListenToDialogueEndQuestStepActionSO] DialogueManager.Instance is null.");
            return UniTask.CompletedTask;
        }

        if (p.listenForDialogue == null)
        {
            Debug.LogWarning(
                "[ListenToDialogueEndQuestStepActionSO] listenForDialogue is null. Assign DialogueSO supaya step hanya complete saat dialogue yang benar selesai.");
        }

        ctx.Manager.SetRuntimeAutoComplete(false);

        if (p.playDialogueOnEnter && p.listenForDialogue != null)
            DialogueManager.Instance.PlayDialogueSO(p.listenForDialogue);

        var stepId = ctx.Step.questId;
        var listenForDialogue = p.listenForDialogue;
        var dialogueManager = DialogueManager.Instance;

        void OnDialogueEndHandler()
        {
            if (ctx.Manager?.CurrentQuest?.questId != stepId)
            {
                dialogueManager.OnDialogueEnded -= OnDialogueEndHandler;
                return;
            }

            if (listenForDialogue != null && !IsDialogueMatch(listenForDialogue))
                return;

            dialogueManager.OnDialogueEnded -= OnDialogueEndHandler;

            if (p.completeStepOnDialogueEnd)
                ctx.Manager.CompleteCurrentStep();
        }

        dialogueManager.OnDialogueEnded -= OnDialogueEndHandler;
        dialogueManager.OnDialogueEnded += OnDialogueEndHandler;

        return UniTask.CompletedTask;
    }

    static bool IsDialogueMatch(DialogueSO expected)
    {
        if (expected == null || DialogueManager.Instance?.DialogueControllerDict == null)
            return false;

        foreach (var controller in DialogueManager.Instance.DialogueControllerDict.Values)
        {
            if (controller?.currentDialogueSO == expected)
                return true;
        }

        return false;
    }
}

[Serializable]
public class ListenToDialogueEndQuestStepActionParameter : QuestStepActionParameterBase
{
    [TitleGroup("Listen")]
    [InfoBox("DialogueSO yang ditunggu selesainya. Saat DialogueManager.OnDialogueEnded terpanggil, dicek currentDialogueSO di DialogueManager.")]
    public DialogueSO listenForDialogue;

    [TitleGroup("Optional Play")]
    public bool playDialogueOnEnter;

    [TitleGroup("Complete")]
    public bool completeStepOnDialogueEnd = true;
}
