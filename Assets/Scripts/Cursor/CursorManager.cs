using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Y.Inventory;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed, item;
    private Grid currentGrid;

    private ItemDetails currentItem;

    // 当前鼠标图片
    private Sprite currentSprite;
    private RectTransform cursorCanvas;

    private bool cursorEnable;

    private Image cursorImage;
    private bool cursorPositionValid;

    // 建造
    private Image buildImage;


    // 鼠标检测
    private Camera mainCam;
    private Vector3Int mouseGridPos;

    private Vector3 mouseWorldPos;


    private static Transform PlayerTransform => FindObjectOfType<Player>().transform;

    private void Start()
    {
        mainCam = Camera.main;

        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();

        // 拿到建造图标
        buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
        buildImage.gameObject.SetActive(false);

        currentSprite = normal;
        SetCursorImage(normal);
    }

    private void Update()
    {
        if (cursorCanvas == null) return;

        cursorImage.transform.position = Input.mousePosition;

        // 和 UI交互时，始终显示默认图标。否则根据不同种类的 item切换
        // cursorEnable：代表选中了一个 itemBar里面的一个物品
        if (!InteractWithUI() && cursorEnable)
        {
            SetCursorImage(currentSprite);
            CheckCursorValid();
            CheckPlayerInput();
        }
        else
        {
            SetCursorImage(normal);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelected;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelected;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
    }

    private void OnBeforeSceneUnload()
    {
        cursorEnable = false;
    }

    private void OnAfterSceneLoaded()
    {
        currentGrid = FindObjectOfType<Grid>();
    }

    private void CheckPlayerInput()
    {
        if (Input.GetMouseButtonDown(0) && cursorPositionValid)
            // 执行方法
            EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
    }

    // 物品选择事件函数
    private void OnItemSelected(ItemDetails details, bool isSelected)
    {
        // 没有选择任何类型的物品时，恢复默认图标
        if (!isSelected)
        {
            currentItem = null;
            cursorEnable = false;
            currentSprite = normal;
            buildImage.gameObject.SetActive(false);
        }
        else
        {
            currentItem = details;
            currentSprite = details.itemType switch
            {
                ItemType.Seed => seed,
                ItemType.Commodity => item,
                ItemType.ChopTool => tool,
                ItemType.HoeTool => tool,
                ItemType.WaterTool => tool,
                ItemType.BreakTool => tool,
                ItemType.ReapTool => tool,
                ItemType.Furniture => tool,
                ItemType.CollectTool => tool,
                _ => normal
            };

            cursorEnable = true;

            // 显示建造物品图片
            if (currentItem.itemType == ItemType.Furniture)
            {
                buildImage.gameObject.SetActive(true);
                buildImage.sprite = details.ItemOnWorldSprite;
                buildImage.SetNativeSize();
            }
        }
    }


    // 判断鼠标是否可用
    private void CheckCursorValid()
    {
        // 根据鼠标的坐标，先转换为世界坐标，再转换为网格坐标
        mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCam.transform.position.z));
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

        // 建造图片跟随移动
        buildImage.rectTransform.position = Input.mousePosition;

        // 获取人物的网格坐标
        Vector3Int playerGridPos = currentGrid.WorldToCell(PlayerTransform.position);

        // 判断是否在物品适用范围内
        if (Mathf.Abs(mouseGridPos.x - playerGridPos.x) > currentItem.itemUseRadius || Mathf.Abs(mouseGridPos.y - playerGridPos.y) > currentItem.itemUseRadius)
        {
            // 如果 x,y 其一超出范围，则设置为不可用，并跳出此方法
            SetCursorInValid();
            return;
        }

        // 获取当前鼠标位置所在的网格
        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
        Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);

        // 网格不为空
        if (currentTile != null)
        {
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);

            switch (currentItem.itemType)
            {
                case ItemType.Seed:
                    if (currentTile.daysSinceDug > -1 && currentTile.seedItemID == -1) SetCursorValid();
                    else SetCursorInValid();
                    break;

                case ItemType.Commodity:
                    // 这个网格可被允许扔东西，并且这个物品可被丢弃，则设置鼠标可用
                    if (currentTile.canDropItem && currentItem.canDropped) SetCursorValid();
                    // 反之，不可用
                    else SetCursorInValid();
                    break;

                case ItemType.HoeTool:
                    if (currentTile.canDig) SetCursorValid();
                    else SetCursorInValid();
                    break;

                case ItemType.WaterTool:
                    // 如果已被锄地并且没有被浇过水
                    if (currentTile.daysSinceDug > -1 && currentTile.daysSinceWatered == -1) SetCursorValid();
                    else SetCursorInValid();
                    break;

                case ItemType.BreakTool:
                case ItemType.ChopTool:
                    if (crop != null)
                    {
                        if (crop.CanHarvest && crop.cropDetails.CheckToolAvailable(currentItem.itemID))
                            SetCursorValid();
                        else
                            SetCursorInValid();
                    }
                    else
                        SetCursorInValid();

                    break;

                case ItemType.CollectTool:
                    if (currentCrop != null)
                    {
                        // 判断当前工具是否可以用于当前农作物
                        if (currentCrop.CheckToolAvailable(currentItem.itemID))
                        {
                            // 格子内有种子，并且生长日期达到总生长日期，意味着成熟，可以收集
                            if (currentTile.growthDays >= currentCrop.TotalGrowthDays) SetCursorValid();
                            else SetCursorInValid();
                        }
                    }
                    // 格子内没有种子，不可收集
                    else
                    {
                        SetCursorInValid();
                    }

                    break;

                case ItemType.ReapTool:
                    if (GridMapManager.Instance.HaveReapableItemsInRadius(mouseWorldPos, currentItem)) SetCursorValid();
                    else SetCursorInValid();

                    break;

                case ItemType.Furniture:
                    buildImage.gameObject.SetActive(true);
                    BluePrintDetails bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(currentItem.itemID);

                    if (currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) && !HaveFurnitureInRadius(bluePrintDetails))
                        SetCursorValid();
                    else
                        SetCursorInValid();
                    break;
            }
        }
        // 网格为空
        else
        {
            SetCursorInValid();
        }
    }

    private bool HaveFurnitureInRadius(BluePrintDetails bluePrintDetails)
    {
        GameObject buildItem = bluePrintDetails.buildPrefab;
        Vector2 point = mouseWorldPos;
        Vector2 size = buildItem.GetComponent<BoxCollider2D>().size;

        Collider2D otherColl = Physics2D.OverlapBox(point, size, 0);
        if (otherColl != null)
            return otherColl.GetComponent<Furniture>();
        return false;
    }
    
    // 是否与 UI互动
    private bool InteractWithUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    #region 设置鼠标样式

    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = Color.white;
    }

    // 设置鼠标可用
    private void SetCursorValid()
    {
        cursorPositionValid = true;
        cursorImage.color = Color.white;
        buildImage.color = new Color(1, 1, 1, 0.5f);
    }

    // 设置鼠标不可用
    private void SetCursorInValid()
    {
        cursorPositionValid = false;
        cursorImage.color = new Color(1, 0, 0, 0.5f);
        buildImage.color = new Color(1, 0, 0, 0.5f);
    }

    #endregion
}