using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class AttackActionEntry
{
    public string Label => AttackActionSO?.ActionName ?? "";

    [OnValueChanged("OnAttackActionChanged")]
    [InlineButton("OnAttackActionChanged", "Sync")]
    public AttackActionBaseSO AttackActionSO;

    [TitleGroup("Parameter")]
    [SerializeReference]
    [HideReferenceObjectPicker]
    public AttackActionParameterBase AttackActionParameter; 

    void OnAttackActionChanged()
    {
        if (AttackActionSO == null)
            return;

        var currentType = AttackActionParameter != null ? AttackActionParameter.GetType() : null;
        var targetType = AttackActionSO.ParameterType;

        if (currentType == targetType)
            return;

        if (AttackActionParameter == null || AttackActionParameter.GetType() != targetType)
        {
            AttackActionParameter = (AttackActionParameterBase)Activator.CreateInstance(targetType);
        }
    }
}