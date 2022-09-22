using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;

    public TileDetails tileDetails;

    private Animator anim;

    private int harvestActionCount;

    public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

    private Transform PlayerTransform => FindObjectOfType<Player>().transform;

    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {
        tileDetails = tile;

        // 工具使用次数
        var requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        if (requireActionCount == -1) return;

        anim = GetComponentInChildren<Animator>();

        // 计数器判断
        if (harvestActionCount < requireActionCount)
        {
            harvestActionCount++;

            // 动画
            if (anim != null && cropDetails.hasAnimation) anim.SetTrigger(PlayerTransform.position.x < transform.position.x ? "RotateRight" : "RotateLeft");

            // 播放粒子
            if (cropDetails.hasParticalEffect)
                EventHandler.CallParticleEffectEvent(cropDetails.particleEffectType, transform.position + cropDetails.effectPos);

            // 播放声音
            if (cropDetails.soundEffect != SoundName.none)
            {
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }
        }

        if (harvestActionCount >= requireActionCount)
        {
            // 如果该东西是生成在人物头顶
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
            {
                // 生成农作物
                SpawnHarvestItems();
            }
            else if (cropDetails.hasAnimation)
            {
                anim.SetTrigger(PlayerTransform.position.x < transform.position.x ? "FallingRight" : "FallingLeft");

                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                StartCoroutine(HarvestAfterAnimation());
            }
        }
    }

    private IEnumerator HarvestAfterAnimation()
    {
        // 如果当前播放的动画名称不是 End
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("End"))
        {
            yield return null;
        }

        // 生成果实
        SpawnHarvestItems();

        // 如果需要转换新物体
        if (cropDetails.transferItemID > 0) CreateTransferCrop();
    }

    /// 生成转换的物体 
    private void CreateTransferCrop()
    {
        tileDetails.seedItemID = cropDetails.transferItemID;
        tileDetails.daysSinceLastHarvest = -1;
        tileDetails.growthDays = 0;

        EventHandler.CallRefreshCurrentMap();
    }


    /// 生成果实
    private void SpawnHarvestItems()
    {
        for (var i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            int amountToProduce;

            // 最大最小值一样的话，认为生成指定数量的
            if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
                amountToProduce = cropDetails.producedMinAmount[i];
            else
                // +1因为 Random.Range区间为 [..)
                amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);

            // 执行生成指定数量的物品
            for (var j = 0; j < amountToProduce; j++)
            {
                // 在人物头上生成
                if (cropDetails.generateAtPlayerPosition)
                {
                    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                }
                // 在世界地图上生成物品（碎石，木头，果实等）
                else
                {
                    // 判断应该生成的物品方向
                    var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;

                    // 一定范围内的随机
                    var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX),
                        transform.position.y + Random.Range(cropDetails.spawnRadius.y, -cropDetails.spawnRadius.y), 0);

                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                }
            }
        }

        if (tileDetails != null)
        {
            tileDetails.daysSinceLastHarvest++;

            // 如果 作物再次生长日期>0 并且 已收割次数没有达到最多收割次数
            if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes)
            {
                // cropDetails.daysToRegrow 意思为
                // 例如：种植葡萄，希望收割后回到第三阶段，那么 cropDetails.daysToRegrow = 第四+第五阶段所需的日期
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;

                // 刷新种子
                EventHandler.CallRefreshCurrentMap();
            }
            // 不可重复生长
            else
            {
                tileDetails.daysSinceLastHarvest = -1;
                tileDetails.seedItemID = -1;

                // tileDetails.daysSinceDug = -1;
            }

            Destroy(gameObject);
        }
    }
}