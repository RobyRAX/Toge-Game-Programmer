using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ReturnToFormationAttackActionSO", menuName = "RAXY/Turn Base/Attack Action/Return To Formation")]
public class ReturnToFormationAttackActionSO : AttackActionBaseSO
{
    public override string ActionName => "Return To Formation";
    public override Type ParameterType => typeof(ReturnToFormationAttackActionParameter);

    [HideLabel]
    [SerializeField]
    ReturnToFormationAttackActionParameter exampleParameter;
    public override AttackActionParameterBase ExampleParameter => exampleParameter;

    public override AttackActionBase_Runtime CreateRuntime(AttackActionEntry entry, CombatantBase combatantOwner)
        => new ReturnToFormationAttackAction_Runtime(entry, combatantOwner);
}

[Serializable]
public class ReturnToFormationAttackActionParameter : AttackActionParameterBase
{
    [ShowIf("@playAnimation")]
    public bool useDefaultAnimation;

    public override bool UseDefaultAnimation => useDefaultAnimation;

    public bool useParabolicJump;

    [ShowIf("@useParabolicJump")]
    public float jumpHeight = 3;

    [PropertySpace(5, 0)]
    [SuffixLabel("seconds")]
    public float timeToReachFormationPosition = 1;

#if UNITY_EDITOR
    [Button]
    [ShowIf("@playAnimation && !useDefaultAnimation")]
    void SyncWithAnimation()
    {
        var clip = animation?.AnimationClip_Editor;
        if (clip == null)
            return;

        timeToReachFormationPosition = clip.length / (animation.speed <= 0f ? 1f : animation.speed);
    }
#endif
}
