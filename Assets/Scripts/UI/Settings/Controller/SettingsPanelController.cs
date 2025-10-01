using System;
using UnityEngine;
using Logger;
using MyGame.UI.Settings.Model;
using MyGame.UI.Settings.View;
using System.Collections.Generic;

namespace MyGame.UI.Settings.Controller
{
    /// <summary>
    /// 设置面板控制器
    /// 负责处理设置面板的逻辑和设置更新
    /// </summary>
    public class SettingsPanelController : BaseController<SettingsPanelView, SettingsModel>
    {
        #region 字段

        [Header("Settings Panel View")]
        [Tooltip("设置面板视图引用")]
        [SerializeField] private SettingsPanelView m_settingsPanelView;

        private const string LOG_MODULE = LogModules.SETTINGS + "Controller";

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 控制器的Awake方法
        /// 初始化组件和绑定事件
        /// </summary>
        protected virtual void Awake()
        {
            Log.Info(LOG_MODULE, "设置面板控制器初始化");
            base.CreateAndInitializeModel();
            // 设置视图引用
            if (m_settingsPanelView != null)
            {
                SetView(m_settingsPanelView);
            }
        }

        /// <summary>
        /// 当控制器被启用时调用
        /// </summary>
        protected virtual void OnEnable()
        {
            Log.DebugLog(LOG_MODULE, "设置面板控制器启用");
            SetModels();
        }
        
        /// <summary>
        /// 模型设置后的回调
        /// 在模型设置完成后执行绑定事件和初始化面板的操作
        /// </summary>
        protected void SetModels()
        {
            Log.DebugLog(LOG_MODULE, "设置面板控制器: 模型已设置");
            BindModelEvents();
        }

        /// <summary>
        /// 当游戏对象禁用时调用
        /// </summary>
        private void OnDisable()
        {
            UnbindModelEvents();
        }

        /// <summary>
        /// 解绑设置模型事件
        /// </summary>
        private void UnbindModelEvents()
        {
            if (m_model != null)
            {
                m_model.OnPropertyChanged -= HandleModelPropertyChanged;
            }
        }

        #endregion

        #region 初始化和事件绑定

        /// <summary>
        /// 初始化设置面板
        /// </summary>
        private void InitializePanel()
        {
            Log.Info(LOG_MODULE, "初始化设置面板视图层");
            if (m_settingsPanelView != null)
            {
                m_settingsPanelView.Initialize();
                UpdateViewWithCurrentSettings();
            }
            else
            {
                Log.Error(LOG_MODULE, "设置面板视图为空，无法初始化");
            }
        }

        /// <summary>
        /// 绑定设置模型事件
        /// </summary>
        private void BindModelEvents()
        {
            Log.Info(LOG_MODULE, "绑定设置模型事件");
            if (m_model != null)
            {
                m_model.OnPropertyChanged += HandleModelPropertyChanged;
            }
            else
            {
                Log.Error(LOG_MODULE, "设置模型为空，无法绑定事件");
            }
        }

        #endregion

        #region 属性变化处理

        /// <summary>
        /// 处理设置模型属性变化
        /// </summary>
        /// <param name="propertyName">变化的属性名称</param>
        private void HandleModelPropertyChanged(string propertyName)
        {
            Log.Info(LOG_MODULE, "检测到设置模型属性变化: " + propertyName);
            UpdateViewWithCurrentSettings();
        }

        #endregion

        #region 通用值更新方法

        /// <summary>
        /// 通用的值更新方法，用于统一检查模型是否为空、值是否真正改变，并记录日志
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="currentValue">当前值</param>
        /// <param name="newValue">新值</param>
        /// <param name="isValueChanged">值比较器，用于确定值是否已改变</param>
        /// <param name="updateAction">值更新操作</param>
        /// <param name="logMessage">日志消息</param>
        /// <param name="errorMessage">模型为空时的错误消息</param>
        /// <param name="debugLogFormat">调试日志格式化函数</param>
        private void UpdateValue<T>(T currentValue, T newValue, Func<T, T, bool> isValueChanged, Action updateAction, string logMessage, string errorMessage, Func<T, T, string> debugLogFormat = null)
        {
            if (m_model != null)
            {
                // 检查值是否真正改变
                if (isValueChanged(currentValue, newValue))
                {
                    Log.Info(LOG_MODULE, logMessage);
                    
                    // 如果提供了调试日志格式化函数，则记录调试日志
                    if (debugLogFormat != null)
                    {
                        Log.DebugLog(LOG_MODULE, debugLogFormat(currentValue, newValue));
                    }
                    
                    // 执行更新操作
                    updateAction();
                }
            }
            else
            {
                Log.Error(LOG_MODULE, errorMessage);
            }
        }

        #endregion

        #region 设置更新方法

        /// <summary>
        /// 更新音乐音量设置
        /// </summary>
        /// <param name="volume">新的音量值</param>
        public void UpdateMusicVolume(float volume)
        {
            UpdateValue(
                m_model?.MusicVolume ?? 0f,
                volume,
                (current, newVal) => !Mathf.Approximately(current, newVal),
                () => m_model.MusicVolume = volume,
                "更新音乐音量: " + volume,
                "设置模型为空，无法更新音乐音量",
                (current, newVal) => string.Format("设置模型: 更新前音乐音量={0}, 更新后音量={1}", current, newVal)
            );
        }

        /// <summary>
        /// 更新音效音量设置
        /// </summary>
        /// <param name="volume">新的音量值</param>
        public void UpdateSfxVolume(float volume)
        {
            UpdateValue(
                m_model?.SfxVolume ?? 0f,
                volume,
                (current, newVal) => !Mathf.Approximately(current, newVal),
                () => m_model.SfxVolume = volume,
                "更新音效音量: " + volume,
                "设置模型为空，无法更新音效音量",
                (current, newVal) => string.Format("设置模型: 更新前音效音量={0}, 更新后音量={1}", current, newVal)
            );
        }

        /// <summary>
        /// 更新画质级别设置
        /// </summary>
        /// <param name="qualityLevel">新的画质级别</param>
        public void UpdateQualityLevel(int qualityLevel)
        {
            UpdateValue(
                m_model?.QualityLevel ?? 0,
                qualityLevel,
                (current, newVal) => current != newVal,
                () => m_model.QualityLevel = qualityLevel,
                "更新画质级别: " + qualityLevel,
                "设置模型为空，无法更新画质级别",
                (current, newVal) => string.Format("设置模型: 更新前画质级别={0}, 更新后画质级别={1}", current, newVal)
            );
        }

        /// <summary>
        /// 更新全屏状态设置
        /// </summary>
        /// <param name="isFullscreen">是否全屏</param>
        public void UpdateFullscreen(bool isFullscreen)
        {
            UpdateValue(
                m_model?.Fullscreen ?? false,
                isFullscreen,
                (current, newVal) => current != newVal,
                () => m_model.Fullscreen = isFullscreen,
                "更新全屏状态: " + (isFullscreen ? "全屏" : "窗口"),
                "设置模型为空，无法更新全屏状态",
                (current, newVal) => string.Format("设置模型: 更新前全屏状态={0}, 更新后全屏状态={1}", 
                    current ? "全屏" : "窗口", newVal ? "全屏" : "窗口")
            );
        }

        /// <summary>
        /// 更新分辨率索引设置
        /// </summary>
        /// <param name="resolutionIndex">新的分辨率索引</param>
        public void UpdateResolutionIndex(int resolutionIndex)
        {
            UpdateValue(
                m_model?.ResolutionIndex ?? 0,
                resolutionIndex,
                (current, newVal) => current != newVal,
                () => m_model.ResolutionIndex = resolutionIndex,
                "更新分辨率索引: " + resolutionIndex,
                "设置模型为空，无法更新分辨率索引",
                (current, newVal) => string.Format("设置模型: 更新前分辨率索引={0}, 更新后分辨率索引={1}", current, newVal)
            );
        }

        /// <summary>
        /// 更新Y轴反转设置
        /// </summary>
        /// <param name="invertYAxis">是否反转Y轴</param>
        public void UpdateInvertYAxis(bool invertYAxis)
        {
            UpdateValue(
                m_model?.InvertYAxis ?? false,
                invertYAxis,
                (current, newVal) => current != newVal,
                () => m_model.InvertYAxis = invertYAxis,
                "更新Y轴反转设置: " + (invertYAxis ? "反转" : "不反转"),
                "设置模型为空，无法更新Y轴反转设置",
                (current, newVal) => string.Format("设置模型: 更新前Y轴反转={0}, 更新后Y轴反转={1}", 
                    current ? "反转" : "不反转", newVal ? "反转" : "不反转")
            );
        }

        #endregion

        #region 设置操作方法

        /// <summary>
        /// 应用当前设置
        /// </summary>
        public void ApplySettings()
        {
            Log.Info(LOG_MODULE, "应用设置");
            if (m_model != null)
            {
                m_model.ApplySettings();
                Log.DebugLog(LOG_MODULE, "设置已成功应用");
            }
            else
            {
                Log.Error(LOG_MODULE, "设置模型为空，无法应用设置");
            }
        }

        /// <summary>
        /// 保存当前设置
        /// </summary>
        public void SaveSettings()
        {
            Log.Info(LOG_MODULE, "保存设置");
            if (m_model != null)
            {
                m_model.SaveSettings();
                Log.DebugLog(LOG_MODULE, "设置已成功保存");
            }
            else
            {
                Log.Error(LOG_MODULE, "设置模型为空，无法保存设置");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 更新视图以显示当前设置
        /// </summary>
        private void UpdateViewWithCurrentSettings()
        {
            if (m_settingsPanelView != null)
            {
                m_settingsPanelView.UpdateAllSettingsComponents();
            }
            else
            {
                Log.Error(LOG_MODULE, "设置面板视图为空，无法更新视图");
            }
        }

        #endregion

        #region 获取设置值方法

        /// <summary>
        /// 获取音乐音量
        /// </summary>
        /// <returns>音乐音量值</returns>
        public float GetMusicVolume()
        {
            if (m_model != null)
            {
                float volume = m_model.MusicVolume;
                Log.DebugLog(LOG_MODULE, "获取音乐音量: " + volume);
                return volume;
            }
            else
            {
                Log.Error(LOG_MODULE, "设置模型为空，返回默认音乐音量");
                return 1f; // 默认最大音量
            }
        }

        /// <summary>
        /// 获取音效音量
        /// </summary>
        /// <returns>音效音量值</returns>
        public float GetSfxVolume()
        {
            if (m_model != null)
            {
                float volume = m_model.SfxVolume;
                Log.DebugLog(LOG_MODULE, "获取音效音量: " + volume);
                return volume;
            }
            else
            {
                Log.Error(LOG_MODULE, "设置模型为空，返回默认音效音量");
                return 1f; // 默认最大音量
            }
        }

        /// <summary>
        /// 获取画质级别
        /// </summary>
        /// <returns>画质级别索引</returns>
        public int GetQualityLevel()
        {
            if (m_model != null)
            {
                int qualityLevel = m_model.QualityLevel;
                return qualityLevel;
            }
            else
            {
                Log.Error(LOG_MODULE, "设置模型为空，返回系统当前画质级别");
                return QualitySettings.GetQualityLevel();
            }
        }

        /// <summary>
        /// 获取全屏状态
        /// </summary>
        /// <returns>是否全屏</returns>
        public bool IsFullscreen()
        {
            if (m_model != null)
            {
                bool isFullscreen = m_model.Fullscreen;
                return isFullscreen;
            }
            else
            {
                Log.Error(LOG_MODULE, "设置模型为空，返回系统当前全屏状态");
                return Screen.fullScreen;
            }
        }

        /// <summary>
        /// 获取分辨率索引
        /// </summary>
        /// <returns>分辨率索引</returns>
        public int GetResolutionIndex()
        {
            if (m_model != null)
            {
                int resolutionIndex = m_model.ResolutionIndex;
                return resolutionIndex;
            }
            else
            {
                Log.Error(LOG_MODULE, "设置模型为空，返回默认分辨率索引");
                return 0;
            }
        }

        /// <summary>
        /// 获取自定义画质名称列表
        /// </summary>
        /// <returns>自定义画质名称列表</returns>
        public List<string> GetCustomQualityNames()
        {
            if (m_model == null)
            {
                Log.Error(LOG_MODULE, "Settings model is null when getting custom quality names");
                return null;
            }
            
            return m_model.CustomQualityNames;
        }

        #endregion
    }
}