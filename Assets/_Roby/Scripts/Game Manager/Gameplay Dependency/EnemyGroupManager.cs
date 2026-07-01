using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using RAXY.Utility;
using UnityEngine;

public class EnemyGroupManager : Singleton<EnemyGroupManager>, ISepObject
{
    #region ISepObject
    public GameObject GetGameObject => gameObject;

    public bool InitDone { get; set; }
    public int Order { get; set; }
    public string SepGroup { get; set; }
    public bool UsePreInit { get; set; }

    public async UniTask Init()
    {
        EnemyGroups = GameObject.FindObjectsByType<EnemyGroup>(
                        FindObjectsInactive.Exclude, 
                        FindObjectsSortMode.None).
                        ToList();

        foreach (var eg in EnemyGroups)
        {
            GameplayDependencyManager.Instance.RegisterSepObject(eg, 
            GameplayDependencyManager.ENEMY_GROUP_SEP_GROUP);
        }

        InitDone = true;
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    public List<EnemyGroup> EnemyGroups;
}
