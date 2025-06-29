using MyGame.Managers; // 引入游戏状态枚举
using System;
using UI.Managers;
using UnityEngine;
using MyGame.Data;

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
        public static void TriggerGameStart()
        {
            Debug.Log("[GameEvents] 触发游戏开始事件");
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// 进入章节选择事件
        /// </summary>
        public static event Action OnChapterSelectMap;
        public static void TriggerChapterSelectMap()
        {
            Debug.Log("[GameEvents] 触发进入章节选择事件");
            OnChapterSelectMap.Invoke();

        }

        /// <summary>
        /// 选择章节事件
        /// </summary>
        public static event Action<ChapterData> OnChapterSelected;
        public static void TriggerChapterSelected(ChapterData chapter)
        {
            Debug.Log($"[GameEvents] 选择章节：{chapter.chapterName}");
            OnChapterSelected?.Invoke(chapter);
        }

        /// <summary>
        /// 选择关卡事件
        /// </summary>
        public static event Action<LevelData> OnLevelSelected;
        public static void TriggerLevelSelected(LevelData level)
        {
            Debug.Log($"[GameEvents] 触发选择关卡事件，选择关卡：{level.levelName}");
            OnLevelSelected?.Invoke(level);
        }


        /// <summary>
        /// 游戏暂停事件。
        /// </summary>
        public static event Action OnGamePause;
        public static void TriggerGamePause()
        {
            Debug.Log("[GameEvents] 触发游戏暂停事件");
            OnGamePause?.Invoke();
        }

        /// <summary>
        /// 游戏继续事件。
        /// </summary>
        public static event Action OnGameResume;
        public static void TriggerGameResume()
        {
            Debug.Log("[GameEvents] 触发游戏继续事件");
            OnGameResume?.Invoke();
        }

        /// <summary>
        /// 游戏结束事件，参数为true表示胜利，false表示失败。
        /// </summary>
        public static event Action<bool> OnGameOver;
        public static void TriggerGameOver(bool isWin)
        {
            Debug.Log($"[GameEvents] 触发游戏结束事件，胜利：{isWin}");
            OnGameOver?.Invoke(isWin);
        }

        ///<summary>
        /// 章节完成事件
        /// </summary>
        public static event Action<ChapterData> OnChapterComplete;
        public static void TriggerChapterComplete(ChapterData chapter)
        {
            Debug.Log($"[GameEvents] 触发章节完成事件，完成章节：{chapter.chapterName}");
            OnChapterComplete?.Invoke(chapter);
        }

        /// <summary>
        /// 游戏状态变更事件。
        /// </summary>
        public static event Action<GameState, GameState> OnGameStateChanged;
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
        public static event Action<string> OnSceneLoad;
        public static void TriggerSceneLoad(string sceneName)
        {
            Debug.Log($"[GameEvents] 开始加载场景: {sceneName}");
            OnSceneLoad?.Invoke(sceneName);
        }
        
        /// <summary>
        /// 场景加载完成事件
        /// </summary>
        public static event Action<string> OnSceneLoadComplete;
        public static void TriggerSceneLoadComplete(string sceneName)
        {
            Debug.Log($"[GameEvents] 场景加载完成: {sceneName}");
            OnSceneLoadComplete?.Invoke(sceneName);
        }
        
        /// <summary>
        /// 场景卸载事件
        /// </summary>
        public static event Action<string> OnSceneUnload;
        public static void TriggerSceneUnload(string sceneName)
        {
            Debug.Log($"[GameEvents] 卸载场景: {sceneName}");
            OnSceneUnload?.Invoke(sceneName);
        }

        #endregion
        #region 对话管理事件
        /// <summary>
        /// 处理故事进入事件, 触发对话系统开始
        /// </summary>
        /// <param name="storyId"></param>
        public static event Action<int> OnStoryEnter;
        public static void TriggerStoryEnter(int storyId)
        {
            Debug.Log($"[GameEvents] 触发故事进入事件,storyId: {storyId}");
            OnStoryEnter?.Invoke(storyId);
        }
        
        /// <summary>
        /// 处理对话结束事件, 触发对话系统结束
        /// </summary>
        public static event Action<int> OnStoryComplete;
        public static void TriggerStoryComplete(int storyId)
        {
            Debug.Log($"[GameEvents] 触发故事完成事件,storyId: {storyId}");
            OnStoryComplete?.Invoke(storyId);
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

        /// <summary>
        /// 显示或隐藏设置
        /// </summary>
        public static event Action<bool> OnSettingsShow;
        public static void TriggerSettingsShow(bool show)
        {
            Debug.Log($"[GameEvents] 设置显示：{show}");
            OnSettingsShow?.Invoke(show);
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
        #region 音效事件
        /// <summary>
        /// 场景音效事件
        /// </summary>
        public static event Action<string> OnSceneSoundTriggered;
        public static void TriggerSceneSound(string soundType)
        {
            OnSceneSoundTriggered?.Invoke(soundType);
        }
        /// <summary>
        /// UI交互音效
        /// </summary>
        public static event Action<string> OnUIInteraction;
        public static void TriggerUIInteraction(string soundType)
        {
            OnUIInteraction?.Invoke(soundType);
        }
        #endregion
        #region CG事件
        /// <summary>
        /// CG开始事件
        /// </summary>
        public static event Action<int> OnCGStart;
        public static void TriggerCGStart(int cgId)
        {
            OnCGStart?.Invoke(cgId);
        }
        /// <summary>
        /// CG结束事件
        /// </summary>
        public static event Action<int> OnCGComplete;
        public static void TriggerCGComplete(int cgId)
        {
            OnCGComplete?.Invoke(cgId);
        }
        #endregion
    }
}