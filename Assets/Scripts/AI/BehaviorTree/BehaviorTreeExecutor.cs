using System;
using System.Collections.Generic;
using UnityEngine;
using Logger;
using AI.BehaviorTree;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 行为树执行器
    /// 负责执行单个行为树
    /// </summary>
    public class BehaviorTreeExecutor
    {
        /// <summary>
        /// 行为树的根节点
        /// </summary>
        public BTNode RootNode { get; private set; }

        /// <summary>
        /// 行为树是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 行为树是否暂停
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// 行为树的当前状态
        /// </summary>
        public BTNodeState CurrentState { get; private set; }

        /// <summary>
        /// 执行间隔（秒）
        /// </summary>
        public float ExecutionInterval { get; set; }

        /// <summary>
        /// 行为树名称
        /// </summary>
        public string TreeName { get; set; }

        /// <summary>
        /// 执行计数
        /// </summary>
        private int executionCount;

        /// <summary>
        /// 距离下次执行的时间
        /// </summary>
        private float timeUntilNextExecution;

        /// <summary>
        /// 树更新事件
        /// </summary>
        public event Action<BTNodeState> OnTreeUpdated;

        /// <summary>
        /// 树开始事件
        /// </summary>
        public event Action OnTreeStarted;

        /// <summary>
        /// 树结束事件
        /// </summary>
        public event Action<BTNodeState> OnTreeCompleted;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rootNode">根节点</param>
        /// <param name="treeName">行为树名称</param>
        /// <param name="executionInterval">执行间隔（秒）</param>
        public BehaviorTreeExecutor(BTNode rootNode, string treeName = "BehaviorTree", float executionInterval = 0f)
        {
            RootNode = rootNode;
            TreeName = treeName;
            ExecutionInterval = executionInterval;
            
            IsRunning = false;
            IsPaused = false;
            CurrentState = BTNodeState.Failure;
            executionCount = 0;
            timeUntilNextExecution = 0f;
        }

        /// <summary>
        /// 开始执行行为树
        /// </summary>
        public void Start()
        {
            if (RootNode == null)
            {
                Log.Error($"行为树 '{TreeName}' 的根节点为空！", LogModules.AI);
                return;
            }

            if (IsRunning)
            {
                Log.Warning($"行为树 '{TreeName}' 已经在运行中！", LogModules.AI);
                return;
            }

            // 重置所有节点
            ResetTree();

            IsRunning = true;
            IsPaused = false;
            executionCount = 0;
            timeUntilNextExecution = 0f;

            OnTreeStarted?.Invoke();
            Log.Info($"行为树 '{TreeName}' 开始执行", LogModules.AI);
        }

        /// <summary>
        /// 停止执行行为树
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;
            IsPaused = false;

            // 重置所有节点
            ResetTree();

            Log.Info($"行为树 '{TreeName}' 停止执行", LogModules.AI);
        }

        /// <summary>
        /// 暂停执行
        /// </summary>
        public void Pause()
        {
            if (IsRunning && !IsPaused)
            {
                IsPaused = true;
                Log.Info($"行为树 '{TreeName}' 暂停执行", LogModules.AI);
            }
        }

        /// <summary>
        /// 恢复执行
        /// </summary>
        public void Resume()
        {
            if (IsRunning && IsPaused)
            {
                IsPaused = false;
                Log.Info($"行为树 '{TreeName}' 恢复执行", LogModules.AI);
            }
        }

        /// <summary>
        /// 重置行为树
        /// </summary>
        public void ResetTree()
        {
            RootNode?.Reset();
            CurrentState = BTNodeState.Failure;
            executionCount = 0;
            timeUntilNextExecution = 0f;
        }

        /// <summary>
        /// 更新行为树（应在Update中调用）
        /// </summary>
        public void Update()
        {
            if (!IsRunning || IsPaused || RootNode == null)
            {
                return;
            }

            // 处理执行间隔
            if (ExecutionInterval > 0f)
            {
                timeUntilNextExecution -= Time.deltaTime;
                if (timeUntilNextExecution > 0f)
                {
                    return;
                }
                timeUntilNextExecution = ExecutionInterval;
            }

            // 执行行为树
            executionCount++;
            CurrentState = RootNode.Execute();

            // 触发更新事件
            OnTreeUpdated?.Invoke(CurrentState);

            // 检查是否完成
            if (CurrentState != BTNodeState.Running)
            {
                IsRunning = false;
                OnTreeCompleted?.Invoke(CurrentState);
                Log.Info($"行为树 '{TreeName}' 执行完成，状态: {CurrentState}，执行次数: {executionCount}", LogModules.AI);
            }
        }

        /// <summary>
        /// 获取调试信息
        /// </summary>
        /// <returns>调试信息字符串</returns>
        public string GetDebugInfo()
        {
            return $"树名: {TreeName}, " +
                   $"状态: {CurrentState}, " +
                   $"运行中: {IsRunning}, " +
                   $"暂停: {IsPaused}, " +
                   $"执行次数: {executionCount}";
        }
    }
}