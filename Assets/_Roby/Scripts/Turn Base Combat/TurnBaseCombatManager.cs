using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Utility;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;
using Random = UnityEngine.Random;

public class TurnBaseCombatManager : Singleton<TurnBaseCombatManager>
{
    #region State Machine

    [TitleGroup("State")]
    [ShowInInspector]
    [ReadOnly]
    public TurnBaseCombatPhase CurrentPhase => StateMachine?.CurrentPhase ?? default;

    [TitleGroup("State")]
    [ShowInInspector]
    [ReadOnly]
    public TurnSide CurrentTurnSide { get; private set; }

    public bool IsPlayerTurn => CurrentTurnSide == TurnSide.Player;

    [TitleGroup("State")]
    [ShowInInspector]
    [ReadOnly]
    public TurnSide WinningSide { get; private set; }

    public bool IsCombatOver => CurrentPhase == TurnBaseCombatPhase.EndCombat;

    public TurnBaseCombatStateMachine StateMachine { get; private set; }

    public event Action<TurnBaseCombatPhase> OnPhaseChanged;
    public event Action OnTurnAdvanced;
    public event Action OnTimelinePreviewUpdated;
    public event Action<CombatantBase> OnCombatantStatsChanged;
    public event Action<CombatHitInfo> OnCombatantDoHit;
    public event Action OnCombatStarted;
    public event Action<TurnSide> OnCombatEnded;

    public bool CanAffordAttack(CombatantBase combatant, Attack_Runtime attack)
    {
        if (combatant == null || attack == null)
            return false;

        if (combatant.CurrentStamina < attack.StaminaCost)
            return false;

        if (attack.IsUltimateAttack && combatant is HeroCombatant hero && !hero.HasFullUltimateGauge)
            return false;

        return true;
    }

    public bool HasValidAttackSelection()
    {
        return CurrentCombatant != null && SelectedAttack != null;
    }

    bool AttackTargetsOpponent => SelectedAttack?.AttackSO != null && SelectedAttack.AttackSO.targetOpponent;
    bool AttackTargetsTeam => SelectedAttack?.AttackSO != null && SelectedAttack.AttackSO.targetTeam;

    public TurnBaseCombatPhase GetFirstTargetPhase()
    {
        if (AttackTargetsOpponent)
            return TurnBaseCombatPhase.SelectTargetOpponent;

        if (AttackTargetsTeam)
            return TurnBaseCombatPhase.SelectTargetTeam;

        return TurnBaseCombatPhase.Attack;
    }

    public TurnBaseCombatPhase GetPhaseAfterOpponentTarget()
    {
        return AttackTargetsTeam ? TurnBaseCombatPhase.SelectTargetTeam : TurnBaseCombatPhase.Attack;
    }

    public void SubmitSelectedAttack(Attack_Runtime attack)
    {
        if (!IsPlayerTurn || StateMachine?.CurrentPhase != TurnBaseCombatPhase.SelectAttack)
            return;

        if (attack == null)
            return;

        if (!CanAffordAttack(CurrentCombatant, attack))
            return;

        SelectedAttack = attack;
        StateMachine.ChangePhase(GetFirstTargetPhase());
    }

    public void SubmitTargetOpponent(CombatantBase target)
    {
        if (!IsPlayerTurn || StateMachine?.CurrentPhase != TurnBaseCombatPhase.SelectTargetOpponent)
            return;

        TargetOpponent = target;
        StateMachine.ChangePhase(GetPhaseAfterOpponentTarget());
    }

    public void SubmitTargetTeam(CombatantBase target)
    {
        if (!IsPlayerTurn || StateMachine?.CurrentPhase != TurnBaseCombatPhase.SelectTargetTeam)
            return;

        TargetTeam = target;
        StateMachine.ChangePhase(TurnBaseCombatPhase.Attack);
    }

    internal void PickRandomAttackForCurrentCombatant()
    {
        if (CurrentCombatant == null)
            return;

        var attacks = CurrentCombatant.AttackBank?.Attacks;
        if (attacks == null || attacks.Count == 0)
            return;

        var affordable = new List<Attack_Runtime>();
        foreach (var attack in attacks)
        {
            if (attack != null && CanAffordAttack(CurrentCombatant, attack))
                affordable.Add(attack);
        }

        if (affordable.Count == 0)
            return;

        SelectedAttack = affordable[Random.Range(0, affordable.Count)];
    }

    internal void AutoPickOpponentTarget()
    {
        TargetOpponent = PickRandomOpponentForCurrentCombatant();
    }

    internal void AutoPickTeamTarget()
    {
        TargetTeam = PickRandomTeamTargetForCurrentCombatant();
    }

    void EnterTurnPhaseForCurrentCombatant()
    {
        if (TurnTimeline?.CurrentTurnCombatant == null)
            return;

        bool isPlayer = PlayerCombatants != null &&
                        PlayerCombatants.Contains(TurnTimeline.CurrentTurnCombatant);

        CurrentTurnSide = isPlayer ? TurnSide.Player : TurnSide.Enemy;
        StateMachine.ChangePhase(TurnBaseCombatPhase.BeginTurn);
    }

    #endregion

    [TitleGroup("UI")]
    [SerializeField]
    CombatUI combatUI;

    [TitleGroup("Celebration")]
    [SerializeField]
    [MinValue(0)]
    float winCelebrationDelay = 0.5f;

    [TitleGroup("Celebration")]
    [SerializeField]
    [MinValue(0)]
    float winCelebrationDuration = 2f;

    [TitleGroup("Camera")]
    public CombatCameraDirector CameraDirector;

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

    [TitleGroup("Formation")]
    [MinValue(0)]
    public Vector2 formationMoveDurationRange = new Vector2(0.4f, 0.8f);

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
    public List<CombatantBase> PlayerCombatants { get; set; }

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

    protected override void Awake()
    {
        base.Awake();

        combatUI?.Shutdown();
    }

    [HorizontalGroup("Current Combatant Attack Queue/Op")]
    [Button]
    public void TakeRandomAttack()
    {
        PickRandomAttackForCurrentCombatant();

        if (SelectedAttack != null)
            SubmitSelectedAttack(SelectedAttack);
    }

    [HorizontalGroup("Current Combatant Attack Queue/Op")]
    [Button]
    public void TakeRandomTarget()
    {
        if (CurrentCombatant == null)
            return;

        if (CurrentPhase == TurnBaseCombatPhase.SelectTargetOpponent)
        {
            SubmitTargetOpponent(PickRandomOpponentForCurrentCombatant());

            if (CurrentPhase == TurnBaseCombatPhase.SelectTargetTeam)
                SubmitTargetTeam(PickRandomTeamTargetForCurrentCombatant());

            return;
        }

        if (CurrentPhase == TurnBaseCombatPhase.SelectTargetTeam)
            SubmitTargetTeam(PickRandomTeamTargetForCurrentCombatant());
    }

    CombatantBase PickRandomOpponentForCurrentCombatant()
    {
        if (PlayerCombatants != null && PlayerCombatants.Contains(CurrentCombatant))
            return PickRandomAliveCombatant(EnemyCombatants);

        if (EnemyCombatants != null && EnemyCombatants.Contains(CurrentCombatant))
            return PickRandomAliveCombatant(PlayerCombatants);

        return null;
    }

    CombatantBase PickRandomTeamTargetForCurrentCombatant()
    {
        if (PlayerCombatants != null && PlayerCombatants.Contains(CurrentCombatant))
            return PickRandomAliveCombatant(PlayerCombatants);

        if (EnemyCombatants != null && EnemyCombatants.Contains(CurrentCombatant))
            return PickRandomAliveCombatant(EnemyCombatants);

        return null;
    }

    static CombatantBase PickRandomAliveCombatant(List<CombatantBase> combatants)
    {
        if (combatants == null || combatants.Count == 0)
            return null;

        var alive = new List<CombatantBase>(combatants.Count);
        foreach (var combatant in combatants)
        {
            if (combatant != null && combatant.IsAlive)
                alive.Add(combatant);
        }

        if (alive.Count == 0)
            return null;

        return alive[Random.Range(0, alive.Count)];
    }

    [HorizontalGroup("Current Combatant Attack Queue/Op2")]
    [Button]
    void TestAction()
    {
        if (CurrentCombatant == null || SelectedAttack == null)
            return;

        CurrentCombatant.ExecuteAttack(SelectedAttack, TargetOpponent, TargetTeam).Forget();
    }

    [HorizontalGroup("Current Combatant Attack Queue/Op2")]
    [Button]
    void DebugExecuteAttack()
    {
        if (CurrentPhase == TurnBaseCombatPhase.SelectTargetTeam)
            SubmitTargetTeam(TargetTeam ?? PickRandomTeamTargetForCurrentCombatant());
    }

    public async UniTask ExecuteAttack()
    {
        if (CurrentCombatant == null || SelectedAttack == null)
            return;

        var attackReq = TurnBaseCombatHelper.
                        BuildAttackRequest(CurrentCombatant,
                                            TargetOpponent,
                                            SelectedAttack.damageProfile);

        TurnBaseCombatHelper.SendAttack(attackReq, out AttackResult attackRes);
        History?.Record(attackRes, TurnTimeline != null ? TurnTimeline.CurrentStep : 0, CurrentTurnCount);

        if (attackRes.IsDefenderDead)
            HandleCombatantDeath(attackRes.Defender);

        await CurrentCombatant.ExecuteAttack(SelectedAttack, TargetOpponent, TargetTeam, attackRes);

        if (attackRes.IsDefenderDead && attackRes.Defender?.StateMachine != null)
            await attackRes.Defender.StateMachine.WaitForDeathAsync();

        DeductStaminaForCurrentAttack();
        SpendUltimateGaugeForCurrentAttack();
        GainUltimateGaugeForCurrentCombatant();

        if (StateMachine?.CurrentPhase == TurnBaseCombatPhase.Attack)
            StateMachine.ChangePhase(TurnBaseCombatPhase.EndTurn);
        else
            CompleteCurrentTurn();
    }

    void HandleCombatantDeath(CombatantBase combatant)
    {
        if (combatant == null)
            return;

        TurnTimeline?.RemoveCombatant(combatant);
        RaiseTimelinePreviewUpdated();

        if (TurnTimeline?.CurrentTurnCombatant == combatant)
        {
            TurnTimeline.CurrentTurnCombatant = null;
            AdvanceTimelineUntilTurnFound();
        }
    }

    public UniTask ResolveEndTurnAsync()
    {
        CompleteCurrentTurn();
        return UniTask.CompletedTask;
    }

    public async UniTask ResolveEndCombatAsync()
    {
        if (WinningSide == TurnSide.Player)
            CameraDirector?.FocusOnPlayerTeam();
        else
            CameraDirector?.FocusOnDefault();

        await PlayWinningSideCelebrationAsync();

        Debug.Log($"{nameof(TurnBaseCombatManager)}: Combat ended. Winner: {WinningSide}.");
        RaiseCombatEnded();
        EndCombatCleanup();
    }

    async UniTask PlayWinningSideCelebrationAsync()
    {
        var winners = WinningSide switch
        {
            TurnSide.Player => PlayerCombatants,
            TurnSide.Enemy => EnemyCombatants,
            _ => null
        };

        if (winners == null)
            return;

        if (winCelebrationDelay > 0f)
            await UniTask.WaitForSeconds(winCelebrationDelay);

        bool anyCelebrating = false;
        foreach (var combatant in winners)
        {
            if (combatant == null || !combatant.IsAlive || combatant.StateMachine == null)
                continue;

            combatant.StateMachine.PlayWin();
            anyCelebrating = true;
        }

        if (anyCelebrating && winCelebrationDuration > 0f)
            await UniTask.WaitForSeconds(winCelebrationDuration);
    }

    void EndCombatCleanup()
    {
        UnsubscribeCombatantStatsEvents();

        if (PlayerCombatants != null)
        {
            foreach (var combatant in PlayerCombatants)
            {
                if (combatant != null && combatant.IsAlive)
                    combatant.SetExplorationEnabled(true);
            }
        }

        combatUI?.Shutdown();

        if (TurnTimeline != null)
            TurnTimeline.CurrentTurnCombatant = null;

        PlayerCombatants = null;
        EnemyCombatants = null;
        AllCombatants = null;
        SelectedAttack = null;
        TargetOpponent = null;
        TargetTeam = null;
    }

    void DeductStaminaForCurrentAttack()
    {
        if (CurrentCombatant == null || SelectedAttack == null)
            return;

        CurrentCombatant.CurrentStamina = Mathf.Max(0f, CurrentCombatant.CurrentStamina - SelectedAttack.StaminaCost);
    }

    internal void RegenStaminaForCurrentCombatant()
    {
        var combatant = CurrentCombatant;
        if (combatant == null || combatant.StatContainer == null)
            return;

        float regen = combatant.StatContainer.GetTotalValue(StatAttribute.StaminaRegen);
        if (regen <= 0f)
            return;

        float maxStamina = combatant.StatContainer.GetTotalValue(StatAttribute.MaxStamina);
        combatant.CurrentStamina = Mathf.Min(maxStamina, combatant.CurrentStamina + regen);
    }

    void SpendUltimateGaugeForCurrentAttack()
    {
        if (CurrentCombatant is HeroCombatant hero && SelectedAttack?.IsUltimateAttack == true)
            hero.SpendUltimateGauge();
    }

    void GainUltimateGaugeForCurrentCombatant()
    {
        if (SelectedAttack?.IsUltimateAttack == true)
            return;

        if (CurrentCombatant is HeroCombatant hero && SelectedAttack != null)
            hero.AddUltimateGauge(SelectedAttack.UltimateRegen);
    }

    internal void RaisePhaseChanged(TurnBaseCombatPhase phase) => OnPhaseChanged?.Invoke(phase);

    internal void RaiseTurnAdvanced()
    {
        OnTurnAdvanced?.Invoke();
        RaiseTimelinePreviewUpdated();
    }

    internal void RaiseTimelinePreviewUpdated() => OnTimelinePreviewUpdated?.Invoke();

    internal void RaiseCombatantStatsChanged(CombatantBase combatant) => OnCombatantStatsChanged?.Invoke(combatant);

    internal void RaiseCombatantDoHit(CombatHitInfo hitInfo) => OnCombatantDoHit?.Invoke(hitInfo);

    internal void RaiseCombatEnded() => OnCombatEnded?.Invoke(WinningSide);

    static bool IsTeamDefeated(List<CombatantBase> team)
    {
        if (team == null || team.Count == 0)
            return true;

        foreach (var combatant in team)
        {
            if (combatant != null && combatant.IsAlive)
                return false;
        }

        return true;
    }

    bool TryEndCombat()
    {
        bool playerDefeated = IsTeamDefeated(PlayerCombatants);
        bool enemyDefeated = IsTeamDefeated(EnemyCombatants);

        if (!playerDefeated && !enemyDefeated)
            return false;

        WinningSide = playerDefeated ? TurnSide.Enemy : TurnSide.Player;
        StateMachine.ChangePhase(TurnBaseCombatPhase.EndCombat);
        return true;
    }

    [TitleGroup("Turn Timeline")]
    [BoxGroup("Turn Timeline/Turn Timeline")]
    [HideLabel]
    public TurnTimeline TurnTimeline;

    [TitleGroup("Turn Timeline")]
    [ShowInInspector]
    [ReadOnly]
    public int CurrentTurnCount { get; private set; }

    [TitleGroup("Combat History")]
    [ShowInInspector]
    [HideReferenceObjectPicker]
    [HideLabel]
    public CombatHistory History { get; private set; }

    [TitleGroup("Debug Functions")]
    [Button]
    public void StartCombat(List<CombatantBase> playerCombatants,
                            List<CombatantBase> enemyCombatants,
                            CombatantBase initialTurn)
    {
        if (formationRoot == null)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: {nameof(formationRoot)} is null.");
            return;
        }

        PlayerCombatants = TurnBaseCombatHelper.FilterNullCombatants(playerCombatants);
        EnemyCombatants = TurnBaseCombatHelper.FilterNullCombatants(enemyCombatants);

        if (PlayerCombatants.Count == 0 || EnemyCombatants.Count == 0)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: Need at least one hero and one enemy combatant.");
            return;
        }

        int neededSlots = Mathf.Max(PlayerCombatants.Count, EnemyCombatants.Count);
        if (neededSlots > maxMemberPerTeam)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: Combatant count ({neededSlots}) exceeds {nameof(maxMemberPerTeam)} ({maxMemberPerTeam}). Extra units will be skipped.");
        }

        if (!EnsureFormationSlots())
            return;

        if (!TurnBaseCombatHelper.TryGetAveragePosition(PlayerCombatants, out Vector3 heroAvg) ||
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

        PlaceCombatantsOnSlots(PlayerCombatants, HeroPositions, heroFacing);
        PlaceCombatantsOnSlots(EnemyCombatants, EnemyPositions, enemyFacing);

        CameraDirector?.Setup(HeroPositions, EnemyPositions, PlayerCombatants);

        AllCombatants = new List<CombatantBase>(PlayerCombatants.Count + EnemyCombatants.Count);
        AllCombatants.AddRange(PlayerCombatants);
        AllCombatants.AddRange(EnemyCombatants);

        foreach (var combatant in AllCombatants)
        {
            combatant?.StateMachine?.ResetForCombat();
            combatant?.SetExplorationEnabled(false);
        }

        EnsureClickTargets();
        SubscribeCombatantStatsEvents();

        if (TurnTimeline == null)
            TurnTimeline = new TurnTimeline();

        if (History == null)
            History = new CombatHistory();
        else
            History.Clear();

        CurrentTurnCount = 0;
        CurrentTurnSide = TurnSide.None;
        WinningSide = TurnSide.None;

        StateMachine ??= new TurnBaseCombatStateMachine(this);
        TurnTimeline.Initialize(AllCombatants, initialTurn);
        combatUI?.Setup(this);
        PlayFormationEntranceThenStartAsync(heroFacing, enemyFacing).Forget();
    }

    void SubscribeCombatantStatsEvents()
    {
        if (AllCombatants == null)
            return;

        foreach (var combatant in AllCombatants)
        {
            if (combatant == null)
                continue;

            combatant.OnStatsChanged -= HandleCombatantStatsChanged;
            combatant.OnStatsChanged += HandleCombatantStatsChanged;

            combatant.InitDisplayedHp();
            combatant.OnDoHit -= HandleCombatantDoHit;
            combatant.OnDoHit += HandleCombatantDoHit;
        }
    }

    void UnsubscribeCombatantStatsEvents()
    {
        if (AllCombatants == null)
            return;

        foreach (var combatant in AllCombatants)
        {
            if (combatant == null)
                continue;

            combatant.OnStatsChanged -= HandleCombatantStatsChanged;
            combatant.OnDoHit -= HandleCombatantDoHit;
        }
    }

    void HandleCombatantDoHit(CombatHitInfo hitInfo)
    {
        hitInfo.Defender?.ApplyVisualHit(hitInfo.HitDamage);

        if (hitInfo.IsLastHit && hitInfo.Defender != null && !hitInfo.Defender.IsAlive)
            hitInfo.Defender.StateMachine?.StartDeath();

        RaiseCombatantDoHit(hitInfo);
    }

    void SyncAllDisplayedHp()
    {
        if (AllCombatants == null)
            return;

        foreach (var combatant in AllCombatants)
            combatant?.SyncDisplayedHp();
    }

    void HandleCombatantStatsChanged(CombatantBase combatant) => RaiseCombatantStatsChanged(combatant);

    void EnsureClickTargets()
    {
        if (AllCombatants == null)
            return;

        foreach (var combatant in AllCombatants)
        {
            if (combatant == null)
                continue;

            if (!combatant.TryGetComponent(out CombatantClickTarget clickTarget))
                clickTarget = combatant.gameObject.AddComponent<CombatantClickTarget>();

            clickTarget.Setup(combatant);
        }
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

            combatant.SetFormationSlot(slot);
        }
    }

    async UniTaskVoid PlayFormationEntranceThenStartAsync(Quaternion? heroFacing, Quaternion? enemyFacing)
    {
        CameraDirector?.RestoreCombatCamera();

        await UniTask.WaitForSeconds(0.5f);

        var moves = new List<UniTask>();
        AddFormationMoves(moves, PlayerCombatants, HeroPositions, heroFacing);
        AddFormationMoves(moves, EnemyCombatants, EnemyPositions, enemyFacing);
        await UniTask.WhenAll(moves);

        StateMachine.ChangePhase(TurnBaseCombatPhase.StartCombat);
        OnCombatStarted?.Invoke();
        Debug.Log("Combat Started");
    }

    void AddFormationMoves(List<UniTask> moves, List<CombatantBase> combatants, Transform[] slots, Quaternion? facing)
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

            float duration = Random.Range(formationMoveDurationRange.x, formationMoveDurationRange.y);
            moves.Add(MoveCombatantToSlotAsync(combatant, slot.position, facing, duration));
        }
    }

    async UniTask MoveCombatantToSlotAsync(CombatantBase combatant, Vector3 destination, Quaternion? facing, float duration)
    {
        var tr = combatant.transform;

        if (TurnBaseCombatHelper.TryGetFlatFacingRotation(tr.position, destination, out Quaternion moveFacing))
            TurnBaseCombatHelper.TeleportTo(tr, tr.position, moveFacing);

        var animancer = combatant.GetComponent<CombatUnitController>()?.AnimancerCont;
        var moveClip = combatant.AnimationClips?.MoveToTarget;
        if (animancer != null && moveClip != null)
            animancer.PlayAnimation(moveClip, AttackActionHelper.DefaultFadeDuration);

        await AttackActionHelper.MoveTransformAsync(tr, destination, duration, false, 0f, facing);

        if (combatant.IsAlive)
            combatant.StateMachine?.ChangeState(CombatantState.Idle);
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
        CurrentTurnCount++;
        SelectedAttack = null;
        TargetOpponent = null;
        TargetTeam = null;

        Debug.Log($"{nameof(TurnBaseCombatManager)}: Current turn -> {TurnTimeline.CurrentTurnCombatant.name} (Step {TurnTimeline.CurrentStep}, Turn {CurrentTurnCount}).");
        EnterTurnPhaseForCurrentCombatant();
        RaiseTurnAdvanced();
        return true;
    }

    public void AdvanceTimelineUntilTurnFound()
    {
        if (TurnTimeline == null || !TurnTimeline.IsInitialized)
            return;

        if (TryAssignCurrentTurnCombatant())
            return;

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
    public void CompleteCurrentTurn()
    {
        if (IsCombatOver)
            return;

        if (TurnTimeline == null || !TurnTimeline.IsInitialized)
        {
            Debug.Log($"{nameof(TurnBaseCombatManager)}: Turn timeline is not initialized.");
            return;
        }

        SyncAllDisplayedHp();

        if (TurnTimeline.CurrentTurnCombatant == null)
        {
            Debug.Log($"{nameof(TurnBaseCombatManager)}: No active turn to resolve.");
            AdvanceTimelineUntilTurnFound();
            return;
        }

        var actingCombatant = TurnTimeline.CurrentTurnCombatant;
        TurnTimeline.NotifyCombatantActed(actingCombatant);
        TurnTimeline.CurrentTurnCombatant = null;

        Debug.Log($"{nameof(TurnBaseCombatManager)}: Resolved turn for {actingCombatant.name} at step {TurnTimeline.CurrentStep}.");
        RaiseTimelinePreviewUpdated();

        if (TryEndCombat())
            return;

        AdvanceTimelineUntilTurnFound();
    }
}