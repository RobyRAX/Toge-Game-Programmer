public abstract class AttackActionBase_Runtime
{
    public CombatantBase CombatantOwner { get; set; }
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
}