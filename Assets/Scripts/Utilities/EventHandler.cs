using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Yang.Dialogue;

public static class EventHandler
{
    public static event Action<InventoryLocation, List<InventoryItem>> updateInventoryUI;

    public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
    {
        updateInventoryUI?.Invoke(location, list);
    }


    public static event Action<int, Vector3> InstantiateItemInScene;

    public static void CallInstantiateItemInScene(int id, Vector3 pos)
    {
        InstantiateItemInScene?.Invoke(id, pos);
    }


    public static event Action<int, Vector3, ItemType> DropItemEvent;

    public static void CallDropItemEvent(int id, Vector3 pos, ItemType itemType)
    {
        DropItemEvent?.Invoke(id, pos, itemType);
    }


    public static event Action<ItemDetails, bool> ItemSelectedEvent;

    public static void CallItemSelectedEvent(ItemDetails details, bool isSelected)
    {
        ItemSelectedEvent?.Invoke(details, isSelected);
    }


    public static event Action<int, int, int, Season> GameMinuteEvent;

    public static void CallGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        GameMinuteEvent?.Invoke(minute, hour, day, season);
    }


    public static event Action<int, Season> GameDayEvent;

    public static void CallGameDayEvent(int day, Season season)
    {
        GameDayEvent?.Invoke(day, season);
    }


    public static event Action<int, int, int, int, Season> GameDataEvent;

    public static void CallGameDataEvent(int hour, int day, int month, int year, Season season)
    {
        GameDataEvent?.Invoke(hour, day, month, year, season);
    }


    public static event Action<string, Vector3> TransitionEvent;

    public static void CallTransitionEvent(string sceneName, Vector3 pos)
    {
        TransitionEvent?.Invoke(sceneName, pos);
    }


    public static event Action BeforeSceneUnloadEvent;

    public static void CallBeforeSceneUnloadEvent()
    {
        BeforeSceneUnloadEvent?.Invoke();
    }


    public static event Action AfterSceneLoadedEvent;

    public static void CallAfterSceneLoadedEvent()
    {
        AfterSceneLoadedEvent?.Invoke();
    }


    public static event Action<Vector3> MoveToPosition;

    public static void CallMoveToPosition(Vector3 pos)
    {
        MoveToPosition?.Invoke(pos);
    }


    public static event Action<Vector3, ItemDetails> MouseClickedEvent;

    public static void CallMouseClickedEvent(Vector3 pos, ItemDetails details)
    {
        MouseClickedEvent?.Invoke(pos, details);
    }


    // 在动画之后的实际的行为
    public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;

    public static void CallExecuteActionAfterAnimation(Vector3 pos, ItemDetails details)
    {
        ExecuteActionAfterAnimation?.Invoke(pos, details);
    }


    public static event Action<int, TileDetails> PlantSeedEvent;

    public static void CallPlantSeed(int id, TileDetails tileDetails)
    {
        PlantSeedEvent?.Invoke(id, tileDetails);
    }

    public static event Action<int> HarvestAtPlayerPosition;

    public static void CallHarvestAtPlayerPosition(int id)
    {
        HarvestAtPlayerPosition?.Invoke(id);
    }

    public static event Action RefreshCurrentMap;

    public static void CallRefreshCurrentMap()
    {
        RefreshCurrentMap?.Invoke();
    }


    public static event Action<ParticleEffectType, Vector3> ParticleEffectEvent;

    public static void CallParticleEffectEvent(ParticleEffectType effectType, Vector3 pos)
    {
        ParticleEffectEvent?.Invoke(effectType, pos);
    }

    public static event Action GenerateCropEvent;

    public static void CallGenerateCropEvent()
    {
        GenerateCropEvent?.Invoke();
    }

    public static event Action<DialoguePiece> ShowDialogueEvent;

    public static void CallShowDialogueEvent(DialoguePiece piece)
    {
        ShowDialogueEvent?.Invoke(piece);
    }

    // 离开商店
    public static event Action<SlotType, InventoryBagSO> BaseBagOpenEvent;

    public static void CallBaseBagOpenEvent(SlotType type, InventoryBagSO bagSO)
    {
        BaseBagOpenEvent?.Invoke(type, bagSO);
    }

    public static event Action<SlotType, InventoryBagSO> BaseBagCloseEvent;

    public static void CallBaseBagCloseEvent(SlotType type, InventoryBagSO bagSO)
    {
        BaseBagCloseEvent?.Invoke(type, bagSO);
    }

    public static event Action<GameState> UpdateGameStateEvent;

    public static void CallUpdateGameStateEvent(GameState state)
    {
        UpdateGameStateEvent?.Invoke(state);
    }

    public static event Action<ItemDetails, bool> ShowTradeUI;

    public static void CallShowTradeUI(ItemDetails details, bool isSell)
    {
        ShowTradeUI?.Invoke(details, isSell);
    }

    // 建造
    public static event Action<int, Vector3> BuildFurnitureEvent;

    public static void CallBuildFurnitureEvent(int id, Vector3 pos)
    {
        BuildFurnitureEvent?.Invoke(id, pos);
    }

    // 灯光
    public static event Action<Season, LightShift, float> LightShiftChangeEvent;

    public static void CallLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        LightShiftChangeEvent?.Invoke(season, lightShift, timeDifference);
    }

    // 音效
    public static event Action<SoundDetails> InitSoundEffectEvent;

    public static void CallInitSoundEffectEvent(SoundDetails soundDetails)
    {
        InitSoundEffectEvent?.Invoke(soundDetails);
    }
    
    public static event Action<SoundName> PlaySoundEvent;

    public static void CallPlaySoundEvent(SoundName soundName)
    {
        PlaySoundEvent?.Invoke(soundName);
    }

    public static event Action<int> StartNewGameEvent;

    public static void CallStartNewGameEvent(int index)
    {
        StartNewGameEvent?.Invoke(index);
    }

    public static event Action EndGameEvent;

    public static void CallEndGameEvent()
    {
        EndGameEvent?.Invoke();
    }
}