using UnityEngine;

namespace Y.Inventory
{
    public class Box : MonoBehaviour
    {
        public InventoryBagSO boxBagTemplate;
        public InventoryBagSO boxBagData;

        public GameObject mouseIcon;

        public int index;
        private bool canOpen;
        private bool isOpen;

        private void Update()
        {
            if (!isOpen && canOpen && Input.GetMouseButtonDown(1))
            {
                // 打开箱子
                EventHandler.CallBaseBagOpenEvent(SlotType.Box, boxBagData);
                isOpen = true;
            }

            if (!canOpen && isOpen)
            {
                // 关闭箱子
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }

            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                // 关闭箱子
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }
        }

        private void OnEnable()
        {
            if (boxBagData == null) boxBagData = Instantiate(boxBagTemplate);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = true;
                mouseIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = false;
                mouseIcon.SetActive(false);
            }
        }

        /// 只有在初始创建和重建时会执行 
        public void InitBox(int boxIndex)
        {
            index = boxIndex;
            string key = name + index;

            // 刷新地图读取数据
            if (InventoryManager.Instance.GetBoxDataList(key) != null)
            {
                // 从字典中找到当前箱子，并取出数据赋值
                boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            // 新建箱子
            else
            {
                InventoryManager.Instance.AddBoxDataDict(this);
            }
        }
    }
}