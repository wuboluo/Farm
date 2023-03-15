using UnityEngine;

namespace Y.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        private Transform spriteTrans;
        private BoxCollider2D coll;

        // 下降速度
        public float gravity = -3.5f;
        // 是否落地
        private bool isGround;
        
        // 飞行方向
        private Vector2 direction;
        // 落地坐标
        private Vector3 targetPos;
        // 飞行距离
        private float distance;

        private void Awake()
        {
            spriteTrans = transform.GetChild(0);
            
            // 关闭碰撞体，避免碰到人物又被拾取回去
            coll = GetComponent<BoxCollider2D>();
            coll.enabled = false;
        }

        private void Update()
        {
            Bounce();
        }

        /// 初始化被扔的物品
        public void InitBounceItem(Vector3 target, Vector2 dir)
        {
            coll.enabled = false;
            // 设置此物体飞向的方向、坐标、距离
            direction = dir;
            targetPos = target;
            distance = Vector3.Distance(target, transform.position);

            // 1.5 是因为要从人物头顶扔出，人物举起物品的节点【HoldItem】的 Y轴为 1.5
            spriteTrans.position += Vector3.up * 1.5f;
        }

        /// 物体被扔出去的飞行运算
        private void Bounce()
        {
            // Y轴小于目标点就认为已经落地
            isGround = spriteTrans.position.y <= transform.position.y;

            // 判断是否飞到目标位置
            if (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position += (Vector3) direction * (distance * -gravity * Time.deltaTime);
            }

            // 高度下降
            if (!isGround)
            {
                spriteTrans.position += Vector3.up * (gravity * Time.deltaTime);
            }
            // 落地后，位置统一，开启碰撞
            else
            {
                spriteTrans.position = transform.position;
                coll.enabled = true;
            }
        }
    }
}