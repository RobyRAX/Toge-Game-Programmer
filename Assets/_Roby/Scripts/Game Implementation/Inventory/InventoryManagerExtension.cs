using System.Collections.Generic;
using System.Linq;
using RAXY.InventorySystem;

public static class InventoryManagerExtension
{
    public static List<ItemInstance_Hero> GetAllItemInstanceHeroes(this InventoryManager inventoryManager)
    {
        return inventoryManager.GetAllItemInstanceHeroes(InventoryManagerBase.PLAYER_INVENTORY_ID);
    }

    public static List<ItemInstance_Hero> GetAllItemInstanceHeroes(this InventoryManager inventoryManager, string inventoryId)
    {
        if (inventoryManager?.InventoryInstances == null ||
            !inventoryManager.InventoryInstances.TryGetValue(inventoryId, out var inventoryInstance) ||
            inventoryInstance?.storedItems == null)
        {
            return new List<ItemInstance_Hero>();
        }

        return inventoryInstance.storedItems.Values
            .OfType<ItemInstance_Hero>()
            .ToList();
    }
}
