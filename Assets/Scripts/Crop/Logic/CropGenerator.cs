using UnityEngine;

public class CropGenerator : MonoBehaviour
{
    public int seedItemID;
    public int growthDays;
    private Grid currentGrid;

    private void Awake()
    {
        currentGrid = FindObjectOfType<Grid>();
    }

    private void OnEnable()
    {
        EventHandler.GenerateCropEvent += GenerateCrop;
    }

    private void OnDisable()
    {
        EventHandler.GenerateCropEvent -= GenerateCrop;
    }

    private void GenerateCrop()
    {
        Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);

        if (seedItemID != 0)
        {
            TileDetails tile = GridMapManager.Instance.GetTileDetailsOnMousePosition(cropGridPos) ?? new TileDetails
            {
                gridX = cropGridPos.x,
                gridY = cropGridPos.y
            };

            tile.daysSinceWatered = -1;
            tile.seedItemID = seedItemID;
            tile.growthDays = growthDays;

            GridMapManager.Instance.UpdateTileDetails(tile);
        }
    }
}