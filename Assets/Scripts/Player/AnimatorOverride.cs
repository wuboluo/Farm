using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Y.Inventory;

public class AnimatorOverride : MonoBehaviour
{
    public SpriteRenderer holdItem;

    // 此列表为身体各部分在不同状态下使用的animator。例如胳膊在跑步、浇水、举起东西等
    [Header("各部分动画列表")] public List<AnimatorType> animatorTypes;

    private readonly Dictionary<string, Animator> animatorNameDict = new();
    private Animator[] animators;

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();

        foreach (Animator anim in animators) animatorNameDict.Add(anim.name, anim);
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
    }


    // 切换场景时，恢复人物动画
    private void OnBeforeSceneUnload()
    {
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }

    private void OnHarvestAtPlayerPosition(int id)
    {
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(id).ItemOnWorldSprite;

        if (holdItem.enabled == false) StartCoroutine(ShowItem(itemSprite));
    }

    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.enabled = true;
        holdItem.sprite = itemSprite;
        yield return new WaitForSeconds(1);

        holdItem.enabled = false;
    }

    /// 当点击物品时，判断此物品类型，用于切换动画 
    private void OnItemSelectedEvent(ItemDetails details, bool isSelected)
    {
        // todo: 不同的工具返回不同动画 在这里补全
        // 判断物品类型
        PartType currentType = details.itemType switch
        {
            ItemType.Seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            ItemType.Furniture => PartType.Carry,
            
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.CollectTool => PartType.Collect,
            ItemType.ChopTool => PartType.Chop,
            ItemType.BreakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            
            _ => PartType.None
        };

        // 若没选择物体，则恢复正常状态，被举起的物品图标隐藏
        if (isSelected == false)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else
        {
            // 若选择了物体，并且这个物体可以被举起，那么切换对应的图标，并显示它
            if (currentType == PartType.Carry)
            {
                holdItem.sprite = details.ItemOnWorldSprite;
                holdItem.enabled = true;
            }

            // 若从可举起的物品切换到不可举起的时候，关闭举起的图标。例如：先拿着土豆种子，后选择锄头，此时应该关闭土豆种子图标
            else
            {
                holdItem.enabled = false;
            }
        }

        SwitchAnimator(currentType);
    }

    private void SwitchAnimator(PartType partType)
    {
        // 注意此时 item.partName是要和 animator文件名称一致，从开始存入的字典里面找到对应的 animator组件，替换为新状态下的 animator，实现动作切换

        // 遍历所有身体部分在不同状态下对应的 animator
        foreach (AnimatorType item in animatorTypes)
        {
            // 如果当前使用的道具存在独立的一组动画
            if (item.partType == partType)
            {
                // 那么就把身体各部分的animator都更新
                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
            }
            // 如果不存在单独的一组动画
            else if (item.partType == PartType.None)
            {
                // 就恢复到默认状态
                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
            }
        }
    }
}