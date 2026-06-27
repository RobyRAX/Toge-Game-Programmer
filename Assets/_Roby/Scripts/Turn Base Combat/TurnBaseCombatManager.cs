using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Utility;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class TurnBaseCombatManager : Singleton<TurnBaseCombatManager>
{
    [TitleGroup("Formation")]
    public CinemachineTargetGroup targetGroup;

    [TitleGroup("Formation")]
    public int maxMemberPerTeam;

    [TitleGroup("Formation")]
    public int distancePerTeamUnit;

    [TitleGroup("Formation")]
    public int twoTeamDistance;

    [TitleGroup("Formation")]
    public Transform formationRoot;

    [TitleGroup("Formation")]
    public Transform heroRoot;

    [TitleGroup("Formation")]
    public Transform enemyRoot;

    [HorizontalGroup("Formation/Pos")]
    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]
    public Transform[] HeroPositions;

    [HorizontalGroup("Formation/Pos")]
    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]
    public Transform[] EnemyPositions;

    [FoldoutGroup("Formation/Gizmos")]
    public bool drawFormationGizmos = true;

    [FoldoutGroup("Formation/Gizmos")]
    [MinValue(0)]
    public float gizmoSlotRadius = 0.25f;

    [FoldoutGroup("Formation/Gizmos")]
    [MinValue(0)]
    public float gizmoRootRadius = 0.35f;

    [FoldoutGroup("Formation/Ground Snap")]
    public LayerMask groundLayerMask;

    [FoldoutGroup("Formation/Ground Snap")]
    [MinValue(0)]
    public float groundRaycastUpDistance = 5f;

    [FoldoutGroup("Formation/Ground Snap")]
    [MinValue(0)]
    public float groundRaycastDownDistance = 10f;

    [FoldoutGroup("Formation/Ground Snap")]
    public float groundSnapYOffset;
    
    [TitleGroup("Formation")]
    [Button]
    void InitFormationPositions()
    {
        if (formationRoot == null)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: {nameof(formationRoot)} is null.");
            return;
        }

        if (heroRoot == null)
        {
            heroRoot = EnsureChildRoot(formationRoot, nameof(heroRoot));
        }

        if (enemyRoot == null)
        {
            enemyRoot = EnsureChildRoot(formationRoot, nameof(enemyRoot));
        }

        // Put the two teams apart (symmetrical around formation root).
        heroRoot.localPosition = Vector3.left * (twoTeamDistance * 0.5f);
        enemyRoot.localPosition = Vector3.right * (twoTeamDistance * 0.5f);

        RecreateChildren(heroRoot);
        RecreateChildren(enemyRoot);

        int count = Mathf.Max(0, maxMemberPerTeam);
        HeroPositions = new Transform[count];
        EnemyPositions = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            // Index 0 at center, then alternate outward (+Z, -Z, +2Z, -2Z, ...).
            float z = TurnBaseCombatHelper.GetCenterOutOffset(i, distancePerTeamUnit);

            var heroSlot = new GameObject($"HeroPos_{i:00}").transform;
            heroSlot.SetParent(heroRoot, false);
            heroSlot.localPosition = new Vector3(0f, 0f, z);
            HeroPositions[i] = heroSlot;

            var enemySlot = new GameObject($"EnemyPos_{i:00}").transform;
            enemySlot.SetParent(enemyRoot, false);
            enemySlot.localPosition = new Vector3(0f, 0f, z);
            EnemyPositions[i] = enemySlot;
        }
    }

    static Transform EnsureChildRoot(Transform parent, string rootName)
    {
        var existing = parent.Find(rootName);
        if (existing != null)
            return existing;

        var go = new GameObject(rootName);
        var t = go.transform;
        t.SetParent(parent, false);
        return t;
    }

    static void RecreateChildren(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            var child = root.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (!drawFormationGizmos)
            return;

        if (formationRoot != null)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.9f);
            Gizmos.DrawWireSphere(formationRoot.position, gizmoRootRadius);
        }

        DrawTeamGizmos(heroRoot, HeroPositions, new Color(0.2f, 0.8f, 1f, 0.9f)); // cyan-ish
        DrawTeamGizmos(enemyRoot, EnemyPositions, new Color(1f, 0.35f, 0.35f, 0.9f)); // red-ish
    }

    void DrawTeamGizmos(Transform teamRoot, Transform[] slots, Color color)
    {
        if (teamRoot == null)
            return;

        Gizmos.color = color;
        Gizmos.DrawWireSphere(teamRoot.position, gizmoRootRadius);

        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            var t = slots[i];
            if (t == null)
                continue;

            Gizmos.DrawSphere(t.position, gizmoSlotRadius);
            Gizmos.DrawLine(teamRoot.position, t.position);
            DrawGroundRayGizmo(t.position);
        }
    }

    void DrawGroundRayGizmo(Vector3 probe)
    {
        Gizmos.color = new Color(1f, 1f, 0.2f, 0.7f);
        Gizmos.DrawLine(probe, probe + Vector3.up * groundRaycastUpDistance);
        Gizmos.DrawLine(probe, probe + Vector3.down * groundRaycastDownDistance);

        if (!TurnBaseCombatHelper.TrySnapToGround(probe,
                                                  groundRaycastUpDistance,
                                                  groundRaycastDownDistance,
                                                  groundLayerMask,
                                                  groundSnapYOffset,
                                                  out Vector3 snapped))
            return;

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(snapped, gizmoSlotRadius * 0.5f);
    }

    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> AllCombatants { get; set; }

    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> HeroCombatants { get; set; }

    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> EnemyCombatants { get; set; }

    [TitleGroup("Current Combatant Attack Queue")]
    [ShowInInspector]
    public CombatantBase CurrentCombatant => TurnTimeline.CurrentTurnCombatant;

    [TitleGroup("Current Combatant Attack Queue")]
    [ShowInInspector]
    public Attack_Runtime SelectedAttack { get; set; }

    [TitleGroup("Current Combatant Attack Queue")]
    [ShowInInspector]
    public CombatantBase TargetOpponent { get; set; }

    [TitleGroup("Current Combatant Attack Queue")]
    [ShowInInspector]
    public CombatantBase TargetTeam { get; set; }

    [TitleGroup("Current Combatant Attack Queue")]
    [Button]
    public void TakeRandomAttack()
    {
        if (CurrentCombatant == null)
            return;
        
        var attacks = CurrentCombatant.AttackBank?.Attacks;
        if (attacks == null || attacks.Count == 0)
            return;

        SelectedAttack = attacks[Random.Range(0, attacks.Count)];
    }


    [TitleGroup("Current Combatant Attack Queue")]
    [Button]
    public void ExecuteAttack()
    {
        var attackReq = TurnBaseCombatHelper.
                        BuildAttackRequest(CurrentCombatant, 
                                            TargetOpponent, 
                                            SelectedAttack.damageProfile);

        TurnBaseCombatHelper.SendAttack(attackReq, out AttackResult attackRes);

        if (CurrentCombatant == null || SelectedAttack == null)
            return;

        SelectedAttack.ExecuteAttackActionSequenceAsync(TargetOpponent, TargetTeam).Forget();
    }

    [TitleGroup("Turn Timeline")]
    [BoxGroup("Turn Timeline/Turn Timeline")]
    [HideLabel]
    public TurnTimeline TurnTimeline;

    [TitleGroup("Debug Functions")]
    [Button]
    public void StartCombat(List<CombatantBase> heroCombatants,
                            List<CombatantBase> enemyCombatants,
                            CombatantBase initialTurn)
    {
        if (formationRoot == null)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: {nameof(formationRoot)} is null.");
            return;
        }

        HeroCombatants = TurnBaseCombatHelper.FilterNullCombatants(heroCombatants);
        EnemyCombatants = TurnBaseCombatHelper.FilterNullCombatants(enemyCombatants);

        if (HeroCombatants.Count == 0 || EnemyCombatants.Count == 0)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: Need at least one hero and one enemy combatant.");
            return;
        }

        int neededSlots = Mathf.Max(HeroCombatants.Count, EnemyCombatants.Count);
        if (neededSlots > maxMemberPerTeam)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: Combatant count ({neededSlots}) exceeds {nameof(maxMemberPerTeam)} ({maxMemberPerTeam}). Extra units will be skipped.");
        }

        if (!EnsureFormationSlots())
            return;

        if (!TurnBaseCombatHelper.TryGetAveragePosition(HeroCombatants, out Vector3 heroAvg) ||
            !TurnBaseCombatHelper.TryGetAveragePosition(EnemyCombatants, out Vector3 enemyAvg))
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: Failed to compute team average positions.");
            return;
        }

        AlignFormationRoot(heroAvg, enemyAvg);
        SnapSlotsToGround(HeroPositions);
        SnapSlotsToGround(EnemyPositions);

        Quaternion? heroFacing = null;
        Quaternion? enemyFacing = null;
        if (TurnBaseCombatHelper.TryGetCombatFacingRotations(heroAvg, enemyAvg, out Quaternion heroRot, out Quaternion enemyRot))
        {
            heroFacing = heroRot;
            enemyFacing = enemyRot;
        }

        PlaceCombatantsOnSlots(HeroCombatants, HeroPositions, heroFacing);
        PlaceCombatantsOnSlots(EnemyCombatants, EnemyPositions, enemyFacing);

        AllCombatants = new List<CombatantBase>(HeroCombatants.Count + EnemyCombatants.Count);
        AllCombatants.AddRange(HeroCombatants);
        AllCombatants.AddRange(EnemyCombatants);

        foreach (var combatant in AllCombatants)
            combatant?.SetExplorationMovementEnabled(false);

        if (TurnTimeline == null)
            TurnTimeline = new TurnTimeline();

        TurnTimeline.Initialize(AllCombatants, initialTurn);
        BeginCurrentTurn();

        Debug.Log("Combat Started");
    }

    bool EnsureFormationSlots()
    {
        if (maxMemberPerTeam <= 0)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: {nameof(maxMemberPerTeam)} must be greater than 0.");
            return false;
        }

        InitFormationPositions();

        if (HeroPositions == null || HeroPositions.Length == 0 ||
            EnemyPositions == null || EnemyPositions.Length == 0)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: Formation slots are not ready.");
            return false;
        }

        return true;
    }

    void AlignFormationRoot(Vector3 heroAvg, Vector3 enemyAvg)
    {
        formationRoot.position = (heroAvg + enemyAvg) * 0.5f;

        Vector3 flatDir = enemyAvg - heroAvg;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude < 0.0001f)
            return;

        formationRoot.rotation = Quaternion.FromToRotation(Vector3.right, flatDir.normalized);
    }

    void SnapSlotsToGround(Transform[] slots)
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null)
                continue;

            Vector3 probe = slot.position;
            if (TurnBaseCombatHelper.TrySnapToGround(probe,
                                                     groundRaycastUpDistance,
                                                     groundRaycastDownDistance,
                                                     groundLayerMask,
                                                     groundSnapYOffset,
                                                     out Vector3 snapped))
            {
                slot.position = snapped;
                continue;
            }

            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: No ground hit for slot '{slot.name}' at {probe}.");
        }
    }

    void PlaceCombatantsOnSlots(List<CombatantBase> combatants, Transform[] slots, Quaternion? facing = null)
    {
        if (combatants == null || slots == null)
            return;

        int count = Mathf.Min(combatants.Count, slots.Length);
        for (int i = 0; i < count; i++)
        {
            var combatant = combatants[i];
            var slot = slots[i];
            if (combatant == null || slot == null)
                continue;

            if (facing.HasValue)
                slot.rotation = facing.Value;

            TurnBaseCombatHelper.TeleportTo(combatant.transform, slot.position, facing);
            combatant.SetFormationSlot(slot);
        }
    }

    void BeginCurrentTurn()
    {
        if (TurnTimeline == null || !TurnTimeline.IsInitialized)
            return;

        if (TryAssignCurrentTurnCombatant())
            return;

        AdvanceTimelineUntilTurnFound();
    }

    bool TryAssignCurrentTurnCombatant()
    {
        var dueCombatants = TurnTimeline.GetCombatantsAtCurrentStep();
        if (dueCombatants.Count == 0)
        {
            TurnTimeline.CurrentTurnCombatant = null;
            return false;
        }

        TurnTimeline.CurrentTurnCombatant = dueCombatants[0];
        Debug.Log($"{nameof(TurnBaseCombatManager)}: Current turn -> {TurnTimeline.CurrentTurnCombatant.name} (Step {TurnTimeline.CurrentStep}).");
        return true;
    }

    void AdvanceTimelineUntilTurnFound()
    {
        const int maxStepScans = 1000;

        for (int i = 0; i < maxStepScans; i++)
        {
            TurnTimeline.AddStep();
            if (TryAssignCurrentTurnCombatant())
                return;
        }

        Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: No combatant found within {maxStepScans} timeline steps.");
    }

    [TitleGroup("Debug Functions")]
    [Button]
    void ResolveCurrentTurn()
    {
        if (TurnTimeline == null || !TurnTimeline.IsInitialized)
        {
            Debug.Log($"{nameof(TurnBaseCombatManager)}: Turn timeline is not initialized.");
            return;
        }

        if (TurnTimeline.CurrentTurnCombatant == null)
        {
            Debug.Log($"{nameof(TurnBaseCombatManager)}: No active turn to resolve.");
            BeginCurrentTurn();
            return;
        }

        var actingCombatant = TurnTimeline.CurrentTurnCombatant;
        TurnTimeline.NotifyCombatantActed(actingCombatant);
        TurnTimeline.CurrentTurnCombatant = null;

        Debug.Log($"{nameof(TurnBaseCombatManager)}: Resolved turn for {actingCombatant.name} at step {TurnTimeline.CurrentStep}.");
        BeginCurrentTurn();
    }

    [TitleGroup("Debug Functions")]
    [Button]
    void AdvanceTimelineStep()
    {
        if (TurnTimeline == null || !TurnTimeline.IsInitialized)
        {
            Debug.Log($"{nameof(TurnBaseCombatManager)}: Turn timeline is not initialized.");
            return;
        }

        TurnTimeline.AddStep();
        Debug.Log($"{nameof(TurnBaseCombatManager)}: Advanced to step {TurnTimeline.CurrentStep}.");
    }

    [TitleGroup("Debug Functions")]
    [Button]
    void LogTimelinePreview()
    {
        if (TurnTimeline == null || !TurnTimeline.IsInitialized)
        {
            Debug.Log($"{nameof(TurnBaseCombatManager)}: Turn timeline is not initialized.");
            return;
        }

        for (int i = 0; i < TurnTimeline.PreviewSteps.Count; i++)
        {
            var turnStep = TurnTimeline.PreviewSteps[i];
            if (turnStep.Combatants == null || turnStep.Combatants.Count == 0)
                continue;

            for (int j = 0; j < turnStep.Combatants.Count; j++)
            {
                var combatant = turnStep.Combatants[j];
                float speed = TurnTimeline.CombatantSpeedDict[combatant];
                Debug.Log($"[Timeline Preview] Step {turnStep.Step}: {combatant.name} (Speed {speed})");
            }
        }
    }
}
