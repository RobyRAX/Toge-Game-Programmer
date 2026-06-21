using System;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class GlobalManager : SepManagerBase<GlobalManager>, IBootstrapper
{
    #region IBootstrapper
    public bool IsInitDone_FirstTime { get; set; }
    public bool IsInitDone { get; set; }

    public GameObject GetGameObject => gameObject;
    public event Action<bool> OnInitDone;

    public async UniTask InitializeAsync_FirstTime()
    {
        IsInitDone = false;

        ItemDatabase.Init().Forget();

        await InitAllSepGroup();

        IsInitDone = true;
        IsInitDone_FirstTime = true;
        OnInitDone?.Invoke(true);
    }
    #endregion

    [TitleGroup("Database")]
    public ItemDatabaseSO ItemDatabase;

    [TitleGroup("Database")]
    public HeroDatabaseSO HeroDatabase;
}
