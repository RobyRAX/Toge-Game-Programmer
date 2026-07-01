using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExploreUI : MonoBehaviour
{
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<PlayerUnitUI> playerUnitUis;

    GameplayManager manager;

    public void Setup(GameplayManager manager)
    {
        Teardown();

        this.manager = manager;
        if (manager == null)
            return;

        manager.OnRespawn -= RefreshPartyUnits;
        manager.OnRespawn += RefreshPartyUnits;

        RefreshPartyUnits();
    }

    public void RefreshPartyUnits()
    {
        if (playerUnitUis == null)
            return;

        for (int i = 0; i < playerUnitUis.Count; i++)
        {
            var slot = playerUnitUis[i];
            if (slot == null)
                continue;

            var combatant = GetPartyCombatantAt(i, out bool isMainHero);

            slot.Setup(combatant);
            if (combatant != null)
                slot.SetCurrentMainUnit(isMainHero);
        }
    }

    CombatantBase GetPartyCombatantAt(int index, out bool isMainHero)
    {
        isMainHero = false;

        if (manager?.partyIds == null || index < 0 || index >= manager.partyIds.Count)
            return null;

        var heroId = manager.partyIds[index];
        if (string.IsNullOrEmpty(heroId) ||
            manager.SpawnedHeroDict == null ||
            !manager.SpawnedHeroDict.TryGetValue(heroId, out var hero) ||
            hero == null)
            return null;

        isMainHero = hero == manager.MainHero;
        return hero.heroCombatant;
    }

    public void Show()
    {
        gameObject?.SetActive(true);
    }

    public void Hide()
    {
        gameObject?.SetActive(false);
    }

    public void Teardown()
    {
        if (manager != null)
            manager.OnRespawn -= RefreshPartyUnits;

        if (playerUnitUis != null)
        {
            foreach (var slot in playerUnitUis)
                slot?.Teardown();
        }

        manager = null;
    }

    void OnDestroy()
    {
        Teardown();
    }
}
