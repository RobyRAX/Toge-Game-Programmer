using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Core;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class InventoryManager : InventoryManagerBase, ISepObject
{
    #region ISepObject
    public GameObject GetGameObject => gameObject;

    public bool FirstInitDone { get; set; }
    public int Order { get; set; }
    public string SepGroup { get; set; }
    public bool UsePreInit { get; set; }

    public async UniTask Init()
    {
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
        }
    }

    public static InventoryManager Instance { get; private set; }
}
