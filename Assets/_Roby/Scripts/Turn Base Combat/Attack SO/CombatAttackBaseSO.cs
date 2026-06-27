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
            var tempTimes = new List<float>();

            if (attackActionEntries != null)
            {
                foreach (var attackAction in attackActionEntries)
                    tempTimes.Add(attackAction?.AttackActionParameter?.MaxTime ?? 0f);
            }

            HitEntry.Set_EditorData(tempTimes);
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
    
    [PropertyRange(0, "AttackActionCount")]
    public int attackActionIndex;

    [PropertyRange(0, "MaxTime")]
    [SuffixLabel("seconds")]
    public float timeToCall;

    string Label => $"{damageProportion}% | {attackActionIndex} | {timeToCall}s";

#if UNITY_EDITOR
    float MaxTime
    {
        get
        {
            if (Times == null || attackActionIndex < 0 || attackActionIndex >= Times.Count)
                return 0;
            
            return Times[attackActionIndex];
        }
    }

    static List<float> Times;
    static int AttackActionCount => Times != null ? Times.Count - 1 : 0;

    public static void Set_EditorData(List<float> times)
    {
        Times = times;
    }
#endif
}