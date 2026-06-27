using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitAnimationClipsSO", menuName = "RAXY/Unit/Animation Clips SO")]
public class UnitAnimationClipsSO : ScriptableObject, ICombatAnimationClipsProvider
{
    [TitleGroup("Exploration")]
    public AnimationClipSet Idle_Exploration;

    [TitleGroup("Exploration")]
    public AnimationClipSet Run;

    [TitleGroup("Exploration")]
    public AnimationClipSet Sprint;

    [TitleGroup("Exploration")]
    public AnimationClipSet Attack_Exploration;

    [TitleGroup("Combat Default")]
    [SerializeField]
    AnimationClipSet combatIdle;

    [TitleGroup("Combat Default")]
    [SerializeField]
    AnimationClipSet combatReady;

    [TitleGroup("Combat Default")]
    [SerializeField]
    AnimationClipSet moveToTarget;

    [TitleGroup("Combat Default")]
    [SerializeField]
    AnimationClipSet backToFormation;

    [TitleGroup("Damaged")]
    [SerializeField]
    AnimationClipSet lightHit;

    [TitleGroup("Damaged")]
    [SerializeField]
    AnimationClipSet mediumHit;

    [TitleGroup("Damaged")]
    [SerializeField]
    AnimationClipSet heavyHit;

    [TitleGroup("Damaged")]
    [SerializeField]
    AnimationClipSet stun;

    [TitleGroup("Damaged")]
    [SerializeField]
    AnimationClipSet die;

    public AnimationClipSet CombatIdle => combatIdle;
    public AnimationClipSet Ready => combatReady;
    public AnimationClipSet MoveToTarget => moveToTarget;
    public AnimationClipSet BackToFormation => backToFormation;
    public AnimationClipSet LightHit => lightHit;
    public AnimationClipSet MediumHit => mediumHit;
    public AnimationClipSet HeavyHit => heavyHit;
    public AnimationClipSet Die => die;
    public AnimationClipSet Stun => stun;
}
