using MyGame.UI.About.Controller;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MyGame.UI.About.View
{
    /// <summary>
    /// 关于面板视图类，负责显示关于界面的UI元素和处理用户输入
    /// </summary>
    public class AboutPanelView : BaseView<AboutPanelController>
    {
        #region 字段

        [Header("UI References")]
        [Tooltip("关于面板根对象")]
        [SerializeField] private GameObject m_aboutPanel;

        [Tooltip("关闭按钮")]
        [SerializeField] private Button m_closeButton;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化面板
        /// </summary>
        protected override void Awake()
        {
            // 设置面板类型
            m_panelType = UIType.AboutPanel;
            base.Awake();

            // 绑定按钮事件
            BindButtonEvents();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 显示面板
        /// </summary>
        public override void Show()
        {
            base.Show();
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public override void Hide()
        {
            base.Hide();
        }


        #endregion

        #region 私有方法

        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        private void BindButtonEvents()
        {
            if (m_closeButton != null)
            {
                m_closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        /// <summary>
        /// 关闭按钮点击事件处理
        /// </summary>
        private void OnCloseButtonClicked()
        {
            if (m_controller != null)
            {
                m_controller.OnCloseButtonClick();
            }
            else
            {
                Hide();
            }
        }

        /// <summary>
        /// 尝试自动绑定控制器
        /// </summary>
        protected override void TryBindController()
        {
            // 尝试在父物体中查找控制器
            if (!transform.parent.TryGetComponent<AboutPanelController>(out var controller))
            {
                // 如果父物体中没有，尝试在根物体中查找
                controller = GetComponentInParent<AboutPanelController>();
                if (controller == null)
                {
                    // 如果都没有，创建一个新的控制器组件
                    controller = gameObject.AddComponent<AboutPanelController>();
                }
            }
            
            BindController(controller);
        }
        
        /// <summary>
        /// 控制器绑定后的回调
        /// </summary>
        protected override void OnControllerBound()
        {
            base.OnControllerBound();
            
            // 初始化控制器
            if (m_controller != null)
            {
                m_controller.Initialize();
            }
        }

        #endregion
    }
}