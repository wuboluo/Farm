using System;
using System.Collections.Generic;
using UnityEngine;
using Y.Transition;

namespace Y.Save
{
    // 存档，string是 guid
    // 每一个存档是一个 DataSlot
    public class DataSlot
    {
        // 每一个挂在了 dataGUID脚本的物体（即有数据需要保存的物体）具体的数据 与对应的 guid 保存为一组数据
        // 每组数据在每一个存档中保持惟一，例如一个存档中只有一个 timeManager的数据，这里不是指 guid的惟一
        public Dictionary<string, GameSaveData> dataDict = new();


        /// 用来显示 UI存档进度详情 
        public string DataTime
        {
            get
            {
                string key = TimeManager.Instance.GUID;

                if (dataDict.ContainsKey(key))
                {
                    GameSaveData timeData = dataDict[key];
                    return timeData.timeDict["gameYear"] + "年/" +
                           (Season) timeData.timeDict["gameSeason"] + "/" +
                           timeData.timeDict["gameMonth"] + "月/" +
                           timeData.timeDict["gameDay"] + "日/";
                }

                return string.Empty;
            }
        }

        public string DataScene
        {
            get
            {
                string key = TransitionManager.Instance.GUID;
                
                if (dataDict.ContainsKey(key))
                {
                    GameSaveData transitionData = dataDict[key];
                    return transitionData.dataSceneName switch
                    {
                        "00.Start" => "海边",
                        "01.Field" => "农场",
                        "02.Home" => "小木屋",
                        "03.Stall" => "市场",
                        _ => string.Empty
                    };
                }

                return string.Empty;
            }
        }
    }
}