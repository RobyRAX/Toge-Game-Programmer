using RAXY.Animation;
using UnityEngine;

public abstract class UnitStateBase_Exploration : UnitStateBase
{
    protected const float MoveInputDeadzoneSqr = 0.01f;
    protected const float AnimationFadeDuration = 0.2f;

    public new UnitStateMachine_Exploration SM { get; set; }
    protected UnitMovement _movementCont;

    protected BrainExplorationBase Brain => SM.Brain;

    protected bool HasMoveInput =>
        Brain != null && Brain.Move.sqrMagnitude > MoveInputDeadzoneSqr;

    public UnitStateBase_Exploration(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
        SM = stateMachine;
        _movementCont = SM.MovementCont;
    }

    protected Vector3 GetMoveDirection()
    {
        if (Brain == null)
            return Vector3.zero;

        Vector2 move = Brain.Move;
        if (move.sqrMagnitude <= MoveInputDeadzoneSqr)
            return Vector3.zero;

        return new Vector3(move.x, 0f, move.y).normalized;
    }

    protected void ApplyHorizontalVelocity(float speed)
    {
        if (_movementCont == null)
            return;

        Vector3 direction = GetMoveDirection();
        if (direction.sqrMagnitude < 0.001f)
        {
            StopHorizontalMovement();
            return;
        }

        _movementCont.Set_HorizontalVelocity(direction * speed);
    }

    protected void StopHorizontalMovement()
    {
        _movementCont?.Set_HorizontalVelocity(Vector3.zero);
    }

    protected void PlayExplorationAnim(AnimationClipSet clipSet)
    {
        if (clipSet == null || SM.AnimancerCont == null)
            return;

        SM.AnimancerCont.PlayAnimation(clipSet, AnimationFadeDuration);
    }

    protected void EvaluateLocomotionTransitions()
    {
        if (Brain == null)
            return;

        if (!HasMoveInput)
        {
            if (SM.CurrentState != SM.Idle)
                SM.ChangeState(SM.Idle);

            return;
        }

        if (Brain.Sprint)
        {
            if (SM.CurrentState != SM.Sprint)
                SM.ChangeState(SM.Sprint);

            return;
        }

        if (SM.CurrentState != SM.Run)
            SM.ChangeState(SM.Run);
    }
}
