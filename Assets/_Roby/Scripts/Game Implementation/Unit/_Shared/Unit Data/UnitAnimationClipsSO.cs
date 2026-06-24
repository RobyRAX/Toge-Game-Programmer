using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitAnimationClipsSO", menuName = "RAXY/Unit/Animation Clips SO")]
public class UnitAnimationClipsSO : ScriptableObject
{
    [TitleGroup("Exploration")]
    public AnimationClipSet Idle_Exploration;

    [TitleGroup("Exploration")]
    public AnimationClipSet Run;

    [TitleGroup("Exploration")]
    public AnimationClipSet Sprint;

    [TitleGroup("Exploration")]
    public AnimationClipSet Attack_Exploration;

    [TitleGroup("Damaged")]
    public AnimationClipSet LightHit;

    [TitleGroup("Damaged")]
    public AnimationClipSet MediumHit;

    [TitleGroup("Damaged")]
    public AnimationClipSet HeavyHit;

    [TitleGroup("Damaged")]
    public AnimationClipSet Die;

    [TitleGroup("Stun")]
    public AnimationClipSet EnterStun;

    [TitleGroup("Stun")]
    public AnimationClipSet Stun;

    [TitleGroup("Stun")]
    public AnimationClipSet ExitStun;
}
