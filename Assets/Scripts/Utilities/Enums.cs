public enum ItemType
{
    Seed,           // 种子
    Commodity,      // 商品
    Furniture,      // 家具

    HoeTool,        // 锄头
    ChopTool,       // 砍树的
    BreakTool,      // 砸石头的
    ReapTool,       // 割草的
    WaterTool,      // 浇水的
    CollectTool,    // 收集的

    ReapableScenery // 可收集的
}


public enum SlotType
{
    Bag,
    Box, 
    Shop
}

public enum InventoryLocation
{
    Player,
    Box
}

public enum PartType
{
    None,
    Carry,
    Hoe,
    Water,
    Collect,
    Chop,
    Break,
    Reap
}

public enum PartName
{
    Body,
    Hair,
    Arm, 
    Tool
}

public enum Season
{
    春天,
    夏天,
    秋天,
    冬天
}

// 格子类型
public enum GridType
{
    Diggable,
    DropItem,
    PlaceFurniture,
    NPCObstacle
}

public enum ParticleEffectType
{
    None,
    // 树叶
    LeavesFalling01,
    LeavesFalling02,
    // 石头
    Rock,
    // 稻草
    ReapableScenery
}

public enum GameState
{
    GamePlay,
    Pause
}

public enum LightShift
{
    Morning,
    Night
}

public enum SoundName
{
    // 空，走路
    none, FootStepSoft, FootStepHard,

    // 使用工具的
    Axe, Pickaxe, Hoe, Reap, Water, Basket, Chop,

    // 采摘，种植，树倒下，割草
    Pickup, Plant, TreeFalling, Rustle,
    
    // 环境音，背景音乐
    AmbientCountryside1, AmbientCountryside2, MusicCalm1, MusicCalm2, MusicCalm3, MusicCalm4, MusicCalm5, MusicCalm6, AmbientIndoor1
}