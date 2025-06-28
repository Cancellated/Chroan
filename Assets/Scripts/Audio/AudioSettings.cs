using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Settings
{
    [System.Serializable]
    public class AudioSettings
    {
        public float masterVolume = 1f;
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;

        public void ApplyToMixer(AudioMixer mixer)
        {
            mixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
            mixer.SetFloat("BGMVolume", Mathf.Log10(bgmVolume) * 20);
            mixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        }
    }
}
