using UnityEngine;

public class UnitSprintState : UnitStateBase_Exploration
{
    public UnitSprintState(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Sprint";
}
