using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightControl : MonoBehaviour
{
    public LightPattenListSO lightData;
    private Light2D currentLight;

    private LightDetails currentLightDetails;

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
    }

    /// 切换灯光
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        currentLightDetails = lightData.GetLightDetails(season, lightShift);

        // 如果当前时间差小于时间过渡时间范围（颜色没有过渡完成）
        if (timeDifference < Settings.lightChangeDuration)
        {
            // 根据时间差占有设置的 lightChangeDuration的半分比计算当前应该显示的颜色数值
            Color colorOffset = (currentLightDetails.lightColor - currentLight.color) / Settings.lightChangeDuration * timeDifference;
            currentLight.color += colorOffset;

            DOTween.To(() => currentLight.color, c => currentLight.color = c, currentLightDetails.lightColor, Settings.lightChangeDuration - timeDifference);
            DOTween.To(() => currentLight.intensity, i => currentLight.intensity = i, currentLightDetails.lightAmount, Settings.lightChangeDuration - timeDifference);
        }

        if (timeDifference >= Settings.lightChangeDuration)
        {
            currentLight.color = currentLightDetails.lightColor;
            currentLight.intensity = currentLightDetails.lightAmount;
        }
    }
}