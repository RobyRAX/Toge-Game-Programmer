using RAXY.Core.Addressable;
using RAXY.InventorySystem;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroDataSO", menuName = "RAXY/Unit/Hero/Hero Data")]
public class HeroDataSO : ScriptableObject, IItemEntry
{
    [SerializeField] 
    string heroId;

    [SerializeField]  
    string heroName;

    [SerializeField]  
    Sprite heroIcon;

    public GameObject heroPrefab;

    public string ItemId => heroId;
    public bool IsStackable => false;
    public string ItemName => heroName;
    public string ItemDescription => "";
    public string ItemAdditionalDescription => "";
    public Sprite ItemIcon => heroIcon;
}
