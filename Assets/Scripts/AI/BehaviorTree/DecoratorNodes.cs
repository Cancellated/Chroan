using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 装饰器节点基类
    /// 装饰器节点只能有一个子节点，用于修改子节点的行为
    /// </summary>
    public abstract class BTDecorator : BTNode
    {
        /// <summary>
        /// 子节点
        /// </summary>
        public BTNode Child { get; protected set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        protected BTDecorator(string nodeName = "Decorator") : base(nodeName)
        {
        }

        /// <summary>
        /// 设置子节点
        /// </summary>
        /// <param name="child">子节点</param>
        public virtual void SetChild(BTNode child)
        {
            Child?.SetParent(null);
            
            Child = child;
            Child?.SetParent(this);
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        public virtual void RemoveChild()
        {
            if (Child != null)
            {
                Child.SetParent(null);
                Child = null;
            }
        }

        /// <summary>
        /// 重置装饰器状态
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            Child?.Reset();
        }

        /// <summary>
        /// 添加子节点（装饰器只能有一个子节点）
        /// </summary>
        /// <param name="child">要添加的子节点</param>
        public override void AddChild(BTNode child)
        {
            SetChild(child);
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        /// <param name="child">要移除的子节点</param>
        public override void RemoveChild(BTNode child)
        {
            if (Child == child)
            {
                RemoveChild();
            }
        }

        /// <summary>
        /// 清除所有子节点
        /// </summary>
        public override void ClearChildren()
        {
            RemoveChild();
        }

        /// <summary>
        /// 获取子节点索引（装饰器只有一个子节点）
        /// </summary>
        /// <param name="child">要查找的子节点</param>
        /// <returns>子节点的索引，如果未找到返回-1</returns>
        public override int GetChildIndex(BTNode child)
        {
            return (Child == child) ? 0 : -1;
        }

        /// <summary>
        /// 获取指定索引的子节点
        /// </summary>
        /// <param name="index">子节点索引</param>
        /// <returns>子节点，如果索引无效返回null</returns>
        public override BTNode GetChild(int index)
        {
            return (index == 0) ? Child : null;
        }
    }

    /// <summary>
    /// 反转器装饰器
    /// 将子节点的执行结果反转：成功->失败，失败->成功
    /// </summary>
    public class BTInverter : BTDecorator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        public BTInverter(string nodeName = "Inverter") : base(nodeName)
        {
        }

        /// <summary>
        /// 执行反转器逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (Child == null)
            {
                State = BTNodeState.Failure;
                return State;
            }

            BTNodeState childState = Child.Execute();

            switch (childState)
            {
                case BTNodeState.Success:
                    State = BTNodeState.Failure;
                    break;
                case BTNodeState.Failure:
                    State = BTNodeState.Success;
                    break;
                case BTNodeState.Running:
                    State = BTNodeState.Running;
                    break;
            }

            return State;
        }
    }

    /// <summary>
    /// 成功器装饰器
    /// 无论子节点执行结果如何，都返回成功
    /// </summary>
    public class BTSucceeder : BTDecorator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        public BTSucceeder(string nodeName = "Succeeder") : base(nodeName)
        {
        }

        /// <summary>
        /// 执行成功器逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (Child == null)
            {
                State = BTNodeState.Success;
                return State;
            }

            Child.Execute();
            State = BTNodeState.Success;
            return State;
        }
    }

    /// <summary>
    /// 失败器装饰器
    /// 无论子节点执行结果如何，都返回失败
    /// </summary>
    public class BTFailer : BTDecorator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        public BTFailer(string nodeName = "Failer") : base(nodeName)
        {
        }

        /// <summary>
        /// 执行失败器逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (Child == null)
            {
                State = BTNodeState.Failure;
                return State;
            }

            Child.Execute();
            State = BTNodeState.Failure;
            return State;
        }
    }

    /// <summary>
    /// 重复器装饰器
    /// 重复执行子节点指定次数或直到失败
    /// </summary>
    public class BTRepeater : BTDecorator
    {
        /// <summary>
        /// 重复次数，-1表示无限重复
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>
        /// 重复直到失败
        /// </summary>
        public bool RepeatUntilFail { get; set; }

        /// <summary>
        /// 重复直到成功
        /// </summary>
        public bool RepeatUntilSuccess { get; set; }

        /// <summary>
        /// 当前重复次数
        /// </summary>
        private int currentRepeatCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repeatCount">重复次数，-1表示无限重复</param>
        /// <param name="nodeName">节点名称</param>
        public BTRepeater(int repeatCount = -1, string nodeName = "Repeater") : base(nodeName)
        {
            RepeatCount = repeatCount;
            RepeatUntilFail = false;
            RepeatUntilSuccess = false;
        }

        /// <summary>
        /// 设置重复直到失败
        /// </summary>
        /// <param name="enabled">是否启用</param>
        /// <returns>重复器实例</returns>
        public BTRepeater SetRepeatUntilFail(bool enabled = true)
        {
            RepeatUntilFail = enabled;
            if (enabled)
            {
                RepeatUntilSuccess = false;
            }
            return this;
        }

        /// <summary>
        /// 设置重复直到成功
        /// </summary>
        /// <param name="enabled">是否启用</param>
        /// <returns>重复器实例</returns>
        public BTRepeater SetRepeatUntilSuccess(bool enabled = true)
        {
            RepeatUntilSuccess = enabled;
            if (enabled)
            {
                RepeatUntilFail = false;
            }
            return this;
        }

        /// <summary>
        /// 执行重复器逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (Child == null)
            {
                State = BTNodeState.Failure;
                return State;
            }

            // 如果是新开始执行，重置计数
            if (State != BTNodeState.Running)
            {
                currentRepeatCount = 0;
                State = BTNodeState.Running;
            }

            while (true)
            {
                BTNodeState childState = Child.Execute();

                if (RepeatUntilFail && childState == BTNodeState.Failure)
                {
                    State = BTNodeState.Success;
                    break;
                }

                if (RepeatUntilSuccess && childState == BTNodeState.Success)
                {
                    State = BTNodeState.Success;
                    break;
                }

                currentRepeatCount++;

                if (RepeatCount > 0 && currentRepeatCount >= RepeatCount)
                {
                    State = BTNodeState.Success;
                    break;
                }

                if (childState == BTNodeState.Running)
                {
                    State = BTNodeState.Running;
                    return State;
                }

                // 重置子节点以备下次执行
                Child.Reset();

                // 如果是无限重复，继续循环
                if (RepeatCount < 0)
                {
                    continue;
                }
            }

            // 重置子节点状态
            Child.Reset();
            return State;
        }

        /// <summary>
        /// 重置重复器状态
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            currentRepeatCount = 0;
        }
    }

    /// <summary>
    /// 延迟装饰器
    /// 在执行子节点之前等待指定时间
    /// </summary>
    public class BTDelay : BTDecorator
    {
        /// <summary>
        /// 延迟时间（秒）
        /// </summary>
        public float DelayTime { get; set; }

        /// <summary>
        /// 当前已等待的时间
        /// </summary>
        private float currentDelayTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="delayTime">延迟时间（秒）</param>
        /// <param name="nodeName">节点名称</param>
        public BTDelay(float delayTime, string nodeName = "Delay") : base(nodeName)
        {
            DelayTime = delayTime;
            currentDelayTime = 0f;
        }

        /// <summary>
        /// 执行延迟器逻辑
        /// </summary>
        /// <returns>执行状态</returns>
        protected override BTNodeState ExecuteNode()
        {
            if (Child == null)
            {
                State = BTNodeState.Failure;
                return State;
            }

            // 如果是新开始执行，重置计时器
            if (State != BTNodeState.Running)
            {
                currentDelayTime = 0f;
                State = BTNodeState.Running;
            }

            // 累积时间
            currentDelayTime += Time.deltaTime;

            if (currentDelayTime < DelayTime)
            {
                // 还在等待中
                State = BTNodeState.Running;
                return State;
            }

            // 延迟结束，执行子节点
            State = Child.Execute();
            return State;
        }

        /// <summary>
        /// 重置延迟器状态
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            currentDelayTime = 0f;
        }
    }
}