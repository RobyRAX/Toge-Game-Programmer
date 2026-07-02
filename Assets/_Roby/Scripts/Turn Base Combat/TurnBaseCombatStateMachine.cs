using Cysharp.Threading.Tasks;
using UnityEngine;

public class TurnBaseCombatStateMachine
{
    TurnBaseCombatManager manager;
    CombatCameraDirector camDir;
    int transitionGeneration;

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

        transitionGeneration++;
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
                manager.RegenStaminaForCurrentCombatant();
                AdvancePhaseAfterMinDuration(TurnBaseCombatPhase.BeginTurn, TurnBaseCombatPhase.SelectAttack).Forget();
                break;

            case TurnBaseCombatPhase.SelectAttack:
                FocusCameraOnHeroSide();
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    manager.PickRandomAttackForCurrentCombatant();
                    AdvancePhaseAfterMinDuration(TurnBaseCombatPhase.SelectAttack, manager.GetFirstTargetPhase()).Forget();
                }
                break;

            case TurnBaseCombatPhase.SelectTargetOpponent:
                FocusCameraOnHeroSide();
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    manager.AutoPickOpponentTarget();
                    AdvancePhaseAfterMinDuration(TurnBaseCombatPhase.SelectTargetOpponent, manager.GetPhaseAfterOpponentTarget()).Forget();
                }
                break;

            case TurnBaseCombatPhase.SelectTargetTeam:
                if (manager.CurrentTurnSide == TurnSide.Enemy)
                {
                    FocusCameraOnHeroSide();
                    manager.AutoPickTeamTarget();
                    AdvancePhaseAfterMinDuration(TurnBaseCombatPhase.SelectTargetTeam, TurnBaseCombatPhase.Attack).Forget();
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
                ResolveEndTurnAfterMinDuration().Forget();
                break;

            case TurnBaseCombatPhase.EndCombat:
                manager.ResolveEndCombatAsync().Forget();
                break;
        }
    }

    async UniTaskVoid AdvancePhaseAfterMinDuration(TurnBaseCombatPhase expectedPhase, TurnBaseCombatPhase nextPhase)
    {
        int gen = transitionGeneration;
        float delay = manager.PhaseMinimumDuration;
        if (delay > 0f)
            await UniTask.WaitForSeconds(delay);

        if (gen != transitionGeneration
            || CurrentPhase != expectedPhase
            || manager.IsCombatOver)
            return;

        ChangePhase(nextPhase);
    }

    async UniTaskVoid ResolveEndTurnAfterMinDuration()
    {
        int gen = transitionGeneration;
        float delay = manager.PhaseMinimumDuration;
        if (delay > 0f)
            await UniTask.WaitForSeconds(delay);

        if (gen != transitionGeneration
            || CurrentPhase != TurnBaseCombatPhase.EndTurn
            || manager.IsCombatOver)
            return;

        manager.ResolveEndTurnAsync().Forget();
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
