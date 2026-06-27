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

    [Tooltip("Putar menghadap arah gerak sebelum move, lalu putar balik ke facing formasi setelah sampai. Untuk anim jalan/lari maju.")]
    public bool useDirectionTurn;

    [ShowIf("@useDirectionTurn")]
    [SuffixLabel("seconds")]
    public float timeToTurnTowardMovement = 0.25f;

    [Tooltip("Waktu putar halus ke facing formasi setelah sampai di slot. Berlaku untuk mode mundur maupun arah gerak.")]
    [SuffixLabel("seconds")]
    public float timeToRestoreFormationFacing = 0.25f;

    public bool useParabolicJump;

    [ShowIf("@useParabolicJump")]
    public float jumpHeight = 3;

    [PropertySpace(5, 0)]
    [Tooltip("x = waktu mulai gerak, y = waktu sampai target (detik). Durasi = y - x.")]
    public Vector2 timeToReachFormationPosition = new Vector2(0f, 1f);

    public override float MaxTime => (useDirectionTurn ? timeToTurnTowardMovement : 0f) + timeToReachFormationPosition.y + timeToRestoreFormationFacing;

#if UNITY_EDITOR
    [Button]
    [ShowIf("@playAnimation && !useDefaultAnimation")]
    void SyncWithAnimation()
    {
        var clip = animation?.AnimationClip_Editor;
        if (clip == null)
            return;

        timeToReachFormationPosition.y = clip.length / (animation.speed <= 0f ? 1f : animation.speed);
    }
#endif
}
