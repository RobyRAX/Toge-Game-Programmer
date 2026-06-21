using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Item Database SO", menuName = "RAXY/Inventory System/Item Database")]
public class ItemDatabaseSO : ScriptableObject, IItemDatabase
{
    [SerializeField]
    List<ScriptableObject> _items;

    List<IItemEntry> _itemEntries = new();

    public List<IItemEntry> Items => _itemEntries;

    public IItemEntry GetItemEntry(string itemId)
    {
        return _itemEntries.Find(x => x.ItemId == itemId);
    }

    public UniTask Init()
    {
        _itemEntries.Clear();

        if (_items == null)
            return UniTask.CompletedTask;

        foreach (var item in _items)
        {
            if (item is IItemEntry entry)
                _itemEntries.Add(entry);
        }

        return UniTask.CompletedTask;
    }

#if UNITY_EDITOR
    [Button]
    void FindAllItemEntry()
    {
        _items ??= new List<ScriptableObject>();
        _items.Clear();

        var itemEntryTypes = TypeCache.GetTypesDerivedFrom<IItemEntry>()
            .Where(type => !type.IsAbstract && typeof(ScriptableObject).IsAssignableFrom(type));

        foreach (var type in itemEntryTypes)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{type.Name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.LoadAssetAtPath(path, type) is ScriptableObject itemSO)
                    _items.Add(itemSO);
            }
        }

        _items = _items
            .Distinct()
            .OrderBy(item => item is IItemEntry entry ? entry.ItemId : item.name)
            .ToList();

        EditorUtility.SetDirty(this);
    }
#endif
}
