using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveToTargetAttackAction_Runtime : AttackActionBase_Runtime
{
    public MoveToTargetAttackAction_Runtime(AttackActionEntry entry, CombatantBase combatantOwner) : base(entry, combatantOwner)
    {
    }

    public override async UniTask ExecuteAsync(CombatantBase targetOpponent, CombatantBase targetTeam)
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
            await AttackActionMovementHelper.RotateTransformAsync(
                ownerTransform,
                towardDestination,
                parameter.timeToTurnTowardMovement);
        }

        var unitClips = CombatantOwner.GetComponent<CombatUnitController>()?.AnimationClips;
        AttackActionAnimationHelper.TryPlay(CombatantOwner, parameter, unitClips?.MoveToTarget);

        await AttackActionMovementHelper.MoveTransformAsync(
            ownerTransform,
            destination,
            parameter.timeToReachTargetPosition,
            parameter.useParabolicJump,
            parameter.jumpHeight,
            null);

        var faceTarget = ResolveFacing(targetOpponent, destination);
        if (faceTarget.HasValue)
        {
            await AttackActionMovementHelper.RotateTransformAsync(
                ownerTransform,
                faceTarget.Value,
                parameter.timeToFaceTarget);
        }
    }

    static Vector3 ResolveDestination(CombatantBase targetOpponent, MoveToTargetAttackActionParameter parameter, Vector3 fallbackPosition)
    {
        if (targetOpponent == null)
            return fallbackPosition;

        Transform targetTransform = targetOpponent.transform;
        Vector3 offset = Quaternion.Euler(0f, parameter.targetPositionAngle, 0f) * Vector3.forward * parameter.distanceFromTarget;
        return targetTransform.position + targetTransform.rotation * offset;
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
