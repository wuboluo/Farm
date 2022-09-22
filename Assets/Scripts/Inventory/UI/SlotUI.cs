using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Y.Inventory
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("组件获取")] [SerializeField] private Image slotImage;
        [SerializeField] private TextMeshProUGUI amountText;
        public Image slotHighlight;
        [SerializeField] private Button button;

        [Header("各自类型")] public SlotType slotType;

        public bool isSelected;

        public int slotIndex;

        public ItemDetails itemDetails;
        public int itemAmount;

        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player
                };
            }
        }

        private bool isDragging;

        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        private void Start()
        {
            isSelected = false;
            if (itemDetails == null) UpdateEmptySlot();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemDetails == null) return;

            inventoryUI.dragItemImage.enabled = true;
            inventoryUI.dragItemImage.sprite = slotImage.sprite;
            inventoryUI.dragItemImage.SetNativeSize();
            isSelected = true;
            inventoryUI.UpdateSlotHighlight(slotIndex);

            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            inventoryUI.dragItemImage.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            inventoryUI.dragItemImage.enabled = false;
            // Debug.Log(eventData.pointerCurrentRaycast.gameObject);

            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    return;

                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                var targetIndex = targetSlot.slotIndex;

                //在Player自身背包范围内交换
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }
                else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag) // 买
                {
                    EventHandler.CallShowTradeUI(itemDetails, false);
                }
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop) // 卖
                {
                    EventHandler.CallShowTradeUI(itemDetails, true);
                }
                else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType) // 箱子到人物或人物到箱子
                {
                    // 跨背包交换物品
                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetIndex);
                }

                //清空所有高亮显示
                inventoryUI.UpdateSlotHighlight(-1);

                isDragging = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (itemDetails == null) return;
            isSelected = !isSelected;

            inventoryUI.UpdateSlotHighlight(slotIndex);

            if (slotType == SlotType.Bag)
                // 通知物品被选中的状态和信息
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
        }

        // 将格子更新为空
        public void UpdateEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;

                inventoryUI.UpdateSlotHighlight(-1);
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }

            itemDetails = null;
            slotImage.enabled = false;
            amountText.text = string.Empty;
            button.interactable = false;
        }

        // 更新格子 UI和信息
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = item.itemIcon;
            slotImage.enabled = true;
            itemAmount = amount;
            amountText.text = amount.ToString();
            button.interactable = true;
        }
    }
}