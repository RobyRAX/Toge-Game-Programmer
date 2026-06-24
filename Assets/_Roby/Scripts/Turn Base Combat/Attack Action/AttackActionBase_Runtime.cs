using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public abstract class AttackActionBase_Runtime
{
    public CombatantBase CombatantOwner { get; set; }

    [HideReferenceObjectPicker]
    public AttackActionEntry entry;

    public bool IsRunning { get; set; }
    public bool IsCompleted { get; set; }

    public AttackActionBase_Runtime(AttackActionEntry entry, CombatantBase combatantOwner)
    {
        this.entry = entry;
        CombatantOwner = combatantOwner;
    }

    public virtual void Start()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        IsCompleted = false;
    }

    public virtual void SetAsCompleted()
    {
        IsRunning = false;
        IsCompleted = true;
    }

    public abstract UniTask ExecuteAsync(CombatantBase targetOpponent, CombatantBase targetTeam);

    protected T GetParameter<T>() where T : AttackActionParameterBase
        => entry?.AttackActionParameter as T;
}