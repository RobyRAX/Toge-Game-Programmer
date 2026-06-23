using System;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

[Serializable]
public class ItemInstance_Hero : ItemInstance_Base
{
    public HeroDataSO heroDataSO => ItemEntry as HeroDataSO;
    public string HeroId => ItemInstanceId;

    public int level = 1;
    public int exp;
    public float currentHp;

    public int NormalAttackTalentLevel = 1;
    public int SkillTalentLevel = 1;
    public int UltimateTalentLevel = 1;

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
