using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Y.Save;

public class GridMapManager : Singleton<GridMapManager>, ISaveable
{
    [Header("种地瓦片切换信息")] public RuleTile digTile;
    public RuleTile waterTile;

    private Tilemap digTilemap;
    private Tilemap waterTilemap;

    [Header("地图信息")] public List<MapDataSO> mapDataList;

    private Season currentSeason;


    /// 场景名字+坐标 和 对应的瓦片信息（保存的是每一块的信息，就是每一个场景一个 dict，记录了几百上千个格子的信息）
    private Dictionary<string, TileDetails> tileDetailsDict = new();

    private Dictionary<string, bool> firstLoadDict = new();

    // 杂草列表
    private List<ReapItem> reapItemsInRadius;

    private Grid currentGrid;

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
        
        foreach (var mapData in mapDataList)
        {
            firstLoadDict.Add(mapData.sceneName, true);

            InitTileDetailDict(mapData);
        }
    }

    private void OnEnable()
    {
        EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
        EventHandler.GameDayEvent += OnGameDay;
        EventHandler.RefreshCurrentMap += RefreshMap;
    }

    private void OnDisable()
    {
        EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
        EventHandler.GameDayEvent -= OnGameDay;
        EventHandler.RefreshCurrentMap -= RefreshMap;
    }

    private void OnAfterSceneLoaded()
    {
        currentGrid = FindObjectOfType<Grid>();

        digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
        waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();

        // 如果该场景是第一次被加载
        if (firstLoadDict[SceneManager.GetActiveScene().name])
        {
            // 为了预先生成农作物
            EventHandler.CallGenerateCropEvent();
            // 更新此场景为非第一次加载
            firstLoadDict[SceneManager.GetActiveScene().name] = false;
        }

        // 更新地图
        // DisplayMap(SceneManager.GetActiveScene().name);
        RefreshMap();
    }


    private void OnGameDay(int day, Season season)
    {
        currentSeason = season;

        // 每过一天
        foreach (var tile in tileDetailsDict)
        {
            // 需要重新浇水
            if (tile.Value.daysSinceWatered > -1) tile.Value.daysSinceWatered = -1;

            // 耕地日期 +1
            if (tile.Value.daysSinceDug > -1) tile.Value.daysSinceDug++;
            // 如果耕地 5天后还没有播种，这块地恢复到未被耕种的状态 
            if (tile.Value.daysSinceDug > 5 && tile.Value.seedItemID == -1)
            {
                tile.Value.daysSinceDug = -1;
                tile.Value.canDig = true;
                tile.Value.growthDays = -1;
            }

            // 如果已经播种，种子生长日期增加
            if (tile.Value.seedItemID != -1)
            {
                tile.Value.growthDays++;
            }
        }

        RefreshMap();
    }


    // ReSharper disable Unity.PerformanceAnalysis
    /// 执行实际工具或物品功能，无需再判断格子是否允许等条件
    private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        var mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
        var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);

        if (currentTile != null)
        {
            var crop = GetCropObject(mouseWorldPos);

            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    EventHandler.CallPlantSeed(itemDetails.itemID, currentTile);
                    EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                    EventHandler.CallPlaySoundEvent(SoundName.Plant);
                    break;

                case ItemType.Commodity:
                    EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                    break;

                case ItemType.HoeTool:
                    SetDigGround(currentTile);
                    // 设置此坑位：日期为 0，不可挖坑，不可扔东西
                    currentTile.daysSinceDug = 0;
                    currentTile.canDig = false;
                    currentTile.canDropItem = false;
                    EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                    break;

                case ItemType.WaterTool:
                    SetWaterGround(currentTile);
                    currentTile.daysSinceWatered = 0;
                    EventHandler.CallPlaySoundEvent(SoundName.Water);
                    break;

                case ItemType.BreakTool:
                case ItemType.ChopTool:
                    crop.ProcessToolAction(itemDetails, crop.tileDetails);
                    break;

                case ItemType.CollectTool:
                    crop.ProcessToolAction(itemDetails, currentTile);
                    break;

                case ItemType.ReapTool:
                    var reapCount = 0;
                    foreach (var t in reapItemsInRadius)
                    {
                        EventHandler.CallParticleEffectEvent(ParticleEffectType.ReapableScenery, t.transform.position + Vector3.up);
                        t.SpawnHarvestItems();
                        Destroy(t.gameObject);

                        // 限制单次收割杂草的上线，避免一下把一片都割掉
                        reapCount++;
                        if (reapCount > Settings.reapAmount)
                            break;
                    }

                    EventHandler.CallPlaySoundEvent(SoundName.Reap);

                    break;

                case ItemType.Furniture:
                    // 在地图上生成物品 ItemMgr
                    // 在背包中移除当前物品 InventoryMgr
                    // 移除指定材料数量 InventoryMgr
                    EventHandler.CallBuildFurnitureEvent(itemDetails.itemID, mouseWorldPos);
                    break;
            }
        }

        // 将处理过的地面（瓦片）信息更新到该地图所在的瓦片信息字典中
        UpdateTileDetails(currentTile);
    }

    /// 通过物理方法判断鼠标点击位置的农作物
    public Crop GetCropObject(Vector3 mouseWorldPos)
    {
        var colliders = Physics2D.OverlapPointAll(mouseWorldPos);
        Crop currentCrop = null;

        foreach (var t in colliders)
        {
            if (t.GetComponent<Crop>()) currentCrop = t.GetComponent<Crop>();
        }

        return currentCrop;
    }

    /// 返回工具范围内的杂草 
    public bool HaveReapableItemsInRadius(Vector3 mouseWorldPos, ItemDetails tool)
    {
        reapItemsInRadius = new List<ReapItem>();

        var colliders = new Collider2D[20];
        Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);

        if (colliders.Length > 0)
        {
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].GetComponent<ReapItem>())
                {
                    var item = colliders[i].GetComponent<ReapItem>();
                    reapItemsInRadius.Add(item);
                }
            }
        }

        return reapItemsInRadius.Count > 0;
    }

    private void InitTileDetailDict(MapDataSO mapData)
    {
        // 遍历 mapDataSO 中记录的瓦片信息
        foreach (var tileProperty in mapData.tileProperties)
        {
            // 声明一个临时变量，记录每一个瓦片信息
            // 记录 坐标
            var tileDetails = new TileDetails
            {
                gridX = tileProperty.tileCoordinate.x,
                gridY = tileProperty.tileCoordinate.y
            };

            // 设置惟一的 key，由 x+y+场景名称组成
            var key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;

            // 如果字典中已存在此 key对应的瓦片属性，赋值给刚声明的临时变量
            if (GetTileDetails(key) != null) tileDetails = GetTileDetails(key);

            // 根据瓦片种类不同，将每种类型对应的【是否操作】合并
            // 例如一个格子既可以扔东西，又可以种东西
            switch (tileProperty.gridType)
            {
                case GridType.Diggable:
                    tileDetails.canDig = tileProperty.boolType;
                    break;
                case GridType.DropItem:
                    tileDetails.canDropItem = tileProperty.boolType;
                    break;
                case GridType.PlaceFurniture:
                    tileDetails.canPlaceFurniture = tileProperty.boolType;
                    break;
                case GridType.NPCObstacle:
                    tileDetails.isNpcObstacle = tileProperty.boolType;
                    break;
            }

            // 如果字典中此 key对应的组已存在，更新。否则添加新项
            if (GetTileDetails(key) != null)
                tileDetailsDict[key] = tileDetails;
            else
                tileDetailsDict.Add(key, tileDetails);
        }
    }

    /// 根据 key 返回瓦片信息（ key：x+y+地图名字 ）
    public TileDetails GetTileDetails(string key)
    {
        return tileDetailsDict.ContainsKey(key) ? tileDetailsDict[key] : null;
    }

    /// 根据鼠标网格返回瓦片信息
    public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
    {
        var key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;
        return GetTileDetails(key);
    }

    /// 显示挖坑瓦片
    private void SetDigGround(TileDetails tile)
    {
        var pos = new Vector3Int(tile.gridX, tile.gridY, 0);
        if (digTilemap != null)
        {
            digTilemap.SetTile(pos, digTile);
        }
    }

    /// 显示浇水瓦片
    private void SetWaterGround(TileDetails tile)
    {
        var pos = new Vector3Int(tile.gridX, tile.gridY, 0);
        if (waterTilemap != null)
        {
            waterTilemap.SetTile(pos, waterTile);
        }
    }

    /// 更新瓦片信息
    public void UpdateTileDetails(TileDetails tileDetails)
    {
        var key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + SceneManager.GetActiveScene().name;

        if (tileDetailsDict.ContainsKey(key))
        {
            tileDetailsDict[key] = tileDetails;
        }
        else
        {
            tileDetailsDict.Add(key, tileDetails);
        }
    }


    /// 刷新当前地图
    private void RefreshMap()
    {
        if (digTilemap != null) digTilemap.ClearAllTiles();
        if (waterTilemap != null) waterTilemap.ClearAllTiles();

        foreach (var crop in FindObjectsOfType<Crop>())
        {
            Destroy(crop.gameObject);
        }

        DisplayMap(SceneManager.GetActiveScene().name);
    }

    /// 更新地图。更新被处理过的地面（挖坑，浇水，种子等）
    private void DisplayMap(string sceneName)
    {
        // 遍历已保存的地图信息
        foreach (var tile in tileDetailsDict)
        {
            var key = tile.Key;
            var tileDetails = tile.Value;

            // 如果其中有包含所要查询的场景名称的 key，那么把这个场景中的所有瓦片都查询一遍（因为 key包含 xy坐标，每个记录的瓦片遍历一次）
            if (key.Contains(sceneName))
            {
                // 被挖过坑的瓦片，更新挖坑瓦片图
                if (tileDetails.daysSinceDug > -1)
                    SetDigGround(tileDetails);

                // 被浇过水的瓦片，更新浇水瓦片图
                if (tileDetails.daysSinceWatered > -1)
                    SetWaterGround(tileDetails);

                // 种子
                if (tileDetails.seedItemID > -1)
                    EventHandler.CallPlantSeed(tileDetails.seedItemID, tileDetails);
            }
        }
    }

    /// 根据场景名字构建网络范围，输出范围和原点
    public bool GetGridDimensions(string sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin)
    {
        gridDimensions = Vector2Int.zero;
        gridOrigin = Vector2Int.zero;

        foreach (var mapData in mapDataList)
        {
            if (mapData.sceneName == sceneName)
            {
                gridDimensions.x = mapData.gridWidth;
                gridDimensions.y = mapData.gridHeight;

                gridOrigin.x = mapData.originX;
                gridOrigin.y = mapData.originY;

                return true;
            }
        }

        return false;
    }

    public string GUID => GetComponent<DataGUID>().guid;

    public GameSaveData GenerateSaveData()
    {
        var saveData = new GameSaveData();
        saveData.tileDetailsDict = tileDetailsDict;
        saveData.firstLoadDict = firstLoadDict;
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        tileDetailsDict = saveData.tileDetailsDict;
        firstLoadDict = saveData.firstLoadDict;
    }
}