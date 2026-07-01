using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class QuestStepActionEntry
{
    public string Label => ActionSO?.ActionName ?? "";

    [OnValueChanged("OnActionChanged")]
    [InlineButton("OnActionChanged", "Sync")]
    public QuestStepActionBaseSO ActionSO;

    [TitleGroup("Parameter")]
    [SerializeReference]
    [HideReferenceObjectPicker]
    public QuestStepActionParameterBase Parameter;

    void OnActionChanged()
    {
        if (ActionSO == null)
            return;

        var currentType = Parameter != null ? Parameter.GetType() : null;
        var targetType = ActionSO.ParameterType;

        if (currentType == targetType)
            return;

        if (Parameter == null || Parameter.GetType() != targetType)
            Parameter = (QuestStepActionParameterBase)Activator.CreateInstance(targetType);
    }
}
