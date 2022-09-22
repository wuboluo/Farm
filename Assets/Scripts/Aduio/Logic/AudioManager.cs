using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    [Header("音乐数据库")] public SoundDetailsListSO soundDetailsData;
    public SceneSoundListSO sceneSoundData;

    [Header("Audio Source")] public AudioSource ambientSource;
    public AudioSource gameSource;

    [Header("Audio Mixer")] public AudioMixer audioMixer;

    [Header("SnapShots")] public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot ambientSnapshot;
    public AudioMixerSnapshot muteSnapshot;

    private readonly float musicTransitionSecond = 3.5f;

    private Coroutine soundRoutine;

    public float MusicStartSecond => Random.Range(2, 5);

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent += OnPlaySoundEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        if (soundRoutine != null)
            StopCoroutine(soundRoutine);
        
        // 全体静音
        muteSnapshot.TransitionTo(1);
    }

    private void OnAfterSceneLoadedEvent()
    {
        var currentScene = SceneManager.GetActiveScene().name;
        var sceneSound = sceneSoundData.GetSceneSoundItem(currentScene);

        if (sceneSound == null) return;

        var ambient = soundDetailsData.GetSoundDetails(sceneSound.ambient);
        var music = soundDetailsData.GetSoundDetails(sceneSound.music);

        if (soundRoutine != null) StopCoroutine(soundRoutine);
        soundRoutine = StartCoroutine(PlaySoundRoutine(music, ambient));
    }

    private void OnPlaySoundEvent(SoundName soundName)
    {
        var soundDetails = soundDetailsData.GetSoundDetails(soundName);

        if (soundDetails != null)
            EventHandler.CallInitSoundEffectEvent(soundDetails);
    }

    private IEnumerator PlaySoundRoutine(SoundDetails music, SoundDetails ambient)
    {
        if (music != null && ambient != null)
        {
            PlayAmbientClip(ambient, 1);
            yield return new WaitForSeconds(MusicStartSecond);

            PlayMusicClip(music, musicTransitionSecond);
        }
    }

    /// 播放背景音乐
    private void PlayMusicClip(SoundDetails soundDetails, float transitionTime)
    {
        audioMixer.SetFloat("MusicVolume", ConvertSoundVolume(soundDetails.soundVolume));

        gameSource.clip = soundDetails.soundClip;
        if (gameSource.isActiveAndEnabled) gameSource.Play();

        normalSnapshot.TransitionTo(transitionTime);
    }

    /// 播放环境音效
    private void PlayAmbientClip(SoundDetails soundDetails, float transitionTime)
    {
        audioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(soundDetails.soundVolume));

        ambientSource.clip = soundDetails.soundClip;
        if (ambientSource.isActiveAndEnabled) ambientSource.Play();

        ambientSnapshot.TransitionTo(transitionTime);
    }

    private float ConvertSoundVolume(float amound)
    {
        return amound * 100 - 80;
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", value * 100 - 80);
    }
}