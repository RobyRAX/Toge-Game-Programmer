using UnityEngine;

public class CombatUnitController : UnitControllerBase
{
    [SerializeField]
    UnitAnimationClipsSO animationClips;

    public override UnitAnimationClipsSO AnimationClips => animationClips;
}
