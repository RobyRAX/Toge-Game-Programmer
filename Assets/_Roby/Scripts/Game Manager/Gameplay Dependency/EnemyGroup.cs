using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyGroup : MonoBehaviour, ISepObject
{
    [PropertyOrder(-1)]
    [ShowInInspector]
    public string GroupId => gameObject.name;

    public bool isCleared;

    [TitleGroup("Combat")]
    [SerializeField]
    int level = 1;
    public int Level => level;

    public List<EnemyController> enemies;

    public event Action OnEnemyMemberAttacked;

    #region ISepObject
    public GameObject GetGameObject => gameObject;

    public bool FirstInitDone { get; set; }
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

        FirstInitDone = true;
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

        foreach (var enemy in enemies)
        {
            if (enemy != null)
                enemy.OnAttacked -= OnAttackedHandler;
        }
    }

    void OnAttackedHandler()
    {
        if (isCleared)
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

        var initialTurn = heroCombatants.Count > 0 ? heroCombatants[0] : null;

        var combatManager = TurnBaseCombatManager.Instance;
        UnsubscribeCombatEnded();
        combatManager.OnCombatEnded += OnCombatEndedHandler;
        combatManager.StartCombat(heroCombatants, enemyCombatants, initialTurn);
    }

    void OnCombatEndedHandler(TurnSide winningSide)
    {
        UnsubscribeCombatEnded();

        if (winningSide == TurnSide.Player)
            MarkAsCleared();
        else if (winningSide == TurnSide.Enemy)
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
        isCleared = true;

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
}
