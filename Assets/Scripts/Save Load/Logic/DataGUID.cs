using System;
using UnityEngine;

[ExecuteAlways] // 使脚本的实例在播放模式期间以及编辑时始终执行
public class DataGUID : MonoBehaviour
{
    public string guid;

    private void Awake()
    {
        if (guid == string.Empty)
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}