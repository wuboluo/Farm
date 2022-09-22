using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public RectTransform dayNightImage;
    public RectTransform clockParent;
    public Image seasonImage;
    public TextMeshProUGUI dataText;
    public TextMeshProUGUI timeText;

    public Sprite[] seasonSprites;

    private List<GameObject> clockBlocks = new();

    private void Awake()
    {
        for (var i = 0; i < clockParent.childCount; i++)
        {
            clockBlocks.Add(clockParent.GetChild(i).gameObject);
            clockParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDataEvent += OnGameDataEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDataEvent -= OnGameDataEvent;
    }

    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        timeText.text = $"{hour:00}:{minute:00}";
    }

    private void OnGameDataEvent(int hour, int day, int month, int year, Season season)
    {
        dataText.text = $"{year}/{month:00}/{day:00}";
        seasonImage.sprite = seasonSprites[(int) season];

        SwitchHourImage(hour);
        DayNightImageRotate(hour);
    }

    // 根据小时切换时间块
    private void SwitchHourImage(int hour)
    {
        var index = hour / 4;

        for (var i = 0; i < clockBlocks.Count; i++)
        {
            clockBlocks[i].SetActive(i < index + 1);
        }
    }

    private void DayNightImageRotate(int hour)
    {
        var target = new Vector3(0, 0, hour * (360f / 24) - 90);
        dayNightImage.DORotate(target, 1);
    }
}