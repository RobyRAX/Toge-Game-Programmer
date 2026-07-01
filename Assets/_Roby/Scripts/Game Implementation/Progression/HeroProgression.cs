using System;
using ToGaProTest.Shared;
using UnityEngine;

public static class HeroProgression
{
    public static event Action<ItemInstance_Hero> OnHeroChanged;
    public static event Action<ItemInstance_Hero, int> OnHeroLevelUp;

    static GameplayConfigSO Config => GameplayConfig.Instance?.ConfigSO;

    public static void AddExpToSpawnedHeroes(int amount)
    {
        if (amount <= 0)
            return;

        var manager = GameplayManager.Instance;
        if (manager?.SpawnedHeroDict == null)
            return;

        foreach (var heroId in manager.SpawnedHeroDict.Keys)
        {
            var hero = InventoryManager.Instance?.GetInstanceHero(heroId);
            if (hero != null)
                AddExp(hero, amount);
        }
    }

    public static void AddExp(ItemInstance_Hero hero, int amount)
    {
        if (hero == null || amount <= 0 || Config == null)
            return;

        if (Config.IsMaxLevel(hero.level))
            return;

        hero.exp += amount;
        int levelsGained = 0;

        while (!Config.IsMaxLevel(hero.level))
        {
            int required = Config.GetExpRequiredForNextLevel(hero.level);
            if (required <= 0 || hero.exp < required)
                break;

            hero.exp -= required;
            hero.level++;
            hero.availableTalentPoints += Config.talentPointsPerLevelUp;
            levelsGained++;
        }

        if (Config.IsMaxLevel(hero.level))
            hero.exp = 0;

        hero.RefreshStatContainer();
        SyncSpawnedHero(hero.HeroId, hero, levelsGained > 0);

        OnHeroChanged?.Invoke(hero);
        if (levelsGained > 0)
            OnHeroLevelUp?.Invoke(hero, levelsGained);
    }

    public static bool TryUpgradeTalent(ItemInstance_Hero hero, HeroTalentType type)
    {
        if (!CanUpgradeTalent(hero, type))
            return false;

        hero.availableTalentPoints--;
        SetTalentLevel(hero, type, GetTalentLevel(hero, type) + 1);
        hero.RefreshStatContainer();
        SyncSpawnedHero(hero.HeroId, hero, false);

        OnHeroChanged?.Invoke(hero);
        return true;
    }

    public static bool CanUpgradeTalent(ItemInstance_Hero hero, HeroTalentType type)
    {
        if (hero == null || Config == null)
            return false;

        if (hero.availableTalentPoints < 1)
            return false;

        return GetTalentLevel(hero, type) < Config.maxTalentLevel;
    }

    public static int GetExpToNextLevel(int level)
    {
        return Config?.GetExpRequiredForNextLevel(level) ?? 0;
    }

    public static (int current, int required) GetExpProgress(ItemInstance_Hero hero)
    {
        if (hero == null || Config == null)
            return (0, 0);

        if (Config.IsMaxLevel(hero.level))
            return (0, 0);

        return (hero.exp, Config.GetExpRequiredForNextLevel(hero.level));
    }

    public static int GetTalentLevel(ItemInstance_Hero hero, HeroTalentType type)
    {
        if (hero == null)
            return 1;

        return type switch
        {
            HeroTalentType.NormalAttack => hero.NormalAttackTalentLevel,
            HeroTalentType.Skill => hero.SkillTalentLevel,
            HeroTalentType.Ultimate => hero.UltimateTalentLevel,
            _ => 1
        };
    }

    static void SetTalentLevel(ItemInstance_Hero hero, HeroTalentType type, int level)
    {
        switch (type)
        {
            case HeroTalentType.NormalAttack:
                hero.NormalAttackTalentLevel = level;
                break;
            case HeroTalentType.Skill:
                hero.SkillTalentLevel = level;
                break;
            case HeroTalentType.Ultimate:
                hero.UltimateTalentLevel = level;
                break;
        }
    }

    static void SyncSpawnedHero(string heroId, ItemInstance_Hero hero, bool fullHealOnLevelUp)
    {
        var manager = GameplayManager.Instance;
        if (manager?.SpawnedHeroDict == null ||
            !manager.SpawnedHeroDict.TryGetValue(heroId, out var controller) ||
            controller?.heroCombatant == null)
        {
            return;
        }

        var combatant = controller.heroCombatant;
        (combatant.AttackBank as HeroAttackBank_Runtime)?.BuildAttacks();

        if (fullHealOnLevelUp && hero.GetStatContainer() != null)
            combatant.CurrentHp = hero.GetStatContainer().GetTotalValue(StatAttribute.MaxHp);

        combatant.SyncDisplayedHp();
        manager.exploreUI?.RefreshPartyUnits();
    }
}
