using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "Map/MapData")]
public class MapDataSO : ScriptableObject
{
    [SceneName] public string sceneName;

    [Header("地图信息")]
    public int gridWidth;
    public int gridHeight;

    [Header("左下角原点")]
    public int originX;
    public int originY;
    
    public List<TileProperty> tileProperties;
}