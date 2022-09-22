using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public void FootStepSoundSoft()
    {
        EventHandler.CallPlaySoundEvent(SoundName.FootStepSoft);
    }
    
    public void FootStepSoundHard()
    {
        EventHandler.CallPlaySoundEvent(SoundName.FootStepHard);
    }
}