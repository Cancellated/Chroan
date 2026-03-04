using System.Collections.Generic;
using UnityEngine;
using Logger;
using MyGame.Events;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 行为树节点基类
    /// 所有行为树节点都必须继承此类并实现Execute方法
    /// </summary>
    public abstract class BTNode
    {
        /// <summary>
        /// 节点的唯一标识符
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// 节点的执行状态私有字段
        /// </summary>
        private BTNodeState _state;

        /// <summary>
        /// 节点的执行状态
        /// 当状态变更时触发事件
        /// </summary>
        public BTNodeState State 
        { 
            get => _state;
            protected set
            {   
                // 状态发生变化时触发事件
                if (_state != value)
                {   
                    BTNodeState oldState = _state;
                    _state = value;
                    
                    // 触发状态变更事件
                    GameEvents.TriggerNodeStateChanged(this, NodeName, oldState, value);
                    Log.Debug(LogModules.AI, $"节点 {NodeName} 状态变更: {oldState} -> {value}");
                }
            } 
        }

        /// <summary>
        /// 节点是否正在运行
        /// </summary>
        public bool IsRunning => State == BTNodeState.Running;

        /// <summary>
        /// 节点的父节点
        /// </summary>
        public BTNode Parent { get; protected set; }

        /// <summary>
        /// 设置父节点
        /// </summary>
        /// <param name="parent">新的父节点</param>
        public void SetParent(BTNode parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// 节点的子节点列表
        /// </summary>
        public virtual List<BTNode> Children { get; protected set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        protected BTNode(string nodeName = "Node")
        {   
            NodeName = nodeName;
            // 直接初始化_state字段，避免触发状态变更事件
            _state = BTNodeState.Failure;
            Children = new List<BTNode>();
        }

        /// <summary>
        /// 执行节点
        /// 触发执行开始和完成事件，并调用实际执行逻辑
        /// </summary>
        /// <returns>节点的执行状态</returns>
        public BTNodeState Execute()
        {   
            // 触发节点执行开始事件
            GameEvents.TriggerNodeExecutionStarted(this, NodeName);
            // Log.Debug(LogModules.AI, $"节点 {NodeName} 开始执行");
            
            // 调用子类实现的实际执行逻辑
            BTNodeState result = ExecuteNode();
            
            // 设置状态
            State = result;
            
            // 触发节点执行完成事件
            GameEvents.TriggerNodeExecutionCompleted(this, NodeName, result);
            // Log.Debug(LogModules.AI, $"节点 {NodeName} 执行完成，结果: {result}");
            
            return result;
        }
        
        /// <summary>
        /// 实际执行节点的逻辑
        /// 子类必须实现此方法
        /// </summary>
        /// <returns>节点的执行状态</returns>
        protected abstract BTNodeState ExecuteNode();

        /// <summary>
        /// 重置节点状态
        /// 触发状态变更事件并递归重置所有子节点
        /// </summary>
        public virtual void Reset()
        {   
            // Log.Debug(LogModules.AI, $"重置节点 {NodeName}");
            
            // 设置状态为Failure (通过属性设置会触发状态变更事件)
            State = BTNodeState.Failure;
            
            // 递归重置所有子节点
            foreach (var child in Children)
            {   
                child.Reset();
            }
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="child">要添加的子节点</param>
        public virtual void AddChild(BTNode child)
        {
            if (child == null) return;
            
            child.SetParent(this);
            Children.Add(child);
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        /// <param name="child">要移除的子节点</param>
        public virtual void RemoveChild(BTNode child)
        {
            if (child == null) return;
            
            child.SetParent(null);
            Children.Remove(child);
        }

        /// <summary>
        /// 清除所有子节点
        /// </summary>
        public virtual void ClearChildren()
        {
            foreach (var child in Children)
            {
                child.SetParent(null);
            }
            Children.Clear();
        }

        /// <summary>
        /// 获取子节点在列表中的索引
        /// </summary>
        /// <param name="child">要查找的子节点</param>
        /// <returns>子节点的索引，如果未找到返回-1</returns>
        public virtual int GetChildIndex(BTNode child)
        {
            return Children.IndexOf(child);
        }

        /// <summary>
        /// 获取指定索引的子节点
        /// </summary>
        /// <param name="index">子节点索引</param>
        /// <returns>子节点，如果索引无效返回null</returns>
        public virtual BTNode GetChild(int index)
        {
            if (index < 0 || index >= Children.Count)
                return null;
            return Children[index];
        }

        #region 事件驱动相关方法

        /// <summary>
        /// 触发节点状态变更事件
        /// </summary>
        /// <param name="oldState">旧状态</param>
        /// <param name="newState">新状态</param>
        protected virtual void OnStateChanged(BTNodeState oldState, BTNodeState newState)
        {   
            // 可以被子类重写以添加额外的状态变更逻辑
        }

        /// <summary>
        /// 触发节点执行开始事件
        /// </summary>
        protected virtual void OnExecutionStarted()
        {   
            // 可以被子类重写以添加额外的执行开始逻辑
        }

        /// <summary>
        /// 触发节点执行完成事件
        /// </summary>
        /// <param name="result">执行结果</param>
        protected virtual void OnExecutionCompleted(BTNodeState result)
        {   
            // 可以被子类重写以添加额外的执行完成逻辑
        }

        #endregion
    }
}