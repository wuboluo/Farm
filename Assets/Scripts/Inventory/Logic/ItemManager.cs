using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Y.Save;

namespace Y.Inventory
{
    public class ItemManager : MonoBehaviour, ISaveable
    {
        public Item itemPrefab;
        public Item bouncePrefab;

        private Transform itemParent;
        private Dictionary<string, List<SceneFurniture>> sceneFurnitureDict = new();

        // 记录场景里的 item
        private Dictionary<string, List<SceneItem>> sceneItemDict = new();

        private Transform PlayerTransform => FindObjectOfType<Player>().transform;

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        private void OnEnable()
        {
            EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
            EventHandler.DropItemEvent += OnDropItem;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;

            // 建造
            EventHandler.BuildFurnitureEvent += OnBuildFurniture;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
            EventHandler.DropItemEvent -= OnDropItem;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;

            // 建造
            EventHandler.BuildFurnitureEvent -= OnBuildFurniture;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        public string GUID => GetComponent<DataGUID>().guid;

        public GameSaveData GenerateSaveData()
        {
            GetAllSceneItems();
            GetAllSceneFurniture();

            var saveData = new GameSaveData();
            saveData.sceneItemDict = sceneItemDict;
            saveData.sceneFurnitureDict = sceneFurnitureDict;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            sceneItemDict = saveData.sceneItemDict;
            sceneFurnitureDict = saveData.sceneFurnitureDict;

            RecreateAllItems();
            RebuildFurniture();
        }

        private void OnStartNewGameEvent(int index)
        {
            sceneItemDict.Clear();
            sceneFurnitureDict.Clear();
        }

        private void OnBeforeSceneUnload()
        {
            // 加载新场景前，把当前场景所有物品记录一下（离开此场景前的最后版本）
            GetAllSceneItems();

            GetAllSceneFurniture();
        }

        private void OnAfterSceneLoaded()
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;

            // 加载场景完成后，更新场景里的物体
            RecreateAllItems();
            RebuildFurniture();
        }

        // 在指定坐标位置生成物品
        private void OnInstantiateItemInScene(int id, Vector3 pos)
        {
            var item = Instantiate(bouncePrefab, pos, Quaternion.identity, itemParent);
            item.itemID = id;

            item.GetComponent<ItemBounce>().InitBounceItem(pos, Vector3.up);
        }


        // 扔东西
        private void OnDropItem(int id, Vector3 mousePos, ItemType itemType)
        {
            if (itemType == ItemType.Seed) return;

            var item = Instantiate(bouncePrefab, PlayerTransform.position, Quaternion.identity, itemParent);
            item.itemID = id;

            var dir = (mousePos - PlayerTransform.position).normalized;
            item.GetComponent<ItemBounce>().InitBounceItem(mousePos, dir);
        }

        // 获取当前场景中所有 item
        private void GetAllSceneItems()
        {
            // 通过 item类型找到所有 item物体
            var currentSceneItems = FindObjectsOfType<Item>().Select(
                item => new SceneItem {itemID = item.itemID, position = new SerializableVector3(item.transform.position)}).ToList();

            // 如果字典中包含当前激活的场景，则更新场景里物体
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
                sceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItems;
            // 否则，将此场景和里面的物体添加进字典
            else
                sceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItems);
        }


        // 刷新重建当前场景的 item
        private void RecreateAllItems()
        {
            if (sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name, out var currentSceneItems))
            {
                // 如果没有记录过这个场景的 item信息，跳出
                if (currentSceneItems == null) return;

                // 找到这个场景中的所有 item，并删除
                foreach (var item in FindObjectsOfType<Item>()) Destroy(item.gameObject);

                // 根据记录中当前场景所存在的 item信息，重新实例，设置位置和 ID
                foreach (var item in currentSceneItems)
                {
                    var newItem = Instantiate(itemPrefab, item.position.ToVector3(), Quaternion.identity, itemParent);
                    newItem.Init(item.itemID);
                }
            }
        }

        private void OnBuildFurniture(int id, Vector3 mousePos)
        {
            // 根据蓝图实例一个家具
            var bluePrint = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(id);
            var buildItem = Instantiate(bluePrint.buildPrefab, mousePos, Quaternion.identity, itemParent);

            // 如果这个家具是一个储物箱
            if (buildItem.GetComponent<Box>())
            {
                // 首次设置这个储物箱的 index为 box字典的长度为序号
                buildItem.GetComponent<Box>().index = InventoryManager.Instance.BoxDataAmount;
                // 根据序号初始化，并添加到 box字典中，从而字典数量增加。之后的箱子 index也自动递增
                buildItem.GetComponent<Box>().InitBox(buildItem.GetComponent<Box>().index);
            }
        }


        // 获取当前场景中所有 Furniture
        private void GetAllSceneFurniture()
        {
            var currentSceneFurniture = new List<SceneFurniture>();

            // 通过 Furniture类型找到所有 Furniture物体
            foreach (var furniture in FindObjectsOfType<Furniture>())
            {
                var sceneFurniture = new SceneFurniture
                {
                    furnitureID = furniture.furnitureID,
                    position = new SerializableVector3(furniture.transform.position)
                };

                if (furniture.GetComponent<Box>()) sceneFurniture.boxIndex = furniture.GetComponent<Box>().index;

                currentSceneFurniture.Add(sceneFurniture);
            }

            // 如果字典中包含当前激活的场景，则更新场景里物体
            if (sceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
                sceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneFurniture;
            // 否则，将此场景和里面的物体添加进字典
            else
                sceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
        }

        /// 重建当前场景家具
        private void RebuildFurniture()
        {
            // 如果当前场景存在已保存的家具列表
            if (sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name, out var currentSceneFurniture))
                // 且这个家具列表不为空，代表保存了一些家具
                if (currentSceneFurniture != null)
                    foreach (var sceneFurniture in currentSceneFurniture)
                    {
                        // 遍历保存的这些家具
                        var bluePrint = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(sceneFurniture.furnitureID);
                        var buildItem = Instantiate(bluePrint.buildPrefab, sceneFurniture.position.ToVector3(), Quaternion.identity, itemParent);

                        if (buildItem.GetComponent<Box>()) buildItem.GetComponent<Box>().InitBox(sceneFurniture.boxIndex);
                    }
        }
    }
}