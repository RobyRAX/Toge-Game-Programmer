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

        SubscribeToInputAction();
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

    public void SubscribeToInputAction()
    {
        if (BrainConfig == null)
            return;

        UnsubFromInputAction();

        BrainConfig.MoveInputActionSO.Subscribe(MoveInputActionChangeHandler);
        BrainConfig.SprintInputActionSO.Subscribe(SprintInputActionChangeHandler);
        BrainConfig.AttackInputActionSO.Subscribe(AttackInputActionChangeHandler);
    }

    public void UnsubFromInputAction()
    {
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
}
