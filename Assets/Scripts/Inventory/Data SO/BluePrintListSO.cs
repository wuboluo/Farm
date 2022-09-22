using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BluePrintListSO", menuName = "Inventory/BluePrintSO")]
public class BluePrintListSO : ScriptableObject
{
    public List<BluePrintDetails> bluePrintDetailsList;

    public BluePrintDetails GetBluePrintDetails(int itemID)
    {
        return bluePrintDetailsList.Find(b => b.id == itemID);
    }
}

[Serializable]
public class BluePrintDetails
{
    public int id;
    public InventoryItem[] resourceItem = new InventoryItem[4];

    public GameObject buildPrefab;
}