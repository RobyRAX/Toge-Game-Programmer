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

    [TitleGroup("Attack Ref")]
    [SerializeField]
    Transform ultimateAttackContainer;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    AttackUI ultimateAttackUi;

    [TitleGroup("Target Selection")]
    [SerializeField]
    CombatTargetSelector targetSelector;

    [TitleGroup("Enemy HP Bar")]
    public CombatEnemyHpBarManager enemyHpBarManager;

    [TitleGroup("Damage Number")]
    public CombatDamageNumberSpawner damageNumberSpawner;

    [TitleGroup("Turn Side Indicator")]
    [SerializeField]
    GameObject playerTurnIndicator;

    [TitleGroup("Turn Side Indicator")]
    [SerializeField]
    GameObject enemyTurnIndicator;

    TurnBaseCombatManager manager;
    List<AttackUI> spawnedAttackUis = new();

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
        enemyHpBarManager?.Setup(manager);
        damageNumberSpawner?.Setup(manager);

        HandlePhaseChanged(manager.CurrentPhase);
        HandleTurnAdvanced();
        RefreshTurnSideIndicators();
    }

    public void Shutdown()
    {
        if (attackContainer != null)
            attackContainer.gameObject.SetActive(false);

        if (ultimateAttackContainer != null)
            ultimateAttackContainer.gameObject.SetActive(false);

        if (playerTurnIndicator != null)
            playerTurnIndicator.SetActive(false);

        if (enemyTurnIndicator != null)
            enemyTurnIndicator.SetActive(false);

        Teardown();
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
        enemyHpBarManager?.Teardown();
        damageNumberSpawner?.Teardown();
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

        if (ultimateAttackContainer != null)
            ultimateAttackContainer.gameObject.SetActive(isPlayerInputPhase);

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

        if (manager?.CurrentCombatant?.AttackBank?.Attacks == null)
            return;

        RefreshRegularAttackButtons();
        RefreshUltimateAttackButton();
    }

    void RefreshRegularAttackButtons()
    {
        if (attackUiPrefab == null || attackContainer == null)
            return;

        foreach (var attack in manager.CurrentCombatant.AttackBank.Attacks)
        {
            if (attack == null || attack.IsUltimateAttack)
                continue;

            var attackUi = Instantiate(attackUiPrefab, attackContainer);
            attackUi.Setup(manager, attack);
            spawnedAttackUis.Add(attackUi);
        }
    }

    void RefreshUltimateAttackButton()
    {
        if (ultimateAttackUi == null)
            return;

        Attack_Runtime ultimateAttack = null;
        if (manager?.CurrentCombatant?.AttackBank is HeroAttackBank_Runtime heroBank)
            ultimateAttack = heroBank.PrimaryUltimateAttack;

        if (ultimateAttack == null)
        {
            ultimateAttackUi.gameObject.SetActive(false);
            return;
        }

        ultimateAttackUi.gameObject.SetActive(true);
        ultimateAttackUi.Setup(manager, ultimateAttack);
    }

    void RefreshAttackInteractable()
    {
        foreach (var attackUi in spawnedAttackUis)
            attackUi?.RefreshInteractable();

        ultimateAttackUi?.RefreshInteractable();
    }

    void ClearAttackButtons()
    {
        foreach (var attackUi in spawnedAttackUis)
        {
            if (attackUi != null)
                Destroy(attackUi.gameObject);
        }

        if (attackContainer != null)
        {
            foreach (Transform child in attackContainer)
                Destroy(child.gameObject);
        }

        spawnedAttackUis.Clear();

        if (ultimateAttackUi != null)
            ultimateAttackUi.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        Teardown();
    }
}
