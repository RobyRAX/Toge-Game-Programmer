using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatTargetSelector : MonoBehaviour
{
    [TitleGroup("Setting")]
    [SerializeField]
    LayerMask targetLayerMask = ~0;

    [TitleGroup("Setting")]
    [SerializeField]
    GameObject highlightPrefab;

    TurnBaseCombatManager manager;
    readonly Dictionary<CombatantBase, GameObject> highlightByCombatant = new();
    CombatantBase hoveredCombatant;
    bool isActive;

    public void Setup(TurnBaseCombatManager manager)
    {
        this.manager = manager;
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (!active)
        {
            ClearHighlights();
            hoveredCombatant = null;
        }
        else
        {
            RefreshHighlights();
        }
    }

    void Update()
    {
        if (!isActive || manager == null)
            return;

        UpdateHover();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TrySubmitHoveredTarget();
    }

    void UpdateHover()
    {
        var target = RaycastCombatant();
        if (hoveredCombatant == target)
            return;

        hoveredCombatant = target;
        RefreshHighlights();
    }

    void TrySubmitHoveredTarget()
    {
        if (hoveredCombatant == null || !IsValidTarget(hoveredCombatant))
            return;

        switch (manager.CurrentPhase)
        {
            case TurnBaseCombatPhase.SelectTargetOpponent:
                manager.SubmitTargetOpponent(hoveredCombatant);
                break;

            case TurnBaseCombatPhase.SelectTargetTeam:
                manager.SubmitTargetTeam(hoveredCombatant);
                break;
        }
    }

    CombatantBase RaycastCombatant()
    {
        if (Camera.main == null || Mouse.current == null)
            return null;

        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, targetLayerMask))
            return null;

        if (hit.collider.TryGetComponent(out CombatantClickTarget clickTarget))
            return clickTarget.Combatant;

        return hit.collider.GetComponentInParent<CombatantClickTarget>()?.Combatant;
    }

    bool IsValidTarget(CombatantBase combatant)
    {
        if (combatant == null || !combatant.IsAlive)
            return false;

        switch (manager.CurrentPhase)
        {
            case TurnBaseCombatPhase.SelectTargetOpponent:
                return manager.EnemyCombatants != null && manager.EnemyCombatants.Contains(combatant);

            case TurnBaseCombatPhase.SelectTargetTeam:
                return manager.PlayerCombatants != null && manager.PlayerCombatants.Contains(combatant);

            default:
                return false;
        }
    }

    void RefreshHighlights()
    {
        ClearHighlights();

        if (!isActive || manager == null || highlightPrefab == null)
            return;

        List<CombatantBase> candidates = manager.CurrentPhase switch
        {
            TurnBaseCombatPhase.SelectTargetOpponent => manager.EnemyCombatants,
            TurnBaseCombatPhase.SelectTargetTeam => manager.PlayerCombatants,
            _ => null,
        };

        if (candidates == null)
            return;

        foreach (var combatant in candidates)
        {
            if (combatant == null || !combatant.IsAlive)
                continue;

            var highlight = Instantiate(highlightPrefab, combatant.transform);
            highlight.transform.localPosition = Vector3.zero;
            highlight.SetActive(combatant == hoveredCombatant);
            highlightByCombatant[combatant] = highlight;
        }
    }

    void ClearHighlights()
    {
        foreach (var pair in highlightByCombatant)
        {
            if (pair.Value != null)
                Destroy(pair.Value);
        }

        highlightByCombatant.Clear();
    }

    void OnDestroy()
    {
        ClearHighlights();
    }
}
