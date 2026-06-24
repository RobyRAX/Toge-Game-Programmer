using Cysharp.Threading.Tasks;
using UnityEngine;

public class ReturnToFormationAttackAction_Runtime : AttackActionBase_Runtime
{
    public ReturnToFormationAttackAction_Runtime(AttackActionEntry entry, CombatantBase combatantOwner) : base(entry, combatantOwner)
    {
    }

    public override async UniTask ExecuteAsync(CombatantBase targetOpponent, CombatantBase targetTeam)
    {
        var parameter = GetParameter<ReturnToFormationAttackActionParameter>();
        if (parameter == null || CombatantOwner == null)
            return;

        var slot = CombatantOwner.FormationSlot;
        if (slot == null)
            return;

        var ownerTransform = CombatantOwner.transform;
        if (ownerTransform == null)
            return;

        if (parameter.useDirectionTurn &&
            TurnBaseCombatHelper.TryGetFlatFacingRotation(ownerTransform.position, slot.position, out Quaternion towardSlot))
        {
            await AttackActionMovementHelper.RotateTransformAsync(
                ownerTransform,
                towardSlot,
                parameter.timeToTurnTowardMovement);
        }

        var unitClips = CombatantOwner.GetComponent<CombatUnitController>()?.AnimationClips;
        AttackActionAnimationHelper.TryPlay(CombatantOwner, parameter, unitClips?.BackToFormation);

        Quaternion? endRotation = parameter.useDirectionTurn ? null : slot.rotation;

        await AttackActionMovementHelper.MoveTransformAsync(
            ownerTransform,
            slot.position,
            parameter.timeToReachFormationPosition,
            parameter.useParabolicJump,
            parameter.jumpHeight,
            endRotation);

        if (parameter.useDirectionTurn)
        {
            await AttackActionMovementHelper.RotateTransformAsync(
                ownerTransform,
                slot.rotation,
                parameter.timeToRestoreFormationFacing);
        }
    }
}
