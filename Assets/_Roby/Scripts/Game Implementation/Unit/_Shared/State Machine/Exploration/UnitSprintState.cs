using UnityEngine;

public class UnitSprintState : UnitStateBase_Exploration
{
    public UnitSprintState(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Sprint";

    public override void Enter()
    {
        PlayExplorationAnim(SM.AnimationClips?.Sprint);
    }

    public override void Update()
    {
        if (_movementCont != null)
            ApplyHorizontalVelocity(_movementCont.SprintSpeed);

        EvaluateLocomotionTransitions();
    }

    public override void Exit()
    {
        StopHorizontalMovement();
    }
}
