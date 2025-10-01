using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace MyGame.UI
{
    /// <summary>
    /// MVC架构中的模型基类
    /// 负责管理数据和业务逻辑
    /// </summary>
    public abstract class BaseModel :IInitializable
    {
        #region 字段和属性
        
        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseModel()
        {
            IsInitialized = false;
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化模型
        /// 提供双重检查机制防止重复初始化
        /// </summary>
        public virtual void Initialize()
        {
            if (!IsInitialized)
            {
                try
                {
                    IsInitialized = true;
                }
                catch (Exception ex)
                {
                    // 记录初始化失败的异常信息
                    UnityEngine.Debug.LogError($"Failed to initialize model {GetType().Name}: {ex.Message}");
                    // 保持IsInitialized为false，允许后续重试初始化
                }
            }
        }
        
        /// <summary>
        /// 清理模型资源
        /// </summary>
        public virtual void Cleanup()
        {
            if (IsInitialized)
            {
                IsInitialized = false;
            }
        }
        
        #endregion
        
    }
    
    /// <summary>
    /// 带数据变化通知的模型基类
    /// 支持属性变更通知机制
    /// </summary>
    public abstract class ObservableModel : BaseModel
    {
        #region 事件
        
        /// <summary>
        /// 数据变更事件
        /// </summary>
        public event Action<string> OnPropertyChanged;

        #endregion

        #region 保护方法

        /// <summary>
        /// 通知属性变更
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged?.Invoke(propertyName);
        }

        /// <summary>
        /// 设置属性值并通知变更
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否成功设置新值</returns>
        protected bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
        
        #endregion
    }
}