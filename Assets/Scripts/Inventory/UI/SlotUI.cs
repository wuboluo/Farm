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

        private InventoryLocation Location
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

        /// 开始拖拽时
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

        /// 拖拽中
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            inventoryUI.dragItemImage.transform.position = Input.mousePosition;
        }

        /// 结束拖拽时
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            inventoryUI.dragItemImage.enabled = false;
            // Debug.Log(eventData.pointerCurrentRaycast.gameObject);

            // 如果拽到了一个UI物体上
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                // 但是这个UI物体不是 SlotUI，没用，直接返回
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    return;

                SlotUI targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                int targetIndex = targetSlot.slotIndex;

                switch (slotType)
                {
                    // 在Player自身背包范围内交换
                    case SlotType.Bag when targetSlot.slotType == SlotType.Bag:
                        InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                        break;
                    // 买
                    case SlotType.Shop when targetSlot.slotType == SlotType.Bag:
                        EventHandler.CallShowTradeUI(itemDetails, false);
                        break;
                    // 卖
                    case SlotType.Bag when targetSlot.slotType == SlotType.Shop:
                        EventHandler.CallShowTradeUI(itemDetails, true);
                        break;

                    default:
                    {
                        // 箱子到人物或人物到箱子
                        if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                        {
                            // 跨背包交换物品
                            InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetIndex);
                        }

                        break;
                    }
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

        /// 将格子更新为空
        public void UpdateEmptySlot()
        {
            // 如果当前被选中，就取消选择
            if (isSelected)
            {
                isSelected = false;

                inventoryUI.UpdateSlotHighlight(-1);
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }

            // 清空格子显示的信息
            itemDetails = null;
            slotImage.enabled = false;
            amountText.text = string.Empty;
            button.interactable = false;
        }

        /// 更新格子 UI和信息
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