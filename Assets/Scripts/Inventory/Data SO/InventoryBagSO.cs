using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryBagSO", menuName = "Inventory/InventoryBag")]
public class InventoryBagSO : ScriptableObject
{
    public List<InventoryItem> itemList;
    
    public InventoryItem GetInventoryItem(int itemID)
    {
        return itemList.Find(b => b.itemID == itemID);
    }
}