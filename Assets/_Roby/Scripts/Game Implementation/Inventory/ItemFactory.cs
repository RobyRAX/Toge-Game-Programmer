using RAXY.InventorySystem;
using UnityEngine;

public class ItemFactory : MonoBehaviour, IItemFactory
{
    public IItemInstance CreateItemInstance(string itemId, int amount = 1)
    {
        var itemEntry = InventoryManager.Instance?.ItemDatabase?.GetItemEntry(itemId);
        return CreateItemInstance(itemEntry, amount);
    }

    public IItemInstance CreateItemInstance(IItemEntry itemEntry, int amount)
    {
        if (itemEntry == null)
            return null;

        if (itemEntry is HeroDataSO)
            return CreateHeroInstance(itemEntry, amount);

        if (itemEntry is CurrencySO)
            return CreateCurrencyInstance(itemEntry, amount);

        return null;
    }

    public IItemInstance CloneInstance(IItemInstance otherItem)
    {
        if (otherItem == null)
            return null;

        var itemDb = InventoryManager.Instance?.ItemDatabase;

        if (otherItem is ItemInstance_Hero heroInstance)
        {
            var clone = new ItemInstance_Hero
            {
                ItemEntry = otherItem.ItemEntry,
                ItemId = heroInstance.ItemId,
                Amount = otherItem.Amount,
                ItemInstanceId = otherItem.ItemInstanceId,
                level = heroInstance.level,
                exp = heroInstance.exp,
                currentHp = heroInstance.currentHp,
                availableTalentPoints = heroInstance.availableTalentPoints,
                NormalAttackTalentLevel = heroInstance.NormalAttackTalentLevel,
                SkillTalentLevel = heroInstance.SkillTalentLevel,
                UltimateTalentLevel = heroInstance.UltimateTalentLevel
            };

            if (clone.ItemEntry == null && itemDb != null)
                clone.ItemEntry = itemDb.GetItemEntry(clone.ItemId);

            return clone;
        }

        if (otherItem is ItemInstance_Currency)
        {
            var clone = new ItemInstance_Currency
            {
                ItemEntry = otherItem.ItemEntry,
                ItemId = otherItem.ItemId,
                Amount = otherItem.Amount,
                ItemInstanceId = otherItem.ItemInstanceId
            };

            if (clone.ItemEntry == null && itemDb != null)
                clone.ItemEntry = itemDb.GetItemEntry(clone.ItemId);

            return clone;
        }

        return null;
    }

    static ItemInstance_Hero CreateHeroInstance(IItemEntry itemEntry, int amount)
    {
        return new ItemInstance_Hero
        {
            ItemEntry = itemEntry,
            ItemId = itemEntry.ItemId,
            Amount = amount,
            ItemInstanceId = itemEntry.ItemId
        };
    }

    static ItemInstance_Currency CreateCurrencyInstance(IItemEntry itemEntry, int amount)
    {
        return new ItemInstance_Currency
        {
            ItemEntry = itemEntry,
            ItemId = itemEntry.ItemId,
            Amount = amount,
            ItemInstanceId = itemEntry.ItemId
        };
    }
}
