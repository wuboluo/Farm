using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Y.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public ItemTooltip itemTooltip;

        [Header("拖拽图片")] 
        public Image dragItemImage;
        
        [Header("玩家背包UI")] 
        [SerializeField] private GameObject bagUI;
        private bool bagOpened;

        [Header("通用背包")] 
        [SerializeField] private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;

        [Header("交易UI")] 
        public TradeUI tradeUI;
        public TextMeshProUGUI playerMoney;

        [SerializeField] private SlotUI[] playerSlots;
        [SerializeField] private List<SlotUI> baseBagSlots;


        private void Start()
        {
            for (var i = 0; i < playerSlots.Length; i++) playerSlots[i].slotIndex = i;

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
            var prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null
            };

            // 打开背包
            baseBag.SetActive(true);

            baseBagSlots = new List<SlotUI>();
            for (var i = 0; i < bagSO.itemList.Count; i++)
            {
                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
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

            foreach (var slot in baseBagSlots)
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
                case InventoryLocation.Player:
                    for (var i = 0; i < playerSlots.Length; i++)
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            playerSlots[i].UpdateEmptySlot();
                        }

                    break;

                case InventoryLocation.Box:
                    for (var i = 0; i < baseBagSlots.Count; i++)
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
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

        // 打开背包 UI
        public void OpenBagUI()
        {
            bagOpened = !bagOpened;
            bagUI.SetActive(bagOpened);
        }

        // 更新 slot高亮显示
        public void UpdateSlotHighlight(int index)
        {
            foreach (var slot in playerSlots)
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