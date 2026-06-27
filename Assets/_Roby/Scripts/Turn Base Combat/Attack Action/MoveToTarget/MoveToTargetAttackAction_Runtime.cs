using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveToTargetAttackAction_Runtime : AttackActionBase_Runtime
{
    public MoveToTargetAttackAction_Runtime(AttackActionEntry entry, CombatantBase combatantOwner) : base(entry, combatantOwner)
    {
    }

    protected override async UniTask ExecuteAsync(CombatantBase targetOpponent, CombatantBase targetTeam)
    {
        var parameter = GetParameter<MoveToTargetAttackActionParameter>();
        if (parameter == null || CombatantOwner == null)
            return;

        var ownerTransform = CombatantOwner.transform;
        if (ownerTransform == null)
            return;

        Vector3 destination = ResolveDestination(targetOpponent, parameter, ownerTransform.position);

        if (TurnBaseCombatHelper.TryGetFlatFacingRotation(
                ownerTransform.position, destination, out Quaternion towardDestination))
        {
            await AttackActionHelper.RotateTransformAsync(
                ownerTransform,
                towardDestination,
                parameter.timeToTurnTowardMovement);
        }

        var unitClips = CombatantOwner.AnimationClips;
        AttackActionHelper.TryPlay(CombatantOwner, parameter, unitClips?.MoveToTarget);

        float startMove = parameter.timeToReachTargetPosition.x;
        float reachMove = parameter.timeToReachTargetPosition.y;
        if (startMove > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(startMove));

        await AttackActionHelper.MoveTransformAsync(
            ownerTransform,
            destination,
            Mathf.Max(0f, reachMove - startMove),
            parameter.useParabolicJump,
            parameter.jumpHeight,
            null);

        if (parameter.moveBehindTarget)
        {
            var faceTarget = ResolveFacing(targetOpponent, destination);
            if (faceTarget.HasValue)
            {
                await AttackActionHelper.RotateTransformAsync(
                    ownerTransform,
                    faceTarget.Value,
                    parameter.timeToFaceTarget);
            }
        }
    }

    static Vector3 ResolveDestination(CombatantBase targetOpponent, MoveToTargetAttackActionParameter parameter, Vector3 ownerPosition)
    {
        if (targetOpponent == null)
            return ownerPosition;

        Transform targetTransform = targetOpponent.transform;
        Vector3 targetPosition = targetTransform.position;

        if (parameter.moveBehindTarget)
        {
            Vector3 flatBack = targetTransform.forward;
            flatBack.y = 0f;
            if (flatBack.sqrMagnitude < 0.0001f)
                return targetPosition;

            return targetPosition - flatBack.normalized * parameter.distanceFromTarget;
        }

        Vector3 flatDir = targetPosition - ownerPosition;
        flatDir.y = 0f;
        if (flatDir.sqrMagnitude < 0.0001f)
            return targetPosition;

        return targetPosition - flatDir.normalized * parameter.distanceFromTarget;
    }

    static Quaternion? ResolveFacing(CombatantBase targetOpponent, Vector3 fromPosition)
    {
        if (targetOpponent == null)
            return null;

        if (!TurnBaseCombatHelper.TryGetFlatFacingRotation(fromPosition, targetOpponent.transform.position, out Quaternion rotation))
            return null;

        return rotation;
    }
}
