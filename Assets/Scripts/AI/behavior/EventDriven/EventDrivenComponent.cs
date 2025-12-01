using UnityEngine;
using Logger;
using System;

namespace AI.Behavior.EventDriven
{
    /// <summary>
    /// 事件驱动行为组件基类
    /// 提供事件订阅和触发的基础功能，作为所有事件驱动组件的父类
    /// </summary>
    public abstract class EventDrivenComponent : MonoBehaviour
    {
        /// <summary>
        /// 组件名称，用于日志记录
        /// </summary>
        protected string ComponentName => GetType().Name;

        /// <summary>
        /// 组件所有者
        /// </summary>
        protected GameObject Owner => gameObject;

        /// <summary>
        /// 初始化状态标志
        /// </summary>
        protected bool IsInitialized { get; private set; }

        /// <summary>
        /// 组件初始化方法
        /// 在子类中实现具体的初始化逻辑
        /// </summary>
        public virtual void Initialize()
        {
            if (IsInitialized)
            {
                Log.Warning(LogModules.AI, $"{ComponentName}: 组件已初始化，避免重复初始化", this);
                return;
            }

            // 注册必要的事件监听器
            RegisterEventListeners();
            
            IsInitialized = true;
            Log.Info(LogModules.AI, $"{ComponentName}: 初始化完成", this);
        }

        /// <summary>
        /// 注册事件监听器
        /// 子类应重写此方法以注册需要监听的事件
        /// </summary>
        protected virtual void RegisterEventListeners()
        {
            // 基类不注册任何事件，由子类实现
        }

        /// <summary>
        /// 注销事件监听器
        /// 子类应重写此方法以注销之前注册的事件
        /// </summary>
        protected virtual void UnregisterEventListeners()
        {
            // 基类不注销任何事件，由子类实现
        }

        /// <summary>
        /// 检查组件是否可以执行
        /// 子类必须实现此方法以提供执行条件检查
        /// </summary>
        /// <returns>如果可以执行返回true，否则返回false</returns>
        public abstract bool CanExecute();

        /// <summary>
        /// 执行组件逻辑
        /// 子类必须实现此方法以提供具体的执行逻辑
        /// </summary>
        /// <returns>执行是否成功</returns>
        public abstract bool Execute();

        /// <summary>
        /// 重置组件状态
        /// 子类应重写此方法以提供具体的重置逻辑
        /// </summary>
        public virtual void Reset()
        {
            // 基类不执行任何重置操作，由子类实现
        }

        /// <summary>
        /// 触发组件相关事件
        /// 便捷方法，用于在组件中触发GameEvents中的事件
        /// </summary>
        /// <param name="triggerAction">触发事件的委托方法</param>
        public void TriggerComponentEvent(Action triggerAction)
        {
            try
            {
                triggerAction?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error(LogModules.AI, $"{ComponentName}: 触发事件时发生异常: {ex.Message}", this);
            }
        }

        /// <summary>
        /// 触发组件相关事件（带参数）
        /// 便捷方法，用于在组件中触发带参数的GameEvents事件
        /// </summary>
        /// <typeparam name="T1">事件参数1类型</typeparam>
        /// <param name="triggerAction">触发事件的委托方法</param>
        /// <param name="arg1">事件参数1</param>
        public void TriggerComponentEvent<T1>(Action<T1> triggerAction, T1 arg1)
        {
            try
            {
                triggerAction?.Invoke(arg1);
            }
            catch (Exception ex)
            {
                Log.Error(LogModules.AI, $"{ComponentName}: 触发事件时发生异常: {ex.Message}", this);
            }
        }

        /// <summary>
        /// 触发组件相关事件（带两个参数）
        /// 便捷方法，用于在组件中触发带两个参数的GameEvents事件
        /// </summary>
        /// <typeparam name="T1">事件参数1类型</typeparam>
        /// <typeparam name="T2">事件参数2类型</typeparam>
        /// <param name="triggerAction">触发事件的委托方法</param>
        /// <param name="arg1">事件参数1</param>
        /// <param name="arg2">事件参数2</param>
        public void TriggerComponentEvent<T1, T2>(Action<T1, T2> triggerAction, T1 arg1, T2 arg2)
        {
            try
            {
                triggerAction?.Invoke(arg1, arg2);
            }
            catch (Exception ex)
            {
                Log.Error(LogModules.AI, $"{ComponentName}: 触发事件时发生异常: {ex.Message}", this);
            }
        }

        /// <summary>
        /// 触发组件相关事件（带三个参数）
        /// 便捷方法，用于在组件中触发带三个参数的GameEvents事件
        /// </summary>
        /// <typeparam name="T1">事件参数1类型</typeparam>
        /// <typeparam name="T2">事件参数2类型</typeparam>
        /// <typeparam name="T3">事件参数3类型</typeparam>
        /// <param name="triggerAction">触发事件的委托方法</param>
        /// <param name="arg1">事件参数1</param>
        /// <param name="arg2">事件参数2</param>
        /// <param name="arg3">事件参数3</param>
        public void TriggerComponentEvent<T1, T2, T3>(Action<T1, T2, T3> triggerAction, T1 arg1, T2 arg2, T3 arg3)
        {
            try
            {
                triggerAction?.Invoke(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                Log.Error(LogModules.AI, $"{ComponentName}: 触发事件时发生异常: {ex.Message}", this);
            }
        }

        /// <summary>
        /// 订阅事件
        /// 将指定的事件处理器添加到事件中
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="eventType">事件类型标识符</param>
        /// <param name="handler">事件处理器</param>
        public void Subscribe<T>(string eventType, Action<T> handler) where T : class
        {
            // 在项目的事件系统中实现事件订阅逻辑
            Log.Debug(LogModules.AI, $"{ComponentName}: 订阅事件 {eventType}", this);
        }

        /// <summary>
        /// 取消订阅事件
        /// 从事件中移除指定的事件处理器
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="eventType">事件类型标识符</param>
        /// <param name="handler">事件处理器</param>
        public void Unsubscribe<T>(string eventType, Action<T> handler) where T : class
        {
            // 在项目的事件系统中实现事件取消订阅逻辑
            Log.Debug(LogModules.AI, $"{ComponentName}: 取消订阅事件 {eventType}", this);
        }

        /// <summary>
        /// 组件启用时调用
        /// 确保组件被正确初始化
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            else
            {
                RegisterEventListeners();
            }
        }

        /// <summary>
        /// 组件禁用时调用
        /// 注销事件监听器以避免内存泄漏
        /// </summary>
        protected virtual void OnDisable()
        {
            UnregisterEventListeners();
        }

        /// <summary>
        /// 组件销毁时调用
        /// 确保所有资源被正确释放
        /// </summary>
        protected virtual void OnDestroy()
        {
            UnregisterEventListeners();
            IsInitialized = false;
        }
    }
}