using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// 在编辑模式下运行
[ExecuteInEditMode]
public class GridMap : MonoBehaviour
{
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

            UpdateTileProperties();

#if UNITY_EDITOR
            if (mapData != null) EditorUtility.SetDirty(mapData);
#endif
        }
    }

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