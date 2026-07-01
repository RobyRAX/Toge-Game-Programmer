using System;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayDependencyManager : SepManagerBase<GameplayDependencyManager>, IBootstrapper
{
    #region IBootstrapper
    public bool IsInitDone_FirstTime { get; set; }
    public bool IsInitDone { get; set; }

    public GameObject GetGameObject => gameObject;
    public event Action<bool> OnInitDone;

    public async UniTask InitializeAsync_FirstTime()
    {
        IsInitDone = false;

        await InitAllSepGroup();

        IsInitDone = true;
        IsInitDone_FirstTime = true;
        OnInitDone?.Invoke(true);
    }
    #endregion

    [Button]
    public async UniTask InitializeAsync_Teleport()
    {
        IsInitDone = false;

        await EnemyGroupManager.Instance.Init();

        var enemyGroupSepGroup = RuntimeSepGroups.Find(x => x.GroupName == ENEMY_GROUP_SEP_GROUP);
        await InitSepGroup(enemyGroupSepGroup);

        var enemySepGroup = RuntimeSepGroups.Find(x => x.GroupName == ENEMY_SEP_GROUP);
        await InitSepGroup(enemySepGroup);

        IsInitDone = true;
    }

    public const string HERO_SEP_GROUP = "Hero";
    public const string ENEMY_GROUP_SEP_GROUP = "Enemy Group";
    public const string ENEMY_SEP_GROUP = "Enemy";

    [TitleGroup("Root")]
    public Transform heroSpawnRoot;

#if UNITY_EDITOR
    [TitleGroup("Sep Groups")]
    [Button]
    void InitSepGroups()
    {
        bool isContainHeroSepGroup = false;
        foreach (var group in PreDefinedSepGroups)
        {
            if (group.GroupName == HERO_SEP_GROUP)
            {
                isContainHeroSepGroup = true;
                break;
            }
        }

        bool isContainEnemyGroupSepGroup = false;
        foreach (var group in PreDefinedSepGroups)
        {
            if (group.GroupName == ENEMY_GROUP_SEP_GROUP)
            {
                isContainEnemyGroupSepGroup = true;
                break;
            }
        }

        bool isContainEnemySepGroup = false;
        foreach (var group in PreDefinedSepGroups)
        {
            if (group.GroupName == ENEMY_SEP_GROUP)
            {
                isContainEnemySepGroup = true;
                break;
            }
        }

        if (isContainHeroSepGroup == false)
            PreDefinedSepGroups.Add(new SepGroupEntry() { GroupName = HERO_SEP_GROUP, ExecutionType = SepGroupExecutionType.Parallel });
        
        if (isContainEnemyGroupSepGroup == false)
            PreDefinedSepGroups.Add(new SepGroupEntry() { GroupName = ENEMY_GROUP_SEP_GROUP, ExecutionType = SepGroupExecutionType.Parallel });
        
        if (isContainEnemySepGroup == false)
            PreDefinedSepGroups.Add(new SepGroupEntry() { GroupName = ENEMY_SEP_GROUP, ExecutionType = SepGroupExecutionType.Parallel });
    }
#endif
}
