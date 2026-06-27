using Cysharp.Threading.Tasks;
using RAXY.Animation;
using UnityEngine;

public static class AttackActionHelper
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

    public static async UniTask MoveTransformAsync(
        Transform transform,
        Vector3 destination,
        float duration,
        bool useParabolicJump,
        float jumpHeight,
        Quaternion? endRotation = null)
    {
        if (transform == null)
            return;

        if (duration <= 0f)
        {
            TurnBaseCombatHelper.TeleportTo(transform, destination, endRotation);
            return;
        }

        Vector3 start = transform.position;
        float startY = start.y;
        float destinationY = destination.y;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 position = Vector3.Lerp(start, destination, t);
            position.y = Mathf.Lerp(startY, destinationY, t);

            if (useParabolicJump)
                position.y += jumpHeight * 4f * t * (1f - t);

            TurnBaseCombatHelper.TeleportTo(transform, position);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        TurnBaseCombatHelper.TeleportTo(transform, destination, endRotation);
    }

    public static async UniTask RotateTransformAsync(
        Transform transform,
        Quaternion targetRotation,
        float duration)
    {
        if (transform == null)
            return;

        if (duration <= 0f)
        {
            TurnBaseCombatHelper.TeleportTo(transform, transform.position, targetRotation);
            return;
        }

        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            TurnBaseCombatHelper.TeleportTo(transform, transform.position, Quaternion.Slerp(startRotation, targetRotation, t));
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        TurnBaseCombatHelper.TeleportTo(transform, transform.position, targetRotation);
    }
}
