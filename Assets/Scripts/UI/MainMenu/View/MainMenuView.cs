using MyGame.UI.MainMenu.Controller;
using MyGame.UI.MainMenu.View.Components;
using UnityEngine;
using UnityEngine.UI;
using Logger;
using MyGame.Managers;

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
        
        [Tooltip("测试场景按钮")]
        [SerializeField] private Button m_testSceneButton;

        [Header("键盘选择系统")]
        [Tooltip("是否启用键盘选择")]
        [SerializeField] private bool m_enableKeyboardSelection = true;

        [Tooltip("选择指示器对象")]
        [SerializeField] private GameObject m_selectionIndicator;

        private MainMenuButtonIndicator m_buttonSelector;

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
            
            // 初始化键盘选择系统
            InitializeKeyboardSelection();
        }
        
        /// <summary>
        /// 初始化键盘选择系统
        /// </summary>
        private void InitializeKeyboardSelection()
        {
            if (!m_enableKeyboardSelection) return;

            // 切换到UI输入模式，确保在主菜单场景中使用正确的输入处理
            if (InputManager.Instance != null)
            {
                InputManager.Instance.SwitchToUIMode();
            }
            else
            {
                Log.Warning(LOG_MODULE, "InputManager实例不存在，无法切换到UI输入模式！");
            }

            // 获取或添加选择器组件
            m_buttonSelector = gameObject.GetComponent<MainMenuButtonIndicator>();
            if (m_buttonSelector == null)
            {
                m_buttonSelector = gameObject.AddComponent<MainMenuButtonIndicator>();
            }

            // 设置已存在的选择指示器
            if (m_selectionIndicator != null)
            {
                m_buttonSelector.SelectionIndicator = m_selectionIndicator;
            }
            else
            {
                Log.Warning(LOG_MODULE, "没有指定选择指示器对象！");
            }

            // 添加按钮到选择器
            if (m_buttonSelector != null)
            {
                // 清除现有按钮列表
                m_buttonSelector.MenuButtons.Clear();
                
                // 添加所有有效按钮
                if (m_startGameButton != null)
                    m_buttonSelector.AddButton(m_startGameButton);
                if (m_loadGameButton != null)
                    m_buttonSelector.AddButton(m_loadGameButton);
                if (m_testSceneButton != null)
                    m_buttonSelector.AddButton(m_testSceneButton);
                if (m_exitGameButton != null)
                    m_buttonSelector.AddButton(m_exitGameButton);
            }
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
            
            // 激活键盘选择系统
            if (m_enableKeyboardSelection && m_buttonSelector != null)
            {
                m_buttonSelector.enabled = true;
                
                // 确保选择第一个可用按钮
                if (m_buttonSelector.MenuButtons.Count > 0)
                {
                    m_buttonSelector.SelectedButtonIndex = 0;
                }
            }
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
            
            // 禁用键盘选择系统
            if (m_enableKeyboardSelection && m_buttonSelector != null)
            {
                m_buttonSelector.enabled = false;
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
            
            // 解绑按钮事件
            UnbindButtonEvents();
            
            // 清理选择器组件
            if (m_buttonSelector != null && 
            m_buttonSelector.gameObject == gameObject &&
            m_buttonSelector.GetInstanceID() == GetInstanceIDOfComponent(gameObject.GetComponent<MainMenuButtonIndicator>()))
            {
                Destroy(m_buttonSelector);
                m_buttonSelector = null;
            }
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
            
            if (m_testSceneButton != null)
                m_testSceneButton.onClick.AddListener(OnTestSceneButtonClick);
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
            
            if (m_testSceneButton != null)
                m_testSceneButton.onClick.RemoveListener(OnTestSceneButtonClick);
        }
        
        /// <summary>
        /// 获取组件的InstanceID，如果组件为null则返回-1
        /// </summary>
        /// <param name="component">要获取ID的组件</param>
        /// <returns>组件的InstanceID或-1</returns>
        private int GetInstanceIDOfComponent(Component component)
        {
            return component != null ? component.GetInstanceID() : -1;
        }

        #endregion

        #region 事件响应

        /// <summary>
        /// 开始游戏按钮点击事件
        /// </summary>
        private void OnStartGameButtonClick()
        {
            // 触发按钮点击动画
            if (m_startGameButton != null)
            {
                m_startGameButton.TriggerClickAnimation();
            }
            
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
            // 触发按钮点击动画
            if (m_settingsButton != null)
            {
                m_settingsButton.TriggerClickAnimation();
            }
            
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
            // 触发按钮点击动画
            if (m_aboutButton != null)
            {
                m_aboutButton.TriggerClickAnimation();
            }
            
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
            // 触发按钮点击动画
            if (m_exitGameButton != null)
            {
                m_exitGameButton.TriggerClickAnimation();
            }
            
            if (m_controller != null)
            {
                m_controller.OnExitGame();
            }
        }
        
        /// <summary>
        /// 测试场景按钮点击事件
        /// </summary>
        private void OnTestSceneButtonClick()
        {
            // 触发按钮点击动画
            if (m_testSceneButton != null)
            {
                m_testSceneButton.TriggerClickAnimation();
            }
            
            if (m_controller != null)
            {
                m_controller.OnLoadTestScene();
            }
        }

        #endregion
    }
}