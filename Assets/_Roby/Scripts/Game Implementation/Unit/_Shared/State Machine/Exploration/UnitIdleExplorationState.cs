using UnityEngine;

public class UnitIdleExplorationState : UnitStateBase_Exploration
{
    public UnitIdleExplorationState(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Idle_Exploration";
}
