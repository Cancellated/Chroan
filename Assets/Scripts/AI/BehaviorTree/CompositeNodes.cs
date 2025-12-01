using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 选择器节点
    /// 按顺序尝试子节点，直到找到一个成功的节点
    /// 如果所有子节点都失败，则选择器失败
    /// </summary>
    public class BTSelector : BTNode
    {
        /// <summary>
        /// 当前正在执行的子节点索引
        /// </summary>
        private int currentChildIndex;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        public BTSelector(string nodeName = "Selector") : base(nodeName)
        {
        }

        /// <summary>
        /// 执行选择器逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            // 如果没有子节点，直接失败
            if (Children.Count == 0)
            {
                State = BTNodeState.Failure;
                return State;
            }

            // 如果是新开始执行，重置索引
            if (State != BTNodeState.Running)
            {
                currentChildIndex = 0;
                State = BTNodeState.Running;
            }

            // 遍历所有子节点
            while (currentChildIndex < Children.Count)
            {
                var child = Children[currentChildIndex];
                
                if (child == null)
                {
                    currentChildIndex++;
                    continue;
                }

                BTNodeState childState = child.Execute();

                if (childState == BTNodeState.Success)
                {
                    // 子节点成功，选择器成功，重置所有子节点
                    State = BTNodeState.Success;
                    ResetChildren();
                    return State;
                }
                else if (childState == BTNodeState.Failure)
                {
                    // 子节点失败，继续下一个
                    currentChildIndex++;
                }
                else // childState == BTNodeState.Running
                {
                    // 子节点正在运行，保持运行状态
                    State = BTNodeState.Running;
                    return State;
                }
            }

            // 所有子节点都失败
            State = BTNodeState.Failure;
            ResetChildren();
            return State;
        }

        /// <summary>
        /// 重置所有子节点状态
        /// </summary>
        private void ResetChildren()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }
        }

        /// <summary>
        /// 重置节点状态
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            currentChildIndex = 0;
        }
    }

    /// <summary>
    /// 序列节点
    /// 按顺序执行所有子节点
    /// 如果所有子节点都成功，序列才成功
    /// 如果有子节点失败，立即失败
    /// </summary>
    public class BTSequence : BTNode
    {
        /// <summary>
        /// 当前正在执行的子节点索引
        /// </summary>
        private int currentChildIndex;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        public BTSequence(string nodeName = "Sequence") : base(nodeName)
        {
        }

        /// <summary>
        /// 执行序列逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            // 如果没有子节点，直接成功
            if (Children.Count == 0)
            {
                State = BTNodeState.Success;
                return State;
            }

            // 如果是新开始执行，重置索引
            if (State != BTNodeState.Running)
            {
                currentChildIndex = 0;
                State = BTNodeState.Running;
            }

            // 按顺序执行所有子节点
            while (currentChildIndex < Children.Count)
            {
                var child = Children[currentChildIndex];
                
                if (child == null)
                {
                    currentChildIndex++;
                    continue;
                }

                BTNodeState childState = child.Execute();

                if (childState == BTNodeState.Failure)
                {
                    // 子节点失败，序列失败，重置所有子节点
                    State = BTNodeState.Failure;
                    ResetChildren();
                    return State;
                }
                else if (childState == BTNodeState.Success)
                {
                    // 子节点成功，继续下一个
                    currentChildIndex++;
                }
                else // childState == BTNodeState.Running
                {
                    // 子节点正在运行，保持运行状态
                    State = BTNodeState.Running;
                    return State;
                }
            }

            // 所有子节点都成功
            State = BTNodeState.Success;
            ResetChildren();
            return State;
        }

        /// <summary>
        /// 重置所有子节点状态
        /// </summary>
        private void ResetChildren()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }
        }

        /// <summary>
        /// 重置节点状态
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            currentChildIndex = 0;
        }
    }

    /// <summary>
    /// 并行节点
    /// 同时执行所有子节点
    /// </summary>
    public class BTParallel : BTNode
    {
        /// <summary>
        /// 并行策略
        /// </summary>
        public enum ParallelStrategy
        {
            /// <summary>
            /// 所有子节点都必须成功
            /// </summary>
            AllMustSucceed,
            
            /// <summary>
            /// 至少一个子节点成功
            /// </summary>
            AtLeastOneMustSucceed,
            
            /// <summary>
            /// 至少一个子节点失败
            /// </summary>
            AtLeastOneMustFail,
            
            /// <summary>
            /// 无论结果如何
            /// </summary>
            IgnoreResults
        }

        /// <summary>
        /// 并行策略
        /// </summary>
        public ParallelStrategy Strategy { get; set; }

        /// <summary>
        /// 成功子节点计数
        /// </summary>
        private int successCount;

        /// <summary>
        /// 失败子节点计数
        /// </summary>
        private int failureCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="strategy">并行策略</param>
        public BTParallel(string nodeName = "Parallel", ParallelStrategy strategy = ParallelStrategy.AtLeastOneMustSucceed) 
            : base(nodeName)
        {
            Strategy = strategy;
        }

        /// <summary>
        /// 执行并行逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            // 如果没有子节点，根据策略决定状态
            if (Children.Count == 0)
            {
                switch (Strategy)
                {
                    case ParallelStrategy.AllMustSucceed:
                        State = BTNodeState.Success;
                        break;
                    case ParallelStrategy.AtLeastOneMustSucceed:
                    case ParallelStrategy.AtLeastOneMustFail:
                        State = BTNodeState.Failure;
                        break;
                    case ParallelStrategy.IgnoreResults:
                        State = BTNodeState.Success;
                        break;
                }
                return State;
            }

            // 如果是新开始执行，重置计数
            if (State != BTNodeState.Running)
            {
                successCount = 0;
                failureCount = 0;
                State = BTNodeState.Running;
            }

            // 重置子节点状态（以便重新开始）
            foreach (var child in Children)
            {
                child.Reset();
            }

            // 执行所有子节点
            foreach (var child in Children)
            {
                if (child == null) continue;

                BTNodeState childState = child.Execute();
                
                if (childState == BTNodeState.Success)
                {
                    successCount++;
                }
                else if (childState == BTNodeState.Failure)
                {
                    failureCount++;
                }
                // Running状态不影响计数
            }

            // 根据策略决定最终状态
            return EvaluateStrategy();
        }

        /// <summary>
        /// 根据策略评估结果
        /// </summary>
        /// <returns>执行状态</returns>
        private BTNodeState EvaluateStrategy()
        {
            switch (Strategy)
            {
                case ParallelStrategy.AllMustSucceed:
                    State = (successCount == Children.Count) ? BTNodeState.Success : BTNodeState.Failure;
                    break;
                    
                case ParallelStrategy.AtLeastOneMustSucceed:
                    State = (successCount > 0) ? BTNodeState.Success : BTNodeState.Failure;
                    break;
                    
                case ParallelStrategy.AtLeastOneMustFail:
                    State = (failureCount > 0) ? BTNodeState.Success : BTNodeState.Failure;
                    break;
                    
                case ParallelStrategy.IgnoreResults:
                    State = BTNodeState.Success;
                    break;
            }

            // 如果还在运行，返回运行状态
            if (State != BTNodeState.Running)
            {
                // 重置子节点状态
                foreach (var child in Children)
                {
                    child.Reset();
                }
            }

            return State;
        }

        /// <summary>
        /// 重置节点状态
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            successCount = 0;
            failureCount = 0;
        }
    }
}