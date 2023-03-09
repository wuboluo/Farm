using UnityEngine;
using UnityEngine.EventSystems;

namespace Y.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private SlotUI slotUI;
        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotUI.itemDetails != null)
            {
                // 显示信息提示栏，设置信息内容
                inventoryUI.itemTooltip.gameObject.SetActive(true);
                inventoryUI.itemTooltip.SetUpTooltip(slotUI.itemDetails, slotUI.slotType);

                // 根据提示栏的锚点Y的位置为底部，调整提示栏到合适的高度
                inventoryUI.itemTooltip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
                inventoryUI.itemTooltip.transform.position = transform.position + Vector3.up * 60;

                // 如果是材料，显示建造蓝图
                if (slotUI.itemDetails.itemType == ItemType.Furniture)
                {
                    inventoryUI.itemTooltip.resourcePanel.SetActive(true);
                    inventoryUI.itemTooltip.SetupResourcePanel(slotUI.itemDetails.itemID);
                }
                else
                {
                    inventoryUI.itemTooltip.resourcePanel.SetActive(false);
                }
            }
            else
            {
                inventoryUI.itemTooltip.gameObject.SetActive(false);
            }
        }

        // 鼠标移出物品格子区域时，隐藏物品提示信息
        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemTooltip.gameObject.SetActive(false);
        }
    }
}