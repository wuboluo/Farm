using UnityEngine;

namespace Y.Inventory
{
    // 
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Item item))
            {
                if (item.itemDetails.canPickedUp)
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