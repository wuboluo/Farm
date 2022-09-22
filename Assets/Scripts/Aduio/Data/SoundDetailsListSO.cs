using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDetailsListSO", menuName = "Audio/SoundDetailsList")]
public class SoundDetailsListSO : ScriptableObject
{
    public List<SoundDetails> soundDetailsList;

    public SoundDetails GetSoundDetails(SoundName name) => soundDetailsList.Find(s => s.soundName == name);
}

[Serializable]
public class SoundDetails
{
    public SoundName soundName;

    public AudioClip soundClip;

    [Range(0.1f, 1.5f)] public float soundPitchMin;
    [Range(0.1f, 1.5f)] public float soundPitchMax;

    [Range(0.1f, 1f)] public float soundVolume;
}