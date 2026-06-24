using Cysharp.Threading.Tasks;
using UnityEngine;

public static class AttackActionMovementHelper
{
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
}
