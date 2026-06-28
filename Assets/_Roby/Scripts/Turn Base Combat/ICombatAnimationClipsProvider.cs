using RAXY.Animation;
using UnityEngine;

public interface ICombatAnimationClipsProvider
{
    public AnimationClipSet CombatIdle { get; }
    public AnimationClipSet Ready { get; }
    public AnimationClipSet MoveToTarget { get; }
    public AnimationClipSet BackToFormation { get; }
    public AnimationClipSet LightHit { get; }
    public AnimationClipSet MediumHit { get; }
    public AnimationClipSet HeavyHit { get; }
    public AnimationClipSet Die { get; }
    public AnimationClipSet Stun { get; }
    public AnimationClipSet Win { get; }
}
