using System;
using UnityEngine;

namespace Y.Inventory
{
    public class Item : MonoBehaviour
    {
        public int itemID;

        private SpriteRenderer spriteRenderer;
        public ItemDetails itemDetails;

        private BoxCollider2D coll;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            coll = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (itemID != 0) Init(itemID);
        }

        public void Init(int id)
        {
            itemID = id;

            // 从 InventoryManager 获得当前数据
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);

            if (itemDetails != null)
            {
                spriteRenderer.sprite = itemDetails.ItemOnWorldSprite ? itemDetails.ItemOnWorldSprite : itemDetails.itemIcon;

                // 修改碰撞体尺寸
                var newSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
                coll.size = newSize;
                coll.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
               
                
                if (itemDetails.itemType == ItemType.ReapableScenery)
                {
                    gameObject.AddComponent<ReapItem>();
                    gameObject.GetComponent<ReapItem>().Init(itemDetails.itemID);
                    
                    gameObject.AddComponent<ItemInteractive>();
                }
            }
        }
    }
}