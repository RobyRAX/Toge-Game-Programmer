using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class TurnTimeline
{
    public int StepPreviewRange = 20;

    public float BaseStepInterval = TurnBaseCombatHelper.TimelineBaseStepInterval;

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public CombatantBase InitialTurnCombatant { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public CombatantBase CurrentTurnCombatant { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public Dictionary<CombatantBase, float> CombatantSpeedDict { get; set; } = new();

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public Dictionary<CombatantBase, int> CombatantIntervalDict { get; set; } = new();

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public Dictionary<CombatantBase, int> CombatantNextStepDict { get; set; } = new();

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public Dictionary<CombatantBase, int> CombatantOrderDict { get; set; } = new();

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public int CurrentStep { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public Vector2 CurrentStepPreviewRange { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public List<TurnStep> PreviewSteps { get; set; } = new();

    public bool IsInitialized => CombatantSpeedDict.Count > 0;

    public void Initialize(List<CombatantBase> combatants, CombatantBase initialTurn)
    {
        CombatantSpeedDict.Clear();
        CombatantIntervalDict.Clear();
        CombatantNextStepDict.Clear();
        CombatantOrderDict.Clear();
        PreviewSteps.Clear();
        CurrentStep = 0;
        InitialTurnCombatant = initialTurn;
        CurrentTurnCombatant = null;

        if (combatants == null || combatants.Count == 0)
        {
            UpdatePreviewSteps();
            return;
        }

        if (initialTurn != null && !combatants.Contains(initialTurn))
        {
            Debug.LogWarning($"{nameof(TurnTimeline)}: Initial turn combatant '{initialTurn.name}' is not in the combatant list.");
            InitialTurnCombatant = null;
        }

        int registrationOrder = 0;
        for (int i = 0; i < combatants.Count; i++)
        {
            var combatant = combatants[i];
            if (combatant == null)
                continue;

            if (combatant.StatContainer == null)
            {
                Debug.LogWarning($"{nameof(TurnTimeline)}: '{combatant.name}' has no StatContainer. Skipped.");
                continue;
            }

            float speed = TurnBaseCombatHelper.GetCombatantAttackSpeed(combatant);
            int interval = TurnBaseCombatHelper.CalculateStepInterval(speed, BaseStepInterval);

            CombatantSpeedDict[combatant] = speed;
            CombatantIntervalDict[combatant] = interval;
            CombatantNextStepDict[combatant] = combatant == InitialTurnCombatant ? 0 : interval;
            CombatantOrderDict[combatant] = registrationOrder;
            registrationOrder++;
        }

        UpdatePreviewSteps();
    }

    public List<CombatantBase> GetCombatantsAtCurrentStep()
    {
        return CollectDueCombatants(CombatantNextStepDict);
    }

    public void NotifyCombatantActed(CombatantBase combatant)
    {
        if (combatant == null || !CombatantNextStepDict.ContainsKey(combatant))
            return;

        int interval = CombatantIntervalDict[combatant];
        CombatantNextStepDict[combatant] = TurnBaseCombatHelper.CalculateNextTurnStep(CurrentStep, interval);
        UpdatePreviewSteps();
    }

    public void RemoveCombatant(CombatantBase combatant)
    {
        if (combatant == null)
            return;

        CombatantSpeedDict.Remove(combatant);
        CombatantIntervalDict.Remove(combatant);
        CombatantNextStepDict.Remove(combatant);
        CombatantOrderDict.Remove(combatant);

        if (CurrentTurnCombatant == combatant)
            CurrentTurnCombatant = null;

        UpdatePreviewSteps();
    }

    public void RefreshCombatant(CombatantBase combatant)
    {
        if (combatant == null || !CombatantSpeedDict.ContainsKey(combatant))
            return;

        float speed = TurnBaseCombatHelper.GetCombatantAttackSpeed(combatant);
        CombatantSpeedDict[combatant] = speed;
        CombatantIntervalDict[combatant] = TurnBaseCombatHelper.CalculateStepInterval(speed, BaseStepInterval);
        UpdatePreviewSteps();
    }

    public void AddStep()
    {
        CurrentStep++;
        UpdatePreviewSteps();
    }

    public void UpdatePreviewSteps()
    {
        CurrentStepPreviewRange = new Vector2(CurrentStep, CurrentStep + StepPreviewRange);
        PreviewSteps.Clear();

        if (!IsInitialized)
            return;

        var simulatedNextSteps = CloneNextStepDict(CombatantNextStepDict);
        int previewEnd = CurrentStep + StepPreviewRange;

        for (int step = CurrentStep; step <= previewEnd; step++)
        {
            var dueCombatants = CollectDueCombatants(simulatedNextSteps, step);

            PreviewSteps.Add(new TurnStep
            {
                Step = step,
                Combatants = dueCombatants,
            });

            for (int i = 0; i < dueCombatants.Count; i++)
            {
                var combatant = dueCombatants[i];
                int interval = CombatantIntervalDict[combatant];
                simulatedNextSteps[combatant] = TurnBaseCombatHelper.CalculateNextTurnStep(step, interval);
            }
        }
    }

    static Dictionary<CombatantBase, int> CloneNextStepDict(Dictionary<CombatantBase, int> source)
    {
        return new Dictionary<CombatantBase, int>(source);
    }

    List<CombatantBase> CollectDueCombatants(Dictionary<CombatantBase, int> nextStepDict, int? step = null)
    {
        int targetStep = step ?? CurrentStep;
        var due = new List<CombatantBase>();

        foreach (var pair in nextStepDict)
        {
            if (pair.Value != targetStep)
                continue;

            if (pair.Key == null || !pair.Key.IsAlive)
                continue;

            due.Add(pair.Key);
        }

        due.Sort(CompareCombatantPriority);
        return due;
    }

    int CompareCombatantPriority(CombatantBase a, CombatantBase b)
    {
        float speedA = CombatantSpeedDict[a];
        float speedB = CombatantSpeedDict[b];
        int orderA = CombatantOrderDict[a];
        int orderB = CombatantOrderDict[b];
        return TurnBaseCombatHelper.CompareTimelineTurnPriority(speedA, orderA, speedB, orderB);
    }
}

[Serializable]
public struct TurnStep
{
    public int Step;
    public List<CombatantBase> Combatants;
}
