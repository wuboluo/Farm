using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneSoundListSO", menuName = "Audio/SceneSoundList")]
public class SceneSoundListSO : ScriptableObject
{
    public List<SceneSoundItem> sceneSoundItemList;
    public SceneSoundItem GetSceneSoundItem(string sceneName) => sceneSoundItemList.Find(s => s.sceneName == sceneName);
}

[Serializable]
public class SceneSoundItem
{
    [SceneName] public string sceneName;
    public SoundName ambient;
    public SoundName music;
}