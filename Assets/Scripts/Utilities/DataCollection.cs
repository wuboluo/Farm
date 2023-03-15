using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemDetails
{
    // 物品惟一ID，名称
    public int itemID;
    public string itemName;

    // 物品类型（例如种子、商品、家具等）
    public ItemType itemType;

    // UI和道具编辑器上的图标
    public Sprite itemIcon;
    // 在世界地图上产生时所显示的图片
    public Sprite ItemOnWorldSprite;

    // 物品描述
    public string itemDescription;

    // 使用范围（周围几个网格内可以使用）
    public int itemUseRadius;

    // 是否可被拾取、丢弃、举起
    public bool canPickedUp;
    public bool canDropped;
    public bool canCarried;

    // 价值
    public int itemPrice;
    // 出售折损比例
    [Range(0, 1)] 
    public float sellPercentage;
}

// 仓库物品结构
[Serializable]
public struct InventoryItem
{
    // 在仓库内（背包、储物箱、货架等）保存物品用的结构
    // 使用 struct而不是class的原因是：struct会避免检查空的步骤，而是直接用 itemID去判断此位置是否为空。同时移除物品的时候也只需要清零即可，无需删除
    
    public int itemID;
    public int itemAmount;
}

[Serializable]
public class AnimatorType
{
    public PartName partName;
    public PartType partType;
    public AnimatorOverrideController overrideController;
}


// Unity不支持序列化 Vector3，所以要自己实现一个 可序列化的类
[Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int) x, (int) y);
    }
}

[Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}

[Serializable]
public class SceneFurniture
{
    public int furnitureID;
    public SerializableVector3 position;
    public int boxIndex;
}


// 格子属性
[Serializable]
public class TileProperty
{
    // 格子的网格坐标
    public Vector2Int tileCoordinate;

    // 此格子类型，是用于挖坑、放东西、家具或者NPC障碍
    public GridType gridType;
    public bool boolType;
}

[Serializable]
public class TileDetails
{
    public int gridX, gridY;

    public bool canDig;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool isNpcObstacle;

    /// 距离上一次【挖坑】过去了多少天
    public int daysSinceDug = -1;

    /// 距离上一次【浇水】过去了多少天
    public int daysSinceWatered = -1;

    /// 种植的种子信息
    public int seedItemID = -1;

    /// 植物生长的日期
    public int growthDays = -1;

    /// 距离上一次【收割】过去了多少天
    public int daysSinceLastHarvest = -1;
}

[Serializable]
public class NpcPosition
{
    public Transform npc;
    public string startScene;
    public Vector3 position;
}


[Serializable]
public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;

    public List<ScenePath> scenePaths;
}

// 场景路径
[Serializable]
public class ScenePath
{
    public string sceneName;
    public Vector2Int fromGridCell;
    public Vector2Int gotoGridCell;
}