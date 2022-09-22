using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReapItem : MonoBehaviour
{
    private CropDetails cropDetails;

    private Transform PlayerTransform => FindObjectOfType<Player>().transform;


    public void Init(int id)
    {
        cropDetails = CropManager.Instance.GetCropDetails(id);
    }
    
    public void SpawnHarvestItems()
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
    }
}