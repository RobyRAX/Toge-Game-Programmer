using System;
using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "StayAnimationAttackActionSO", menuName = "RAXY/Turn Base/Attack Action/Stay Animation")]
public class StayAnimationAttackActionSO : AttackActionBaseSO
{
    public override Type ParameterType => typeof(StayAnimationAttackActionParameter);

    [HideLabel]
    [SerializeField]
    StayAnimationAttackActionParameter exampleParameter;
    public override AttackActionParameterBase ExampleParameter => exampleParameter;

    public override string ActionName => "Stay Animation";
}

[Serializable]
public class StayAnimationAttackActionParameter : AttackActionParameterBase
{
    [SuffixLabel("seconds")]
    public float stayDuration = 1;

#if UNITY_EDITOR
    [Button]
    [ShowIf("@playAnimation")]
    void SyncWithAnimation()
    {
        var clip = animation?.AnimationClip_Editor;
        if (clip == null)
            return;

        stayDuration = clip.length / (animation.speed <= 0f ? 1f : animation.speed);
    }
#endif
}