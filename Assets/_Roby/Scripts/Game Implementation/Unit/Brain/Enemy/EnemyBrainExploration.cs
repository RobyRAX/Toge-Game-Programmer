using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyBrainExploration : BrainExplorationBase
{
    public enum EnemyExplorationMode
    {
        Idle,
        Patrol,
        Chase
    }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    [ReadOnly]
    public EnemyExplorationMode CurrentMode { get; private set; }

    public EnemyBrainExplorationConfigSO BrainConfig { get; }
    public EnemyGroup EnemyGroup { get; }

    EnemyController _enemyCont;
    Vector3 _patrolTarget;
    float _idleTimer;

    public EnemyBrainExploration(EnemyController enemyController,
                                 EnemyBrainExplorationConfigSO config,
                                 EnemyGroup enemyGroup) : base(enemyController)
    {
        _enemyCont = enemyController;
        BrainConfig = config;
        EnemyGroup = enemyGroup;

        CurrentMode = EnemyExplorationMode.Idle;
        _idleTimer = BrainConfig?.patrolIdleDuration ?? 0f;

        if (TurnBaseCombatManager.Instance != null)
        {
            TurnBaseCombatManager.Instance.OnCombatStarted -= CombatStartedHandler;
            TurnBaseCombatManager.Instance.OnCombatStarted += CombatStartedHandler;

            TurnBaseCombatManager.Instance.OnCombatEnded -= CombatEndedHandler;
            TurnBaseCombatManager.Instance.OnCombatEnded += CombatEndedHandler;
        }

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnRespawn -= RespawnHandler;
            GameplayManager.Instance.OnRespawn += RespawnHandler;
        }
    }

    public override void Update()
    {
        if (BrainConfig == null || EnemyGroup == null || EnemyGroup.IsCleared)
            return;

        if (GameplayManager.Instance == null ||
            GameplayManager.Instance.CurrentState != GameplayState.Explore)
            return;

        var hero = GameplayManager.Instance.MainHero;
        if (hero == null)
            return;

        if (UnitSM != null && UnitSM.CurrentState == UnitSM.Attack)
            return;

        float distToHero = GetHorizontalDistance(Cont.transform.position, hero.transform.position);
        bool heroInsideGroupRadius = IsInsideGroupRadius(hero.transform.position);

        if (distToHero <= BrainConfig.attackRadius)
        {
            FaceTarget(hero.transform.position);
            Move = Vector2.zero;
            Sprint = false;
            Attack = true;
            return;
        }

        if (distToHero <= BrainConfig.chaseRadius && heroInsideGroupRadius)
        {
            CurrentMode = EnemyExplorationMode.Chase;
            Sprint = BrainConfig.useSprintWhileChasing;
            SetMoveToward(hero.transform.position);
            return;
        }

        if (CurrentMode == EnemyExplorationMode.Chase)
        {
            BeginIdlePatrolCycle();
            return;
        }

        switch (CurrentMode)
        {
            case EnemyExplorationMode.Idle:
                UpdateIdle();
                break;

            case EnemyExplorationMode.Patrol:
                UpdatePatrol();
                break;
        }
    }

    void UpdateIdle()
    {
        Move = Vector2.zero;
        Sprint = false;
        Attack = false;

        _idleTimer -= Time.deltaTime;
        if (_idleTimer > 0f)
            return;

        _patrolTarget = EnemyGroup.GetRandomPatrolPoint();
        CurrentMode = EnemyExplorationMode.Patrol;
    }

    void UpdatePatrol()
    {
        Attack = false;
        Sprint = false;

        float dist = GetHorizontalDistance(Cont.transform.position, _patrolTarget);
        if (dist <= BrainConfig.patrolArrivalThreshold)
        {
            BeginIdlePatrolCycle();
            return;
        }

        SetMoveToward(_patrolTarget);
    }

    void BeginIdlePatrolCycle()
    {
        CurrentMode = EnemyExplorationMode.Idle;
        _idleTimer = BrainConfig.patrolIdleDuration;
        Move = Vector2.zero;
        Sprint = false;
        Attack = false;
    }

    void SetMoveToward(Vector3 worldTarget)
    {
        Vector3 dir = worldTarget - Cont.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
        {
            Move = Vector2.zero;
            return;
        }

        dir.Normalize();
        Move = new Vector2(dir.x, dir.z);
    }

    void FaceTarget(Vector3 worldTarget)
    {
        Vector3 dir = worldTarget - Cont.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Cont.transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    static float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    bool IsInsideGroupRadius(Vector3 heroWorldPos)
    {
        if (EnemyGroup == null)
            return false;

        float radius = EnemyGroup.radius;
        if (radius <= 0f)
            return false;

        return GetHorizontalDistance(EnemyGroup.transform.position, heroWorldPos) <= radius;
    }

    void CombatStartedHandler()
    {
        ResetAllInput();
    }

    void CombatEndedHandler(TurnSide winningSide)
    {
        if (winningSide == TurnSide.Enemy)
            BeginIdlePatrolCycle();
    }

    void RespawnHandler()
    {
        BeginIdlePatrolCycle();
    }

    public override void OnDestroy()
    {
        if (TurnBaseCombatManager.Instance != null)
        {
            TurnBaseCombatManager.Instance.OnCombatStarted -= CombatStartedHandler;
            TurnBaseCombatManager.Instance.OnCombatEnded -= CombatEndedHandler;
        }

        if (GameplayManager.Instance != null)
            GameplayManager.Instance.OnRespawn -= RespawnHandler;
    }

    public void DrawGizmos()
    {
        if (BrainConfig == null)
            return;

        Vector3 pos = Cont.transform.position;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(pos, BrainConfig.chaseRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawWireSphere(pos, BrainConfig.attackRadius);
    }
}
