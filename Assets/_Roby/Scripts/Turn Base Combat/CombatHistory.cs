using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public struct CombatHistoryEntry
{
    public int TurnStep;
    public List<AttackResult> Results;
}

public class CombatHistory
{
    [ShowInInspector]
    [ReadOnly]
    Dictionary<int, CombatHistoryEntry> entries = new();

    public IReadOnlyDictionary<int, CombatHistoryEntry> Entries => entries;

    public void Record(AttackResult result, int turnStep, int turnCount)
    {
        if (entries.TryGetValue(turnCount, out var entry))
        {
            entry.Results.Add(result);
            return;
        }

        entries[turnCount] = new CombatHistoryEntry
        {
            TurnStep = turnStep,
            Results = new List<AttackResult> { result },
        };
    }

    public void Clear()
    {
        entries.Clear();
    }

    public List<AttackResult> GetAttackResults(CombatantBase attacker, CombatantBase defender)
    {
        var matches = new List<AttackResult>();

        foreach (var entry in entries.Values)
        {
            if (entry.Results == null)
                continue;

            foreach (var result in entry.Results)
            {
                if (result.Attacker == attacker && result.Defender == defender)
                    matches.Add(result);
            }
        }

        return matches;
    }
}
