using UnityEngine;

[CreateAssetMenu(fileName = "CurrencySO", menuName = "RAXY/Inventory System/Item/Currency")]
public class CurrencySO : ItemBaseSO
{
    public override bool IsStackable => true;
}
