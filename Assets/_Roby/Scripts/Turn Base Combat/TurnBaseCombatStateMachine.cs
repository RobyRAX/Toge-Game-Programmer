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
                FocusCameraOnHeroSide();
                ChangePhase(TurnBaseCombatPhase.SelectAttack);
                break;

            case TurnBaseCombatPhase.SelectAttack:
                FocusCameraOnHeroSide();
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    manager.PickRandomAttackForCurrentCombatant();
                    ChangePhase(manager.GetFirstTargetPhase());
                }
                break;

            case TurnBaseCombatPhase.SelectTargetOpponent:
                FocusCameraOnHeroSide();
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    manager.AutoPickOpponentTarget();
                    ChangePhase(manager.GetPhaseAfterOpponentTarget());
                }
                break;

            case TurnBaseCombatPhase.SelectTargetTeam:
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    FocusCameraOnHeroSide();
                    manager.AutoPickTeamTarget();
                    ChangePhase(TurnBaseCombatPhase.Attack);
                }
                else
                {
                    camDir?.FocusOnPlayerTeam();
                }
                break;

            case TurnBaseCombatPhase.Attack:
                FocusCameraOnHeroSide();
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
                manager.ResolveEndTurnAsync().Forget();
                break;

            case TurnBaseCombatPhase.EndCombat:
                manager.ResolveEndCombatAsync().Forget();
                break;
        }
    }

    void Exit(TurnBaseCombatPhase phase)
    {
        if (phase == TurnBaseCombatPhase.SelectTargetTeam)
            camDir?.RestoreCombatCamera();
    }

    // Camera always anchors to a hero slot, never to an enemy.
    // - Player turn: camera sits behind the acting hero.
    // - Enemy turn: camera swings behind the hero being targeted (once known), otherwise the default hero slot.
    void FocusCameraOnHeroSide()
    {
        if (camDir == null)
            return;

        if (manager.CurrentTurnSide == TurnSide.Enemy)
        {
            if (manager.TargetOpponent != null)
                camDir.FocusOnCombatant(manager.TargetOpponent);
            else
                camDir.FocusOnDefault();
            return;
        }

        camDir.FocusOnCombatant(manager.CurrentCombatant);
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
