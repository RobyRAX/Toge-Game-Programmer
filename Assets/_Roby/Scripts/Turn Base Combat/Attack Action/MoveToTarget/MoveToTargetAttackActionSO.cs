using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveToTargetAttackActionSO", menuName = "RAXY/Turn Base/Attack Action/Move To Target")]
public class MoveToTargetAttackActionSO : AttackActionBaseSO
{
    public override string ActionName => "Move to Target";
    public override Type ParameterType => typeof(MoveToTargetAttackActionParameter);

    [HideLabel]
    [SerializeField]
    MoveToTargetAttackActionParameter exampleParameter;
    public override AttackActionParameterBase ExampleParameter => exampleParameter;
}

[Serializable]
public class MoveToTargetAttackActionParameter : AttackActionParameterBase
{
    [SuffixLabel("°")]
    [Range(0, 360)]
    [Tooltip("0 is target's front")]
    public float targetPositionAngle = 0;
    public float distanceFromTarget = 2;
    public bool useParabolicJump;

    [ShowIf("@useParabolicJump")]
    public float jumpHeight = 3;

    [PropertySpace(5, 0)]
    [SuffixLabel("seconds")]
    public float timeToReachTargetPosition = 1;

#if UNITY_EDITOR
    [Button]
    [ShowIf("@playAnimation")]
    void SyncWithAnimation()
    {
        var clip = animation?.AnimationClip_Editor;
        if (clip == null)
            return;

        timeToReachTargetPosition = clip.length / (animation.speed <= 0f ? 1f : animation.speed);
    }
#endif
}