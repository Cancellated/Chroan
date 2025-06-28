using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using MyGame.System;
using MyGame.Data;

namespace Audio
{
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
        private AudioSource _bgmSource;
        private Coroutine _bgmFadeCoroutine;
        
        // 类成员区域
        private string _currentBGM;
        private SaveData _saveData; // 通过SaveManager初始化
        
        private void Initialize()
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _saveData = SaveLoad.Instance.saveData;
        }
        
        public void PlayBGM(string bgmKey, float fadeDuration = 1f)
        {
            if(_currentBGM == bgmKey) return;
            
            AudioClip clip = soundLibrary.GetClip(bgmKey);
            if(clip == null) return;
        
            if(_bgmFadeCoroutine != null) StopCoroutine(_bgmFadeCoroutine);
            _bgmFadeCoroutine = StartCoroutine(FadeBGM(clip, fadeDuration));
            _currentBGM = bgmKey;
        }
        
        private IEnumerator FadeBGM(AudioClip newClip, float duration)
        {
            // 淡出当前BGM
            if(_bgmSource.isPlaying)
            {
                float startVolume = _bgmSource.volume;
                for(float t = 0; t < duration; t += Time.deltaTime)
                {
                    _bgmSource.volume = Mathf.Lerp(startVolume, 0, t/duration);
                    yield return null;
                }
                _bgmSource.Stop();
            }
        
            // 淡入新BGM
            _bgmSource.clip = newClip;
            _bgmSource.Play();
            float targetVolume = _saveData.masterVolume * _saveData.bgmVolume;
            for(float t = 0; t < duration; t += Time.deltaTime)
            {
                _bgmSource.volume = Mathf.Lerp(0, targetVolume, t/duration);
                yield return null;
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
}