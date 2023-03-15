using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// 在编辑模式下运行
[ExecuteInEditMode]
public class GridMap : MonoBehaviour
{
    // 需要存入的该场景的地图信息SO
    public MapDataSO mapData;
    public GridType gridType;

    private Tilemap currentTilemap;

    private void OnEnable()
    {
        // 如果这个代码/物体没有在运行
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();

            // 如果已添加对应地图的 dataSO文件，则先清空里面存储的 tileProperties数组数据
            if (mapData != null) mapData.tileProperties.Clear();
        }
    }

    private void OnDisable()
    {
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();

            // 更新瓦片信息
            UpdateTileProperties();

#if UNITY_EDITOR
            if (mapData != null) EditorUtility.SetDirty(mapData);
#endif
        }
    }

    /// 把每个区域（挖坑、扔东西...等）所绘制的瓦片添加进此地图信息SO中
    /// 同一个格子可能会被标记为很多种功能的区域，那么这个格子会被重复添加，但是type不同
    private void UpdateTileProperties()
    {
        // 获得瓦片地图中真实有瓦片的范围
        // 例如：10*10的地图中，只有中间 2*2的区域画了东西，则可以通过这个函数压缩至最小可执行范围
        currentTilemap.CompressBounds();

        if (!Application.IsPlaying(this))
        {
            if (mapData != null)
            {
                // 已绘制范围的左下角坐标
                Vector3Int startPos = currentTilemap.cellBounds.min;

                // 已绘制范围的右上角坐标
                Vector3Int endPos = currentTilemap.cellBounds.max;

                for (int x = startPos.x; x < endPos.x; x++)
                for (int y = startPos.y; y < endPos.y; y++)
                {
                    TileBase tile = currentTilemap.GetTile(new Vector3Int(x, y, 0));

                    if (tile != null)
                    {
                        TileProperty newTile = new TileProperty
                        {
                            tileCoordinate = new Vector2Int(x, y),
                            gridType = gridType,
                            boolType = true
                        };

                        mapData.tileProperties.Add(newTile);
                    }
                }
            }
        }
    }
}