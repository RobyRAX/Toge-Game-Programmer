using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public abstract class AttackActionBase_Runtime
{
    public CombatantBase CombatantOwner { get; set; }

    [HideReferenceObjectPicker]
    public AttackActionEntry entry;

    [TitleGroup("Status")]
    [ShowInInspector]
    public bool IsRunning { get; set; }

    [TitleGroup("Status")]
    [ShowInInspector]
    public bool IsCompleted { get; set; }

    public AttackActionBase_Runtime(AttackActionEntry entry, CombatantBase combatantOwner)
    {
        this.entry = entry;
        CombatantOwner = combatantOwner;
    }

    public async UniTask Start(CombatantBase targetOpponent, CombatantBase targetTeam)
    {
        if (IsRunning)
            return;

        IsRunning = true;
        IsCompleted = false;

        await ExecuteAsync(targetOpponent, targetTeam);

        IsRunning = false;
        IsCompleted = true;
    }

    protected abstract UniTask ExecuteAsync(CombatantBase targetOpponent, CombatantBase targetTeam);

    protected T GetParameter<T>() where T : AttackActionParameterBase
        => entry?.AttackActionParameter as T;
}
