using System.Linq;
using UnityEngine;

public class CropManager : Singleton<CropManager>
{
    public CropDataListSO cropData;

    private Transform cropParent;
    private Grid currentGrid;
    private Season currentSeason;

    private void OnEnable()
    {
        EventHandler.PlantSeedEvent += OnPlantSeed;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
        EventHandler.GameDayEvent += OnGameDay;
    }

    private void OnDisable()
    {
        EventHandler.PlantSeedEvent -= OnPlantSeed;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
        EventHandler.GameDayEvent -= OnGameDay;
    }

    private void OnPlantSeed(int id, TileDetails tileDetails)
    {
        var currentCrop = GetCropDetails(id);

        // 种子不为空 并且 当前季节可以播种 并且 该瓦片内没有被播种
        // 用于第一次播种
        if (currentCrop != null && SeasonAvailable(currentCrop) && tileDetails.seedItemID == -1)
        {
            tileDetails.seedItemID = id;
            tileDetails.growthDays = 0;

            // 显示农作物
            DisplayCropPlant(tileDetails, currentCrop);
        }
        // 用于刷新地图
        else if (tileDetails.seedItemID != -1)
        {
            // 显示农作物
            DisplayCropPlant(tileDetails, currentCrop);
        }
    }

    private void OnAfterSceneLoaded()
    {
        currentGrid = FindObjectOfType<Grid>();
        cropParent = GameObject.FindWithTag("CropParent").transform;
    }

    private void OnGameDay(int day, Season season)
    {
        currentSeason = season;
    }

    /// 根据物品 id查找种子信息
    public CropDetails GetCropDetails(int id)
    {
        return cropData.cropDetailsList.Find(c => c.seedItemID == id);
    }


    /// 判断当前季节是否可以种植
    private bool SeasonAvailable(CropDetails crop)
    {
        // 当前季节等于该农作物任意一个可播种的季节就返回 true
        return crop.seasons.Any(t => t == currentSeason);
    }


    /// 显示农作物
    private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
    {
        // 成长阶段
        var growthStages = cropDetails.growthDays.Length;
        var currentStage = 0;
        var dayCounter = cropDetails.TotalGrowthDays;

        // 倒序计算当前的成长阶段
        for (var i = growthStages - 1; i >= 0; i--)
        {
            if (tileDetails.growthDays >= dayCounter)
            {
                currentStage = i;
                break;
            }
            dayCounter -= cropDetails.growthDays[i];
        }

        // 获取当前阶段的 Prefab
        var cropPrefab = cropDetails.growthPrefabs[currentStage];
        var cropSprite = cropDetails.growthSprites[currentStage];

        var pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);

        var cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
        cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;

        cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
        cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
    }
}