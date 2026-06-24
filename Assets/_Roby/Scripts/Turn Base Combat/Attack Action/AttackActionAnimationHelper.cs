using RAXY.Animation;

public static class AttackActionAnimationHelper
{
    public const float DefaultFadeDuration = 0.2f;

    public static void TryPlay(
        CombatantBase owner,
        AttackActionParameterBase parameter,
        AnimationClipSet unitDefaultClip = null,
        float fadeDuration = DefaultFadeDuration)
    {
        if (owner == null || parameter == null || !parameter.playAnimation)
            return;

        AnimationClipSet clipSet = parameter.UseDefaultAnimation
            ? unitDefaultClip
            : parameter.animation;

        if (clipSet == null)
            return;

        var animancer = owner.GetComponent<CombatUnitController>()?.AnimancerCont;
        animancer?.PlayAnimation(clipSet, fadeDuration);
    }
}
