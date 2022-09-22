using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataListSO", menuName = "Inventory/ItemDataList")]
public class ItemDataListSO : ScriptableObject
{
    public List<ItemDetails> itemDetailsList;
}