using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroDataSO", menuName = "RAXY/Unit/Hero/Hero Data")]
public class HeroDataSO : UnitDataSO, IItemEntry
{
    [TitleGroup("Combat Data")]
    [HideLabel]
    public HeroCombatDataSO heroCombatDataSO;

    public override CombatDataBaseSO CombatDataSO => heroCombatDataSO;

    public string ItemId => unitId;
    public bool IsStackable => false;
    public string ItemName => unitName;
    public string ItemDescription => "";
    public string ItemAdditionalDescription => "";
    public Sprite ItemIcon => unitIcon;
}
