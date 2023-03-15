using UnityEngine;

namespace Y.Inventory
{
    // 被扔出来的物体的 阴影组件
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemShadow : MonoBehaviour
    {
        // 物品 Sprite
        public SpriteRenderer itemSprite;
        // 用于显示阴影的 Sprite
        private SpriteRenderer shadowSprite;

        private void Awake()
        {
            shadowSprite = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            // 设置阴影的图片为物品的图片，并且设置为灰黑色
            shadowSprite.sprite = itemSprite.sprite;
            shadowSprite.color = new Color(0, 0, 0, 0.3f);
        }
    }
}