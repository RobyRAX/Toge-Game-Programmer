using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyGroup : MonoBehaviour, ISepObject
{
    [TitleGroup("Status")]
    [ShowInInspector]
    public string GroupId => gameObject.name;

    [TitleGroup("Status")]
    [ShowInInspector]
    public bool IsCleared { get; set; }

    public int level = 1;
    public int expReward;
    [FormerlySerializedAs("patrolRadius")]
    public float radius = 5f;

    public List<EnemyController> enemies;

    public event Action OnEnemyMemberAttacked;

    #region ISepObject
    public GameObject GetGameObject => gameObject;

    public bool InitDone { get; set; }
    public int Order { get; set; }
    public string SepGroup { get; set; }
    public bool UsePreInit { get; set; }


    public async UniTask Init()
    {
        GetEnemyMembers();

        foreach (var enemy in enemies)
        {
            enemy.OnAttacked -= OnAttackedHandler;
            enemy.OnAttacked += OnAttackedHandler;
        }

        InitDone = true;
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    [Button]
    public void GetEnemyMembers()
    {
        enemies = new();

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out EnemyController enemyCont))
            {
                if (GameplayDependencyManager.Instance != null)
                {
                    GameplayDependencyManager.Instance.RegisterSepObject(enemyCont, 
                    GameplayDependencyManager.ENEMY_SEP_GROUP);
                }

                enemyCont.EnemyGroup = this;
                enemies.Add(enemyCont);
            }
        }
    }

    void OnDestroy()
    {
        UnsubscribeCombatEnded();

        if (GameplayManager.Instance != null)
            GameplayManager.Instance.OnRespawn -= OnRespawnHandler;

        foreach (var enemy in enemies)
        {
            if (enemy != null)
                enemy.OnAttacked -= OnAttackedHandler;
        }
    }

    void OnAttackedHandler()
    {
        if (IsCleared)
            return;

        CombatantBase initialTurn = null;
        if (GameplayManager.Instance != null)
        {
            foreach (var hero in GameplayManager.Instance.SpawnedHeroDict.Values)
            {
                initialTurn = hero.GetComponent<HeroCombatant>();
                break;
            }
        }

        StartCombatInternal(initialTurn);
    }

    public void StartCombatFromEnemyAttack(EnemyController attacker)
    {
        if (attacker == null || attacker.enemyCombatant == null)
            return;

        StartCombatInternal(attacker.enemyCombatant);
    }

    void StartCombatInternal(CombatantBase initialTurn)
    {
        if (IsCleared)
            return;

        if (GameplayManager.Instance == null ||
            GameplayManager.Instance.CurrentState != GameplayState.Explore)
            return;

        var heroes = GameplayManager.Instance.SpawnedHeroDict.Values;
        var heroCombatants = new List<CombatantBase>();
        foreach (var hero in heroes)
        {
            heroCombatants.Add(hero.GetComponent<HeroCombatant>());
        }

        var enemyCombatants = new List<CombatantBase>();
        foreach (var enemy in enemies)
        {
            enemyCombatants.Add(enemy.GetComponent<EnemyCombatant>());
        }

        if (heroCombatants.Count == 0 || enemyCombatants.Count == 0)
            return;

        var combatManager = TurnBaseCombatManager.Instance;
        UnsubscribeCombatEnded();
        combatManager.OnCombatEnded += OnCombatEndedHandler;
        combatManager.StartCombat(heroCombatants, enemyCombatants, initialTurn, transform);
    }

    public Vector3 GetRandomPatrolPoint()
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;
        return transform.position + new Vector3(offset.x, 0f, offset.y);
    }

    void OnCombatEndedHandler(TurnSide winningSide)
    {
        UnsubscribeCombatEnded();

        if (winningSide == TurnSide.Player)
        {
            if (expReward > 0)
                GameplayManager.Instance?.AddExpToSpawnedHeroes(expReward);

            MarkAsCleared();
        }
        else if (winningSide == TurnSide.Enemy)
            DeferRestoreUntilRespawn();
    }

    void DeferRestoreUntilRespawn()
    {
        if (GameplayManager.Instance == null)
        {
            RestoreEnemies();
            return;
        }

        GameplayManager.Instance.OnRespawn -= OnRespawnHandler;
        GameplayManager.Instance.OnRespawn += OnRespawnHandler;
    }

    void OnRespawnHandler()
    {
        if (GameplayManager.Instance != null)
            GameplayManager.Instance.OnRespawn -= OnRespawnHandler;

        RestoreEnemies();
    }

    void RestoreEnemies()
    {
        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.enemyCombatant == null)
                continue;

            enemy.enemyCombatant.SetAlive();
            enemy.enemyCombatant.StateMachine?.ChangeState(CombatantState.Idle);
        }
    }

    void MarkAsCleared()
    {
        IsCleared = true;

        foreach (var enemy in enemies)
        {
            if (enemy == null)
                continue;

            enemy.OnAttacked -= OnAttackedHandler;
            enemy.gameObject.SetActive(false);
        }
    }

    void UnsubscribeCombatEnded()
    {
        if (TurnBaseCombatManager.Instance == null)
            return;

        TurnBaseCombatManager.Instance.OnCombatEnded -= OnCombatEndedHandler;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
