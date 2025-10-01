using Logger;
using MyGame.UI;
using UnityEngine;
using MyGame.Events;
using System.Collections.Generic;

namespace MyGame.UI.Settings.Model
{
    /// <summary>
    /// 设置面板数据模型
    /// 负责存储和管理设置数据
    /// </summary>
    public class SettingsModel : ObservableModel
    {
        #region 字段

        // 音量设置
        private float m_musicVolume = 1.0f;
        private float m_sfxVolume = 1.0f;

        // 画质设置
        private int m_qualityLevel = 2;
        private bool m_fullscreen = true;
        private int m_resolutionIndex = 0;
        // 自定义画质名称列表
        private List<string> m_customQualityNames = new();

        // 游戏设置
        private bool m_invertYAxis = false;

        private const string LOG_MODULE = LogModules.SETTINGS + "Model";

        #endregion

        #region 属性

        /// <summary>
        /// 音乐音量
        /// </summary>
        public float MusicVolume
        {
            get { return m_musicVolume; }
            set { SetProperty(ref m_musicVolume, value, nameof(MusicVolume)); }
        }

        /// <summary>
        /// 音效音量
        /// </summary>
        public float SfxVolume
        {
            get { return m_sfxVolume; }
            set { SetProperty(ref m_sfxVolume, value, nameof(SfxVolume)); }
        }

        /// <summary>
        /// 画质等级
        /// </summary>
        public int QualityLevel
        {
            get { return m_qualityLevel; }
            set { SetProperty(ref m_qualityLevel, value, nameof(QualityLevel)); }
        }
        
        /// <summary>
        /// 自定义画质名称列表
        /// </summary>
        public List<string> CustomQualityNames
        {
            get { return m_customQualityNames; }
            set { SetProperty(ref m_customQualityNames, value, nameof(CustomQualityNames)); }
        }

        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool Fullscreen
        {
            get { return m_fullscreen; }
            set { SetProperty(ref m_fullscreen, value, nameof(Fullscreen)); }
        }

        /// <summary>
        /// 分辨率索引
        /// </summary>
        public int ResolutionIndex
        {
            get { return m_resolutionIndex; }
            set { SetProperty(ref m_resolutionIndex, value, nameof(ResolutionIndex)); }
        }

        /// <summary>
        /// 是否反转Y轴
        /// </summary>
        public bool InvertYAxis
        {
            get { return m_invertYAxis; }
            set { SetProperty(ref m_invertYAxis, value, nameof(InvertYAxis)); }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化设置数据
        /// 从PlayerPrefs加载保存的设置
        /// </summary>
        public override void Initialize()
        {
            // 从PlayerPrefs加载设置
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
            SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1.0f);
            QualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
            Fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            ResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
            InvertYAxis = PlayerPrefs.GetInt("InvertYAxis", 0) == 1;
            
            // 初始化自定义画质名称
            InitializeCustomQualityNames();
        }
        
        /// <summary>
        /// 初始化自定义画质名称
        /// 这里可以根据需要修改为从配置文件加载或使用硬编码的名称
        /// </summary>
        private void InitializeCustomQualityNames()
        {
            // 默认使用Unity的画质等级名称作为后备
            string[] unityQualityNames = QualitySettings.names;
            
            // 根据项目需求设置自定义名称
            List<string> customNames = new();
            
            // 创建Unity默认画质名称到自定义名称的映射
            Dictionary<string, string> qualityNameMapping = new()
            {
                { "Performant", "低" },
                { "Balanced", "中" },
                { "High Fidelity", "高" },
                // 如果想要定义更多等级画质，在Edit > Project Settings > Quality 打开画质设置面板，点击"+"按钮添加新的画质级别
                // 然后在qualityNameMapping中添加对应的映射关系
            };
            
            // 使用foreach循环遍历所有Unity默认画质名称
            foreach (string unityName in unityQualityNames)
            {
                // 检查是否有对应的自定义名称，如果有则使用自定义名称，否则使用原始名称
                if (qualityNameMapping.TryGetValue(unityName, out string customName))
                {
                    customNames.Add(customName);
                }
                else
                {
                    customNames.Add(unityName);
                }
            }
            
            CustomQualityNames = customNames;
        }

        /// <summary>
        /// 保存设置到PlayerPrefs
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            PlayerPrefs.SetFloat("SfxVolume", SfxVolume);
            PlayerPrefs.SetInt("QualityLevel", QualityLevel);
            PlayerPrefs.SetInt("Fullscreen", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("ResolutionIndex", ResolutionIndex);
            PlayerPrefs.SetInt("InvertYAxis", InvertYAxis ? 1 : 0);
            
            PlayerPrefs.Save();
            
            // 触发设置保存完成事件
            GameEvents.TriggerSettingsSaved();
        }

        /// <summary>
        /// 应用设置到游戏
        /// </summary>
        public void ApplySettings()
        {
            // 应用画质设置
            QualitySettings.SetQualityLevel(QualityLevel);
            
            // 应用分辨率和全屏设置
            Resolution[] resolutions = Screen.resolutions;
            if (resolutions != null && resolutions.Length > 0 && ResolutionIndex >= 0 && ResolutionIndex < resolutions.Length)
            {
                Resolution selectedResolution = resolutions[ResolutionIndex];
                Screen.SetResolution(selectedResolution.width, selectedResolution.height, Fullscreen);
                Log.Info(LOG_MODULE, $"应用分辨率设置: {selectedResolution.width}x{selectedResolution.height}, 全屏: {Fullscreen}");
            }
            
            // 应用音量设置
            // 这里可以添加音量的具体实现
            
            // 触发设置应用完成事件
            GameEvents.TriggerSettingsApplied();
        }

        #endregion
    }
}