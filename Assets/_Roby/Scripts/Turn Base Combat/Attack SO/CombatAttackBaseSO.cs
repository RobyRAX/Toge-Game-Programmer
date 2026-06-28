using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

public abstract class CombatAttackBaseSO : ScriptableObject
{
    public bool targetOpponent = true;
    public bool targetTeam;
    public int staminaCost;

    [TitleGroup("Display")]
    public string displayName;

    [TitleGroup("Display")]
    public Sprite displayIcon;

    public string GetDisplayName() => string.IsNullOrEmpty(displayName) ? name : displayName;

    [TitleGroup("Attack Action Sequence")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label", Expanded = true)]
    public List<AttackActionEntry> attackActionEntries;

    [TitleGroup("Attack Hit Config")]
    [ListDrawerSettings(OnTitleBarGUI = "DrawRefreshButton_HitEntries", ShowIndexLabels = true, ListElementLabelName = "Label")]
    [InfoBox("Damage Proportion isn't 100% in total", InfoMessageType.Warning, "ValidateHitEntries")]
    public List<HitEntry> hitEntries;

    public abstract DamageProfileWithAttribute DamageProfile { get; }

#if UNITY_EDITOR
    void DrawRefreshButton_HitEntries()
    {
        if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
        {
            float totalTime = 0f;

            if (attackActionEntries != null)
            {
                foreach (var attackAction in attackActionEntries)
                    totalTime += attackAction?.AttackActionParameter?.MaxTime ?? 0f;
            }

            HitEntry.Set_EditorData(totalTime);
        }
    }

    bool ValidateHitEntries
    {
        get
        {
            if (hitEntries == null || hitEntries.Count == 0)
                return false;

            float total = 0f;
            foreach (var hitEntry in hitEntries)
                total += hitEntry?.damageProportion ?? 0f;

            return !Mathf.Approximately(total, 100f);
        }
    }
#endif
}

[Serializable]
public class HitEntry
{
    [Range(0, 100)]
    [SuffixLabel("%")]
    public float damageProportion;

    [PropertyRange(0, "MaxTime")]
    [SuffixLabel("seconds")]
    public float timeToCall;

    string Label => $"{damageProportion}% @ {timeToCall}s";

#if UNITY_EDITOR
    float MaxTime => TotalAttackDuration;

    static float TotalAttackDuration;

    public static void Set_EditorData(float totalAttackDuration)
    {
        TotalAttackDuration = totalAttackDuration;
    }
#endif
}