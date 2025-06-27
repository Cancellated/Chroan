using System.Collections;
using MyGame.System;
using UnityEngine;

namespace UI.Managers
{
    /// <summary>
    /// 全局UI管理器，负责调度和管理所有UI界面。
    /// 通过事件系统与其他模块通信，实现解耦。
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        #region UI引用

        [Header("UI面板引用")]
        public CanvasGroup mainMenu;
        public CanvasGroup pauseMenu;
        public CanvasGroup resultPanel;
        public CanvasGroup hudPanel;
        public CanvasGroup loadingPanel;
        public CanvasGroup consolePanel;
        public CanvasGroup inventoryPanel;

        [Header("动画设置")]
        [Tooltip("UI淡入淡出动画时长（秒）")]
        public float fadeDuration = 0.3f;

        [Header("动画组件")]
        public Animator inventoryAnimator;  // 背包动画组件

        #endregion

        #region 枚举和状态

        public enum UIState
        {
            None,
            MainMenu,
            PauseMenu,
            ResultPanel,
            HUD,
            Loading,
            Console,
            Inventory,
        }

        public UIState currentState = UIState.None;
        private GameControl _inputActions;

        #endregion

        #region 生命周期

        protected override void Awake()
        {
            base.Awake();
            _inputActions = new GameControl();  // 初始化输入系统
            
            // 初始隐藏所有UI
            HideAllUI();

            // 注册UI相关事件监听
            GameEvents.OnMenuShow += OnMenuShow;    //这个是通用窗口显隐处理
            GameEvents.OnMainMenuShow += ShowMainMenu;
            GameEvents.OnPauseMenuShow += ShowPauseMenu;
            GameEvents.OnResultPanelShow += ShowResultPanel;
            GameEvents.OnHUDShow += ShowHUD;
            GameEvents.OnConsoleShow += ShowConsole;
            GameEvents.OnInventoryShow += ShowInventory;
            // 加载界面显隐处理方法
            GameEvents.OnSceneLoadStart += ShowLoading;
            GameEvents.OnSceneLoadComplete += HideLoading;
        }

        private void OnDestroy()
        {
            // 注销事件监听
            GameEvents.OnMenuShow -= OnMenuShow;
            GameEvents.OnMainMenuShow -= ShowMainMenu;
            GameEvents.OnPauseMenuShow -= ShowPauseMenu;
            GameEvents.OnResultPanelShow -= ShowResultPanel;
            GameEvents.OnHUDShow -= ShowHUD;
            GameEvents.OnConsoleShow -= ShowConsole;
            GameEvents.OnInventoryShow -= ShowInventory;
            GameEvents.OnSceneLoadStart -= ShowLoading;
            GameEvents.OnSceneLoadComplete -= HideLoading;
        }

        #endregion

        #region UI控制核心方法

        /// <summary>
        /// 隐藏所有UI界面(控制台除外)
        /// </summary>
        private void HideAllUI()
        {
            ShowCanvasGroup(mainMenu, false);
            ShowCanvasGroup(pauseMenu, false);
            ShowCanvasGroup(resultPanel, false);
            ShowCanvasGroup(hudPanel, false);
            ShowCanvasGroup(loadingPanel, false);
            ShowCanvasGroup(inventoryPanel, false);
            currentState = UIState.None;
        }

        /// <summary>
        /// 设置UI状态并处理互斥关系
        /// </summary>
        private void SetUIState(UIState state, bool show)
        {
            // 处理互斥关系
            if (show)
            {
                _inputActions.GamePlay.Disable();
                _inputActions.UI.Enable();
                switch (state)
                {
                    case UIState.MainMenu:
                        SetUIState(UIState.PauseMenu, false);
                        SetUIState(UIState.HUD, false);
                        break;
                    case UIState.PauseMenu:
                        SetUIState(UIState.MainMenu, false);
                        SetUIState(UIState.ResultPanel, false);
                        break;
                    case UIState.ResultPanel:
                        SetUIState(UIState.PauseMenu, false);
                        SetUIState(UIState.HUD, false);
                        break;
                    case UIState.Inventory:
                        SetUIState(UIState.MainMenu, false);
                        SetUIState(UIState.PauseMenu, false);
                        break;
                    case UIState.Loading:
                        // 加载界面不与其他UI互斥
                    case UIState.Console:
                        // 调试界面不与其他UI互斥
                        break;
                }
            }

            // 更新当前状态
            if (show) currentState = state;
            else if (currentState == state) currentState = UIState.None;

            // 根据状态显示/隐藏对应UI
            switch (state)
            {
                case UIState.MainMenu:
                    ShowCanvasGroup(mainMenu, show);
                    break;
                case UIState.PauseMenu:
                    ShowCanvasGroup(pauseMenu, show);
                    break;
                case UIState.ResultPanel:
                    ShowCanvasGroup(resultPanel, show);
                    break;
                case UIState.HUD:
                    ShowCanvasGroup(hudPanel, show);
                    break;
                case UIState.Console:
                    SetCanvasGroup(consolePanel, show);
                    break;
                case UIState.Inventory:
                    ShowCanvasGroup(inventoryPanel, show);
                    break;
                case UIState.Loading:
                    ShowCanvasGroup(loadingPanel, show);
                    break;
            }
        }
        

            #region 通用UI显隐处理方法
        /// <summary>
        /// CanvasGroup显隐通用方法（带动画）
        /// </summary>
        private void ShowCanvasGroup(CanvasGroup group, bool show)
        {
            if (group == null) return;

            // 停止可能正在进行的动画
            StopAllCoroutines();

            // 启动新动画
            StartCoroutine(FadeCanvasGroup(group, show));
        }

        /// <summary>
        /// CanvasGroup淡入淡出动画协程
        /// </summary>
        private IEnumerator FadeCanvasGroup(CanvasGroup group, bool show)
        {
            float startAlpha = group.alpha;
            float targetAlpha = show ? 1f : 0f;
            float elapsed = 0f;

            // 动画期间禁用交互
            group.interactable = false;

            while (elapsed < fadeDuration)
            {
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                elapsed += Time.unscaledDeltaTime; // 使用unscaledTime确保暂停时也能工作
                yield return null;
            }

            group.alpha = targetAlpha;

            // 动画结束后恢复交互状态
            if (show)
            {
                group.interactable = true;
                group.blocksRaycasts = true;
            }
            else
            {
                group.blocksRaycasts = false;
            }
        }
        
        /// <summary>
        /// 直接显隐，不带动画
        /// </summary>
        /// <param name="canvasGroup">要设置的CanvasGroup</param>
        /// <param name="show">是否显示</param>
        private void SetCanvasGroup(CanvasGroup canvasGroup, bool show)
        {
            canvasGroup.alpha = show ? 1f : 0f;
            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;
        }
            #endregion
            
            #region 独有UI处理方法
                #region 加载界面显隐处理方法(因为加载界面用的参数不同所以单独拿出来)
        /// <summary>
        /// 显示加载界面
        /// </summary>
        private void ShowLoading(string sceneName)
        {
            SetUIState(UIState.Loading, true);
        }

        /// <summary>
        /// 隐藏加载界面
        /// </summary>
        private void HideLoading(string sceneName)
        {
            SetUIState(UIState.Loading, false);
        }
                #endregion
            #endregion
        #endregion

        #region UI事件响应(独特UI处理)

        private void ShowMainMenu(bool show)
        {
            SetUIState(UIState.MainMenu, show);
        }

        private void ShowPauseMenu(bool show)
        {
            SetUIState(UIState.PauseMenu, show);
        }

        private void ShowResultPanel(bool isWin)
        {
            SetUIState(UIState.ResultPanel, true);
            // 可在此处根据isWin显示不同内容
        }

        private void ShowHUD(bool show)
        {
            SetUIState(UIState.HUD, show);
        }

        private void ShowConsole(bool show)
        {
            SetUIState(UIState.Console, show);
        }
        /// <summary>
        /// 使用动画状态机管理背包显隐，制作独特动画
        /// <param name="IsOpen">动画状态机参数</param>
        /// 确保状态机有一个布尔参数IsOpen以及Open和Close状态
        /// </summary>
        private void ShowInventory(bool show) 
        {
            if (inventoryAnimator != null)
            {
                inventoryAnimator.SetBool("IsOpen", show);
                SetUIState(UIState.Inventory, show);
            }
            else
            {
                // 若没有动画状态机回退到原有动画逻辑
                ShowCanvasGroup(inventoryPanel, show);
            }
        }
        
        private void OnMenuShow(UIState state, bool show)
        {
            SetUIState(state, show);
        }

        #endregion

        #region 调试方法

#if UNITY_EDITOR
        [ContextMenu("隐藏所有UI")]
        public void DebugHideAllUI()
        {
            HideAllUI();
        }

        [ContextMenu("显示主菜单")]
        public void DebugShowMainMenu()
        {
            SetUIState(UIState.MainMenu, true);
        }

        [ContextMenu("显示暂停菜单")]
        public void DebugShowPauseMenu()
        {
            SetUIState(UIState.PauseMenu, true);
        }

        [ContextMenu("显示结算面板")]
        public void DebugShowResultPanel()
        {
            SetUIState(UIState.ResultPanel, true);
        }

        [ContextMenu("显示HUD")]
        public void DebugShowHUD()
        {
            SetUIState(UIState.HUD, true);
        }
#endif
        #endregion
    }
}