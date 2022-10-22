using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    private ItemDetails activeItem;
    private ItemDataListSO dataBase;

    // 默认图标
    private Sprite defaultIcon;
    
    private List<ItemDetails> itemList = new();

    // 获得 VisualElement
    private ListView itemListView;
    private ScrollView itemDetailsSection;
    private VisualElement iconPreview;

    // 列表处道具模板
    private VisualTreeAsset itemRowTemplate;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // 拿到模板数据
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemRowTemplate.uxml");

        // 变量赋值
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        itemDetailsSection = root.Q<ScrollView>("ItemDetails");
        iconPreview = itemDetailsSection.Q<VisualElement>("Icon");

        // 获取默认 Icon图标
        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_M.png");

        // 获得按键
        root.Q<Button>("AddButton").clicked += OnAddButtonClicked;
        root.Q<Button>("DeleteButton").clicked += OnDeleteButtonClicked;

        // 加载数据
        LoadDataBase();

        // 生成 ListView
        GenerateListView();
    }


    [MenuItem("Y STUDIO/ItemEditor")]
    public static void ShowExample()
    {
        var wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    private void OnDeleteButtonClicked()
    {
        itemList.Remove(activeItem);
        itemListView.Rebuild();

        itemDetailsSection.visible = false;
    }

    private void OnAddButtonClicked()
    {
        var newItem = new ItemDetails
        {
            itemName = "New Item",
            itemID = 1001 + itemList.Count
        };

        itemList.Add(newItem);
        itemListView.Rebuild();
    }

    private void LoadDataBase()
    {
        var dataArray = AssetDatabase.FindAssets("ItemDataListSO");

        if (dataArray.Length >= 1)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            dataBase = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataListSO)) as ItemDataListSO;
        }

        if (dataBase == null) return;
        itemList = dataBase.itemDetailsList;

        // 如果不标记则无法保存数据
        EditorUtility.SetDirty(dataBase);
    }

    private void GenerateListView()
    {
        VisualElement MakeItem()
        {
            return itemRowTemplate.CloneTree();
        }

        void BindItem(VisualElement e, int i)
        {
            if (i >= itemList.Count) return;

            if (null != itemList[i].itemIcon)
                e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIcon.texture;

            e.Q<Label>("Name").text = itemList[i] == null ? "No Item" : itemList[i].itemName;
        }

        itemListView.fixedItemHeight = 50;

        itemListView.itemsSource = itemList;
        itemListView.makeItem = MakeItem;
        itemListView.bindItem = BindItem;

        itemListView.onSelectionChange += OnListSelectionChange;

        // 未选择的时候，右侧详细面板不可见
        itemDetailsSection.visible = false;
    }

    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        // todo:在左侧列表允许被重新排列时，拖动物品经过其他物品时会报异常，所以在此处进行判断，或许会有更合理的办法
        var enumerable = selectedItem as object[] ?? selectedItem.ToArray();
        if (enumerable.ToList().Count <= 0) return;
        
        activeItem = enumerable.First() as ItemDetails;
        GetItemDetails();

        // 选择 item时，右侧详细面板显示
        itemDetailsSection.visible = true;
    }

    private void GetItemDetails()
    {
        // 允许保存数据，更改数据，撤销等
        itemDetailsSection.MarkDirtyRepaint();

        // ID
        itemDetailsSection.Q<IntegerField>("ItemID").value = activeItem.itemID;
        itemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt => activeItem.itemID = evt.newValue);

        // 名称
        itemDetailsSection.Q<TextField>("ItemName").value = activeItem.itemName;
        itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            activeItem.itemName = evt.newValue;
            itemListView.Rebuild();
        });

        // 图标
        iconPreview.style.backgroundImage = activeItem.itemIcon == null ? defaultIcon.texture : activeItem.itemIcon.texture;
        itemDetailsSection.Q<ObjectField>("ItemIcon").value = activeItem.itemIcon;
        itemDetailsSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            // 如果选择的 item没有配置图标，则使用 item默认图标
            var newIcon = evt.newValue as Sprite;
            activeItem.itemIcon = newIcon;
            iconPreview.style.backgroundImage = newIcon == null ? defaultIcon.texture : newIcon.texture;

            itemListView.Rebuild();
        });

        // 地图上显示的图标
        itemDetailsSection.Q<ObjectField>("ItemSprite").value = activeItem.ItemOnWorldSprite;
        itemDetailsSection.Q<ObjectField>("ItemSprite").RegisterValueChangedCallback(evt => activeItem.ItemOnWorldSprite = evt.newValue as Sprite);

        // 类型
        itemDetailsSection.Q<EnumField>("ItemType").Init(activeItem.itemType);
        itemDetailsSection.Q<EnumField>("ItemType").value = activeItem.itemType;
        itemDetailsSection.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt => activeItem.itemType = (ItemType) evt.newValue);

        // 描述
        itemDetailsSection.Q<TextField>("Description").value = activeItem.itemDescription;
        itemDetailsSection.Q<TextField>("Description").RegisterValueChangedCallback(evt => activeItem.itemDescription = evt.newValue);

        // 使用范围
        itemDetailsSection.Q<IntegerField>("ItemUseRadius").value = activeItem.itemUseRadius;
        itemDetailsSection.Q<IntegerField>("ItemUseRadius").RegisterValueChangedCallback(evt => activeItem.itemUseRadius = evt.newValue);

        // 可否被拾取
        itemDetailsSection.Q<Toggle>("CanPickedup").value = activeItem.canPickedup;
        itemDetailsSection.Q<Toggle>("CanPickedup").RegisterValueChangedCallback(evt => activeItem.canPickedup = evt.newValue);

        // 可否被丢弃
        itemDetailsSection.Q<Toggle>("CanDropped").value = activeItem.canDropped;
        itemDetailsSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt => activeItem.canDropped = evt.newValue);

        // 可否被举着
        itemDetailsSection.Q<Toggle>("CanCarried").value = activeItem.canCarried;
        itemDetailsSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback(evt => activeItem.canCarried = evt.newValue);

        // 价值
        itemDetailsSection.Q<IntegerField>("Price").value = activeItem.itemPrice;
        itemDetailsSection.Q<IntegerField>("Price").RegisterValueChangedCallback(evt => activeItem.itemPrice = evt.newValue);

        // 出售折损比例
        itemDetailsSection.Q<Slider>("SellPercentage").value = activeItem.sellPercentage;
        itemDetailsSection.Q<Slider>("SellPercentage").RegisterValueChangedCallback(evt => activeItem.sellPercentage = evt.newValue);
    }
}