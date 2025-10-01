using UnityEngine;
using Unity.VisualScripting;

namespace MyGame.UI
{
    /// <summary>
    /// MVC架构中的控制器基类
    /// 负责处理用户输入、更新模型和通知视图更新
    /// </summary>
    public abstract class BaseController : MonoBehaviour
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
        public BaseController()
        {
            IsInitialized = false;
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化控制器
        /// </summary>
        public virtual void Initialize()
        {
            if (!IsInitialized)
            {
                OnInitialize();
                IsInitialized = true;
            }
        }
        
        /// <summary>
        /// 清理控制器资源
        /// </summary>
        public virtual void Cleanup()
        {
            if (IsInitialized)
            {
                OnCleanup();
                IsInitialized = false;
            }
        }
        
        #endregion
        
        #region 保护方法
        
        /// <summary>
        /// 初始化逻辑
        /// 子类可以重写此方法来实现特定的初始化逻辑
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 清理逻辑
        /// 子类可以重写此方法来实现特定的清理逻辑
        /// </summary>
        protected virtual void OnCleanup() { }
        
        #endregion
    }
    
    /// <summary>
    /// 泛型版本的控制器基类
    /// 支持与特定的视图和模型类型关联
    /// </summary>
    /// <typeparam name="TView">视图类型</typeparam>
    /// <typeparam name="TModel">模型类型</typeparam>
    public abstract class BaseController<TView, TModel> : BaseController 
        where TView : class
        where TModel : class, new()
    {
        #region 字段和属性
        
        /// <summary>
        /// 视图引用
        /// </summary>
        protected TView m_view;
        
        /// <summary>
        /// 模型引用
        /// </summary>
        protected TModel m_model;
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 创建并初始化模型实例
        /// 由于模型通常不是MonoBehaviour，不能直接挂载到游戏对象上
        /// 通过代码创建实例来解决这个问题
        /// </summary>
        /// <returns>创建并初始化后的模型实例</returns>
        protected TModel CreateAndInitializeModel()
        {
            // 创建模型实例
            TModel model = new();
            
            // 使用反射检查并调用Initialize方法
            var initializeMethod = typeof(TModel).GetMethod("Initialize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
            initializeMethod?.Invoke(model, null);
            
            // 设置模型引用
            SetModel(model);
            
            return model;
        }
        
        /// <summary>
        /// 设置视图引用
        /// </summary>
        /// <param name="view">视图实例</param>
        public virtual void SetView(TView view)
        {
            m_view = view;
        }
        
        /// <summary>
        /// 设置模型引用
        /// </summary>
        /// <param name="model">模型实例</param>
        public virtual void SetModel(TModel model)
        {
            m_model = model;
        }
        
        #endregion
    }
}