using System;
using System.Collections.Generic;
using UnityEngine;
using Logger;
using MyGame;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 行为树管理器
    /// 管理多个行为树的执行
    /// 继承自Singleton基类实现单例模式
    /// </summary>
    public class BehaviorTreeManager : Singleton<BehaviorTreeManager>
    {

        /// <summary>
        /// 行为树字典
        /// </summary>
        private Dictionary<string, BehaviorTreeExecutor> behaviorTrees;

        /// <summary>
        /// 是否初始化
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// 事件：行为树开始
        /// </summary>
        public event Action<string> OnBehaviorTreeStarted;

        /// <summary>
        /// 事件：行为树结束
        /// </summary>
        public event Action<string, BTNodeState> OnBehaviorTreeCompleted;

        /// <summary>
        /// 事件：行为树更新
        /// </summary>
        public event Action<string, BTNodeState> OnBehaviorTreeUpdated;

        /// <summary>
        /// Awake
        /// 调用基类的Awake方法以确保单例逻辑正确执行
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            // 只有当当前实例是有效的单例实例时才进行初始化
            if (this == Instance)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            if (isInitialized) return;

            behaviorTrees = new Dictionary<string, BehaviorTreeExecutor>();
            isInitialized = true;

            Log.Info("行为树管理器初始化完成", LogModules.AI);
        }

        /// <summary>
        /// Update
        /// </summary>
        private void Update()
        {
            if (!isInitialized) return;

            // 更新所有运行中的行为树
            foreach (var tree in behaviorTrees.Values)
            {
                tree.Update();
            }
        }

        /// <summary>
        /// 注册行为树
        /// </summary>
        /// <param name="treeName">行为树名称</param>
        /// <param name="rootNode">根节点</param>
        /// <param name="executionInterval">执行间隔</param>
        /// <param name="autoStart">是否自动开始</param>
        /// <returns>行为树执行器</returns>
        public BehaviorTreeExecutor RegisterTree(string treeName, BTNode rootNode, float executionInterval = 0f, bool autoStart = false)
        {
            if (string.IsNullOrEmpty(treeName))
            {
                Log.Error("行为树名称不能为空！", LogModules.AI);
                return null;
            }

            if (rootNode == null)
            {
                Log.Error($"行为树 '{treeName}' 的根节点不能为空！", LogModules.AI);
                return null;
            }

            if (behaviorTrees.ContainsKey(treeName))
            {
                Log.Warning($"行为树 '{treeName}' 已经存在，将被覆盖！", LogModules.AI);
                UnregisterTree(treeName);
            }

            var executor = new BehaviorTreeExecutor(rootNode, treeName, executionInterval);

            // 绑定事件
            executor.OnTreeStarted += () => OnBehaviorTreeStarted?.Invoke(treeName);
            executor.OnTreeCompleted += (state) => OnBehaviorTreeCompleted?.Invoke(treeName, state);
            executor.OnTreeUpdated += (state) => OnBehaviorTreeUpdated?.Invoke(treeName, state);

            behaviorTrees[treeName] = executor;

            if (autoStart)
            {
                executor.Start();
            }

            Log.Info($"行为树 '{treeName}' 注册成功", LogModules.AI);
            return executor;
        }

        /// <summary>
        /// 注销行为树
        /// </summary>
        /// <param name="treeName">行为树名称</param>
        public void UnregisterTree(string treeName)
        {
            if (string.IsNullOrEmpty(treeName))
            {
                return;
            }

            if (behaviorTrees.ContainsKey(treeName))
            {
                var executor = behaviorTrees[treeName];
                executor.Stop();
                behaviorTrees.Remove(treeName);
                Log.Info($"行为树 '{treeName}' 注销成功", LogModules.AI);
            }
        }

        /// <summary>
        /// 获取行为树执行器
        /// </summary>
        /// <param name="treeName">行为树名称</param>
        /// <returns>行为树执行器</returns>
        public BehaviorTreeExecutor GetTree(string treeName)
        {
            if (string.IsNullOrEmpty(treeName) || !behaviorTrees.ContainsKey(treeName))
            {
                return null;
            }

            return behaviorTrees[treeName];
        }

        /// <summary>
        /// 开始行为树
        /// </summary>
        /// <param name="treeName">行为树名称</param>
        public void StartTree(string treeName)
        {
            var executor = GetTree(treeName);
            if (executor != null)
            {
                executor.Start();
            }
            else
            {
                Log.Error($"未找到行为树 '{treeName}'", LogModules.AI);
            }
        }

        /// <summary>
        /// 停止行为树
        /// </summary>
        /// <param name="treeName">行为树名称</param>
        public void StopTree(string treeName)
        {
            var executor = GetTree(treeName);
            executor?.Stop();
        }

        /// <summary>
        /// 暂停行为树
        /// </summary>
        /// <param name="treeName">行为树名称</param>
        public void PauseTree(string treeName)
        {
            var executor = GetTree(treeName);
            executor?.Pause();
        }

        /// <summary>
        /// 恢复行为树
        /// </summary>
        /// <param name="treeName">行为树名称</param>
        public void ResumeTree(string treeName)
        {
            var executor = GetTree(treeName);
            executor?.Resume();
        }

        /// <summary>
        /// 获取所有行为树名称
        /// </summary>
        /// <returns>行为树名称列表</returns>
        public List<string> GetAllTreeNames()
        {
            return new List<string>(behaviorTrees.Keys);
        }

        /// <summary>
        /// 清理所有行为树
        /// </summary>
        public void ClearAllTrees()
        {
            foreach (var executor in behaviorTrees.Values)
            {
                executor.Stop();
            }
            behaviorTrees.Clear();
            Log.Info("所有行为树已清理", LogModules.AI);
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        private void OnDestroy()
        {
            ClearAllTrees();
            // 不再需要手动设置Instance = null，基类会处理单例逻辑
        }
    }
}