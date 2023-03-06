using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Y.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        // 每一个有数据需要保存的物体的数据集合（timeMgr，player，npcMovement……等等）
        private readonly List<ISavable> savableList = new();
        private int currentDataIndex;

        public readonly List<DataSlot> dataSlots = new(new DataSlot[3]);

        // 数据保存到本地的目录
        private string jsonFolder;

        protected override void Awake()
        {
            base.Awake();

            // Application.persistentDataPath：不同平台路径不一样，详见代码手册
            // Windows  C:\Users\username\AppData\LocalLow\company name\product name
            jsonFolder = Application.persistentDataPath + "/SAVE DATA/";

            ReadSaveData();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I)) Save(currentDataIndex);
            if (Input.GetKeyDown(KeyCode.O)) Load(currentDataIndex);
        }

        private void OnEnable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            Save(currentDataIndex);
        }

        private void OnStartNewGameEvent(int index)
        {
            currentDataIndex = index;
        }

        /// 在每一个继承 ISavable（即挂在了 dataGUID脚本的物体） 的 start方法中自动注册，添加到 list中
        public void RegisterSavable(ISavable savable)
        {
            if (!savableList.Contains(savable)) savableList.Add(savable);
        }

        /// 读游戏进度
        private void ReadSaveData()
        {
            if (Directory.Exists(jsonFolder))
            {
                for (int i = 0; i < dataSlots.Count; i++)
                {
                    string resultPath = jsonFolder + "data" + i + ".json";
                    if (File.Exists(resultPath))
                    {
                        string stringData = File.ReadAllText(resultPath);
                        DataSlot jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                        dataSlots[i] = jsonData;
                    }
                }
            }
        }


        /// 保存游戏数据到指定的存档（目前最多 3 个）
        private void Save(int index)
        {
            DataSlot data = new DataSlot();
            foreach (ISavable saveable in savableList)
            {
                // 把每一个需要保存的【物体对应的 guid】和【物体的具体的数据】对应保存到存档数据大集合中
                data.dataDict.Add(saveable.GUID, saveable.GenerateSaveData());
            }

            dataSlots[index] = data;


            string resultPath = jsonFolder + "data" + index + ".json";

            // 序列化 json
            // Formatting.Indented：内容回行，看起来整齐方便。实际开发可以不写，尽量乱一点避免修改数据
            string jsonData = JsonConvert.SerializeObject(dataSlots[index], Formatting.Indented);

            if (!File.Exists(resultPath)) Directory.CreateDirectory(jsonFolder);

            print("DATA" + index + " SAVED!");

            File.WriteAllText(resultPath, jsonData);
        }

        public void Load(int index)
        {
            currentDataIndex = index;

            string resultPath = jsonFolder + "data" + index + ".json";
            string stringData = File.ReadAllText(resultPath);
            DataSlot jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);

            foreach (ISavable savable in savableList)
            {
                if (jsonData != null)
                {
                    // 通过每一个数据物体的 guid，让他们自己去读取数据
                    savable.RestoreData(jsonData.dataDict[savable.GUID]);
                }
            }
        }
    }
}