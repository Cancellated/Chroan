using UnityEngine;
using TMPro;
using Logger;
using System.Collections.Generic;
using UnityEngine.UI;
using MyGame.UI.Components;

namespace MyGame.UI.Settings.Components
{
    /// <summary>
    /// 图形设置组件
    /// 负责处理图形相关设置的UI和交互
    /// </summary>
    public class GraphicsSettingsComponent : BaseSettingsComponent
    {
        #region 字段

        [Header("Graphics Settings")]
        [Tooltip("画质等级下拉框")]
        [SerializeField] private TMP_Dropdown m_qualityDropdown;

        [Tooltip("全屏开关")]
        [SerializeField] private ToggleSwitch m_fullscreenToggle;

        [Tooltip("分辨率下拉框")]
        [SerializeField] private TMP_Dropdown m_resolutionDropdown;

        private const string LOG_MODULE = LogModules.SETTINGS;
        private Resolution[] m_resolutions;

        #endregion

        #region 抽象方法实现

        /// <summary>
        /// 初始化图形设置组件的UI和数据
        /// </summary>
        protected override void InitializeComponent()
        {
            InitializeResolutionDropdown();
            InitializeQualityDropdown();
        }

        /// <summary>
        /// 绑定图形设置相关的用户交互事件
        /// </summary>
        protected override void BindEvents()
        {
            // 绑定画质设置事件
            if (m_qualityDropdown != null)
            {
                m_qualityDropdown.onValueChanged.AddListener(OnQualityLevelChanged);
            }

            // 绑定全屏设置事件
            if (m_fullscreenToggle != null)
            {
                m_fullscreenToggle.OnValueChanged += OnFullscreenChanged;
            }

            // 绑定分辨率设置事件
            if (m_resolutionDropdown != null)
            {
                m_resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }
        }

        /// <summary>
        /// 更新图形设置组件的显示状态
        /// </summary>
        public override void UpdateView()
        {
            if (m_controller == null)
                return;
                
            // 更新全屏状态
            if (m_fullscreenToggle != null)
            {
                bool isFullscreen = m_controller.IsFullscreen();
                // 临时移除事件监听器，避免设置值时触发事件
                m_fullscreenToggle.OnValueChanged -= OnFullscreenChanged;
                // 设置值
                m_fullscreenToggle.IsOn = isFullscreen;
                // 重新添加事件监听器
                m_fullscreenToggle.OnValueChanged += OnFullscreenChanged;
            }
            
            // 更新画质等级
            if (m_qualityDropdown != null)
            {
                int qualityLevel = m_controller.GetQualityLevel();
                m_qualityDropdown.SetValueWithoutNotify(qualityLevel);
            }
            
            // 更新分辨率
            SetCurrentResolution();
        }

        /// <summary>
        /// 清理图形设置组件资源，解绑事件
        /// </summary>
        protected override void Cleanup()
        {
            // 解绑事件监听
            if (m_qualityDropdown != null)
            {
                m_qualityDropdown.onValueChanged.RemoveListener(OnQualityLevelChanged);
            }

            if (m_fullscreenToggle != null)
            {
                m_fullscreenToggle.OnValueChanged -= OnFullscreenChanged;
            }

            if (m_resolutionDropdown != null)
            {
                m_resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            }
        }

        #endregion

        #region 事件处理方法

        /// <summary>
        /// 画质等级变化事件处理
        /// </summary>
        /// <param name="value">新的画质等级索引</param>
        private void OnQualityLevelChanged(int value)
        {
            if (m_controller != null)
            {
                m_controller.UpdateQualityLevel(value);
            }
        }

        /// <summary>
        /// 全屏设置变化事件处理
        /// </summary>
        /// <param name="value">新的全屏状态</param>
        private void OnFullscreenChanged(bool value)
        {
            if (m_controller != null)
            {
                m_controller.UpdateFullscreen(value);
            }
        }

        /// <summary>
        /// 分辨率变化事件处理
        /// </summary>
        /// <param name="value">新的分辨率索引</param>
        private void OnResolutionChanged(int value)
        {
            if (m_controller != null)
            {
                m_controller.UpdateResolutionIndex(value);
            }
        }

        #endregion

        #region 初始化辅助方法

        /// <summary>
        /// 初始化分辨率下拉框
        /// </summary>
        private void InitializeResolutionDropdown()
        {
            if (m_resolutionDropdown == null)
                return;

            // 清空现有选项
            m_resolutionDropdown.ClearOptions();

            // 获取系统支持的所有分辨率
            m_resolutions = Screen.resolutions;
            
            // 移除重复的分辨率
            List<Resolution> uniqueResolutions = new();
            HashSet<string> resolutionKeys = new();

            foreach (Resolution resolution in m_resolutions)
            {
                string key = $"{resolution.width}x{resolution.height}";
                if (!resolutionKeys.Contains(key))
                {
                    resolutionKeys.Add(key);
                    uniqueResolutions.Add(resolution);
                }
            }

            // 更新分辨率数组为去重后的数组
            m_resolutions = uniqueResolutions.ToArray();

            // 生成分辨率选项文本
            List<string> options = new();
            for (int i = 0; i < m_resolutions.Length; i++)
            {
                string resolutionText = $"{m_resolutions[i].width}x{m_resolutions[i].height}";
                options.Add(resolutionText);
            }

            // 添加选项到下拉框
            m_resolutionDropdown.AddOptions(options);

            // 设置当前分辨率为默认选中项
            SetCurrentResolution();
        }

        /// <summary>
        /// 设置当前分辨率为选中项
        /// 优先使用控制器中存储的分辨率索引，只有在没有保存设置时才使用当前屏幕分辨率
        /// </summary>
        private void SetCurrentResolution()
        {
            if (m_resolutionDropdown == null || m_resolutions == null || m_resolutions.Length == 0)
                return;

            int currentResolutionIndex = -1;
            bool hasSavedResolution = false;

            // 优先使用控制器中保存的分辨率设置
            if (m_controller != null)
            {
                currentResolutionIndex = m_controller.GetResolutionIndex();
                // 确保索引在有效范围内
                if (currentResolutionIndex >= 0 && currentResolutionIndex < m_resolutions.Length)
                {
                    hasSavedResolution = true;
                }
            }

            // 如果没有保存的分辨率设置或者保存的索引无效，则使用当前屏幕分辨率
            if (!hasSavedResolution)
            {
                Resolution currentResolution = Screen.currentResolution;
                for (int i = 0; i < m_resolutions.Length; i++)
                {
                    if (m_resolutions[i].width == currentResolution.width &&
                        m_resolutions[i].height == currentResolution.height)
                    {
                        currentResolutionIndex = i;
                        break;
                    }
                }
            }

            // 设置选中项
            m_resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
        }

        /// <summary>
        /// 初始化画质下拉框
        /// </summary>
        private void InitializeQualityDropdown()
        {
            if (m_qualityDropdown == null)
                return;

            // 清空现有选项
            m_qualityDropdown.ClearOptions();

            List<string> qualityNames = null;
            
            // 优先使用控制器中提供的自定义画质名称
            if (m_controller != null)
            {
                qualityNames = m_controller.GetCustomQualityNames();
            }
            
            // 如果没有自定义画质名称，则使用Unity的默认画质名称
            if (qualityNames == null || qualityNames.Count == 0)
            {
                qualityNames = new List<string>(QualitySettings.names);
            }

            // 添加到下拉框
            m_qualityDropdown.AddOptions(qualityNames);

            // 设置当前画质等级为默认选中项
            m_qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        }

        #endregion
    }
}