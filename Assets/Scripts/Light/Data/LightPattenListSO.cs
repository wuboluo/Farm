using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LightPattenListSO", menuName = "Light/Light Patten")]
public class LightPattenListSO : ScriptableObject
{
    [SerializeField]
    private List<LightDetails> lightDetailsList;

    /// 根据季节和周期返回灯光详情 
    public LightDetails GetLightDetails(Season season, LightShift lightShift)
    {
        return lightDetailsList.Find(l => l.season == season && l.lightShift == lightShift);
    }
}

[Serializable]
public class LightDetails
{
    public Season season;
    public LightShift lightShift;
    public Color lightColor;
    public float lightAmount;
}