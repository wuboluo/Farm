using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// 继承 PlayableAsset：适用于可在运行时用于实例化 playable的资源
// 继承 ITimelineClipAsset：实现此接口可以支持 timeline剪辑的高级功能
public class DialogueClip : PlayableAsset, ITimelineClipAsset
{
    /// 返回描述，内容为代表实现此接口的可播放项的剪辑支持的功能
    public ClipCaps clipCaps => ClipCaps.None;
    public DialogueBehaviour dailogue = new DialogueBehaviour();

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<DialogueBehaviour> playable = ScriptPlayable<DialogueBehaviour>.Create(graph, dailogue);
        return playable;
    }
}

