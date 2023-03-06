using System.Collections.Generic;
using UnityEngine;

// 一个仓库
// 由于仓库只需要记录存放的【物品种类】和【对应的数量】，所以从这个意义上来说，玩家背包、储物箱和商人的货架没有区别
// 那么这些不同种类的仓库统一使用一样的数据结构，只需要创建不同的多个 ScriptableObject，分别存储即可

[CreateAssetMenu(fileName = "InventoryBagSO", menuName = "Inventory/InventoryBag")]
public class InventoryBagSO : ScriptableObject
{
    /// 当前仓库的存放列表
    public List<InventoryItem> itemList;
    
    /// 此仓库的引导，通过ID返回此仓库对应的物品 
    public InventoryItem GetInventoryItem(int itemID)
    {
        return itemList.Find(b => b.itemID == itemID);
    }
}