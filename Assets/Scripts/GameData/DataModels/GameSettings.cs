using System;
using UnityEngine;

namespace MyGame.Data
{
    /// <summary>
    /// 游戏设置类
    /// 存储游戏的所有可配置设置
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        #region 字段

        // 音量设置
        private float m_musicVolume = 1.0f;
        private float m_sfxVolume = 1.0f;

        // 画质设置
        private int m_qualityLevel = 2;
        private bool m_fullscreen = true;
        private int m_resolutionIndex = 0;

        // 游戏设置
        private bool m_invertYAxis = false;

        #endregion

        #region 属性

        /// <summary>
        /// 音乐音量
        /// 范围: 0.0f - 1.0f
        /// </summary>
        public float MusicVolume
        {
            get { return m_musicVolume; }
            set { m_musicVolume = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// 音效音量
        /// 范围: 0.0f - 1.0f
        /// </summary>
        public float SfxVolume
        {
            get { return m_sfxVolume; }
            set { m_sfxVolume = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// 画质等级
        /// </summary>
        public int QualityLevel
        {
            get { return m_qualityLevel; }
            set { m_qualityLevel = value; }
        }

        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool Fullscreen
        {
            get { return m_fullscreen; }
            set { m_fullscreen = value; }
        }

        /// <summary>
        /// 分辨率索引
        /// </summary>
        public int ResolutionIndex
        {
            get { return m_resolutionIndex; }
            set { m_resolutionIndex = value; }
        }

        /// <summary>
        /// 是否反转Y轴
        /// </summary>
        public bool InvertYAxis
        {
            get { return m_invertYAxis; }
            set { m_invertYAxis = value; }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public GameSettings() { }

        /// <summary>
        /// 构造函数，从现有设置复制
        /// </summary>
        /// <param name="other">要复制的设置对象</param>
        public GameSettings(GameSettings other)
        {
            if (other != null)
            {
                MusicVolume = other.MusicVolume;
                SfxVolume = other.SfxVolume;
                QualityLevel = other.QualityLevel;
                Fullscreen = other.Fullscreen;
                ResolutionIndex = other.ResolutionIndex;
                InvertYAxis = other.InvertYAxis;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 重置设置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            MusicVolume = 1.0f;
            SfxVolume = 1.0f;
            QualityLevel = 2;
            Fullscreen = true;
            ResolutionIndex = 0;
            InvertYAxis = false;
        }

        /// <summary>
        /// 从PlayerPrefs加载设置
        /// </summary>
        public void LoadFromPlayerPrefs()
        {
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
            SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1.0f);
            QualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
            Fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            ResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
            InvertYAxis = PlayerPrefs.GetInt("InvertYAxis", 0) == 1;
        }

        /// <summary>
        /// 保存设置到PlayerPrefs
        /// </summary>
        public void SaveToPlayerPrefs()
        {
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            PlayerPrefs.SetFloat("SfxVolume", SfxVolume);
            PlayerPrefs.SetInt("QualityLevel", QualityLevel);
            PlayerPrefs.SetInt("Fullscreen", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("ResolutionIndex", ResolutionIndex);
            PlayerPrefs.SetInt("InvertYAxis", InvertYAxis ? 1 : 0);
            
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 应用设置到游戏
        /// </summary>
        public void ApplyToGame()
        {
            // 应用画质设置
            QualitySettings.SetQualityLevel(QualityLevel);
            
            // 应用分辨率和全屏设置
            Resolution[] resolutions = Screen.resolutions;
            if (resolutions != null && resolutions.Length > 0 && ResolutionIndex >= 0 && ResolutionIndex < resolutions.Length)
            {
                Resolution selectedResolution = resolutions[ResolutionIndex];
                Screen.SetResolution(selectedResolution.width, selectedResolution.height, Fullscreen);
            }
        }

        #endregion
    }
}