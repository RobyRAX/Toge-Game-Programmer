using UnityEngine;

public class UnitRunState : UnitStateBase_Exploration
{
    public UnitRunState(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Run";

    public override void Enter()
    {
        PlayExplorationAnim(SM.AnimationClips?.Run);
    }

    public override void Update()
    {
        if (_movementCont != null)
            ApplyHorizontalVelocity(_movementCont.RunSpeed);

        EvaluateLocomotionTransitions();
    }

    public override void Exit()
    {
        StopHorizontalMovement();
    }
}
