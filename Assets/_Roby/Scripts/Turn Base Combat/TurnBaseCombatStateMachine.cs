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
        Enter(phase);
    }

    void Enter(TurnBaseCombatPhase phase)
    {
        switch (phase)
        {
            case TurnBaseCombatPhase.Start:
                camDir.FocusOnDefault();
                Debug.Log($"{nameof(TurnBaseCombatStateMachine)}: Combat started.");
                manager.AdvanceTimelineUntilTurnFound();
                break;

            case TurnBaseCombatPhase.PlayerSelectAttack:
                camDir.FocusOnCombatant(manager.CurrentCombatant);
                break;

            case TurnBaseCombatPhase.PlayerSelectTargetEnemy:
            case TurnBaseCombatPhase.PlayerSelectTargetTeam:
                break;

            case TurnBaseCombatPhase.PlayerAttack:
                if (!manager.HasValidAttackSelection())
                {
                    Debug.LogWarning($"{nameof(TurnBaseCombatStateMachine)}: Invalid player attack selection.");
                    return;
                }

                manager.ExecuteAttack().Forget();
                break;

            case TurnBaseCombatPhase.EnemySelectAttack:
                manager.PickRandomAttackForCurrentCombatant();
                manager.PickRandomTargetsForCurrentCombatant();
                ChangePhase(TurnBaseCombatPhase.EnemyAttack);
                break;

            case TurnBaseCombatPhase.EnemyAttack:
                if (!manager.HasValidAttackSelection())
                {
                    Debug.LogWarning($"{nameof(TurnBaseCombatStateMachine)}: Invalid enemy attack selection.");
                    manager.AdvanceTimelineUntilTurnFound();
                    return;
                }

                manager.ExecuteAttack().Forget();
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
    Start,
    PlayerSelectAttack,
    PlayerSelectTargetEnemy,
    PlayerSelectTargetTeam,
    PlayerAttack,
    EnemySelectAttack,
    EnemyAttack
}
