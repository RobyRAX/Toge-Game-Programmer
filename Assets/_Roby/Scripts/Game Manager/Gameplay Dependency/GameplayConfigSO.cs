using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GameplayConfigSO", menuName = "RAXY/Gameplay Config SO")]
public class GameplayConfigSO : ScriptableObject, IStatEntryProvider
{
    public int maxTalentLevel;

    [TitleGroup("Stat")]
    [SerializeField]
    [ListDrawerSettings(ListElementLabelName = "statName")]
    List<StatEntry> statEntries;
    public List<StatEntry> StatEntries => statEntries;

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
                statName = attribute.ToString(),
            });
        }

        EditorUtility.SetDirty(this);
    }
#endif
}
