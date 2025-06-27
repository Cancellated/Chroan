using MyGame.Managers; // 引入游戏状态枚举
using System;
using UI.Managers;
using UnityEngine;

namespace MyGame.System
{

    /// <summary>
    /// 全局静态事件系统，支持类型安全的事件注册与触发。
    /// 用于模块间解耦通信。
    /// </summary>
    public static class GameEvents
    {
        #region 游戏流程事件

        /// <summary>
        /// 游戏开始事件。
        /// </summary>
        public static event Action OnGameStart;

        /// <summary>
        /// 游戏暂停事件。
        /// </summary>
        public static event Action OnGamePause;

        /// <summary>
        /// 游戏继续事件。
        /// </summary>
        public static event Action OnGameResume;

        /// <summary>
        /// 游戏结束事件，参数为true表示胜利，false表示失败。
        /// </summary>
        public static event Action<bool> OnGameOver;

        /// <summary>
        /// 游戏状态变更事件。
        /// </summary>
        public static event Action<GameState, GameState> OnGameStateChanged;

        /// <summary>
        /// 触发游戏开始事件。
        /// </summary>
        public static void TriggerGameStart()
        {
            Debug.Log("[GameEvents] 触发游戏开始事件");
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// 触发游戏暂停事件。
        /// </summary>
        public static void TriggerGamePause()
        {
            Debug.Log("[GameEvents] 触发游戏暂停事件");
            OnGamePause?.Invoke();
        }

        /// <summary>
        /// 触发游戏继续事件。
        /// </summary>
        public static void TriggerGameResume()
        {
            Debug.Log("[GameEvents] 触发游戏继续事件");
            OnGameResume?.Invoke();
        }

        /// <summary>
        /// 触发游戏结束事件。
        /// </summary>
        /// <param name="isWin">true为胜利，false为失败</param>
        public static void TriggerGameOver(bool isWin)
        {
            Debug.Log($"[GameEvents] 触发游戏结束事件，胜利：{isWin}");
            OnGameOver?.Invoke(isWin);
        }

        /// <summary>
        /// 触发游戏状态变更事件。
        /// </summary>
        /// <param name="from">原状态</param>
        /// <param name="to">新状态</param>
        public static void TriggerGameStateChanged(GameState from, GameState to)
        {
            Debug.Log($"[GameEvents] 游戏状态变更：{from} -> {to}");
            OnGameStateChanged?.Invoke(from, to);
        }

        #endregion

        #region 场景管理事件

        /// <summary>
        /// 场景加载开始事件
        /// </summary>
        public static event Action<string> OnSceneLoadStart;
        
        /// <summary>
        /// 场景加载完成事件
        /// </summary>
        public static event Action<string> OnSceneLoadComplete;
        
        /// <summary>
        /// 场景卸载事件
        /// </summary>
        public static event Action<string> OnSceneUnload;
        
        /// <summary>
        /// 触发场景加载开始事件
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public static void TriggerSceneLoadStart(string sceneName)
        {
            Debug.Log($"[GameEvents] 开始加载场景: {sceneName}");
            OnSceneLoadStart?.Invoke(sceneName);
        }
        
        /// <summary>
        /// 触发场景加载完成事件
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public static void TriggerSceneLoadComplete(string sceneName)
        {
            Debug.Log($"[GameEvents] 场景加载完成: {sceneName}");
            OnSceneLoadComplete?.Invoke(sceneName);
        }
        
        /// <summary>
        /// 触发场景卸载事件
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public static void TriggerSceneUnload(string sceneName)
        {
            Debug.Log($"[GameEvents] 卸载场景: {sceneName}");
            OnSceneUnload?.Invoke(sceneName);
        }

        #endregion

        #region UI事件

        /// <summary>
        /// 显示或隐藏主菜单
        /// </summary>
        public static event Action<bool> OnMainMenuShow;
        public static void TriggerMainMenuShow(bool show)
        {
            Debug.Log($"[GameEvents] 主菜单显示：{show}");
            OnMainMenuShow?.Invoke(show);
        }

        /// <summary>
        /// 显示或隐藏暂停菜单
        /// </summary>
        public static event Action<bool> OnPauseMenuShow;
        public static void TriggerPauseMenuShow(bool show)
        {
            Debug.Log($"[GameEvents] 暂停菜单显示：{show}");
            OnPauseMenuShow?.Invoke(show);
        }

        /// <summary>
        /// 显示结算界面（参数：true胜利，false失败）
        /// </summary>
        public static event Action<bool> OnResultPanelShow;
        public static void TriggerResultPanelShow(bool isWin)
        {
            Debug.Log($"[GameEvents] 结算界面显示，胜利：{isWin}");
            OnResultPanelShow?.Invoke(isWin);
        }

        /// <summary>
        /// 显示或隐藏HUD
        /// </summary>
        public static event Action<bool> OnHUDShow;
        public static void TriggerHUDShow(bool show)
        {
            Debug.Log($"[GameEvents] HUD显示：{show}");
            OnHUDShow?.Invoke(show);
        }
        
        /// <summary>
        /// 显示或隐藏控制台
        /// </summary>
        public static event Action<bool> OnConsoleShow;
        public static void TriggerConsoleShow(bool show)
        {
            Debug.Log($"[GameEvents] 控制台显示：{show}");
            OnConsoleShow?.Invoke(show);
        }
        /// <summary>
        /// 显示或隐藏背包
        /// </summary>
        public static event Action<bool> OnInventoryShow;
        public static void TriggerInventoryShow(bool show)
        {
            Debug.Log($"[GameEvents] 背包显示：{show}");
            OnInventoryShow?.Invoke(show);
        }
            #region UI切换事件

        /// <summary>
        /// UI状态切换事件（互斥显示）
        /// </summary>
        public static event Action<UIManager.UIState, bool> OnMenuShow;


        /// <summary>
        /// 触发UI状态切换事件
        /// </summary>
        public static void TriggerMenuShow(UIManager.UIState state, bool show)
        {
            Debug.Log($"[GameEvents] 菜单切换：{state} 显示：{show}");
            OnMenuShow?.Invoke(state, show);
        }

            #endregion
        #endregion
    }
}