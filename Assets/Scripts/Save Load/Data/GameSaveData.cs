using System;
using System.Collections.Generic;

namespace Y.Save
{
    [Serializable]
    public class GameSaveData
    {
        public string dataSceneName;

        /// 储存人物坐标  string:人物名称
        public Dictionary<string, SerializableVector3> characterPosDict;
        
        /// 储存场景物品
        public Dictionary<string, List<SceneItem>> sceneItemDict;
        /// 储存场景家具
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDict;
        
        /// 储存瓦片信息
        public Dictionary<string, TileDetails> tileDetailsDict;
        
        /// 储存场景是否是第一次加载
        public Dictionary<string, bool> firstLoadDict;
        
        /// 储存背包和箱子物品信息
        public Dictionary<string, List<InventoryItem>> inventoryDict;
        
        /// 储存时间
        public Dictionary<string, int> timeDict;
        
        /// 储存人物金钱
        public int playerMoney;
        
        // Npc
        public string targetScene;
        public bool interactable;
        public int animationInstanceID;
    }
}