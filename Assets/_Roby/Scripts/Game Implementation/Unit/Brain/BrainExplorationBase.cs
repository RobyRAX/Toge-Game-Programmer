using System;
using RAXY.Movement;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BrainExplorationBase
{
    public event Action<bool> OnSprintChange;
    public event Action<bool> OnAttackChange;

    public UnitControllerBase Cont { get; }
    public UnitStateMachine_Exploration UnitSM { get; set; }
    public UnitMovement MovementCont { get; }
    public GroundChecker GroundChecker { get; }

    public BrainExplorationBase(UnitControllerBase unitController)
    {
        if (unitController == null)
        {
            Debug.LogError("[BrainExplorationBase] unitController is NULL");
            return;
        }

        Cont = unitController;
        UnitSM = unitController.StateMachine_Exploration;

        MovementCont = unitController.GetComponent<UnitMovement>();
        GroundChecker = unitController.GetComponent<GroundChecker>();
    }

    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual Vector2 Move { get; set; }

    bool _sprint;
    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual bool Sprint
    {
        get => _sprint;
        set
        {
            if (_sprint == value)
                return;

            _sprint = value;
            OnSprintChange?.Invoke(_sprint);
        }
    }

    bool _attack;
    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual bool Attack
    {
        get => _attack;
        set
        {
            if (_attack == value)
                return;

            _attack = value;
            OnAttackChange?.Invoke(_attack);
        }
    }

    public virtual void Update() { }
    public virtual void OnDestroy() { }

    public virtual void ResetAllInput()
    {
        Move = default;
        Sprint = false;
        Attack = false;
    }

    public virtual void Subscribe() { }
    public virtual void Unsubscribe() { }
}
