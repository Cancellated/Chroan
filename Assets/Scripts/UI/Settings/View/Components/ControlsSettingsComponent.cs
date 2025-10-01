using UnityEngine;
using Logger;
using MyGame.UI.Components;

namespace MyGame.UI.Settings.Components
{
    /// <summary>
    /// 控制设置组件
    /// 负责处理控制相关设置的UI和交互
    /// </summary>
    public class ControlsSettingsComponent : BaseSettingsComponent
    {
        #region 字段

        [Header("Gameplay Settings")]
        [Tooltip("Y轴反转开关")]
        [SerializeField] private ToggleSwitch m_invertYAxisToggle;

        // 可以根据需要添加更多控制相关的UI元素

        private const string LOG_MODULE = LogModules.SETTINGS;

        #endregion

        #region 抽象方法实现

        /// <summary>
        /// 初始化控制设置组件的UI和数据
        /// </summary>
        protected override void InitializeComponent()
        {
            // 初始化Y轴反转开关为默认状态（不反转）
            if (m_invertYAxisToggle != null)
            {
                m_invertYAxisToggle.IsOn = false;
            }
        }

        /// <summary>
        /// 绑定控制设置相关的用户交互事件
        /// </summary>
        protected override void BindEvents()
        {
            // 绑定Y轴反转设置事件
            if (m_invertYAxisToggle != null)
            {
                m_invertYAxisToggle.OnValueChanged += OnInvertYAxisChanged;
            }

            // 可以根据需要添加更多控制相关的事件绑定
        }

        /// <summary>
        /// 更新控制设置组件的显示状态
        /// </summary>
        public override void UpdateView()
        {
            if (m_controller == null)
                return;

        }

        /// <summary>
        /// 清理控制设置组件资源，解绑事件
        /// </summary>
        protected override void Cleanup()
        {
            // 解绑事件监听
            if (m_invertYAxisToggle != null)
            {
                m_invertYAxisToggle.OnValueChanged -= OnInvertYAxisChanged;
            }

            // 可以根据需要添加更多控制相关的事件解绑
        }

        #endregion

        #region 事件处理方法

        /// <summary>
        /// Y轴反转设置变化事件处理
        /// </summary>
        /// <param name="value">新的Y轴反转状态</param>
        private void OnInvertYAxisChanged(bool value)
        {
            if (m_controller != null)
            {
                m_controller.UpdateInvertYAxis(value);
            }
        }

        // 可以根据需要添加更多控制相关的事件处理方法

        #endregion
    }
}