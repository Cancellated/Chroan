using MyGame.Events;
using MyGame.Managers;
using Logger;
using UnityEngine;
using MyGame.DevTool;

namespace MyGame.UI.Control
{
    /// <summary>
    /// UI控制器，负责处理UI相关的输入事件和控制UI面板的显示隐藏
    /// 它监听唤出UI的按键输入，并通知事件系统-对应面板监听到对应事件后完成相应的方法
    /// </summary>
    public class UIController : Singleton<UIController>
    {
        #region 常量
        private const string LOG_MODULE = LogModules.UI;
        #endregion

        #region 字段
        private GameControl _inputActions;
        #endregion

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            
            // 延迟一帧获取InputManager，避免初始化顺序问题
            StartCoroutine(InitializeInputActionsCoroutine());
        }
        
        private System.Collections.IEnumerator InitializeInputActionsCoroutine()
        {
            // 等待一帧，确保所有Awake方法都已执行
            yield return null;
            
            // 获取InputManager中的InputActions实例
            try
            {
                if (InputManager.Instance != null && InputManager.Instance.InputActions != null)
                {
                    _inputActions = InputManager.Instance.InputActions;
                    Log.Info(LOG_MODULE, "UI控制器初始化完成");
                    // 重新注册输入回调，确保使用正确的_inputActions
                    UnregisterInputCallbacks();
                    RegisterInputCallbacks();
                }
                else
                {
                    Log.Error(LOG_MODULE, "InputManager instance not found! Creating fallback input actions.");
                    _inputActions = new GameControl();
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(LOG_MODULE, $"初始化输入动作时发生异常: {ex.Message}");
                // 确保即使发生异常，也有一个可用的输入动作实例
                _inputActions ??= new GameControl();
            }
        }

        private void OnEnable()
        {
            // 注册按键回调
            RegisterInputCallbacks();
        }

        private void OnDisable()
        {
            // 注销按键回调
            UnregisterInputCallbacks();
        }
        #endregion

        #region 输入处理
        /// <summary>
        /// 注册输入回调
        /// </summary>
        private void RegisterInputCallbacks()
        {
            if (_inputActions != null)
            {
                // 注册游戏玩法中的UI相关按键
                _inputActions.GamePlay.Pause.performed += OnPausePerformed;
                _inputActions.GamePlay.Inventory.performed += OnInventoryPerformed;
                _inputActions.GamePlay.Console.performed += OnConsolePerformed;
            }
        }

        /// <summary>
        /// 注销输入回调
        /// </summary>
        private void UnregisterInputCallbacks()
        {
            if (_inputActions != null)
            {
                _inputActions.GamePlay.Pause.performed -= OnPausePerformed;
                _inputActions.GamePlay.Inventory.performed -= OnInventoryPerformed;
                _inputActions.GamePlay.Console.performed -= OnConsolePerformed;
            }
        }

        /// <summary>
        /// 暂停菜单按键处理
        /// </summary>
        private void OnPausePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Log.Info(LOG_MODULE, "暂停菜单按键被按下");
                
                // 检查暂停菜单当前状态并切换
                if (UIManager.Instance != null && UIManager.Instance.PanelMap.ContainsKey(UIType.PauseMenu))
                {
                    bool isCurrentlyVisible = UIManager.Instance.PanelMap[UIType.PauseMenu].IsVisible;
                    // 触发暂停菜单显示/隐藏事件，切换当前状态
                    GameEvents.TriggerMenuShow(UIType.PauseMenu, !isCurrentlyVisible);
                }
                else
                {
                    // 如果无法获取面板状态，默认显示
                    GameEvents.TriggerMenuShow(UIType.PauseMenu, true);
                }
            }
        }

        /// <summary>
        /// 物品栏按键处理
        /// </summary>
        private void OnInventoryPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Log.Info(LOG_MODULE, "物品栏按键被按下");
                
                // 检查物品栏当前状态并切换
                if (UIManager.Instance != null && UIManager.Instance.PanelMap.ContainsKey(UIType.Inventory))
                {
                    bool isCurrentlyVisible = UIManager.Instance.PanelMap[UIType.Inventory].IsVisible;
                    // 触发物品栏显示/隐藏事件，切换当前状态
                    GameEvents.TriggerMenuShow(UIType.Inventory, !isCurrentlyVisible);
                }
                else
                {
                    // 如果无法获取面板状态，默认显示
                    GameEvents.TriggerMenuShow(UIType.Inventory, true);
                }
            }
        }

        /// <summary>
        /// 控制台按键处理
        /// </summary>
        private void OnConsolePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Log.Info(LOG_MODULE, "控制台按键被按下");

                // 检查控制台当前状态并切换
                bool isCurrentlyVisible = DebugConsole.Instance.IsVisible;

                // 触发控制台显示/隐藏事件，切换当前状态
                GameEvents.TriggerMenuShow(UIType.Console, !isCurrentlyVisible);
            }
        }
        #endregion
    }
}