using RAXY.StateMachine;
using UnityEngine;

public abstract class UnitStateBase : State
{
    public UnitStateBase(UnitStateMachineBase stateMachine) : base(stateMachine)
    {
    }
}
