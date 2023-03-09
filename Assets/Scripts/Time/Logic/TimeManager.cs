using System;
using System.Collections.Generic;
using UnityEngine;
using Y.Save;

public class TimeManager : Singleton<TimeManager>, ISavable
{
    // 控制时间的暂停
    public bool gameClockPause;

    // 秒、分钟、小时、天、月、年
    private int gameSecond, gameMinute, gameHour, gameDay, gameMonth, gameYear;

    // 每 3个月一个季节
    private int monthInSeason = 3;
    private Season gameSeason = Season.春天;

    // 时间计时器
    private float tikTime;

    // 灯光时间差
    private float timeDifference;

    public TimeSpan GameTime => new(gameHour, gameMinute, gameSecond);

    private void Start()
    {
        ISavable savable = this;
        savable.RegisterSavable();

        // 开场动画结束恢复时间
        gameClockPause = true;

        // EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);
        // EventHandler.CallGameDataEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        // EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
    }

    private void Update()
    {
        if (!gameClockPause)
        {
            tikTime += Time.deltaTime;

            if (tikTime >= Settings.secondThreshold)
            {
                // 每超过这个阈值之后，就减去这个阈值，并等待下一次达到的时候
                tikTime -= Settings.secondThreshold;
                UpdateGameTime();
            }
        }

        // 作弊选项
        if (Input.GetKey(KeyCode.T))
        {
            for (int i = 0; i < 120; i++)
            {
                UpdateGameTime();
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            gameDay++;
            EventHandler.CallGameDayEvent(gameDay, gameSeason);
            EventHandler.CallGameDataEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        }
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    public string GUID => GetComponent<DataGUID>().guid;

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData
        {
            timeDict = new Dictionary<string, int>
            {
                { "gameYear", gameYear },
                { "gameSeason", (int)gameSeason },
                { "gameMonth", gameMonth },
                { "gameDay", gameDay },
                { "gameHour", gameHour },
                { "gameMinute", gameMinute },
                { "gameSecond", gameSecond }
            }
        };


        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        gameYear = saveData.timeDict["gameYear"];
        gameSeason = (Season)saveData.timeDict["gameSeason"];
        gameMonth = saveData.timeDict["gameMonth"];
        gameDay = saveData.timeDict["gameDay"];
        gameHour = saveData.timeDict["gameHour"];
        gameMinute = saveData.timeDict["gameMinute"];
        gameSecond = saveData.timeDict["gameSecond"];
    }

    /// 游戏结束时
    private void OnEndGameEvent()
    {
        gameClockPause = true;
    }

    /// 开始新游戏时
    private void OnStartNewGameEvent(int index)
    {
        NewGameTime();
        gameClockPause = false;
    }

    /// 默认新游戏时间日期
    private void NewGameTime()
    {
        gameSecond = 0;
        gameMinute = 0;
        gameHour = 7;
        gameDay = 1;
        gameMonth = 1;
        gameYear = 2022;
        gameSeason = Season.春天;
    }

    private void UpdateGameTime()
    {
        gameSecond++;

        // 秒 >59，分钟 +1
        if (gameSecond > Settings.secondHold)
        {
            gameMinute++;
            gameSecond = 0;

            // 分钟 >59，小时 +1
            if (gameMinute > Settings.minuteHold)
            {
                gameHour++;
                gameMinute = 0;

                // 小时 >23，天 +1
                if (gameHour > Settings.hourHold)
                {
                    gameDay++;
                    gameHour = 0;

                    // 天 >10(30天太久了，自定义为10)，月 +1
                    if (gameDay > Settings.dayHold)
                    {
                        gameMonth++;
                        gameDay = 1; // 每月从 1号开始

                        if (gameMonth > 12) gameMonth = 1;

                        // 扣除当前季节所剩月份
                        monthInSeason--;
                        // 当前季节过完之后
                        if (monthInSeason == 0)
                        {
                            // 开启下一个季节，为期 3个月
                            monthInSeason = 3;

                            // 根据当前季节，获取下一个季节的序号
                            int seasonNumber = (int)gameSeason;
                            seasonNumber++;

                            // 当季节交替 3次以后，认为当前已经过去一年
                            if (seasonNumber > Settings.seasonHold)
                            {
                                seasonNumber = 0;
                                gameYear++;
                            }

                            // 更新季节
                            gameSeason = (Season)seasonNumber;


                            if (gameYear > 9999) gameYear = 2022;
                        }
                    }

                    // 每天更新，用来刷新地图和农作物生长
                    EventHandler.CallGameDayEvent(gameDay, gameSeason);
                }

                // 每小时调用
                EventHandler.CallGameDataEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
            }

            // 每分钟调用
            EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);

            // 切换灯光
            EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
        }
    }

    private void OnBeforeSceneUnloadEvent()
    {
        gameClockPause = true;
    }

    private void OnAfterSceneLoadedEvent()
    {
        gameClockPause = false;

        EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);
        EventHandler.CallGameDataEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
    }

    private void OnUpdateGameStateEvent(GameState state)
    {
        gameClockPause = state == GameState.Pause;
    }

    /// 返回 lightShift的同时计算 时间差
    private LightShift GetCurrentLightShift()
    {
        // 白天
        if (GameTime >= Settings.morningTime && GameTime < Settings.nightTime)
        {
            timeDifference = (float)(GameTime - Settings.morningTime).TotalMinutes;
            return LightShift.Morning;
        }

        if (GameTime < Settings.morningTime || GameTime >= Settings.nightTime)
        {
            timeDifference = Mathf.Abs((float)(GameTime - Settings.nightTime).TotalMinutes);
            return LightShift.Night;
        }

        return LightShift.Morning;
    }
}