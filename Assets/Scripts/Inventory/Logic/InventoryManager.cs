using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Y.Save;

namespace Y.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>, ISaveable
    {
        [Header("物品数据")] public ItemDataListSO itemDataListSO;

        [Header("建造蓝图")] public BluePrintListSO bluePrintData;

        [Header("背包数据")]
        public InventoryBagSO playerBagTemp;
        public InventoryBagSO playerBag;
        private InventoryBagSO currentBoxBag;

        [Header("金钱")] public int playerMoney;

        private Dictionary<string, List<InventoryItem>> boxDataDict = new();
        public int BoxDataAmount => boxDataDict.Count;

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            // EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        private void OnEnable()
        {
            EventHandler.DropItemEvent += OnCallDropItem;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent += OnBuildFurniture;
            EventHandler.BaseBagOpenEvent += OnBaseBoxBagOpen;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.DropItemEvent -= OnCallDropItem;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent -= OnBuildFurniture;
            EventHandler.BaseBagOpenEvent -= OnBaseBoxBagOpen;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int index)
        {
            playerBag = Instantiate(playerBagTemp);
            playerMoney = Settings.playerStartMoney;
            
            boxDataDict.Clear();
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        private void OnBuildFurniture(int id, Vector3 pos)
        {
            RemoveItem(id, 1);
            var bluePrint = bluePrintData.GetBluePrintDetails(id);

            foreach (var item in bluePrint.resourceItem)
            {
                RemoveItem(item.itemID, item.itemAmount);
            }
        }

        private void OnBaseBoxBagOpen(SlotType slotType, InventoryBagSO bagSO)
        {
            currentBoxBag = bagSO;
        }

        /// 根据位置返回背包数据列表
        /// <returns></returns>
        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                _ => null
            };
        }

        // 通过 id返回物品信息
        public ItemDetails GetItemDetails(int id)
        {
            return itemDataListSO.itemDetailsList.Find(r => r.itemID == id);
        }

        // 添加物品到背包
        public void AddItem(Item item, bool toDestroy)
        {
            // 背包是否已存在此物品
            var index = GetItemIndexInBag(item.itemID);
            // 添加进背包里
            AddItemAtIndex(item.itemID, index, 1);

            if (toDestroy) Destroy(item.gameObject);

            // 更新 UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        // 通过物品 ID找到此物品在背包中序号，没有则返回 -1
        private int GetItemIndexInBag(int id)
        {
            for (var i = 0; i < playerBag.itemList.Count; i++)
                if (playerBag.itemList[i].itemID == id)
                    return i;

            return -1;
        }

        // 检查背包是否有空位
        private bool CheckBagCapacity()
        {
            for (var i = 0; i < playerBag.itemList.Count; i++)
                if (playerBag.itemList[i].itemID == 0)
                    return true;

            return false;
        }

        // 在背包指定序号添加物品
        private void AddItemAtIndex(int id, int index, int amount)
        {
            // 如果背包没有这个物品，并且还有富余的容量
            if (index == -1 && CheckBagCapacity())
            {
                // 创建一个这个物品
                var item = new InventoryItem {itemID = id, itemAmount = amount};

                // 找到背包的一个空位，添加进去
                for (var i = 0; i < playerBag.itemList.Count; i++)
                {
                    if (playerBag.itemList[i].itemID != 0) continue;

                    playerBag.itemList[i] = item;
                    break;
                }
            }
            // 如果背包有这个物品
            else
            {
                var currentAmount = playerBag.itemList[index].itemAmount + amount;

                var item = new InventoryItem {itemID = id, itemAmount = currentAmount};
                playerBag.itemList[index] = item;
            }
        }

        // 交换物品位置
        public void SwapItem(int from, int to)
        {
            var currentItem = playerBag.itemList[from];
            var targetItem = playerBag.itemList[to];

            if (targetItem.itemID != 0)
            {
                playerBag.itemList[from] = targetItem;
                playerBag.itemList[to] = currentItem;
            }
            else
            {
                playerBag.itemList[to] = currentItem;
                playerBag.itemList[from] = new InventoryItem();
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);

            // 保存玩家背包数据，避免重开 unity的时候背包数据丢失
            EditorUtility.SetDirty(playerBag);
            AssetDatabase.SaveAssets();
        }

        public void SwapItem(InventoryLocation locationFrom, int fromIndex, InventoryLocation locationTarget, int targetIndex)
        {
            // 获取当前和目标的 背包/箱子 的列表
            var currentList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);

            // 获取当前要移动的物品
            var currentItem = currentList[fromIndex];

            if (targetIndex < targetList.Count)
            {
                // 想被交换的物品（可能为空）
                var targetItem = targetList[targetIndex];

                // 目标格子物品不为空，且与被交换的物品不一样，则两个物品交换位置
                if (targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)
                {
                    currentList[fromIndex] = targetItem;
                    targetList[targetIndex] = currentItem;
                }
                // 两个相同的物品
                else if (currentItem.itemID == targetItem.itemID)
                {
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                // 目标是一个空格子
                else
                {
                    targetList[targetIndex] = currentItem;
                    currentList[fromIndex] = new InventoryItem();
                }

                // 更新 UI
                EventHandler.CallUpdateInventoryUI(locationFrom, currentList);
                EventHandler.CallUpdateInventoryUI(locationTarget, targetList);
            }
        }

        // 移除指定数量的背包物品
        private void RemoveItem(int id, int removeAmount)
        {
            var index = GetItemIndexInBag(id);
            if (playerBag.itemList[index].itemAmount > removeAmount)
            {
                // 算出剩余的物品数量
                var amount = playerBag.itemList[index].itemAmount - removeAmount;
                // 创建一个新的 item，设置为剩余数量的之前的 item，覆盖到背包对应位置
                var item = new InventoryItem {itemID = id, itemAmount = amount};
                playerBag.itemList[index] = item;
            }
            else if (playerBag.itemList[index].itemAmount == removeAmount)
            {
                var item = new InventoryItem();
                playerBag.itemList[index] = item;
            }

            // 更新 UI面板
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }


        // 扔东西
        private void OnCallDropItem(int id, Vector3 pos, ItemType itemType)
        {
            RemoveItem(id, 1);
        }


        private void OnHarvestAtPlayerPosition(int id)
        {
            // 背包是否已存在此物品
            var index = GetItemIndexInBag(id);
            // 添加进背包里
            AddItemAtIndex(id, index, 1);

            // 更新 UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// 交易物品
        public void TradeItem(ItemDetails details, int amount, bool isSell)
        {
            var cost = details.itemPrice * amount;

            // 获得物品背包位置
            var index = GetItemIndexInBag(details.itemID);

            // 卖
            if (isSell)
            {
                // 如果想卖的物品在背包里存在想卖的数量
                if (playerBag.itemList[index].itemAmount >= amount)
                {
                    // 从背包移除
                    RemoveItem(details.itemID, amount);
                    // 卖出总价
                    cost = (int) (cost * details.sellPercentage);
                    // 增加余额
                    playerMoney += cost;
                }
            }
            // 买，并且钱够
            else if (playerMoney - cost >= 0)
            {
                // 检查是否存在格子存放
                if (CheckBagCapacity()) AddItemAtIndex(details.itemID, index, amount);

                // 扣钱
                playerMoney -= cost;
            }

            // 刷新人物背包 UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// 检查建造资源物品库存（id：图纸id）
        public bool CheckStock(int bluePrintID)
        {
            var bluePrintDetails = bluePrintData.GetBluePrintDetails(bluePrintID);

            foreach (var resourceItem in bluePrintDetails.resourceItem)
            {
                var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
                if (itemStock.itemAmount >= resourceItem.itemAmount) continue;

                return false;
            }

            return true;
        }

        /// 查找箱子数据
        public List<InventoryItem> GetBoxDataList(string key)
        {
            return boxDataDict.ContainsKey(key) ? boxDataDict[key] : null;
        }

        /// 加入箱子数据
        public void AddBoxDataDict(Box box)
        {
            var key = box.name + box.index;
            if (!boxDataDict.ContainsKey(key))
            {
                boxDataDict.Add(key, box.boxBagData.itemList);
            }

            print("box key: " + key);
        }

        public string GUID => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            var saveData = new GameSaveData();
            saveData.playerMoney = playerMoney;

            saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
            
            // 把人物背包的信息存储进去
            saveData.inventoryDict.Add(playerBag.name, playerBag.itemList);
            // 把所有的箱子所保存的物体信息都存储进去
            foreach (var (key, value) in boxDataDict)
            {
                saveData.inventoryDict.Add(key, value);
            }

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            playerMoney = saveData.playerMoney;
            playerBag = Instantiate(playerBagTemp);
            playerBag.itemList = saveData.inventoryDict[playerBag.name];
            
            foreach (var (key, value) in saveData.inventoryDict)
            {
                // 如果场景中有之前进度中保存的这个箱子
                if (boxDataDict.ContainsKey(key))
                {
                    boxDataDict[key] = value;
                }
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
    }
}