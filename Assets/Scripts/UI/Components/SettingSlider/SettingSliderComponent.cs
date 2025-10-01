using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Logger;

namespace MyGame.UI.Components.SettingSlider
{
    /// <summary>
    /// 设置滑块组件
    /// 包含滑块、标题和百分比显示的通用设置控制单元
    /// 适用于游戏中各种需要滑动条调整的设置项
    /// </summary>
    public class SettingSliderComponent : MonoBehaviour
    {
        #region 字段

        [Header("Setting Slider Components")]
        [Tooltip("设置控制滑块")]
        [SerializeField] private Slider m_slider;

        [Tooltip("百分比显示文本")]
        [SerializeField] private TextMeshProUGUI m_percentageText;

        [Tooltip("滑块标题文本")]
        [SerializeField] private TextMeshProUGUI m_titleText;

        private const string LOG_MODULE = LogModules.UI_COMPONENTS;
        private float m_currentValue = 1f;

        #endregion

        #region 事件定义

        /// <summary>
        /// 滑块值变化事件
        /// </summary>
        public event System.Action<float> OnValueChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置当前滑块值(0-1范围)
        /// </summary>
        public float CurrentValue
        {
            get { return m_currentValue; }
            set { SetValue(value); }
        }

        /// <summary>
        /// 获取或设置滑块标题
        /// </summary>
        public string Title
        {
            get { return m_titleText != null ? m_titleText.text : string.Empty; }
            set
            {
                if (m_titleText != null)
                {
                    m_titleText.text = value;
                }
            }
        }

        #endregion

        #region Unity生命周期

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void Awake()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 销毁组件，清理事件监听
        /// </summary>
        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置滑块值，更新UI并触发事件
        /// </summary>
        /// <param name="value">滑块值(0-1范围)</param>
        public void SetValue(float value)
        {
            // 确保值在有效范围内
            value = Mathf.Clamp01(value);
            m_currentValue = value;

            if (m_slider != null)
            {
                // 暂时移除事件监听以避免循环调用
                m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
                m_slider.value = value;
                m_slider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            UpdatePercentageText(value);
        }

        /// <summary>
        /// 设置滑块的交互状态
        /// </summary>
        /// <param name="interactable">是否可交互</param>
        public void SetInteractable(bool interactable)
        {
            if (m_slider != null)
            {
                m_slider.interactable = interactable;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化组件的UI和数据
        /// </summary>
        private void InitializeComponent()
        {
            Log.Info(LOG_MODULE, "初始化设置滑块组件");

            // 设置滑块范围
            if (m_slider != null)
            {
                m_slider.minValue = 0f;
                m_slider.maxValue = 1f;
                m_slider.value = m_currentValue;
                m_slider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            // 初始化百分比文本
            UpdatePercentageText(m_currentValue);
        }

        /// <summary>
        /// 滑块值变化事件处理
        /// </summary>
        /// <param name="value">新的滑块值</param>
        private void OnSliderValueChanged(float value)
        {
            m_currentValue = value;
            UpdatePercentageText(value);

            // 触发值变化事件
            OnValueChanged?.Invoke(value);
        }

        /// <summary>
        /// 更新百分比文本显示
        /// </summary>
        /// <param name="value">滑块值(0-1范围)</param>
        private void UpdatePercentageText(float value)
        {
            if (m_percentageText != null)
            {
                // 将0-1范围转换为0-100百分比
                int percentage = Mathf.RoundToInt(value * 100f);
                m_percentageText.text = $"{percentage}";
            }
        }

        /// <summary>
        /// 清理资源，解绑事件监听
        /// </summary>
        private void Cleanup()
        {
            if (m_slider != null)
            {
                m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }

            // 清空事件订阅者
            OnValueChanged = null;
        }

        #endregion
    }
}