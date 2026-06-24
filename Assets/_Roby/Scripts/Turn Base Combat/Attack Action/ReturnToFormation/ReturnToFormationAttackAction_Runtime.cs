using Cysharp.Threading.Tasks;

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

        var unitClips = CombatantOwner.GetComponent<CombatUnitController>()?.AnimationClips;
        AttackActionAnimationHelper.TryPlay(CombatantOwner, parameter, unitClips?.BackToFormation);

        await AttackActionMovementHelper.MoveTransformAsync(
            ownerTransform,
            slot.position,
            parameter.timeToReachFormationPosition,
            parameter.useParabolicJump,
            parameter.jumpHeight,
            slot.rotation);
    }
}
