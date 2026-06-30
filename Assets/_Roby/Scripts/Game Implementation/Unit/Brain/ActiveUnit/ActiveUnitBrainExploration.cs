using System;
using RAXY.InputSystem;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

public class ActiveUnitBrainExploration : BrainExplorationBase
{
    public ActiveUnitBrainExploration(UnitControllerBase unitController, 
                                        ActiveUnitBrainExplorationConfigSO config,
                                        Transform camTransform) : base(unitController)
    {
        BrainConfig = config;
        _camTransform = camTransform;

        Subscribe();

        TurnBaseCombatManager.Instance.OnCombatStarted -= CombatStartedHandler;
        TurnBaseCombatManager.Instance.OnCombatStarted += CombatStartedHandler;

        TurnBaseCombatManager.Instance.OnCombatEnded -= CombatEndedHandler;
        TurnBaseCombatManager.Instance.OnCombatEnded += CombatEndedHandler;

        GameplayManager.Instance.OnRespawn -= RespawnHandler;
        GameplayManager.Instance.OnRespawn += RespawnHandler;
    }

    void RespawnHandler()
    {
        Subscribe();
    }

    private void CombatEndedHandler(TurnSide winningSide)
    {
        if (winningSide == TurnSide.Player)
            Subscribe();
    }

    private void CombatStartedHandler()
    {
        Unsubscribe();
    }

    [TitleGroup("Brain Config")]
    [ShowInInspector]
    [ReadOnly]
    public ActiveUnitBrainExplorationConfigSO BrainConfig { get; set; }

    Transform _camTransform;
    Vector2 _rawMove;
    public override Vector2 Move
    {
        get
        {
            // no camera? return raw
            if (_camTransform == null)
                return _rawMove;

            // get camera forward/right on XZ plane
            Vector3 camForward = _camTransform.forward;
            Vector3 camRight = _camTransform.right;

            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            // convert input (x = left/right, y = forward/back)
            Vector3 worldDir = camForward * _rawMove.y + camRight * _rawMove.x;

            // return as Vector2 on XZ plane
            return new Vector2(worldDir.x, worldDir.z);
        }
        set => _rawMove = value;
    }

    public override void Subscribe()
    {
        if (BrainConfig == null)
            return;
        
        base.Subscribe();

        Unsubscribe();

        BrainConfig.MoveInputActionSO.Subscribe(MoveInputActionChangeHandler);
        BrainConfig.SprintInputActionSO.Subscribe(SprintInputActionChangeHandler);
        BrainConfig.AttackInputActionSO.Subscribe(AttackInputActionChangeHandler);
    }

    public override void Unsubscribe()
    {
        if (BrainConfig == null)
            return;

        base.Unsubscribe();

        BrainConfig.MoveInputActionSO.Unsubscribe(MoveInputActionChangeHandler);
        BrainConfig.SprintInputActionSO.Unsubscribe(SprintInputActionChangeHandler);
        BrainConfig.AttackInputActionSO.Unsubscribe(AttackInputActionChangeHandler);
    }

    void MoveInputActionChangeHandler(InputContext ctx)
    {
        _rawMove = ctx.Vector2Value;
    }

    void SprintInputActionChangeHandler(InputContext ctx)
    {
        Sprint = ctx.BoolValue;
    }

    void AttackInputActionChangeHandler(InputContext ctx)
    {
        Attack = ctx.BoolValue;
    }

    public override void OnDestroy()
    {
        Unsubscribe();
    }
}
