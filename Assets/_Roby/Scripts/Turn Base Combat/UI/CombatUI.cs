using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatUI : MonoBehaviour
{
    const int RegularAttackSlotCount = 4;

    public PlayerUnitListUI partyListUI;
    public TurnTimelineUI turnTimelineUI;
    public TurnPhaseTimelineUI turnPhaseTimelineUI;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    AttackUI attackUiSlot0;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    AttackUI attackUiSlot1;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    AttackUI attackUiSlot2;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    AttackUI attackUiSlot3;

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

    public void Setup(TurnBaseCombatManager manager)
    {
        Teardown();

        this.manager = manager;
        if (manager == null)
            return;

        gameObject.SetActive(true);

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
        ClearAttackButtons();

        if (playerTurnIndicator != null)
            playerTurnIndicator.SetActive(false);

        if (enemyTurnIndicator != null)
            enemyTurnIndicator.SetActive(false);

        gameObject.SetActive(false);

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
        var regularAttacks = GetNonUltimateAttacks();
        var regularSlots = GetRegularAttackUiSlots();

        for (int i = 0; i < RegularAttackSlotCount; i++)
        {
            var slot = regularSlots[i];
            if (slot == null)
                continue;

            if (i < regularAttacks.Count && regularAttacks[i] != null)
            {
                slot.gameObject.SetActive(true);
                slot.Setup(manager, regularAttacks[i]);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
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
        foreach (var attackUi in GetAllAttackUiSlots())
        {
            if (attackUi == null || !attackUi.gameObject.activeSelf)
                continue;

            attackUi.RefreshInteractable();
        }
    }

    void ClearAttackButtons()
    {
        foreach (var attackUi in GetAllAttackUiSlots())
        {
            if (attackUi != null)
                attackUi.gameObject.SetActive(false);
        }
    }

    List<Attack_Runtime> GetNonUltimateAttacks()
    {
        var result = new List<Attack_Runtime>();

        if (manager?.CurrentCombatant?.AttackBank?.Attacks == null)
            return result;

        foreach (var attack in manager.CurrentCombatant.AttackBank.Attacks)
        {
            if (attack == null || attack.IsUltimateAttack)
                continue;

            result.Add(attack);
        }

        return result;
    }

    AttackUI[] GetRegularAttackUiSlots()
    {
        return new[]
        {
            attackUiSlot0,
            attackUiSlot1,
            attackUiSlot2,
            attackUiSlot3
        };
    }

    IEnumerable<AttackUI> GetAllAttackUiSlots()
    {
        yield return attackUiSlot0;
        yield return attackUiSlot1;
        yield return attackUiSlot2;
        yield return attackUiSlot3;
        yield return ultimateAttackUi;
    }

    void OnDestroy()
    {
        Teardown();
    }
}
