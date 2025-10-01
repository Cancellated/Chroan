using UnityEngine;
using UnityEngine.UI;
using Logger;

namespace MyGame.UI.Settings.Components
{
    /// <summary>
    /// 音频设置组件
    /// 负责处理音频相关设置的UI和交互
    /// </summary>
    public class AudioSettingsComponent : BaseSettingsComponent
    {
        #region 字段

        [Header("Audio Settings")]
        [Tooltip("音乐音量滑块")]
        [SerializeField] private Slider m_musicVolumeSlider;

        [Tooltip("音效音量滑块")]
        [SerializeField] private Slider m_sfxVolumeSlider;

        private const string LOG_MODULE = LogModules.SETTINGS;

        #endregion

        #region 抽象方法实现

        /// <summary>
        /// 初始化音频设置组件的UI和数据
        /// </summary>
        protected override void InitializeComponent()
        {
            // 设置滑块范围
            if (m_musicVolumeSlider != null)
            {
                m_musicVolumeSlider.minValue = 0f;
                m_musicVolumeSlider.maxValue = 1f;
                m_musicVolumeSlider.value = 1f; // 默认最大音量
            }

            if (m_sfxVolumeSlider != null)
            {
                m_sfxVolumeSlider.minValue = 0f;
                m_sfxVolumeSlider.maxValue = 1f;
                m_sfxVolumeSlider.value = 1f; // 默认最大音量
            }
        }

        /// <summary>
        /// 绑定音频设置相关的用户交互事件
        /// </summary>
        protected override void BindEvents()
        {
            // 绑定音量设置事件
            if (m_musicVolumeSlider != null)
            {
                m_musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (m_sfxVolumeSlider != null)
            {
                m_sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }
        }

        /// <summary>
        /// 更新音频设置组件的显示状态
        /// </summary>
        public override void UpdateView()
        {
            if (m_controller == null)
                return;
        }

        /// <summary>
        /// 清理音频设置组件资源，解绑事件
        /// </summary>
        protected override void Cleanup()
        {
            // 解绑事件监听
            if (m_musicVolumeSlider != null)
            {
                m_musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }

            if (m_sfxVolumeSlider != null)
            {
                m_sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            }
        }

        #endregion

        #region 事件处理方法

        /// <summary>
        /// 音乐音量变化事件处理
        /// </summary>
        /// <param name="value">新的音量值</param>
        private void OnMusicVolumeChanged(float value)
        {
            if (m_controller != null)
            {
                m_controller.UpdateMusicVolume(value);
            }
        }

        /// <summary>
        /// 音效音量变化事件处理
        /// </summary>
        /// <param name="value">新的音量值</param>
        private void OnSfxVolumeChanged(float value)
        {
            if (m_controller != null)
            {
                m_controller.UpdateSfxVolume(value);
            }
        }

        #endregion
    }
}