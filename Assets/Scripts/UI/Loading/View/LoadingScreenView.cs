using Logger;
using UnityEngine;

using MyGame.UI.Loading.Controller;
using UnityEngine.UI;

namespace MyGame.UI.Loading.View
{
    /// <summary>
    /// 加载界面组件
    /// MVC架构中的View层，负责显示加载界面的UI元素和动画效果
    /// 使用Animator组件控制由bool参数触发的动画
    /// </summary>
    public class LoadingScreenView : BaseView<LoadingScreenController>
    {
        private const string LOG_MODULE = LogModules.LOADING;
        
        // 动画参数名称常量
        private const string ANIM_PARAM_SHOW_LOADING = "ShowLoading";
        private const string ANIM_PARAM_HIDE_LOADING = "HideLoading";

        [Header("层级设置")]
        [Tooltip("加载界面Canvas的Sorting Order。值越高，显示层级越高，不易被其他UI遮挡。")]
        public int canvasSortingOrder = 1000;
        
        [Header("动画设置")]
        [Tooltip("控制加载界面显隐动画的Animator组件")]
        [SerializeField] private Animator m_animator;
        

        /// <summary>
        /// 初始化加载界面
        /// </summary>
        protected override void Awake()
        {
            // 设置面板类型为Loading
            m_panelType = UIType.Loading;
            
            // 调用基类的Awake方法，完成基础初始化
            base.Awake();
            
            // 确保使用全局Canvas
            EnsureGlobalCanvasParent();
            
            // 自动获取Animator组件
            if (m_animator == null)
            {
                m_animator = GetComponent<Animator>();
                if (m_animator == null)
                {
                    m_animator = gameObject.AddComponent<Animator>();
                    Log.Info(LOG_MODULE, "已自动添加Animator组件", this);
                }
            }
        }
        
        /// <summary>
        /// 确保加载界面使用全局Canvas作为父级并设置正确的排序层级
        /// </summary>
        private void EnsureGlobalCanvasParent()
        {
            // 获取全局Canvas
            GameObject globalCanvasObj = GameObject.Find("GlobalUI");
            Canvas globalCanvas = globalCanvasObj != null ? globalCanvasObj.GetComponent<Canvas>() : null;
            if (globalCanvas != null)
            {
                // 如果当前对象不在全局Canvas下，则将其移动到全局Canvas下
                if (transform.parent != globalCanvas.transform)
                {
                    transform.SetParent(globalCanvas.transform, false);
                }
                
                // 获取或添加面板自身的Canvas组件
                if (!TryGetComponent<Canvas>(out var panelCanvas))
                {
                    panelCanvas = gameObject.AddComponent<Canvas>();
                    panelCanvas.overrideSorting = true;
                }
                
                // 设置Canvas的排序层级为1000，与PanelLoader中设置的Loading类型排序层级保持一致
                panelCanvas.sortingOrder = canvasSortingOrder;
                Log.Info(LOG_MODULE, $"已设置加载界面Canvas排序层级为: {canvasSortingOrder}", this);
            }
            else
            {
                Log.Warning(LOG_MODULE, "未找到全局Canvas组件。", this);
            }
        }
        
        /// <summary>
        /// 尝试自动绑定控制器
        /// 创建并绑定LoadingScreenController实例
        /// </summary>
        protected override void TryBindController()
        {
            // 正确的方法：在GameObject上添加控制器组件
            LoadingScreenController controller = gameObject.AddComponent<LoadingScreenController>();
            
            // 初始化控制器
            controller.Initialize();
            
            // 设置控制器的视图引用
            controller.SetView(this);
            
            // 绑定控制器到视图
            BindController(controller);
        }
        
        /// <summary>
        /// 初始化面板
        /// 重写IUIPanel接口的Initialize方法
        /// </summary>
        public override void Initialize()
        {
            // 可以在这里进行额外的初始化逻辑
        }
        
        /// <summary>
        /// 清理面板资源
        /// 重写IUIPanel接口的Cleanup方法
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
        }
        
        /// <summary>
        /// 显示加载界面
        /// 重写IUIPanel接口的Show方法，通过设置bool参数触发进入动画
        /// </summary>
        public override void Show()
        {
            if (!IsVisible)
            {  
                // 确保CanvasGroup的alpha值为1，使面板可见
                if (m_canvasGroup != null)
                {
                    m_canvasGroup.alpha = 1f;
                }  
                // 通过设置bool参数触发进入动画
                if (m_animator != null)
                {
                    m_animator.SetBool(ANIM_PARAM_SHOW_LOADING, true);
                    m_animator.SetBool(ANIM_PARAM_HIDE_LOADING, false);
                }
                
                IsVisible = true;
                
                Log.Info(LOG_MODULE, "加载界面显示，触发ShowLoading动画");
            }
        }
        
        /// <summary>
        /// 隐藏加载界面
        /// 重写IUIPanel接口的Hide方法，通过设置bool参数触发退出动画
        /// </summary>
        public override void Hide()
        {
            if (IsVisible)
            {
                Log.Info(LOG_MODULE, "隐藏加载界面，触发HideLoading动画");
                
                // 通过设置bool参数触发退出动画
                if (m_animator != null)
                {
                    m_animator.SetBool(ANIM_PARAM_HIDE_LOADING, true);
                    m_animator.SetBool(ANIM_PARAM_SHOW_LOADING, false);
                }
                else
                {
                    // 如果没有动画组件，直接隐藏
                    OnHideAnimationComplete();
                }
            }
        }
        
        /// <summary>
        /// 隐藏动画完成后的回调处理
        /// 此方法应由动画状态机在动画结束时调用
        /// </summary>
        public void OnHideAnimationComplete()
        {
            IsVisible = false; 
            // 重置动画状态
            if (m_animator != null)
            {
                m_animator.SetBool(ANIM_PARAM_HIDE_LOADING, false);
            }
            
            // 通知控制器加载界面已隐藏
            if (m_controller != null)
            {
                var loadingController = m_controller as LoadingScreenController;
                if (loadingController != null)
                {
                    loadingController.OnHideAnimationComplete();
                }
            }
        }
    
    }
}