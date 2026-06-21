using UnityEngine;

public class UnitRunState : UnitStateBase_Exploration
{
    public UnitRunState(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Run";
}
