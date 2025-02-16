using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Y.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public ItemTooltip itemTooltip;

        [Header("拖拽图片")] public Image dragItemImage;

        [Header("玩家背包UI")] [SerializeField] private GameObject bagUI;
        private bool bagOpened;

        [Header("通用背包")] [SerializeField] private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;

        [Header("交易UI")] public TradeUI tradeUI;
        public TextMeshProUGUI playerMoney;

        [SerializeField] private SlotUI[] playerSlots;
        [SerializeField] private List<SlotUI> baseBagSlots;


        private void Start()
        {
            for (int i = 0; i < playerSlots.Length; i++) playerSlots[i].slotIndex = i;

            bagOpened = bagUI.activeInHierarchy;
            playerMoney.text = InventoryManager.Instance.playerMoney.ToString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B)) OpenBagUI();
        }

        private void OnEnable()
        {
            EventHandler.updateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpen;
            EventHandler.BaseBagCloseEvent += OnBaseBagClose;
            EventHandler.ShowTradeUI += OnShowTradeUI;
        }

        private void OnDisable()
        {
            EventHandler.updateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpen;
            EventHandler.BaseBagCloseEvent -= OnBaseBagClose;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
        }

        private void OnShowTradeUI(ItemDetails details, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(details, isSell);
        }

        // 切换场景时，取消物品选择高亮
        private void OnBeforeSceneUnload()
        {
            // -1：取消所有
            UpdateSlotHighlight(-1);
        }

        private void OnBaseBagOpen(SlotType slotType, InventoryBagSO bagSO)
        {
            GameObject prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null
            };

            // 打开背包
            baseBag.SetActive(true);

            baseBagSlots = new List<SlotUI>();
            for (int i = 0; i < bagSO.itemList.Count; i++)
            {
                SlotUI slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
                slot.slotIndex = i;
                baseBagSlots.Add(slot);
            }

            // 强制刷新 UI
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

            // 如果打开的是商店的 UI
            if (slotType == SlotType.Shop)
            {
                // 顺便打开人物的背包，方便购买和拖拽
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-1, 0.5f);
                bagUI.SetActive(true);
                bagOpened = true;
            }

            OnUpdateInventoryUI(InventoryLocation.Box, bagSO.itemList);
        }

        private void OnBaseBagClose(SlotType slotType, InventoryBagSO bagSO)
        {
            baseBag.SetActive(false);
            itemTooltip.gameObject.SetActive(false);

            UpdateSlotHighlight(-1);

            foreach (SlotUI slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }

            baseBagSlots.Clear();

            // 如果关闭的是商店的 UI，同时关闭玩家背包 UI并且归位
            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                bagUI.SetActive(false);
                bagOpened = false;
            }
        }

        // 更新玩家背包内物品 UI显示信息
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                // 更新背包UI数据
                case InventoryLocation.Player:
                    // 遍历背包中的所有格子
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        // 检查背包数据SO中，对应序号是否存在物品
                        if (list[i].itemAmount > 0)
                        {
                            // 将背包数据SO中的物品信息更新给格子UI
                            ItemDetails item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            // 不存在物品就清空格子
                            playerSlots[i].UpdateEmptySlot();
                        }
                    }

                    break;

                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                        if (list[i].itemAmount > 0)
                        {
                            ItemDetails item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            baseBagSlots[i].UpdateEmptySlot();
                        }

                    break;
            }

            playerMoney.text = InventoryManager.Instance.playerMoney.ToString();
        }

        /// 打开背包 UI
        public void OpenBagUI()
        {
            bagOpened = !bagOpened;
            bagUI.SetActive(bagOpened);
        }

        /// 更新Slot高亮显示
        public void UpdateSlotHighlight(int index)
        {
            foreach (SlotUI slot in playerSlots)
            {
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHighlight.gameObject.SetActive(true);
                }
                else
                {
                    slot.isSelected = false;
                    slot.slotHighlight.gameObject.SetActive(false);
                }
            }
        }
    }
}