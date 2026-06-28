public class UnitStateMachine_Exploration : UnitStateMachineBase
{
    public UnitIdleExplorationState Idle { get; set; }
    public UnitRunState Run { get; set; }
    public UnitSprintState Sprint { get; set; }
    public UnitAttackExplorationState Attack { get; set; }

    public BrainExplorationBase Brain { get; set; }

    public UnitStateMachine_Exploration(UnitControllerBase ownerCont) : base(ownerCont)
    {
        Idle = new UnitIdleExplorationState(this);
        Run = new UnitRunState(this);
        Sprint = new UnitSprintState(this);
        Attack = new UnitAttackExplorationState(this);

        Brain = ownerCont.Brain_Exploration;

        ChangeState(Idle);
    }

    public void ChangeState_Idle()
    {
        ChangeState(Idle);
    }
}
