using System;
using UnityEngine;
using Logger;
using System.Collections.Generic;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 叶子节点基类
    /// 叶子节点是行为树的末端节点，实际执行具体的动作
    /// </summary>
    public abstract class BTLeafNode : BTNode
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        protected BTLeafNode(string nodeName = "Leaf") : base(nodeName)
        {
        }
    }

    /// <summary>
    /// 动作委托类型的叶子节点
    /// 使用委托来执行自定义动作
    /// </summary>
    public class BTActionNode : BTLeafNode
    {
        /// <summary>
        /// 动作委托类型
        /// </summary>
        public Func<BTNodeState> Action { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action">要执行的动作委托</param>
        /// <param name="nodeName">节点名称</param>
        public BTActionNode(Func<BTNodeState> action, string nodeName = "Action") : base(nodeName)
        {
            Action = action;
        }

        /// <summary>
        /// 设置动作委托
        /// </summary>
        /// <param name="action">新的动作委托</param>
        public void SetAction(Func<BTNodeState> action)
        {
            Action = action;
            Reset();
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (Action == null)
            {
                Log.Warning($"动作节点 '{NodeName}' 的动作委托为空！", LogModules.AI);
                State = BTNodeState.Failure;
                return State;
            }

            State = Action();
            return State;
        }
    }

    /// <summary>
    /// 条件检查叶子节点
    /// 用于检查某个条件是否满足
    /// </summary>
    public class BTConditionNode : BTLeafNode
    {
        /// <summary>
        /// 条件委托类型
        /// </summary>
        public Func<bool> Condition { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="condition">要检查的条件委托</param>
        /// <param name="nodeName">节点名称</param>
        public BTConditionNode(Func<bool> condition, string nodeName = "Condition") : base(nodeName)
        {
            Condition = condition;
        }

        /// <summary>
        /// 设置条件委托
        /// </summary>
        /// <param name="condition">新的条件委托</param>
        public void SetCondition(Func<bool> condition)
        {
            Condition = condition;
            Reset();
        }

        /// <summary>
        /// 执行条件检查
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (Condition == null)
            {
                Log.Warning($"条件节点 '{NodeName}' 的条件委托为空！", LogModules.AI);
                State = BTNodeState.Failure;
                return State;
            }

            bool result = Condition();
            State = result ? BTNodeState.Success : BTNodeState.Failure;
            return State;
        }
    }

    /// <summary>
    /// Unity组件操作叶子节点
    /// 用于操作Unity游戏对象和组件
    /// </summary>
    public class BTComponentNode : BTLeafNode
    {
        /// <summary>
        /// 目标游戏对象
        /// </summary>
        public GameObject TargetObject { get; set; }

        /// <summary>
        /// 组件操作委托
        /// </summary>
        public Func<GameObject, BTNodeState> ComponentAction { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="targetObject">目标游戏对象</param>
        /// <param name="componentAction">组件操作委托</param>
        /// <param name="nodeName">节点名称</param>
        public BTComponentNode(GameObject targetObject, Func<GameObject, BTNodeState> componentAction, string nodeName = "ComponentAction") : base(nodeName)
        {
            TargetObject = targetObject;
            ComponentAction = componentAction;
        }

        /// <summary>
        /// 设置目标对象和操作
        /// </summary>
        /// <param name="targetObject">目标游戏对象</param>
        /// <param name="componentAction">组件操作委托</param>
        public void SetTarget(GameObject targetObject, Func<GameObject, BTNodeState> componentAction)
        {
            TargetObject = targetObject;
            ComponentAction = componentAction;
            Reset();
        }

        /// <summary>
        /// 执行组件操作
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (TargetObject == null)
            {
                Log.Warning($"组件操作节点 '{NodeName}' 的目标对象为空！", LogModules.AI);
                State = BTNodeState.Failure;
                return State;
            }

            if (ComponentAction == null)
            {
                Log.Warning($"组件操作节点 '{NodeName}' 的操作委托为空！", LogModules.AI);
                State = BTNodeState.Failure;
                return State;
            }

            State = ComponentAction(TargetObject);
            return State;
        }
    }

    /// <summary>
    /// 等待叶子节点
    /// 用于让行为树等待一段时间
    /// </summary>
    public class BTWaitNode : BTLeafNode
    {
        /// <summary>
        /// 等待时间（秒）
        /// </summary>
        public float WaitTime { get; set; }

        /// <summary>
        /// 当前已等待的时间
        /// </summary>
        private float currentWaitTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="waitTime">等待时间（秒）</param>
        /// <param name="nodeName">节点名称</param>
        public BTWaitNode(float waitTime, string nodeName = "Wait") : base(nodeName)
        {
            WaitTime = waitTime;
            currentWaitTime = 0f;
        }

        /// <summary>
        /// 执行等待逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            // 如果是新开始等待，重置计时器
            if (State != BTNodeState.Running)
            {
                currentWaitTime = 0f;
                State = BTNodeState.Running;
            }

            // 累积时间
            currentWaitTime += Time.deltaTime;

            if (currentWaitTime >= WaitTime)
            {
                // 等待完成
                State = BTNodeState.Success;
            }
            else
            {
                // 继续等待
                State = BTNodeState.Running;
            }

            return State;
        }

        /// <summary>
        /// 重置等待节点
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            currentWaitTime = 0f;
        }
    }

    /// <summary>
    /// 日志记录叶子节点
    /// 用于在行为树执行过程中记录日志
    /// </summary>
    public class BTLogNode : BTLeafNode
    {
        /// <summary>
        /// 日志消息
        /// </summary>
        public string LogMessage { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType LogType { get; set; }

        /// <summary>
        /// 是否只记录一次（每次重置时记录）
        /// </summary>
        public bool LogOnce { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logMessage">日志消息</param>
        /// <param name="logType">日志类型</param>
        /// <param name="logOnce">是否只记录一次</param>
        /// <param name="nodeName">节点名称</param>
        public BTLogNode(string logMessage, LogType logType = LogType.Log, bool logOnce = true, string nodeName = "Log") : base(nodeName)
        {
            LogMessage = logMessage;
            LogType = logType;
            LogOnce = logOnce;
        }

        /// <summary>
        /// 执行日志记录
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            // 根据日志类型记录日志
            switch (LogType)
            {
                case LogType.Log:
                    Log.Info(LogMessage, LogModules.AI);
                    break;
                case LogType.Warning:
                    Log.Warning(LogMessage, LogModules.AI);
                    break;
                case LogType.Error:
                    Log.Error(LogMessage, LogModules.AI);
                    break;
                default:
                    Log.Info(LogMessage, LogModules.AI);
                    break;
            }

            State = BTNodeState.Success;
            return State;
        }

        /// <summary>
        /// 重置日志节点
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            if (LogOnce)
            {
                // 如果只记录一次，重置状态为失败，这样下次执行时会重新记录
                State = BTNodeState.Failure;
            }
        }
    }

    /// <summary>
    /// 随机选择叶子节点
    /// 从多个选项中随机选择一个执行
    /// </summary>
    public class BTRandomSelector : BTLeafNode
    {
        /// <summary>
        /// 随机选项列表
        /// </summary>
        private List<Func<BTNodeState>> options;

        /// <summary>
        /// 当前选择的选项索引
        /// </summary>
        private int selectedIndex;

        /// <summary>
        /// 随机数生成器
        /// </summary>
        private System.Random random;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        public BTRandomSelector(string nodeName = "RandomSelector") : base(nodeName)
        {
            options = new List<Func<BTNodeState>>();
            random = new System.Random();
        }

        /// <summary>
        /// 添加随机选项
        /// </summary>
        /// <param name="option">选项委托</param>
        public void AddOption(Func<BTNodeState> option)
        {
            options.Add(option);
        }

        /// <summary>
        /// 执行随机选择
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (options.Count == 0)
            {
                State = BTNodeState.Failure;
                return State;
            }

            // 随机选择一个选项
            selectedIndex = random.Next(options.Count);
            State = options[selectedIndex]();
            return State;
        }

        /// <summary>
        /// 重置随机选择器
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            selectedIndex = -1;
        }
    }
}