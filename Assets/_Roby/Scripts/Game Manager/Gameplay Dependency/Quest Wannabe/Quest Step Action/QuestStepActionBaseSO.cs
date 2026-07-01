using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class QuestStepActionBaseSO : ScriptableObject
{
    public abstract string ActionName { get; }
    public abstract Type ParameterType { get; }
    public abstract QuestStepActionParameterBase ExampleParameter { get; }

    public virtual void Execute(QuestStepActionContext ctx, QuestStepActionParameterBase param) { }

    public virtual UniTask ExecuteAsync(QuestStepActionContext ctx, QuestStepActionParameterBase param)
    {
        Execute(ctx, param);
        return UniTask.CompletedTask;
    }
}

public abstract class QuestStepActionParameterBase { }
