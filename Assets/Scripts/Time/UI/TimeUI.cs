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

    private readonly List<GameObject> clockBlocks = new();

    private void Awake()
    {
        for (int i = 0; i < clockParent.childCount; i++)
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

    // 每分钟调用，更新时间UI
    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        timeText.text = $"{hour:00}:{minute:00}";
    }

    /// 每小时调用，更新时间相关UI
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
        // 每 4个小时，亮一个格子
        int index = hour / 4;
        for (int i = 0; i < clockBlocks.Count; i++)
        {
            // index+1，是因为hour最大等于23，之后会进1，由于23/4会被取为5，所以+1
            clockBlocks[i].SetActive(i < index + 1);
        }
    }

    /// 每天的时间图样更新，日出日落白天深夜，旋转更替
    private void DayNightImageRotate(int hour)
    {
        Vector3 target = new Vector3(0, 0, hour * (360f / 24) - 90);
        dayNightImage.DORotate(target, 1);
    }
}