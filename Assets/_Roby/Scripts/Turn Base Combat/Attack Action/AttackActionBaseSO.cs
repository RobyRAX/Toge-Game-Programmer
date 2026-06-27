using System;
using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class AttackActionBaseSO : ScriptableObject
{
    public abstract string ActionName { get; }
    public abstract Type ParameterType { get; }
    public abstract AttackActionParameterBase ExampleParameter { get; }

    public abstract AttackActionBase_Runtime CreateRuntime(AttackActionEntry entry, CombatantBase combatantOwner);
}

public abstract class AttackActionParameterBase
{
    public bool playAnimation;

    public virtual bool UseDefaultAnimation => false;

    [ShowIf("@playAnimation && !UseDefaultAnimation")]
    public AnimationClipSet animation;
}