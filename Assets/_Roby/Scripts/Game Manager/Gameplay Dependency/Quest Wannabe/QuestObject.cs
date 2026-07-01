using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum QuestObjectDefaultState
{
    Enable,
    Disable
}

public class QuestObject : MonoBehaviour
{
    [SerializeField]
    QuestObjectDefaultState defaultState = QuestObjectDefaultState.Disable;

    [SerializeField]
    List<string> questIds = new();

    [SerializeField]
    List<GameObject> targets = new();

    CancellationTokenSource enableCts;

    void OnEnable()
    {
        enableCts = new CancellationTokenSource();
        SubscribeWhenReadyAsync(enableCts.Token).Forget();
    }

    void OnDestroy()
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
        var shouldBeActive = ShouldBeActive(currentQuest);
        var resolvedTargets = GetResolvedTargets();

        foreach (var target in resolvedTargets)
        {
            if (target != null)
                target.SetActive(shouldBeActive);
        }
    }

    bool ShouldBeActive(QuestStepEntry currentQuest)
    {
        var active = defaultState == QuestObjectDefaultState.Enable;

        if (currentQuest != null && questIds != null && questIds.Contains(currentQuest.questId))
            active = !active;

        return active;
    }

    List<GameObject> GetResolvedTargets()
    {
        if (targets != null && targets.Count > 0)
            return targets;

        return new List<GameObject> { gameObject };
    }
}
