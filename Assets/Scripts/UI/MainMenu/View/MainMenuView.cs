using Logger;
using MyGame.UI.MainMenu.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.UI.MainMenu.View
{
    /// <summary>
    /// 主菜单视图，负责显示主菜单UI和处理用户输入
    /// </summary>
    public class MainMenuView : BaseView<MainMenuController>
    {
        #region 字段

        [Header("按钮")]
        [Tooltip("开始游戏按钮")]
        [SerializeField] private Button m_startGameButton;

        [Tooltip("加载游戏按钮")]
        [SerializeField] private Button m_loadGameButton;
        
        [Tooltip("设置按钮")]
        [SerializeField] private Button m_settingsButton;
        
        [Tooltip("关于按钮")]
        [SerializeField] private Button m_aboutButton;
        
        [Tooltip("退出游戏按钮")]
        [SerializeField] private Button m_exitGameButton;

        private const string LOG_MODULE = LogModules.MAINMENU;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化面板
        /// </summary>
        protected override void Awake()
        {
            // 设置面板类型
            m_panelType = UIType.MainMenu;
            base.Awake();
            
            // 绑定按钮事件
            BindButtonEvents();
        }

        /// <summary>
        /// 尝试自动绑定控制器
        /// </summary>
        protected override void TryBindController()
        {
            // 尝试在父物体中查找控制器
            if (!transform.parent.TryGetComponent<MainMenuController>(out var controller))
            {
                // 如果父物体中没有，尝试在根物体中查找
                controller = GetComponentInParent<MainMenuController>();
                if (controller == null)
                {
                    // 如果都没有，创建一个新的控制器组件
                    controller = gameObject.AddComponent<MainMenuController>();
                }
            }
            
            BindController(controller);
        }


        #endregion

        #region 公共方法

        /// <summary>
        /// 显示面板
        /// </summary>
        public override void Show()
        {
            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 1f;
                m_canvasGroup.interactable = true;
                m_canvasGroup.blocksRaycasts = true;
            }
            Log.Info(LOG_MODULE, "显示主菜单面板");
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public override void Hide()
        {
            // 主菜单直接显隐
            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 0f;
                m_canvasGroup.interactable = false;
                m_canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// 初始化面板
        /// </summary>
        public override void Initialize()
        {
            // 初始化时显示主菜单面板
            Show();
        }

        /// <summary>
        /// 清理面板资源
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
            
            // 清理资源
            UnbindButtonEvents();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        private void BindButtonEvents()
        {
            if (m_startGameButton != null)
                m_startGameButton.onClick.AddListener(OnStartGameButtonClick);
            
            if (m_settingsButton != null)
                m_settingsButton.onClick.AddListener(OnSettingsButtonClick);
            
            if (m_aboutButton != null)
                m_aboutButton.onClick.AddListener(OnAboutButtonClick);
            
            if (m_exitGameButton != null)
                m_exitGameButton.onClick.AddListener(OnExitGameButtonClick);
        }

        /// <summary>
        /// 解绑按钮事件
        /// </summary>
        private void UnbindButtonEvents()
        {
            if (m_startGameButton != null)
                m_startGameButton.onClick.RemoveListener(OnStartGameButtonClick);
            
            if (m_settingsButton != null)
                m_settingsButton.onClick.RemoveListener(OnSettingsButtonClick);
            
            if (m_aboutButton != null)
                m_aboutButton.onClick.RemoveListener(OnAboutButtonClick);
            
            if (m_exitGameButton != null)
                m_exitGameButton.onClick.RemoveListener(OnExitGameButtonClick);
        }

        #endregion

        #region 事件响应

        /// <summary>
        /// 开始游戏按钮点击事件
        /// </summary>
        private void OnStartGameButtonClick()
        {
            if (m_controller != null)
            {
                m_controller.OnStartGame();
            }
        }

        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private void OnSettingsButtonClick()
        {
            if (m_controller != null)
            {
                m_controller.OnShowSettings();
            }
        }

        /// <summary>
        /// 关于按钮点击事件
        /// </summary>
        private void OnAboutButtonClick()
        {
            if (m_controller != null)
            {
                m_controller.OnShowAbout();
            }
        }

        /// <summary>
        /// 退出游戏按钮点击事件
        /// </summary>
        private void OnExitGameButtonClick()
        {
            if (m_controller != null)
            {
                m_controller.OnExitGame();
            }
        }

        #endregion
    }
}