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
            await AttackActionHelper.RotateTransformAsync(
                ownerTransform,
                towardSlot,
                parameter.timeToTurnTowardMovement);
        }

        var unitClips = CombatantOwner.AnimationClips;
        AttackActionHelper.TryPlay(CombatantOwner, parameter, unitClips?.BackToFormation);

        float startMove = parameter.timeToReachFormationPosition.x;
        float reachMove = parameter.timeToReachFormationPosition.y;
        if (startMove > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(startMove));

        await AttackActionHelper.MoveTransformAsync(
            ownerTransform,
            slot.position,
            Mathf.Max(0f, reachMove - startMove),
            parameter.useParabolicJump,
            parameter.jumpHeight,
            null);

        await AttackActionHelper.RotateTransformAsync(
            ownerTransform,
            slot.rotation,
            parameter.timeToRestoreFormationFacing);
    }
}
