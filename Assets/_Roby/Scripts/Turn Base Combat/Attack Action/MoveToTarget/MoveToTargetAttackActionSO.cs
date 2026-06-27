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

    public override AttackActionBase_Runtime CreateRuntime(AttackActionEntry entry, CombatantBase combatantOwner)
        => new MoveToTargetAttackAction_Runtime(entry, combatantOwner);
}

[Serializable]
public class MoveToTargetAttackActionParameter : AttackActionParameterBase
{
    [ShowIf("@playAnimation")]
    public bool useDefaultAnimation;

    public override bool UseDefaultAnimation => useDefaultAnimation;

    [SuffixLabel("°")]
    [Range(0, 360)]
    [Tooltip("0 is target's front")]
    public float targetPositionAngle = 0;
    public float distanceFromTarget = 2;
    public bool useParabolicJump;

    [ShowIf("@useParabolicJump")]
    public float jumpHeight = 3;

    [SuffixLabel("seconds")]
    public float timeToTurnTowardMovement = 0.25f;

    [SuffixLabel("seconds")]
    public float timeToFaceTarget = 0.25f;

    [PropertySpace(5, 0)]
    [Tooltip("x = waktu mulai gerak, y = waktu sampai target (detik). Durasi = y - x.")]
    public Vector2 timeToReachTargetPosition = new Vector2(0f, 1f);

#if UNITY_EDITOR
    [Button]
    [ShowIf("@playAnimation && !useDefaultAnimation")]
    void SyncWithAnimation()
    {
        var clip = animation?.AnimationClip_Editor;
        if (clip == null)
            return;

        timeToReachTargetPosition.y = clip.length / (animation.speed <= 0f ? 1f : animation.speed);
    }
#endif
}