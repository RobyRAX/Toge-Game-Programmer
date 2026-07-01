using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI questDescTmp;

    CancellationTokenSource enableCts;

    void OnEnable()
    {
        enableCts = new CancellationTokenSource();
        SubscribeWhenReadyAsync(enableCts.Token).Forget();
    }

    void OnDisable()
    {
        enableCts?.Cancel();
        enableCts?.Dispose();
        enableCts = null;

        if (QuestWannabe.Instance != null)
            QuestWannabe.Instance.OnQuestStepChanged -= Refresh;
    }

    async UniTask SubscribeWhenReadyAsync(CancellationToken ct)
    {
        await UniTask.WaitUntil(() => QuestWannabe.Instance != null, cancellationToken: ct);

        QuestWannabe.Instance.OnQuestStepChanged += Refresh;
        Refresh(QuestWannabe.Instance.CurrentQuest);
    }

    void Refresh(QuestStepEntry currentQuest)
    {
        if (questDescTmp == null)
            return;

        var hasQuest = currentQuest != null;
        questDescTmp.text = hasQuest ? currentQuest.questDesc : string.Empty;
        questDescTmp.gameObject.SetActive(hasQuest);
    }
}
