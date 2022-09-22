using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemDetails
{
    public int itemID;
    public string itemName;

    public ItemType itemType;

    public Sprite itemIcon;

    // 在世界地图上产生时所显示的图片
    public Sprite ItemOnWorldSprite;

    public string itemDescription;

    // 使用范围
    public int itemUseRadius;

    // 是否可被拾取，丢弃，举着
    public bool canPickedup;
    public bool canDropped;
    public bool canCarried;

    // 价值
    public int itemPrice;

    // 出售折损比例
    [Range(0, 1)] public float sellPercentage;
}

[Serializable]
public struct InventoryItem
{
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


// todo: 为什么说 v3不能保存人物位置？
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


[Serializable]
public class TileProperty
{
    public Vector2Int tileCoordinate;

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