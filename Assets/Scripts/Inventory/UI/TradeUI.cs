using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Y.Inventory;

public class TradeUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TMP_InputField tradeAmount;
    public Button submitButton, cancelButton;

    private ItemDetails itemDetails;

    private bool isSellTrade;

    private void Awake()
    {
        submitButton.onClick.AddListener(TradeItem);
        cancelButton.onClick.AddListener(CancelTrade);
    }

    private void OnEnable()
    {
        EventHandler.UpdateGameStateEvent += OnUpdateGameState;
    }

    private void OnDisable()
    {
        EventHandler.UpdateGameStateEvent -= OnUpdateGameState;
    }

    private void OnUpdateGameState(GameState state)
    {
        if (state == GameState.GamePlay) CancelTrade();
    }

    /// 设置 TradeUI 显示详情 
    public void SetupTradeUI(ItemDetails details, bool isSell)
    {
        itemDetails = details;

        itemIcon.sprite = details.itemIcon;
        itemName.text = details.itemName;
        isSellTrade = isSell;
        tradeAmount.text = string.Empty;
    }

    private void TradeItem()
    {
        var amount = Convert.ToInt32(tradeAmount.text);
        InventoryManager.Instance.TradeItem(itemDetails, amount, isSellTrade);

        // 关闭窗口
        CancelTrade();
    }


    private void CancelTrade()
    {
        gameObject.SetActive(false);
    }
}