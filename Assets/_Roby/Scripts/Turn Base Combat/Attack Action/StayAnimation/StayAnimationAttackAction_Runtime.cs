using Cysharp.Threading.Tasks;

public class StayAnimationAttackAction_Runtime : AttackActionBase_Runtime
{
    public StayAnimationAttackAction_Runtime(AttackActionEntry entry, CombatantBase combatantOwner) : base(entry, combatantOwner)
    {
    }

    public override async UniTask ExecuteAsync(CombatantBase targetOpponent, CombatantBase targetTeam)
    {
        var parameter = GetParameter<StayAnimationAttackActionParameter>();
        if (parameter == null)
            return;

        AttackActionAnimationHelper.TryPlay(CombatantOwner, parameter);

        if (parameter.stayDuration <= 0f)
            return;

        int delayMs = (int)(parameter.stayDuration * 1000f);
        await UniTask.Delay(delayMs);
    }
}
