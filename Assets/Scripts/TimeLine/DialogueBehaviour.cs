using System;
using UnityEngine;
using UnityEngine.Playables;
using Yang.Dialogue;

[Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    public DialoguePiece dialoguePiece;
    private PlayableDirector director;

    public override void OnPlayableCreate(Playable playable)
    {
        director = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        //呼叫启动 UI
        EventHandler.CallShowDialogueEvent(dialoguePiece);
        if (Application.isPlaying)
        {
            if (dialoguePiece.hasToPause)
                // 暂停timeline
                TimelineManager.Instance.PauseTimeline(director);
            else
                EventHandler.CallShowDialogueEvent(null);
        }
    }

    /// 在 timeline播放期间每帧执行
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (Application.isPlaying) TimelineManager.Instance.IsDone = dialoguePiece.isDone;
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(null);
    }

    public override void OnGraphStart(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }

    public override void OnGraphStop(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
        TimelineManager.Instance.startDirector.gameObject.SetActive(false);
    }
}