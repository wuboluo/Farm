using System;
using UnityEngine;

public static class Settings
{
    // -------------------- 可半透明物体的透明度设置（树，灌木丛等）
    public const float itemFadeDuration = 0.35f;
    public const float targetAlpha = 0.45f;


    // -------------------- 时间相关
    public const float secondThreshold = 0.1f; // 数值越小时间越快
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 10; // 一个月设置为10天，加快游戏进度，避免一年过太慢
    public const int seasonHold = 3; // 每交替 3次季节，认为过去一年
    
    
    // -------------------- 场景切换
    public const float fadeDuration = 0.35f;
    
    // -------------------- 单次收割杂草上限
    public const int reapAmount = 2; 
    
    // -------------------- Npc网格移动
    public const float gridCellSize = 1f;
    public const float gridCellDiagonalSize = 1.41f;

    // -------------------- 像素距离
    public const float pixelSize = 0.05f;   // 像素比例是 20*20，占一个 unit，1/20=0.05
    
    // -------------------- Npc动画间隔
    public const float animationBreakTime = 5;

    // 限制 npc移动边界（sceneRoute中设置为 99999，判断时只要比它小）
    public const int maxGridSize = 9999;
    
    
    // -------------------- 灯光
    public const float lightChangeDuration = 25f;
    public static TimeSpan morningTime = new(5, 0, 0);
    public static TimeSpan nightTime = new(19, 0, 0);
    
    
    // -------------------- player 初始坐标
    public static Vector3 playerStartPos = new(-12, -16.5f, 0);

    // -------------------- player 初始金钱
    public const int playerStartMoney = 100;
}