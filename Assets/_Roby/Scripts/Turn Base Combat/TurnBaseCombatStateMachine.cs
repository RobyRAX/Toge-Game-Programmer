using Cysharp.Threading.Tasks;
using UnityEngine;

public class TurnBaseCombatStateMachine
{
    TurnBaseCombatManager manager;
    CombatCameraDirector camDir;

    public TurnBaseCombatPhase CurrentPhase { get; private set; }

    public TurnBaseCombatStateMachine(TurnBaseCombatManager manager)
    {
        this.manager = manager;
        camDir = manager.CameraDirector;
    }

    public void ChangePhase(TurnBaseCombatPhase phase)
    {
        if (CurrentPhase == phase)
            return;

        Exit(CurrentPhase);
        CurrentPhase = phase;
        manager.RaisePhaseChanged(phase);
        Enter(phase);
    }

    void Enter(TurnBaseCombatPhase phase)
    {
        switch (phase)
        {
            case TurnBaseCombatPhase.StartCombat:
                camDir?.FocusOnDefault();
                Debug.Log($"{nameof(TurnBaseCombatStateMachine)}: Combat started.");
                manager.AdvanceTimelineUntilTurnFound();
                break;

            case TurnBaseCombatPhase.BeginTurn:
                camDir?.FocusOnCombatant(manager.CurrentCombatant);
                ChangePhase(TurnBaseCombatPhase.SelectAttack);
                break;

            case TurnBaseCombatPhase.SelectAttack:
                camDir?.FocusOnCombatant(manager.CurrentCombatant);
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    manager.PickRandomAttackForCurrentCombatant();
                    ChangePhase(TurnBaseCombatPhase.SelectTargetOpponent);
                }
                break;

            case TurnBaseCombatPhase.SelectTargetOpponent:
                camDir?.FocusOnCombatant(manager.CurrentCombatant);
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    manager.AutoPickOpponentTarget();
                    ChangePhase(TurnBaseCombatPhase.SelectTargetTeam);
                }
                break;

            case TurnBaseCombatPhase.SelectTargetTeam:
                camDir?.FocusOnCombatant(manager.CurrentCombatant);
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    manager.AutoPickTeamTarget();
                    ChangePhase(TurnBaseCombatPhase.Attack);
                }
                break;

            case TurnBaseCombatPhase.Attack:
                if (!manager.HasValidAttackSelection())
                {
                    Debug.LogWarning($"{nameof(TurnBaseCombatStateMachine)}: Invalid attack selection.");
                    if (manager.IsPlayerTurn)
                        ChangePhase(TurnBaseCombatPhase.SelectAttack);
                    else
                        manager.AdvanceTimelineUntilTurnFound();
                    return;
                }

                manager.ExecuteAttack().Forget();
                break;

            case TurnBaseCombatPhase.EndTurn:
                manager.CompleteCurrentTurn();
                break;

            case TurnBaseCombatPhase.EndCombat:
                camDir?.FocusOnDefault();
                Debug.Log($"{nameof(TurnBaseCombatStateMachine)}: Combat ended. Winner: {manager.WinningSide}.");
                manager.RaiseCombatEnded();
                break;
        }
    }

    void Exit(TurnBaseCombatPhase phase)
    {
    }
}

public enum TurnBaseCombatPhase
{
    None,
    StartCombat,
    BeginTurn,
    SelectAttack,
    SelectTargetOpponent,
    SelectTargetTeam,
    Attack,
    EndTurn,
    EndCombat
}

public enum TurnSide
{
    None,
    Player,
    Enemy
}
