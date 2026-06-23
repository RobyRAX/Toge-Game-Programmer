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

    private void OnAttackedHandler()
    {
        var heroes = GameplayPartyManager.Instance.SpawnedHeroObjDict.Values;
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

        TurnBaseCombatManager.Instance.StartCombat(heroCombatants, enemyCombatants);
    }
}