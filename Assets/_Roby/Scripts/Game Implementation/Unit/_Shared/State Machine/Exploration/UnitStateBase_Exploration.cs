using UnityEngine;

public abstract class UnitStateBase_Exploration : UnitStateBase
{
    public new UnitStateMachine_Exploration SM { get; set; }
    protected UnitMovement _movementCont;

    public UnitStateBase_Exploration(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
        SM = stateMachine;
        _movementCont = SM.MovementCont;
    }
}
