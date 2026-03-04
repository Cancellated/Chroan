using UnityEngine;
using MyGame.Events;
using MyGame.Input;

namespace MyGame.Managers
{
    /// <summary>
    /// 输入管理器，集中管理InputSystem实例
    /// 避免重复创建InputSystem实例，确保输入系统状态一致性
    /// </summary>
    public class InputManager : Singleton<InputManager>
    {
        #region 字段
        private GameControl _inputActions;
        #endregion

        #region 属性
        /// <summary>
        /// 全局唯一的InputActions实例
        /// </summary>
        public GameControl InputActions
        {
            get { return _inputActions; }
        }
        #endregion

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            
            // 创建InputActions实例
            _inputActions = new GameControl();
            
            // 默认启用游戏玩法输入
            _inputActions.GamePlay.Enable();
            
            // 初始化事件监听
            InitializeEventListeners();
        }

        private void OnDestroy()
        {
            // 清理资源
            if (_inputActions != null)
            {
                _inputActions.Disable();
                _inputActions.Dispose();
            }
            
            // 注销事件监听
            GameEvents.OnInputModeChangeRequest -= HandleInputModeChangeRequest;
            GameEvents.OnInputModeChange -= HandleInputModeChange;
        }
        #endregion

        #region 输入模式切换
        /// <summary>
        /// 切换到游戏玩法输入模式
        /// </summary>
        public void SwitchToGamePlayMode()
        {
            _inputActions.UI.Disable();
            _inputActions.GamePlay.Enable();
        }

        /// <summary>
        /// 切换到UI输入模式
        /// 特殊处理：保留控制台按键的功能，即使在UI模式下也能响应
        /// </summary>
        public void SwitchToUIMode()
        {
            _inputActions.GamePlay.Disable();
            // 单独启用控制台按键，确保在任何模式下都能唤出控制台
            _inputActions.GamePlay.Console.Enable();
            _inputActions.UI.Enable();
        }

        /// <summary>
        /// 同时启用游戏玩法和UI输入模式
        /// </summary>
        public void EnableBothModes()
        {
            _inputActions.GamePlay.Enable();
            _inputActions.UI.Enable();
        }

        /// <summary>
        /// 禁用所有输入
        /// </summary>
        public void DisableAllInputs()
        {
            _inputActions.GamePlay.Disable();
            _inputActions.UI.Disable();
        }
        #endregion

        #region 事件监听

        /// <summary>
        /// 初始化事件监听
        /// </summary>
        private void InitializeEventListeners()
        {
            // 监听输入模式切换请求
            GameEvents.OnInputModeChangeRequest += HandleInputModeChangeRequest;
            
            // 监听输入模式切换事件（直接执行）
            GameEvents.OnInputModeChange += HandleInputModeChange;
        }

        /// <summary>
        /// 处理输入模式切换请求
        /// </summary>
        /// <param name="targetMode">目标模式</param>
        /// <param name="force">是否强制切换</param>
        private void HandleInputModeChangeRequest(InputMode targetMode, bool force)
        {
            // 可以在这里添加权限检查、条件判断等逻辑
            if (CanSwitchToMode(targetMode) || force)
            {
                SwitchToMode(targetMode);
                
                // 触发实际切换事件
                GameEvents.TriggerInputModeChange(targetMode);
            }
        }

        /// <summary>
        /// 处理输入模式切换事件（直接执行）
        /// </summary>
        /// <param name="targetMode">目标模式</param>
        private void HandleInputModeChange(InputMode targetMode)
        {
            SwitchToMode(targetMode);
        }

        /// <summary>
        /// 检查是否可以切换到指定模式
        /// </summary>
        /// <param name="targetMode">目标模式</param>
        /// <returns>是否可以切换</returns>
        private bool CanSwitchToMode(InputMode targetMode)
        {
            // 可以在这里添加切换条件检查
            // 例如：某些特殊情况下不允许切换到游戏玩法模式
            return true;
        }

        /// <summary>
        /// 统一的输入模式切换方法
        /// </summary>
        /// <param name="targetMode">目标模式</param>
        private void SwitchToMode(InputMode targetMode)
        {
            switch (targetMode)
            {
                case InputMode.GamePlay:
                    SwitchToGamePlayMode();
                    break;
                case InputMode.UI:
                    SwitchToUIMode();
                    break;
                case InputMode.Both:
                    EnableBothModes();
                    break;
                case InputMode.None:
                    DisableAllInputs();
                    break;
            }
            
            UnityEngine.Debug.Log($"输入模式已切换到: {targetMode}");
        }

        #endregion
    }
}