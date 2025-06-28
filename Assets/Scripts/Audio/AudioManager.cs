using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using MyGame.System;

public class AudioManager : Singleton<AudioManager>
{
    #region 变量
    [Header("音频混合器")]
    public AudioMixer audioMixer;

    [Header("音频源")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;

    [Header("音频库")]
    public SoundLibrary soundLibrary;
    #endregion

    #region 生命周期
    protected override void Awake()
    {
        base.Awake();
        SetMasterVolume(SaveLoad.Instance.saveData.masterVolume);
        SetBGMVolume(SaveLoad.Instance.saveData.bgmVolume);
        SetSFXVolume(SaveLoad.Instance.saveData.sfxVolume);
    }
    private void OnEnable()
    {
        GameEvents.OnSceneSoundTriggered += HandleSceneSound;
        GameEvents.OnUIInteraction += HandleUISound;
    }
    private void OnDisable()
    {
        GameEvents.OnSceneSoundTriggered -= HandleSceneSound;
        GameEvents.OnUIInteraction -= HandleUISound;
    }

    
    #endregion

    #region 方法   
    private void HandleSceneSound(string soundType)
    {
        switch(soundType)
        {
            case "BGM_MainMenu":
                PlayBGM(soundType);
                break;
            case "SFX_DoorOpen":
                PlaySFX(soundType);
                break;
        }
    }

    private void HandleUISound(string soundType)
    {
        PlaySFX(soundType); // 播放按钮点击等UI音效
    }
    /// <summary>
    /// 播放BGM
    /// </summary>
    /// <param name="clipName"></param>
    public void PlayBGM(string clipName)
    {
        AudioClip clip = soundLibrary.GetClip(clipName);
        if(clip) {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 播放SFX
    /// </summary>
    /// <param name="clipName"></param>
    public void PlaySFX(string clipName)
    {
        AudioClip clip = soundLibrary.GetClip(clipName);
        if(clip) {
            sfxSource.PlayOneShot(clip);
        }
    }

    #endregion
    
    #region 设置
        /// <summary>
    /// 设置SFX音量
    /// </summary>
    /// <param name="volume"></param>
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        SaveLoad.Instance.saveData.sfxVolume = volume;
    }
    /// <summary>
    /// 设置BGM音量
    /// </summary>
    /// <param name="volume"></param>
    public void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
        SaveLoad.Instance.saveData.bgmVolume = volume;
    }
    /// <summary>

    /// 设置主音量
    /// </summary>
    /// <param name="volume"></param>
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        SaveLoad.Instance.saveData.masterVolume = volume;
    }
    #endregion
}