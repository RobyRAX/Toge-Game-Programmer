using System;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

[Serializable]
public class ItemInstance_Hero : ItemInstance_Base
{
    public static event Action<ItemInstance_Hero> OnHeroChanged;
    public static event Action<ItemInstance_Hero, int> OnHeroLevelUp;

    public HeroDataSO heroDataSO => ItemEntry as HeroDataSO;
    public string HeroId => ItemInstanceId;

    public int level = 1;
    public int exp;
    public float currentHp;

    public int availableTalentPoints;

    public int NormalAttackTalentLevel = 1;
    public int SkillTalentLevel = 1;
    public int UltimateTalentLevel = 1;

    static GameplayConfigSO Config => GameplayConfig.Instance?.ConfigSO;

    [TitleGroup("Debug Functions")]
    [Button]
    public void AddExp(int amount)
    {
        if (amount <= 0 || Config == null)
            return;

        if (Config.IsMaxLevel(level))
            return;

        exp += amount;
        int levelsGained = 0;

        while (!Config.IsMaxLevel(level))
        {
            int required = Config.GetExpRequiredForNextLevel(level);
            if (required <= 0 || exp < required)
                break;

            exp -= required;
            level++;
            availableTalentPoints += Config.talentPointsPerLevelUp;
            levelsGained++;
        }

        if (Config.IsMaxLevel(level))
            exp = 0;

        RefreshStatContainer();
        SyncSpawnedHero(levelsGained > 0);

        OnHeroChanged?.Invoke(this);
        if (levelsGained > 0)
            OnHeroLevelUp?.Invoke(this, levelsGained);
    }

    public bool TryUpgradeTalent(HeroTalentType type)
    {
        if (!CanUpgradeTalent(type))
            return false;

        availableTalentPoints--;
        SetTalentLevel(type, GetTalentLevel(type) + 1);
        RefreshStatContainer();
        SyncSpawnedHero(false);

        OnHeroChanged?.Invoke(this);
        return true;
    }

    public bool CanUpgradeTalent(HeroTalentType type)
    {
        if (Config == null)
            return false;

        if (availableTalentPoints < 1)
            return false;

        return GetTalentLevel(type) < Config.maxTalentLevel;
    }

    public (int current, int required) GetExpProgress()
    {
        if (Config == null)
            return (0, 0);

        if (Config.IsMaxLevel(level))
            return (0, 0);

        return (exp, Config.GetExpRequiredForNextLevel(level));
    }

    public int GetTalentLevel(HeroTalentType type)
    {
        return type switch
        {
            HeroTalentType.NormalAttack => NormalAttackTalentLevel,
            HeroTalentType.Skill => SkillTalentLevel,
            HeroTalentType.Ultimate => UltimateTalentLevel,
            _ => 1
        };
    }

    void SetTalentLevel(HeroTalentType type, int talentLevel)
    {
        switch (type)
        {
            case HeroTalentType.NormalAttack:
                NormalAttackTalentLevel = talentLevel;
                break;
            case HeroTalentType.Skill:
                SkillTalentLevel = talentLevel;
                break;
            case HeroTalentType.Ultimate:
                UltimateTalentLevel = talentLevel;
                break;
        }
    }

    void SyncSpawnedHero(bool fullHealOnLevelUp)
    {
        var manager = GameplayManager.Instance;
        if (manager?.SpawnedHeroDict == null ||
            !manager.SpawnedHeroDict.TryGetValue(HeroId, out var controller) ||
            controller?.heroCombatant == null)
        {
            return;
        }

        var combatant = controller.heroCombatant;
        (combatant.AttackBank as HeroAttackBank_Runtime)?.BuildAttacks();

        if (fullHealOnLevelUp && GetStatContainer() != null)
            combatant.CurrentHp = GetStatContainer().GetTotalValue(StatAttribute.MaxHp);

        combatant.SyncDisplayedHp();
        manager.exploreUI?.RefreshPartyUnits();
    }

    [TitleGroup("Stat")]
    [ShowInInspector]
    StatContainer_Runtime _cachedStatContainer;
    public StatContainer_Runtime GetStatContainer(bool refresh = false)
    {
        if (refresh)
        {
            RefreshStatContainer();
            return _cachedStatContainer;
        }
        else
        {
            if (_cachedStatContainer == null)
            {
                SetCachedContainer(true);
                return _cachedStatContainer;
            }
            else
                return _cachedStatContainer;
        }
    }

    [TitleGroup("Stat")]
    [Button]
    public void RefreshStatContainer()
    {
        SetCachedContainer(false);
    }

    void SetCachedContainer(bool reset)
    {
        if (GameplayConfig.Instance == null)
            return;

        if (reset)
            _cachedStatContainer = new StatContainer_Runtime(GameplayConfig.Instance);
        else
        {
            if (_cachedStatContainer == null)
                _cachedStatContainer = new StatContainer_Runtime(GameplayConfig.Instance);
        }

        heroDataSO.StatGrowth.ApplyMainStatsTo(_cachedStatContainer, level);
    }
}
