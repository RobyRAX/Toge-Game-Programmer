using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;
using RAXY.InteractionSystem;
using RAXY.InputSystem;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GameplayConfigSO", menuName = "RAXY/Gameplay Config SO")]
public class GameplayConfigSO : ScriptableObject, IStatEntryProvider, IInteractableTagProvider
{
    [TitleGroup("Brain Config")]
    public ActiveUnitBrainExplorationConfigSO defaultActiveUnitBrainExplorationConfigSO;

    [TitleGroup("Brain Config")]
    public EnemyBrainExplorationConfigSO defaultEnemyBrainExplorationConfigSO;
    
    [TitleGroup("Stat")]
    [SerializeField]
    [ListDrawerSettings(ListElementLabelName = "statName")]
    List<StatEntry> statEntries;
    public List<StatEntry> StatEntries => statEntries;

    [TitleGroup("Interaction")]
    public InputActionEventSO InteractEventSO;

    [TitleGroup("Interaction")]
    [SerializeField]
    List<string> interactionTag;
    public List<string> Tags => interactionTag;

    [TitleGroup("Hitbox")]
    public HitboxSetting heroHitboxSetting;

    [TitleGroup("Hitbox")]
    public HitboxSetting enemyHitboxSetting;

    [TitleGroup("Misc")]
    public string initialSpawnPoint;

    [TitleGroup("Misc")]
    public int maxTalentLevel;

    [TitleGroup("Progression")]
    public int talentPointsPerLevelUp = 2;

    [TitleGroup("Progression")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    [Tooltip("Index level-1 = EXP required to advance from level N to N+1. List count defines max level.")]
    public List<int> expToNextLevel = new();

    public int GetExpRequiredForNextLevel(int currentLevel)
    {
        if (expToNextLevel == null || expToNextLevel.Count == 0)
            return 0;

        int index = currentLevel - 1;
        if (index < 0 || index >= expToNextLevel.Count)
            return 0;

        return Mathf.Max(0, expToNextLevel[index]);
    }

    public bool IsMaxLevel(int level)
    {
        if (expToNextLevel == null || expToNextLevel.Count == 0)
            return true;

        return level > expToNextLevel.Count;
    }

    public int MaxHeroLevel => expToNextLevel?.Count ?? 0;

#if UNITY_EDITOR
    [TitleGroup("Stat")]
    [Button]
    void InitStatEntries()
    {
        if (statEntries == null)
            statEntries = new List<StatEntry>();

        var existingEntries = new Dictionary<StatAttribute, StatEntry>();
        foreach (var entry in statEntries)
        {
            if (entry == null)
                continue;

            if (existingEntries.ContainsKey(entry.attribute))
                continue;

            existingEntries.Add(entry.attribute, entry);
        }

        statEntries.Clear();

        foreach (StatAttribute attribute in Enum.GetValues(typeof(StatAttribute)))
        {
            if (attribute == StatAttribute.None)
                continue;

            if (existingEntries.TryGetValue(attribute, out var existingEntry))
            {
                statEntries.Add(existingEntry);
                continue;
            }

            statEntries.Add(new StatEntry
            {
                attribute = attribute,
                defaultValue = 0f,
                isRound = ToGaProTestShared.ShouldRoundStat_Static(attribute),
                statName = attribute.ToString(),
            });
        }

        EditorUtility.SetDirty(this);
    }
#endif
}
