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

    public bool InitDone { get; set; }
    public int Order { get; set; }
    public string SepGroup { get; set; }
    public bool UsePreInit { get; set; }

    public async UniTask Init()
    {
        SetItemDatabase(GlobalManager.Instance.ItemDatabase);
        SetItemFactory(itemFactory);

        SendInitialItems();

        InitDone = true;
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    public static InventoryManager Instance { get; private set; }

    [TitleGroup("Item Factory")]
    public ItemFactory itemFactory;

    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
