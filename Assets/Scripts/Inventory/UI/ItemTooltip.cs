using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Y.Inventory;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Text valueText;
    [SerializeField] private GameObject bottomPart;

    [Header("建造")] public GameObject resourcePanel;
    [SerializeField] private Image[] resourceItems;

    public void SetUpTooltip(ItemDetails details, SlotType slotType)
    {
        nameText.text = details.itemName;
        typeText.text = GetItemType(details.itemType);
        descriptionText.text = details.itemDescription;


        if (details.itemType is ItemType.Seed or ItemType.Commodity or ItemType.Furniture)
        {
            bottomPart.SetActive(true);

            int price = details.itemPrice;
            if (slotType == SlotType.Bag)
            {
                price = (int) (price * details.sellPercentage);
            }

            valueText.text = price.ToString();
        }
        else
        {
            bottomPart.SetActive(false);
        }

        // 立刻刷新
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private string GetItemType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Seed => "种子",
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.BreakTool => "工具",
            ItemType.ChopTool => "工具",
            ItemType.CollectTool => "工具",
            ItemType.HoeTool => "工具",
            ItemType.ReapTool => "工具",
            ItemType.WaterTool => "工具",
            _ => "无"
        };
    }

    public void SetupResourcePanel(int id)
    {
        BluePrintDetails bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(id);

        for (int i = 0; i < resourceItems.Length; i++)
        {
            if (i < bluePrintDetails.resourceItem.Length)
            {
                InventoryItem item = bluePrintDetails.resourceItem[i];

                resourceItems[i].gameObject.SetActive(true);
                resourceItems[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
                resourceItems[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.itemAmount.ToString();
            }
            else
            {
                resourceItems[i].gameObject.SetActive(false);
            }
        }
    }
}