using RAXY.Animation;
using UnityEngine;

public class UnitAttackExplorationState : UnitStateBase_Exploration
{
    CombatUnitController _combatCont;
    float _elapsedTime;
    float _clipDuration;
    float _timeToExecuteHitbox;
    bool _hitboxFired;

    public UnitAttackExplorationState(UnitStateMachine_Exploration stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Attack_Exploration";

    public override void Enter()
    {
        StopHorizontalMovement();

        if (Brain != null)
            Brain.Attack = false;

        _combatCont = SM.Cont as CombatUnitController;
        _elapsedTime = 0f;
        _hitboxFired = false;

        var clipSet = SM.Cont.UnitData?.Attack_Exploration;
        if (clipSet == null)
        {
            EvaluateLocomotionTransitions();
            return;
        }

        PlayExplorationAnim(clipSet);
        _clipDuration = GetClipDuration(clipSet);
        _timeToExecuteHitbox = SM.Cont.UnitData?.timeToExecuteHitbox ?? 0f;
    }

    public override void Update()
    {
        if (_clipDuration <= 0f)
            return;

        _elapsedTime += Time.deltaTime;

        if (!_hitboxFired && _elapsedTime >= _timeToExecuteHitbox)
        {
            _combatCont?.ActivateHitbox();
            _hitboxFired = true;
        }

        if (_elapsedTime >= _clipDuration)
        {
            EvaluateLocomotionTransitions();
        }
    }

    public override void Exit()
    {
        StopHorizontalMovement();
    }

    static float GetClipDuration(AnimationClipSet clipSet)
    {
        var clip = clipSet?.AnimationClip;
        if (clip == null)
            return 0f;

        float speed = Mathf.Abs(clipSet.speed);
        if (speed < 0.0001f)
            speed = 1f;

        return clip.length / speed;
    }
}
