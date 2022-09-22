using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Y.Save;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI dataTime, dataScene;

    private Button currentButton;
    private DataSlot currentData;

    private int Index => transform.GetSiblingIndex();
    private void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
    }

    private void OnEnable()
    {
        SetupSlotUI();
    }

    private void SetupSlotUI()
    {
        currentData = SaveLoadManager.Instance.dataSlots[Index];

        if (currentData != null)
        {
            dataTime.text = currentData.DataTime;
            dataScene.text = currentData.DataScene;
        }
        else
        {
            dataTime.text = "这个世界还未开始…";
            dataScene.text = "未知地区…";
        }
    }
    
    private void LoadGameData()
    {
        if (currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
            Debug.Log("新游戏");
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}