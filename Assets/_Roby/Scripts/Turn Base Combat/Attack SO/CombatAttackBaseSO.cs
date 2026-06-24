using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class CombatAttackBaseSO : ScriptableObject
{
    public int staminaCost;

    [TitleGroup("Attack Hit Config")]
    public List<HitEntry> hitEntries;

    [TitleGroup("Attack Action Sequence")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label", Expanded = true)]
    public List<AttackActionEntry> attackActionEntries;

    public abstract DamageProfileWithAttribute DamageProfile { get; }
}

[Serializable]
public class HitEntry
{
    [Range(0, 100)]
    [SuffixLabel("%")]
    public float damageProportion;
}