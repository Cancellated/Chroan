using MyGame.Managers;
using System;
using UnityEngine;
using Logger;
using MyGame.UI;
using AI.BehaviorTree;

namespace MyGame.Events
{

    /// <summary>
    /// 全局静态事件系统，支持类型安全的事件注册与触发。
    /// 用于模块间解耦通信。
    /// </summary>
    public static class GameEvents
    {
        const string module = LogModules.GAMEEVENTS;
        #region 游戏流程事件

        /// <summary>
        /// 游戏开始事件。
        /// </summary>
        public static event Action OnGameStart;
        
        public static void TriggerGameStart()
        {
            Log.Info(module, "触发游戏开始事件");
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// 游戏暂停事件。
        /// </summary>
        public static event Action OnGamePause;
        
        public static void TriggerGamePause()
        {
            Log.Info(module, "触发游戏暂停事件");
            OnGamePause?.Invoke();
        }

        /// <summary>
        /// 游戏继续事件。
        /// </summary>
        public static event Action OnGameResume;
        
        public static void TriggerGameResume()
        {
            Log.Info(module, "触发游戏继续事件");
            OnGameResume?.Invoke();
        }

        /// <summary>
        /// 游戏结束事件，参数为true表示胜利，false表示失败。
        /// </summary>
        public static event Action<bool> OnGameOver;
        
        public static void TriggerGameOver(bool isWin)
        {
            Log.Info(module, $"触发游戏结束事件，胜利：{isWin}");
            OnGameOver?.Invoke(isWin);
        }

        /// <summary>
        /// 游戏状态变更事件。
        /// </summary>
        public static event Action<GameState, GameState> OnGameStateChanged;
        
        public static void TriggerGameStateChanged(GameState from, GameState to)
        {
            Log.Info(module, $"游戏状态变更：{from} -> {to}");
            OnGameStateChanged?.Invoke(from, to);
        }

        #endregion

        #region 场景管理事件

        /// <summary>
        /// 场景加载开始事件
        /// </summary>
        public static event Action<string> OnSceneLoadStart;
        
        public static void TriggerSceneLoadStart(string sceneName)
        {
            Log.Info(module, $"触发开始加载场景事件: {sceneName}");
            OnSceneLoadStart?.Invoke(sceneName);
        }
        
        /// <summary>
        /// 场景加载完成事件
        /// </summary>
        public static event Action<string> OnSceneLoadComplete;
        
        public static void TriggerSceneLoadComplete(string sceneName)
        {
            Log.Info(module, $"触发场景加载完成事件: {sceneName}");
            OnSceneLoadComplete?.Invoke(sceneName);
        }
        
        /// <summary>
        /// 加载界面准备就绪事件
        /// 当加载界面完全显示后触发，用于开始实际的场景加载
        /// </summary>
        public static event Action<string> OnLoadingScreenReady;
        
        public static void TriggerLoadingScreenReady(string sceneName)
        {
            Log.Info(module, $"触发加载界面准备就绪事件: {sceneName}");
            OnLoadingScreenReady?.Invoke(sceneName);
        }
        
        /// <summary>
        /// 场景卸载事件
        /// </summary>
        public static event Action<string> OnSceneUnload;
        
        public static void TriggerSceneUnload(string sceneName)
        {
            Log.Info(module, $"触发场景卸载事件: {sceneName}");
            OnSceneUnload?.Invoke(sceneName);
        }

        #endregion

        #region 存档相关事件

        /// <summary>
        /// 新游戏创建事件
        /// </summary>
        public static event Action OnCreateNewGame;
        
        public static void TriggerCreateNewGame()
        {
            Log.Info(module, "触发新游戏创建事件");
            OnCreateNewGame?.Invoke();
        }

        /// <summary>
        /// 游戏数据保存完成事件
        /// </summary>
        public static event Action<string> OnSaveGame;
        
        public static void TriggerSaveGame(string slotName)
        {
            Log.Info(module, $"触发游戏数据保存完成事件: {slotName}");
            OnSaveGame?.Invoke(slotName);
        }

        /// <summary>
        /// 自动保存事件
        /// </summary>
        public static event Action<string> OnAutoSave;
        
        public static void TriggerAutoSave(string slotName = "AutoSave")
        {
            Log.Info(module, $"触发自动保存事件: {slotName}");
            OnAutoSave?.Invoke(slotName);
        }

        /// <summary>
        /// 游戏数据加载完成事件
        /// </summary>
        public static event Action<string> OnLoadGame;
        
        public static void TriggerLoadGame(string slotName)
        {
            Log.Info(module, $"触发游戏数据加载完成事件: {slotName}");
            OnLoadGame?.Invoke(slotName);
        }

        /// <summary>
        /// 游戏数据删除事件
        /// </summary>
        public static event Action<string> OnDeleteSave;
        
        public static void TriggerDeleteSave(string slotName)
        {
            Log.Info(module, $"触发游戏数据删除事件: {slotName}");
            OnDeleteSave?.Invoke(slotName);
        }

        #endregion

        #region 设置相关事件

        /// <summary>
        /// 设置应用完成事件
        /// 当游戏设置被应用到游戏时触发
        /// </summary>
        public static event Action OnSettingsApplied;
        
        public static void TriggerSettingsApplied()
        {
            Log.Info(module, "触发设置应用完成事件");
            OnSettingsApplied?.Invoke();
        }
        
        /// <summary>
        /// 设置保存完成事件
        /// 当游戏设置被保存时触发
        /// </summary>
        public static event Action OnSettingsSaved;
        
        public static void TriggerSettingsSaved()
        {
            Log.Info(module, "触发设置保存完成事件");
            OnSettingsSaved?.Invoke();
        }
        
        #endregion

        #region UI事件

        /// <summary>
        /// UI状态切换事件（统一管理所有UI的显示/隐藏）
        /// </summary>
        public static event Action<UIType, bool> OnMenuShow;
        
        public static void TriggerMenuShow(UIType menu, bool show)
        {
            Log.Info(module, $"UI切换：{menu} 显示：{show}");
            OnMenuShow?.Invoke(menu, show);
        }

        #endregion

        #region AI相关事件
        /// <summary>
        /// 感知到威胁事件
        /// </summary>
        public static event Action<GameObject, GameObject> OnThreatDetected;
        
        public static void TriggerThreatDetected(GameObject self, GameObject threatSource)
        {
            Log.Info(module, $"触发威胁检测事件: {self.name} 检测到威胁 {(threatSource != null ? threatSource.name : null)}");
            OnThreatDetected?.Invoke(self, threatSource);
        }

        /// <summary>
        /// 行为开始事件
        /// </summary>
        public static event Action<GameObject, string> OnBehaviorStarted;
        
        public static void TriggerBehaviorStarted(GameObject self, string behaviorName)
        {
            // Log.Info(module, $"触发行为开始事件: {self.name} 开始行为 {behaviorName}");
            OnBehaviorStarted?.Invoke(self, behaviorName);
        }

        /// <summary>
        /// 行为完成事件
        /// </summary>
        public static event Action<GameObject, string, bool> OnBehaviorCompleted;
        
        public static void TriggerBehaviorCompleted(GameObject self, string behaviorName, bool success)
        {
            // Log.Info(module, $"触发行为完成事件: {self.name} 完成行为 {behaviorName}, 结果: {success}");
            OnBehaviorCompleted?.Invoke(self, behaviorName, success);
        }

        /// <summary>
        /// 行为中断事件
        /// </summary>
        public static event Action<GameObject, string, string> OnBehaviorInterrupted;
        
        public static void TriggerBehaviorInterrupted(GameObject self, string behaviorName, string reason)
        {
            // Log.Info(module, $"触发行为中断事件: {self.name} 中断行为 {behaviorName}, 原因: {reason}");
            OnBehaviorInterrupted?.Invoke(self, behaviorName, reason);
        }


        /// <summary>
        /// 节点执行开始事件
        /// </summary>
        public static event Action<BTNode, string> OnNodeExecutionStarted;
        
        public static void TriggerNodeExecutionStarted(BTNode node, string nodeName)
        {
            // Log.Info(module, $"触发节点执行开始事件: 节点 {nodeName}");
            OnNodeExecutionStarted?.Invoke(node, nodeName);
        }

        /// <summary>
        /// 节点执行完成事件
        /// </summary>
        public static event Action<BTNode, string, BTNodeState> OnNodeExecutionCompleted;
        
        public static void TriggerNodeExecutionCompleted(BTNode node, string nodeName, BTNodeState result)
        {
            // Log.Info(module, $"触发节点执行完成事件: 节点 {nodeName}, 结果: {result}");
            OnNodeExecutionCompleted?.Invoke(node, nodeName, result);
        }

        /// <summary>
        /// 节点状态改变事件
        /// </summary>
        public static event Action<BTNode, string, BTNodeState, BTNodeState> OnNodeStateChanged;
        
        public static void TriggerNodeStateChanged(BTNode node, string nodeName, BTNodeState oldState, BTNodeState newState)
        {
            // Log.Info(module, $"触发节点状态改变事件: 节点 {nodeName}, 从 {oldState} 变为 {newState}");
            OnNodeStateChanged?.Invoke(node, nodeName, oldState, newState);
        }

        /// <summary>
        /// 开始移动事件
        /// </summary>
        public static event Action<GameObject, Vector3> OnMovementStarted;
        
        public static void TriggerMovementStarted(GameObject self, Vector3 targetPosition)
        {
            // Log.Info(module, $"触发移动开始事件: {self.name} 开始向位置 {targetPosition} 移动");
            OnMovementStarted?.Invoke(self, targetPosition);
        }

        /// <summary>
        /// 移动完成事件
        /// </summary>
        public static event Action<GameObject, Vector3, bool> OnMovementCompleted;
        
        public static void TriggerMovementCompleted(GameObject self, Vector3 targetPosition, bool reached)
        {
            // Log.Info(module, $"触发移动完成事件: {self.name} 移动到位置 {targetPosition}, 到达: {reached}");
            OnMovementCompleted?.Invoke(self, targetPosition, reached);
        }

        /// <summary>
        /// 移动被阻挡事件
        /// </summary>
        public static event Action<GameObject, GameObject> OnMovementBlocked;
        
        public static void TriggerMovementBlocked(GameObject self, GameObject obstacle)
        {
            // Log.Info(module, $"触发移动被阻挡事件: {self.name} 被 {(obstacle != null ? obstacle.name : null)} 阻挡");
            OnMovementBlocked?.Invoke(self, obstacle);
        }

        #endregion
    }
}