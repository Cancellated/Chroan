using UnityEngine;

namespace AI.Behavior
{
    /// <summary>
    /// 动作组件接口
    /// 定义所有AI动作组件必须实现的核心功能
    /// </summary>
    public interface IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        string ComponentName { get; }

        /// <summary>
        /// 是否可执行
        /// 检查当前环境和状态是否允许执行该动作
        /// </summary>
        /// <returns>是否可以执行动作</returns>
        bool CanExecute();

        /// <summary>
        /// 执行动作
        /// 执行该组件定义的核心行为逻辑
        /// </summary>
        /// <returns>执行是否成功</returns>
        bool Execute();

        /// <summary>
        /// 初始化组件
        /// 设置必要的引用和初始状态
        /// </summary>
        /// <param name="owner">拥有该组件的GameObject</param>
        void Initialize(GameObject owner);

        /// <summary>
        /// 更新组件状态
        /// 在每帧更新时调用，用于处理持续的动作逻辑
        /// </summary>
        void Update();

        /// <summary>
        /// 重置组件
        /// 重置组件状态到初始值
        /// </summary>
        void Reset();
    }
}
