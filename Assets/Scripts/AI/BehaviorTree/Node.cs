using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 行为树节点执行状态
    /// </summary>
    public enum BTNodeState
    {
        /// <summary>
        /// 节点正在运行
        /// </summary>
        Running,
        
        /// <summary>
        /// 节点执行成功
        /// </summary>
        Success,
        
        /// <summary>
        /// 节点执行失败
        /// </summary>
        Failure
    }

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
        /// 节点的执行状态
        /// </summary>
        public BTNodeState State { get; protected set; }

        /// <summary>
        /// 节点是否正在运行
        /// </summary>
        public bool IsRunning => State == BTNodeState.Running;

        /// <summary>
        /// 节点的父节点
        /// </summary>
        public BTNode Parent { get; protected set; }

        /// <summary>
        /// 安全设置父节点
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
            State = BTNodeState.Failure;
            Children = new List<BTNode>();
        }

        /// <summary>
        /// 执行节点
        /// 子类必须实现此方法
        /// </summary>
        /// <returns>节点的执行状态</returns>
        public abstract BTNodeState Execute();

        /// <summary>
        /// 重置节点状态
        /// </summary>
        public virtual void Reset()
        {
            State = BTNodeState.Failure;
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
    }
}