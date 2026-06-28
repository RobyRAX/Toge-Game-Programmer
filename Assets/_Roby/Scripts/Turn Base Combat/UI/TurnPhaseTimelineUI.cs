using Sirenix.OdinInspector;
using UnityEngine;

public class TurnPhaseTimelineUI : MonoBehaviour
{
    [TitleGroup("Phase Ref")]
    [SerializeField]
    CombatPhaseUI beginTurnPhase;

    [TitleGroup("Phase Ref")]
    [SerializeField]
    CombatPhaseUI selectAttackPhase;

    [TitleGroup("Phase Ref")]
    [SerializeField]
    CombatPhaseUI targetEnemyPhase;

    [TitleGroup("Phase Ref")]
    [SerializeField]
    CombatPhaseUI targetTeamPhase;

    [TitleGroup("Phase Ref")]
    [SerializeField]
    CombatPhaseUI executePhase;

    [TitleGroup("Phase Ref")]
    [SerializeField]
    CombatPhaseUI endPhase;

    [TitleGroup("Setting")]
    [SerializeField]
    Color finishedColor = Color.black;

    [TitleGroup("Setting")]
    [SerializeField]
    Color currentColor = Color.white;

    [TitleGroup("Setting")]
    [SerializeField]
    Color upcomingColor = Color.grey;

    public void Setup(TurnBaseCombatManager manager)
    {
        SetPhase(manager?.CurrentPhase ?? TurnBaseCombatPhase.None);
    }

    public void SetPhase(TurnBaseCombatPhase phase)
    {
        int currentIndex = GetFlowIndex(phase);

        ApplyState(beginTurnPhase, 0, currentIndex);
        ApplyState(selectAttackPhase, 1, currentIndex);
        ApplyState(targetEnemyPhase, 2, currentIndex);
        ApplyState(targetTeamPhase, 3, currentIndex);
        ApplyState(executePhase, 4, currentIndex);
        ApplyState(endPhase, 5, currentIndex);
    }

    static int GetFlowIndex(TurnBaseCombatPhase phase)
    {
        switch (phase)
        {
            case TurnBaseCombatPhase.BeginTurn:
                return 0;
            case TurnBaseCombatPhase.SelectAttack:
                return 1;
            case TurnBaseCombatPhase.SelectTargetOpponent:
                return 2;
            case TurnBaseCombatPhase.SelectTargetTeam:
                return 3;
            case TurnBaseCombatPhase.Attack:
                return 4;
            case TurnBaseCombatPhase.EndTurn:
                return 5;
            default:
                return -1;
        }
    }

    void ApplyState(CombatPhaseUI phaseUI, int phaseIndex, int currentIndex)
    {
        if (phaseUI == null)
            return;

        Color color;
        bool isCurrent;

        if (currentIndex < 0)
        {
            color = upcomingColor;
            isCurrent = false;
        }
        else if (phaseIndex < currentIndex)
        {
            color = finishedColor;
            isCurrent = false;
        }
        else if (phaseIndex == currentIndex)
        {
            color = currentColor;
            isCurrent = true;
        }
        else
        {
            color = upcomingColor;
            isCurrent = false;
        }

        phaseUI.ApplyState(color, isCurrent);
    }
}
