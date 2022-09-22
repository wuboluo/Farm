using UnityEngine;

namespace Y.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Item>(out var item))
            {
                if (item.itemDetails.canPickedup)
                {
                    // 拾取物品添加到背包
                    InventoryManager.Instance.AddItem(item, true);
                    
                    // 音效
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);
                }
            }
        }
    }
}