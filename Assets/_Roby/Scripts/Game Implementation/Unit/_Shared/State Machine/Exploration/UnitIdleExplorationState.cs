using UnityEngine;

public class UnitIdleExplorationState : UnitStateBase_Exploration
{
    public UnitIdleExplorationState(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Idle_Exploration";

    public override void Enter()
    {
        StopHorizontalMovement();
        PlayExplorationAnim(SM.AnimationClips?.Idle_Exploration);
    }

    public override void Update()
    {
        EvaluateLocomotionTransitions();
    }
}
