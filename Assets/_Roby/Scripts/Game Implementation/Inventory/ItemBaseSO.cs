using RAXY.Core.Addressable;
using RAXY.InventorySystem;
using RAXY.Utility.Localization;
using UnityEngine;

public abstract class ItemBaseSO : ScriptableObject, IItemEntry
{
    [SerializeField]
    string itemId;
    public string ItemId => itemId;

    [SerializeField]
    string itemName;
    public string ItemName => itemName;
    public virtual bool IsStackable => false;

    [SerializeField]
    string itemDescription;
    public string ItemDescription => itemDescription;

    [SerializeField]
    string itemAdditionalDescription;
    public string ItemAdditionalDescription => itemAdditionalDescription;

    [SerializeField]
    Sprite itemIcon;
    public Sprite ItemIcon => itemIcon;
}
