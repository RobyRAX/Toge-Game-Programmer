using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatUI : MonoBehaviour
{
    public PlayerUnitListUI partyListUI;
    public TurnTimelineUI turnTimelineUI;
    public TurnPhaseTimelineUI turnPhaseTimelineUI;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    Transform attackContainer;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    AttackUI attackUiPrefab;

    [TitleGroup("Target Selection")]
    [SerializeField]
    CombatTargetSelector targetSelector;

    [TitleGroup("Turn Side Indicator")]
    [SerializeField]
    GameObject playerTurnIndicator;

    [TitleGroup("Turn Side Indicator")]
    [SerializeField]
    GameObject enemyTurnIndicator;

    TurnBaseCombatManager manager;
    readonly List<AttackUI> spawnedAttackUis = new();

    public void Setup(TurnBaseCombatManager manager)
    {
        Teardown();

        this.manager = manager;
        if (manager == null)
            return;

        manager.OnPhaseChanged += HandlePhaseChanged;
        manager.OnTurnAdvanced += HandleTurnAdvanced;
        manager.OnTimelinePreviewUpdated += HandleTimelinePreviewUpdated;
        manager.OnCombatantStatsChanged += HandleCombatantStatsChanged;

        partyListUI?.Setup(manager);
        turnTimelineUI?.Setup(manager);
        turnPhaseTimelineUI?.Setup(manager);
        targetSelector?.Setup(manager);

        HandlePhaseChanged(manager.CurrentPhase);
        HandleTurnAdvanced();
        RefreshTurnSideIndicators();
    }

    void Teardown()
    {
        if (manager != null)
        {
            manager.OnPhaseChanged -= HandlePhaseChanged;
            manager.OnTurnAdvanced -= HandleTurnAdvanced;
            manager.OnTimelinePreviewUpdated -= HandleTimelinePreviewUpdated;
            manager.OnCombatantStatsChanged -= HandleCombatantStatsChanged;
        }

        ClearAttackButtons();
        targetSelector?.SetActive(false);
        manager = null;
    }

    void HandlePhaseChanged(TurnBaseCombatPhase phase)
    {
        turnPhaseTimelineUI?.SetPhase(phase);

        bool isPlayerTurn = manager != null && manager.IsPlayerTurn;
        bool isPlayerInputPhase = isPlayerTurn && phase == TurnBaseCombatPhase.SelectAttack;
        bool isTargetPhase = isPlayerTurn &&
                             (phase == TurnBaseCombatPhase.SelectTargetOpponent ||
                              phase == TurnBaseCombatPhase.SelectTargetTeam);

        if (attackContainer != null)
            attackContainer.gameObject.SetActive(isPlayerInputPhase);

        if (isPlayerInputPhase)
            RefreshAttackButtons();
        else
            ClearAttackButtons();

        targetSelector?.SetActive(isTargetPhase);
    }

    void RefreshTurnSideIndicators()
    {
        if (manager == null)
            return;

        if (playerTurnIndicator != null)
            playerTurnIndicator.SetActive(manager.CurrentTurnSide == TurnSide.Player);

        if (enemyTurnIndicator != null)
            enemyTurnIndicator.SetActive(manager.CurrentTurnSide == TurnSide.Enemy);
    }

    void HandleTurnAdvanced()
    {
        partyListUI?.SetCurrentPlayerUnit(manager?.CurrentCombatant);
        turnTimelineUI?.Refresh();
        RefreshTurnSideIndicators();
    }

    void HandleTimelinePreviewUpdated()
    {
        turnTimelineUI?.Refresh();
    }

    void HandleCombatantStatsChanged(CombatantBase combatant)
    {
        partyListUI?.RefreshStatsFor(combatant);
        RefreshAttackInteractable();
    }

    void RefreshAttackButtons()
    {
        ClearAttackButtons();

        if (manager?.CurrentCombatant?.AttackBank?.Attacks == null ||
            attackUiPrefab == null ||
            attackContainer == null)
            return;

        foreach (var attack in manager.CurrentCombatant.AttackBank.Attacks)
        {
            if (attack == null)
                continue;

            var attackUi = Instantiate(attackUiPrefab, attackContainer);
            attackUi.Setup(manager, attack);
            spawnedAttackUis.Add(attackUi);
        }
    }

    void RefreshAttackInteractable()
    {
        foreach (var attackUi in spawnedAttackUis)
            attackUi?.RefreshInteractable();
    }

    void ClearAttackButtons()
    {
        foreach (var attackUi in spawnedAttackUis)
        {
            if (attackUi != null)
                Destroy(attackUi.gameObject);
        }

        spawnedAttackUis.Clear();
    }

    void OnDestroy()
    {
        Teardown();
    }
}
