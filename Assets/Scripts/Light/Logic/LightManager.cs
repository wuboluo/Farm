using UnityEngine;

public class LightManager : MonoBehaviour
{
    private LightShift currentLightShift;
    private Season currentSeason;
    private LightControl[] sceneLights;
    private float timeDifference = Settings.lightChangeDuration;

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
        EventHandler.LightShiftChangeEvent += OnLightShiftChange;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChange;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int index)
    {
        currentLightShift = LightShift.Morning;
    }

    private void OnAfterSceneLoaded()
    {
        sceneLights = FindObjectsOfType<LightControl>();

        foreach (LightControl lightControl in sceneLights)
            // 改变灯光
            lightControl.ChangeLightShift(currentSeason, currentLightShift, timeDifference);
    }

    private void OnLightShiftChange(Season season, LightShift lightShift, float timeDifference)
    {
        currentSeason = season;
        this.timeDifference = timeDifference;

        if (currentLightShift != lightShift)
        {
            currentLightShift = lightShift;

            foreach (LightControl lightControl in sceneLights)
                // 改变灯光的方法
                lightControl.ChangeLightShift(currentSeason, currentLightShift, this.timeDifference);
        }
    }
}